using UnityEngine;

namespace IdleonGame.Upgrades
{
    public enum UpgradeEffectType
    {
        AttackDamage = 0
    }

    [CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "IdleonGame/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField, TextArea] private string nextLevelDescription;
        [SerializeField] private UpgradeEffectType effectType;
        [SerializeField] private int maxLevel = 1000;
        [SerializeField] private int baseCostCopper = 6;
        [SerializeField] private int costIncreasePerLevelCopper = 2;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string NextLevelDescription => nextLevelDescription;
        public UpgradeEffectType EffectType => effectType;
        public int MaxLevel => Mathf.Max(1, maxLevel);
        public int BaseCostCopper => Mathf.Max(0, baseCostCopper);
        public int CostIncreasePerLevelCopper => Mathf.Max(0, costIncreasePerLevelCopper);

        public int GetCostCopper(int currentLevel)
        {
            return BaseCostCopper + GetSum(Mathf.Max(0, currentLevel), CostIncreasePerLevelCopper);
        }

        private static int GetValue(int x, int d)
        {
            int n = (x + 2) / 3;
            return 2 + d * n * (n - 1) / 2;
        }

        private static int GetSum(int x, int d)
        {
            int n = (x + 2) / 3;
            int r = x - 3 * (n - 1);

            return
                6 * (n - 1) +
                d * (n - 1) * n * (n - 2) / 2 +
                r * (2 + d * n * (n - 1) / 2);
        }

        public int GetNextLevelEffectValue(int currentLevel)
        {
            return effectType switch
            {
                UpgradeEffectType.AttackDamage => currentLevel + 1,
                _ => 0
            };
        }

        public int GetTotalEffectValue(int currentLevel)
        {
            return effectType switch
            {
                UpgradeEffectType.AttackDamage => Mathf.Max(0, currentLevel),
                _ => 0
            };
        }

        public string GetNextEffectDescription(int currentLevel)
        {
            return effectType switch
            {
                UpgradeEffectType.AttackDamage => $"+{GetNextLevelEffectValue(currentLevel)} Damage",
                _ => nextLevelDescription
            };
        }

#if UNITY_EDITOR
        public void EditorSetData(
            string id,
            string name,
            Sprite upgradeIcon,
            string description,
            UpgradeEffectType effect,
            int levelCap,
            int baseCost,
            int costIncreasePerLevel)
        {
            upgradeId = id;
            displayName = name;
            icon = upgradeIcon;
            nextLevelDescription = description;
            effectType = effect;
            maxLevel = Mathf.Max(1, levelCap);
            baseCostCopper = Mathf.Max(0, baseCost);
            costIncreasePerLevelCopper = Mathf.Max(0, costIncreasePerLevel);
        }
#endif
    }
}
