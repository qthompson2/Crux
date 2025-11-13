using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private InventorySlot inventorySlotPrefab; // Your prefab with InventorySlot component
    [SerializeField] private Transform slotsParent;            // The parent UI panel/container for slots
    [SerializeField] private int maxSlots = 4;

    private List<InventorySlot> uiSlots = new List<InventorySlot>();

    private void Awake()
    {
        // Clear any existing children (optional)
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        // Instantiate slots dynamically
        for (int i = 0; i < maxSlots; i++)
        {
            InventorySlot slot = Instantiate(inventorySlotPrefab, slotsParent);
            slot.name = $"Slot_{i + 1}";
            uiSlots.Add(slot);
        }
    }

    /// <summary>
    /// Call this to update the UI slots with current inventory data.
    /// </summary>
    /// <param name="items">Array or list of ItemClass in inventory</param>
    /// <param name="selectedIndex">Current selected slot index</param>
    public void UpdateInventoryUI(ItemClass[] items, int selectedIndex)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            InventorySlot slot = uiSlots[i];
            ItemClass item = i < items.Length ? items[i] : null;
            slot.UpdateSlot(item);
            slot.SetSelected(i == selectedIndex);
        }
    }
}
