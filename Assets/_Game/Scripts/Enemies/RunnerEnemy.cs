using UnityEngine;

namespace TD.Enemies
{
    public class RunnerEnemy : EnemyBase
    {
        private enum RunnerState
        {
            Move,
            WindUp,
            Attack,
            Cooldown
        }

        private RunnerState state;
        private float stateTimer;

        public override void Initialize(Transform targetBase)
        {
            base.Initialize(targetBase);
            state = RunnerState.Move;
            stateTimer = 0f;
        }

        protected override void UpdateBehavior()
        {
            switch (state)
            {
                case RunnerState.Move:
                    MoveToBase();
                    if (IsInBaseAttackRange() && IsOnScreen())
                    {
                        StopMovement();
                        stateTimer = data.attackWindUpDuration;
                        TriggerWindUpFeedback(1.2f);
                        state = RunnerState.WindUp;
                    }
                    break;

                case RunnerState.WindUp:
                    stateTimer -= Time.deltaTime;
                    SetVisualPulse(1f + 0.22f * Mathf.Abs(Mathf.Sin(Time.time * 18f)));
                    if (stateTimer <= 0f)
                    {
                        state = RunnerState.Attack;
                    }
                    break;

                case RunnerState.Attack:
                    SetVisualPulse(1f);
                    DealBaseDamage();
                    stateTimer = data.attackCooldown;
                    state = RunnerState.Cooldown;
                    break;

                case RunnerState.Cooldown:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = RunnerState.Move;
                    }
                    break;
            }
        }
    }
}
