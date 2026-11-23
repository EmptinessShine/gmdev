using UnityEngine;

namespace TD.Towers
{
    [RequireComponent(typeof(LineRenderer))]
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField] private int segments = 48;
        [SerializeField] private float yHeight = 0.12f;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.enabled = false;
        }

        public void Show(float worldRadius)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            if (worldRadius <= 0f || segments < 3)
            {
                Hide();
                return;
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.positionCount = segments;

            Vector3 center = new Vector3(transform.position.x, yHeight, transform.position.z);
            float step = Mathf.PI * 2f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * step;
                Vector3 pos = center + new Vector3(Mathf.Cos(angle) * worldRadius, 0f, Mathf.Sin(angle) * worldRadius);
                lineRenderer.SetPosition(i, pos);
            }

            lineRenderer.enabled = true;
        }

        public void Hide()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
