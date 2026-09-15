using UnityEngine;

public struct WeaponHit
{
    public bool Hit;
    public Vector3 Point;
    public Vector3 Normal;
    public GameObject HitObject;
    public HitZone HitZone;
    public float Damage;
    public ImpactType ImpactType;
}