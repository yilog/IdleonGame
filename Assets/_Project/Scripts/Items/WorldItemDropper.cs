using System;
using System.Collections.Generic;
using IdleonGame.Core;
using UnityEngine;

namespace IdleonGame.Items
{
    public static class WorldItemDropper
    {
        private const float DefaultScatterRadius = 0.2f;
        private static readonly Vector3 DefaultSpawnOffset = new Vector3(0f, 0.15f, 0f);

        public static void SpawnRandomDrops<TDrop>(
            IEnumerable<TDrop> drops,
            Vector3 origin,
            Func<TDrop, string> itemIdSelector,
            Func<TDrop, int> minCountSelector,
            Func<TDrop, int> maxCountSelector,
            Func<TDrop, float> dropChanceSelector,
            string sourceName = null)
        {
            if (drops == null)
            {
                return;
            }

            foreach (var drop in drops)
            {
                if (drop == null)
                {
                    continue;
                }

                var itemId = itemIdSelector?.Invoke(drop);
                if (string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                var chance = Mathf.Clamp01(dropChanceSelector?.Invoke(drop) ?? 0f);
                if (UnityEngine.Random.value > chance)
                {
                    continue;
                }

                var minCount = Mathf.Max(1, minCountSelector?.Invoke(drop) ?? 1);
                var maxCount = Mathf.Max(minCount, maxCountSelector?.Invoke(drop) ?? minCount);
                var count = UnityEngine.Random.Range(minCount, maxCount + 1);
                SpawnWorldItem(itemId, count, origin, sourceName);
            }
        }

        public static GameObject SpawnWorldItem(
            string itemId,
            int count,
            Vector3 origin,
            string sourceName = null,
            float scatterRadius = DefaultScatterRadius)
        {
            var item = ItemDatabase.Find(itemId);
            if (item == null)
            {
                var source = string.IsNullOrEmpty(sourceName) ? "Unknown source" : sourceName;
                Debug.LogWarning($"{source} tried to drop unknown item id: {itemId}");
                return null;
            }

            var dropObject = new GameObject($"Drop_{itemId}");
            dropObject.transform.position = origin + DefaultSpawnOffset + new Vector3(UnityEngine.Random.Range(-scatterRadius, scatterRadius), 0f, 0f);

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.Icon;
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.WorldItem;

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.55f, 0.55f);

            var pickup = dropObject.AddComponent<WorldItemPickup>();
            pickup.Configure(itemId, count);

            return dropObject;
        }
    }
}
