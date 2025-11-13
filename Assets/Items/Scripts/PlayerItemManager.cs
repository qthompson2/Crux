using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 4;
    private ItemClass[] slots; // Changed to ItemClass[] for consistency with your base class

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private StaminaManager staminaManager;
    [SerializeField] private UseIndicatorUI useIndicator;
    [SerializeField] private InventoryUIManager inventoryUIManager;

    private PlayerItem inputActions;

    public ItemClass currentItem { get; private set; }
    private int currentSlotIndex = -1;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        slots = new ItemClass[maxSlots];
        inputActions = new PlayerItem();

        inputActions.ItemInteraction.SetCallbacks(new ItemInteractionHandler(this));
        inputActions.InventorySlots.SetCallbacks(new InventorySlotsHandler(this));

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    // === INVENTORY MANAGEMENT ===

    public bool AddItem(ItemClass item)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to add null item.");
            return false;
        }

        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                Debug.Log($"Picked up {item.itemName} and added to slot {i + 1}.");
                SelectSlot(i);
                UpdateUI();
                return true;
            }
        }
        Debug.LogWarning("Inventory full! Could not add item.");
        return false;
    }


    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
            return;

        if (slots[slotIndex] == null)
            return;

        Debug.Log($"Removed {slots[slotIndex].itemName} from slot {slotIndex + 1}");

        if (currentSlotIndex == slotIndex)
        {
            currentItem = null;
            currentSlotIndex = -1;
        }

        slots[slotIndex] = null;
        UpdateUI();
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            Debug.LogWarning("Invalid slot index selected.");
            currentItem = null;
            currentSlotIndex = -1;
            UpdateUI();
            return;
        }

        currentItem = slots[slotIndex];
        currentSlotIndex = slotIndex;

        if (currentItem != null)
            Debug.Log($"Selected slot {slotIndex + 1}: {currentItem.itemName}");
        else
            Debug.Log($"Slot {slotIndex + 1} is empty.");

        UpdateUI();
    }

    public void PrintInventory()
    {
        Debug.Log("Current inventory:");
        for (int i = 0; i < slots.Length; i++)
        {
            Debug.Log(slots[i] != null
                ? $"Slot {i + 1}: {slots[i].itemName}"
                : $"Slot {i + 1}: empty");
        }
    }

    private void UpdateUI()
    {
        if (inventoryUIManager != null)
            inventoryUIManager.UpdateInventoryUI(slots, currentSlotIndex);
        else
            Debug.LogWarning("InventoryUIManager reference not set!");
    }

    // === PICKUP LOGIC ===

    public void TryPickupViaRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("No player camera assigned!");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.yellow, 0.25f);

        if (Physics.Raycast(ray, out hit, pickupRange, itemLayerMask))
        {
            ItemClass item = hit.collider.GetComponent<ItemClass>();

            Debug.DrawLine(ray.origin, hit.point, Color.green, 0.25f);

            if (item != null)
            {
                Debug.Log($"[Raycast] Found item '{item.itemName}' to pick up at {hit.point}.");
                PickUpItem(item);
            }
            else
            {
                Debug.Log($"[Raycast] Hit non-item object: {hit.collider.name}");
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 0.25f);
            Debug.Log("No item found within pickup range.");
        }
    }

    public void PickUpItem(ItemClass item)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to pick up a null item.");
            return;
        }

        bool added = AddItem(item);
        if (added)
        {
            item.OnPickedUp(staminaManager, useIndicator, this);
        }
        else
        {
            Debug.LogWarning($"Failed to add item {item.itemName} to inventory.");
        }
    }

    // === ITEM USAGE ===

    public void UseCurrentItem()
    {
        if (currentItem == null)
        {
            Debug.Log("No item selected to use.");
            return;
        }

        currentItem.BeginUse();
    }
    public void ClearItem()
    {
        if (currentItem != null || slots[currentSlotIndex] != null)
        {
            currentItem = null;
            slots[currentSlotIndex] = null;
            Debug.Log("Clear Item");
            UpdateUI();
        }
    }
    public void CancelUse()
    {
        if (currentItem == null)
            return;

        currentItem.CancelUse();
    }

    // === INPUT HANDLERS ===

    private class ItemInteractionHandler : PlayerItem.IItemInteractionActions
    {
        private readonly InventoryManager manager;

        public ItemInteractionHandler(InventoryManager manager)
        {
            this.manager = manager;
        }

        public void OnPickUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (manager.currentItem == null)
                {
                    Debug.Log("No item selected, performing pickup.");
                    manager.TryPickupViaRaycast();
                }
                else
                {
                    Debug.Log("Item selected, pickup disabled.");
                }
            }
        }


        public void OnDrop(InputAction.CallbackContext context)
        {
            if (context.performed && manager.currentItem != null)
            {
                Debug.Log($"Dropped {manager.currentItem.itemName}");

                // Desired drop distance, but capped max distance
                float desiredDropDistance = 1.5f;
                float maxDropDistance = 3f; // max distance allowed to drop

                // Calculate initial target drop position based on camera forward
                Vector3 dropPos = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * desiredDropDistance;

                // Check distance from player to dropPos
                float distance = Vector3.Distance(manager.playerCamera.transform.position, dropPos);
                if (distance > maxDropDistance)
                {
                    dropPos = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * maxDropDistance;
                }

                // Raycast downward to find ground below drop position
                RaycastHit hit;
                float raycastHeight = 5f;
                Vector3 rayOrigin = dropPos + Vector3.up * raycastHeight;

                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastHeight + 2f))
                {
                    dropPos = hit.point + Vector3.up * 0.1f;
                }
                else
                {
                    Debug.LogWarning("No ground detected below drop position, dropping at default position.");
                }

                manager.currentItem.OnDropped(dropPos);

                int index = System.Array.IndexOf(manager.slots, manager.currentItem);
                manager.RemoveItemFromSlot(index);
            }
        }


        public void OnUse(InputAction.CallbackContext context)
        {
            Debug.Log($"OnUse called: phase={context.phase}");
            if (context.started)
                manager.UseCurrentItem();
            else if (context.canceled)
                manager.CancelUse();
        }
    }

    private class InventorySlotsHandler : PlayerItem.IInventorySlotsActions
    {
        private readonly InventoryManager manager;

        public InventorySlotsHandler(InventoryManager manager)
        {
            this.manager = manager;
        }

        public void OnSlot1(InputAction.CallbackContext context) { if (context.performed) manager.SelectSlot(0); }
        public void OnSlot2(InputAction.CallbackContext context) { if (context.performed) manager.SelectSlot(1); }
        public void OnSlot3(InputAction.CallbackContext context) { if (context.performed) manager.SelectSlot(2); }
        public void OnSlot4(InputAction.CallbackContext context) { if (context.performed) manager.SelectSlot(3); }
    }
}
