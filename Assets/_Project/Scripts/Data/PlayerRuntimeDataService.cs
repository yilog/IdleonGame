using IdleonGame.Character;
using IdleonGame.Levels;
using UnityEngine;

namespace IdleonGame.Data
{
    public sealed class PlayerRuntimeDataService : MonoBehaviour
    {
        private const string PlayerPrefsKey = "IdleonGame.PlayerRuntimeData";

        public static PlayerRuntimeDataService Instance { get; private set; }

        [SerializeField] private PlayerRuntimeData data = new();
        [SerializeField] private bool autoSave = true;

        public PlayerRuntimeData Data => data;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public static PlayerRuntimeDataService EnsureExists()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var serviceObject = new GameObject("PlayerRuntimeDataService");
            return serviceObject.AddComponent<PlayerRuntimeDataService>();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                data = new PlayerRuntimeData();
                EnsureRuntimeDefaults();
                return;
            }

            data = JsonUtility.FromJson<PlayerRuntimeData>(PlayerPrefs.GetString(PlayerPrefsKey)) ?? new PlayerRuntimeData();
            EnsureRuntimeDefaults();
        }

        private void EnsureRuntimeDefaults()
        {
            if (string.IsNullOrEmpty(data.playerName))
            {
                data.playerName = "YILOG222";
            }

            if (data.experienceToNextLevel <= 0)
            {
                data.experienceToNextLevel = 280000;
            }

            if (data.maxHealth <= 0)
            {
                data.maxHealth = 100;
            }

            if (data.currentHealth <= 0)
            {
                data.currentHealth = data.maxHealth;
            }

            if (data.maxMana < 0)
            {
                data.maxMana = 50;
            }

            if (data.currentMana < 0)
            {
                data.currentMana = data.maxMana;
            }

            if (data.baseAttack <= 0)
            {
                data.baseAttack = 6;
            }

            data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);
            data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);
            data.UnlockLevel("level1_1");
        }

        public void Save()
        {
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void SyncFromStats(CharacterStats stats)
        {
            if (stats == null)
            {
                return;
            }

            var changed = data.maxHealth != stats.MaxHealth
                || data.currentHealth != stats.CurrentHealth
                || data.maxMana != stats.MaxMana
                || data.currentMana != stats.CurrentMana
                || data.baseAttack != stats.BaseAttack
                || data.defense != stats.Defense;

            if (!changed)
            {
                return;
            }

            data.maxHealth = stats.MaxHealth;
            data.currentHealth = stats.CurrentHealth;
            data.maxMana = stats.MaxMana;
            data.currentMana = stats.CurrentMana;
            data.baseAttack = stats.BaseAttack;
            data.defense = stats.Defense;
            SaveIfNeeded();
        }

        public void ApplyToStats(CharacterStats stats)
        {
            if (stats == null)
            {
                return;
            }

            EnsureRuntimeDefaults();
            stats.ConfigureSnapshot(
                data.maxHealth,
                data.currentHealth,
                data.maxMana,
                data.currentMana,
                data.baseAttack,
                data.defense);
        }

        public void SetCurrentLevel(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                return;
            }

            data.currentLevelId = levelId;
            data.UnlockLevel(levelId);
            SaveIfNeeded();
        }

        public bool IsLevelUnlocked(LevelDefinition level)
        {
            if (level == null)
            {
                return false;
            }

            return data.IsLevelUnlocked(level.LevelId) || level.IsUnlocked(data);
        }

        public void RefreshLevelUnlocks(LevelDatabase database)
        {
            if (database == null)
            {
                return;
            }

            foreach (var level in database.Levels)
            {
                if (level != null && level.IsUnlocked(data))
                {
                    data.UnlockLevel(level.LevelId);
                }
            }

            SaveIfNeeded();
        }

        public void RecordMonsterKill(string monsterId)
        {
            data.AddMonsterKill(monsterId);
            RefreshLevelUnlocks(LevelDatabase.Instance);
            SaveIfNeeded();
        }

        private void SaveIfNeeded()
        {
            if (autoSave)
            {
                Save();
            }
        }
    }
}
