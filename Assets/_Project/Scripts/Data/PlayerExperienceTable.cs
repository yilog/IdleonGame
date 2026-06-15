namespace IdleonGame.Data
{
    public static class PlayerExperienceTable
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 100;

        private static readonly double[] RequiredExperienceByLevel = BuildRequiredExperience();

        public static double GetRequiredExperience(int level)
        {
            var clampedLevel = UnityEngine.Mathf.Clamp(level, MinLevel, MaxLevel);
            return RequiredExperienceByLevel[clampedLevel - 1];
        }

        public static bool CanLevelUp(int level)
        {
            return level >= MinLevel && level < MaxLevel;
        }

        private static double[] BuildRequiredExperience()
        {
            var values = new double[MaxLevel];
            values[0] = 50d;
            values[1] = 80d;

            for (var i = 2; i < values.Length; i++)
            {
                values[i] = values[i - 1] + values[i - 2];
            }

            return values;
        }
    }
}
