using System;
using UnityEngine;
using UnityEngine.AI;
using TD.Audio;
using TD.Core;
using TD.Data;
using TD.UI;

namespace TD.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected EnemyData data;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip windUpClip;
        [SerializeField] protected Transform visualRoot;
        [SerializeField] protected DamagePopup damagePopupPrefab;

        protected NavMeshAgent agent;
        protected Transform baseTarget;
        protected float currentHp;
        protected float slowTimer;
        protected float currentSlowMultiplier = 1f;
        protected float attackTimer;
        protected Vector3 initialVisualScale;

        private static AudioClip fallbackWindUpBeep;
        private Vector3 lastPosition;

        public bool IsDead { get; private set; }
        public float DistanceTraveled { get; private set; }
        public EnemyData Data => data;

        public event Action<EnemyBase> OnDied;
        public event Action OnWindUpStarted;
        public event Action OnAttackPerformed;
        public event Action<float, float> OnHealthChanged;
        public event Action<float, bool> OnDamaged;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            initialVisualScale = visualRoot != null ? visualRoot.localScale : transform.localScale;
        }

        public virtual void Initialize(Transform targetBase)
        {
            baseTarget = targetBase;
            currentHp = data.maxHp;
            currentSlowMultiplier = 1f;
            slowTimer = 0f;
            attackTimer = 0f;
            DistanceTraveled = 0f;
            lastPosition = transform.position;

            agent.speed = data.moveSpeed;
            agent.stoppingDistance = data.attackRange;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.autoBraking = false;
            agent.angularSpeed = 360f;
            agent.acceleration = 20f;
            agent.autoRepath = true;
            SetVisualPulse(1f);
            OnHealthChanged?.Invoke(currentHp, data.maxHp);

            if (baseTarget != null && agent.isOnNavMesh)
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

            DistanceTraveled += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

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

            if (IsInBaseAttackRange())
            {
                StopMovement();
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    DealBaseDamage();
                    attackTimer = data.attackCooldown;
                }
            }
            else
            {
                MoveToBase();
            }
        }

        protected bool IsInBaseAttackRange()
        {
            if (baseTarget == null || data == null) return false;
            float sqrDist = (baseTarget.position - transform.position).sqrMagnitude;
            return sqrDist <= data.attackRange * data.attackRange;
        }

        protected bool IsOnScreen()
        {
            Camera cam = Camera.main;
            if (cam == null) return true;
            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            return vp.z > 0f && vp.x >= 0.02f && vp.x <= 0.98f && vp.y >= 0.02f && vp.y <= 0.98f;
        }

        protected void MoveToBase()
        {
            if (baseTarget == null || !agent.isOnNavMesh) return;
            agent.isStopped = false;
            if (!agent.hasPath ||
                agent.pathStatus != NavMeshPathStatus.PathComplete ||
                (agent.destination - baseTarget.position).sqrMagnitude > 0.25f)
            {
                agent.SetDestination(baseTarget.position);
            }
        }

        protected void StopMovement()
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

        protected void DealBaseDamage()
        {
            if (GameManager.Instance != null && data != null)
            {
                GameManager.Instance.DamageBase(data.baseDamage);
            }

            OnAttackPerformed?.Invoke();
        }

        protected void TriggerWindUpFeedback(float pitch = 1f)
        {
            OnWindUpStarted?.Invoke();
            if (audioSource == null) return;

            AudioClip clip = windUpClip != null ? windUpClip : GetFallbackBeep();
            audioSource.pitch = pitch * UnityEngine.Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(clip, 0.45f);
        }

        protected void SetVisualPulse(float scaleMultiplier)
        {
            Transform target = visualRoot != null ? visualRoot : transform;
            target.localScale = initialVisualScale * scaleMultiplier;
        }

        private static AudioClip GetFallbackBeep()
        {
            if (fallbackWindUpBeep != null) return fallbackWindUpBeep;

            const int sampleRate = 22050;
            int length = Mathf.RoundToInt(sampleRate * 0.14f);
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (float)i / length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * 440f * t) * env * 0.35f;
            }

            fallbackWindUpBeep = AudioClip.Create("WindUpBeep", length, 1, sampleRate, false);
            fallbackWindUpBeep.SetData(samples, 0);
            return fallbackWindUpBeep;
        }

        public virtual void TakeDamage(float amount)
        {
            ApplyFinalDamage(amount, false);
        }

        protected void ApplyFinalDamage(float amount, bool wasReduced)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            currentHp = Mathf.Max(0f, currentHp - amount);
            OnHealthChanged?.Invoke(currentHp, data.maxHp);
            OnDamaged?.Invoke(amount, wasReduced);

            if (damagePopupPrefab != null)
            {
                DamagePopup.Spawn(damagePopupPrefab, transform.position + Vector3.up * 1.4f, amount, wasReduced);
            }

            AudioManager.Instance?.PlayHit();

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

            AudioManager.Instance?.PlayDeath();
            AudioManager.Instance?.PlayPickup();

            OnDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
