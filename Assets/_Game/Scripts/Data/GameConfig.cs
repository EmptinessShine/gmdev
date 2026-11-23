using UnityEngine;

namespace TD.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TD/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Economy & Base")]
        public int startingGold = 150;
        public int startingBaseHp = 20;
        [Range(0f, 1f)] public float sellRefundPercent = 0.7f;

        [Header("MVP Test Wave")]
        public int testEnemyCount = 10;
        public float testSpawnInterval = 1.5f;
    }
}
