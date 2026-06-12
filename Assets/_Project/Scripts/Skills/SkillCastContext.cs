using IdleonGame.Character;
using UnityEngine;

namespace IdleonGame.Skills
{
    public readonly struct SkillCastContext
    {
        public SkillCastContext(GameObject owner, CharacterStats ownerStats, int facingDirection, LayerMask targetLayers, Damageable target)
        {
            Owner = owner;
            OwnerStats = ownerStats;
            FacingDirection = facingDirection >= 0 ? 1 : -1;
            TargetLayers = targetLayers;
            Target = target;
        }

        public GameObject Owner { get; }
        public CharacterStats OwnerStats { get; }
        public int FacingDirection { get; }
        public LayerMask TargetLayers { get; }
        public Damageable Target { get; }
    }
}
