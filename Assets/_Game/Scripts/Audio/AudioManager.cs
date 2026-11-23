using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TD.Save;

namespace TD.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() => Instance = null;

        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerSnapshot normalSnapshot;
        [SerializeField] private AudioMixerSnapshot pausedSnapshot;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private float crossfadeDuration = 0.8f;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip sfxBuild;
        [SerializeField] private AudioClip sfxUpgrade;
        [SerializeField] private AudioClip sfxSell;
        [SerializeField] private AudioClip sfxShootGunner;
        [SerializeField] private AudioClip sfxShootSniper;
        [SerializeField] private AudioClip sfxShootSlow;
        [SerializeField] private AudioClip sfxHit;
        [SerializeField] private AudioClip sfxPickup;
        [SerializeField] private AudioClip sfxWindUp;
        [SerializeField] private AudioClip sfxDeath;
        [SerializeField] private AudioClip sfxFreeze;
        [SerializeField] private AudioClip sfxUIClick;
        [SerializeField] private AudioClip sfxWin;
        [SerializeField] private AudioClip sfxLose;

        [Header("SFX Settings")]
        [SerializeField] private int sfxPoolSize = 6;
        [SerializeField] private float minPitch = 0.92f;
        [SerializeField] private float maxPitch = 1.08f;

        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private AudioSource[] sfxPool;
        private int sfxIndex;

        private bool isCrossfading;
        private float crossfadeTimer;
        private AudioSource fadeInSource;
        private AudioSource fadeOutSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSourceA = CreateMusicSource("MusicSource_A");
            musicSourceB = CreateMusicSource("MusicSource_B");
            activeMusicSource = musicSourceA;

            int count = Mathf.Max(2, sfxPoolSize);
            sfxPool = new AudioSource[count];
            for (int i = 0; i < count; i++)
            {
                GameObject child = new GameObject($"SFX_Pool_{i}");
                child.transform.SetParent(transform, false);
                AudioSource src = child.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.outputAudioMixerGroup = sfxGroup;
                sfxPool[i] = src;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            ApplySavedVolumes();
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        private void Update()
        {
            if (!isCrossfading) return;

            crossfadeTimer += Time.unscaledDeltaTime;
            float t = crossfadeDuration > 0.01f ? Mathf.Clamp01(crossfadeTimer / crossfadeDuration) : 1f;

            if (fadeInSource != null) fadeInSource.volume = t;
            if (fadeOutSource != null) fadeOutSource.volume = 1f - t;

            if (t >= 1f)
            {
                if (fadeOutSource != null)
                {
                    fadeOutSource.Stop();
                    fadeOutSource.volume = 0f;
                }
                isCrossfading = false;
            }
        }

        private AudioSource CreateMusicSource(string objName)
        {
            GameObject child = new GameObject(objName);
            child.transform.SetParent(transform, false);
            AudioSource src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f;
            src.outputAudioMixerGroup = musicGroup;
            return src;
        }

        public static float LinearToDecibels(float linear)
        {
            return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        }

        public void ApplySavedVolumes()
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

            if (mainMixer != null)
            {
                mainMixer.SetFloat(MasterParam, LinearToDecibels(master));
                mainMixer.SetFloat(MusicParam, LinearToDecibels(music));
                mainMixer.SetFloat(SfxParam, LinearToDecibels(sfx));
            }
        }

        public void SetMasterVolume(float linear)
        {
            if (mainMixer != null) mainMixer.SetFloat(MasterParam, LinearToDecibels(linear));
            SaveVolumes(master: linear);
        }

        public void SetMusicVolume(float linear)
        {
            if (mainMixer != null) mainMixer.SetFloat(MusicParam, LinearToDecibels(linear));
            SaveVolumes(music: linear);
        }

        public void SetSfxVolume(float linear)
        {
            if (mainMixer != null) mainMixer.SetFloat(SfxParam, LinearToDecibels(linear));
            SaveVolumes(sfx: linear);
        }

        private static void SaveVolumes(float? master = null, float? music = null, float? sfx = null)
        {
            if (SaveManager.Instance == null || SaveManager.Instance.Data == null) return;
            SaveData d = SaveManager.Instance.Data;
            SaveManager.Instance.UpdateAudioSettings(
                master ?? d.masterVolume,
                music ?? d.musicVolume,
                sfx ?? d.sfxVolume);
        }

        public void SetPaused(bool paused)
        {
            if (paused && pausedSnapshot != null)
            {
                pausedSnapshot.TransitionTo(0.15f);
            }
            else if (!paused && normalSnapshot != null)
            {
                normalSnapshot.TransitionTo(0.15f);
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetPaused(false);
            AudioClip target = scene.name == "Gameplay" ? battleMusic : menuMusic;
            PlayMusic(target);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            if (activeMusicSource != null && activeMusicSource.clip == clip && activeMusicSource.isPlaying)
            {
                return;
            }

            if (activeMusicSource == null || !activeMusicSource.isPlaying)
            {
                activeMusicSource = musicSourceA;
                activeMusicSource.clip = clip;
                activeMusicSource.volume = 1f;
                activeMusicSource.Play();
                isCrossfading = false;
                return;
            }

            AudioSource nextSource = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
            nextSource.clip = clip;
            nextSource.volume = 0f;
            nextSource.Play();

            fadeOutSource = activeMusicSource;
            fadeInSource = nextSource;
            activeMusicSource = nextSource;
            crossfadeTimer = 0f;
            isCrossfading = true;
        }

        public void PlaySfx(AudioClip clip, bool varyPitch = true, float volume = 1f)
        {
            if (clip == null || sfxPool == null || sfxPool.Length == 0) return;

            AudioSource src = sfxPool[sfxIndex];
            sfxIndex = (sfxIndex + 1) % sfxPool.Length;
            if (src == null) return;
            src.pitch = varyPitch ? Random.Range(minPitch, maxPitch) : 1f;
            src.PlayOneShot(clip, volume);
        }

        public void PlayBuild() => PlaySfx(sfxBuild, true, 0.85f);
        public void PlayUpgrade() => PlaySfx(sfxUpgrade, false, 0.90f);
        public void PlaySell() => PlaySfx(sfxSell, true, 0.85f);
        public void PlayHit() => PlaySfx(sfxHit, true, 0.55f);
        public void PlayPickup() => PlaySfx(sfxPickup, true, 0.70f);
        public void PlayWindUp() => PlaySfx(sfxWindUp, true, 0.70f);
        public void PlayDeath() => PlaySfx(sfxDeath, true, 0.80f);
        public void PlayFreeze() => PlaySfx(sfxFreeze, true, 0.85f);
        public void PlayUIClick() => PlaySfx(sfxUIClick, true, 0.75f);
        public void PlayWin() => PlaySfx(sfxWin, false, 0.95f);
        public void PlayLose() => PlaySfx(sfxLose, false, 0.95f);

        public void PlayShoot(string towerName)
        {
            if (string.IsNullOrEmpty(towerName))
            {
                PlaySfx(sfxShootGunner, true, 0.60f);
                return;
            }

            if (towerName.Contains("Sniper"))
            {
                PlaySfx(sfxShootSniper, true, 0.75f);
            }
            else if (towerName.Contains("Slow"))
            {
                PlaySfx(sfxShootSlow, true, 0.65f);
            }
            else
            {
                PlaySfx(sfxShootGunner, true, 0.55f);
            }
        }
    }
}
