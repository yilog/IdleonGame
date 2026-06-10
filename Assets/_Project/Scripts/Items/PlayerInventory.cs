using System.Text;
using UnityEngine;

namespace IdleonGame.Items
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int slotCount = 24;
        [SerializeField] private InventorySlot[] slots;

        public InventorySlot[] Slots => slots;

        private void Awake()
        {
            EnsureSlots();
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
    }
}