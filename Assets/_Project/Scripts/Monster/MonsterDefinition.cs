using System;
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

    [Serializable]
    public sealed class MonsterDropEntry
    {
        [SerializeField] private string itemId;
        [SerializeField] private int minCount = 1;
        [SerializeField] private int maxCount = 1;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

        public string ItemId => itemId;
        public int MinCount => Mathf.Max(1, minCount);
        public int MaxCount => Mathf.Max(MinCount, maxCount);
        public float DropChance => Mathf.Clamp01(dropChance);

#if UNITY_EDITOR
        public void EditorSetData(string dropItemId, int min, int max, float chance)
        {
            itemId = dropItemId;
            minCount = Mathf.Max(1, min);
            maxCount = Mathf.Max(minCount, max);
            dropChance = Mathf.Clamp01(chance);
        }
#endif
    }

    [CreateAssetMenu(fileName = "MonsterDefinition", menuName = "IdleonGame/Monster Definition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField] private string monsterId = "test_walker";
        [SerializeField] private string displayName = "Test Walker";
        [SerializeField] private Sprite sprite;
        [SerializeField] private Vector2Int tileSize = Vector2Int.one;
        [SerializeField] private int maxHealth = 20;
        [SerializeField] private int maxMana;
        [SerializeField] private int attackPower = 0;
        [SerializeField] private int defense;
        [SerializeField] private MonsterAttackType attackType = MonsterAttackType.None;
        [SerializeField] private float moveSpeed = 1.25f;
        [SerializeField] private bool canAttack;
        [SerializeField] private float deathDestroyDelay = 2f;
        [SerializeField] private MonsterDropEntry[] drops = Array.Empty<MonsterDropEntry>();

        public string MonsterId => monsterId;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public Vector2Int TileSize => tileSize;
        public int MaxHealth => maxHealth;
        public int MaxMana => maxMana;
        public int AttackPower => attackPower;
        public int Defense => defense;
        public MonsterAttackType AttackType => attackType;
        public float MoveSpeed => moveSpeed;
        public bool CanAttack => canAttack;
        public float DeathDestroyDelay => deathDestroyDelay;
        public MonsterDropEntry[] Drops => drops;

#if UNITY_EDITOR
        public void EditorSetData(
            string id,
            string name,
            Sprite monsterSprite,
            Vector2Int size,
            int health,
            int mana,
            int attack,
            int armor,
            MonsterAttackType type,
            float speed,
            bool attacks,
            float destroyDelay,
            MonsterDropEntry[] dropTable)
        {
            monsterId = id;
            displayName = name;
            sprite = monsterSprite;
            tileSize = size;
            maxHealth = health;
            maxMana = mana;
            attackPower = attack;
            defense = armor;
            attackType = type;
            moveSpeed = speed;
            canAttack = attacks;
            deathDestroyDelay = destroyDelay;
            drops = dropTable ?? Array.Empty<MonsterDropEntry>();
        }
#endif
    }
}
