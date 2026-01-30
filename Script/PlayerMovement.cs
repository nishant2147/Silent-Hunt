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

    private NavMeshAgent agent;
    private SpriteRenderer[] spriteRenderers;
    private Animator animator;

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
    }

    void Update()
    {
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
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0f;

            if (Physics2D.OverlapPoint(clickPos, obstacleLayer))
                return;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(clickPos);
                moveArrowLine.enabled = true;
            }
        }
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
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            agent.ResetPath();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
            SetOpacity(grassOpacity);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
            SetOpacity(normalOpacity);
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
