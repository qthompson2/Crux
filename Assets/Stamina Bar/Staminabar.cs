using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Image staminaFill; // Bottom layer (usable stamina)
    [SerializeField] private Image hungerFill;  // Middle layer (hunger reduction)
    [SerializeField] private Image weightFill;
    [SerializeField] private Image damageFill;  // Top layer (health reduction)

    public void UpdateBar(float currentStamina, float maxStamina, float hungerLoss, float damageLoss, float weightLoss)
    {
        // Clamp all values to stay within 0–maxStamina
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        hungerLoss = Mathf.Clamp(hungerLoss, 0f, maxStamina);
        damageLoss = Mathf.Clamp(damageLoss, 0f, maxStamina);
        weightLoss = Mathf.Clamp(weightLoss, 0f, maxStamina);

        // Convert to normalized 0–1 values
        float staminaNormalized = Mathf.Clamp01(currentStamina / maxStamina);
        float damageNormalized = Mathf.Clamp01(damageLoss / maxStamina);
        float hungerNormalized = Mathf.Clamp01((damageLoss + hungerLoss) / maxStamina);
        float weightNormalized = Mathf.Clamp01((damageLoss + hungerLoss + weightLoss) / maxStamina);

        // Apply fills in order
        if (staminaFill != null)
            staminaFill.fillAmount = staminaNormalized;    // actual usable stamina

        if (damageFill != null)
            damageFill.fillAmount = damageNormalized;      // red = damage only

        if (hungerFill != null)
            hungerFill.fillAmount = hungerNormalized;      // yellow = hunger + damage (offset)

        if (weightFill!= null)
            weightFill.fillAmount = weightNormalized;      // orange = hunger + damage + weight (offset)
    }
}
