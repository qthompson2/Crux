using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Stamina Rates")]
    public float staminaRegenRate = 10f;
    public float climbCost = 20f;
    public float sprintCost = 25f;
    public float jumpCost = 30f;

    [Header("Thresholds")]
    public float labourousActionThreshold = 0.2f;

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        RegenerateStamina();
    }

    public void DrainStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public void DrainOverTime(float ratePerSecond)
    {
        DrainStamina(ratePerSecond * Time.deltaTime);
    }

    public void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }

    public bool HasStamina(float minimum = 1f)
    {
        return currentStamina >= minimum;
    }

    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }

    public bool LabourousActionAllowed()
    {
        return GetStaminaPercentage() > labourousActionThreshold;
    }
}
