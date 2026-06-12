using UnityEngine;
using IdleonGame.Data;

namespace IdleonGame.Levels
{
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "IdleonGame/Levels/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private string scenePath;
        [SerializeField] private string defaultSpawnPointId = "default";
        [SerializeField] private Vector2 defaultSpawnPosition = new(-8f, -2f);
        [SerializeField] private LevelUnlockCondition[] unlockConditions = System.Array.Empty<LevelUnlockCondition>();

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public string ScenePath => scenePath;
        public string DefaultSpawnPointId => defaultSpawnPointId;
        public Vector2 DefaultSpawnPosition => defaultSpawnPosition;
        public LevelUnlockCondition[] UnlockConditions => unlockConditions;

        public bool IsUnlocked(PlayerRuntimeData data)
        {
            if (unlockConditions == null || unlockConditions.Length == 0)
            {
                return true;
            }

            foreach (var condition in unlockConditions)
            {
                if (condition != null && !condition.IsMet(data))
                {
                    return false;
                }
            }

            return true;
        }

#if UNITY_EDITOR
        public void EditorSetData(
            string newLevelId,
            string newDisplayName,
            string newSceneName,
            string newScenePath,
            string newDefaultSpawnPointId,
            Vector2 newDefaultSpawnPosition,
            LevelUnlockCondition[] newUnlockConditions = null)
        {
            levelId = newLevelId;
            displayName = newDisplayName;
            sceneName = newSceneName;
            scenePath = newScenePath;
            defaultSpawnPointId = newDefaultSpawnPointId;
            defaultSpawnPosition = newDefaultSpawnPosition;
            unlockConditions = newUnlockConditions ?? System.Array.Empty<LevelUnlockCondition>();
        }
#endif
    }
}
