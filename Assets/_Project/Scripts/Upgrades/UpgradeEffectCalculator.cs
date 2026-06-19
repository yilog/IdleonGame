using IdleonGame.Data;

namespace IdleonGame.Upgrades
{
    public static class UpgradeEffectCalculator
    {
        public static int GetAttackDamageBonus(PlayerRuntimeData data)
        {
            if (data == null)
            {
                return 0;
            }

            return data.GetUpgradeLevel("bigger_damage");
        }

        public static float GetMonsterCurrencyDropMultiplier(PlayerRuntimeData data)
        {
            if (data == null)
            {
                return 1f;
            }

            var level = data.GetUpgradeLevel("monster_tax");
            return 1f + level * 0.02f;
        }
    }
}
