using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TD.Core;

namespace TD.UI
{
    public class HudUI : MonoBehaviour
    {
        [Header("HUD Text")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text baseHpText;
        [SerializeField] private TMP_Text enemiesText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text levelText;

        [Header("Pause")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMenuButton;

        [Header("End Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button winRestartButton;
        [SerializeField] private Button winMenuButton;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button loseRestartButton;
        [SerializeField] private Button loseMenuButton;

        private void Start()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                return;
            }

            gm.OnGoldChanged += UpdateGold;
            gm.OnBaseHpChanged += UpdateBaseHp;
            gm.OnEnemiesChanged += UpdateEnemies;
            gm.OnWaveChanged += UpdateWave;
            gm.OnStateChanged += UpdateStatePanels;

            if (pauseButton != null) pauseButton.onClick.AddListener(gm.TogglePause);
            if (resumeButton != null) resumeButton.onClick.AddListener(gm.TogglePause);
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(gm.RestartLevel);
            if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(gm.ReturnToMenu);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(gm.LoadNextLevel);
            if (winRestartButton != null) winRestartButton.onClick.AddListener(gm.RestartLevel);
            if (winMenuButton != null) winMenuButton.onClick.AddListener(gm.ReturnToMenu);
            if (loseRestartButton != null) loseRestartButton.onClick.AddListener(gm.RestartLevel);
            if (loseMenuButton != null) loseMenuButton.onClick.AddListener(gm.ReturnToMenu);

            if (levelText != null && gm.ActiveLevel != null)
            {
                levelText.text = gm.ActiveLevel.levelName;
            }

            UpdateGold(gm.Gold);
            UpdateBaseHp(gm.BaseHp);
            UpdateEnemies(gm.RemainingEnemies);
            UpdateWave(gm.CurrentWave, gm.TotalWaves);
            UpdateStatePanels(gm.State);
        }

        private void OnDestroy()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                return;
            }

            gm.OnGoldChanged -= UpdateGold;
            gm.OnBaseHpChanged -= UpdateBaseHp;
            gm.OnEnemiesChanged -= UpdateEnemies;
            gm.OnWaveChanged -= UpdateWave;
            gm.OnStateChanged -= UpdateStatePanels;
        }

        private void UpdateGold(int gold)
        {
            if (goldText != null) goldText.text = $"Gold: {gold}";
        }

        private void UpdateBaseHp(int hp)
        {
            if (baseHpText != null) baseHpText.text = $"Base HP: {hp}";
        }

        private void UpdateEnemies(int remaining)
        {
            if (enemiesText != null) enemiesText.text = $"Enemies: {remaining}";
        }

        private void UpdateWave(int current, int total)
        {
            if (waveText != null)
            {
                waveText.text = total > 0 ? $"Wave: {current}/{total}" : string.Empty;
            }
        }

        private void UpdateStatePanels(GameState state)
        {
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
            if (winPanel != null) winPanel.SetActive(state == GameState.Won);
            if (losePanel != null) losePanel.SetActive(state == GameState.Lost);

            if (nextLevelButton != null && GameManager.Instance != null)
            {
                nextLevelButton.gameObject.SetActive(state == GameState.Won && GameManager.Instance.HasNextLevel);
            }
        }
    }
}
