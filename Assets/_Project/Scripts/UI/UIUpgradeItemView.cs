using System;
using IdleonGame.Data;
using IdleonGame.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UIUpgradeItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text effectText;
        [SerializeField] private Text costText;
        [SerializeField] private Text levelText;
        [SerializeField] private Button upgradeButton;

        private UpgradeDefinition upgrade;
        private Action<UpgradeDefinition> upgradeClicked;
        private UICurrencyAmountView costAmountView;

        public void Initialize(UpgradeDefinition definition, Action<UpgradeDefinition> onUpgradeClicked)
        {
            upgrade = definition;
            upgradeClicked = onUpgradeClicked;

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (upgrade == null)
            {
                return;
            }

            var runtime = PlayerRuntimeDataService.EnsureExists().Data;
            var currentLevel = runtime.GetUpgradeLevel(upgrade.UpgradeId);
            var cost = upgrade.GetCostCopper(currentLevel);

            if (iconImage != null)
            {
                iconImage.sprite = upgrade.Icon;
            }

            SetText(nameText, upgrade.DisplayName);
            SetText(effectText, upgrade.GetNextEffectDescription(currentLevel));
            costAmountView ??= UICurrencyAmountView.AttachTo(costText);
            if (costAmountView != null)
            {
                costAmountView.SetAmount(cost);
            }

            SetText(levelText, $"Lv {currentLevel}");

            if (upgradeButton != null)
            {
                upgradeButton.interactable = currentLevel < upgrade.MaxLevel && runtime.coins >= cost;
            }
        }

        private void OnUpgradeButtonClicked()
        {
            upgradeClicked?.Invoke(upgrade);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
