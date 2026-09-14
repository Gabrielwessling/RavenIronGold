using UnityEngine;

public class ImpactSurface : MonoBehaviour
{
    [SerializeField]
    private ImpactType impactType = ImpactType.Default;

    public ImpactType ImpactType => impactType;
}