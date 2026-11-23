using System;
using UnityEngine;
using UnityEngine.AI;
using TD.Core;
using TD.Data;

namespace TD.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected EnemyData data;

        protected NavMeshAgent agent;
        protected Transform baseTarget;
        protected float currentHp;
        protected float slowTimer;
        protected float currentSlowMultiplier = 1f;
        protected float attackTimer;

        public bool IsDead { get; private set; }
        public EnemyData Data => data;

        public event Action<EnemyBase> OnDied;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public virtual void Initialize(Transform targetBase)
        {
            baseTarget = targetBase;
            currentHp = data.maxHp;
            currentSlowMultiplier = 1f;
            slowTimer = 0f;
            attackTimer = 0f;

            agent.speed = data.moveSpeed;
            agent.stoppingDistance = data.attackRange;

            if (baseTarget != null)
            {
                agent.SetDestination(baseTarget.position);
            }
        }

        protected virtual void Update()
        {
            if (IsDead || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            UpdateSlow();
            UpdateBehavior();
        }

        private void UpdateSlow()
        {
            if (slowTimer <= 0f)
            {
                return;
            }

            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                currentSlowMultiplier = 1f;
                agent.speed = data.moveSpeed;
            }
        }

        protected virtual void UpdateBehavior()
        {
            if (baseTarget == null)
            {
                return;
            }

            float sqrDist = (baseTarget.position - transform.position).sqrMagnitude;
            float sqrRange = data.attackRange * data.attackRange;

            if (sqrDist <= sqrRange)
            {
                agent.isStopped = true;
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    GameManager.Instance.DamageBase(data.baseDamage);
                    attackTimer = data.attackCooldown;
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(baseTarget.position);
            }
        }

        public virtual void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            currentHp -= amount;
            if (currentHp <= 0f)
            {
                Die();
            }
        }

        public void ApplySlow(float multiplier, float duration)
        {
            if (IsDead || duration <= 0f || multiplier >= 1f)
            {
                return;
            }

            currentSlowMultiplier = slowTimer > 0f
                ? Mathf.Min(currentSlowMultiplier, multiplier)
                : multiplier;
            slowTimer = Mathf.Max(slowTimer, duration);
            agent.speed = data.moveSpeed * currentSlowMultiplier;
        }

        protected virtual void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(data.goldReward);
                GameManager.Instance.NotifyEnemyDefeated();
            }

            OnDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
