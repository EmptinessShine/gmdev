using System;
using UnityEngine;

namespace TD.Save
{
    [Serializable]
    public class SaveData
    {
        public int unlockedLevelCount = 1;
        public int[] bestBaseHp;
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public bool juiceEnabled = true;

        public static SaveData CreateDefault(int levelCount)
        {
            return new SaveData
            {
                unlockedLevelCount = 1,
                bestBaseHp = new int[Mathf.Max(1, levelCount)],
                masterVolume = 1f,
                musicVolume = 0.8f,
                sfxVolume = 1f,
                juiceEnabled = true
            };
        }
    }
}
