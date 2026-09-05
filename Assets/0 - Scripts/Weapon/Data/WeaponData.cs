using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponData",
    menuName = "Weapons/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public string weaponDescription;

    [Header("Fire")]
    public FireMode fireMode;
    public float damage = 10f;
    public float fireRate = 5f; // Rounds per second
    public float range = 100f;

    [Header("Magazine")]
    public int magazineSize = 30;

    [Header("Reload")]
    public float reloadTime = 2f;
}