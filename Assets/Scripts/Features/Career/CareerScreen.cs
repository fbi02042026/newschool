using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Career
{
    public class CareerScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [SerializeField] private Button backToHomeButton;
        [SerializeField] private Button continueButton;
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
                if (state.CurrentProgress < GameProgress.Career)
                {
                    state.CurrentProgress = GameProgress.Career;
                }
            }

            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.Career));
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
                titleText.text = "人生启程";
            }

            if (bodyText != null)
            {
                bodyText.text = $"大学毕业了，你正式踏入社会。\n\n从校园到职场，从学生到社会人，这是人生的重要转折点。\n\n找工作、创业、继续深造……每一个选择都会写就不同的人生篇章。回顾高中一路走到现在，你的努力和选择在此刻开花结果。\n\n当前金币：{state?.Money ?? 0}";
            }
        }

        private void BindEvents()
        {
            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.RemoveAllListeners();
                backToHomeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ProceedToSummary);
            }
        }

        private void ProceedToSummary()
        {
            var state = GameState.Instance;
            if (state == null) return;

            state.CurrentProgress = GameProgress.Summary;
            state.HasSaveData = true;
            NavigateTo(ScreenType.Summary, true);
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && continueButton != null && titleText != null && bodyText != null)
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
            var bgSprite = RuntimeArt.LoadBg("bg_life");
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

            continueButton = CreatePrimaryButton("看人生总结 →", body, font, new Color32(255, 183, 77, 255), Color.white);
            var buttonRect = (RectTransform)continueButton.transform;
            buttonRect.anchorMin = new Vector2(0.10f, 0.06f);
            buttonRect.anchorMax = new Vector2(0.90f, 0.20f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            continueButton.gameObject.AddComponent<UiPressScale>();
        }
    }
}