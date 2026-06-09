using UnityEngine;

namespace IdleonGame.Combat
{
    public enum AttackSkillType
    {
        BasicMelee,
        RangedProjectile
    }

    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "IdleonGame/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [SerializeField] private string skillId = "basic_melee";
        [SerializeField] private string displayName = "Basic Attack";
        [SerializeField] private AttackSkillType skillType = AttackSkillType.BasicMelee;
        [SerializeField] private int attackPower = 5;
        [SerializeField] private int manaCost;
        [SerializeField] private float cooldownSeconds = 0.45f;
        [SerializeField] private float range = 0.8f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(0.9f, 0.8f);

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public AttackSkillType SkillType => skillType;
        public int AttackPower => attackPower;
        public int ManaCost => manaCost;
        public float CooldownSeconds => cooldownSeconds;
        public float Range => range;
        public Vector2 HitboxSize => hitboxSize;

#if UNITY_EDITOR
        public void EditorSetData(string id, string name, AttackSkillType type, int power, int mana, float cooldown, float attackRange, Vector2 size)
        {
            skillId = id;
            displayName = name;
            skillType = type;
            attackPower = power;
            manaCost = mana;
            cooldownSeconds = cooldown;
            range = attackRange;
            hitboxSize = size;
        }
#endif
    }
}