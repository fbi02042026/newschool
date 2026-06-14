using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Gaokao
{
    public class GaokaoScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backToHomeButton;
        [SerializeField] private Button enterGaokaoButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

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
                if (state.CurrentProgress < GameProgress.Gaokao)
                {
                    state.CurrentProgress = GameProgress.Gaokao;
                }
            }

            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.Gaokao));
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            if (titleText != null)
            {
                titleText.text = "决胜高考";
            }

            if (bodyText != null)
            {
                bodyText.text = $"叮铃铃——考试开始的铃声响起……\n\n教室里安静得只剩笔尖划过答题卡的沙沙声，偶尔传来翻卷子的哗啦响。\n你深吸一口气，脑海里闪过这三年的点点滴滴——清晨的早读、深夜的刷题、课间的嬉闹……\n那些努力的日子，都变成了此刻笔下的底气！\n\n学习能力：{state?.StatIntelligence ?? 0}    情绪管理：{state?.StatPsychology ?? 0}\n人际关系：{state?.StatSocial ?? 0}    健康状态：{state?.StatHealth ?? 0}\n\n别紧张，你已经很棒啦～点击下方按钮，写下属于你的答卷吧！";
            }
        }

        private void BindEvents()
        {
            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.RemoveAllListeners();
                backToHomeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }

            if (enterGaokaoButton != null)
            {
                enterGaokaoButton.onClick.RemoveAllListeners();
                enterGaokaoButton.onClick.AddListener(CompleteGaokao);
            }
        }

        private void CompleteGaokao()
        {
            var state = GameState.Instance;
            if (state == null) return;

            state.CurrentProgress = GameProgress.Volunteer;
            state.HasSaveData = true;
            NavigateTo(ScreenType.Volunteer, true);
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && enterGaokaoButton != null && titleText != null && bodyText != null)
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
            var bgSprite = RuntimeArt.LoadBg("bg_gaokao");
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

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.78f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 74, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.34f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.90f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            backToHomeButton = CreateSmallButton("← 返回主界面", header, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backToHomeButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.72f);
            backRect.anchorMax = new Vector2(0.34f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backToHomeButton.gameObject.AddComponent<UiPressScale>();

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.78f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var infoCard = CreateUiObject("InfoCard", body);
            infoCard.anchorMin = new Vector2(0.06f, 0.28f);
            infoCard.anchorMax = new Vector2(0.94f, 0.98f);
            infoCard.offsetMin = Vector2.zero;
            infoCard.offsetMax = Vector2.zero;
            var infoImage = infoCard.gameObject.AddComponent<Image>();
            infoImage.color = Color.white;
            RuntimeArt.ApplyRounded(infoImage);
            var infoShadow = infoCard.gameObject.AddComponent<Shadow>();
            infoShadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            infoShadow.effectDistance = new Vector2(0f, -10f);

            bodyText = CreateText("BodyText", infoCard, font, 36, FontStyle.Normal, UITheme.TextSoft);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.rectTransform.anchorMin = new Vector2(0.06f, 0.06f);
            bodyText.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            bodyText.rectTransform.offsetMin = Vector2.zero;
            bodyText.rectTransform.offsetMax = Vector2.zero;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;

            enterGaokaoButton = CreatePrimaryButton("进入高考考场 →", body, font, new Color32(255, 138, 101, 255), Color.white);
            var buttonRect = (RectTransform)enterGaokaoButton.transform;
            buttonRect.anchorMin = new Vector2(0.10f, 0.06f);
            buttonRect.anchorMax = new Vector2(0.90f, 0.20f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            enterGaokaoButton.gameObject.AddComponent<UiPressScale>();
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
            if (text != null) text.fontSize = Mathf.RoundToInt(36 * UiTextScale);
            var layout = btn.GetComponent<LayoutElement>();
            if (layout != null) layout.preferredHeight = 100f;
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