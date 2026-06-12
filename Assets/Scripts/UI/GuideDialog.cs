using System;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    public sealed class GuideDialog : MonoBehaviour
    {
        private const float GuideTextScale = 1.75f;

        [SerializeField] private RectTransform maskRoot;
        [SerializeField] private RectTransform dialogCard;
        [SerializeField] private Image characterImage;
        [SerializeField] private Text characterFallbackText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text nextButtonText;

        private GuideStep[] steps;
        private int index;
        private Action onClosed;

        public static void Show(Transform screenRoot, string key, GuideStep[] steps, Action onClosed = null)
        {
            if (screenRoot == null || steps == null || steps.Length == 0)
            {
                return;
            }

            var instance = EnsureInstance(screenRoot);
            instance.onClosed = onClosed;
            instance.steps = steps;
            instance.index = 0;
            instance.gameObject.SetActive(true);
            instance.Refresh();
        }

        private static GuideDialog EnsureInstance(Transform screenRoot)
        {
            var existing = screenRoot.GetComponentInChildren<GuideDialog>(true);
            if (existing != null)
            {
                return existing;
            }

            var root = new GameObject("GuideDialog", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(screenRoot, false);
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var dialog = root.AddComponent<GuideDialog>();
            dialog.BuildRuntimeLayout();
            root.SetActive(false);
            return dialog;
        }

        private void BuildRuntimeLayout()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = (RectTransform)transform;

            maskRoot = CreateUiObject("Mask", root);
            Stretch(maskRoot);
            var maskImage = maskRoot.gameObject.AddComponent<Image>();
            maskImage.color = new Color(0f, 0f, 0f, 0.45f);

            dialogCard = CreateUiObject("DialogCard", root);
            dialogCard.anchorMin = new Vector2(0.04f, 0.14f);
            dialogCard.anchorMax = new Vector2(0.96f, 0.78f);
            dialogCard.offsetMin = Vector2.zero;
            dialogCard.offsetMax = Vector2.zero;
            var cardImage = dialogCard.gameObject.AddComponent<Image>();
            cardImage.color = Color.white;
            dialogCard.gameObject.AddComponent<UiAutoRounded>();
            var shadow = dialogCard.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            shadow.effectDistance = new Vector2(0f, -18f);

            var topBar = CreateUiObject("TopBar", dialogCard);
            topBar.anchorMin = new Vector2(0.04f, 0.74f);
            topBar.anchorMax = new Vector2(0.96f, 0.96f);
            topBar.offsetMin = Vector2.zero;
            topBar.offsetMax = Vector2.zero;

            titleText = CreateText("Title", topBar, font, 44, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.rectTransform.anchorMin = new Vector2(0f, 0f);
            titleText.rectTransform.anchorMax = new Vector2(0.78f, 1f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;
            titleText.text = "新手引导";

            closeButton = CreateSmallIconButton("Close", topBar, font, "✕");
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.86f, 0.12f);
            closeRect.anchorMax = new Vector2(1f, 0.88f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            closeButton.onClick.AddListener(Close);

            var body = CreateUiObject("Body", dialogCard);
            body.anchorMin = new Vector2(0.04f, 0.08f);
            body.anchorMax = new Vector2(0.96f, 0.74f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var characterWrap = CreateUiObject("CharacterWrap", body);
            characterWrap.anchorMin = new Vector2(0f, 0f);
            characterWrap.anchorMax = new Vector2(0.30f, 1f);
            characterWrap.offsetMin = Vector2.zero;
            characterWrap.offsetMax = Vector2.zero;

            var characterBg = characterWrap.gameObject.AddComponent<Image>();
            characterBg.color = UITheme.CardButter;
            characterWrap.gameObject.AddComponent<UiAutoRounded>();

            var characterImageRect = CreateUiObject("CharacterImage", characterWrap);
            Stretch(characterImageRect);
            characterImage = characterImageRect.gameObject.AddComponent<Image>();
            characterImage.preserveAspect = true;
            characterImage.color = Color.white;
            characterImage.type = Image.Type.Simple;
            characterImageRect.gameObject.AddComponent<UiFloatBob>().Configure(8f, 0.5f, 0f);

            characterImage.sprite = Resources.Load<Sprite>("UI/Guide/guide_character");

            characterFallbackText = CreateText("Fallback", characterWrap, font, 52, FontStyle.Bold, UITheme.Text);
            Stretch(characterFallbackText.rectTransform);
            characterFallbackText.alignment = TextAnchor.MiddleCenter;
            characterFallbackText.text = "🎓";

            var bubble = CreateUiObject("Bubble", body);
            bubble.anchorMin = new Vector2(0.34f, 0f);
            bubble.anchorMax = new Vector2(1f, 1f);
            bubble.offsetMin = Vector2.zero;
            bubble.offsetMax = Vector2.zero;
            var bubbleImage = bubble.gameObject.AddComponent<Image>();
            bubbleImage.color = new Color32(250, 250, 250, 255);
            bubble.gameObject.AddComponent<UiAutoRounded>();

            bodyText = CreateText("BodyText", bubble, font, 32, FontStyle.Normal, UITheme.TextSoft);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.rectTransform.anchorMin = new Vector2(0.06f, 0.26f);
            bodyText.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
            bodyText.rectTransform.offsetMin = Vector2.zero;
            bodyText.rectTransform.offsetMax = Vector2.zero;
            bodyText.text = "……";

            nextButton = CreatePrimaryButton("NextButton", bubble, font, "知道啦 →");
            var nextRect = (RectTransform)nextButton.transform;
            nextRect.anchorMin = new Vector2(0.44f, 0.02f);
            nextRect.anchorMax = new Vector2(0.94f, 0.20f);
            nextRect.offsetMin = Vector2.zero;
            nextRect.offsetMax = Vector2.zero;
            nextButton.onClick.AddListener(Next);

            nextButtonText = nextButton.GetComponentInChildren<Text>();
        }

        private void Refresh()
        {
            if (steps == null || steps.Length == 0)
            {
                Close();
                return;
            }

            index = Mathf.Clamp(index, 0, steps.Length - 1);
            var step = steps[index];

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(step.Title) ? "新手引导" : step.Title;
            }

            if (bodyText != null)
            {
                bodyText.text = step.Body ?? string.Empty;
            }

            if (characterImage != null)
            {
                characterImage.enabled = characterImage.sprite != null;
            }

            if (characterFallbackText != null)
            {
                characterFallbackText.gameObject.SetActive(characterImage == null || characterImage.sprite == null);
            }

            if (nextButtonText != null)
            {
                nextButtonText.text = index >= steps.Length - 1 ? "开始吧 →" : "下一条 →";
            }
        }

        private void Next()
        {
            if (steps == null)
            {
                Close();
                return;
            }

            if (index >= steps.Length - 1)
            {
                Close();
                return;
            }

            index++;
            Refresh();
        }

        private void Close()
        {
            gameObject.SetActive(false);
            var cb = onClosed;
            onClosed = null;
            cb?.Invoke();
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
            text.fontSize = Mathf.RoundToInt(size * GuideTextScale);
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.lineSpacing = 1.14f;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateSmallIconButton(string name, Transform parent, Font font, string label)
        {
            var rect = CreateUiObject(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color32(255, 255, 255, 248);
            rect.gameObject.AddComponent<UiAutoRounded>();
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            outline.effectDistance = new Vector2(3f, -3f);
            var button = rect.gameObject.AddComponent<Button>();
            rect.gameObject.AddComponent<UiPressScale>();

            var text = CreateText("Text", rect, font, 28, FontStyle.Bold, UITheme.Text);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            return button;
        }

        private static Button CreatePrimaryButton(string name, Transform parent, Font font, string label)
        {
            var rect = CreateUiObject(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            rect.gameObject.AddComponent<UiAutoRounded>();
            var grad = rect.gameObject.AddComponent<UiCornerGradient>();
            grad.SetColors(UITheme.Confirm, UITheme.ConfirmHover, UITheme.ConfirmHover, UITheme.Confirm);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(UITheme.Confirm.r / 255f, UITheme.Confirm.g / 255f, UITheme.Confirm.b / 255f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -10f);
            var button = rect.gameObject.AddComponent<Button>();
            rect.gameObject.AddComponent<UiPressScale>();

            var text = CreateText("Text", rect, font, 32, FontStyle.Bold, Color.white);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            return button;
        }
    }

    public readonly struct GuideStep
    {
        public readonly string Title;
        public readonly string Body;

        public GuideStep(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }
}

