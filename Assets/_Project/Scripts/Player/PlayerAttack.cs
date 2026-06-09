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
        [SerializeField] private CharacterStats stats;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private KeyCode basicAttackKey = KeyCode.J;

        private readonly Collider2D[] hitResults = new Collider2D[8];
        private float nextBasicAttackTime;
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

        private void TryUseBasicAttack()
        {
            if (basicAttack == null || stats == null || Time.time < nextBasicAttackTime || stats.IsDead)
            {
                return;
            }

            if (!stats.SpendMana(basicAttack.ManaCost))
            {
                return;
            }

            nextBasicAttackTime = Time.time + basicAttack.CooldownSeconds;
            ExecuteMeleeHit();
        }

        private void ExecuteMeleeHit()
        {
            var center = (Vector2)transform.position + Vector2.right * facingDirection * basicAttack.Range;
            var hitCount = Physics2D.OverlapBoxNonAlloc(center, basicAttack.HitboxSize, 0f, hitResults, targetLayers);
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

                var rawDamage = CombatResolver.CalculateRawDamage(stats, basicAttack);
                var finalDamage = CombatResolver.CalculateFinalDamage(stats, damageable.Stats, basicAttack);
                damageable.ApplyDamage(new DamageInfo(gameObject, hit.gameObject, basicAttack, rawDamage, finalDamage));
            }
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