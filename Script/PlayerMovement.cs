using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask obstacleLayer;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Opacity")]
    [Range(0f, 1f)] public float grassOpacity = 0.49f;
    public float normalOpacity = 1f;

    [Header("Arrow Line Renderer")]
    public LineRenderer moveArrowLine;

    [Header("EnemyBlood Effect")]
    public GameObject EnemybloodEffectPrefab;

    [HideInInspector] public bool isInGrass;

    [Header("Auto Exit")]
    public bool autoMoveOnStart;
    public Transform exitPoint;

    private bool isAutoMoving;

    public float attackDuration = 0.4f;

    private bool isAttacking;
    private GameObject currentEnemy;

    private NavMeshAgent agent;
    private SpriteRenderer[] spriteRenderers;
    private Animator animator;
    private Transform followTarget;
    private bool isFollowingEnemy;
    private GameObject currentTargetIndicator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = moveSpeed;
        agent.acceleration = 50f;
        agent.stoppingDistance = 0.05f;
        agent.autoBraking = true;

        moveArrowLine.positionCount = 0;
        moveArrowLine.enabled = false;

        if (autoMoveOnStart && exitPoint != null)
        {
            StartCoroutine(AutoMove());
        }
    }
    IEnumerator AutoMove()
    {
        yield return new WaitForSeconds(0.2f);

        if (agent.isOnNavMesh)
        {
            isAutoMoving = true;
            agent.SetDestination(exitPoint.position);
        }
    }
    void Update()
    {
        if (isAutoMoving)
        {
            RotateTowardsMovement();
            UpdateAnimation();

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.ResetPath();
                animator.SetBool("isWalking", false);
                isAutoMoving = false;

                Debug.Log("Reached Exit Point");
            }

            return;
        }

        if (!GameManager.Instance.isGameStarted)
        {
            agent.ResetPath();
            animator.SetBool("isWalking", false);
            return;
        }

        HandleMouseClick();
        RotateTowardsMovement();
        UpdateAnimation();

        UpdateLineFromNavMesh();

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            agent.hasPath)
        {
            HideArrowLine();
            agent.ResetPath();
        }
    }
    public void StartAutoMove(Transform target)
    {
        if (target == null || !agent.isOnNavMesh) return;

        ClearTargetIndicator();
        isFollowingEnemy = false;
        agent.ResetPath();

        agent.SetDestination(target.position);
    }
    void HideArrowLine()
    {
        moveArrowLine.positionCount = 0;
        moveArrowLine.enabled = false;
    }
    void UpdateLineFromNavMesh()
    {
        if (!agent.hasPath || agent.path.corners.Length < 2)
            return;

        NavMeshPath path = agent.path;
        moveArrowLine.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 pos = path.corners[i];
            pos.z = -1f;
            moveArrowLine.SetPosition(i, pos);
        }

        if (currentEnemy != null && !isAttacking)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(currentEnemy.transform.position);
            }
        }
    }
    
    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0f;
            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null)
            {
                if (hit.CompareTag("Enemy"))
                {
                    followTarget = hit.transform;
                    isFollowingEnemy = true;
                    moveArrowLine.enabled = false;
                    ShowTargetIndicator(hit.gameObject);
                    return;
                }

                if (((1 << hit.gameObject.layer) & obstacleLayer) != 0)
                    return;
            }

            ClearTargetIndicator();

            isFollowingEnemy = false;
            followTarget = null;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(clickPos);
                moveArrowLine.enabled = true;
            }
        }

        FollowEnemyTarget();
    }
    void FollowEnemyTarget()
    {
        if (!isFollowingEnemy || followTarget == null)
            return;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(followTarget.position);
        }
    }
    void ShowTargetIndicator(GameObject enemy)
    {
        if (currentTargetIndicator != null)
        {
            currentTargetIndicator.SetActive(false);
            currentTargetIndicator.transform.localScale = Vector3.one;
        }

        Transform indicator = enemy.transform.Find("TargetIndicator");

        if (indicator != null)
        {
            currentTargetIndicator = indicator.gameObject;
            currentTargetIndicator.SetActive(true);

            StartCoroutine(TargetScaleEffect(currentTargetIndicator.transform));
        }
    }
    void ClearTargetIndicator()
    {
        if (currentTargetIndicator != null)
        {
            currentTargetIndicator.SetActive(false);
            currentTargetIndicator.transform.localScale = Vector3.one;
            currentTargetIndicator = null;
        }

        followTarget = null;
        isFollowingEnemy = false;
    }
    IEnumerator TargetScaleEffect(Transform target)
    {
        float duration = 0.2f;
        float timer = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 maxScale = Vector3.one * 1.5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, maxScale, timer / duration);
            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(maxScale, startScale, timer / duration);
            yield return null;
        }

        target.localScale = startScale;
    }
    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    void UpdateAnimation()
    {
        if (isAttacking) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", agent.velocity.x);
            animator.SetFloat("MoveY", agent.velocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isAttacking)
        {
            currentEnemy = collision.gameObject;
            StartCoroutine(AttackEnemy());
        }

        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            agent.ResetPath();
        }
    }
    IEnumerator AttackEnemy()
    {
        isAttacking = true;

        agent.ResetPath();

        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", true);

        yield return new WaitForSeconds(attackDuration);


        if (currentEnemy != null)
        {
            Vector3 killPos = currentEnemy.transform.position;

            Instantiate(EnemybloodEffectPrefab, killPos, Quaternion.identity);

            GameManager.Instance.AlertEnemies(killPos);

            Destroy(currentEnemy);
        }

        if (currentTargetIndicator != null)
        {
            currentTargetIndicator.SetActive(false);
            currentTargetIndicator = null;
        }

        animator.SetBool("isAttacking", false);
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
        {
            isInGrass = true;
            SetOpacity(grassOpacity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
        {
            isInGrass = false;
            SetOpacity(normalOpacity);
        }
    }

    void SetOpacity(float alpha)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
