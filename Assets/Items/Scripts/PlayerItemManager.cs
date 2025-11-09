using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 4;

    private ItemClass[] slots;
    private PlayerItem inputActions;

    public ItemClass currentItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        slots = new ItemClass[maxSlots];

        inputActions = new PlayerItem();

        // Setup callbacks for inputs
        inputActions.ItemInteraction.SetCallbacks(new ItemInteractionHandler(this));
        inputActions.InventorySlots.SetCallbacks(new InventorySlotsHandler(this));
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Add item to a specific slot
    public void AddItemToSlot(ItemClass item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            UnityEngine.Debug.LogWarning("Invalid slot index");
            return;
        }

        if (slots[slotIndex] != null)
        {
            UnityEngine.Debug.LogWarning($"Slot {slotIndex + 1} is already occupied.");
            return;
        }

        slots[slotIndex] = item;
        UnityEngine.Debug.Log($"Added {item.itemName} to slot {slotIndex + 1}");

        // Auto-select if no item selected
        if (currentItem == null)
            SelectSlot(slotIndex);
    }

    // Remove item from a specific slot
    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;

        if (slots[slotIndex] != null)
        {
            UnityEngine.Debug.Log($"Removed {slots[slotIndex].itemName} from slot {slotIndex + 1}");
            slots[slotIndex] = null;

            // Deselect if currentItem removed
            if (currentItem == slots[slotIndex])
                currentItem = null;
        }
    }

    // Select a slot (sets currentItem)
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            UnityEngine.Debug.LogWarning("Invalid slot index");
            currentItem = null;
            return;
        }

        currentItem = slots[slotIndex];
        if (currentItem != null)
            UnityEngine.Debug.Log($"Selected slot {slotIndex + 1}: {currentItem.itemName}");
        else
            UnityEngine.Debug.Log($"Slot {slotIndex + 1} is empty.");
    }

    public void PrintInventory()
    {
        UnityEngine.Debug.Log("Current inventory:");
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                UnityEngine.Debug.Log($"Slot {i + 1}: {slots[i].itemName}");
            else
                UnityEngine.Debug.Log($"Slot {i + 1}: empty");
        }
    }

    // === Input handlers ===
    // These handle input callbacks and call the InventoryManager methods.

    private class ItemInteractionHandler : PlayerItem.IItemInteractionActions
    {
        private InventoryManager manager;

        public ItemInteractionHandler(InventoryManager invManager)
        {
            manager = invManager;
        }

        public void OnPickUpInteract(InputAction.CallbackContext context)
        {
            if (manager.currentItem == null) return;

            if (context.started)
                manager.currentItem.BeginUse();
            else if (context.canceled)
                manager.currentItem.CancelUse();
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            // Implement drop if you want
        }
    }

    private class InventorySlotsHandler : PlayerItem.IInventorySlotsActions
    {
        private InventoryManager manager;

        public InventorySlotsHandler(InventoryManager invManager)
        {
            manager = invManager;
        }

        public void OnSlot1(InputAction.CallbackContext context)
        {
            if (context.performed)
                manager.SelectSlot(0);
        }

        public void OnSlot2(InputAction.CallbackContext context)
        {
            if (context.performed)
                manager.SelectSlot(1);
        }

        public void OnSlot3(InputAction.CallbackContext context)
        {
            if (context.performed)
                manager.SelectSlot(2);
        }

        public void OnSlot4(InputAction.CallbackContext context)
        {
            if (context.performed)
                manager.SelectSlot(3);
        }
    }
}
