using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TD.Audio;
using TD.Data;
using TD.Save;
using TD.VFX;

namespace TD.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() => Instance = null;

        [SerializeField] private GameConfig config;
        [SerializeField] private LevelLoader levelLoader;

        public GameConfig Config => config;
        public LevelData ActiveLevel { get; private set; }
        public int Gold { get; private set; }
        public int BaseHp { get; private set; }
        public int RemainingEnemies { get; private set; }
        public int CurrentWave { get; private set; }
        public int TotalWaves { get; private set; }
        public GameState State { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action<int> OnBaseHpChanged;
        public event Action<int> OnEnemiesChanged;
        public event Action<int, int> OnWaveChanged;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;

            ActiveLevel = levelLoader != null ? levelLoader.LoadActiveLevel() : null;
            Gold = ActiveLevel != null ? ActiveLevel.startingGold : (config != null ? config.startingGold : 0);
            BaseHp = ActiveLevel != null ? ActiveLevel.startingBaseHp : (config != null ? config.startingBaseHp : 1);
            State = GameState.Playing;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterTotalEnemies(int count)
        {
            RemainingEnemies = Mathf.Max(0, count);
            OnEnemiesChanged?.Invoke(RemainingEnemies);
        }

        public void SetWaveProgress(int current, int total)
        {
            CurrentWave = Mathf.Max(0, current);
            TotalWaves = Mathf.Max(0, total);
            OnWaveChanged?.Invoke(CurrentWave, TotalWaves);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public void DamageBase(int damage)
        {
            if (State != GameState.Playing || damage <= 0)
            {
                return;
            }

            BaseHp = Mathf.Max(0, BaseHp - damage);
            OnBaseHpChanged?.Invoke(BaseHp);
            JuiceManager.Instance?.PlayBaseDamaged();

            if (BaseHp <= 0)
            {
                SetState(GameState.Lost);
            }
        }

        public void NotifyEnemyDefeated()
        {
            if (RemainingEnemies <= 0)
            {
                return;
            }

            RemainingEnemies = Mathf.Max(0, RemainingEnemies - 1);
            OnEnemiesChanged?.Invoke(RemainingEnemies);

            if (RemainingEnemies == 0 && State == GameState.Playing && BaseHp > 0)
            {
                if (SaveManager.Instance != null)
                {
                    int idx = ActiveLevel != null ? ActiveLevel.levelIndex : 0;
                    SaveManager.Instance.RecordLevelWin(idx, BaseHp);
                }
                SetState(GameState.Won);
            }
        }

        public bool HasNextLevel =>
            SaveManager.Instance != null &&
            ActiveLevel != null &&
            ActiveLevel.levelIndex + 1 < SaveManager.Instance.LevelCount;

        public void LoadNextLevel()
        {
            if (!HasNextLevel) return;
            SaveManager.Instance.SelectedLevelIndex = ActiveLevel.levelIndex + 1;
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
            SceneManager.LoadScene("MainMenu");
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
                AudioManager.Instance?.SetPaused(true);
            }
            else if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
                AudioManager.Instance?.SetPaused(false);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            if (State == GameState.Won)
            {
                AudioManager.Instance?.PlayWin();
                Vector3 winPos = levelLoader != null ? levelLoader.BasePosition : Vector3.zero;
                JuiceManager.Instance?.PlayWin(winPos);
            }
            else if (State == GameState.Lost) AudioManager.Instance?.PlayLose();
            OnStateChanged?.Invoke(State);
        }
    }
}
