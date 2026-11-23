using UnityEngine;

namespace TD.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Base Stats")]
        public string enemyName;
        public float maxHp = 40f;
        public float moveSpeed = 3f;
        public int goldReward = 20;
        public int baseDamage = 2;
        public float attackRange = 1.5f;
        public float attackCooldown = 1.2f;

        [Header("A10 Wind-Up & Abilities")]
        public float attackWindUpDuration = 0.5f;
        public float abilityWindUpDuration = 0.5f;
        public float abilityCooldown = 5f;

        [Header("Tank Shield")]
        public float shieldDuration = 3f;
        [Range(0f, 0.95f)] public float shieldDamageReduction = 0.7f;

        [Header("Frost Mage Sight & Freeze")]
        public float detectRange = 6.5f;
        [Range(10f, 180f)] public float detectHalfAngle = 60f;
        public float freezeDuration = 0.8f;
    }
}
