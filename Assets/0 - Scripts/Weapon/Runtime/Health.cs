using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    [System.Serializable]
    private struct HitZoneMultiplier
    {
        public HitZone hitZone;
        public float multiplier;
    }

    [SerializeField]
    private HitZoneMultiplier[] hitZoneMultipliers;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, HitZone hitZone)
    {
        float damageMultiplier = GetDamageMultiplier(hitZone);
        float finalDamage = damage * damageMultiplier;

        currentHealth -= finalDamage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private float GetDamageMultiplier(HitZone hitZone)
    {
        foreach (HitZoneMultiplier multiplier in hitZoneMultipliers)
        {
            if (multiplier.hitZone == hitZone)
                return multiplier.multiplier;
        }

        return 1f;
    }

    private void OnGUI()
    {
        if (Camera.main == null)
            return;

        Vector3 worldPosition = transform.position + Vector3.up * 2f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        if (screenPosition.z <= 0f)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = DebugSettingsHelper.FontSize
        };

        GUI.Label(
            new Rect(
                screenPosition.x,
                Screen.height - screenPosition.y,
                250,
                75f
            ),
            $"{CurrentHealth:F0}/{MaxHealth:F0}",
            style
        );
    }
}