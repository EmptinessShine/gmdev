using System;
using UnityEngine;
using UnityEngine.Rendering;
using TD.Save;

namespace TD.VFX
{
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() => Instance = null;

        [Header("Scene VFX References")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private HitStop hitStop;

        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem hitFxPrefab;
        [SerializeField] private ParticleSystem pickupFxPrefab;
        [SerializeField] private ParticleSystem deathFxPrefab;
        [SerializeField] private ParticleSystem buildFxPrefab;
        [SerializeField] private ParticleSystem freezeFxPrefab;
        [SerializeField] private ParticleSystem winFxPrefab;

        [Header("Shake & Hit Stop Tuning")]
        [SerializeField] private float baseHitShakeIntensity = 0.22f;
        [SerializeField] private float baseHitShakeDuration = 0.20f;
        [SerializeField] private float tankDeathShakeIntensity = 0.14f;
        [SerializeField] private float tankDeathShakeDuration = 0.16f;
        [SerializeField] private float buildShakeIntensity = 0.06f;
        [SerializeField] private float buildShakeDuration = 0.10f;
        [SerializeField] private float hitStopDuration = 0.05f;

        public bool JuiceEnabled { get; private set; } = true;
        public event Action<bool> OnJuiceToggled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            JuiceEnabled = SaveManager.Instance != null && SaveManager.Instance.Data != null
                ? SaveManager.Instance.Data.juiceEnabled
                : true;

            ApplyVolumeState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ToggleJuice()
        {
            JuiceEnabled = !JuiceEnabled;
            SaveManager.Instance?.SetJuiceEnabled(JuiceEnabled);
            ApplyVolumeState();
            if (!JuiceEnabled && cameraShake != null)
            {
                cameraShake.StopShake();
            }
            OnJuiceToggled?.Invoke(JuiceEnabled);
        }

        private void ApplyVolumeState()
        {
            if (postProcessVolume != null)
            {
                postProcessVolume.enabled = JuiceEnabled;
            }
        }

        public void PlayHit(Vector3 pos)
        {
            SpawnFx(hitFxPrefab, pos);
        }

        public void PlayPickup(Vector3 pos)
        {
            SpawnFx(pickupFxPrefab, pos);
        }

        public void PlayDeath(Vector3 pos, bool isHeavy)
        {
            SpawnFx(deathFxPrefab, pos);
            if (!JuiceEnabled || !isHeavy) return;

            cameraShake?.Shake(tankDeathShakeIntensity, tankDeathShakeDuration);
            hitStop?.Trigger(hitStopDuration);
        }

        public void PlayBuildOrUpgrade(Vector3 pos)
        {
            SpawnFx(buildFxPrefab, pos);
            if (!JuiceEnabled) return;
            cameraShake?.Shake(buildShakeIntensity, buildShakeDuration);
        }

        public void PlayFreeze(Vector3 pos)
        {
            SpawnFx(freezeFxPrefab, pos);
        }

        public void PlayBaseDamaged()
        {
            if (!JuiceEnabled) return;
            cameraShake?.Shake(baseHitShakeIntensity, baseHitShakeDuration);
            hitStop?.Trigger(hitStopDuration);
        }

        public void PlayWin(Vector3 pos)
        {
            SpawnFx(winFxPrefab, pos);
        }

        private void SpawnFx(ParticleSystem prefab, Vector3 pos)
        {
            if (!JuiceEnabled || prefab == null) return;

            ParticleSystem fx = Instantiate(prefab, pos, Quaternion.identity);
            ParticleSystem.MainModule main = fx.main;
            Destroy(fx.gameObject, main.duration + main.startLifetime.constantMax + 0.25f);
        }
    }
}
