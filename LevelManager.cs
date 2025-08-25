using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevels = 10;
    public float levelLength = 100f;

    [Header("Level Progression")]
    public float speedIncreasePerLevel = 0.5f;
    public int obstacleIncreasePerLevel = 2;

    [Header("References")]
    public Transform levelStartPoint;
    public Transform levelEndPoint;
    public GameObject[] levelPrefabs;
    public ConveyorBelt conveyorBelt;

    private UIManager uiManager;
    private PlayerController player;
    private GameManager gameManager;
    private bool levelCompleted = false;
    private float levelStartTime;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        InitializeLevel();
    }

    void Update()
    {
        if (player != null && !levelCompleted)
        {
            CheckLevelProgress();
        }
    }

    private void InitializeLevel()
    {
        levelStartTime = Time.time;
        levelCompleted = false;

        if (player != null)
        {
            player.speed = 5f + (currentLevel - 1) * speedIncreasePerLevel;
        }

        if (uiManager != null)
        {
            uiManager.SetConveyorUIActive(false);
        }

        LoadLevelContent();
    }

    private void LoadLevelContent()
    {
        ClearLevelContent();
        //sonra doldurcam
    }

    private void ClearLevelContent()
    {
        //sonra doldurcam
    }

    private void CheckLevelProgress()
    {
        if (player == null || levelEndPoint == null) return;

        float distanceToEnd = Vector3.Distance(player.transform.position, levelEndPoint.position);
        float totalDistance = Vector3.Distance(levelStartPoint.position, levelEndPoint.position);
        float progress = 1f - (distanceToEnd / totalDistance);


        if (distanceToEnd < 5f && !levelCompleted)
        {
            TriggerLevelEnd();
        }
    }

    private void TriggerLevelEnd()
    {
        if (conveyorBelt != null)
        {
            levelCompleted = true;

            if (uiManager != null)
            {
                uiManager.SetConveyorUIActive(true);
            }
        }
        else
        {
            CompleteLevel(0f, 0); // fallback
        }
    }

    public void CompleteLevel(float levelMoney, int itemCount)
    {
        levelCompleted = true;

        float levelTime = Time.time - levelStartTime;

        if (uiManager != null)
        {
            uiManager.SetConveyorUIActive(false);
            uiManager.ShowLevelComplete(levelMoney, itemCount, player.permanentMoney);
        }

        SaveProgress();

        Debug.Log($"Level {currentLevel} completed! Money: ${levelMoney}, Items: {itemCount}, Time: {levelTime:F1}s");
        Debug.Log($"Player permanent money: ${player.permanentMoney}");
    }

    public void NextLevel()
    {
        if (currentLevel < maxLevels)
        {
            currentLevel++;
            RestartLevel();
        }
        else
        {
            ShowGameComplete();
        }
    }

    public void RestartLevel()
    {
        if (uiManager != null)
        {
            uiManager.SetConveyorUIActive(false);
        }

        if (player != null && levelStartPoint != null)
        {
            player.transform.position = levelStartPoint.position;
            player.transform.rotation = levelStartPoint.rotation;
            player.collectedList.Clear();
            player.inGameMoney = 0f; 
            player.enabled = true;
        }

        InitializeLevel();

        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }

    public void RestartGame()
    {
        currentLevel = 1;
        if (player != null)
        {
            player.permanentMoney = 0f; 
            player.inGameMoney = 0f;
        }
        RestartLevel();
    }

    private void ShowGameComplete()
    {
        Debug.Log("Congratulations! You completed all levels!");
        Debug.Log($"Final money: ${player.permanentMoney}");
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetFloat("PermanentMoney", player?.permanentMoney ?? 0f);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (player != null)
        {
            player.permanentMoney = PlayerPrefs.GetFloat("PermanentMoney", 0f);
        }
    }

    public void LoadGame()
    {
        LoadProgress();
        RestartLevel();
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        currentLevel = 1;
        if (player != null)
        {
            player.permanentMoney = 0f;
            player.inGameMoney = 0f;
        }
        RestartLevel();
    }
}