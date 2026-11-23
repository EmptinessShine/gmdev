using System;
using UnityEngine;
using TD.Enemies;

namespace TD.Data
{
    [Serializable]
    public struct WaveEntry
    {
        public EnemyBase enemyPrefab;
        public int count;
        public float spawnInterval;
    }

    [Serializable]
    public struct WaveDefinition
    {
        public string waveName;
        public WaveEntry[] entries;
        public float delayBeforeNextWave;
    }

    [CreateAssetMenu(fileName = "WaveSetData", menuName = "TD/Wave Set Data")]
    public class WaveSetData : ScriptableObject
    {
        public WaveDefinition[] waves;

        public int GetTotalEnemyCount()
        {
            if (waves == null)
            {
                return 0;
            }

            int total = 0;
            for (int w = 0; w < waves.Length; w++)
            {
                WaveEntry[] entries = waves[w].entries;
                if (entries == null)
                {
                    continue;
                }

                for (int e = 0; e < entries.Length; e++)
                {
                    total += Mathf.Max(0, entries[e].count);
                }
            }

            return total;
        }
    }
}
