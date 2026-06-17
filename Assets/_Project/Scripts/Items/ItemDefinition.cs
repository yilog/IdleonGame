using UnityEngine;

namespace IdleonGame.Items
{
    public enum ItemType
    {
        Consumable,
        Material,
        Equipment,
        Quest,
        Currency
    }

    public enum EquipmentSlotType
    {
        None,
        Hat,
        Top,
        Pants,
        Weapon,
        Ring,
        Shoes
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "IdleonGame/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = "test_item";
        [SerializeField] private string displayName = "Test Item";
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemType itemType = ItemType.Material;
        [SerializeField] private int maxStackCount = 99;
        [SerializeField] private EquipmentSlotType equipmentSlot = EquipmentSlotType.None;
        [SerializeField] private int attackBonus;
        [SerializeField] private int defenseBonus;
        [SerializeField] private int maxHealthBonus;
        [SerializeField] private int maxManaBonus;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public ItemType ItemType => itemType;
        public int MaxStackCount => Mathf.Max(1, maxStackCount);
        public EquipmentSlotType EquipmentSlot => equipmentSlot;
        public int AttackBonus => Mathf.Max(0, attackBonus);
        public int DefenseBonus => Mathf.Max(0, defenseBonus);
        public int MaxHealthBonus => Mathf.Max(0, maxHealthBonus);
        public int MaxManaBonus => Mathf.Max(0, maxManaBonus);

        public bool Use(GameObject user)
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                    Debug.Log($"Used item {displayName} on {user.name}.");
                    return true;
                default:
                    Debug.Log($"Item {displayName} cannot be used yet.");
                    return false;
            }
        }

#if UNITY_EDITOR
        public void EditorSetData(string id, string name, Sprite itemIcon, ItemType type, int stackCount)
        {
            itemId = id;
            displayName = name;
            icon = itemIcon;
            itemType = type;
            maxStackCount = Mathf.Max(1, stackCount);
        }

        public void EditorSetEquipmentData(
            string id,
            string name,
            Sprite itemIcon,
            EquipmentSlotType slot,
            int attack,
            int defense,
            int health,
            int mana)
        {
            itemId = id;
            displayName = name;
            icon = itemIcon;
            itemType = ItemType.Equipment;
            maxStackCount = 1;
            equipmentSlot = slot;
            attackBonus = Mathf.Max(0, attack);
            defenseBonus = Mathf.Max(0, defense);
            maxHealthBonus = Mathf.Max(0, health);
            maxManaBonus = Mathf.Max(0, mana);
        }
#endif
    }
}
