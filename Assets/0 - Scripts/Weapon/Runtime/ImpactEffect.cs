using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField]
    private float duration = 0.1f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }
}