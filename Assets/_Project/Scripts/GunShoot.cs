using System.Collections;
using UnityEngine;

public class GunShoot : MonoBehaviour
{
    public enum FireMode { SemiAuto, FullAuto }

    [System.Serializable]
    public class WeaponProfile
    {
        public string weaponName = "Weapon";
        public FireMode fireMode = FireMode.SemiAuto;

        [Header("Fire")]
        public float fireInterval = 0.17f;
        public float range = 100f;

        [Header("Spread")]
        public float baseSpread = 0.1f;
        public float movePenalty = 0.3f;
        public float jumpPenalty = 0.6f;
        public float crouchBonus = 0.05f;
        public float shotSpreadIncrease = 0.12f;
        public float spreadRecoverySpeed = 2f;

        [Header("Recoil")]
        public float kickDegrees = 0.9f;
        public float recoilSnappiness = 25f;
        public float recoilReturnSpeed = 12f;
        public float recoilReturnDelay = 0.08f;
    }

    [Header("Weapon Profiles")]
    [SerializeField] private WeaponProfile pistol = new WeaponProfile
    {
        weaponName = "Pistol",
        fireMode = FireMode.SemiAuto,
        fireInterval = 0.17f,
        kickDegrees = 0.7f
    };

    [SerializeField] private WeaponProfile ak74 = new WeaponProfile
    {
        weaponName = "AK74",
        fireMode = FireMode.FullAuto,
        fireInterval = 0.10f,
        kickDegrees = 1.2f
    };

    [Header("Weapon Selection")]
    [SerializeField] private bool useAK74 = true;

    [Header("Weapon Visuals")]
    [SerializeField] private GameObject akVisual;
    [SerializeField] private GameObject pistolVisual;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform recoilPivot;
    [SerializeField] private LineRenderer tracer;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Muzzle Points")]
    [SerializeField] private Transform akMuzzle;
    [SerializeField] private Transform pistolMuzzle;

    [Header("Tracer")]
    [SerializeField] private float tracerDuration = 0.05f;
    [SerializeField] private float tracerWidth = 0.015f;

    [Header("Auto Find")]
    [SerializeField] private string muzzlePointName = "MuzzlePoint";

    [Header("Hit FX")]
    [SerializeField] private GameObject hitSparkPrefab; // wall
    [SerializeField] private GameObject hitBloodPrefab; // enemy
    [SerializeField] private float hitFxLifetime = 2f;
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("Damage")]
    [SerializeField] private float pistolDamage = 25f;
    [SerializeField] private float akDamage = 12f;
    [SerializeField] private bool damageEnemyOnly = true;  // true = 只打 Enemy 扣血
    [SerializeField] private bool debugDamageLog = true;   // 输出扣血信息

    [Header("Debug")]
    [SerializeField] private bool logMuzzleEachShot = false;
    [SerializeField] private bool debugDrawRays = false;
    [SerializeField] private float debugRayTime = 0.5f;
    [SerializeField] private bool debugHitLog = true;     // HIT/MISS & object
    [SerializeField] private bool debugFxLog = true;      // FX spawn logs

    private WeaponProfile W => useAK74 ? ak74 : pistol;
    private float CurrentDamage => useAK74 ? akDamage : pistolDamage;

    private Transform CurrentMuzzle
    {
        get
        {
            Transform m = useAK74 ? akMuzzle : pistolMuzzle;

            if (m == null)
            {
                GameObject vis = useAK74 ? akVisual : pistolVisual;
                if (vis != null)
                {
                    Transform found = FindDeepChild(vis.transform, muzzlePointName);
                    if (found != null) m = found;
                }
            }

            return m;
        }
    }

    private CharacterController _controller;
    private Transform _recoilPivot;
    private Quaternion _basePivotRot;

    private float _nextFireTime;
    private float _shotSpreadExtra;

    private float _recoilTarget;
    private float _recoilCurrent;
    private float _recoilVel;
    private float _lastShotTime;

    private Coroutine _tracerRoutine;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        _controller = GetComponent<CharacterController>();

        _recoilPivot = recoilPivot;
        if (_recoilPivot == null && playerCamera != null)
            _recoilPivot = playerCamera.transform.parent;

        if (_recoilPivot != null)
            _basePivotRot = _recoilPivot.localRotation;

        SetupTracer();
        ApplyWeaponVisuals();
    }

    private void Update()
    {
        HandleWeaponSwitchInput();
        HandleFireInput();
        UpdateSpread(Time.deltaTime);
        UpdateRecoil(Time.deltaTime);
    }

    private void HandleWeaponSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeapon(true);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeapon(false);
    }

    private void SetWeapon(bool toAK)
    {
        if (useAK74 == toAK) return;

        useAK74 = toAK;

        _shotSpreadExtra = 0f;
        _recoilTarget = 0f;
        _recoilCurrent = 0f;
        _recoilVel = 0f;
        _nextFireTime = 0f;
        _lastShotTime = 0f;

        if (_recoilPivot != null)
            _recoilPivot.localRotation = _basePivotRot;

        if (tracer != null)
            tracer.enabled = false;

        ApplyWeaponVisuals();
    }

    private void ApplyWeaponVisuals()
    {
        if (akVisual != null) akVisual.SetActive(useAK74);
        if (pistolVisual != null) pistolVisual.SetActive(!useAK74);
    }

    private void SetupTracer()
    {
        if (tracer == null) return;

        tracer.positionCount = 2;
        tracer.useWorldSpace = true;
        tracer.enabled = false;

        tracer.startWidth = tracerWidth;
        tracer.endWidth = tracerWidth;
    }

    private void HandleFireInput()
    {
        bool wantShoot = (W.fireMode == FireMode.FullAuto)
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (!wantShoot) return;
        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + W.fireInterval;
        ShootOnce();
    }

    /// <summary>
    /// Hit detection uses CAMERA ray (matches crosshair),
    /// tracer uses MUZZLE -> hitPoint for visuals.
    /// </summary>
    private void ShootOnce()
    {
        if (playerCamera == null) return;

        Transform m = CurrentMuzzle;
        if (m == null) return;

        if (logMuzzleEachShot)
            Debug.Log($"[GunShoot] Weapon={W.weaponName}, Muzzle={m.name}, MuzzlePos={m.position}");

        _lastShotTime = Time.time;

        _shotSpreadExtra += W.shotSpreadIncrease;
        _recoilTarget += W.kickDegrees;

        // ---- CAMERA RAY (crosshair) ----
        Vector3 camOrigin = playerCamera.transform.position;
        Vector3 camDir = playerCamera.transform.forward;

        float spread = ComputeCurrentSpreadDegrees();
        float yaw = Random.Range(-spread, spread);
        float pitch = Random.Range(-spread, spread);
        Vector3 shotDir = (Quaternion.Euler(pitch, yaw, 0f) * camDir).normalized;

        if (debugDrawRays)
            Debug.DrawRay(camOrigin, shotDir * W.range, Color.cyan, debugRayTime);

        Vector3 hitPoint = camOrigin + shotDir * W.range;

        bool didHit = Physics.Raycast(
            camOrigin,
            shotDir,
            out RaycastHit hit,
            W.range,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        if (debugHitLog)
        {
            Debug.Log(didHit ? "HIT" : "MISS");
            if (didHit)
                Debug.Log($"Hit object: {hit.collider.name} | Layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }

        if (didHit)
        {
            hitPoint = hit.point;

            // ✅ 先处理伤害
            TryApplyDamage(hit);

            // ✅ 再生成特效（敌人喷血 / 墙面火花）
            SpawnHitFX(hit);
        }

        // ---- TRACER VISUAL: MUZZLE -> HIT POINT ----
        if (tracer != null)
        {
            tracer.useWorldSpace = true;
            tracer.enabled = true;

            Vector3 muzzleOrigin = m.position;

            if (debugDrawRays)
                Debug.DrawLine(muzzleOrigin, hitPoint, Color.red, debugRayTime);

            tracer.SetPosition(0, muzzleOrigin);
            tracer.SetPosition(1, hitPoint);

            if (_tracerRoutine != null) StopCoroutine(_tracerRoutine);
            _tracerRoutine = StartCoroutine(DisableTracer());
        }
    }

    private void TryApplyDamage(RaycastHit hit)
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        bool isEnemy = (hit.collider.gameObject.layer == enemyLayer);

        if (damageEnemyOnly && !isEnemy) return;

        // 支持：Collider 在子物体上，Health 在父物体
        Health hp = hit.collider.GetComponentInParent<Health>();
        if (hp == null) return;

        float dmg = CurrentDamage;
        hp.TakeDamage(dmg);

        if (debugDamageLog)
            Debug.Log($"[GunShoot] Damage {dmg} -> {hp.name}");
    }

    private void SpawnHitFX(RaycastHit hit)
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        bool isEnemy = (hit.collider.gameObject.layer == enemyLayer);

        GameObject prefab = isEnemy ? hitBloodPrefab : hitSparkPrefab;
        if (prefab == null)
        {
            if (debugFxLog) Debug.Log("[GunShoot] FX Prefab is NULL!");
            return;
        }

        Quaternion rot = Quaternion.LookRotation(-hit.normal);

        // 往外偏移一点，避免生成在墙体内部导致看不到
        Vector3 spawnPos = hit.point + hit.normal * 0.05f;

        GameObject fx = Instantiate(prefab, spawnPos, rot);
        Destroy(fx, hitFxLifetime);

        if (debugFxLog) Debug.Log("[GunShoot] Spawned FX at: " + spawnPos);
    }

    private IEnumerator DisableTracer()
    {
        yield return new WaitForSeconds(tracerDuration);
        if (tracer != null) tracer.enabled = false;
        _tracerRoutine = null;
    }

    private float ComputeCurrentSpreadDegrees()
    {
        float spread = W.baseSpread;

        if (_controller != null)
        {
            Vector3 hv = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            if (hv.magnitude > 0.1f) spread += W.movePenalty;
            if (!_controller.isGrounded) spread += W.jumpPenalty;
        }

        if (Input.GetKey(KeyCode.LeftControl)) spread -= W.crouchBonus;

        spread += _shotSpreadExtra;
        return Mathf.Max(0f, spread);
    }

    private void UpdateSpread(float dt)
    {
        _shotSpreadExtra = Mathf.Lerp(_shotSpreadExtra, 0f, W.spreadRecoverySpeed * dt);
    }

    private void UpdateRecoil(float dt)
    {
        if (_recoilPivot == null) return;

        if (Time.time - _lastShotTime > W.recoilReturnDelay)
            _recoilTarget = Mathf.Lerp(_recoilTarget, 0f, W.recoilReturnSpeed * dt);

        _recoilCurrent = Mathf.SmoothDamp(
            _recoilCurrent,
            _recoilTarget,
            ref _recoilVel,
            1f / Mathf.Max(1f, W.recoilSnappiness)
        );

        _recoilPivot.localRotation = _basePivotRot * Quaternion.Euler(-_recoilCurrent, 0f, 0f);
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform c = parent.GetChild(i);
            if (c.name == childName) return c;

            Transform r = FindDeepChild(c, childName);
            if (r != null) return r;
        }

        return null;
    }
}