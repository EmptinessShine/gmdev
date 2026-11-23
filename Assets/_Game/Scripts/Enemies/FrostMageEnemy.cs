using UnityEngine;
using TD.Towers;

namespace TD.Enemies
{
    public class FrostMageEnemy : EnemyBase
    {
        private enum MageState
        {
            Move,
            CastWindUp,
            CastFreeze,
            AttackWindUp,
            AttackBase,
            Cooldown
        }

        [SerializeField] private LayerMask towerLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LineRenderer telegraphBeam;
        [SerializeField] private Transform castOrigin;

        private MageState state;
        private float stateTimer;
        private float castCooldownTimer;
        private Tower targetTower;

        public override void Initialize(Transform targetBase)
        {
            base.Initialize(targetBase);
            state = MageState.Move;
            stateTimer = 0f;
            castCooldownTimer = 0f;
            targetTower = null;
            if (telegraphBeam != null) telegraphBeam.enabled = false;
        }

        protected override void UpdateBehavior()
        {
            if (castCooldownTimer > 0f)
            {
                castCooldownTimer -= Time.deltaTime;
            }

            switch (state)
            {
                case MageState.Move:
                    MoveToBase();
                    Tower visibleTower = FindVisibleTower();
                    if (visibleTower != null)
                    {
                        targetTower = visibleTower;
                        StopMovement();
                        stateTimer = data.abilityWindUpDuration;
                        if (telegraphBeam != null) telegraphBeam.enabled = true;
                        TriggerWindUpFeedback(1.45f);
                        state = MageState.CastWindUp;
                    }
                    else if (IsInBaseAttackRange() && IsOnScreen())
                    {
                        StopMovement();
                        stateTimer = data.attackWindUpDuration;
                        TriggerWindUpFeedback(1.05f);
                        state = MageState.AttackWindUp;
                    }
                    break;

                case MageState.CastWindUp:
                    if (targetTower == null)
                    {
                        CancelCast();
                        break;
                    }

                    RotateToward(targetTower.transform.position);
                    UpdateBeam(targetTower.transform.position);
                    SetVisualPulse(1f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 20f)));
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = MageState.CastFreeze;
                    }
                    break;

                case MageState.CastFreeze:
                    SetVisualPulse(1f);
                    if (telegraphBeam != null) telegraphBeam.enabled = false;
                    if (targetTower != null)
                    {
                        targetTower.ApplyFreeze(data.freezeDuration);
                    }
                    targetTower = null;
                    castCooldownTimer = data.abilityCooldown;
                    state = MageState.Move;
                    break;

                case MageState.AttackWindUp:
                    stateTimer -= Time.deltaTime;
                    SetVisualPulse(1f + 0.18f * Mathf.Abs(Mathf.Sin(Time.time * 16f)));
                    if (stateTimer <= 0f)
                    {
                        state = MageState.AttackBase;
                    }
                    break;

                case MageState.AttackBase:
                    SetVisualPulse(1f);
                    DealBaseDamage();
                    stateTimer = data.attackCooldown;
                    state = MageState.Cooldown;
                    break;

                case MageState.Cooldown:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                    {
                        state = MageState.Move;
                    }
                    break;
            }
        }

        private Tower FindVisibleTower()
        {
            if (!IsOnScreen() || castCooldownTimer > 0f)
            {
                return null;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, data.detectRange, towerLayer);
            Tower best = null;
            float bestSqrDist = float.MaxValue;
            Vector3 eyeOrigin = transform.position + Vector3.up * 0.8f;

            for (int i = 0; i < hits.Length; i++)
            {
                Tower tower = hits[i].GetComponentInParent<Tower>();
                if (tower == null || tower.IsFrozen)
                {
                    continue;
                }

                Vector3 flatDir = tower.transform.position - transform.position;
                flatDir.y = 0f;
                if (flatDir.sqrMagnitude < 0.001f)
                {
                    continue;
                }

                if (Vector3.Angle(transform.forward, flatDir) > data.detectHalfAngle)
                {
                    continue;
                }

                Vector3 targetPos = tower.transform.position + Vector3.up * 0.6f;
                Vector3 losDir = targetPos - eyeOrigin;
                float losDist = losDir.magnitude;

                if (Physics.Raycast(eyeOrigin, losDir / losDist, losDist, wallLayer))
                {
                    continue;
                }

                if (flatDir.sqrMagnitude < bestSqrDist)
                {
                    bestSqrDist = flatDir.sqrMagnitude;
                    best = tower;
                }
            }

            return best;
        }

        private void RotateToward(Vector3 worldPos)
        {
            Vector3 dir = worldPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void UpdateBeam(Vector3 towerPos)
        {
            if (telegraphBeam == null) return;
            Vector3 start = castOrigin != null ? castOrigin.position : transform.position + Vector3.up * 0.9f;
            telegraphBeam.SetPosition(0, start);
            telegraphBeam.SetPosition(1, towerPos + Vector3.up * 0.6f);
        }

        private void CancelCast()
        {
            SetVisualPulse(1f);
            if (telegraphBeam != null) telegraphBeam.enabled = false;
            targetTower = null;
            state = MageState.Move;
        }

        protected override void Die()
        {
            if (telegraphBeam != null) telegraphBeam.enabled = false;
            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.detectRange);

            Gizmos.color = Color.yellow;
            Vector3 leftRay = Quaternion.Euler(0f, -data.detectHalfAngle, 0f) * transform.forward * data.detectRange;
            Vector3 rightRay = Quaternion.Euler(0f, data.detectHalfAngle, 0f) * transform.forward * data.detectRange;
            Gizmos.DrawRay(transform.position, leftRay);
            Gizmos.DrawRay(transform.position, rightRay);
        }
    }
}
