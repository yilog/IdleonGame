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
    }
}
