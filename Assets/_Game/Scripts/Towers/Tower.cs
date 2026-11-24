using System;
using UnityEngine;
using TD.Audio;
using TD.Core;
using TD.Data;
using TD.Enemies;
using TD.VFX;

namespace TD.Towers
{
    public class Tower : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform turretHead;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject[] levelVisuals;
        [SerializeField] private GameObject frozenVisual;
        [SerializeField] private RangeIndicator rangeIndicator;

        private float fireCooldown;
        private float freezeTimer;
        private bool isRangeVisible;

        public TowerData Data { get; private set; }
        public int LevelIndex { get; private set; }
        public int LevelNumber => LevelIndex + 1;
        public int TotalInvestedGold { get; private set; }
        public TowerLevelStats CurrentStats => Data.GetLevel(LevelIndex);
        public bool CanUpgrade => Data != null && LevelIndex + 1 < Data.MaxLevel;
        public bool IsFrozen => freezeTimer > 0f;

        public event Action OnFired;
        public event Action<float> OnFrozen;

        public void Initialize(TowerData towerData)
        {
            Data = towerData;
            LevelIndex = 0;
            TotalInvestedGold = CurrentStats.cost;
            fireCooldown = 0f;
            freezeTimer = 0f;
            if (frozenVisual != null) frozenVisual.SetActive(false);
            SetRangeVisible(false);
            ApplyLevelVisual();
        }

        public void SetRangeVisible(bool visible)
        {
            isRangeVisible = visible;
            if (rangeIndicator == null)
            {
                return;
            }

            if (visible && Data != null)
            {
                rangeIndicator.Show(CurrentStats.range);
            }
            else
            {
                rangeIndicator.Hide();
            }
        }

        public int GetNextUpgradeCost()
        {
            return CanUpgrade ? Data.GetLevel(LevelIndex + 1).cost : 0;
        }

        public void Upgrade()
        {
            if (!CanUpgrade)
            {
                return;
            }

            LevelIndex++;
            TotalInvestedGold += CurrentStats.cost;
            ApplyLevelVisual();
            if (isRangeVisible && rangeIndicator != null)
            {
                rangeIndicator.Show(CurrentStats.range);
            }
        }

        public void ApplyFreeze(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            freezeTimer = Mathf.Max(freezeTimer, duration);
            if (frozenVisual != null) frozenVisual.SetActive(true);
            AudioManager.Instance?.PlayFreeze();
            JuiceManager.Instance?.PlayFreeze(transform.position + Vector3.up * 0.8f);
            OnFrozen?.Invoke(duration);
        }

        private void ApplyLevelVisual()
        {
            float scale = CurrentStats.visualScale > 0f ? CurrentStats.visualScale : 1f;
            transform.localScale = Vector3.one * scale;

            if (levelVisuals == null || levelVisuals.Length == 0)
            {
                return;
            }

            for (int i = 0; i < levelVisuals.Length; i++)
            {
                if (levelVisuals[i] != null)
                {
                    levelVisuals[i].SetActive(i == LevelIndex);
                }
            }
        }

        private void Update()
        {
            if (Data == null || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (freezeTimer > 0f)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f && frozenVisual != null)
                {
                    frozenVisual.SetActive(false);
                }
                return;
            }

            fireCooldown -= Time.deltaTime;
            EnemyBase target = FindFirstTarget();
            if (target == null)
            {
                return;
            }

            RotateToward(target.transform.position);

            if (fireCooldown <= 0f)
            {
                FireAt(target);
                fireCooldown = 1f / Mathf.Max(0.01f, CurrentStats.fireRate);
            }
        }

        private EnemyBase FindFirstTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, CurrentStats.range, enemyLayer);
            EnemyBase firstEnemy = null;
            float bestDistance = float.MinValue;

            for (int i = 0; i < hits.Length; i++)
            {
                EnemyBase enemy = hits[i].GetComponentInParent<EnemyBase>();
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                if (enemy.DistanceTraveled > bestDistance)
                {
                    bestDistance = enemy.DistanceTraveled;
                    firstEnemy = enemy;
                }
            }

            return firstEnemy;
        }

        private void RotateToward(Vector3 worldPos)
        {
            if (turretHead == null)
            {
                return;
            }

            Vector3 dir = worldPos - turretHead.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                turretHead.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void FireAt(EnemyBase target)
        {
            if (Data.projectilePrefab == null)
            {
                return;
            }

            Transform spawn = firePoint != null ? firePoint : transform;
            Projectile proj = Instantiate(Data.projectilePrefab, spawn.position, spawn.rotation);
            proj.Init(target, CurrentStats);
            AudioManager.Instance?.PlayShoot(Data.towerName);
            OnFired?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (Data == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
        }
    }
}
