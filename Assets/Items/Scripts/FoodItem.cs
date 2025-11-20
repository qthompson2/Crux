using UnityEngine;

public class FoodItem : ItemClass
{
    [SerializeField, Range(0f, 1f)]
    private float foodValue = 0.1f;

    public override void Use()
    {
        if (staminaManager != null)
        {
            staminaManager.Eat(foodValue);
            Debug.Log($"{ItemName} used: Restored {foodValue} stamina.");
        }
        else
        {
            Debug.LogWarning("StaminaManager not assigned; cannot restore stamina.");
        }
    }
}
