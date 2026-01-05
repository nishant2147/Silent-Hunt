using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 4f;
    public float stopDistance = 0.05f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Movement Limits")]
    public float minX, maxX, minY, maxY;

    private Vector3 targetPosition;
    private bool isMoving;

    void Start()
    {
        targetPosition = transform.position;

        transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    void Update()
    {
        HandleInput();
        MoveToTarget();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPos.z = transform.position.z;

            clickPos.x = Mathf.Clamp(clickPos.x, minX, maxX);
            clickPos.y = Mathf.Clamp(clickPos.y, minY, maxY);

            targetPosition = clickPos;
            isMoving = true;
        }
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 dir = targetPosition - transform.position;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            transform.rotation = Quaternion.Lerp(
           transform.rotation,
           targetRotation,
           rotationSpeed * Time.deltaTime
       );
        }

        if (Vector3.Distance(transform.position, targetPosition) < stopDistance)
        {
            isMoving = false;
        }
    }
}
