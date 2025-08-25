using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Start Screen UI")]
    public GameObject startPanel;
    public TextMeshProUGUI startMoneyText;
    public TextMeshProUGUI startLevelText;
    public TextMeshProUGUI tapToPlayText;

    [Header("Game UI")]
    public GameObject gameUIParent;
    public TextMeshProUGUI gameLevelText;
    public TextMeshProUGUI permanentMoneyText;
    public TextMeshProUGUI currentLevelMoneyText; 
    public Button restartButton;

    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI levelCompleteMoneyText;
    public TextMeshProUGUI totalMoneyText;
    public Button continueButton;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;

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
        if (restartButton != null)
            restartButton.onClick.AddListener(() => levelManager.RestartLevel());

        if (continueButton != null)
            continueButton.onClick.AddListener(() =>
            {
                levelManager.NextLevel(); 
                ShowStartScreen();           
            });

        if (retryButton != null)
            retryButton.onClick.AddListener(() => levelManager.RestartLevel());
    }

    public void ShowStartScreen()
    {
        gameStarted = false;

        startPanel?.SetActive(true);
        gameUIParent?.SetActive(false);
        levelCompletePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        if (startMoneyText != null && player != null)
            startMoneyText.text = $"${player.permanentMoney:F0}";

        if (startLevelText != null && levelManager != null)
            startLevelText.text = $"Level {levelManager.currentLevel}";
    }

    private void StartGame()
    {
        gameStarted = true;
        startPanel?.SetActive(false);
        tapToPlayText?.gameObject.SetActive(false);
        gameUIParent?.SetActive(true);

        gameManager?.StartGame();
    }

    public void UpdateGameUI()
    {
        if (gameLevelText != null && levelManager != null)
            gameLevelText.text = $"Level {levelManager.currentLevel}";

        if (permanentMoneyText != null && player != null)
            permanentMoneyText.text = $"${player.permanentMoney:F0}";

        if (currentLevelMoneyText != null && player != null)
            currentLevelMoneyText.text = $"${player.inGameMoney:F0}";
    }

    public void ShowLevelComplete(float levelMoney, int itemCount, float totalMoney)
    {
        gameUIParent?.SetActive(false);
        levelCompletePanel?.SetActive(true);

        if (levelCompleteMoneyText != null)
            levelCompleteMoneyText.text = $"${levelMoney:F0}";

        if (totalMoneyText != null)
            totalMoneyText.text = $"${totalMoney:F0}";
    }

    public void ShowGameOver()
    {
        gameUIParent?.SetActive(false);
        gameOverPanel?.SetActive(true);
    }
}
