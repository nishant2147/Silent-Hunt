using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted;
    public Vector3 lastKillPosition;
    public bool globalAlert;

    void Awake()
    {
        Instance = this;
    }

    public void AlertEnemies(Vector3 killPos)
    {
        lastKillPosition = killPos;
        globalAlert = true;
    }

    public void ResetAlert()
    {
        globalAlert = false;
    }
}
