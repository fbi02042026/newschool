using UnityEngine;
using UnityEngine.EventSystems;

namespace GaokaoSimulator.UI.Effects
{
    [DisallowMultipleComponent]
    public class UiPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressedScale = 0.96f;
        [SerializeField] private float returnSpeed = 14f;
        [SerializeField] private bool useUnscaledTime = true;

        private RectTransform rect;
        private Vector3 baseScale = Vector3.one;
        private float target = 1f;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                baseScale = rect.localScale;
            }
        }

        private void OnEnable()
        {
            if (rect != null)
            {
                baseScale = rect.localScale;
            }
            target = 1f;
        }

        private void Update()
        {
            if (rect == null) return;
            var dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            var current = rect.localScale.x / (baseScale.x == 0 ? 1f : baseScale.x);
            var next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-returnSpeed * dt));
            rect.localScale = baseScale * next;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            target = pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            target = 1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            target = 1f;
        }
    }
}

