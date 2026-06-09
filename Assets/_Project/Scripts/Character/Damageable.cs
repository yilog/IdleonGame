using IdleonGame.Combat;

namespace IdleonGame.Character
{
    public interface Damageable
    {
        bool IsDead { get; }
        CharacterStats Stats { get; }
        void ApplyDamage(DamageInfo damageInfo);
    }
}