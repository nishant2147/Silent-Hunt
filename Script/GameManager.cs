using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted;
    public bool isPlayerSpotted;

    void Awake()
    {
        Instance = this;
    }
}
