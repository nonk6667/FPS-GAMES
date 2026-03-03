using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    [Header("Death & Score")]
    [SerializeField] private int deathScore = 10;     // ✅ 敌人死亡加多少分
    [SerializeField] private bool awardScoreOnDeath = true;

    [Header("Respawn")]
    [SerializeField] private bool enableRespawn = true;
    [SerializeField] private float respawnDelay = 3f;

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private Renderer[] _renderers;
    private Collider[] _colliders;

    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        currentHP = maxHP;

        _spawnPos = transform.position;
        _spawnRot = transform.rotation;

        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHP = Mathf.Max(0f, currentHP - damage);
        Debug.Log($"[Health] {name} took {damage}, HP={currentHP}/{maxHP}");

        if (currentHP <= 0f)
            Die();
    }

    private void Die()
    {
        Debug.Log($"[Health] {name} died");

        // ✅ 加分（通常只给敌人：你把 Enemy 的 awardScoreOnDeath 勾上即可）
        if (awardScoreOnDeath && ScoreManager.Instance != null && deathScore > 0)
        {
            ScoreManager.Instance.AddScore(deathScore);
        }

        // 隐藏 + 关闭碰撞
        foreach (var r in _renderers) r.enabled = false;
        foreach (var c in _colliders) c.enabled = false;

        if (enableRespawn)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        transform.position = _spawnPos;
        transform.rotation = _spawnRot;

        currentHP = maxHP;

        foreach (var r in _renderers) r.enabled = true;
        foreach (var c in _colliders) c.enabled = true;

        Debug.Log($"[Health] {name} respawned");
    }
}