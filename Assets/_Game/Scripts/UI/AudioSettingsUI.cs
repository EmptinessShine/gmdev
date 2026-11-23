using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TD.Audio;
using TD.Save;

namespace TD.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text masterValueText;
        [SerializeField] private TMP_Text musicValueText;
        [SerializeField] private TMP_Text sfxValueText;

        private void Awake()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        private void OnEnable()
        {
            RefreshSliders();
        }

        public void RefreshSliders()
        {
            float master = 1f;
            float music = 0.8f;
            float sfx = 1f;

            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                master = SaveManager.Instance.Data.masterVolume;
                music = SaveManager.Instance.Data.musicVolume;
                sfx = SaveManager.Instance.Data.sfxVolume;
            }

            if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);

            UpdateLabel(masterValueText, master);
            UpdateLabel(musicValueText, music);
            UpdateLabel(sfxValueText, sfx);
        }

        private void OnMasterChanged(float val)
        {
            AudioManager.Instance?.SetMasterVolume(val);
            UpdateLabel(masterValueText, val);
        }

        private void OnMusicChanged(float val)
        {
            AudioManager.Instance?.SetMusicVolume(val);
            UpdateLabel(musicValueText, val);
        }

        private void OnSfxChanged(float val)
        {
            AudioManager.Instance?.SetSfxVolume(val);
            UpdateLabel(sfxValueText, val);
        }

        private static void UpdateLabel(TMP_Text label, float val)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(val * 100f)}%";
            }
        }
    }
}
