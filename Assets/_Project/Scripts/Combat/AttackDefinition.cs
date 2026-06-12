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
        [SerializeField] private float startupSeconds = 0.05f;
        [SerializeField] private float activeSeconds = 0.05f;
        [SerializeField] private float recoverySeconds = 0.2f;
        [SerializeField] private bool canMoveDuringStartup = true;
        [SerializeField] private bool cancelStartupOnMove = true;
        [SerializeField] private bool canMoveDuringRecovery;
        [SerializeField] private float range = 0.8f;
        [SerializeField] private Vector2 hitboxSize = new Vector2(0.9f, 0.8f);
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetime = 1.5f;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public AttackSkillType SkillType => skillType;
        public int AttackPower => attackPower;
        public int ManaCost => manaCost;
        public float CooldownSeconds => cooldownSeconds;
        public float StartupSeconds => Mathf.Max(0f, startupSeconds);
        public float ActiveSeconds => Mathf.Max(0f, activeSeconds);
        public float RecoverySeconds => Mathf.Max(0f, recoverySeconds);
        public bool CanMoveDuringStartup => canMoveDuringStartup;
        public bool CancelStartupOnMove => cancelStartupOnMove;
        public bool CanMoveDuringRecovery => canMoveDuringRecovery;
        public float Range => range;
        public Vector2 HitboxSize => hitboxSize;
        public float ProjectileSpeed => Mathf.Max(0.1f, projectileSpeed);
        public float ProjectileLifetime => Mathf.Max(0.1f, projectileLifetime);

#if UNITY_EDITOR
        public void EditorSetData(
            string id,
            string name,
            AttackSkillType type,
            int power,
            int mana,
            float cooldown,
            float attackRange,
            Vector2 size,
            float speed = 8f,
            float lifetime = 1.5f,
            float startup = 0.05f,
            float active = 0.05f,
            float recovery = 0.2f)
        {
            skillId = id;
            displayName = name;
            skillType = type;
            attackPower = power;
            manaCost = mana;
            cooldownSeconds = cooldown;
            startupSeconds = Mathf.Max(0f, startup);
            activeSeconds = Mathf.Max(0f, active);
            recoverySeconds = Mathf.Max(0f, recovery);
            canMoveDuringStartup = true;
            cancelStartupOnMove = true;
            canMoveDuringRecovery = false;
            range = attackRange;
            hitboxSize = size;
            projectileSpeed = Mathf.Max(0.1f, speed);
            projectileLifetime = Mathf.Max(0.1f, lifetime);
        }
#endif
    }
}
