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

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            climb = GetComponent<PlayerClimb>();
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
        }

        private void Update()
        {
            var horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            var vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            var jumpPressed = UnityEngine.Input.GetButtonDown("Jump");

            climb.SetInput(vertical);
            movement.SetInput(horizontal, jumpPressed);
        }
    }
}