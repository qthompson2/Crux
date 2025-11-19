using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] public TMP_Text SlotNumber;
    [SerializeField] public Image ImageSlot;

    public void UpdateSlot(ItemClass item)
    {
        // --- Case: Empty slot ---
        if (item == null)
        {
            ImageSlot.enabled = false;
            itemNameText.enabled = true;
            itemNameText.text = "Empty";
            return;
        }

        // --- Case: Item has an icon ---
        if (item.itemIcon != null)
        {
            itemNameText.enabled = false;
            ImageSlot.enabled = true;
            ImageSlot.sprite = item.itemIcon;
            return;
        }

        // --- Case: Item has no icon, show name ---
        
        ImageSlot.enabled = false;
        itemNameText.enabled = true;
        itemNameText.text = item.itemName;
    }

    public void SetSelected(bool isSelected)
    {
        itemNameText.color = isSelected ? Color.yellow : Color.white;
        SlotNumber.color = isSelected ? Color.yellow : Color.white;
    }
}
