using System;
using IdleonGame.Data;
using UnityEngine;

namespace IdleonGame.Levels
{
    public enum LevelUnlockConditionType
    {
        None,
        KillMonster
    }

    [Serializable]
    public sealed class LevelUnlockCondition
    {
        [SerializeField] private LevelUnlockConditionType conditionType = LevelUnlockConditionType.None;
        [SerializeField] private string monsterId;
        [SerializeField] private int requiredCount = 1;

        public LevelUnlockConditionType ConditionType => conditionType;
        public string MonsterId => monsterId;
        public int RequiredCount => Mathf.Max(1, requiredCount);

        public bool IsMet(PlayerRuntimeData data)
        {
            if (conditionType == LevelUnlockConditionType.None)
            {
                return true;
            }

            if (data == null)
            {
                return false;
            }

            return conditionType switch
            {
                LevelUnlockConditionType.KillMonster => data.GetMonsterKillCount(monsterId) >= RequiredCount,
                _ => false
            };
        }

#if UNITY_EDITOR
        public void EditorSetKillMonsterCondition(string requiredMonsterId, int count)
        {
            conditionType = LevelUnlockConditionType.KillMonster;
            monsterId = requiredMonsterId;
            requiredCount = Mathf.Max(1, count);
        }
#endif
    }
}
