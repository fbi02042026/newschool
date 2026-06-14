using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    public static class ScreenFlowHint
    {
        private const string HintObjectName = "NextScreenHint";
        private const int DefaultFontSize = 40;

        public static void Ensure(Transform parent, string text)
        {
            if (parent == null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var rect = parent.Find(HintObjectName) as RectTransform;
            Text label;

            if (rect == null)
            {
                var go = new GameObject(HintObjectName, typeof(RectTransform));
                rect = go.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.localScale = Vector3.one;
                rect.anchorMin = new Vector2(0.45f, 0.02f);
                rect.anchorMax = new Vector2(0.98f, 0.10f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                label = go.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = DefaultFontSize;
                label.fontStyle = FontStyle.Bold;
                label.color = new Color32(160, 143, 167, 255);
                label.alignment = TextAnchor.LowerRight;
                label.supportRichText = false;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Overflow;
                label.raycastTarget = false;

                var shadow = go.AddComponent<Shadow>();
                shadow.effectColor = new Color(1f, 1f, 1f, 0.72f);
                shadow.effectDistance = new Vector2(0f, 3f);
            }
            else
            {
                label = rect.GetComponent<Text>();
                if (label == null)
                {
                    label = rect.gameObject.AddComponent<Text>();
                    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }

            if (label != null)
            {
                label.fontSize = DefaultFontSize;
                label.raycastTarget = false;
            }

            label.text = text;
        }

        public static void Clear(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            var existing = parent.Find(HintObjectName);
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }
        }

        public static string GetNextLabel(ScreenType current)
        {
            switch (current)
            {
                case ScreenType.Launch:
                    return "下一步：创建人物 →";
                case ScreenType.Profile:
                    return "下一步：家庭背景 →";
                case ScreenType.Family:
                    return "下一步：选择省市 →";
                case ScreenType.Province:
                    return "下一步：选科 →";
                case ScreenType.Subject:
                    return "下一步：进入主界面 →";
                case ScreenType.Home:
                    return "继续主线推进学期，或探索功能 →";
                case ScreenType.TalentTree:
                    return "下一步：进入学期 →";
                case ScreenType.Semester:
                    return "结算成绩，推进学期 →";
                case ScreenType.SemesterResult:
                    return "查看成绩后返回主界面 →";
                case ScreenType.Gaokao:
                    return "下一步：志愿填报 →";
                case ScreenType.Volunteer:
                    return "下一步：大学阶段 →";
                case ScreenType.University:
                    return "下一步：毕业去向 →";
                case ScreenType.Career:
                    return "下一步：人生总结 →";
                case ScreenType.Summary:
                    return "回顾一生，可选择重新开始 →";
                default:
                    return string.Empty;
            }
        }
    }
}
