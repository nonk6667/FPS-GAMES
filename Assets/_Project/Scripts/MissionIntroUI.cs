using UnityEngine;
using TMPro;

public class MissionIntroUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private TMP_Text missionText;

    [Header("Settings")]
    [SerializeField] private float displayTime = 5f;

    private void Start()
    {
        if (missionPanel != null)
            missionPanel.SetActive(true);

        if (missionText != null)
        {
            missionText.text =
                "Mission 1: Infiltrate the Factory\n\n" +
                "Avoid staying too close to enemy guards.\n" +
                "Crouch to reduce suspicion.\n" +
                "Eliminate all enemies to enter the factory.";
        }

        Invoke(nameof(HideMissionPanel), displayTime);
    }

    private void HideMissionPanel()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);
    }
}