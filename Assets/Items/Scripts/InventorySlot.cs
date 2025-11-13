using TMPro; // or UnityEngine.UI if you prefer Text
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;

    /// <summary>
    /// Update the UI to show the item name or "Empty" if no item.
    /// </summary>
    public void UpdateSlot(ItemClass item)
    {
        if (item != null)
            itemNameText.text = item.itemName;
        else
            itemNameText.text = "Empty";
    }

    /// <summary>
    /// Highlight this slot if it is selected.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        itemNameText.color = isSelected ? Color.yellow : Color.white;
        // You can also update background color, add outlines, etc.
    }
}
