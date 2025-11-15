using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 4;
    private ItemClass[] slots;
    public float weightPenalty = 0f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private StaminaManager staminaManager;
    [SerializeField] private InventoryUIManager inventoryUIManager;

    [Header("UI References")]
    [SerializeField] private UseIndicatorUI useIndicator;
    [SerializeField] private TextMeshProUGUI lookAtPrompt;

    private PlayerItem inputActions;

    public ItemClass currentItem { get; private set; }
    private int currentSlotIndex = -1;
    public int MaxSlots => maxSlots;
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
        if (lookAtPrompt != null)
            lookAtPrompt.gameObject.SetActive(false);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    private void Update()
    {
        // Don't do anything if the prompt UI isn't assigned
        if (lookAtPrompt == null || playerCamera == null)
            return;

        // Create the ray
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Shoot the ray
        if (Physics.Raycast(ray, out hit, pickupRange, itemLayerMask))
        {
            // We hit something on the item layer!
            ItemClass item = hit.collider.GetComponent<ItemClass>();

            if (item != null)
            {
                // === START OF MODIFICATION ===
                // Check if picking up this item would make the stamina cap < 0
                if (staminaManager.maxCap - item.weight*100 >0f)
                {
                    // Item is pick-up-able
                    lookAtPrompt.text = $"(E) - {item.ItemName}";
                    lookAtPrompt.gameObject.SetActive(true);
                }
                else
                {
                    // Item is too heavy
                    lookAtPrompt.text = "Too heavy";
                    lookAtPrompt.gameObject.SetActive(true);
                }
                // === END OF MODIFICATION ===
            }
            else
            {
                lookAtPrompt.gameObject.SetActive(false);
            }
        }
        else
        {
            // We are not looking at an item, so hide the prompt.
            lookAtPrompt.gameObject.SetActive(false);
        }
    }
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
            weightPenalty -= currentItem.weight;
            currentItem = null;
            currentSlotIndex = -1;
        }

        slots[slotIndex] = null;
        UpdateUI();
    }

    public void SelectSlot(int slotIndex)
    {
        if (currentItem != null && currentItem.IsBeingUsed)
        {
            Debug.Log($"Cannot switch items while using {currentItem.itemName}!");
            return; // Abort the slot switch
        }
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
                if (!(staminaManager.maxCap - item.weight * 100 > 0f))
                {
                    Debug.LogWarning($"Cannot pick up {item.itemName}. Item is too heavy!");
                    return; // Stop the pickup
                }
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
            weightPenalty += item.weight;
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
                Debug.Log("Pickup key pressed, attempting raycast...");
                manager.TryPickupViaRaycast();
            }
        }


        public void OnDrop(InputAction.CallbackContext context)
        {
            if (context.performed && manager.currentItem != null && !manager.currentItem.IsBeingUsed)
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
            else if (context.performed && manager.currentItem != null && manager.currentItem.IsBeingUsed)
            {
                Debug.Log($"Cannot drop {manager.currentItem.itemName} while it is being used!");
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
