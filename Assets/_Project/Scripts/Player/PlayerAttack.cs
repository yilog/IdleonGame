using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition rangedAttack;
        [SerializeField] private CharacterStats stats;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private KeyCode basicAttackKey = KeyCode.J;
        [SerializeField] private KeyCode rangedAttackKey = KeyCode.K;

        private readonly Collider2D[] hitResults = new Collider2D[8];
        private float nextBasicAttackTime;
        private float nextRangedAttackTime;
        private int facingDirection = 1;

        private void Reset()
        {
            stats = GetComponent<CharacterStats>();
            targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
        }

        private void Awake()
        {
            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }

            if (targetLayers.value == 0)
            {
                targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
            }

            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (playerLayer >= 0 && monsterLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, monsterLayer, true);
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(basicAttackKey))
            {
                TryUseBasicAttack();
            }

            if (UnityEngine.Input.GetKeyDown(rangedAttackKey))
            {
                TryUseRangedAttack();
            }
        }

        public void SetFacingDirection(float horizontal)
        {
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                facingDirection = horizontal > 0f ? 1 : -1;
            }
        }

        public void Configure(AttackDefinition attackDefinition, LayerMask targets)
        {
            basicAttack = attackDefinition;
            targetLayers = targets;
        }

        public void Configure(AttackDefinition meleeAttack, AttackDefinition projectileAttack, LayerMask targets)
        {
            basicAttack = meleeAttack;
            rangedAttack = projectileAttack;
            targetLayers = targets;
        }

        public bool IsTargetInBasicAttackRange(Damageable target)
        {
            if (basicAttack == null || target == null || target.IsDead)
            {
                return false;
            }

            var targetTransform = (target as Component)?.transform;
            if (targetTransform == null)
            {
                return false;
            }

            var targetDelta = targetTransform.position - transform.position;
            var targetDirection = targetDelta.x >= 0f ? 1 : -1;
            var center = (Vector2)transform.position + Vector2.right * targetDirection * basicAttack.Range;
            var hitCount = Physics2D.OverlapBoxNonAlloc(center, basicAttack.HitboxSize, 0f, hitResults, targetLayers);
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hitResults[i];
                if (hit == null)
                {
                    continue;
                }

                if (ReferenceEquals(hit.GetComponentInParent<Damageable>(), target))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBasicAttackReady()
        {
            return basicAttack != null
                && stats != null
                && !stats.IsDead
                && Time.time >= nextBasicAttackTime;
        }

        public bool IsTargetInRangedAttackRange(Damageable target)
        {
            if (rangedAttack == null || target == null || target.IsDead)
            {
                return false;
            }

            var targetTransform = (target as Component)?.transform;
            if (targetTransform == null)
            {
                return false;
            }

            var delta = targetTransform.position - transform.position;
            var maxDistance = rangedAttack.ProjectileSpeed * rangedAttack.ProjectileLifetime;
            var verticalTolerance = Mathf.Max(0.75f, rangedAttack.HitboxSize.y * 2f);
            return Mathf.Abs(delta.x) <= maxDistance && Mathf.Abs(delta.y) <= verticalTolerance;
        }

        public bool IsRangedAttackReady()
        {
            return rangedAttack != null
                && stats != null
                && !stats.IsDead
                && Time.time >= nextRangedAttackTime;
        }

        public bool TryUseBasicAttack(Damageable preferredTarget = null)
        {
            if (!IsBasicAttackReady())
            {
                return false;
            }

            if (preferredTarget != null)
            {
                FaceTarget(preferredTarget);
                if (!IsTargetInBasicAttackRange(preferredTarget))
                {
                    return false;
                }
            }

            if (!stats.SpendMana(basicAttack.ManaCost))
            {
                return false;
            }

            nextBasicAttackTime = Time.time + basicAttack.CooldownSeconds;
            return ExecuteMeleeHit(preferredTarget);
        }

        public bool TryUseRangedAttack(Damageable preferredTarget = null)
        {
            if (!IsRangedAttackReady())
            {
                return false;
            }

            if (preferredTarget != null)
            {
                FaceTarget(preferredTarget);
                if (!IsTargetInRangedAttackRange(preferredTarget))
                {
                    return false;
                }
            }

            if (!stats.SpendMana(rangedAttack.ManaCost))
            {
                return false;
            }

            nextRangedAttackTime = Time.time + rangedAttack.CooldownSeconds;
            FireArrow(preferredTarget);
            return true;
        }

        private void FireArrow(Damageable preferredTarget = null)
        {
            var arrowObject = new GameObject("Projectile_Arrow");
            arrowObject.transform.position = transform.position + Vector3.right * facingDirection * 0.55f;
            arrowObject.AddComponent<SpriteRenderer>();
            arrowObject.AddComponent<Rigidbody2D>();
            arrowObject.AddComponent<BoxCollider2D>();
            arrowObject.AddComponent<ArrowProjectile>().Launch(gameObject, stats, rangedAttack, facingDirection, targetLayers, preferredTarget);
        }

        private bool ExecuteMeleeHit(Damageable preferredTarget = null)
        {
            var center = (Vector2)transform.position + Vector2.right * facingDirection * basicAttack.Range;
            var hitCount = Physics2D.OverlapBoxNonAlloc(center, basicAttack.HitboxSize, 0f, hitResults, targetLayers);
            var didHit = false;
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

                if (preferredTarget != null && !ReferenceEquals(damageable, preferredTarget))
                {
                    continue;
                }

                var rawDamage = CombatResolver.CalculateRawDamage(stats, basicAttack);
                var finalDamage = CombatResolver.CalculateFinalDamage(stats, damageable.Stats, basicAttack);
                damageable.ApplyDamage(new DamageInfo(gameObject, hit.gameObject, basicAttack, rawDamage, finalDamage));
                didHit = true;
            }

            return didHit;
        }

        private void FaceTarget(Damageable target)
        {
            var targetTransform = (target as Component)?.transform;
            if (targetTransform == null)
            {
                return;
            }

            SetFacingDirection(targetTransform.position.x - transform.position.x);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (basicAttack == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            var center = (Vector2)transform.position + Vector2.right * facingDirection * basicAttack.Range;
            Gizmos.DrawWireCube(center, basicAttack.HitboxSize);
        }
#endif
    }
}
