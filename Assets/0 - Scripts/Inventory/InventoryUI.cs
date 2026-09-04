using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Image[] itemIcons = new Image[4];
    [SerializeField] private TextMeshProUGUI[] quantityTexts = new TextMeshProUGUI[4];
    [SerializeField] private Sprite blankSlotSprite;

    private void OnEnable()
    {
        if (inventoryManager == null)
            inventoryManager = InventoryManager.instance;

        if (inventoryManager != null)
            inventoryManager.InventoryChanged += RefreshInventory;

        RefreshInventory();
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
            inventoryManager.InventoryChanged -= RefreshInventory;
    }

    private void RefreshInventory()
    {
        if (inventoryManager == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            var slot = i < inventoryManager.Slots.Length ? inventoryManager.Slots[i] : null;
            bool hasItem = slot != null && slot.item != null;

            if (i < itemIcons.Length && itemIcons[i] != null)
            {
                itemIcons[i].sprite = hasItem ? slot.item.itemIcon : blankSlotSprite;
                itemIcons[i].enabled = true;
            }

            if (i < quantityTexts.Length && quantityTexts[i] != null)
            {
                if (hasItem)
                {
                    int quantity = slot.item.isStackable ? slot.quantity : 1;
                    quantityTexts[i].text = quantity.ToString();
                    quantityTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    quantityTexts[i].text = string.Empty;
                    quantityTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
