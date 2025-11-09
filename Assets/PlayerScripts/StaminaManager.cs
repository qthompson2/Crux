using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StaminaBar staminaBar;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f; // This is Initialized Starting Capactity
    [SerializeField] public float currentStamina; //Current value of Stamina Available
    [SerializeField] public float maxCap; //This Maximum Stamina a user can regenerate up to

    [Header("Stamina Rates")]
    [SerializeField] public float staminaRegenRate = 10f;
    [SerializeField] public float climbCost = 20f;
    [SerializeField] public float sprintCost = 25f;
    [SerializeField] public float jumpCost = 30f;

    [Header("Penalties (0–1)")]
    [SerializeField, Range(0f, 1f)] private float hungerPenalty = 0f;
    [SerializeField, Range(0f, 1f)] private float damagePenalty = 0f;

    [Header("Hunger Settings")]
    [SerializeField] private float hungerIncreaseInterval = 60f;  // seconds between increases
    [SerializeField, Range(0f, 1f)] private float hungerIncreaseAmount = 0.05f;  // amount to increase each interval
    private float hungerTimer = 0f;              // internal timer accumulator

    [Header("Falling Variables")]
    public float start_height;
    public float end_height;
    public float start_time;
    public float end_time;
    [SerializeField] float safeThreashold = 2f;

    [Header("Thresholds")]
    [SerializeField] private float labourousActionThreshold = 0.2f;

    private void Start()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        UpdateHungerOverTime();
        UpdateStamina();
    }
    private void UpdateHungerOverTime()
    {
        hungerTimer += Time.deltaTime;

        if (hungerTimer >= hungerIncreaseInterval)
        {
            hungerTimer = 0f;
            AddHunger(hungerIncreaseAmount);
        }
    }
    private void UpdateStamina()
    {
        // Calculate penalties
        float hungerLoss = maxStamina * hungerPenalty;
        float damageLoss = maxStamina * damagePenalty;
        maxCap = Mathf.Max(0f, maxStamina - hungerLoss - damageLoss);

        // Instantly clamp stamina if the new cap is lower
        if (currentStamina > maxCap)
            currentStamina = maxCap;
        else
            // Smoothly regenerate stamina if below the cap
            currentStamina = Mathf.MoveTowards(currentStamina, maxCap, staminaRegenRate * Time.deltaTime);

        // Update UI
        if (staminaBar != null)
            staminaBar.UpdateBar(currentStamina, maxStamina, hungerLoss, damageLoss);
    }

    // -------------------------------
    // Stamina Actions
    // -------------------------------
    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0f);
    }

    public void DrainOverTime(float ratePerSecond)
    {
        DrainStamina(ratePerSecond * Time.deltaTime);
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

    // -------------------------------
    // FallDamage Calculations
    // -------------------------------
    public void checkFallDamage()
    {
        // Calculate how far the player fell
        float fallDistance = start_height - end_height;

        if (fallDistance <= safeThreashold) //If player fell the safe distance don't apply dmg
        {
            // Ignore small hops
            return;
        }

        // Base flat damage for medium falls
        float baseDamage = 0.01f; // 1% cap loss (No matter how small the fall they will take 1 damage)

        float damageMultiplier = 0.02f;
        float exponent = 1.5f;

        float exponentialFactor = damageMultiplier * Mathf.Pow(fallDistance - safeThreashold, exponent); // Exponential scaling for large falls
        //Player will not take extra damage on the falling with the safe amount

        // Combine damage
        float totalDamage = baseDamage + exponentialFactor;

        // Clamp the final value to percentage
        totalDamage = Mathf.Clamp01(totalDamage);

        //Clamp to Max Damage per Fall (60%)
        //totalDamage = Mathf.Clamp(totalDamage, 0f, 0.6f);


        takeFallDamage(totalDamage);

        Debug.Log($"💥 Fall Damage: {totalDamage * 100f:F1}% (fell {fallDistance:F2}m)");
    }

    private void takeFallDamage(float damage)
    {
        AddDamage(damage);
    }

    // -------------------------------
    // Penalty Modifiers
    // -------------------------------
    public void AddDamage(float fraction = 0.1f)
        => damagePenalty = Mathf.Clamp01(damagePenalty + fraction);

    public void HealDamage(float fraction = 0.1f)
        => damagePenalty = Mathf.Clamp01(damagePenalty - fraction);

    public void AddHunger(float fraction = 0.1f)
        => hungerPenalty = Mathf.Clamp01(hungerPenalty + fraction);

    public void Eat(float fraction = 0.1f)
        => hungerPenalty = Mathf.Clamp01(hungerPenalty - fraction);
}
