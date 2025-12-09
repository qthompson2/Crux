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
    [SerializeField] private PlayerStateManager playerstateManager;

    [Header("UI References")]
    [SerializeField] private UseIndicatorUI useIndicator;
    [SerializeField] private TextMeshProUGUI lookAtPrompt;

    private PlayerItem inputActions;

    public ItemClass currentItem { get; private set; }
    private int currentSlotIndex = -1;
    private PlayerBaseState currentstate;
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
        //Update Current State
        currentstate = playerstateManager.currentState;
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
            return false;
        }

        for (int i = 0; i < maxSlots; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                SelectSlot(i);
                UpdateUI();
                return true;
            }
        }
        return false;
    }


    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
            return;

        if (slots[slotIndex] == null)
            return;

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
            return; // Abort the slot switch
        }
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            currentItem = null;
            currentSlotIndex = -1;
            UpdateUI();
            return;
        }

        currentItem = slots[slotIndex];
        currentSlotIndex = slotIndex;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (inventoryUIManager != null)
            inventoryUIManager.UpdateInventoryUI(slots, currentSlotIndex);
    }

    // === PICKUP LOGIC ===

    public void TryPickupViaRaycast()
    {
        if (playerCamera == null)
        {
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
                    return; // Stop the pickup
                }
                PickUpItem(item);
            }
        }
    }

    public void PickUpItem(ItemClass item)
    {
        if (item == null)
        {
            return;
        }

        bool added = AddItem(item);
        if (added)
        {
            item.OnPickedUp(staminaManager, useIndicator, this, playerCamera);
            weightPenalty += item.weight;
        }
    }

    // === ITEM USAGE ===

    public void UseCurrentItem()
    {
        if (currentstate is not ClimbingState)
        { 
            if (currentItem == null)
            {
                return;
            }

            currentItem.BeginUse();
        }
    }
    public void ClearItem()
    {
        if (currentItem != null || slots[currentSlotIndex] != null)
        {
            currentItem = null;
            slots[currentSlotIndex] = null;
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
                manager.TryPickupViaRaycast();
            }
        }


        public void OnDrop(InputAction.CallbackContext context)
        {
            if (context.performed && manager.currentItem != null && !manager.currentItem.IsBeingUsed)
            {
                // Player body's forward
                Transform playerTransform = manager.playerCamera.transform.root;
                Vector3 flatForward = manager.playerCamera.transform.forward;
                flatForward.y = 0;
                if (flatForward.sqrMagnitude < 0.01f)
                {
                    flatForward = playerTransform.forward; 
                }
                flatForward.Normalize();

                // Desired drop distance, but capped max distance
                float desiredDropDistance = 1.5f;
                float maxDropDistance = 3f; // max distance allowed to drop

                // (Old drop) Calculate initial target drop position based on camera forward
                // Vector3 dropPos = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * desiredDropDistance;

                // (New drop) Start drop position using *flat* forward
                Vector3 dropPos = manager.playerCamera.transform.position + flatForward * desiredDropDistance;

                // Check distance from player to dropPos
                float distance = Vector3.Distance(manager.playerCamera.transform.position, dropPos);
                if (distance > maxDropDistance)
                {
                    //dropPos = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * maxDropDistance;
                    dropPos = manager.playerCamera.transform.position + flatForward * maxDropDistance;
                }

                // Raycast downward to find ground below drop position
                RaycastHit hit;
                float raycastHeight = 5f;
                Vector3 rayOrigin = dropPos + Vector3.up * raycastHeight;

                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastHeight + 2f))
                {
                    dropPos = hit.point + Vector3.up * 0.1f;
                }

                manager.currentItem.OnDropped(dropPos);
                int index = System.Array.IndexOf(manager.slots, manager.currentItem);
                manager.RemoveItemFromSlot(index);
                
            }
        }


        public void OnUse(InputAction.CallbackContext context)
        {
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
