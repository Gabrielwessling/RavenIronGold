using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponState currentState = WeaponState.Ready;

    #region Properties
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    private WeaponAmmo weaponAmmo;
    public WeaponAmmo WeaponAmmo => weaponAmmo;
    #endregion

    #region Timers
    private float fireTimer;
    private float reloadTimer;
    #endregion

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    [SerializeField] private WeaponDebugScale debugScale = WeaponDebugScale.Normal;
    public WeaponState CurrentState => currentState;
    public float FireTimer => fireTimer;
    public float ReloadTimer => reloadTimer;

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
}
