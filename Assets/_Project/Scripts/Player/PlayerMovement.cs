using IdleonGame.Character;
using UnityEngine;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpVelocity = 8f;
        [SerializeField] private LayerMask groundMask = -1;
        [SerializeField] private float groundCheckDistance = 0.08f;
        [SerializeField] private float minimumGroundNormalY = 0.65f;
        [SerializeField] private PlayerClimb climb;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private float horizontalInput;
        private bool jumpQueued;

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            climb = GetComponent<PlayerClimb>();
            ConfigureDefaults();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();

            if (climb == null)
            {
                climb = GetComponent<PlayerClimb>();
            }

            ConfigureDefaults();
        }

        public void SetInput(float horizontal, bool jumpPressed)
        {
            horizontalInput = Mathf.Clamp(horizontal, -1f, 1f);
            if (jumpPressed)
            {
                jumpQueued = true;
            }
        }

        private void FixedUpdate()
        {
            if (climb != null && climb.IsClimbing)
            {
                jumpQueued = false;
                return;
            }

            var velocity = body.velocity;
            velocity.x = horizontalInput * moveSpeed;

            if (jumpQueued && IsGrounded())
            {
                velocity.y = jumpVelocity;
            }

            body.velocity = velocity;
            jumpQueued = false;
        }

        private bool IsGrounded()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundMask,
                useTriggers = false
            };

            var hitCount = bodyCollider.Cast(Vector2.down, filter, groundHits, groundCheckDistance);
            for (var i = 0; i < hitCount; i++)
            {
                if (groundHits[i].normal.y >= minimumGroundNormalY)
                {
                    return true;
                }
            }

            return false;
        }

        private void ConfigureDefaults()
        {
            if (body != null)
            {
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            if (bodyCollider != null)
            {
                bodyCollider.size = CharacterAnchor2D.PlayerColliderSize;
                bodyCollider.offset = CharacterAnchor2D.PlayerColliderOffset;
            }
        }
    }
}
