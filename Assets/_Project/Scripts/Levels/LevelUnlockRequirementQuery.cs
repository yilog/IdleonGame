using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Levels
{
    public static class LevelUnlockRequirementQuery
    {
        public static bool TryGetRemainingKillRequirement(
            LevelDefinition level,
            PlayerRuntimeData runtimeData,
            out string monsterId,
            out int remainingCount)
        {
            monsterId = null;
            remainingCount = 0;

            var runtimeService = PlayerRuntimeDataService.EnsureExists();
            if (level == null || runtimeData == null || runtimeService.IsLevelUnlocked(level))
            {
                return false;
            }

            var conditions = level.UnlockConditions;
            if (conditions == null)
            {
                return false;
            }

            foreach (var condition in conditions)
            {
                if (condition == null || condition.ConditionType != LevelUnlockConditionType.KillMonster)
                {
                    continue;
                }

                monsterId = condition.MonsterId;
                var currentCount = runtimeData.GetMonsterKillCount(condition.MonsterId);
                remainingCount = Mathf.Max(0, condition.RequiredCount - currentCount);
                return remainingCount > 0;
            }

            return false;
        }
    }
}
