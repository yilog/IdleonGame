using IdleonGame.Character;
using IdleonGame.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerMovement : MonoBehaviour, ILevelSceneReferenceClient
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpVelocity = 8f;
        [SerializeField] private LayerMask groundMask = -1;
        [SerializeField] private float groundCheckDistance = 0.08f;
        [SerializeField] private float minimumGroundNormalY = 0.65f;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private float stepUpDuration = 0.2f;
        [SerializeField] private PlayerClimb climb;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private float horizontalInput;
        private bool jumpQueued;
        private bool isStepUpTransitioning;
        private float stepUpElapsed;
        private float stepUpStartY;
        private float stepUpTargetY;

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
            FindSceneReferences();
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

            FindSceneReferences();

            var velocity = body.velocity;
            velocity.x = horizontalInput * moveSpeed;

            if (isStepUpTransitioning)
            {
                body.velocity = velocity;
                TickStepUpTransition();
                jumpQueued = false;
                return;
            }

            if (TryStartStepUpTransition())
            {
                body.velocity = velocity;
                jumpQueued = false;
                return;
            }

            if (jumpQueued && IsGrounded())
            {
                velocity.y = jumpVelocity;
            }

            body.velocity = velocity;
            jumpQueued = false;
        }

        public void OnLevelSceneWillUnload(Scene scene)
        {
            if (groundTilemap != null && groundTilemap.gameObject.scene == scene)
            {
                groundTilemap = null;
            }
        }

        public void OnLevelSceneLoaded(Scene scene)
        {
            groundTilemap = null;
            FindSceneReferences();
        }

        private bool TryStartStepUpTransition()
        {
            if (groundTilemap == null || body == null || bodyCollider == null || Mathf.Abs(horizontalInput) < 0.1f || !IsGrounded())
            {
                return false;
            }

            var currentCell = groundTilemap.WorldToCell(body.position);
            currentCell.z = 0;
            if (!IsStandCell(currentCell))
            {
                return false;
            }

            var direction = horizontalInput > 0f ? 1 : -1;
            var bounds = bodyCollider.bounds;
            var leadingEdgeX = bounds.center.x + bounds.extents.x * direction;
            var nextLeadingEdgeX = leadingEdgeX + horizontalInput * moveSpeed * Time.fixedDeltaTime;
            var nextPosition = new Vector2(nextLeadingEdgeX, body.position.y);
            var nextCell = groundTilemap.WorldToCell(nextPosition);
            nextCell.z = 0;
            var xDelta = nextCell.x - currentCell.x;
            if (xDelta == 0)
            {
                return false;
            }

            direction = xDelta > 0 ? 1 : -1;
            var blockedCell = currentCell + new Vector3Int(direction, 0, 0);
            if (!groundTilemap.HasTile(blockedCell))
            {
                return false;
            }

            var stepUpCell = currentCell + new Vector3Int(direction, 1, 0);
            if (!IsStandCell(stepUpCell))
            {
                return false;
            }

            BeginStepUpTransition(stepUpCell);
            return true;
        }

        private void BeginStepUpTransition(Vector3Int targetStandCell)
        {
            stepUpElapsed = 0f;
            stepUpStartY = body.position.y;
            stepUpTargetY = GetStandCellFootWorldPosition(targetStandCell).y + 0.05f;
            isStepUpTransitioning = true;
        }

        private void TickStepUpTransition()
        {
            stepUpElapsed += Time.fixedDeltaTime;
            var t = Mathf.Clamp01(stepUpElapsed / Mathf.Max(0.01f, stepUpDuration));
            var position = body.position;
            position.y = Mathf.Lerp(stepUpStartY, stepUpTargetY, t);
            body.position = position;

            if (t >= 1f)
            {
                position.y = stepUpTargetY;
                body.position = position;
                isStepUpTransitioning = false;
            }
        }

        private Vector3 GetStandCellFootWorldPosition(Vector3Int standCell)
        {
            var center = groundTilemap.GetCellCenterWorld(standCell);
            return center + Vector3.down * (groundTilemap.layoutGrid.cellSize.y * 0.5f);
        }

        private bool IsStandCell(Vector3Int cell)
        {
            return groundTilemap.HasTile(cell + Vector3Int.down) && !groundTilemap.HasTile(cell);
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

        private void FindSceneReferences()
        {
            if (groundTilemap == null)
            {
                groundTilemap = LevelSceneReferenceResolver.FindInActiveSceneByName<Tilemap>("Tilemap_Ground");
            }
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
