using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using IdleonGame.Skills;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class PlayerSkillController : MonoBehaviour
    {
        private const string DefaultArrowPrefabPath = "Prefabs/Projectiles/Arrow";

        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition rangedAttack;
        [SerializeField] private CharacterStats stats;
        [SerializeField] private PlayerAnimator playerAnimator;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Vector3 arrowOffset = new(0, 0.5f, 0);
        [SerializeField] private string arrowPrefabPath = DefaultArrowPrefabPath;

        private readonly Collider2D[] hitResults = new Collider2D[8];
        private MeleeAttackSkill basicAttackSkill;
        private ArrowProjectileSkill rangedAttackSkill;
        private PlayerSkill activeSkill;
        private int facingDirection = 1;
        private float movementInputMagnitude;
        private int lastTickFrame = -1;

        public bool BlocksMovement => activeSkill != null && activeSkill.BlocksMovement;

        private void Reset()
        {
            stats = GetComponent<CharacterStats>();
            playerAnimator = GetComponent<PlayerAnimator>();
            targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
        }

        private void Awake()
        {
            FindReferences();
            RebuildSkills();
            IgnorePlayerMonsterCollision();
        }

        private void Update()
        {
            TickSkill(movementInputMagnitude);
        }

        public void TickSkill(float inputMagnitude)
        {
            if (lastTickFrame == Time.frameCount)
            {
                return;
            }

            lastTickFrame = Time.frameCount;
            movementInputMagnitude = Mathf.Clamp01(inputMagnitude);
            activeSkill?.Tick(movementInputMagnitude);
            if (activeSkill != null && !activeSkill.IsCasting)
            {
                activeSkill = null;
            }

            movementInputMagnitude = 0f;
        }

        public void Configure(AttackDefinition meleeAttack, AttackDefinition projectileAttack, LayerMask targets)
        {
            basicAttack = meleeAttack;
            rangedAttack = projectileAttack;
            targetLayers = targets;
            RebuildSkills();
        }

        public void SetFacingDirection(float horizontal)
        {
            if (Mathf.Abs(horizontal) <= 0.1f)
            {
                return;
            }

            facingDirection = horizontal > 0f ? 1 : -1;
            playerAnimator?.SetFacingDirection(horizontal);
        }

        public void SetMovementInputMagnitude(float magnitude)
        {
            movementInputMagnitude = Mathf.Clamp01(magnitude);
        }

        public bool IsTargetInBasicAttackRange(Damageable target)
        {
            return basicAttackSkill != null && basicAttackSkill.IsTargetInRange(transform, target, targetLayers);
        }

        public bool IsTargetInRangedAttackRange(Damageable target)
        {
            return rangedAttackSkill != null && rangedAttackSkill.IsTargetInRange(transform, target);
        }

        public bool IsBasicAttackReady()
        {
            return basicAttackSkill != null && basicAttackSkill.IsReady;
        }

        public bool IsRangedAttackReady()
        {
            return rangedAttackSkill != null && rangedAttackSkill.IsReady;
        }

        public bool TryUseBasicAttack(Damageable preferredTarget = null)
        {
            return TryBeginSkill(basicAttackSkill, preferredTarget);
        }

        public bool TryUseRangedAttack(Damageable preferredTarget = null)
        {
            return TryBeginSkill(rangedAttackSkill, preferredTarget);
        }

        public void CancelCurrentSkill()
        {
            activeSkill?.Cancel();
            activeSkill = null;
        }

        private bool TryBeginSkill(PlayerSkill skill, Damageable preferredTarget)
        {
            if (skill == null || activeSkill != null)
            {
                return false;
            }

            if (preferredTarget != null)
            {
                FaceTarget(preferredTarget);
            }

            var context = new SkillCastContext(gameObject, stats, facingDirection, targetLayers, preferredTarget);
            if (!skill.TryBegin(context))
            {
                return false;
            }

            activeSkill = skill;
            playerAnimator?.PlayAttack();
            return true;
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

        private void FindReferences()
        {
            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponent<PlayerAnimator>();
            }

            if (targetLayers.value == 0)
            {
                targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
            }

            if (string.IsNullOrWhiteSpace(arrowPrefabPath))
            {
                arrowPrefabPath = DefaultArrowPrefabPath;
            }
        }

        private void RebuildSkills()
        {
            basicAttackSkill = basicAttack != null ? new MeleeAttackSkill(basicAttack, hitResults) : null;
            rangedAttackSkill = rangedAttack != null ? new ArrowProjectileSkill(rangedAttack, arrowPrefabPath, arrowOffset) : null;
            activeSkill = null;
        }

        private static void IgnorePlayerMonsterCollision()
        {
            var playerLayer = LayerMask.NameToLayer(GameLayerNames.Player);
            var monsterLayer = LayerMask.NameToLayer(GameLayerNames.Monster);
            if (playerLayer >= 0 && monsterLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, monsterLayer, true);
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
