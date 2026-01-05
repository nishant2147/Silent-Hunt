using System.Collections;
using UnityEngine;

public class DoorSlide : MonoBehaviour
{
    public enum SlideAxis { X, Y }
    public SlideAxis slideAxis;

    public float slideDistance = 2f;
    public float slideSpeed = 3f;
    private float closeDelay = 0.6f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen;
    private Coroutine moveRoutine;
    private Coroutine closeRoutine;

    void Start()
    {
        closedPos = transform.position;

        if (slideAxis == SlideAxis.X)
            openPos = closedPos + Vector3.right * slideDistance;
        else
            openPos = closedPos + Vector3.up * slideDistance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        OpenDoor();

        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(CloseAfterDelay());
    }

    void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(SlideDoor(openPos));
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        CloseDoor();
    }

    void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(SlideDoor(closedPos));
    }

    IEnumerator SlideDoor(Vector3 target)
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
    }
}
