using IdleonGame.Data;
using IdleonGame.Upgrades;
using UnityEngine;

namespace IdleonGame.Items
{
    public enum WorldPickupKind
    {
        Item = 0,
        Currency = 1
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private WorldPickupKind pickupKind;
        [SerializeField] private string itemId;
        [SerializeField] private int count = 1;
        [SerializeField] private int currencyAmount;
        [SerializeField] private Sprite copperCoinSprite;
        [SerializeField] private Sprite silverCoinSprite;
        [SerializeField] private Sprite goldCoinSprite;

        public void Configure(string droppedItemId, int itemCount)
        {
            pickupKind = WorldPickupKind.Item;
            itemId = droppedItemId;
            count = Mathf.Max(1, itemCount);
            UpdateSprite();
        }

        public void ConfigureCurrency(int amount, Sprite copperSprite, Sprite silverSprite, Sprite goldSprite)
        {
            pickupKind = WorldPickupKind.Currency;
            itemId = WorldItemDropper.CurrencyItemId;
            currencyAmount = Mathf.Max(1, amount);
            copperCoinSprite = copperSprite;
            silverCoinSprite = silverSprite;
            goldCoinSprite = goldSprite;
            UpdateSprite();
        }

        private void Awake()
        {
            var itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;
            UpdateSprite();
        }

        public bool TryPickup(PlayerInventory inventory = null)
        {
            if (pickupKind == WorldPickupKind.Currency)
            {
                PlayerRuntimeDataService.EnsureExists().AddCoins(currencyAmount);
                Debug.Log($"Picked up coins: {CurrencyFormatter.Format(currencyAmount)}. Current coins: {CurrencyFormatter.Format(PlayerRuntimeDataService.Instance.Data.coins)}");
                Destroy(gameObject);
                return true;
            }

            if (inventory == null)
            {
                inventory = FindObjectOfType<PlayerInventory>();
            }

            if (inventory == null || string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            if (!inventory.AddItem(itemId, count))
            {
                Debug.Log($"Could not pick up {itemId} x{count}; inventory is full or item id is unknown.");
                return false;
            }

            Debug.Log($"Picked up {itemId} x{count}. {inventory.GetInventorySummary()}");
            Destroy(gameObject);
            return true;
        }

        private void UpdateSprite()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return;
            }

            if (pickupKind == WorldPickupKind.Currency)
            {
                spriteRenderer.sprite = GetCurrencySprite();
                return;
            }

            var item = ItemDatabase.Find(itemId);
            if (spriteRenderer != null && item != null && item.Icon != null)
            {
                spriteRenderer.sprite = item.Icon;
            }
        }

        private Sprite GetCurrencySprite()
        {
            if (currencyAmount >= CurrencyFormatter.CopperPerGold && goldCoinSprite != null)
            {
                return goldCoinSprite;
            }

            if (currencyAmount >= CurrencyFormatter.CopperPerSilver && silverCoinSprite != null)
            {
                return silverCoinSprite;
            }

            return copperCoinSprite;
        }
    }
}
