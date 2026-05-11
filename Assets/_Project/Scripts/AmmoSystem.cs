using System.Collections;
using TMPro;
using UnityEngine;

public class AmmoSystem : MonoBehaviour
{
    [Header("Magazine")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int currentAmmo = 30;

    [Header("Reserve Magazines")]
    [SerializeField] private int spareMagazines = 2;

    [Header("Reload")]
    [SerializeField] private float reloadTime = 1.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reloadSound;

    [Header("UI")]
    [SerializeField] private TMP_Text ammoText;

    private bool isReloading;

    public bool IsReloading => isReloading;

    private void Start()
    {
        currentAmmo = magazineSize;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }

    public bool CanShoot()
    {
        return !isReloading && currentAmmo > 0;
    }

    public void UseBullet()
    {
        if (isReloading) return;
        if (currentAmmo <= 0) return;

        currentAmmo--;

        UpdateUI();
    }

    private void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo >= magazineSize) return;
        if (spareMagazines <= 0) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        // 播放换弹音效
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        UpdateUI();

        yield return new WaitForSeconds(reloadTime);

        spareMagazines--;

        currentAmmo = magazineSize;

        isReloading = false;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (ammoText == null) return;

        if (isReloading)
        {
            ammoText.text = "Reloading...";
        }
        else
        {
            ammoText.text = currentAmmo + " / " + spareMagazines + " mags";
        }
    }
}