using UnityEngine;
using TD.Core;
using TD.Data;
using TD.Enemies;

namespace TD.Towers
{
    public class Projectile : MonoBehaviour
    {
        private EnemyBase target;
        private Vector3 lastTargetPos;
        private float damage;
        private float speed;
        private float slowMultiplier;
        private float slowDuration;

        public void Init(EnemyBase targetEnemy, TowerLevelStats stats)
        {
            target = targetEnemy;
            damage = stats.damage;
            speed = Mathf.Max(1f, stats.projectileSpeed);
            slowMultiplier = stats.slowMultiplier;
            slowDuration = stats.slowDuration;

            if (target != null)
            {
                lastTargetPos = target.transform.position;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (target != null && !target.IsDead)
            {
                lastTargetPos = target.transform.position;
            }

            Vector3 toTarget = lastTargetPos - transform.position;
            float step = speed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= Mathf.Max(0.04f, step * step))
            {
                HitTarget();
                return;
            }

            transform.position += toTarget.normalized * step;
        }

        private void HitTarget()
        {
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage);
                if (slowDuration > 0f && slowMultiplier < 1f)
                {
                    target.ApplySlow(slowMultiplier, slowDuration);
                }
            }

            Destroy(gameObject);
        }
    }
}
