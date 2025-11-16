using UnityEngine;

public class HealItem : ItemClass
{
    [SerializeField, Range(0f, 1f)]
    private float healAmount = 0.2f; // 20% heal

    public override void Use()
    {
        if (staminaManager != null)
        {
            staminaManager.HealDamage(healAmount);
            Debug.Log($"{ItemName} used: Healed {healAmount} points.");
        }
        else
        {
            Debug.LogWarning("StaminaManager not assigned; cannot heal.");
        }
    }
}
