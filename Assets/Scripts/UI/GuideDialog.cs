using UnityEngine;

namespace GaokaoSimulator.UI
{
    public static class GuideDialog
    {
        public static void Show(Transform screenRoot, string key, GuideStep[] steps)
        {
            if (screenRoot == null || steps == null || steps.Length == 0)
            {
                return;
            }

            Debug.Log($"[GuideDialog] Show guide '{key}' with {steps.Length} steps");
            // TODO: 实现引导对话框 UI
            for (int i = 0; i < steps.Length; i++)
            {
                Debug.Log($"[GuideDialog] Step {i + 1}: {steps[i].Title} - {steps[i].Description}");
            }
        }
    }
}