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
        [SerializeField] private RopeTilemap ropeTilemap;
        [SerializeField] private Collider2D[] climbThroughColliders;

        private readonly List<Collider2D> ignoredColliders = new List<Collider2D>();
        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private float verticalInput;
        private float originalGravityScale;
        private bool isClimbing;

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
            var isOnRope = IsOnRope();

            if (!isClimbing && isOnRope && Mathf.Abs(verticalInput) > 0.1f)
            {
                BeginClimb();
            }

            if (!isClimbing)
            {
                return;
            }

            if (!isOnRope)
            {
                TryEndClimb();
                return;
            }

            body.gravityScale = 0f;
            body.velocity = new Vector2(0f, verticalInput * climbSpeed);

            if (Mathf.Abs(verticalInput) <= 0.1f && CanRestoreGroundCollisions())
            {
                EndClimb();
            }
        }

        private bool IsOnRope()
        {
            return ropeTilemap != null && ropeTilemap.HasRopeNearBounds(bodyCollider.bounds);
        }

        private void BeginClimb()
        {
            isClimbing = true;
            body.gravityScale = 0f;
            body.velocity = Vector2.zero;
            IgnoreGroundCollisions(true);
        }

        private void TryEndClimb()
        {
            if (CanRestoreGroundCollisions())
            {
                EndClimb();
                return;
            }

            body.gravityScale = 0f;
            body.velocity = new Vector2(0f, Mathf.Max(verticalInput, 0f) * climbSpeed);
        }

        private void EndClimb()
        {
            isClimbing = false;
            body.gravityScale = originalGravityScale;
            IgnoreGroundCollisions(false);
        }

        private void IgnoreGroundCollisions(bool shouldIgnore)
        {
            if (bodyCollider == null || climbThroughColliders == null)
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

        private void FindSceneReferences()
        {
            if (ropeTilemap == null)
            {
                ropeTilemap = Object.FindObjectOfType<RopeTilemap>();
            }

            if (climbThroughColliders != null && climbThroughColliders.Length > 0)
            {
                return;
            }

            var ground = GameObject.Find("Tilemap_Ground");
            if (ground == null)
            {
                climbThroughColliders = new Collider2D[0];
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
            IgnoreGroundCollisions(false);

            if (body != null)
            {
                body.gravityScale = originalGravityScale;
            }
        }
    }
}