using UnityEngine;
using TD.Core;
using TD.Data;

namespace TD.Enemies
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveSetData waveSet;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform basePoint;

        private int waveIndex;
        private int entryIndex;
        private int spawnedInEntry;
        private int aliveInWave;
        private float spawnTimer;
        private float waveDelayTimer;
        private bool waitingNextWave;

        private void Start()
        {
            if (GameManager.Instance == null || waveSet == null || waveSet.waves == null || waveSet.waves.Length == 0)
            {
                return;
            }

            GameManager.Instance.RegisterTotalEnemies(waveSet.GetTotalEnemyCount());
            GameManager.Instance.SetWaveProgress(1, waveSet.waves.Length);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (waveSet == null || waveSet.waves == null || waveIndex >= waveSet.waves.Length)
            {
                return;
            }

            if (waitingNextWave)
            {
                waveDelayTimer -= Time.deltaTime;
                if (waveDelayTimer <= 0f)
                {
                    waitingNextWave = false;
                    waveIndex++;
                    entryIndex = 0;
                    spawnedInEntry = 0;
                    spawnTimer = 0f;
                    GameManager.Instance.SetWaveProgress(waveIndex + 1, waveSet.waves.Length);
                }
                return;
            }

            WaveDefinition wave = waveSet.waves[waveIndex];
            if (wave.entries != null && entryIndex < wave.entries.Length)
            {
                WaveEntry entry = wave.entries[entryIndex];
                if (spawnedInEntry < entry.count)
                {
                    spawnTimer -= Time.deltaTime;
                    if (spawnTimer <= 0f)
                    {
                        SpawnEnemy(entry.enemyPrefab);
                        spawnedInEntry++;
                        spawnTimer = entry.spawnInterval;
                    }
                    return;
                }

                entryIndex++;
                spawnedInEntry = 0;
                spawnTimer = 0f;
                return;
            }

            if (aliveInWave == 0 && waveIndex + 1 < waveSet.waves.Length)
            {
                waitingNextWave = true;
                waveDelayTimer = wave.delayBeforeNextWave;
            }
        }

        private void SpawnEnemy(EnemyBase prefab)
        {
            if (prefab == null || spawnPoint == null) return;

            EnemyBase enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            aliveInWave++;
            enemy.OnDied += HandleEnemyDied;
            enemy.Initialize(basePoint);
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            enemy.OnDied -= HandleEnemyDied;
            aliveInWave = Mathf.Max(0, aliveInWave - 1);
        }
    }
}
