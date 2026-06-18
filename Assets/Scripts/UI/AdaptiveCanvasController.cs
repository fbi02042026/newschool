using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasController : MonoBehaviour
    {
        private CanvasScaler canvasScaler;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1242, 2760);
                canvasScaler.matchWidthOrHeight = 1f;
            }
        }
    }
}