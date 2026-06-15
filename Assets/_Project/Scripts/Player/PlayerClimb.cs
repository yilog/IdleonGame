using System.Collections.Generic;
using IdleonGame.Levels;
using IdleonGame.Map;
using IdleonGame.Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerClimb : MonoBehaviour, ILevelSceneReferenceClient
    {
        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private float groundCheckDistance = 0.08f;
        [SerializeField] private float minimumGroundNormalY = 0.65f;
        [SerializeField] private RopeTilemap ropeTilemap;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Collider2D[] climbThroughColliders;

        private readonly List<Collider2D> ignoredColliders = new List<Collider2D>();
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private float verticalInput;
        private float originalGravityScale;
        private bool isClimbing;
        private bool isIgnoringGround;

        public bool IsClimbing => isClimbing;
        public bool IsGroundedForNavigation => IsGrounded();

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            ConfigureDefaults();
            FindSceneReferences();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            originalGravityScale = body.gravityScale;
            ConfigureDefaults();
            FindSceneReferences();
        }

        public void SetInput(float vertical)
        {
            verticalInput = Mathf.Clamp(vertical, -1f, 1f);
        }

        public void StopClimbingForNavigation()
        {
            if (!isClimbing)
            {
                return;
            }

            verticalInput = 0f;
            EndClimb();
        }

        public void OnLevelSceneWillUnload(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            verticalInput = 0f;
            if (isClimbing)
            {
                EndClimb();
            }
            else
            {
                SetGroundCollisionsIgnored(false);
            }

            if (ropeTilemap != null && ropeTilemap.gameObject.scene == scene)
            {
                ropeTilemap = null;
            }

            if (groundTilemap != null && groundTilemap.gameObject.scene == scene)
            {
                groundTilemap = null;
            }

            if (ContainsColliderFromScene(climbThroughColliders, scene))
            {
                climbThroughColliders = new Collider2D[0];
            }
        }

        public void OnLevelSceneLoaded(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            ropeTilemap = null;
            groundTilemap = null;
            climbThroughColliders = new Collider2D[0];
            FindSceneReferences();
        }

        private void FixedUpdate()
        {
            if (!isClimbing && CanStartClimb())
            {
                BeginClimb();
            }

            if (!isClimbing)
            {
                return;
            }

            if (!HasRopeAtFoot() && !HasRopeBelowFoot())
            {
                EndClimb();
                return;
            }

            var allowedInput = GetAllowedVerticalInput();
            body.gravityScale = 0f;
            body.velocity = new Vector2(0f, allowedInput * climbSpeed);
            AlignToRopeColumn();
            SetGroundCollisionsIgnored(true);

            if (Mathf.Abs(allowedInput) <= 0.1f && Mathf.Abs(verticalInput) > 0.1f && IsGrounded())
            {
                EndClimb();
            }
        }

        private bool CanStartClimb()
        {
            if (Mathf.Abs(verticalInput) <= 0.1f)
            {
                return false;
            }

            if (verticalInput > 0.1f)
            {
                return HasRopeAtFoot();
            }

            return HasRopeAtFoot() || HasRopeBelowFoot();
        }

        private void BeginClimb()
        {
            isClimbing = true;
            body.gravityScale = 0f;
            body.velocity = Vector2.zero;
            AlignToRopeColumn();
            SetGroundCollisionsIgnored(true);
        }

        private void EndClimb()
        {
            isClimbing = false;
            body.gravityScale = originalGravityScale;
            SetGroundCollisionsIgnored(false);
        }

        private float GetAllowedVerticalInput()
        {
            if (Mathf.Abs(verticalInput) <= 0.1f)
            {
                return 0f;
            }

            if (verticalInput > 0.1f)
            {
                return CanMoveUp() ? verticalInput : 0f;
            }

            return CanMoveDown() ? verticalInput : 0f;
        }

        private bool CanMoveUp()
        {
            var footCell = GetFootCell();
            return HasRopeAtCell(footCell) || HasRopeAtCell(footCell + Vector3Int.up);
        }

        private bool CanMoveDown()
        {
            var footCell = GetFootCell();
            return HasRopeAtCell(footCell) || HasRopeAtCell(footCell + Vector3Int.down);
        }

        private bool HasRopeAtFoot()
        {
            return HasRopeAtCell(GetFootCell());
        }

        private bool HasRopeBelowFoot()
        {
            return HasRopeAtCell(GetFootCell() + Vector3Int.down);
        }

        private bool HasRopeAtCell(Vector3Int cell)
        {
            return ropeTilemap != null && ropeTilemap.HasRopeAtCell(cell);
        }

        private Vector3Int GetFootCell()
        {
            if (ropeTilemap != null)
            {
                return ropeTilemap.WorldToCell(transform.position);
            }

            return groundTilemap != null ? groundTilemap.WorldToCell(transform.position) : Vector3Int.zero;
        }

        private void AlignToRopeColumn()
        {
            if (ropeTilemap == null)
            {
                return;
            }

            var ropeCell = HasRopeAtFoot() ? GetFootCell() : GetFootCell() + Vector3Int.down;
            var ropeCenter = ropeTilemap.GetCellCenterWorld(ropeCell);
            transform.position = new Vector3(ropeCenter.x, transform.position.y, transform.position.z);
        }

        private void SetGroundCollisionsIgnored(bool shouldIgnore)
        {
            if (isIgnoringGround == shouldIgnore || bodyCollider == null || climbThroughColliders == null)
            {
                return;
            }

            if (shouldIgnore)
            {
                ignoredColliders.Clear();
            }

            foreach (var target in climbThroughColliders)
            {
                if (target == null || target == bodyCollider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(bodyCollider, target, shouldIgnore);

                if (shouldIgnore && !ignoredColliders.Contains(target))
                {
                    ignoredColliders.Add(target);
                }
            }

            isIgnoringGround = shouldIgnore;

            if (!shouldIgnore)
            {
                ignoredColliders.Clear();
            }
        }

        private bool CanRestoreGroundCollisions()
        {
            if (bodyCollider == null)
            {
                return true;
            }

            foreach (var target in ignoredColliders)
            {
                if (target == null)
                {
                    continue;
                }

                var distance = Physics2D.Distance(bodyCollider, target);
                if (distance.isOverlapped)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsGrounded()
        {
            if (bodyCollider == null)
            {
                return false;
            }

            var filter = new ContactFilter2D
            {
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

        private void FindSceneReferences()
        {
            if (ropeTilemap == null)
            {
                ropeTilemap = LevelSceneReferenceResolver.FindInActiveScene<RopeTilemap>();
            }

            var ground = LevelSceneReferenceResolver.FindInActiveSceneByName<Tilemap>("Tilemap_Ground");
            if (ground == null)
            {
                groundTilemap = null;
                climbThroughColliders = new Collider2D[0];
                return;
            }

            if (groundTilemap == null)
            {
                groundTilemap = ground;
            }

            if (climbThroughColliders != null && climbThroughColliders.Length > 0)
            {
                return;
            }

            var groundObject = ground.gameObject;
            var composite = groundObject.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                climbThroughColliders = new Collider2D[] { composite };
                return;
            }

            var tilemapCollider = groundObject.GetComponent<TilemapCollider2D>();
            climbThroughColliders = tilemapCollider != null ? new Collider2D[] { tilemapCollider } : new Collider2D[0];
        }

        private static bool ContainsColliderFromScene(Collider2D[] colliders, Scene scene)
        {
            if (colliders == null)
            {
                return false;
            }

            foreach (var target in colliders)
            {
                if (target != null && target.gameObject.scene == scene)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            SetGroundCollisionsIgnored(false);

            if (body != null)
            {
                body.gravityScale = originalGravityScale;
            }
        }
    }
}
