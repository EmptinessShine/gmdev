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

        [Header("Pause")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;

        [Header("End Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button winRestartButton;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button loseRestartButton;

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
            gm.OnStateChanged += UpdateStatePanels;

            if (pauseButton != null) pauseButton.onClick.AddListener(gm.TogglePause);
            if (resumeButton != null) resumeButton.onClick.AddListener(gm.TogglePause);
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(gm.RestartLevel);
            if (winRestartButton != null) winRestartButton.onClick.AddListener(gm.RestartLevel);
            if (loseRestartButton != null) loseRestartButton.onClick.AddListener(gm.RestartLevel);

            UpdateGold(gm.Gold);
            UpdateBaseHp(gm.BaseHp);
            UpdateEnemies(gm.RemainingEnemies);
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

        private void UpdateStatePanels(GameState state)
        {
            if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
            if (winPanel != null) winPanel.SetActive(state == GameState.Won);
            if (losePanel != null) losePanel.SetActive(state == GameState.Lost);
        }
    }
}
