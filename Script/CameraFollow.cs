using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Follow Settings")]
    public float smoothTime = 0.15f;

    [Header("Camera Clamp Limits")]
    public Vector2 minLimit;
    public Vector2 maxLimit;

    private Vector3 velocity = Vector3.zero;
    private float fixedZ;
    private Camera cam;

    void Start()
    {
        if (player == null) return;

        cam = GetComponent<Camera>();
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(
            player.position.x,
            player.position.y,
            fixedZ
        );

        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        smoothPos.x = Mathf.Clamp(
            smoothPos.x,
            minLimit.x + halfWidth,
            maxLimit.x - halfWidth
        );

        smoothPos.y = Mathf.Clamp(
            smoothPos.y,
            minLimit.y + halfHeight,
            maxLimit.y - halfHeight
        );

        smoothPos.z = fixedZ;

        transform.position = smoothPos;
    }
}
