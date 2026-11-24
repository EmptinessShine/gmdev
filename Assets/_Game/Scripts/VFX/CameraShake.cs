using UnityEngine;
using TD.Core;

namespace TD.VFX
{
    public class CameraShake : MonoBehaviour
    {
        private Vector3 baseLocalPosition;
        private float remainingTime;
        private float totalDuration;
        private float currentIntensity;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
        }

        public void Shake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f) return;
            currentIntensity = Mathf.Max(currentIntensity, intensity);
            totalDuration = Mathf.Max(duration, 0.01f);
            remainingTime = Mathf.Max(remainingTime, duration);
        }

        public void StopShake()
        {
            remainingTime = 0f;
            currentIntensity = 0f;
            transform.localPosition = baseLocalPosition;
        }

        private void LateUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
            {
                transform.localPosition = baseLocalPosition;
                return;
            }

            if (remainingTime <= 0f)
            {
                transform.localPosition = baseLocalPosition;
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;
            if (remainingTime <= 0f)
            {
                StopShake();
                return;
            }

            float damper = Mathf.Clamp01(remainingTime / totalDuration);
            Vector2 offset = Random.insideUnitCircle * (currentIntensity * damper);
            transform.localPosition = baseLocalPosition + new Vector3(offset.x, offset.y, 0f);
        }

        private void OnDisable()
        {
            transform.localPosition = baseLocalPosition;
        }
    }
}
