// ClimbableSurface.cs
using System.Diagnostics;
using UnityEngine;

public class ClimbableSurface : MonoBehaviour
{
    [Header("Climbing Properties")]
    public float gripStrength = 1f; // Mock no idea what this will do
    public bool allowsHorizontalMovement = true;
    public bool allowsUpwardMovement = true;
    public float climbSpeedMultiplier = 1f;

    [Header("Visual Feedback")]
    public Material climbableMaterial; // Optional: different material for climbable surfaces
    private Material originalMaterial;

    void Start()
    {
        // Optional: Change material to indicate climbable surface
        if (climbableMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material;
                renderer.material = climbableMaterial;
            }
        }

        // Ensure the object is on the correct layer
        if (gameObject.layer != LayerMask.NameToLayer("Climbable"))
        {
            UnityEngine.Debug.LogWarning($"{gameObject.name} has ClimbableSurface component but is not on Climbable layer!");
        }
    }

    // Optional: Restore original material when component is removed
    void OnDestroy()
    {
        if (originalMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }
        }
    }
}