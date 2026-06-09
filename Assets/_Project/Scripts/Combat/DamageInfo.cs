using UnityEngine;

namespace IdleonGame.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(GameObject source, GameObject target, AttackDefinition attack, int rawDamage, int finalDamage)
        {
            Source = source;
            Target = target;
            Attack = attack;
            RawDamage = rawDamage;
            FinalDamage = finalDamage;
        }

        public GameObject Source { get; }
        public GameObject Target { get; }
        public AttackDefinition Attack { get; }
        public int RawDamage { get; }
        public int FinalDamage { get; }
    }
}