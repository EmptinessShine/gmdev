using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TD.Audio;
using TD.Data;
using TD.Save;

namespace TD.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Level Select")]
        [SerializeField] private Button[] levelButtons;
        [SerializeField] private TMP_Text[] levelButtonLabels;
        [SerializeField] private TMP_Text saveInfoText;

        [Header("Reset Progress")]
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private GameObject confirmResetPanel;
        [SerializeField] private Button confirmResetYesButton;
        [SerializeField] private Button confirmResetCancelButton;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeSettingsButton;

        private void Start()
        {
            Time.timeScale = 1f;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int index = i;
                if (levelButtons[i] != null)
                {
                    levelButtons[i].onClick.AddListener(() => StartLevel(index));
                }
            }

            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(OpenResetConfirm);
            if (confirmResetYesButton != null) confirmResetYesButton.onClick.AddListener(ConfirmReset);
            if (confirmResetCancelButton != null) confirmResetCancelButton.onClick.AddListener(CloseResetConfirm);

            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);

            if (confirmResetPanel != null) confirmResetPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            RefreshLevelButtons();
        }

        private void RefreshLevelButtons()
        {
            SaveManager sm = SaveManager.Instance;
            LevelData[] levels = sm != null ? sm.Levels : null;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                bool hasLevel = levels != null && i < levels.Length && levels[i] != null;
                if (levelButtons[i] != null) levelButtons[i].gameObject.SetActive(hasLevel);
                if (!hasLevel) continue;

                bool unlocked = sm.IsLevelUnlocked(i);
                int bestHp = sm.GetBestBaseHp(i);
                levelButtons[i].interactable = unlocked;

                if (i < levelButtonLabels.Length && levelButtonLabels[i] != null)
                {
                    string status = !unlocked
                        ? "LOCKED"
                        : (bestHp > 0 ? $"Best HP: {bestHp}" : "Unlocked");
                    levelButtonLabels[i].text = $"{levels[i].levelName}\n{status}";
                }
            }

            if (saveInfoText != null && sm != null)
            {
                saveInfoText.text = $"Unlocked Levels: {sm.Data.unlockedLevelCount}/{sm.LevelCount}";
            }
        }

        private void StartLevel(int index)
        {
            AudioManager.Instance?.PlayUIClick();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SelectedLevelIndex = index;
            }
            SceneManager.LoadScene("Gameplay");
        }

        private void OpenResetConfirm()
        {
            AudioManager.Instance?.PlayUIClick();
            if (confirmResetPanel != null) confirmResetPanel.SetActive(true);
        }

        private void CloseResetConfirm()
        {
            AudioManager.Instance?.PlayUIClick();
            if (confirmResetPanel != null) confirmResetPanel.SetActive(false);
        }

        private void ConfirmReset()
        {
            AudioManager.Instance?.PlayUIClick();
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ResetProgress();
            }
            CloseResetConfirm();
            RefreshLevelButtons();
        }

        private void OpenSettings()
        {
            AudioManager.Instance?.PlayUIClick();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            AudioManager.Instance?.PlayUIClick();
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
    }
}
