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
    [SerializeField] private GunShoot gunShoot;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip akReloadSound;
    [SerializeField] private AudioClip pistolReloadSound;

    [Header("UI")]
    [SerializeField] private TMP_Text ammoText;

    private bool isReloading;

    public bool IsReloading => isReloading;

    private void Start()
    {
        currentAmmo = magazineSize;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ConfigureSfxAudioSource();

        if (gunShoot == null)
            gunShoot = GetComponent<GunShoot>();

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
        AudioClip selectedReloadSound = GetCurrentReloadSound();
        if (audioSource != null && selectedReloadSound != null)
        {
            audioSource.PlayOneShot(selectedReloadSound);
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
            ammoText.text = currentAmmo + " / " + spareMagazines;
        }
    }

    private AudioClip GetCurrentReloadSound()
    {
        if (gunShoot == null)
        {
            return reloadSound;
        }

        if (gunShoot.IsUsingAK74)
        {
            return akReloadSound != null ? akReloadSound : reloadSound;
        }

        return pistolReloadSound != null ? pistolReloadSound : reloadSound;
    }

    private void ConfigureSfxAudioSource()
    {
        if (audioSource == null) return;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.dopplerLevel = 0f;
        audioSource.bypassReverbZones = true;
    }
}
