using TMPro;
using UnityEngine;

public class EnemyCounterManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private Health[] enemies;

    [Header("UI")]
    [SerializeField] private TMP_Text enemyCounterText;

    private int totalEnemies;

    private void Start()
    {
        totalEnemies = enemies.Length;
        UpdateEnemyCounterUI();
    }

    private void Update()
    {
        UpdateEnemyCounterUI();
    }

    private void UpdateEnemyCounterUI()
    {
        if (enemyCounterText == null) return;

        int aliveEnemies = 0;

        foreach (Health enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                aliveEnemies++;
            }
        }

        enemyCounterText.text = "Enemies: " + aliveEnemies + " / " + totalEnemies;
    }
}