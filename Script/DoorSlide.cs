using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DoorSlide : MonoBehaviour
{
    public enum SlideAxis { X, Y }
    public SlideAxis slideAxis;

    public float slideDistance = 2f;
    public float slideSpeed = 3f;
    public float closeDelay = 0.6f;

    private Vector3 closedPos;
    private Vector3 openPos;

    private Coroutine slideRoutine;
    private Coroutine closeRoutine;
    private bool isOpen;

    private NavMeshObstacle navObstacle;

    void Start()
    {
        navObstacle = GetComponent<NavMeshObstacle>();

        closedPos = transform.position;

        openPos = slideAxis == SlideAxis.X
            ? closedPos + Vector3.right * slideDistance
            : closedPos + Vector3.up * slideDistance;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameManager.Instance.isGameStarted)
            return;

        if (!IsPlayerOrEnemy(collision.collider)) return;

        OpenDoor();

        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(CloseAfterDelay());
    }
    bool IsPlayerOrEnemy(Collider2D col)
    {
        return col.CompareTag("Player") || col.CompareTag("Enemy");
    }

    void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        navObstacle.enabled = false;

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(Slide(openPos));
    }

    void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        navObstacle.enabled = true; 

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(Slide(closedPos));
    }


    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        CloseDoor();
    }

    IEnumerator Slide(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;
    }
}
