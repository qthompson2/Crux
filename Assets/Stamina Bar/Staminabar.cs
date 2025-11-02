using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image staminaFill;
    [SerializeField] private UnityEngine.UI.Image capOverlay;

    public void UpdateBar(float currentStamina, float maxStamina, float maxCap)
    {
        UnityEngine.Debug.Log($"Updating bar: {currentStamina}/{maxStamina} (cap={maxCap})");
        if (staminaFill != null)
        {
            float normalized = Mathf.Clamp01(currentStamina / maxStamina);
            staminaFill.fillAmount = normalized;
        }

        if (capOverlay != null)
        {
            float capNormalized = Mathf.Clamp01(maxCap / maxStamina);
            capOverlay.fillAmount = capNormalized;
        }
    }
}
