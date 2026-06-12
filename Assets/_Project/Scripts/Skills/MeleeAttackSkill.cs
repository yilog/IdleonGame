using IdleonGame.Character;
using IdleonGame.Combat;
using UnityEngine;

namespace IdleonGame.Skills
{
    public sealed class MeleeAttackSkill : PlayerSkill
    {
        private readonly Collider2D[] hitResults;

        public MeleeAttackSkill(AttackDefinition definition, Collider2D[] sharedHitResults)
            : base(definition)
        {
            hitResults = sharedHitResults;
        }

        public bool IsTargetInRange(Transform ownerTransform, Damageable target, LayerMask targetLayers)
        {
            if (Definition == null || ownerTransform == null || target == null || target.IsDead)
            {
                return false;
            }

            var targetTransform = (target as Component)?.transform;
            if (targetTransform == null)
            {
                return false;
            }

            var targetDelta = targetTransform.position - ownerTransform.position;
            var targetDirection = targetDelta.x >= 0f ? 1 : -1;
            var center = (Vector2)ownerTransform.position + Vector2.right * targetDirection * Definition.Range;
            var hitCount = Physics2D.OverlapBoxNonAlloc(center, Definition.HitboxSize, 0f, hitResults, targetLayers);
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hitResults[i];
                if (hit != null && ReferenceEquals(hit.GetComponentInParent<Damageable>(), target))
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool CanBegin(SkillCastContext castContext)
        {
            return base.CanBegin(castContext)
                && (castContext.Target == null || IsTargetInRange(castContext.Owner.transform, castContext.Target, castContext.TargetLayers));
        }

        protected override void ApplyEffect(SkillCastContext castContext)
        {
            var center = (Vector2)castContext.Owner.transform.position + Vector2.right * castContext.FacingDirection * Definition.Range;
            var hitCount = Physics2D.OverlapBoxNonAlloc(center, Definition.HitboxSize, 0f, hitResults, castContext.TargetLayers);
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hitResults[i];
                if (hit == null)
                {
                    continue;
                }

                var damageable = hit.GetComponentInParent<Damageable>();
                if (damageable == null || damageable.IsDead)
                {
                    continue;
                }

                if (castContext.Target != null && !ReferenceEquals(damageable, castContext.Target))
                {
                    continue;
                }

                var rawDamage = CombatResolver.CalculateRawDamage(castContext.OwnerStats, Definition);
                var finalDamage = CombatResolver.CalculateFinalDamage(castContext.OwnerStats, damageable.Stats, Definition);
                damageable.ApplyDamage(new DamageInfo(castContext.Owner, hit.gameObject, Definition, rawDamage, finalDamage));
            }
        }
    }
}
