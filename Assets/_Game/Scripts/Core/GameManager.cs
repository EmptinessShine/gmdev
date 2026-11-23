using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TD.Data;

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

        [SerializeField] private GameConfig config;

        public GameConfig Config => config;
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

            Gold = config != null ? config.startingGold : 0;
            BaseHp = config != null ? config.startingBaseHp : 1;
            State = GameState.Playing;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
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
                SetState(GameState.Won);
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            OnStateChanged?.Invoke(State);
        }
    }
}
