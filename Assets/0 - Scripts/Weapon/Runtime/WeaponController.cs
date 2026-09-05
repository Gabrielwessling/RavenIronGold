using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    public WeaponData WeaponData => weaponData;

    private WeaponState currentState = WeaponState.Ready;
    public WeaponState CurrentState => currentState;

    private WeaponAmmo weaponAmmo;
    public WeaponAmmo WeaponAmmo => weaponAmmo;

    private void Awake()
    {
        weaponData = GetComponent<WeaponData>();
        weaponAmmo = GetComponent<WeaponAmmo>();
    }

    public void SetState(WeaponState newState)
    {
        currentState = newState;
    }
}
