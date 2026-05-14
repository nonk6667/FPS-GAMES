using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostageRescue : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Rescue")]
    [SerializeField] private float rescueTime = 5f;

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Image rescueFill;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winSound;

    [Header("Scene")]
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private float endSceneDelay = 1.5f;

    private bool rescued;
    private float currentRescueTime;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (rescueFill != null)
            rescueFill.transform.parent.gameObject.SetActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.dopplerLevel = 0f;
        audioSource.bypassReverbZones = true;
    }

    private void Update()
    {
        if (rescued) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool canInteract = distance <= interactDistance;

        if (!canInteract)
        {
            ResetProgress();
            return;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = "Hold E to rescue hostage";
        }

        if (Input.GetKey(interactKey))
        {
            currentRescueTime += Time.deltaTime;

            if (rescueFill != null)
            {
                rescueFill.transform.parent.gameObject.SetActive(true);
                rescueFill.fillAmount = currentRescueTime / rescueTime;
            }

            if (currentRescueTime >= rescueTime)
            {
                RescueHostage();
            }
        }
        else
        {
            currentRescueTime = 0f;

            if (rescueFill != null)
            {
                rescueFill.fillAmount = 0f;
                rescueFill.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void ResetProgress()
    {
        currentRescueTime = 0f;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (rescueFill != null)
        {
            rescueFill.fillAmount = 0f;
            rescueFill.transform.parent.gameObject.SetActive(false);
        }
    }

    private void RescueHostage()
    {
        rescued = true;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = "Hostage rescued!";
        }

        StartCoroutine(LoadEndSceneAfterWinSound());
    }

    private IEnumerator LoadEndSceneAfterWinSound()
    {
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
            yield return new WaitForSeconds(Mathf.Max(endSceneDelay, winSound.length));
        }
        else
        {
            yield return new WaitForSeconds(endSceneDelay);
        }

        SceneLoadHelper.LoadScene(endSceneName, "Hostage rescue");
    }
}
