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
    public int burstCount = 3;
    public float damage = 10f;
    public float fireRate = 5f; // Rounds per second
    public float range = 100f;

    [Header("Recoil")]
    public float recoilVertical = 1f;
    public float recoilHorizontal = 0.2f;
    public float recoilBack = 0.05f;
    public float maxRecoil = 100f;
    public float recoilRecovery = 20f;
    public AnimationCurve recoilDamping;

    [Header("Magazine")]
    public int magazineSize = 30;

    [Header("Reload")]
    public float reloadTime = 2f;
    
    [Header("Hit Detection")]
    public LayerMask hitMask = ~0;
}