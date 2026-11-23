using System;
using System.IO;
using UnityEngine;
using TD.Data;

namespace TD.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] private LevelData[] levels;

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public SaveData Data { get; private set; }
        public int SelectedLevelIndex { get; set; }
        public LevelData[] Levels => levels;
        public int LevelCount => levels != null && levels.Length > 0 ? levels.Length : 3;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            int count = LevelCount;
            if (!File.Exists(SaveFilePath))
            {
                Data = SaveData.CreateDefault(count);
                Save();
                return;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded == null)
                {
                    throw new Exception("Parsed SaveData is null.");
                }

                Data = loaded;
                SanitizeData(count);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load save.json ({e.Message}). Resetting to defaults.");
                Data = SaveData.CreateDefault(count);
                Save();
            }
        }

        public void Save()
        {
            if (Data == null) return;

            try
            {
                string dir = Path.GetDirectoryName(SaveFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to save save.json: {e.Message}");
            }
        }

        private void SanitizeData(int count)
        {
            Data.unlockedLevelCount = Mathf.Clamp(Data.unlockedLevelCount, 1, count);
            if (Data.bestBaseHp == null || Data.bestBaseHp.Length != count)
            {
                int[] resized = new int[count];
                if (Data.bestBaseHp != null)
                {
                    int copyLen = Mathf.Min(Data.bestBaseHp.Length, count);
                    Array.Copy(Data.bestBaseHp, resized, copyLen);
                }
                Data.bestBaseHp = resized;
            }

            Data.masterVolume = Mathf.Clamp(Data.masterVolume, 0.0001f, 1f);
            Data.musicVolume = Mathf.Clamp(Data.musicVolume, 0.0001f, 1f);
            Data.sfxVolume = Mathf.Clamp(Data.sfxVolume, 0.0001f, 1f);
        }

        public LevelData GetSelectedLevel()
        {
            if (levels == null || levels.Length == 0) return null;
            int idx = Mathf.Clamp(SelectedLevelIndex, 0, levels.Length - 1);
            return levels[idx];
        }

        public bool IsLevelUnlocked(int index)
        {
            return Data != null && index >= 0 && index < Data.unlockedLevelCount;
        }

        public int GetBestBaseHp(int index)
        {
            if (Data == null || Data.bestBaseHp == null || index < 0 || index >= Data.bestBaseHp.Length)
            {
                return 0;
            }

            return Data.bestBaseHp[index];
        }

        public void RecordLevelWin(int levelIndex, int remainingBaseHp)
        {
            if (Data == null) return;

            SanitizeData(LevelCount);
            if (levelIndex >= 0 && levelIndex < Data.bestBaseHp.Length)
            {
                Data.bestBaseHp[levelIndex] = Mathf.Max(Data.bestBaseHp[levelIndex], remainingBaseHp);
            }

            int nextUnlocked = Mathf.Clamp(levelIndex + 2, 1, LevelCount);
            Data.unlockedLevelCount = Mathf.Max(Data.unlockedLevelCount, nextUnlocked);
            Save();
        }

        public void UpdateAudioSettings(float master, float music, float sfx)
        {
            if (Data == null) return;
            Data.masterVolume = Mathf.Clamp(master, 0.0001f, 1f);
            Data.musicVolume = Mathf.Clamp(music, 0.0001f, 1f);
            Data.sfxVolume = Mathf.Clamp(sfx, 0.0001f, 1f);
            Save();
        }

        public void SetJuiceEnabled(bool enabled)
        {
            if (Data == null) return;
            Data.juiceEnabled = enabled;
            Save();
        }

        public void ResetProgress()
        {
            if (Data == null)
            {
                Data = SaveData.CreateDefault(LevelCount);
            }
            else
            {
                Data.unlockedLevelCount = 1;
                Data.bestBaseHp = new int[LevelCount];
            }

            SelectedLevelIndex = 0;
            Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
