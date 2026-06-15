using System;
using System.Collections.Generic;
using IdleonGame.Core;
using IdleonGame.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Items
{
    public static class WorldItemDropper
    {
        public const string CurrencyItemId = "currency_coin";
        private const string CopperCoinSpritePath = "Items/Currency/CoinCopper";
        private const string SilverCoinSpritePath = "Items/Currency/CoinSilver";
        private const string GoldCoinSpritePath = "Items/Currency/CoinGold";
        private const float DefaultScatterRadius = 0.2f;
        private static readonly Vector3 DefaultSpawnOffset = new Vector3(0f, 0.18f, 0f);

        public static void SpawnRandomDrops<TDrop>(
            IEnumerable<TDrop> drops,
            Vector3 origin,
            Func<TDrop, string> itemIdSelector,
            Func<TDrop, int> minCountSelector,
            Func<TDrop, int> maxCountSelector,
            Func<TDrop, float> dropChanceSelector,
            string sourceName = null,
            Scene targetScene = default)
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
                SpawnWorldItem(itemId, count, origin, sourceName, DefaultScatterRadius, targetScene);
            }
        }

        public static GameObject SpawnWorldItem(
            string itemId,
            int count,
            Vector3 origin,
            string sourceName = null,
            float scatterRadius = DefaultScatterRadius,
            Scene targetScene = default)
        {
            var item = ItemDatabase.Find(itemId);
            if (item == null)
            {
                var source = string.IsNullOrEmpty(sourceName) ? "Unknown source" : sourceName;
                Debug.LogWarning($"{source} tried to drop unknown item id: {itemId}");
                return null;
            }

            var dropObject = new GameObject($"Drop_{itemId}");
            if (targetScene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(dropObject, targetScene);
            }

            dropObject.transform.position = origin + DefaultSpawnOffset + new Vector3(UnityEngine.Random.Range(-scatterRadius, scatterRadius), 0f, 0f);

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.Icon;
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.WorldItem;

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.27f, 0.27f);

            var pickup = dropObject.AddComponent<WorldItemPickup>();
            pickup.Configure(itemId, count);

            return dropObject;
        }

        public static GameObject SpawnCurrency(
            int amount,
            Vector3 origin,
            string sourceName = null,
            float scatterRadius = DefaultScatterRadius,
            Scene targetScene = default)
        {
            if (amount <= 0)
            {
                return null;
            }

            var copperSprite = Resources.Load<Sprite>(CopperCoinSpritePath);
            var silverSprite = Resources.Load<Sprite>(SilverCoinSpritePath);
            var goldSprite = Resources.Load<Sprite>(GoldCoinSpritePath);
            var selectedSprite = amount >= CurrencyFormatter.CopperPerGold
                ? goldSprite
                : amount >= CurrencyFormatter.CopperPerSilver
                    ? silverSprite
                    : copperSprite;

            if (selectedSprite == null)
            {
                Debug.LogWarning($"{(string.IsNullOrEmpty(sourceName) ? "Unknown source" : sourceName)} tried to drop currency, but currency sprites are missing.");
                return null;
            }

            var dropObject = new GameObject($"Drop_{CurrencyItemId}_{amount}");
            if (targetScene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(dropObject, targetScene);
            }

            dropObject.transform.position = origin + DefaultSpawnOffset + new Vector3(UnityEngine.Random.Range(-scatterRadius, scatterRadius), 0f, 0f);

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = selectedSprite;
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.WorldItem;

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.27f, 0.27f);

            var pickup = dropObject.AddComponent<WorldItemPickup>();
            pickup.ConfigureCurrency(amount, copperSprite, silverSprite, goldSprite);
            return dropObject;
        }
    }
}
