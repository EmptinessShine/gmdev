using UnityEngine;
using TD.Core;

namespace TD.Enemies
{
    public class TestWaveSpawner : MonoBehaviour
    {
        [SerializeField] private EnemyBase enemyPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform basePoint;

        private int totalToSpawn;
        private int spawnedCount;
        private float spawnTimer;

        private void Start()
        {
            if (GameManager.Instance == null || GameManager.Instance.Config == null)
            {
                return;
            }

            totalToSpawn = GameManager.Instance.Config.testEnemyCount;
            GameManager.Instance.RegisterTotalEnemies(totalToSpawn);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (spawnedCount >= totalToSpawn || enemyPrefab == null || spawnPoint == null)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                EnemyBase enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemy.Initialize(basePoint);
                spawnedCount++;
                spawnTimer = GameManager.Instance.Config.testSpawnInterval;
            }
        }
    }
}
