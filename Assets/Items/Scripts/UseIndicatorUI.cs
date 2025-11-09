using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class UseIndicatorUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image fillImage;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<UnityEngine.UI.Image>();

        fillImage.fillAmount = 0f;
    }

    // Call this every frame while holding the key
    public void UpdateProgress(float progress)
    {
        fillImage.fillAmount = Mathf.Clamp01(progress);
    }

    // Reset or hide indicator
    public void ResetProgress()
    {
        fillImage.fillAmount = 0f;
    }
}
