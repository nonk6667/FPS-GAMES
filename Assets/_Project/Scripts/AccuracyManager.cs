using TMPro;
using UnityEngine;

public class AccuracyManager : MonoBehaviour
{
    public static AccuracyManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text accuracyText;

    private int shotsFired;
    private int shotsHit;

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
        UpdateUI();
    }

    public void RegisterShot()
    {
        shotsFired++;
        UpdateUI();
    }

    public void RegisterHit()
    {
        shotsHit++;
        UpdateUI();
    }

    public float GetAccuracy()
    {
        if (shotsFired <= 0) return 100f;
        return (float)shotsHit / shotsFired * 100f;
    }

    private void UpdateUI()
    {
        if (accuracyText == null) return;
        accuracyText.text = $"Accuracy: {GetAccuracy():0}%";
    }
}