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
          //  uiManager.UpdateUI();
        }

        LoadLevelContent();
    }

    private void LoadLevelContent()
    {
        ClearLevelContent();

        //if (levelPrefabs != null && levelPrefabs.Length > 0)
        //{
        //    int levelIndex = Mathf.Min(currentLevel - 1, levelPrefabs.Length - 1);
        //    if (levelPrefabs[levelIndex] != null)
        //    {
        //        Instantiate(levelPrefabs[levelIndex]);
        //    }
        //}
    }

    private void ClearLevelContent()
    {
       // GameObject[] collectables = GameObject.FindGameObjectsWithTag("Collectable");
        //GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        //foreach (GameObject obj in collectables)
        //{
        //    Destroy(obj);
        //}

        //foreach (GameObject obj in obstacles)
        //{
        //    Destroy(obj);
        //}
    }

    private void CheckLevelProgress()
    {
        if (player == null || levelEndPoint == null) return;

        float distanceToEnd = Vector3.Distance(player.transform.position, levelEndPoint.position);
        float totalDistance = Vector3.Distance(levelStartPoint.position, levelEndPoint.position);
        float progress = 1f - (distanceToEnd / totalDistance);

        if (uiManager != null)
        {
          //  uiManager.UpdateProgress(Mathf.Clamp01(progress));
        }

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
        }
        else
        {
            CompleteLevel(0f, 0); //fallback
        }
    }

    public void CompleteLevel(float levelMoney, int itemCount)
    {
        levelCompleted = true;

        float levelTime = Time.time - levelStartTime;
;

        if (uiManager != null)
        {
            uiManager.ShowLevelComplete(levelMoney, itemCount, player.moneyAmount);
        }

        SaveProgress();

        Debug.Log($"Level {currentLevel} completed! Money: ${levelMoney}, Items: {itemCount}, Time: {levelTime:F1}s");
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
           // uiManager.HideAllPanels();
        }

        if (player != null && levelStartPoint != null)
        {
            player.transform.position = levelStartPoint.position;
            player.transform.rotation = levelStartPoint.rotation;
            player.collectedList.Clear();
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
            player.moneyAmount = 0f;
        }
        RestartLevel();
    }

    private void ShowGameComplete()
    {
        Debug.Log("Congratulations! You completed all levels!");
     
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetFloat("TotalMoney", player?.moneyAmount ?? 0f);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (player != null)
        {
            player.moneyAmount = PlayerPrefs.GetFloat("TotalMoney", 0f);
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
            player.moneyAmount = 0f;
        }
        RestartLevel();
    }
}