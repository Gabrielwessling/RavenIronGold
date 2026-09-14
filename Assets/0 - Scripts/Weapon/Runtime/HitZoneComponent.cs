using UnityEngine;

public class HitZoneComponent : MonoBehaviour
{
    [SerializeField]
    private HitZone hitZone = HitZone.Default;

    [SerializeField]
    private float damageMultiplier = 1f;

    public HitZone HitZone => hitZone;
    public float DamageMultiplier => damageMultiplier;
}