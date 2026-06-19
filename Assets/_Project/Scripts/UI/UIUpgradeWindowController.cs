using IdleonGame.Data;
using IdleonGame.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UIUpgradeWindowController : UIWindowController
    {
        public const string WindowIdConst = "ui_upgrade";

        [SerializeField] private Text coinText;
        [SerializeField] private RectTransform upgradeListRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private UIUpgradeItemView upgradeItemPrefab;

        private UICurrencyAmountView coinAmountView;

        protected override void OnOpen(object args)
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(760f, 520f);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            Rebuild();
        }

        private void Update()
        {
            RefreshCoins();
        }

        private void Rebuild()
        {
            RefreshCoins();
            if (upgradeListRoot == null)
            {
                return;
            }

            for (var i = upgradeListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(upgradeListRoot.GetChild(i).gameObject);
            }

            var database = UpgradeDatabase.Instance;
            if (database == null)
            {
                return;
            }

            foreach (var upgrade in database.Upgrades)
            {
                if (upgrade != null)
                {
                    CreateUpgradeRow(upgrade);
                }
            }
        }

        private void CreateUpgradeRow(UpgradeDefinition upgrade)
        {
            if (upgradeItemPrefab == null)
            {
                Debug.LogError("UIUpgradeWindowController requires an upgrade item prefab.");
                return;
            }

            var item = Instantiate(upgradeItemPrefab, upgradeListRoot);
            item.name = upgrade.UpgradeId;
            item.Initialize(upgrade, OnUpgradeItemClicked);
        }

        private void RefreshCoins()
        {
            var coins = PlayerRuntimeDataService.EnsureExists().Data.coins;
            coinAmountView ??= UICurrencyAmountView.AttachTo(coinText);
            if (coinAmountView != null)
            {
                coinAmountView.SetAmount(coins);
            }
        }

        private void OnUpgradeItemClicked(UpgradeDefinition upgrade)
        {
            if (PlayerRuntimeDataService.EnsureExists().TryPurchaseUpgrade(upgrade))
            {
                Rebuild();
            }
        }
    }
}
