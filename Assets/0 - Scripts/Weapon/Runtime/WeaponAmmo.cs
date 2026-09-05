using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [SerializeField]
    private WeaponController weaponController;

    private int currentAmmo;

    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => weaponController.WeaponData.magazineSize;

    private void Awake()
    {
        weaponController = GetComponent<WeaponController>();
        currentAmmo = MagazineSize;
    }

    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    public bool TryConsumeAmmo()
    {
        if (!HasAmmo())
            return false;

        currentAmmo--;
        return true;
    }
}