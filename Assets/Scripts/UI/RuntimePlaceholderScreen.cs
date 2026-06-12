using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    /// <summary>
    /// 运行时占位界面
    /// 在目标界面尚未实现时提供可返回的占位页，保证流程不断。
    /// </summary>
    public class RuntimePlaceholderScreen : ScreenBase
    {
        private ScreenType configuredType;
        private Text titleText;
        private Text descriptionText;
        private Button backButton;

        public void Configure(ScreenType screenType)
        {
            configuredType = screenType;
        }

        protected override void Initialize()
        {
            BuildLayout();
            ScreenFlowHint.Ensure(transform, ScreenFlowHint.GetNextLabel(configuredType));
            RefreshTexts();
        }

        protected override void OnScreenOpen()
        {
            RefreshTexts();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            RefreshTexts();
        }

        private void BuildLayout()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var background = CreateUiObject("Background", transform);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = new Color32(255, 247, 252, 255);
            Stretch(background);

            var card = CreateUiObject("Card", transform);
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color32(255, 255, 255, 248);
            cardImage.type = Image.Type.Sliced;
            var cardShadow = card.gameObject.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0.55f, 0.38f, 0.56f, 0.18f);
            cardShadow.effectDistance = new Vector2(0f, -16f);

            card.anchorMin = new Vector2(0.08f, 0.18f);
            card.anchorMax = new Vector2(0.92f, 0.82f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;

            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 80, 48);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            titleText = CreateText("Title", card, font, 72, FontStyle.Bold, new Color32(84, 56, 104, 255));
            titleText.alignment = TextAnchor.MiddleCenter;

            descriptionText = CreateText("Description", card, font, 42, FontStyle.Normal, new Color32(101, 83, 112, 255));
            descriptionText.alignment = TextAnchor.MiddleCenter;
            descriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descriptionText.verticalOverflow = VerticalWrapMode.Overflow;

            backButton = CreateButton("返回启动页", card, font, new Color32(255, 181, 203, 255), new Color32(115, 60, 92, 255));
            backButton.onClick.AddListener(() => NavigateTo(ScreenType.Launch, false));
        }

        private void RefreshTexts()
        {
            if (titleText == null || descriptionText == null)
            {
                return;
            }

            titleText.text = $"{GetScreenDisplayName(configuredType)}";
            descriptionText.text = "这个界面还在开发中。\n当前先保留流程入口，方便你直接运行测试启动链路和按钮跳转。";
            ScreenFlowHint.Ensure(transform, ScreenFlowHint.GetNextLabel(configuredType));
        }

        private static string GetScreenDisplayName(ScreenType screenType)
        {
            switch (screenType)
            {
                case ScreenType.Profile: return "创建人物";
                case ScreenType.Family: return "家庭背景";
                case ScreenType.Province: return "选择省份";
                case ScreenType.Subject: return "选科";
                case ScreenType.Home: return "主界面";
                case ScreenType.TalentTree: return "天赋树";
                case ScreenType.Semester: return "学期主界面";
                case ScreenType.Gaokao: return "高考";
                case ScreenType.Volunteer: return "志愿填报";
                case ScreenType.University: return "大学阶段";
                case ScreenType.Career: return "毕业到30岁";
                case ScreenType.Summary: return "人生总结";
                default: return "开发中";
            }
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, Color color)
        {
            var rect = CreateUiObject(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.resizeTextForBestFit = false;

            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = size * 2.2f;
            return text;
        }

        private static Button CreateButton(string label, Transform parent, Font font, Color backgroundColor, Color textColor)
        {
            var rect = CreateUiObject($"Button_{label}", parent);
            rect.sizeDelta = new Vector2(0f, 170f);

            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.05f;
            colors.pressedColor = backgroundColor * 0.92f;
            colors.selectedColor = backgroundColor;
            button.colors = colors;

            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.4f, 0.2f, 0.3f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -8f);

            var text = CreateText("Label", rect, font, 46, FontStyle.Bold, textColor);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 26;
            text.resizeTextMaxSize = 46;

            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 170f;

            return button;
        }
    }
}
