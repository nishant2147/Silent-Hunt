using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject homePanel;
    public PlayerMovement player;
    public DoorSlide door;
    public Transform exitPoint;
    void Start()
    {
        homePanel.SetActive(true);
        GameManager.Instance.isGameStarted = false;
    }

    public void StartGame()
    {
        homePanel.SetActive(false);
        GameManager.Instance.isGameStarted = true;       

        door.OpenDoor();
               
        player.StartAutoMove(exitPoint);
    }
}
