using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "IdleonGame/Upgrade Database")]
    public sealed class UpgradeDatabase : ScriptableObject
    {
        private const string ResourcesPath = "UpgradeDatabase";

        [SerializeField] private List<UpgradeDefinition> upgrades = new();

        private readonly Dictionary<string, UpgradeDefinition> byId = new();
        private bool lookupBuilt;

        public static UpgradeDatabase Instance => Resources.Load<UpgradeDatabase>(ResourcesPath);
        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;

        public UpgradeDefinition GetUpgrade(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                return null;
            }

            BuildLookup();
            return byId.TryGetValue(upgradeId, out var upgrade) ? upgrade : null;
        }

        private void BuildLookup()
        {
            if (lookupBuilt)
            {
                return;
            }

            byId.Clear();
            foreach (var upgrade in upgrades)
            {
                if (upgrade != null && !string.IsNullOrEmpty(upgrade.UpgradeId))
                {
                    byId[upgrade.UpgradeId] = upgrade;
                }
            }

            lookupBuilt = true;
        }

#if UNITY_EDITOR
        public void EditorSetData(IEnumerable<UpgradeDefinition> newUpgrades)
        {
            upgrades = new List<UpgradeDefinition>(newUpgrades);
            byId.Clear();
            lookupBuilt = false;
        }
#endif
    }
}
