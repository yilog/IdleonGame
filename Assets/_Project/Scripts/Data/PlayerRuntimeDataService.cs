using System;
using IdleonGame.Character;
using IdleonGame.Levels;
using IdleonGame.Player;
using IdleonGame.Talents;
using IdleonGame.Upgrades;
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
        public event Action<int, int> LevelUp;
        public event Action<string, int> TalentUpgraded;
        public event Action<string, int> UpgradePurchased;

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
                data.experienceToNextLevel = PlayerExperienceTable.GetRequiredExperience(data.level);
            }

            data.level = Mathf.Clamp(data.level, PlayerExperienceTable.MinLevel, PlayerExperienceTable.MaxLevel);
            data.experienceToNextLevel = PlayerExperienceTable.GetRequiredExperience(data.level);
            data.experience = Math.Max(0d, data.experience);
            if (!PlayerExperienceTable.CanLevelUp(data.level))
            {
                data.experience = Math.Min(data.experience, data.experienceToNextLevel);
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

            data.coins = Math.Max(0, data.coins);

            data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);
            data.currentMana = Mathf.Clamp(data.currentMana, 0, data.maxMana);
            data.UnlockLevel("level1_1");
        }

        public void AddExperience(double amount)
        {
            if (amount <= 0d)
            {
                return;
            }

            EnsureRuntimeDefaults();
            data.experience += amount;
            CheckLevelUps();
            SaveIfNeeded();
        }

        public bool TryUpgradeTalent(string talentId)
        {
            var talent = TalentDatabase.Instance != null ? TalentDatabase.Instance.GetTalent(talentId) : null;
            return TryUpgradeTalent(talent);
        }

        public bool TryUpgradeTalent(TalentDefinition talent)
        {
            if (talent == null || data.talentUpgradePoints <= 0)
            {
                return false;
            }

            if (talent.TalentType == TalentType.Class
                && talent.ClassType != PlayerClassType.None
                && talent.ClassType != data.playerClass)
            {
                return false;
            }

            var currentLevel = data.GetTalentLevel(talent.TalentId);
            if (currentLevel >= talent.MaxLevel)
            {
                return false;
            }

            var nextLevel = currentLevel + 1;
            data.talentUpgradePoints--;
            data.SetTalentLevel(talent.TalentId, nextLevel);
            ApplyTalentUpgradeEffect(talent, nextLevel);
            ApplyToActivePlayerStats();
            TalentUpgraded?.Invoke(talent.TalentId, nextLevel);
            SaveIfNeeded();
            return true;
        }

        public bool TryPurchaseUpgrade(string upgradeId)
        {
            var upgrade = UpgradeDatabase.Instance != null ? UpgradeDatabase.Instance.GetUpgrade(upgradeId) : null;
            return TryPurchaseUpgrade(upgrade);
        }

        public bool TryPurchaseUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            var currentLevel = data.GetUpgradeLevel(upgrade.UpgradeId);
            if (currentLevel >= upgrade.MaxLevel)
            {
                return false;
            }

            var cost = upgrade.GetCostCopper(currentLevel);
            if (data.coins < cost)
            {
                return false;
            }

            var nextLevel = currentLevel + 1;
            data.coins -= cost;
            data.SetUpgradeLevel(upgrade.UpgradeId, nextLevel);
            UpgradePurchased?.Invoke(upgrade.UpgradeId, nextLevel);
            SaveIfNeeded();
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureRuntimeDefaults();
            data.coins = Math.Max(0, data.coins + amount);
            SaveIfNeeded();
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

        private void CheckLevelUps()
        {
            while (PlayerExperienceTable.CanLevelUp(data.level)
                && data.experience >= PlayerExperienceTable.GetRequiredExperience(data.level))
            {
                var previousLevel = data.level;
                data.experience -= PlayerExperienceTable.GetRequiredExperience(data.level);
                data.level++;
                data.experienceToNextLevel = PlayerExperienceTable.GetRequiredExperience(data.level);
                OnLevelUp(previousLevel, data.level);
            }

            data.experienceToNextLevel = PlayerExperienceTable.GetRequiredExperience(data.level);
            if (!PlayerExperienceTable.CanLevelUp(data.level))
            {
                data.experience = Math.Min(data.experience, data.experienceToNextLevel);
            }
        }

        private void OnLevelUp(int previousLevel, int currentLevel)
        {
            data.talentUpgradePoints++;
            LevelUp?.Invoke(previousLevel, currentLevel);
            PlayLevelUpPresentation(previousLevel, currentLevel);
        }

        private void PlayLevelUpPresentation(int previousLevel, int currentLevel)
        {
            Debug.Log($"Player level up: {previousLevel} -> {currentLevel}");
        }

        private void SaveIfNeeded()
        {
            if (autoSave)
            {
                Save();
            }
        }

        private void ApplyTalentUpgradeEffect(TalentDefinition talent, int nextLevel)
        {
            var value = talent.GetUpgradeValueForLevel(nextLevel);
            switch (talent.EffectType)
            {
                case TalentEffectType.MaxHealth:
                    data.maxHealth += Mathf.CeilToInt((float)value);
                    data.currentHealth += Mathf.CeilToInt((float)value);
                    break;
                case TalentEffectType.MaxMana:
                    data.maxMana += Mathf.CeilToInt((float)value);
                    data.currentMana += Mathf.CeilToInt((float)value);
                    break;
            }
        }

        private void ApplyToActivePlayerStats()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            ApplyToStats(player.GetComponent<CharacterStats>());
        }
    }
}
