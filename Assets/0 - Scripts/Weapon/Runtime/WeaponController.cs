using UnityEngine;

public class WeaponController : MonoBehaviour
{

    [Header("References")]
    private WeaponState currentState = WeaponState.Ready;
    public WeaponState CurrentState => currentState;

    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    private WeaponAmmo weaponAmmo;
    public WeaponAmmo WeaponAmmo => weaponAmmo;

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [SerializeField] private Camera playerCamera;
    public Camera PlayerCamera => playerCamera;

    private WeaponHit lastHit;
    public WeaponHit LastHit => lastHit;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    public float FireTimer => fireTimer;
    public float ReloadTimer => reloadTimer;
    public bool ShowDebugRay = false;
    public float DebugRayDuration = 1f;

    private float fireTimer;
    private float reloadTimer;
    private bool fireHeld;
    private int burstShotsRemaining;
    private WeaponRecoil weaponRecoil;
    private BallisticRecoil ballisticRecoil;

    private void Awake()
    {
        weaponAmmo = GetComponent<WeaponAmmo>();
        weaponRecoil = GetComponentInParent<WeaponRecoil>();
    }

    private void Update()
    {
        if (currentState == WeaponState.Firing)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                FinishFiring();

                if (weaponData.fireMode == FireMode.Auto && fireHeld)
                {
                    TryFire();
                }
                else if (weaponData.fireMode == FireMode.Burst && burstShotsRemaining > 0)
                {
                    if (!TryFire())
                    {
                        burstShotsRemaining = 0;
                    }
                }
            }
        }

        if (currentState == WeaponState.Reloading)
        {
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0f)
                FinishReloading();
        }
    }

    private bool TryGetAimPoint(out Vector3 aimPoint)
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            weaponData.range
        ))
        {
            aimPoint = hit.point;
            return true;
        }

        aimPoint = ray.origin + ray.direction * weaponData.range;
        return false;
    }

    private Vector3 GetFireDirection(Vector3 aimPoint)
    {
        Vector3 fireDirection =
            (aimPoint - firePoint.position).normalized;

        fireDirection =
            Quaternion.AngleAxis(
                -ballisticRecoil.vertical,
                playerCamera.transform.right
            ) * fireDirection;

        fireDirection =
            Quaternion.AngleAxis(
                ballisticRecoil.horizontal,
                playerCamera.transform.up
            ) * fireDirection;

        return fireDirection.normalized;
    }

    public void SetState(WeaponState newState)
    {
        currentState = newState;
    }

    public bool CanFire()
    {
        if (currentState != WeaponState.Ready)
            return false;

        if (fireTimer > 0f)
            return false;

        if (!weaponAmmo.HasAmmo())
            return false;

        return true;
    }

    public bool TryFire()
    {
        if (!CanFire())
            return false;

        if (!weaponAmmo.TryConsumeAmmo())
            return false;

        ballisticRecoil = weaponRecoil.AddRecoil(weaponData.recoilVertical);

        if (weaponData.fireMode == FireMode.Burst)
        {
            burstShotsRemaining--;
        }

        lastHit = PerformHitscan();

        fireTimer = 1f / weaponData.fireRate;

        SetState(WeaponState.Firing);

        return true;
    }

    public void StartFiring()
    {
        fireHeld = true;

        if (weaponData.fireMode == FireMode.Burst && currentState == WeaponState.Ready)
        {
            burstShotsRemaining = weaponData.burstCount;
        }

        TryFire();
    }

    public void FinishFiring()
    {
        SetState(WeaponState.Ready);
    }

    public void StopFiring()
    {
        fireHeld = false;
    }

    public bool CanReload()
    {
        if (currentState != WeaponState.Ready)
            return false;

        if (weaponAmmo.CurrentAmmo >= weaponAmmo.MagazineSize)
            return false;

        return true;
    }

    public bool TryReload()
    {
        if (!CanReload())
            return false;

        reloadTimer = weaponData.reloadTime;

        SetState(WeaponState.Reloading);

        return true;
    }
    public void FinishReloading()
    {
        weaponAmmo.RefillMagazine();

        SetState(WeaponState.Ready);
    }
    public void Disable()
    {
        SetState(WeaponState.Disabled);
    }

    public void Enable()
    {
        SetState(WeaponState.Ready);
    }

    // DEBUG

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = DebugSettingsHelper.FontSize
        };

        float lineHeight = style.fontSize + 8f;

        GUI.Label(
            new Rect(10, 10, 500, lineHeight),
            $"State: {CurrentState}",
            style
        );

        GUI.Label(
            new Rect(10, 10 + lineHeight, 500, lineHeight),
            $"Ammo: {weaponAmmo.CurrentAmmo}/{weaponAmmo.MagazineSize}",
            style
        );

        GUI.Label(
            new Rect(10, 10 + lineHeight * 2, 500, lineHeight),
            $"Fire Timer: {fireTimer:F2}",
            style
        );

        GUI.Label(
            new Rect(10, 10 + lineHeight * 3, 500, lineHeight),
            $"Reload Timer: {reloadTimer:F2}",
            style
        );

        GUI.Label(
            new Rect(10, 10 + lineHeight * 4, 500, lineHeight),
            $"Hit: {LastHit.Hit}",
            style
        );

        GUI.Label(
            new Rect(10, 10 + lineHeight * 5, 500, lineHeight),
            $"Hit Object: {(LastHit.HitObject != null ? LastHit.HitObject.name : "None")}",
            style
        );
    }

    private WeaponHit PerformHitscan()
    {
        WeaponHit weaponHit = new WeaponHit();

        if (!TryGetAimPoint(out Vector3 aimPoint))
            return weaponHit;

        Vector3 fireDirection = GetFireDirection(aimPoint);

        if (Physics.Raycast(
            firePoint.position,
            fireDirection,
            out RaycastHit hit,
            weaponData.range,
            weaponData.hitMask
        ))
        {
            weaponHit.Hit = true;
            weaponHit.Point = hit.point;
            weaponHit.Normal = hit.normal;
            weaponHit.HitObject = hit.collider.gameObject;
            HitZoneComponent hitZoneComponent = hit.collider.GetComponent<HitZoneComponent>();

            if (hitZoneComponent != null)
            {
                weaponHit.HitZone = hitZoneComponent.HitZone;
            }
            else
            {
                weaponHit.HitZone = HitZone.Default;
            }

            ImpactSurface impactSurface = hit.collider.GetComponent<ImpactSurface>();

            if (impactSurface != null)
            {
                weaponHit.ImpactType = impactSurface.ImpactType;
            }
            else
            {
                weaponHit.ImpactType = ImpactType.Default;
            }

            weaponHit.Damage = weaponData.damage;

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            Debug.Log($"Hit object: {hit.collider.gameObject.name}, Damageable: {damageable?.GetType().Name ?? "None"}");

            if (damageable != null)
            {
                damageable.TakeDamage(weaponHit.Damage, weaponHit.HitZone);
            }

            ImpactEffectLibrary.Instance.Spawn(weaponHit);

            if (ShowDebugRay)
            {
                Debug.DrawLine(
                    firePoint.position,
                    hit.point,
                    Color.red,
                    DebugRayDuration
                );

                Debug.DrawRay(
                    hit.point,
                    hit.normal * 1f,
                    Color.cadetBlue,
                    DebugRayDuration * 2
                );
            }

            return weaponHit;
        }

        if (ShowDebugRay)
        {
            Debug.DrawRay(
                firePoint.position,
                fireDirection * weaponData.range,
                Color.red,
                DebugRayDuration
            );
        }

        return weaponHit;
    }
}