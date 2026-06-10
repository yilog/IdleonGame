using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Monster
{
    [CreateAssetMenu(fileName = "MonsterDatabase", menuName = "IdleonGame/Monster Database")]
    public sealed class MonsterDatabase : ScriptableObject
    {
        private const string ResourcesPath = "MonsterDatabase";

        [SerializeField] private MonsterDefinition[] monsters = Array.Empty<MonsterDefinition>();

        private static MonsterDatabase cached;
        private Dictionary<string, MonsterDefinition> lookup;

        public static MonsterDatabase Current
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<MonsterDatabase>(ResourcesPath);
                }

                return cached;
            }
        }

        public static MonsterDefinition Find(string monsterId)
        {
            return Current != null ? Current.Get(monsterId) : null;
        }

        public MonsterDefinition Get(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId))
            {
                return null;
            }

            EnsureLookup();
            return lookup.TryGetValue(monsterId, out var monster) ? monster : null;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<string, MonsterDefinition>();
            foreach (var monster in monsters)
            {
                if (monster == null || string.IsNullOrEmpty(monster.MonsterId))
                {
                    continue;
                }

                lookup[monster.MonsterId] = monster;
            }
        }

#if UNITY_EDITOR
        public void EditorSetMonsters(MonsterDefinition[] monsterDefinitions)
        {
            monsters = monsterDefinitions ?? Array.Empty<MonsterDefinition>();
            lookup = null;
            cached = this;
        }
#endif
    }
}
