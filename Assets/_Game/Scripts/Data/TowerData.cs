using System;
using UnityEngine;
using TD.Towers;

namespace TD.Data
{
    [Serializable]
    public struct TowerLevelStats
    {
        public int cost;
        public float damage;
        public float fireRate;
        public float range;
        public float projectileSpeed;
        [Range(0.1f, 1f)] public float slowMultiplier;
        public float slowDuration;
        public float visualScale;
    }

    [CreateAssetMenu(fileName = "TowerData", menuName = "TD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        public string towerName;
        public Tower prefab;
        public Projectile projectilePrefab;
        public TowerLevelStats[] levels;

        public int MaxLevel => levels != null ? levels.Length : 0;

        public TowerLevelStats GetLevel(int index)
        {
            if (levels == null || levels.Length == 0)
            {
                return default;
            }

            int clamped = Mathf.Clamp(index, 0, levels.Length - 1);
            return levels[clamped];
        }
    }
}
