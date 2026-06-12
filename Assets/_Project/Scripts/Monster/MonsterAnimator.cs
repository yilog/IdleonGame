using IdleonGame.Character;
using UnityEngine;

namespace IdleonGame.Monster
{
    [DisallowMultipleComponent]
    public sealed class MonsterAnimator : MonoBehaviour
    {
        public static class Parameters
        {
            public const string MoveSpeed = "MoveSpeed";
            public const string VerticalSpeed = "VerticalSpeed";
            public const string IsGrounded = "IsGrounded";
            public const string IsDead = "IsDead";
            public const string Attack = "Attack";
            public const string GetHit = "GetHit";
            public const string Death = "Death";
        }

        public static class States
        {
            public const string Idle = "Idle";
            public const string Run = "Run";
            public const string Death = "Death";
            public const string Attack = "Attack";
            public const string GetHit = "GetHit";
            public const string Jump = "Jump";
            public const string Fall = "Fall";
        }

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private CharacterStats stats;
        [SerializeField] private float groundedVerticalThreshold = 0.05f;
        [SerializeField] private float actionLockSeconds = 0.25f;
        [SerializeField] private float crossFadeSeconds = 0.02f;

        private int facingDirection = 1;
        private string currentState;
        private float actionLockedUntil;

        private void Reset()
        {
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void LateUpdate()
        {
            if (body == null)
            {
                return;
            }

            var velocity = body.velocity;
            var isDead = stats != null && stats.IsDead;
            if (animator != null)
            {
                animator.SetFloat(Parameters.MoveSpeed, Mathf.Abs(velocity.x));
                animator.SetFloat(Parameters.VerticalSpeed, velocity.y);
                animator.SetBool(Parameters.IsGrounded, Mathf.Abs(velocity.y) <= groundedVerticalThreshold);
                animator.SetBool(Parameters.IsDead, isDead);
            }

            if (isDead)
            {
                PlayState(States.Death);
                return;
            }

            if (Time.time < actionLockedUntil)
            {
                return;
            }

            if (Mathf.Abs(velocity.x) > 0.05f)
            {
                SetFacingDirection(velocity.x);
            }

            if (velocity.y > groundedVerticalThreshold)
            {
                PlayState(States.Jump);
                return;
            }

            if (velocity.y < -groundedVerticalThreshold)
            {
                PlayState(States.Fall);
                return;
            }

            PlayState(Mathf.Abs(velocity.x) > 0.05f ? States.Run : States.Idle);
        }

        public void SetFacingDirection(float horizontal)
        {
            if (spriteRenderer == null || Mathf.Abs(horizontal) <= 0.05f)
            {
                return;
            }

            facingDirection = horizontal > 0f ? 1 : -1;
            spriteRenderer.flipX = facingDirection < 0;
        }

        public void PlayRun()
        {
            PlayState(States.Run, true);
        }

        public void PlayIdle()
        {
            PlayState(States.Idle, true);
        }

        public void PlayAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(Parameters.Attack);
            }

            actionLockedUntil = Time.time + actionLockSeconds;
            PlayState(States.Attack, true);
        }

        public void PlayGetHit()
        {
            if (animator != null)
            {
                animator.SetTrigger(Parameters.GetHit);
            }

            actionLockedUntil = Time.time + actionLockSeconds;
            PlayState(States.GetHit, true);
        }

        public void PlayDeath()
        {
            if (animator != null)
            {
                animator.SetBool(Parameters.IsDead, true);
                animator.SetTrigger(Parameters.Death);
            }

            PlayState(States.Death, true);
        }

        private void PlayState(string stateName, bool force = false)
        {
            if (animator == null || animator.runtimeAnimatorController == null || (!force && currentState == stateName))
            {
                return;
            }

            currentState = stateName;
            animator.CrossFade(stateName, crossFadeSeconds);
        }

        private void CacheReferences()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            if (stats == null)
            {
                stats = GetComponent<CharacterStats>();
            }
        }
    }
}
