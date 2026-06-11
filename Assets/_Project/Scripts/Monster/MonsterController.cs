using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Items;
using IdleonGame.Levels;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Monster
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class MonsterController : MonoBehaviour, Damageable
    {
        [SerializeField] private MonsterDefinition definition;
        [SerializeField] private CharacterStats stats;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private int facingDirection = 1;
        [SerializeField] private float edgeProbeDistance = 0.55f;
        [SerializeField] private float groundProbeDistance = 0.12f;
        [SerializeField] private float wallProbeDistance = 0.08f;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private bool isDead;
        private MovementState movementState;
        private float movementStateEndsAt;

        public MonsterDefinition Definition => definition;
        public bool IsDead => isDead || (stats != null && stats.IsDead);
        public CharacterStats Stats => stats;
        public int CurrentHealth => stats != null ? stats.CurrentHealth : 0;

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            stats = GetComponent<CharacterStats>();
            ConfigurePhysics();
            FindSceneReferences();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            stats = GetComponent<CharacterStats>();
            ConfigurePhysics();
            FindSceneReferences();
            ConfigureLayerCollision();

            if (definition != null && stats != null && stats.CurrentHealth <= 0)
            {
                stats.Configure(definition.MaxHealth, definition.MaxMana, definition.AttackPower, definition.Defense);
            }

            PickNextMovementState();
        }

        private void FixedUpdate()
        {
            if (IsDead || definition == null)
            {
                return;
            }

            if (Time.time >= movementStateEndsAt)
            {
                PickNextMovementState();
            }

            if (movementState == MovementState.Idle)
            {
                StopHorizontalMovement();
                return;
            }

            if (!CanContinueForward())
            {
                facingDirection *= -1;
            }

            var velocity = body.velocity;
            velocity.x = facingDirection * definition.MoveSpeed;
            body.velocity = velocity;
        }

        public void Configure(MonsterDefinition monsterDefinition, Tilemap ground)
        {
            definition = monsterDefinition;
            groundTilemap = ground;

            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }

            if (definition != null && stats != null)
            {
                stats.Configure(definition.MaxHealth, definition.MaxMana, definition.AttackPower, definition.Defense);
            }

            PickNextMovementState();
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (IsDead || stats == null)
            {
                return;
            }

            stats.ApplyDamage(damageInfo.FinalDamage);
            if (stats.IsDead)
            {
                Die();
            }
        }

        public void TakeDamage(int amount)
        {
            ApplyDamage(new DamageInfo(null, gameObject, null, amount, amount));
        }

        private void Die()
        {
            isDead = true;
            body.velocity = Vector2.zero;
            body.simulated = false;

            var monsterCollider = GetComponent<Collider2D>();
            if (monsterCollider != null)
            {
                monsterCollider.enabled = false;
            }

            WorldItemDropper.SpawnRandomDrops(
                definition != null ? definition.Drops : null,
                transform.position,
                drop => drop.ItemId,
                drop => drop.MinCount,
                drop => drop.MaxCount,
                drop => drop.DropChance,
                name,
                gameObject.scene);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color32(80, 80, 80, 180);
            }

            var delay = definition != null ? definition.DeathDestroyDelay : 2f;
            Destroy(gameObject, delay);
        }

        private bool CanContinueForward()
        {
            if (groundTilemap == null || bodyCollider == null)
            {
                return true;
            }

            var bounds = bodyCollider.bounds;
            var frontX = bounds.center.x + facingDirection * (bounds.extents.x + edgeProbeDistance);
            var groundProbe = new Vector2(frontX, bounds.min.y - groundProbeDistance);
            var wallProbe = new Vector2(bounds.center.x + facingDirection * (bounds.extents.x + wallProbeDistance), bounds.center.y);

            var hasGroundAhead = groundTilemap.HasTile(groundTilemap.WorldToCell(groundProbe));
            var hasWallAhead = groundTilemap.HasTile(groundTilemap.WorldToCell(wallProbe));
            return hasGroundAhead && !hasWallAhead;
        }

        private void PickNextMovementState()
        {
            if (definition == null)
            {
                movementState = MovementState.Move;
                movementStateEndsAt = float.PositiveInfinity;
                return;
            }

            movementState = Random.value < definition.IdleChance ? MovementState.Idle : MovementState.Move;
            var duration = movementState == MovementState.Idle
                ? Random.Range(definition.MinIdleDuration, definition.MaxIdleDuration)
                : Random.Range(definition.MinMoveDuration, definition.MaxMoveDuration);
            movementStateEndsAt = Time.time + duration;

            if (movementState == MovementState.Move && Random.value < 0.5f)
            {
                facingDirection *= -1;
            }
        }

        private void StopHorizontalMovement()
        {
            var velocity = body.velocity;
            velocity.x = 0f;
            body.velocity = velocity;
        }

        private void ConfigurePhysics()
        {
            if (body != null)
            {
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = CharacterAnchor2D.MonsterColliderSize;
                bodyCollider.offset = CharacterAnchor2D.MonsterColliderOffset;
            }
        }

        private void ConfigureLayerCollision()
        {
            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (playerLayer >= 0 && monsterLayer >= 0)
            {
                gameObject.layer = monsterLayer;
                Physics2D.IgnoreLayerCollision(playerLayer, monsterLayer, true);
                Physics2D.IgnoreLayerCollision(monsterLayer, monsterLayer, true);
            }
        }

        private void FindSceneReferences()
        {
            if (groundTilemap != null)
            {
                return;
            }

            groundTilemap = LevelSceneReferenceResolver.FindInSceneByName<Tilemap>(gameObject.scene, "Tilemap_Ground");
        }

        private enum MovementState
        {
            Idle,
            Move
        }
    }
}
