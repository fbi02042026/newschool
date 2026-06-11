using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;

namespace GaokaoSimulator.UI.Effects
{
    [RequireComponent(typeof(Image))]
    public class UiAutoRounded : MonoBehaviour
    {
        private void Awake()
        {
            var image = GetComponent<Image>();
            RuntimeArt.ApplyRounded(image);
        }
    }
}

