using UnityEngine;

public class BeamBlink : MonoBehaviour
{
    [Tooltip("Time interval between toggles (seconds)")]
    public float interval = 0.5f;

    private Renderer[] childRenderers;

    void Start()
    {
        // Get all renderers in children
        childRenderers = GetComponentsInChildren<Renderer>();

        // Start toggling repeatedly
        InvokeRepeating(nameof(ToggleBeam), 0f, interval);
    }

    void ToggleBeam()
    {
        foreach (var rend in childRenderers)
        {
            // Toggle each child renderer
            rend.enabled = !rend.enabled;
        }
    }
}
