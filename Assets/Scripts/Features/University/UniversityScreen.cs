using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.University
{
    public class UniversityScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;
        private static readonly string[] YearNames = { "大一", "大二", "大三", "大四" };
        private static readonly string[] YearDescriptions =
        {
            "大学的第一年，一切都是新鲜的。新的城市、新的朋友、新的知识——你满怀憧憬地开始了这段旅程。",
            "大二了，你逐渐找到了自己的节奏。社团、学业、社交之间，你学会平衡，也在悄悄成长。",
            "大三的你，开始认真思考未来。考研？实习？还是直接工作？每一个选择都沉甸甸的。",
            "大四，站在大学的尾巴上。回望四年，你发现自己早已不是当年那个青涩的少年。",
        };

        [SerializeField] private Button backToHomeButton;
        [SerializeField] private Button advanceButton;
        private Image universityIllustration;
        [SerializeField] private Text titleText;
        [SerializeField] private Text yearLabelText;
        [SerializeField] private Text bodyText;
        private Text pageIndicatorText;

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            var state = GameState.Instance;
            if (state != null)
            {
                state.HasSaveData = true;
                if (state.CurrentProgress < GameProgress.University)
                {
                    state.CurrentProgress = GameProgress.University;
                    state.UniversityYearIndex = 0;
                }
            }

            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.University));
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            var yearIdx = state?.UniversityYearIndex ?? 0;

            if (titleText != null)
            {
                titleText.text = "大学时光";
            }

            if (yearLabelText != null && yearIdx < YearNames.Length)
            {
                yearLabelText.text = YearNames[yearIdx];
            }

            if (bodyText != null && yearIdx < YearDescriptions.Length)
            {
                bodyText.text = YearDescriptions[yearIdx];
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"{yearIdx + 1} / 4";
            }

            if (universityIllustration != null)
            {
                var genderPrefix = state?.Gender == PlayerGender.Female ? "女" : "男";
                var yearName = yearIdx < YearNames.Length ? YearNames[yearIdx] : "大一";
                var sprite = RuntimeArt.LoadSprite($"UI/大学学期/{genderPrefix}/{yearName}");
                if (sprite != null)
                {
                    universityIllustration.sprite = sprite;
                    universityIllustration.color = Color.white;
                }
            }

            // 按钮文字：大四时显示"大学毕业"，否则显示"下一学年"
            if (advanceButton != null)
            {
                var btnLabel = advanceButton.GetComponentInChildren<Text>();
                if (btnLabel != null)
                {
                    btnLabel.text = yearIdx >= 3 ? "大学毕业 →" : $"进入{YearNames[Mathf.Min(yearIdx + 1, 3)]} →";
                }
            }
        }

        private void BindEvents()
        {
            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.RemoveAllListeners();
                backToHomeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }

            if (advanceButton != null)
            {
                advanceButton.onClick.RemoveAllListeners();
                advanceButton.onClick.AddListener(AdvanceYear);
            }
        }

        private void AdvanceYear()
        {
            var state = GameState.Instance;
            if (state == null) return;

            if (state.UniversityYearIndex >= 3)
            {
                // 大学毕业，进入人生启程
                state.CurrentProgress = GameProgress.Career;
                state.HasSaveData = true;
                NavigateTo(ScreenType.Career, true);
            }
            else
            {
                state.UniversityYearIndex++;
                state.HasSaveData = true;
                Refresh();
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && advanceButton != null && titleText != null && bodyText != null && yearLabelText != null)
                return;

            BuildRuntimeLayout();
        }

        private void BuildRuntimeLayout()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = (RectTransform)transform;
            Stretch(root);

            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            var bgSprite = RuntimeArt.LoadBg("bg_college");
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Simple;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = UITheme.Bg;
            }

            var panel = CreateUiObject("Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = Color.white;
            panel.gameObject.AddComponent<UiAutoRounded>();
            var panelShadow = panel.gameObject.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            panelShadow.effectDistance = new Vector2(0f, -10f);

            // Header
            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.86f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 64, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.2f, 0.3f);
            titleText.rectTransform.anchorMax = new Vector2(0.8f, 0.9f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            backToHomeButton = CreateSmallButton("← 返回", header, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backToHomeButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.2f);
            backRect.anchorMax = new Vector2(0.2f, 0.8f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backToHomeButton.gameObject.AddComponent<UiPressScale>();

            pageIndicatorText = CreateText("PageIndicator", header, font, 28, FontStyle.Normal, UITheme.TextLight);
            pageIndicatorText.alignment = TextAnchor.MiddleCenter;
            pageIndicatorText.rectTransform.anchorMin = new Vector2(0.8f, 0.2f);
            pageIndicatorText.rectTransform.anchorMax = new Vector2(1f, 0.8f);
            pageIndicatorText.rectTransform.offsetMin = Vector2.zero;
            pageIndicatorText.rectTransform.offsetMax = Vector2.zero;

            // Body
            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.86f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            // Year label
            yearLabelText = CreateText("YearLabel", body, font, 48, FontStyle.Bold, UITheme.FromHex("5C8BCF"));
            yearLabelText.alignment = TextAnchor.MiddleCenter;
            yearLabelText.rectTransform.anchorMin = new Vector2(0.06f, 0.88f);
            yearLabelText.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            yearLabelText.rectTransform.offsetMin = Vector2.zero;
            yearLabelText.rectTransform.offsetMax = Vector2.zero;

            // Large illustration
            var illFrame = CreateUiObject("UniversityIllustration", body);
            illFrame.anchorMin = new Vector2(0.06f, 0.44f);
            illFrame.anchorMax = new Vector2(0.94f, 0.86f);
            illFrame.offsetMin = Vector2.zero;
            illFrame.offsetMax = Vector2.zero;
            var illBg = illFrame.gameObject.AddComponent<Image>();
            illBg.color = new Color32(245, 245, 250, 255);
            RuntimeArt.ApplyRounded(illBg);

            var illObj = CreateUiObject("IllImage", illFrame);
            illObj.anchorMin = new Vector2(0.04f, 0.04f);
            illObj.anchorMax = new Vector2(0.96f, 0.96f);
            illObj.offsetMin = Vector2.zero;
            illObj.offsetMax = Vector2.zero;
            universityIllustration = illObj.gameObject.AddComponent<Image>();
            universityIllustration.preserveAspect = true;

            // Description text
            bodyText = CreateText("BodyText", body, font, 30, FontStyle.Normal, UITheme.TextSoft);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.rectTransform.anchorMin = new Vector2(0.06f, 0.20f);
            bodyText.rectTransform.anchorMax = new Vector2(0.94f, 0.42f);
            bodyText.rectTransform.offsetMin = Vector2.zero;
            bodyText.rectTransform.offsetMax = Vector2.zero;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;

            // Advance button
            advanceButton = CreatePrimaryButton("进入大二 →", body, font, new Color32(186, 104, 200, 255), Color.white);
            var buttonRect = (RectTransform)advanceButton.transform;
            buttonRect.anchorMin = new Vector2(0.10f, 0.04f);
            buttonRect.anchorMax = new Vector2(0.90f, 0.18f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            advanceButton.gameObject.AddComponent<UiPressScale>();
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
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
            text.fontSize = Mathf.RoundToInt(size * UiTextScale);
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.resizeTextForBestFit = false;
            return text;
        }

        private static Button CreateButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var go = new GameObject(label, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            var image = go.AddComponent<Image>();
            image.color = bgColor;
            var button = go.AddComponent<Button>();
            RuntimeArt.ApplyRounded(image);

            var text = CreateText("Text", rect, font, 54, FontStyle.Bold, textColor);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 160f;
            return button;
        }

        private static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var btn = CreateButton(label, parent, font, bgColor, textColor);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = Mathf.RoundToInt(34 * UiTextScale);
            var layout = btn.GetComponent<LayoutElement>();
            if (layout != null) layout.preferredHeight = 80f;
            return btn;
        }

        private static Button CreatePrimaryButton(string label, Transform parent, Font font, Color a, Color textColor)
        {
            var button = CreateButton(label, parent, font, Color.white, textColor);
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
                var grad = button.gameObject.AddComponent<UiCornerGradient>();
                grad.SetColors(a, new Color(a.r * 0.85f, a.g * 0.85f, a.b * 0.85f), new Color(a.r * 0.85f, a.g * 0.85f, a.b * 0.85f), a);
                var shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(a.r / 255f, a.g / 255f, a.b / 255f, 0.35f);
                shadow.effectDistance = new Vector2(0f, -12f);
            }
            return button;
        }
    }
}