using UnityEngine;

public class ImpactEffectLibrary : MonoBehaviour
{
    public static ImpactEffectLibrary Instance { get; private set; }

    [SerializeField]
    private ImpactEffectData[] effects;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject GetEffect(ImpactType impactType)
    {
        foreach (ImpactEffectData effect in effects)
        {
            if (effect.impactType == impactType)
                return effect.prefab;
        }

        return null;
    }

    public void Spawn(WeaponHit weaponHit)
    {
        if (!weaponHit.Hit)
            return;

        GameObject effectPrefab =
            GetEffect(weaponHit.ImpactType);

        if (effectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                effectPrefab,
                weaponHit.Point,
                Quaternion.LookRotation(weaponHit.Normal)
            );
    }
}