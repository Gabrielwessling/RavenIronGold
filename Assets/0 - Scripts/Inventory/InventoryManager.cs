using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        public int quantity;

        public bool IsEmpty => item == null;
    }

    [SerializeField] private InventorySlot[] slots = new InventorySlot[4];

    public event Action InventoryChanged;

    public InventorySlot[] Slots => slots;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (slots == null)
            slots = new InventorySlot[4];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new InventorySlot();
        }
    }

    private void NotifyInventoryChanged()
    {
        InventoryChanged?.Invoke();
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        InitializeSlots();

        if (!item.isStackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].item = item;
                    slots[i].quantity = 1;
                    NotifyInventoryChanged();
                    return true;
                }
            }

            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                int freeSpace = item.maxStackSize - slots[i].quantity;
                if (freeSpace > 0)
                {
                    int toAdd = Mathf.Min(amount, freeSpace);
                    slots[i].quantity += toAdd;
                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        NotifyInventoryChanged();
                        return true;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                int toAdd = Mathf.Min(amount, item.maxStackSize);
                slots[i].item = item;
                slots[i].quantity = toAdd;
                amount -= toAdd;

                if (amount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        return false;
    }

    public bool RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        InitializeSlots();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != item)
                continue;

            if (slots[i].quantity <= amount)
            {
                amount -= slots[i].quantity;
                slots[i].item = null;
                slots[i].quantity = 0;
            }
            else
            {
                slots[i].quantity -= amount;
                amount = 0;
            }

            if (amount <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }

        return false;
    }
}

