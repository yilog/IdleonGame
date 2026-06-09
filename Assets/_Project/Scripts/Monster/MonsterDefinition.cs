using UnityEngine;

namespace IdleonGame.Monster
{
    public enum MonsterAttackType
    {
        None,
        Contact,
        Melee,
        Ranged
    }

    [CreateAssetMenu(fileName = "MonsterDefinition", menuName = "IdleonGame/Monster Definition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField] private string monsterId = "test_walker";
        [SerializeField] private string displayName = "Test Walker";
        [SerializeField] private Vector2Int tileSize = Vector2Int.one;
        [SerializeField] private int maxHealth = 20;
        [SerializeField] private int maxMana;
        [SerializeField] private int attackPower = 0;
        [SerializeField] private int defense;
        [SerializeField] private MonsterAttackType attackType = MonsterAttackType.None;
        [SerializeField] private float moveSpeed = 1.25f;
        [SerializeField] private bool canAttack;
        [SerializeField] private float deathDestroyDelay = 2f;

        public string MonsterId => monsterId;
        public string DisplayName => displayName;
        public Vector2Int TileSize => tileSize;
        public int MaxHealth => maxHealth;
        public int MaxMana => maxMana;
        public int AttackPower => attackPower;
        public int Defense => defense;
        public MonsterAttackType AttackType => attackType;
        public float MoveSpeed => moveSpeed;
        public bool CanAttack => canAttack;
        public float DeathDestroyDelay => deathDestroyDelay;

#if UNITY_EDITOR
        public void EditorSetData(
            string id,
            string name,
            Vector2Int size,
            int health,
            int mana,
            int attack,
            int armor,
            MonsterAttackType type,
            float speed,
            bool attacks,
            float destroyDelay)
        {
            monsterId = id;
            displayName = name;
            tileSize = size;
            maxHealth = health;
            maxMana = mana;
            attackPower = attack;
            defense = armor;
            attackType = type;
            moveSpeed = speed;
            canAttack = attacks;
            deathDestroyDelay = destroyDelay;
        }
#endif
    }
}