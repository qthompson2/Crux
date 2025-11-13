using System.Collections;
using UnityEngine;

public abstract class ItemClass : MonoBehaviour
{
    [Header("General Item Properties")]
    [SerializeField] public string itemName = "New Item";
    [SerializeField, Range(0f, 1f)] public float weight = 0.1f;
    [SerializeField] public float useTime = 1f;

    protected StaminaManager staminaManager;
    protected UseIndicatorUI useIndicator;
    protected InventoryManager ItemManager;

    private bool isBeingUsed = false;
    private Coroutine useRoutine;

    public string ItemName => itemName;
    public float Weight => weight;
    public float UseTime => useTime;

    /// <summary>
    /// Call this to start using the item (e.g., eating, healing).
    /// </summary>
    public void BeginUse()
    {
        if (isBeingUsed) return;
        useRoutine = StartCoroutine(UseRoutine());
    }

    /// <summary>
    /// Call this to cancel using the item (before useTime completes).
    /// </summary>
    public void CancelUse()
    {
        if (!isBeingUsed) return;

        StopCoroutine(useRoutine);
        isBeingUsed = false;

        useIndicator?.ResetProgress();

        Debug.Log($"{itemName} use cancelled!");
    }

    private IEnumerator UseRoutine()
    {
        isBeingUsed = true;
        Debug.Log($"Started using {itemName}...");

        float elapsed = 0f;
        while (elapsed < useTime)
        {
            elapsed += Time.deltaTime;
            useIndicator?.UpdateProgress(elapsed / useTime);
            yield return null;
        }

        isBeingUsed = false;

        useIndicator?.ResetProgress();

        Use();
        Destroy(gameObject);
        //Clear Inventory Slot Name
        ItemManager.ClearItem();
        Debug.Log($"{itemName} use complete!");
    }

    /// <summary>
    /// Override this to define what happens when the item is used.
    /// </summary>
    public abstract void Use();

    /// <summary>
    /// Called when the item is picked up; assigns references and disables world object.
    /// </summary>
    public virtual void OnPickedUp(StaminaManager staminaManagerRef, UseIndicatorUI useIndicatorRef, InventoryManager ItemManagerRef)
    {
        Debug.Log($"{itemName} was picked up.");

        // Instead of disabling whole GameObject, just hide renderer and collider
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = false;

        // Detach from world and keep active for logic
        transform.SetParent(null);

        staminaManager = staminaManagerRef;
        useIndicator = useIndicatorRef;
        ItemManager = ItemManagerRef;
    }


    /// <summary>
    /// Called when the item is dropped; spawns prefab at the drop position.
    /// </summary>
    public virtual void OnDropped(Vector3 dropPosition)
    {
        Debug.Log($"{itemName} dropped.");

        transform.position = dropPosition;

        // Show the item again
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = true;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = true;

        gameObject.SetActive(true); // you can optionally call this too if you want to ensure active
    }

}
