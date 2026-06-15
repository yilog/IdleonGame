using IdleonGame.Data;

namespace IdleonGame.Talents
{
    public static class TalentEffectCalculator
    {
        public static double GetArrowDamageMultiplier(PlayerRuntimeData data)
        {
            if (data == null)
            {
                return 1d;
            }

            var level = data.GetTalentLevel("archer_arrow_power");
            return 1d + level * 0.02d;
        }
    }
}
