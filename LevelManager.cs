using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevels = 10;

    [Header("References")]
    public Transform playerStartPoint;
    public GameObject[] levelPrefabs;
    public ConveyorBelt conveyorBelt;

    private UIManager uiManager;
    private PlayerController player;
    private GameManager gameManager;
    private bool levelCompleted = false;
    private GameObject currentLevelInstance;
    private Vector3 levelWorldPosition = new Vector3(-4, 3, 120); 

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        InitializeLevel();
    }

    private void InitializeLevel()
    {
        levelCompleted = false;

        if (currentLevelInstance != null)
            Destroy(currentLevelInstance);

        if (levelPrefabs.Length >= currentLevel)
        {
            currentLevelInstance = Instantiate(
                levelPrefabs[currentLevel - 1],
                levelWorldPosition,
                Quaternion.identity
            );
        }

        if (player != null && playerStartPoint != null)
        {
            player.transform.position = playerStartPoint.position;
            player.transform.rotation = playerStartPoint.rotation;
            player.collectedList.Clear();
            player.inGameMoney = 0f;
            player.enabled = true;
            //player.speed = 5f + (currentLevel - 1) * 0.5f;
        }

        uiManager?.ShowStartScreen();
    }

    public void LevelFinished(float levelMoney, int itemCount)
    {
        levelCompleted = true;

        player.permanentMoney += levelMoney;

        if (levelMoney > 0)
            uiManager?.ShowLevelComplete(levelMoney, itemCount, player.permanentMoney);
        else
            uiManager?.ShowGameOver();
    }

    public void RestartLevel()
    {
        InitializeLevel();
        gameManager?.StartGame();
    }

    public void NextLevel()
    {
        if (currentLevel < maxLevels)
            currentLevel++;

        InitializeLevel();
    }
}
