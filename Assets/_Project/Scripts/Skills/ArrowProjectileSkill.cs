using IdleonGame.Character;
using IdleonGame.Combat;
using UnityEngine;

namespace IdleonGame.Skills
{
    public sealed class ArrowProjectileSkill : PlayerSkill
    {
        private readonly string arrowPrefabPath;
        private readonly Vector3 arrowOffset;
        private GameObject arrowPrefab;

        public ArrowProjectileSkill(AttackDefinition definition, string prefabPath, Vector3 offset)
            : base(definition)
        {
            arrowPrefabPath = prefabPath;
            arrowOffset = offset;
        }

        public bool IsTargetInRange(Transform ownerTransform, Damageable target)
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

            var delta = targetTransform.position - ownerTransform.position;
            var maxDistance = Definition.ProjectileSpeed * Definition.ProjectileLifetime;
            var verticalTolerance = Mathf.Max(0.75f, Definition.HitboxSize.y * 2f);
            return Mathf.Abs(delta.x) <= maxDistance && Mathf.Abs(delta.y) <= verticalTolerance;
        }

        protected override bool CanBegin(SkillCastContext castContext)
        {
            return base.CanBegin(castContext)
                && (castContext.Target == null || IsTargetInRange(castContext.Owner.transform, castContext.Target));
        }

        protected override void ApplyEffect(SkillCastContext castContext)
        {
            if (arrowPrefab == null)
            {
                arrowPrefab = Resources.Load<GameObject>(arrowPrefabPath);
            }

            if (arrowPrefab == null)
            {
                Debug.LogError($"Arrow prefab was not found in Resources: {arrowPrefabPath}");
                return;
            }

            var arrowObject = Object.Instantiate(arrowPrefab);
            arrowObject.name = "Projectile_Arrow";
            arrowObject.transform.position = castContext.Owner.transform.position
                + arrowOffset
                + Vector3.right * castContext.FacingDirection * 0.55f;
            EnsureComponent<Rigidbody2D>(arrowObject);
            EnsureComponent<BoxCollider2D>(arrowObject);
            EnsureComponent<ArrowProjectile>(arrowObject)
                .Launch(castContext.Owner, castContext.OwnerStats, Definition, castContext.FacingDirection, castContext.TargetLayers, castContext.Target);
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
