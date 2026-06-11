using UnityEngine;

namespace GaokaoSimulator.UI.Effects
{
    [DisallowMultipleComponent]
    public class UiFloatBob : MonoBehaviour
    {
        [SerializeField] private float amplitude = 10f;
        [SerializeField] private float frequency = 0.6f;
        [SerializeField] private float phase = 0f;
        [SerializeField] private bool useUnscaledTime = true;

        private RectTransform rect;
        private Vector2 baseAnchoredPos;

        public void Configure(float amplitudeValue, float frequencyValue, float phaseValue = 0f)
        {
            amplitude = amplitudeValue;
            frequency = frequencyValue;
            phase = phaseValue;
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                baseAnchoredPos = rect.anchoredPosition;
            }
        }

        private void OnEnable()
        {
            if (rect != null)
            {
                baseAnchoredPos = rect.anchoredPosition;
            }
        }

        private void Update()
        {
            if (rect == null) return;
            var t = useUnscaledTime ? Time.unscaledTime : Time.time;
            var y = Mathf.Sin((t + phase) * Mathf.PI * 2f * frequency) * amplitude;
            rect.anchoredPosition = baseAnchoredPos + new Vector2(0f, y);
        }
    }
}

