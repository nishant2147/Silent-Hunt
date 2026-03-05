using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted;
    public Vector3 lastKillPosition;
    public bool globalAlert;

    public Text enemyText;

    private int totalEnemies;
    private int killedEnemies;

    [Header("Levels")]
    public GameObject[] levels;

    public GameObject gameCompletePanel;

    private GameObject currentLevel;
    private int currentLevelIndex = 0;
   
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameCompletePanel.SetActive(false);
        DetectCurrentLevel();
        SetupEnemies();

        SpawnLevel();
    }
    void SpawnLevel()
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        currentLevel = Instantiate(levels[currentLevelIndex], Vector3.zero, Quaternion.identity);

        SetupEnemies();

        CameraFollow cam = FindObjectOfType<CameraFollow>();

        if (cam != null)
        {
            cam.FindPlayer();
        }
    }
    void DetectCurrentLevel()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].activeSelf)
            {
                currentLevelIndex = i;
                break;
            }
        }
    }
    void SetupEnemies()
    {
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        killedEnemies = 0;

        UpdateUI();
    }

    public void EnemyKilled()
    {
        killedEnemies++;

        UpdateUI();

        if (killedEnemies >= totalEnemies)
        {
            LevelComplete();
        }
    }

    void UpdateUI()
    {
        enemyText.text = killedEnemies + "/" + totalEnemies;
    }
    void LevelComplete()
    {
        Debug.Log("Level Complete");

        isGameStarted = false;

        gameCompletePanel.SetActive(true);
    }

    public void LoadNextLevel()
    {
        gameCompletePanel.SetActive(false);

        levels[currentLevelIndex].SetActive(false);

        currentLevelIndex++;

        if (currentLevelIndex < levels.Length)
        {
            levels[currentLevelIndex].SetActive(true);

            SetupEnemies();
            SpawnLevel();

            UIManager.Instance.ShowHome();
            //isGameStarted = true;
        }
        else
        {
            Debug.Log("All Levels Completed");
        }
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
