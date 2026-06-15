using IdleonGame.Character;
using IdleonGame.Data;
using IdleonGame.Talents;
using IdleonGame.Upgrades;
using IdleonGame.Player;

namespace IdleonGame.Combat
{
    public static class CombatResolver
    {
        public static int CalculateRawDamage(CharacterStats attacker, AttackDefinition attack)
        {
            var baseAttack = attacker != null ? attacker.BaseAttack : 0;
            var skillPower = attack != null ? attack.AttackPower : 0;
            var rawDamage = baseAttack + skillPower;
            if (attacker != null && attacker.GetComponent<PlayerController>() != null)
            {
                rawDamage += UpgradeEffectCalculator.GetAttackDamageBonus(PlayerRuntimeDataService.Instance?.Data);
            }

            if (attack != null && attack.SkillType == AttackSkillType.RangedProjectile)
            {
                rawDamage = System.Math.Max(1, (int)System.Math.Round(rawDamage * TalentEffectCalculator.GetArrowDamageMultiplier(PlayerRuntimeDataService.Instance?.Data)));
            }

            return rawDamage;
        }

        public static int CalculateFinalDamage(CharacterStats attacker, CharacterStats defender, AttackDefinition attack)
        {
            var rawDamage = CalculateRawDamage(attacker, attack);
            var defense = defender != null ? defender.Defense : 0;
            return System.Math.Max(1, rawDamage - defense);
        }
    }
}
