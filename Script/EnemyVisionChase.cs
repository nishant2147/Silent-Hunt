using UnityEngine;
using UnityEngine.AI;

public class EnemyVisionChase : MonoBehaviour
{
    public Transform player;
    public float detectRadius;
    public Transform[] patrolPoints;
    public float waitTime;
    public Transform sprite;

    [Header("Search Behaviour")]
    public float PlayersearchWaitTime;
    public float searchRotateSpeed = 180f;

    private Vector3 lastSeenPosition;
    private bool wasChasing;
    private bool isSearching;
    private float searchTimer;

    [Header("Last Seen Wait")]
    public float waitBeforeSearch;

    [Header("Attack")]
    public float attackRange;
    private int currentAttackIndex;
    public float attackDelay;
    private float attackTimer;

    private bool isAttacking;

    private float waitBeforeSearchTimer;
    private bool isWaitingAtLastSeen;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate;
    private float fireTimer;

    private NavMeshAgent agent;
    private Animator animator;

    private int patrolIndex;
    private float waitTimer;
    private bool isWaiting;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (!GameManager.Instance.isGameStarted)
        {
            agent.ResetPath();
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (GameManager.Instance.isPlayerSpotted)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= attackRange)
            {
                StartAttack();

                if (isAttacking)
                    HandleAttackLoop();
                AutoShoot();
            }
            else
            {
                StopAttack();
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            RotateTowardsPlayer();
            UpdateAnimator();
            LockZPosition();
            return;
        }

        if (PlayerDetected())
        {
            AutoShoot();
        }

        if (PlayerInAttackRange())
        {
            StartAttack();
        }
        else if (PlayerDetected())
        {
            StopAttack();
            wasChasing = true;
            isSearching = false;
            isWaitingAtLastSeen = false;
        }
        else
        {
            StopAttack();

            if (wasChasing)
            {
                GoToLastSeenPosition();
            }
            else if (isWaitingAtLastSeen)
            {
                HandleWaitBeforeSearch();
            }
            else if (isSearching)
            {
                HandleSearchLook();
            }
            else
            {
                Patrol();
            }
        }

        if (isAttacking)
        {
            HandleAttackLoop();
        }

        RotateSprite();
    }
    void AutoShoot()
    {
        Debug.Log("Shooting...");
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            FireBullet();
        }
    }
    void FireBullet()
    {
        if (player == null) return;

        Vector2 direction = (player.position - firePoint.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
          bulletPrefab,
          firePoint.position,
          Quaternion.Euler(0f, 0f, angle - 90f)
      );


        bullet.GetComponent<EnemyBullet>().SetDirection(direction);
    }
    void RotateTowardsPlayer()
    {
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        sprite.rotation = Quaternion.Euler(0, 0, angle);
    }
    void HandleAttackLoop()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackDelay)
        {
            attackTimer = 0f;

            currentAttackIndex++;

            if (currentAttackIndex > 2)
                currentAttackIndex = 0;

            animator.SetInteger("attackIndex", currentAttackIndex);
        }

        RotateTowardsPlayer();
    }
    void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        currentAttackIndex = 0;
        attackTimer = 0f;

        agent.ResetPath();
        agent.isStopped = true;

        animator.SetInteger("attackIndex", currentAttackIndex);
        animator.SetBool("isAttacking", true);
        animator.SetFloat("Speed", 0f);
    }
    void StopAttack()
    {
        if (!isAttacking) return;

        isAttacking = false;
        attackTimer = 0f;

        agent.isStopped = false;

        animator.SetBool("isAttacking", false);
        animator.SetFloat("Speed", 0f);
    }
    bool PlayerInAttackRange()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= attackRange;
    }
    bool PlayerDetected()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= detectRadius;
    }

    void ChasePlayer()
    {
        lastSeenPosition = player.position;
        agent.SetDestination(lastSeenPosition);
    }
    void GoToLastSeenPosition()
    {
        agent.SetDestination(lastSeenPosition);

        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            agent.ResetPath();
            isWaitingAtLastSeen = true;
            waitBeforeSearchTimer = 0f;

            wasChasing = false;
        }
    }
    void HandleWaitBeforeSearch()
    {
        waitBeforeSearchTimer += Time.deltaTime;
        animator.SetFloat("Speed", 0f);

        if (waitBeforeSearchTimer >= waitBeforeSearch)
        {
            isWaitingAtLastSeen = false;
            isSearching = true;
            searchTimer = 0f;
        }
    }
    void HandleSearchLook()
    {
        if (!isSearching) return;

        searchTimer += Time.deltaTime;
        sprite.Rotate(0, 0, searchRotateSpeed * Time.deltaTime);

        if (searchTimer >= PlayersearchWaitTime)
        {
            isSearching = false;
            sprite.rotation = Quaternion.identity;

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            agent.isStopped = true;
            animator.SetBool("isWalking", false);

            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;

                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.isStopped = false;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance > 0.2f)
        {
            animator.SetBool("isWalking", true);
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
        {
            isWaiting = true;
            agent.ResetPath();
            animator.SetBool("isWalking", false);
        }
    }
    void UpdateAnimator()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetInteger("attackIndex", currentAttackIndex);
    }

    void RotateSprite()
    {
        Vector2 velocity = agent.velocity;

        if (velocity.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            sprite.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void LockZPosition()
    {
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            0f
        );
    }
}