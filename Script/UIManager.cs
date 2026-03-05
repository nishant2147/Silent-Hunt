using System;
using System.Collections;
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

        if (door != null)
            door.OpenDoor();

        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        door.OpenDoor();

        yield return new WaitForSeconds(0.3f);

        player.StartAutoMove(exitPoint);
    }

    public void ShowHome()
    {
        homePanel.SetActive(true);
        GameManager.Instance.isGameStarted = false;
    }
}
