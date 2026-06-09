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

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            climb = GetComponent<PlayerClimb>();
            autoNavigator = GetComponent<PlayerAutoNavigator>();
            attack = GetComponent<PlayerAttack>();
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
        }

        private void Update()
        {
            if (autoNavigator != null && autoNavigator.TryGetInput(out var autoHorizontal, out var autoVertical, out var autoJumpPressed))
            {
                attack?.SetFacingDirection(autoHorizontal);
                climb.SetInput(autoVertical);
                movement.SetInput(autoHorizontal, autoJumpPressed);
                return;
            }

            var horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            var vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            var jumpPressed = UnityEngine.Input.GetButtonDown("Jump");

            attack?.SetFacingDirection(horizontal);
            climb.SetInput(vertical);
            movement.SetInput(horizontal, jumpPressed);
        }
    }
}