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
                "Mission 1: Tutorial Training\n\n" +
                "Read the signs ahead of your spawn point.\n" +
                "Practice movement, jumping, crouching, and shooting.\n" +
                "Eliminate the first marked Bot to complete Level 1.";
        }

        Invoke(nameof(HideMissionPanel), displayTime);
    }

    private void HideMissionPanel()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);
    }
}
