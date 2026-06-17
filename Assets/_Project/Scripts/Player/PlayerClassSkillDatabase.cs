using System;
using System.Collections.Generic;
using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Player
{
    [CreateAssetMenu(fileName = "PlayerClassSkillDatabase", menuName = "IdleonGame/Player Class Skill Database")]
    public sealed class PlayerClassSkillDatabase : ScriptableObject
    {
        private const string ResourcesPath = "PlayerClassSkillDatabase";

        [SerializeField] private PlayerClassSkillDefinition[] skillSets = Array.Empty<PlayerClassSkillDefinition>();

        private static PlayerClassSkillDatabase cached;
        private Dictionary<PlayerClassType, PlayerClassSkillDefinition> lookup;

        public static PlayerClassSkillDatabase Current
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<PlayerClassSkillDatabase>(ResourcesPath);
                }

                return cached;
            }
        }

        public static PlayerClassSkillDefinition Find(PlayerClassType classType)
        {
            return Current != null ? Current.Get(classType) : null;
        }

        public PlayerClassSkillDefinition Get(PlayerClassType classType)
        {
            EnsureLookup();
            return lookup.TryGetValue(classType, out var skillSet) ? skillSet : null;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
            {
                return;
            }

            lookup = new Dictionary<PlayerClassType, PlayerClassSkillDefinition>();
            foreach (var skillSet in skillSets)
            {
                if (skillSet == null)
                {
                    continue;
                }

                lookup[skillSet.ClassType] = skillSet;
            }
        }

#if UNITY_EDITOR
        public void EditorSetSkillSets(PlayerClassSkillDefinition[] definitions)
        {
            skillSets = definitions ?? Array.Empty<PlayerClassSkillDefinition>();
            lookup = null;
            cached = this;
        }
#endif
    }
}
