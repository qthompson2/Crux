using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Header("Bar Fills")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image hungerFill;
    [SerializeField] private Image weightFill;
    [SerializeField] private Image damageFill;

    [Header("Settings")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectTransform damageIcon;
    [SerializeField] private RectTransform hungerIcon;
    [SerializeField] private RectTransform weightIcon;

    [Tooltip("Hide icon if the segment is smaller than this many pixels")]
    [SerializeField] private float minWidthToShow = 20f;

    public void UpdateBar(float currentStamina, float maxStamina, float hungerLoss, float damageLoss, float weightLoss)
    {
        // 1. Clamp Inputs
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        hungerLoss = Mathf.Clamp(hungerLoss, 0f, maxStamina);
        damageLoss = Mathf.Clamp(damageLoss, 0f, maxStamina);
        weightLoss = Mathf.Clamp(weightLoss, 0f, maxStamina);

        // 2. Normalize Values (0.0 to 1.0)
        // These are the "sizes" of each chunk
        float stamPct = Mathf.Clamp01(currentStamina / maxStamina);
        float damagePct = Mathf.Clamp01(damageLoss / maxStamina);
        float hungerPct = Mathf.Clamp01(hungerLoss / maxStamina);
        float weightPct = Mathf.Clamp01(weightLoss / maxStamina);

        // 3. Update Fills (Assuming Right-to-Left stacking implies overlaps)
        // Total cumulative fills for the images
        if (staminaFill != null) staminaFill.fillAmount = stamPct;
        if (damageFill != null) damageFill.fillAmount = damagePct;
        if (hungerFill != null) hungerFill.fillAmount = damagePct + hungerPct;
        if (weightFill != null) weightFill.fillAmount = damagePct + hungerPct + weightPct;

        // 4. Update Icons (Stacking from Right (1.0) to Left)

        // Damage: Starts at 1.0, Ends at (1.0 - Damage)
        float currentRightEdge = 1.0f;
        UpdateIconPosition(damageIcon, currentRightEdge, currentRightEdge - damagePct);

        // Hunger: Starts where Damage ended
        currentRightEdge -= damagePct;
        UpdateIconPosition(hungerIcon, currentRightEdge, currentRightEdge - hungerPct);

        // Weight: Starts where Hunger ended
        currentRightEdge -= hungerPct;
        UpdateIconPosition(weightIcon, currentRightEdge, currentRightEdge - weightPct);
    }

    private void UpdateIconPosition(RectTransform icon, float rightNorm, float leftNorm)
    {
        if (icon == null || barRect == null) return;

        float totalWidth = barRect.rect.width;

        // Calculate the width of this specific segment
        // (rightNorm is the higher number, e.g., 1.0)
        float segmentSize = (rightNorm - leftNorm) * totalWidth;

        // Hide if too small
        if (segmentSize < minWidthToShow)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(true);

        // Calculate Center Point (0.0 to 1.0)
        // Since we are moving Left, the center is the Left Edge + half the size
        float centerNorm = leftNorm + ((rightNorm - leftNorm) / 2f);

        // Calculate Pixel Position (Assuming Anchor is Middle-Left)
        // 0.0 = Left Edge, 1.0 = Right Edge
        float pixelX = centerNorm * totalWidth;

        icon.anchoredPosition = new Vector2(pixelX, 0f);
    }
}