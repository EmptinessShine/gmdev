using UnityEngine;

namespace TD.VFX
{
    public class HitFlash : MonoBehaviour
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private float flashDuration = 0.10f;

        private MaterialPropertyBlock propBlock;
        private float timer;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }
            propBlock = new MaterialPropertyBlock();
        }

        public void TriggerFlash()
        {
            if (targetRenderer == null) return;
            if (JuiceManager.Instance != null && !JuiceManager.Instance.JuiceEnabled) return;

            timer = flashDuration;
            ApplyFlash(1f);
        }

        private void Update()
        {
            if (timer <= 0f) return;

            timer -= Time.deltaTime;
            float amount = timer > 0f && flashDuration > 0f ? Mathf.Clamp01(timer / flashDuration) : 0f;
            ApplyFlash(amount);
        }

        private void OnDisable()
        {
            timer = 0f;
            ApplyFlash(0f);
        }

        private void ApplyFlash(float amount)
        {
            if (targetRenderer == null || propBlock == null) return;
            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(FlashAmountId, amount);
            targetRenderer.SetPropertyBlock(propBlock);
        }
    }
}
