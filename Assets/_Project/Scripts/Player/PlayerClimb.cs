using System.Collections.Generic;
using IdleonGame.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace IdleonGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerClimb : MonoBehaviour
    {
        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private float descendGrabDistance = 0.65f;
        [SerializeField] private float climbThroughProbeDistance = 0.2f;
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

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            FindSceneReferences();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            originalGravityScale = body.gravityScale;
            FindSceneReferences();
        }

        public void SetInput(float vertical)
        {
            verticalInput = Mathf.Clamp(vertical, -1f, 1f);
        }

        private void FixedUpdate()
        {
            var canGrabRopeBelow = CanGrabRopeBelow();
            var isOnRope = IsOnRope();

            if (!isClimbing && CanStartClimb(isOnRope, canGrabRopeBelow))
            {
                BeginClimb();
            }

            if (!isClimbing)
            {
                return;
            }

            if (verticalInput < -0.1f && IsBlockedByNonRopeGroundTile(verticalInput))
            {
                EndClimb();
                return;
            }

            var verticalVelocity = GetAllowedClimbInput() * climbSpeed;
            UpdateGroundCollisionMode(canGrabRopeBelow, verticalVelocity);

            if (!IsOnRope() && !CanGrabRopeBelow())
            {
                EndClimb();
                return;
            }

            body.gravityScale = 0f;
            body.velocity = new Vector2(0f, verticalVelocity);

            if (Mathf.Abs(verticalInput) <= 0.1f && CanRestoreGroundCollisions())
            {
                EndClimb();
            }
        }

        private bool CanStartClimb(bool isOnRope, bool canGrabRopeBelow)
        {
            if (Mathf.Abs(verticalInput) <= 0.1f)
            {
                return false;
            }

            if (canGrabRopeBelow)
            {
                return true;
            }

            if (!isOnRope)
            {
                return false;
            }

            return verticalInput > 0.1f || !IsGrounded();
        }

        private bool IsOnRope()
        {
            return ropeTilemap != null && ropeTilemap.HasRopeNearBounds(bodyCollider.bounds);
        }

        private bool CanGrabRopeBelow()
        {
            if (ropeTilemap == null || bodyCollider == null || verticalInput >= -0.1f)
            {
                return false;
            }

            var bounds = bodyCollider.bounds;
            var belowFeet = new Vector2(bounds.center.x, bounds.min.y - descendGrabDistance);
            return ropeTilemap.HasRopeAtWorldPosition(belowFeet);
        }

        private void BeginClimb()
        {
            isClimbing = true;
            body.gravityScale = 0f;
            body.velocity = Vector2.zero;
        }

        private void EndClimb()
        {
            isClimbing = false;
            body.gravityScale = originalGravityScale;
            SetGroundCollisionsIgnored(false);
        }

        private float GetAllowedClimbInput()
        {
            if (Mathf.Abs(verticalInput) <= 0.1f)
            {
                return 0f;
            }

            return IsBlockedByNonRopeGroundTile(verticalInput) ? 0f : verticalInput;
        }

        private bool IsBlockedByNonRopeGroundTile(float direction)
        {
            if (groundTilemap == null || ropeTilemap == null || bodyCollider == null || Mathf.Abs(direction) <= 0.1f)
            {
                return false;
            }

            var bounds = bodyCollider.bounds;
            var sampleY = direction > 0f
                ? bounds.max.y + climbThroughProbeDistance
                : bounds.min.y - climbThroughProbeDistance;
            var cell = groundTilemap.WorldToCell(new Vector2(bounds.center.x, sampleY));

            return groundTilemap.HasTile(cell) && !ropeTilemap.HasRopeAtCell(cell);
        }

        private void UpdateGroundCollisionMode(bool canGrabRopeBelow, float verticalVelocity)
        {
            var shouldIgnore = canGrabRopeBelow || IsOverlappingGroundRopeCell() || IsApproachingGroundRopeCell(verticalVelocity);

            if (!shouldIgnore && !CanRestoreGroundCollisions())
            {
                shouldIgnore = true;
            }

            SetGroundCollisionsIgnored(shouldIgnore);
        }

        private bool IsOverlappingGroundRopeCell()
        {
            if (ropeTilemap == null || groundTilemap == null || bodyCollider == null)
            {
                return false;
            }

            var bounds = bodyCollider.bounds;
            return IsGroundRopeCellAt(new Vector2(bounds.center.x, bounds.center.y))
                || IsGroundRopeCellAt(new Vector2(bounds.center.x, bounds.max.y - 0.05f))
                || IsGroundRopeCellAt(new Vector2(bounds.center.x, bounds.min.y + 0.05f));
        }

        private bool IsApproachingGroundRopeCell(float verticalVelocity)
        {
            if (bodyCollider == null || Mathf.Abs(verticalVelocity) <= 0.01f)
            {
                return false;
            }

            var bounds = bodyCollider.bounds;
            var sampleY = verticalVelocity > 0f
                ? bounds.max.y + climbThroughProbeDistance
                : bounds.min.y - climbThroughProbeDistance;

            return IsGroundRopeCellAt(new Vector2(bounds.center.x, sampleY));
        }

        private bool IsGroundRopeCellAt(Vector2 worldPosition)
        {
            var cell = groundTilemap.WorldToCell(worldPosition);
            return groundTilemap.HasTile(cell) && ropeTilemap.HasRopeAtCell(cell);
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

        private void FindSceneReferences()
        {
            if (ropeTilemap == null)
            {
                ropeTilemap = Object.FindObjectOfType<RopeTilemap>();
            }

            var ground = GameObject.Find("Tilemap_Ground");
            if (ground == null)
            {
                groundTilemap = null;
                climbThroughColliders = new Collider2D[0];
                return;
            }

            if (groundTilemap == null)
            {
                groundTilemap = ground.GetComponent<Tilemap>();
            }

            if (climbThroughColliders != null && climbThroughColliders.Length > 0)
            {
                return;
            }

            var composite = ground.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                climbThroughColliders = new Collider2D[] { composite };
                return;
            }

            var tilemapCollider = ground.GetComponent<TilemapCollider2D>();
            climbThroughColliders = tilemapCollider != null ? new Collider2D[] { tilemapCollider } : new Collider2D[0];
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
