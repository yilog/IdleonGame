using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Talents
{
    [CreateAssetMenu(fileName = "TalentDefinition", menuName = "IdleonGame/Talent Definition")]
    public sealed class TalentDefinition : ScriptableObject
    {
        [SerializeField] private string talentId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private TalentType talentType;
        [SerializeField] private PlayerClassType classType;
        [SerializeField] private int maxLevel = 100;
        [SerializeField] private TalentEffectType effectType;
        [SerializeField, TextArea] private string upgradeEffectDescription;

        public string TalentId => talentId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public TalentType TalentType => talentType;
        public PlayerClassType ClassType => classType;
        public int MaxLevel => Mathf.Max(1, maxLevel);
        public TalentEffectType EffectType => effectType;
        public string UpgradeEffectDescription => upgradeEffectDescription;

        public double GetUpgradeValueForLevel(int nextLevel)
        {
            var level = Mathf.Clamp(nextLevel, 1, MaxLevel);
            return effectType switch
            {
                TalentEffectType.MaxHealth => 1d + level * 0.1d,
                TalentEffectType.MaxMana => 1d + level * 0.1d,
                TalentEffectType.ArrowDamagePercent => 0.02d,
                _ => 0d
            };
        }

        public string GetNextUpgradeDescription(int currentLevel)
        {
            if (currentLevel >= MaxLevel)
            {
                return "Max level reached.";
            }

            var nextLevel = currentLevel + 1;
            return effectType switch
            {
                TalentEffectType.MaxHealth => $"+{GetUpgradeValueForLevel(nextLevel):0.0} Max HP",
                TalentEffectType.MaxMana => $"+{GetUpgradeValueForLevel(nextLevel):0.0} Max MP",
                TalentEffectType.ArrowDamagePercent => "+2% Arrow Damage",
                _ => upgradeEffectDescription
            };
        }

#if UNITY_EDITOR
        public void EditorSetData(
            string id,
            string name,
            string detail,
            Sprite talentIcon,
            TalentType type,
            PlayerClassType requiredClass,
            int levelCap,
            TalentEffectType effect,
            string upgradeDescription)
        {
            talentId = id;
            displayName = name;
            description = detail;
            icon = talentIcon;
            talentType = type;
            classType = requiredClass;
            maxLevel = Mathf.Max(1, levelCap);
            effectType = effect;
            upgradeEffectDescription = upgradeDescription;
        }
#endif
    }
}
