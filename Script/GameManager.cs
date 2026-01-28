using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted;

    void Awake()
    {
        Instance = this;
    }
}
