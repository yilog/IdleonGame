using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Talents
{
    [CreateAssetMenu(fileName = "TalentDatabase", menuName = "IdleonGame/Talent Database")]
    public sealed class TalentDatabase : ScriptableObject
    {
        private const string ResourcesPath = "TalentDatabase";

        [SerializeField] private List<TalentDefinition> talents = new();

        private readonly Dictionary<string, TalentDefinition> byId = new();
        private bool lookupBuilt;

        public static TalentDatabase Instance => Resources.Load<TalentDatabase>(ResourcesPath);
        public IReadOnlyList<TalentDefinition> Talents => talents;

        public TalentDefinition GetTalent(string talentId)
        {
            if (string.IsNullOrEmpty(talentId))
            {
                return null;
            }

            BuildLookup();
            return byId.TryGetValue(talentId, out var talent) ? talent : null;
        }

        public List<TalentDefinition> GetTalentsByType(TalentType type)
        {
            var result = new List<TalentDefinition>();
            foreach (var talent in talents)
            {
                if (talent != null && talent.TalentType == type)
                {
                    result.Add(talent);
                }
            }

            return result;
        }

        private void BuildLookup()
        {
            if (lookupBuilt)
            {
                return;
            }

            byId.Clear();
            foreach (var talent in talents)
            {
                if (talent != null && !string.IsNullOrEmpty(talent.TalentId))
                {
                    byId[talent.TalentId] = talent;
                }
            }

            lookupBuilt = true;
        }

#if UNITY_EDITOR
        public void EditorSetData(IEnumerable<TalentDefinition> newTalents)
        {
            talents = new List<TalentDefinition>(newTalents);
            lookupBuilt = false;
            byId.Clear();
        }
#endif
    }
}
