using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask obstacleLayer;

    [Header("Movement")]
    public float moveSpeed;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Opacity")]
    [Range(0f, 1f)] public float grassOpacity = 0.49f;
    public float normalOpacity = 1f;

    private NavMeshAgent agent;
    private SpriteRenderer[] spriteRenderers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = moveSpeed;
        agent.acceleration = 50f;
        agent.stoppingDistance = 0.01f;
        agent.autoBraking = true;
    }

    void Update()
    {
        HandleMouseClick();
        RotateTowardsMovement();
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(clickPos, obstacleLayer);

            if (hit != null)
            {
                return;
            }

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(clickPos);
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
