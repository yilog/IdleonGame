using UnityEngine;

namespace IdleonGame.Items
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private int count = 1;

        public void Configure(string droppedItemId, int itemCount)
        {
            itemId = droppedItemId;
            count = Mathf.Max(1, itemCount);
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
            var item = ItemDatabase.Find(itemId);
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && item != null && item.Icon != null)
            {
                spriteRenderer.sprite = item.Icon;
            }
        }
    }
}
