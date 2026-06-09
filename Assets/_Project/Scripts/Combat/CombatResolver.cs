using IdleonGame.Character;

namespace IdleonGame.Combat
{
    public static class CombatResolver
    {
        public static int CalculateRawDamage(CharacterStats attacker, AttackDefinition attack)
        {
            var baseAttack = attacker != null ? attacker.BaseAttack : 0;
            var skillPower = attack != null ? attack.AttackPower : 0;
            return baseAttack + skillPower;
        }

        public static int CalculateFinalDamage(CharacterStats attacker, CharacterStats defender, AttackDefinition attack)
        {
            var rawDamage = CalculateRawDamage(attacker, attack);
            var defense = defender != null ? defender.Defense : 0;
            return System.Math.Max(1, rawDamage - defense);
        }
    }
}