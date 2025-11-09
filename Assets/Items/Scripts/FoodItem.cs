using System.Diagnostics;
using UnityEngine;

public class FoodItem : ItemClass
{
    [Range(0f, 1f)]
    [SerializeField]
    public float foodValue = .1f;
    [SerializeField] private StaminaManager staminaManager;

    public override void Use()
    {
        if (staminaManager != null)
        {
            staminaManager.Eat(foodValue);
            UnityEngine.Debug.Log($"{itemName} used: Restored {foodValue} stamina.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("StaminaManager not found, cannot restore stamina.");
        }
    }
}
