using System.Text;
using UnityEngine;

namespace IdleonGame.Items
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int slotCount = 24;
        [SerializeField] private bool grantInitialItemsOnAwake = true;
        [SerializeField] private InitialInventoryItem[] initialItems =
        {
            new InitialInventoryItem("coarse_cloth_shirt", 1),
            new InitialInventoryItem("coarse_cloth_pants", 1)
        };
        [SerializeField] private InventorySlot[] slots;

        public InventorySlot[] Slots => slots;

        private void Awake()
        {
            EnsureSlots();
            GrantInitialItems();
        }

        public bool AddItem(string itemId, int count)
        {
            EnsureSlots();
            if (string.IsNullOrEmpty(itemId) || count <= 0 || ItemDatabase.Find(itemId) == null)
            {
                return false;
            }

            var remaining = count;
            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].CanStack(itemId))
                {
                    remaining = slots[i].Add(itemId, remaining);
                }
            }

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    remaining = slots[i].Add(itemId, remaining);
                }
            }

            return remaining == 0;
        }

        public bool DropItem(int slotIndex, int count)
        {
            EnsureSlots();
            if (!IsValidSlot(slotIndex) || count <= 0 || slots[slotIndex].IsEmpty)
            {
                return false;
            }

            return slots[slotIndex].Remove(count) > 0;
        }

        public bool RemoveItem(int slotIndex, int count, out string itemId, out int removedCount)
        {
            EnsureSlots();
            itemId = string.Empty;
            removedCount = 0;
            if (!IsValidSlot(slotIndex) || count <= 0 || slots[slotIndex].IsEmpty)
            {
                return false;
            }

            itemId = slots[slotIndex].itemId;
            removedCount = slots[slotIndex].Remove(count);
            return removedCount > 0;
        }

        public bool IsValidIndex(int slotIndex)
        {
            EnsureSlots();
            return IsValidSlot(slotIndex);
        }

        public bool UseItem(int slotIndex)
        {
            EnsureSlots();
            if (!IsValidSlot(slotIndex) || slots[slotIndex].IsEmpty)
            {
                return false;
            }

            var slot = slots[slotIndex];
            var item = ItemDatabase.Find(slot.itemId);
            if (item == null || !item.Use(gameObject))
            {
                return false;
            }

            slot.Remove(1);
            return true;
        }

        public string GetInventorySummary()
        {
            EnsureSlots();
            var builder = new StringBuilder();
            builder.Append("Inventory: ");
            var hasAnyItem = false;

            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty)
                {
                    continue;
                }

                if (hasAnyItem)
                {
                    builder.Append(", ");
                }

                builder.Append('[').Append(i).Append("] ")
                    .Append(slot.itemId).Append(" x").Append(slot.count);
                hasAnyItem = true;
            }

            if (!hasAnyItem)
            {
                builder.Append("empty");
            }

            return builder.ToString();
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < slots.Length;
        }

        private void GrantInitialItems()
        {
            if (!grantInitialItemsOnAwake || initialItems == null)
            {
                return;
            }

            foreach (var item in initialItems)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId) || item.Count <= 0 || ContainsItem(item.ItemId))
                {
                    continue;
                }

                AddItem(item.ItemId, item.Count);
            }
        }

        private bool ContainsItem(string itemId)
        {
            EnsureSlots();
            for (var i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].itemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureSlots()
        {
            slotCount = Mathf.Max(1, slotCount);
            if (slots != null && slots.Length == slotCount)
            {
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null)
                    {
                        slots[i] = new InventorySlot();
                    }
                }

                return;
            }

            slots = new InventorySlot[slotCount];
            for (var i = 0; i < slots.Length; i++)
            {
                slots[i] = new InventorySlot();
            }
        }

        [System.Serializable]
        private sealed class InitialInventoryItem
        {
            [SerializeField] private string itemId;
            [SerializeField] private int count = 1;

            public InitialInventoryItem(string itemId, int count)
            {
                this.itemId = itemId;
                this.count = count;
            }

            public string ItemId => itemId;
            public int Count => Mathf.Max(0, count);
        }
    }
}
