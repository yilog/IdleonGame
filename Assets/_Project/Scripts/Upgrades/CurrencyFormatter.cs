namespace IdleonGame.Upgrades
{
    public static class CurrencyFormatter
    {
        public const int CopperPerSilver = 100;
        public const int SilverPerGold = 100;
        public const int CopperPerGold = CopperPerSilver * SilverPerGold;

        public static void Split(int copperValue, out int gold, out int silver, out int copper)
        {
            var value = System.Math.Max(0, copperValue);
            gold = value / CopperPerGold;
            value %= CopperPerGold;
            silver = value / CopperPerSilver;
            copper = value % CopperPerSilver;
        }

        public static string Format(int copperValue)
        {
            Split(copperValue, out var gold, out var silver, out var copper);
            return $"{gold}G {silver}S {copper}C";
        }
    }
}
