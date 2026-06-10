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
        private static Sprite cachedArrowSprite;

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
            box.size = attack != null ? attack.HitboxSize : new Vector2(0.7f, 0.18f);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = CreateRuntimeArrowSprite();
            spriteRenderer.sortingOrder = GameRenderLayers.SortingOrders.Projectile;
            transform.localScale = new Vector3(direction, 1f, 1f);
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

        private static Sprite CreateRuntimeArrowSprite()
        {
            if (cachedArrowSprite != null)
            {
                return cachedArrowSprite;
            }

            var texture = new Texture2D(16, 4, TextureFormat.RGBA32, false);
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var shaft = y == 1 || y == 2;
                    var tip = x >= 12 && y >= 0 && y <= 3;
                    var color = shaft || tip ? new Color32(230, 205, 120, 255) : new Color32(0, 0, 0, 0);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            cachedArrowSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
            return cachedArrowSprite;
        }
    }
}
