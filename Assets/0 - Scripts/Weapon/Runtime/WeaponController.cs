using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponState currentState = WeaponState.Ready;
    public WeaponState CurrentState => currentState;

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
        if (currentState == WeaponState.Reloading)
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
}
