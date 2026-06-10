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

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "IdleonGame/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = "test_item";
        [SerializeField] private string displayName = "Test Item";
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemType itemType = ItemType.Material;
        [SerializeField] private int maxStackCount = 99;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public ItemType ItemType => itemType;
        public int MaxStackCount => Mathf.Max(1, maxStackCount);

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
#endif
    }
}