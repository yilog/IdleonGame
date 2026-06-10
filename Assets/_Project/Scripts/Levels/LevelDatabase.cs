using System.Collections.Generic;
using UnityEngine;

namespace IdleonGame.Levels
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "IdleonGame/Levels/Level Database")]
    public sealed class LevelDatabase : ScriptableObject
    {
        private const string ResourcesPath = "LevelDatabase";

        private static LevelDatabase cached;

        [SerializeField] private List<LevelDefinition> levels = new();
        [SerializeField] private string defaultLevelId = "level1_1";

        public IReadOnlyList<LevelDefinition> Levels => levels;
        public string DefaultLevelId => defaultLevelId;

        public static LevelDatabase Instance
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<LevelDatabase>(ResourcesPath);
                }

                return cached;
            }
        }

        public static LevelDefinition Find(string levelId)
        {
            var database = Instance;
            return database != null ? database.GetLevel(levelId) : null;
        }

        public LevelDefinition GetDefaultLevel()
        {
            return GetLevel(defaultLevelId) ?? (levels.Count > 0 ? levels[0] : null);
        }

        public LevelDefinition GetLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                return null;
            }

            foreach (var level in levels)
            {
                if (level != null && level.LevelId == levelId)
                {
                    return level;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public void EditorSetData(IEnumerable<LevelDefinition> newLevels, string newDefaultLevelId)
        {
            levels = newLevels != null ? new List<LevelDefinition>(newLevels) : new List<LevelDefinition>();
            defaultLevelId = newDefaultLevelId;
            cached = this;
        }
#endif
    }
}
