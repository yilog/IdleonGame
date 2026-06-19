using IdleonGame.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UICurrencyAmountView : MonoBehaviour
    {
        private const string CopperSpritePath = "Items/Currency/CoinCopper";
        private const string SilverSpritePath = "Items/Currency/CoinSilver";
        private const string GoldSpritePath = "Items/Currency/CoinGold";

        [SerializeField] private Vector2 iconSize = new Vector2(24f, 24f);
        [SerializeField] private float itemWidth = 34f;
        [SerializeField] private float itemSpacing = 4f;
        [SerializeField] private int amountFontSize = 14;

        private Sprite goldSprite;
        private Sprite silverSprite;
        private Sprite copperSprite;
        private Text sourceText;

        private void Awake()
        {
            EnsureInitialized();
        }

        public static UICurrencyAmountView AttachTo(Text text)
        {
            if (text == null)
            {
                return null;
            }

            var view = text.GetComponent<UICurrencyAmountView>();
            if (view == null)
            {
                view = text.gameObject.AddComponent<UICurrencyAmountView>();
            }

            view.EnsureInitialized();
            return view;
        }

        public void SetAmount(int copperValue)
        {
            EnsureInitialized();
            ClearChildren();

            CurrencyFormatter.Split(copperValue, out var gold, out var silver, out var copper);
            var visibleCount = 0;
            if (gold > 0)
            {
                visibleCount++;
            }

            if (silver > 0)
            {
                visibleCount++;
            }

            if (copper > 0 || visibleCount == 0)
            {
                visibleCount++;
            }

            var index = 0;
            if (gold > 0)
            {
                CreateCurrencyItem(index++, visibleCount, goldSprite, gold);
            }

            if (silver > 0)
            {
                CreateCurrencyItem(index++, visibleCount, silverSprite, silver);
            }

            if (copper > 0 || visibleCount == 1 && gold == 0 && silver == 0)
            {
                CreateCurrencyItem(index, visibleCount, copperSprite, copper);
            }
        }

        private void EnsureInitialized()
        {
            sourceText = GetComponent<Text>();
            if (sourceText != null)
            {
                sourceText.text = string.Empty;
            }

            goldSprite ??= Resources.Load<Sprite>(GoldSpritePath);
            silverSprite ??= Resources.Load<Sprite>(SilverSpritePath);
            copperSprite ??= Resources.Load<Sprite>(CopperSpritePath);
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void CreateCurrencyItem(int index, int visibleCount, Sprite sprite, int amount)
        {
            var root = new GameObject("CurrencyItem", typeof(RectTransform));
            root.transform.SetParent(transform, false);

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(itemWidth, iconSize.y + 18f);

            var totalWidth = visibleCount * itemWidth + Mathf.Max(0, visibleCount - 1) * itemSpacing;
            var x = -totalWidth * 0.5f + itemWidth * 0.5f + index * (itemWidth + itemSpacing);
            rootRect.anchoredPosition = new Vector2(x, 0f);

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(root.transform, false);
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = iconSize;

            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var amountObject = new GameObject("Amount", typeof(RectTransform), typeof(Text));
            amountObject.transform.SetParent(root.transform, false);
            var amountRect = amountObject.GetComponent<RectTransform>();
            amountRect.anchorMin = new Vector2(0f, 0f);
            amountRect.anchorMax = new Vector2(1f, 0f);
            amountRect.pivot = new Vector2(0.5f, 0f);
            amountRect.anchoredPosition = Vector2.zero;
            amountRect.sizeDelta = new Vector2(0f, 18f);

            var text = amountObject.GetComponent<Text>();
            text.text = amount.ToString();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = amountFontSize;
            text.color = sourceText != null ? sourceText.color : Color.white;
            text.font = sourceText != null ? sourceText.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.raycastTarget = false;
        }
    }
}
