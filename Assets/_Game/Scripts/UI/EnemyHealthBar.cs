using UnityEngine;
using UnityEngine.UI;
using TD.Enemies;

namespace TD.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private EnemyBase enemy;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color highColor = new Color(0.25f, 0.85f, 0.35f);
        [SerializeField] private Color midColor = new Color(0.95f, 0.80f, 0.20f);
        [SerializeField] private Color lowColor = new Color(0.92f, 0.25f, 0.22f);

        private void OnEnable()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (fillImage == null) return;

            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fillImage.fillAmount = ratio;

            if (ratio > 0.5f)
            {
                fillImage.color = highColor;
            }
            else if (ratio > 0.25f)
            {
                fillImage.color = midColor;
            }
            else
            {
                fillImage.color = lowColor;
            }
        }
    }
}
