using IdleonGame.Character;
using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Items
{
    [DisallowMultipleComponent]
    public sealed class PlayerEquipment : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot[] slots;
        [SerializeField] private CharacterStats stats;

        private int appliedAttackBonus;
        private int appliedDefenseBonus;
        private int appliedHealthBonus;
        private int appliedManaBonus;

        public EquipmentSlot[] Slots => slots;

        private void Awake()
        {
            EnsureSlots();
            ApplyBonusesToStats();
        }

        public string GetEquippedItemId(EquipmentSlotType slotType)
        {
            var slot = GetSlot(slotType);
            return slot != null ? slot.itemId : string.Empty;
        }

        public bool CanEquip(string itemId, EquipmentSlotType slotType)
        {
            var item = ItemDatabase.Find(itemId);
            return item != null
                && item.ItemType == ItemType.Equipment
                && item.EquipmentSlot == slotType
                && GetSlot(slotType) != null;
        }

        public bool TryEquipFromInventory(PlayerInventory inventory, int inventorySlotIndex, EquipmentSlotType slotType)
        {
            if (inventory == null || !inventory.IsValidIndex(inventorySlotIndex))
            {
                return false;
            }

            var inventorySlot = inventory.Slots[inventorySlotIndex];
            if (inventorySlot == null || inventorySlot.IsEmpty || !CanEquip(inventorySlot.itemId, slotType))
            {
                return false;
            }

            var targetSlot = GetSlot(slotType);
            if (targetSlot == null)
            {
                return false;
            }

            var newItemId = inventorySlot.itemId;
            var oldItemId = targetSlot.itemId;
            if (!string.IsNullOrEmpty(oldItemId) && !inventory.AddItem(oldItemId, 1))
            {
                return false;
            }

            if (!inventory.RemoveItem(inventorySlotIndex, 1, out _, out _))
            {
                return false;
            }

            targetSlot.itemId = newItemId;
            ApplyBonusesToStats();
            Debug.Log($"Equipped {newItemId} in {slotType}.");
            return true;
        }

        public bool TryUnequipToInventory(EquipmentSlotType slotType, PlayerInventory inventory)
        {
            var slot = GetSlot(slotType);
            if (slot == null || string.IsNullOrEmpty(slot.itemId) || inventory == null)
            {
                return false;
            }

            var itemId = slot.itemId;
            if (!inventory.AddItem(itemId, 1))
            {
                return false;
            }

            slot.itemId = string.Empty;
            ApplyBonusesToStats();
            Debug.Log($"Unequipped {itemId} from {slotType}.");
            return true;
        }

        public void ApplyBonusesToStats()
        {
            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }

            if (stats == null)
            {
                return;
            }

            var newAttackBonus = 0;
            var newDefenseBonus = 0;
            var newHealthBonus = 0;
            var newManaBonus = 0;

            EnsureSlots();
            foreach (var slot in slots)
            {
                var item = slot != null ? ItemDatabase.Find(slot.itemId) : null;
                if (item == null)
                {
                    continue;
                }

                newAttackBonus += item.AttackBonus;
                newDefenseBonus += item.DefenseBonus;
                newHealthBonus += item.MaxHealthBonus;
                newManaBonus += item.MaxManaBonus;
            }

            var baseMaxHealth = Mathf.Max(1, stats.MaxHealth - appliedHealthBonus);
            var baseCurrentHealth = Mathf.Clamp(stats.CurrentHealth - appliedHealthBonus, 0, baseMaxHealth);
            var baseMaxMana = Mathf.Max(0, stats.MaxMana - appliedManaBonus);
            var baseCurrentMana = Mathf.Clamp(stats.CurrentMana - appliedManaBonus, 0, baseMaxMana);
            var baseAttack = Mathf.Max(0, stats.BaseAttack - appliedAttackBonus);
            var baseDefense = Mathf.Max(0, stats.Defense - appliedDefenseBonus);

            stats.ConfigureSnapshot(
                baseMaxHealth + newHealthBonus,
                Mathf.Clamp(baseCurrentHealth + newHealthBonus, 0, baseMaxHealth + newHealthBonus),
                baseMaxMana + newManaBonus,
                Mathf.Clamp(baseCurrentMana + newManaBonus, 0, baseMaxMana + newManaBonus),
                baseAttack + newAttackBonus,
                baseDefense + newDefenseBonus);

            appliedAttackBonus = newAttackBonus;
            appliedDefenseBonus = newDefenseBonus;
            appliedHealthBonus = newHealthBonus;
            appliedManaBonus = newManaBonus;
            PlayerRuntimeDataService.Instance?.SyncFromStats(stats);
        }

        private EquipmentSlot GetSlot(EquipmentSlotType slotType)
        {
            EnsureSlots();
            foreach (var slot in slots)
            {
                if (slot != null && slot.slotType == slotType)
                {
                    return slot;
                }
            }

            return null;
        }

        private void EnsureSlots()
        {
            if (slots != null && slots.Length == 6)
            {
                return;
            }

            slots = new[]
            {
                new EquipmentSlot { slotType = EquipmentSlotType.Hat },
                new EquipmentSlot { slotType = EquipmentSlotType.Top },
                new EquipmentSlot { slotType = EquipmentSlotType.Pants },
                new EquipmentSlot { slotType = EquipmentSlotType.Weapon },
                new EquipmentSlot { slotType = EquipmentSlotType.Ring },
                new EquipmentSlot { slotType = EquipmentSlotType.Shoes }
            };
        }
    }

    [System.Serializable]
    public sealed class EquipmentSlot
    {
        public EquipmentSlotType slotType;
        public string itemId;
    }
}
