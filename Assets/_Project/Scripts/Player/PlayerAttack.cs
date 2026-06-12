using IdleonGame.Character;
using IdleonGame.Combat;
using IdleonGame.Core;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSkillController))]
    public sealed class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private AttackDefinition basicAttack;
        [SerializeField] private AttackDefinition rangedAttack;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private KeyCode basicAttackKey = KeyCode.J;
        [SerializeField] private KeyCode rangedAttackKey = KeyCode.K;
        [SerializeField] private PlayerSkillController skillController;

        private void Reset()
        {
            skillController = GetComponent<PlayerSkillController>();
            targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
        }

        private void Awake()
        {
            if (skillController == null)
            {
                skillController = GetComponent<PlayerSkillController>();
            }

            if (targetLayers.value == 0)
            {
                targetLayers = LayerMask.GetMask(GameLayerNames.Monster);
            }

            skillController.Configure(basicAttack, rangedAttack, targetLayers);
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
            skillController?.SetFacingDirection(horizontal);
        }

        public void Configure(AttackDefinition attackDefinition, LayerMask targets)
        {
            Configure(attackDefinition, rangedAttack, targets);
        }

        public void Configure(AttackDefinition meleeAttack, AttackDefinition projectileAttack, LayerMask targets)
        {
            basicAttack = meleeAttack;
            rangedAttack = projectileAttack;
            targetLayers = targets;

            if (skillController == null)
            {
                skillController = GetComponent<PlayerSkillController>();
            }

            skillController.Configure(basicAttack, rangedAttack, targetLayers);
        }

        public bool IsTargetInBasicAttackRange(Damageable target)
        {
            return skillController != null && skillController.IsTargetInBasicAttackRange(target);
        }

        public bool IsBasicAttackReady()
        {
            return skillController != null && skillController.IsBasicAttackReady();
        }

        public bool IsTargetInRangedAttackRange(Damageable target)
        {
            return skillController != null && skillController.IsTargetInRangedAttackRange(target);
        }

        public bool IsRangedAttackReady()
        {
            return skillController != null && skillController.IsRangedAttackReady();
        }

        public bool TryUseBasicAttack(Damageable preferredTarget = null)
        {
            return skillController != null && skillController.TryUseBasicAttack(preferredTarget);
        }

        public bool TryUseRangedAttack(Damageable preferredTarget = null)
        {
            return skillController != null && skillController.TryUseRangedAttack(preferredTarget);
        }
    }
}
