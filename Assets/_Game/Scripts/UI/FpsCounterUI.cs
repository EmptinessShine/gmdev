using TMPro;
using UnityEngine;

namespace TD.UI
{
    public class FpsCounterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private float updateInterval = 0.25f;

        private int frameCount;
        private float elapsed;

        private void Update()
        {
            frameCount++;
            elapsed += Time.unscaledDeltaTime;

            if (elapsed < updateInterval) return;

            int fps = Mathf.RoundToInt(frameCount / elapsed);
            frameCount = 0;
            elapsed = 0f;

            if (fpsText == null) return;

            fpsText.text = $"FPS: {fps}";
            if (fps >= 45)
            {
                fpsText.color = new Color(0.45f, 0.95f, 0.50f);
            }
            else if (fps >= 30)
            {
                fpsText.color = new Color(0.98f, 0.85f, 0.30f);
            }
            else
            {
                fpsText.color = new Color(0.95f, 0.35f, 0.32f);
            }
        }
    }
}
