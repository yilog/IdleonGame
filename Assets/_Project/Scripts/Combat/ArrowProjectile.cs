using IdleonGame.Character;
using IdleonGame.Core;
using UnityEngine;

namespace IdleonGame.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ArrowProjectile : MonoBehaviour
    {
        private AttackDefinition attack;
        private CharacterStats ownerStats;
        private GameObject owner;
        private LayerMask targetLayers;
        private Damageable lockedTarget;
        private int direction = 1;

        public void Launch(
            GameObject projectileOwner,
            CharacterStats attackerStats,
            AttackDefinition attackDefinition,
            int facingDirection,
            LayerMask targets,
            Damageable target = null)
        {
            owner = projectileOwner;
            ownerStats = attackerStats;
            attack = attackDefinition;
            targetLayers = targets;
            lockedTarget = target;
            direction = facingDirection >= 0 ? 1 : -1;

            ConfigureComponents();
            Destroy(gameObject, attack != null ? attack.ProjectileLifetime : 1.5f);
        }

        private void ConfigureComponents()
        {
            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.velocity = Vector2.right * direction * (attack != null ? attack.ProjectileSpeed : 8f);

            var box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = attack != null ? attack.HitboxSize : new Vector2(0.5f, 0.18f);

            ConfigurePresentation();
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        private void ConfigurePresentation()
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("ArrowProjectile requires an arrow prefab with a SpriteRenderer for rendering.");
                return;
            }

            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Projectile;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            var damageable = other.GetComponentInParent<Damageable>();
            if (damageable == null || damageable.IsDead)
            {
                return;
            }

            if (lockedTarget != null && !ReferenceEquals(damageable, lockedTarget))
            {
                return;
            }

            var rawDamage = CombatResolver.CalculateRawDamage(ownerStats, attack);
            var finalDamage = CombatResolver.CalculateFinalDamage(ownerStats, damageable.Stats, attack);
            damageable.ApplyDamage(new DamageInfo(owner, other.gameObject, attack, rawDamage, finalDamage));
            Destroy(gameObject);
        }
    }
}
