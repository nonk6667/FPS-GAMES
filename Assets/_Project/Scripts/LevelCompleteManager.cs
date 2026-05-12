using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private Health[] enemies;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Settings")]
    [SerializeField] private float loadDelay = 2f;
    [SerializeField] private float startCheckDelay = 1f;

    private bool levelCompleted;
    private float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (levelCompleted) return;
        if (Time.time - startTime < startCheckDelay) return;
        if (enemies == null || enemies.Length == 0) return;

        foreach (Health enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                return;
            }
        }

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelCompleted = true;
        Debug.Log("LEVEL COMPLETE!");

        Invoke(nameof(LoadNextScene), loadDelay);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}