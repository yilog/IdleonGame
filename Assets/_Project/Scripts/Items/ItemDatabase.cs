using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "IdleonGame/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        private const string ResourcesPath = "ItemDatabase";

        [SerializeField] private ItemDefinition[] items = Array.Empty<ItemDefinition>();

        private static ItemDatabase cached;
        private Dictionary<string, ItemDefinition> lookup;

        public static ItemDatabase Current
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<ItemDatabase>(ResourcesPath);
                }

                return cached;
            }
        }

        public static ItemDefinition Find(string itemId)
        {
            return Current != null ? Current.Get(itemId) : null;
        }

        public ItemDefinition Get(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            EnsureLookup();
            return lookup.TryGetValue(itemId, out var item) ? item : null;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<string, ItemDefinition>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                {
                    continue;
                }

                lookup[item.ItemId] = item;
            }
        }

#if UNITY_EDITOR
        public void EditorSetItems(ItemDefinition[] itemDefinitions)
        {
            items = itemDefinitions ?? Array.Empty<ItemDefinition>();
            lookup = null;
            cached = this;
        }
#endif
    }
}