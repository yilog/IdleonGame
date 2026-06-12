using IdleonGame.Items;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemIconView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countText;
        [SerializeField] private Text nameText;
        [SerializeField] private string itemId;
        [SerializeField] private int count;

        public string ItemId => itemId;
        public int Count => count;

        private void Awake()
        {
            Refresh();
        }

        public void SetItem(string newItemId, int newCount)
        {
            itemId = newItemId;
            count = Mathf.Max(0, newCount);
            Refresh();
        }

        public void Clear()
        {
            itemId = string.Empty;
            count = 0;
            Refresh();
        }

        public void Refresh()
        {
            var definition = ItemDatabase.Find(itemId);
            if (iconImage != null)
            {
                iconImage.enabled = definition != null && definition.Icon != null;
                iconImage.sprite = definition != null ? definition.Icon : null;
            }

            if (countText != null)
            {
                countText.text = count > 1 ? count.ToString() : string.Empty;
            }

            if (nameText != null)
            {
                nameText.text = definition != null ? definition.DisplayName : string.Empty;
            }
        }
    }
}
