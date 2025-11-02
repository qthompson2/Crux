using UnityEngine;

public class StaminaSystemTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StaminaBar staminaBar;  // 👈 plug in your StaminaBar here

    [Header("Values")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float hungerPenalty = 0f; // 0–1 fraction
    [SerializeField] private float damagePenalty = 0f; // 0–1 fraction
    [SerializeField] private float regenSpeed = 20f;

    private void Update()
    {
        float maxCap = maxStamina * (1f - hungerPenalty) * (1f - damagePenalty);

        // Regen stamina toward cap
        currentStamina = Mathf.MoveTowards(currentStamina, maxCap, regenSpeed * Time.deltaTime);

        // Call the UI update through the bar
        if (staminaBar != null)
        {
            staminaBar.UpdateBar(currentStamina, maxStamina, maxCap);
        }
    }

    // ----- TEST BUTTON HOOKS -----
    public void UseStamina(float amount = 10f)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0);
    }

    public void RegenStamina(float amount = 10f)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
    }

    public void AddDamage(float fraction = 0.1f)
    {
        damagePenalty = Mathf.Clamp01(damagePenalty + fraction);
    }

    public void HealDamage(float fraction = 0.1f)
    {
        damagePenalty = Mathf.Clamp01(damagePenalty - fraction);
    }

    public void AddHunger(float fraction = 0.1f)
    {
        hungerPenalty = Mathf.Clamp01(hungerPenalty + fraction);
    }

    public void Eat(float fraction = 0.1f)
    {
        hungerPenalty = Mathf.Clamp01(hungerPenalty - fraction);
    }
}
