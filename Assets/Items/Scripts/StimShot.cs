using UnityEngine;
public class StimShot : ItemClass
{
    [Header("Stim Properties")]
    [SerializeField] private float instantStamina = 20f;   // How much to give immediately
    [SerializeField] private float regenMultiplier = 4f;   // 4x speed regen
    [SerializeField] private float buffDuration = 10f;     // Lasts 10 seconds


    

    private void Awake()
    {
        // Set to true if you want the item to disappear after use
        destroyOnUse = true;
    }

    public override void Use()
    {
        Inject();
    }

    private void Inject()
    {
        if (staminaManager != null)
        {
            // 1. Apply the effect
            staminaManager.ApplyStim(instantStamina, regenMultiplier, buffDuration);

            Debug.Log("Stim Shot Used!");
        }
        else
        {
            Debug.LogError("StimShot: No StaminaManager found in scene!");
        }
    }
}