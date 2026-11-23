using UnityEngine;

namespace TD.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public float maxHp = 40f;
        public float moveSpeed = 3f;
        public int goldReward = 20;
        public int baseDamage = 2;
        public float attackRange = 1.5f;
        public float attackCooldown = 1.2f;
    }
}
