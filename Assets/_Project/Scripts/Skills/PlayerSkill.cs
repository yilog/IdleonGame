using IdleonGame.Character;
using IdleonGame.Combat;
using UnityEngine;

namespace IdleonGame.Skills
{
    public enum SkillCastState
    {
        Idle,
        Startup,
        Active,
        Recovery
    }

    public abstract class PlayerSkill
    {
        private float nextReadyTime;
        private float phaseEndsAt;
        private bool effectApplied;
        private SkillCastContext context;

        protected PlayerSkill(AttackDefinition definition)
        {
            Definition = definition;
        }

        public AttackDefinition Definition { get; }
        public SkillCastState State { get; private set; } = SkillCastState.Idle;
        public bool IsCasting => State != SkillCastState.Idle;
        public bool IsReady => Definition != null && Time.time >= nextReadyTime && !IsCasting;

        public bool BlocksMovement
        {
            get
            {
                if (Definition == null)
                {
                    return false;
                }

                return State switch
                {
                    SkillCastState.Startup => !Definition.CanMoveDuringStartup,
                    SkillCastState.Active => true,
                    SkillCastState.Recovery => !Definition.CanMoveDuringRecovery,
                    _ => false
                };
            }
        }

        public bool TryBegin(SkillCastContext castContext)
        {
            if (!CanBegin(castContext))
            {
                return false;
            }

            if (!castContext.OwnerStats.SpendMana(Definition.ManaCost))
            {
                return false;
            }

            context = castContext;
            State = SkillCastState.Startup;
            effectApplied = false;
            nextReadyTime = Time.time + Definition.CooldownSeconds;
            phaseEndsAt = Time.time + Definition.StartupSeconds;
            OnBegin(context);

            if (Definition.StartupSeconds <= 0f)
            {
                EnterActive();
            }

            return true;
        }

        public void Tick(float movementInputMagnitude)
        {
            if (State == SkillCastState.Idle || Definition == null)
            {
                return;
            }

            if (State == SkillCastState.Startup
                && Definition.CancelStartupOnMove
                && movementInputMagnitude > 0.1f)
            {
                Cancel();
                return;
            }

            if (Time.time < phaseEndsAt)
            {
                return;
            }

            switch (State)
            {
                case SkillCastState.Startup:
                    EnterActive();
                    break;
                case SkillCastState.Active:
                    EnterRecovery();
                    break;
                case SkillCastState.Recovery:
                    Finish();
                    break;
            }
        }

        public void Cancel()
        {
            if (State == SkillCastState.Idle)
            {
                return;
            }

            OnCancel(context);
            State = SkillCastState.Idle;
            effectApplied = false;
        }

        public virtual bool IsTargetInRange(Transform ownerTransform, Damageable target, LayerMask targetLayers)
        {
            return target == null || !target.IsDead;
        }

        protected virtual bool CanBegin(SkillCastContext castContext)
        {
            return IsReady && Definition != null && castContext.OwnerStats != null && !castContext.OwnerStats.IsDead;
        }

        protected virtual void OnBegin(SkillCastContext castContext)
        {
        }

        protected virtual void OnCancel(SkillCastContext castContext)
        {
        }

        protected abstract void ApplyEffect(SkillCastContext castContext);

        private void EnterActive()
        {
            State = SkillCastState.Active;
            phaseEndsAt = Time.time + Definition.ActiveSeconds;
            if (!effectApplied)
            {
                effectApplied = true;
                ApplyEffect(context);
            }

            if (Definition.ActiveSeconds <= 0f)
            {
                EnterRecovery();
            }
        }

        private void EnterRecovery()
        {
            State = SkillCastState.Recovery;
            phaseEndsAt = Time.time + Definition.RecoverySeconds;
            if (Definition.RecoverySeconds <= 0f)
            {
                Finish();
            }
        }

        private void Finish()
        {
            State = SkillCastState.Idle;
            effectApplied = false;
        }
    }
}
