using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponState currentState = WeaponState.Ready;
    public WeaponState CurrentState => currentState;

    [Header("References")]
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    private WeaponAmmo weaponAmmo;
    public WeaponAmmo WeaponAmmo => weaponAmmo;

    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;

    [SerializeField] private Camera playerCamera;
    public Camera PlayerCamera => playerCamera;

    [Header("Timers")]
    private float fireTimer;
    private float reloadTimer;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    [SerializeField] private WeaponDebugScale debugScale = WeaponDebugScale.Normal;
    public float FireTimer => fireTimer;
    public float ReloadTimer => reloadTimer;
    public bool ShowDebugRay = false;
    public float DebugRayDuration = 1f;

    private void Awake()
    {
        weaponAmmo = GetComponent<WeaponAmmo>();
    }

    private void Update()
    {
        if (currentState == WeaponState.Firing)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
                FinishFiring();
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
        return (aimPoint - firePoint.position).normalized;
    }

    public void SetState(WeaponState newState)
    {
        currentState = newState;
    }

    public bool CanFire()
    {
        if (currentState != WeaponState.Ready)
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

        PerformHitscan();

        fireTimer = 1f / weaponData.fireRate;

        SetState(WeaponState.Firing);

        return true;
    }

    public void FinishFiring()
    {
        SetState(WeaponState.Ready);
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
            fontSize = GetDebugFontSize()
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
    }
    private int GetDebugFontSize()
    {
        return debugScale switch
        {
            WeaponDebugScale.Small => 18,
            WeaponDebugScale.Normal => 24,
            WeaponDebugScale.Large => 32,
            WeaponDebugScale.ExtraLarge => 36,
            _ => 24
        };
    }
    private void PerformHitscan()
    {
        if (!TryGetAimPoint(out Vector3 aimPoint))
            return;

        Vector3 fireDirection = GetFireDirection(aimPoint);

        if (Physics.Raycast(
            firePoint.position,
            fireDirection,
            out RaycastHit hit,
            weaponData.range,
            weaponData.hitMask
        ))
        {
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
                    DebugRayDuration*2
                );
            }
        }
        else
        {
            if (ShowDebugRay)
            {
                Debug.DrawRay(
                    firePoint.position,
                    fireDirection * weaponData.range,
                    Color.red,
                    DebugRayDuration
                );
            }
        }
    }
}
