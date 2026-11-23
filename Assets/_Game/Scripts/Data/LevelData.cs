using System;
using UnityEngine;

namespace TD.Data
{
    [Serializable]
    public struct WallBlock
    {
        public Vector3 position;
        public Vector3 scale;
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "TD/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex;
        public string levelName;
        public int startingGold = 110;
        public int startingBaseHp = 20;
        public Color groundColor = new Color(0.18f, 0.22f, 0.20f);
        public Vector3 spawnPosition;
        public Vector3 basePosition;
        public WallBlock[] interiorWalls;
        public Vector3[] slotPositions;
        public WaveSetData waveSet;
    }
}
