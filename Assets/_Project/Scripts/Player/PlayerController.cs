using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerClimb))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerClimb climb;
        [SerializeField] private PlayerAutoNavigator autoNavigator;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerSkillController skillController;

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            climb = GetComponent<PlayerClimb>();
            autoNavigator = GetComponent<PlayerAutoNavigator>();
            attack = GetComponent<PlayerAttack>();
            skillController = GetComponent<PlayerSkillController>();
        }

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerMovement>();
            }

            if (climb == null)
            {
                climb = GetComponent<PlayerClimb>();
            }

            if (autoNavigator == null)
            {
                autoNavigator = GetComponent<PlayerAutoNavigator>();
            }

            if (attack == null)
            {
                attack = GetComponent<PlayerAttack>();
            }

            if (skillController == null)
            {
                skillController = GetComponent<PlayerSkillController>();
            }
        }

        private void Update()
        {
            if (autoNavigator != null && autoNavigator.TryGetInput(out var autoHorizontal, out var autoVertical, out var autoJumpPressed))
            {
                attack?.SetFacingDirection(autoHorizontal);
                var inputMagnitude = Mathf.Abs(autoHorizontal) + Mathf.Abs(autoVertical) + (autoJumpPressed ? 1f : 0f);
                skillController?.SetMovementInputMagnitude(inputMagnitude);
                skillController?.TickSkill(inputMagnitude);
                if (skillController != null && skillController.BlocksMovement)
                {
                    climb.SetInput(0f);
                    movement.SetInput(0f, false);
                    return;
                }

                climb.SetInput(autoVertical);
                movement.SetInput(autoHorizontal, autoJumpPressed);
                return;
            }

            var horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            var vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            var jumpPressed = UnityEngine.Input.GetButtonDown("Jump");

            attack?.SetFacingDirection(horizontal);
            var manualInputMagnitude = Mathf.Abs(horizontal) + Mathf.Abs(vertical) + (jumpPressed ? 1f : 0f);
            skillController?.SetMovementInputMagnitude(manualInputMagnitude);
            skillController?.TickSkill(manualInputMagnitude);
            if (skillController != null && skillController.BlocksMovement)
            {
                climb.SetInput(0f);
                movement.SetInput(0f, false);
                return;
            }

            climb.SetInput(vertical);
            movement.SetInput(horizontal, jumpPressed);
        }
    }
}
