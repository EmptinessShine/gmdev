using UnityEngine;

namespace TD.Enemies
{
    public class TankEnemy : EnemyBase
    {
        private enum TankState
        {
            Move,
            ShieldWindUp,
            ShieldedMove,
            AttackWindUp,
            Attack,
            Cooldown
        }

        [SerializeField] private GameObject shieldVisual;

        private TankState state;
        private float stateTimer;
        private float shieldCooldownTimer;

        public override void Initialize(Transform targetBase)
        {
            base.Initialize(targetBase);
            state = TankState.Move;
            stateTimer = 0f;
            shieldCooldownTimer = 0f;
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }

        public override void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            if (state == TankState.ShieldedMove)
            {
                amount *= 1f - data.shieldDamageReduction;
            }

            base.TakeDamage(amount);

            if (!IsDead && state == TankState.Move && shieldCooldownTimer <= 0f)
            {
                StopMovement();
                stateTimer = data.abilityWindUpDuration;
                if (shieldVisual != null) shieldVisual.SetActive(true);
                TriggerWindUpFeedback(0.75f);
                state = TankState.ShieldWindUp;
            }
        }

        protected override void UpdateBehavior()
        {
            if (shieldCooldownTimer > 0f)
            {
                shieldCooldownTimer -= Time.deltaTime;
            }

            switch (state)
            {
                case TankState.Move:
                    MoveToBase();
                    if (IsInBaseAttackRange() && IsOnScreen())
                    {
                        EnterAttackWindUp();
                    }
                    break;

                case TankState.ShieldWindUp:
                    stateTimer -= Time.deltaTime;
                    SetVisualPulse(1f + 0.14f * Mathf.Abs(Mathf.Sin(Time.time * 16f)));
                    if (stateTimer <= 0f)
                    {
                        SetVisualPulse(1f);
                        stateTimer = data.shieldDuration;
                        MoveToBase();
                        state = TankState.ShieldedMove;
                    }
                    break;

                case TankState.ShieldedMove:
                    MoveToBase();
                    stateTimer -= Time.deltaTime;
                    if (IsInBaseAttackRange() && IsOnScreen())
                    {
                        EndShield();
                        EnterAttackWindUp();
                    }
                    else if (stateTimer <= 0f)
                    {
                        EndShield();
                        state = TankState.Move;
                    }
                    break;

                case TankState.AttackWindUp:
                    stateTimer -= Time.deltaTime;
                    SetVisualPulse(1f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 14f)));
                    if (stateTimer <= 0f)
                    {
                        state = TankState.Attack;
                    }
                    break;

                case TankState.Attack:
                    SetVisualPulse(1f);
                    DealBaseDamage();
                    stateTimer = data.attackCooldown;
                    state = TankState.Cooldown;
                    break;

                case TankState.Cooldown:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = TankState.Move;
                    }
                    break;
            }
        }

        private void EnterAttackWindUp()
        {
            StopMovement();
            stateTimer = data.attackWindUpDuration;
            TriggerWindUpFeedback(0.85f);
            state = TankState.AttackWindUp;
        }

        private void EndShield()
        {
            if (shieldVisual != null) shieldVisual.SetActive(false);
            shieldCooldownTimer = data.abilityCooldown;
        }
    }
}
