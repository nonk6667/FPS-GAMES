using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private Health[] enemies;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Level2_Combat";

    [Header("Delay")]
    [SerializeField] private float loadDelay = 2f;

    private bool levelCompleted = false;

    private void Update()
    {
        if (levelCompleted) return;

        foreach (Health enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
                return;
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
        SceneManager.LoadScene(nextSceneName);
    }
}