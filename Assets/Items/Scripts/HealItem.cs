using System.Diagnostics;
using UnityEngine;

public class HealItem : ItemClass
{
    [Range(0f, 1f)]
    [SerializeField]
    private float healAmount = 0.2f; // 20% heal
    [SerializeField] private StaminaManager staminaManager;

    public override void Use()
    {
        if (staminaManager != null)
        {
            staminaManager.HealDamage(healAmount);
            UnityEngine.Debug.Log($"{itemName} used: Healed {healAmount} points.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("StaminaManager not found, cannot heal.");
        }
    }
}
