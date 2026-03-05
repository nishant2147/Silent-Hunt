using UnityEngine;

public class LevelReset : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;
    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void ResetObject()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        gameObject.SetActive(true);
    }
}
