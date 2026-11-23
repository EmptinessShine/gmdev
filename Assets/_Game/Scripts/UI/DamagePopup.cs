using TMPro;
using UnityEngine;

namespace TD.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 1.6f;
        [SerializeField] private float lifetime = 0.65f;
        [SerializeField] private Color normalColor = new Color(1f, 0.92f, 0.25f);
        [SerializeField] private Color reducedColor = new Color(0.45f, 0.85f, 1f);

        private TextMeshPro tmp;
        private float elapsed;
        private Color startColor;

        public static void Spawn(DamagePopup prefab, Vector3 worldPos, float damage, bool wasReduced)
        {
            if (prefab == null) return;

            Vector3 offset = new Vector3(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.25f, 0.25f));
            DamagePopup popup = Instantiate(prefab, worldPos + offset, Quaternion.identity);
            popup.Setup(damage, wasReduced);
        }

        private void Awake()
        {
            tmp = GetComponent<TextMeshPro>();
        }

        private void Setup(float damage, bool wasReduced)
        {
            if (tmp == null) tmp = GetComponent<TextMeshPro>();
            tmp.text = Mathf.Max(1, Mathf.RoundToInt(damage)).ToString();
            startColor = wasReduced ? reducedColor : normalColor;
            tmp.color = startColor;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
            }

            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, lifetime));
            Color c = startColor;
            c.a = 1f - t;
            tmp.color = c;

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
