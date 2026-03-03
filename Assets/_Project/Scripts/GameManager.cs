using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private float matchDuration = 60f;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;   // 一个Panel
    [SerializeField] private TMP_Text finalScoreText;    // Panel里的文字

    [Header("Optional: disable control on end")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable; // FPSController / GunShoot等

    private float timeLeft;
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 不建议这里用，先别跨场景
    }

    private void Start()
    {
        StartMatch();
    }

    public void StartMatch()
    {
        IsGameOver = false;
        timeLeft = matchDuration;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateTimerUI();
    }

    private void Update()
    {
        if (IsGameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndMatch();
        }

        UpdateTimerUI();

        // 方便测试：按 R 重开
        if (Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int sec = Mathf.CeilToInt(timeLeft);
        timerText.text = $"Time: {sec}";
    }

    private void EndMatch()
    {
        IsGameOver = true;

        // 禁用控制（可选但建议）
        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
                if (s != null) s.enabled = false;
        }

        // 弹 GameOver 面板 + 最终分数
        int score = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetScore() : 0;

        if (finalScoreText != null)
            finalScoreText.text = $"Time Up!\nFinal Score: {score}\nPress R to Restart";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}