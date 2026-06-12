using IdleonGame.Character;
using UnityEngine;

namespace IdleonGame.Data
{
    [DisallowMultipleComponent]
    public sealed class PlayerRuntimeDataBinder : MonoBehaviour
    {
        [SerializeField] private CharacterStats stats;
        [SerializeField] private float syncIntervalSeconds = 1f;

        private float nextSyncTime;
        private int lastHealth = -1;
        private int lastMana = -1;
        private int lastMaxHealth = -1;
        private int lastMaxMana = -1;
        private int lastBaseAttack = -1;
        private int lastDefense = -1;

        private void Awake()
        {
            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }

            PlayerRuntimeDataService.EnsureExists().ApplyToStats(stats);
            SyncNow();
        }

        private void LateUpdate()
        {
            if (Time.time < nextSyncTime)
            {
                return;
            }

            SyncNow();
        }

        private void SyncNow()
        {
            if (stats == null)
            {
                return;
            }

            nextSyncTime = Time.time + syncIntervalSeconds;
            if (stats.CurrentHealth == lastHealth
                && stats.CurrentMana == lastMana
                && stats.MaxHealth == lastMaxHealth
                && stats.MaxMana == lastMaxMana
                && stats.BaseAttack == lastBaseAttack
                && stats.Defense == lastDefense)
            {
                return;
            }

            lastHealth = stats.CurrentHealth;
            lastMana = stats.CurrentMana;
            lastMaxHealth = stats.MaxHealth;
            lastMaxMana = stats.MaxMana;
            lastBaseAttack = stats.BaseAttack;
            lastDefense = stats.Defense;
            PlayerRuntimeDataService.EnsureExists().SyncFromStats(stats);
        }
    }
}
