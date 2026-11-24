using UnityEngine;
using TD.Core;

namespace TD.VFX
{
    public class HitStop : MonoBehaviour
    {
        private bool isActive;
        private float remainingRealtime;

        public void Trigger(float duration)
        {
            if (duration <= 0f) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            isActive = true;
            remainingRealtime = Mathf.Max(remainingRealtime, duration);
            Time.timeScale = 0.05f;
        }

        private void Update()
        {
            if (!isActive) return;

            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
            {
                isActive = false;
                remainingRealtime = 0f;
                return;
            }

            remainingRealtime -= Time.unscaledDeltaTime;
            if (remainingRealtime <= 0f)
            {
                isActive = false;
                remainingRealtime = 0f;
                Time.timeScale = 1f;
            }
        }

        private void OnDisable()
        {
            RestoreIfActive();
        }

        private void OnDestroy()
        {
            RestoreIfActive();
        }

        private void RestoreIfActive()
        {
            if (!isActive) return;
            isActive = false;
            remainingRealtime = 0f;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Paused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
