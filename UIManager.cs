using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Start Screen UI")]
    public GameObject startPanel;
    public TextMeshProUGUI startMoneyText;
    public TextMeshProUGUI startLevelText;
    public TextMeshProUGUI slideToMoveText;

    [Header("Game UI")]
    public TextMeshProUGUI gameMoneyText;
    public TextMeshProUGUI gameCollectableCountText;
    public TextMeshProUGUI gameLevelText;

    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI levelCompleteMoneyText;
    public TextMeshProUGUI levelCompleteCountText;
    public TextMeshProUGUI totalMoneyText;
    public Button nextLevelButton;
    public Button restartButton;

    [Header("Conveyor Belt UI")]
    public TextMeshProUGUI conveyorMoneyText; 
    public TextMeshProUGUI conveyorCountText; 

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Button restartGameButton;

    private PlayerController player;
    private LevelManager levelManager;
    private GameManager gameManager;
    private bool gameStarted = false;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        levelManager = FindObjectOfType<LevelManager>();
        gameManager = FindObjectOfType<GameManager>();

        SetupButtonListeners();
        ShowStartScreen();
    }

    void Update()
    {
        if (!gameStarted && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }

        if (gameStarted && player != null)
        {
            UpdateGameUI();
        }
    }

    private void SetupButtonListeners()
    {
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() => levelManager?.NextLevel());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => levelManager?.RestartLevel());

        if (restartGameButton != null)
            restartGameButton.onClick.AddListener(() => levelManager?.RestartGame());
    }

    public void ShowStartScreen()
    {
        gameStarted = false;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (startMoneyText != null && player != null)
            startMoneyText.text = $"Money: ${player.permanentMoney:F0}";

        if (startLevelText != null && levelManager != null)
            startLevelText.text = $"Level {levelManager.currentLevel}";

        if (slideToMoveText != null)
            slideToMoveText.text = "Slide to Move";
    }

    private void StartGame()
    {
        gameStarted = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (slideToMoveText != null)
            slideToMoveText.gameObject.SetActive(false);

        if (gameManager != null)
            gameManager.StartGame();
    }

    public void UpdateGameUI()
    {
        if (gameMoneyText != null && player != null)
        {
            gameMoneyText.text = $"${player.inGameMoney:F0}";
        }

        if (gameCollectableCountText != null && player != null)
            gameCollectableCountText.text = $"Collected: {player.collectedList.Count}";

        if (gameLevelText != null && levelManager != null)
            gameLevelText.text = $"Level {levelManager.currentLevel}";
    }

    public void UpdateLevelMoney(float money)
    {
        if (conveyorMoneyText != null)
            conveyorMoneyText.text = $"Level Money: ${money:F0}";

        if (levelCompleteMoneyText != null)
            levelCompleteMoneyText.text = $"Level Money: ${money:F0}";
    }

    public void UpdateCollectableCount(int count)
    {
        if (conveyorCountText != null)
            conveyorCountText.text = $"Items Processed: {count}";

        if (levelCompleteCountText != null)
            levelCompleteCountText.text = $"Items Collected: {count}";
    }

    public void ShowLevelComplete(float levelMoney, int itemCount, float totalMoney)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (levelCompleteMoneyText != null)
                levelCompleteMoneyText.text = $"Level Money: ${levelMoney:F0}";

            if (levelCompleteCountText != null)
                levelCompleteCountText.text = $"Items Collected: {itemCount}";

            if (totalMoneyText != null)
                totalMoneyText.text = $"Total Money: ${totalMoney:F0}";
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void SetConveyorUIActive(bool active)
    {
        if (conveyorMoneyText != null)
            conveyorMoneyText.gameObject.SetActive(active);

        if (conveyorCountText != null)
            conveyorCountText.gameObject.SetActive(active);
    }
}