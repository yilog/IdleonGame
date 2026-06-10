using System;

namespace IdleonGame.Items
{
    [Serializable]
    public sealed class InventorySlot
    {
        public string itemId;
        public int count;

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;
        public bool CanStack(string targetItemId)
        {
            if (IsEmpty || itemId != targetItemId)
            {
                return false;
            }

            var item = ItemDatabase.Find(itemId);
            return item != null && count < item.MaxStackCount;
        }

        public int Add(string targetItemId, int amount)
        {
            if (string.IsNullOrEmpty(targetItemId) || amount <= 0)
            {
                return amount;
            }

            var item = ItemDatabase.Find(targetItemId);
            if (item == null)
            {
                return amount;
            }

            if (IsEmpty)
            {
                itemId = targetItemId;
                var addedToEmpty = Math.Min(amount, item.MaxStackCount);
                count = addedToEmpty;
                return amount - addedToEmpty;
            }

            if (itemId != targetItemId)
            {
                return amount;
            }

            var capacity = item.MaxStackCount - count;
            var added = Math.Min(capacity, amount);
            count += added;
            return amount - added;
        }

        public int Remove(int amount)
        {
            if (IsEmpty || amount <= 0)
            {
                return 0;
            }

            var removed = Math.Min(count, amount);
            count -= removed;
            if (count <= 0)
            {
                itemId = string.Empty;
                count = 0;
            }

            return removed;
        }
    }
}