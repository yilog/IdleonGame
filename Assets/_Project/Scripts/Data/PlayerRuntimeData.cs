using System;
using System.Collections.Generic;

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
        public int experience;
        public int experienceToNextLevel = 280000;
        public string currentLevelId = "level1_1";
        public List<string> unlockedLevelIds = new() { "level1_1" };
        public List<MonsterKillCountData> monsterKillCounts = new();

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
    }

    [Serializable]
    public sealed class MonsterKillCountData
    {
        public string monsterId;
        public int count;
    }
}
