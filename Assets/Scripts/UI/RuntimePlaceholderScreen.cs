using UnityEngine;

namespace GaokaoSimulator.UI
{
    public class RuntimePlaceholderScreen : ScreenBase
    {
        private ScreenType screenType;

        public void Configure(ScreenType type)
        {
            screenType = type;
            ScreenId = type;
        }

        protected override void Initialize()
        {
            Debug.Log($"[RuntimePlaceholderScreen] Placeholder for {screenType}");
        }
    }
}