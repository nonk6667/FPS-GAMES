using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 60f;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Disable Gameplay Objects On End")]
    [SerializeField] private GameObject[] gameplayRootsToDisable;

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
    }

    private void Start()
    {
        StartMatch();
    }

    public void StartMatch()
    {
        IsGameOver = false;
        timeLeft = matchDuration;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeLeft);
        timerText.text = $"Time: {seconds}";
    }

    private void EndMatch()
    {
        IsGameOver = true;

        if (gameplayRootsToDisable != null)
        {
            foreach (var go in gameplayRootsToDisable)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int score = 0;
        if (ScoreManager.Instance != null)
            score = ScoreManager.Instance.GetScore();

        if (finalScoreText != null)
            finalScoreText.text = $"Time Up!\nFinal Score: {score}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}