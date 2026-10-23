using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Счёт, таймер, экран Game Over, перезапуск.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI gameOverText; // необязательно: итоговый счёт
    [SerializeField] private GameObject gameOverPanel;

    // Подписи. Стандартный шрифт TMP не содержит кириллицы,
    // поэтому по умолчанию английский. Для русского создайте Font Asset
    // с диапазоном 20-7E,400-4FF и замените строки ниже.
    private const string ScoreLabel = "Score: ";
    private const string TimeLabel = "Time: ";
    private const string GameOverLabel = "Game Over\nScore: ";

    private float timeLeft = 30f;
    private int score = 0;
    private bool isRunning = true;

    public bool IsRunning => isRunning;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        scoreText.text = ScoreLabel + "0";
        timerText.text = TimeLabel + "30";
    }

    private void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;
        // CeilToInt: показываем 30..1, а не 29.99 и т.п.
        timerText.text = TimeLabel + Mathf.CeilToInt(timeLeft);

        if (timeLeft <= 0f) GameOver();
    }

    public void AddScore(int value)
    {
        score += value;
        scoreText.text = ScoreLabel + score;
    }

    private void GameOver()
    {
        isRunning = false;
        timerText.text = TimeLabel + "0";
        if (gameOverText != null) gameOverText.text = GameOverLabel + score;
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
