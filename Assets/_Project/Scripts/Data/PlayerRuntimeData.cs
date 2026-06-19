using System;
using System.Collections.Generic;
using IdleonGame.Talents;

namespace IdleonGame.Data
{
    [Serializable]
    public sealed class PlayerRuntimeData
    {
        public string playerName = "YILOG222";
        public PlayerClassType playerClass = PlayerClassType.Archer;
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int maxMana = 50;
        public int currentMana = 50;
        public int baseAttack = 6;
        public int defense = 1;
        public int level = 1;
        public double experience;
        public double experienceToNextLevel = 50;
        public int talentUpgradePoints;
        public int coins = 0;
        public string currentLevelId = "level1_1";
        public List<string> unlockedLevelIds = new() { "level1_1" };
        public List<MonsterKillCountData> monsterKillCounts = new();
        public List<PlayerTalentLevelData> talentLevels = new();
        public List<PlayerUpgradeLevelData> upgradeLevels = new();

        public bool IsLevelUnlocked(string levelId)
        {
            return !string.IsNullOrEmpty(levelId) && unlockedLevelIds.Contains(levelId);
        }

        public void UnlockLevel(string levelId)
        {
            if (!string.IsNullOrEmpty(levelId) && !unlockedLevelIds.Contains(levelId))
            {
                unlockedLevelIds.Add(levelId);
            }
        }

        public int GetMonsterKillCount(string monsterId)
        {
            if (string.IsNullOrEmpty(monsterId))
            {
                return 0;
            }

            foreach (var entry in monsterKillCounts)
            {
                if (entry != null && entry.monsterId == monsterId)
                {
                    return entry.count;
                }
            }

            return 0;
        }

        public void AddMonsterKill(string monsterId, int amount = 1)
        {
            if (string.IsNullOrEmpty(monsterId) || amount <= 0)
            {
                return;
            }

            foreach (var entry in monsterKillCounts)
            {
                if (entry != null && entry.monsterId == monsterId)
                {
                    entry.count += amount;
                    return;
                }
            }

            monsterKillCounts.Add(new MonsterKillCountData { monsterId = monsterId, count = amount });
        }

        public int GetTalentLevel(string talentId)
        {
            if (string.IsNullOrEmpty(talentId))
            {
                return 0;
            }

            foreach (var entry in talentLevels)
            {
                if (entry != null && entry.talentId == talentId)
                {
                    return Math.Max(0, entry.level);
                }
            }

            return 0;
        }

        public void SetTalentLevel(string talentId, int level)
        {
            if (string.IsNullOrEmpty(talentId))
            {
                return;
            }

            foreach (var entry in talentLevels)
            {
                if (entry != null && entry.talentId == talentId)
                {
                    entry.level = Math.Max(0, level);
                    return;
                }
            }

            talentLevels.Add(new PlayerTalentLevelData { talentId = talentId, level = Math.Max(0, level) });
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                return 0;
            }

            foreach (var entry in upgradeLevels)
            {
                if (entry != null && entry.upgradeId == upgradeId)
                {
                    return Math.Max(0, entry.level);
                }
            }

            return 0;
        }

        public void SetUpgradeLevel(string upgradeId, int level)
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                return;
            }

            foreach (var entry in upgradeLevels)
            {
                if (entry != null && entry.upgradeId == upgradeId)
                {
                    entry.level = Math.Max(0, level);
                    return;
                }
            }

            upgradeLevels.Add(new PlayerUpgradeLevelData { upgradeId = upgradeId, level = Math.Max(0, level) });
        }
    }

    [Serializable]
    public sealed class MonsterKillCountData
    {
        public string monsterId;
        public int count;
    }

    [Serializable]
    public sealed class PlayerTalentLevelData
    {
        public string talentId;
        public int level;
    }

    [Serializable]
    public sealed class PlayerUpgradeLevelData
    {
        public string upgradeId;
        public int level;
    }
}
