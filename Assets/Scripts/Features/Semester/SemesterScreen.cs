using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Semester
{
    public class SemesterScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backToHomeButton;
        [SerializeField] private Button finishSemesterButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text progressText;

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
                if (state.CurrentProgress < GameProgress.Semester)
                {
                    state.CurrentProgress = GameProgress.Semester;
                }
            }

            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            var semester = state != null ? Mathf.Max(0, state.SemesterIndex) + 1 : 1;
            var total = state != null ? Mathf.Max(1, state.TotalSemesters) : 6;

            if (titleText != null)
            {
                titleText.text = $"第 {semester} 学期";
            }

            if (subtitleText != null)
            {
                var labels = new[] { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };
                var label = semester <= labels.Length ? labels[semester - 1] : $"第{semester}学期";
                subtitleText.text = $"「{label}」";
            }

            if (bodyText != null)
            {
                bodyText.text = $"第 {semester} 学期\n\n" +
                    $"课堂学习、课后作业、社团活动、同学交往……\n" +
                    $"一个学期的时光匆匆而过。\n\n" +
                    $"当前能力值：\n" +
                    $"学习能力 {state?.StatIntelligence ?? 0}    情绪管理 {state?.StatPsychology ?? 0}\n" +
                    $"人际关系 {state?.StatSocial ?? 0}    健康状态 {state?.StatHealth ?? 0}\n" +
                    $"金币：{state?.Money ?? 0}\n\n" +
                    $"点击按钮结算本学期成绩，进入放假。";
            }

            if (progressText != null)
            {
                progressText.text = $"进度：{semester - 1}/{total} 学期已完成";
            }

            if (finishSemesterButton != null)
            {
                var label = finishSemesterButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "结算本学期 → 进入假期";
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

            if (finishSemesterButton != null)
            {
                finishSemesterButton.onClick.RemoveAllListeners();
                finishSemesterButton.onClick.AddListener(FinishSemester);
            }
        }

        private void FinishSemester()
        {
            var state = GameState.Instance;
            if (state == null)
            {
                NavigateTo(ScreenType.Home, false);
                return;
            }

            var total = Mathf.Max(1, state.TotalSemesters);

            var completedIndex = state.SemesterIndex;
            var grade = GenerateGrade(completedIndex, state);
            while (state.SemesterGrades.Count <= completedIndex)
            {
                state.SemesterGrades.Add(grade);
            }
            state.SemesterGrades[completedIndex] = grade;

            state.SemesterIndex = Mathf.Clamp(state.SemesterIndex + 1, 0, total);

            if (state.SemesterIndex >= total)
            {
                state.CurrentProgress = GameProgress.Gaokao;
            }
            else
            {
                state.CurrentProgress = GameProgress.Semester;
            }

            state.HasSaveData = true;
            NavigateTo(ScreenType.SemesterResult, false);
        }

        private static string GenerateGrade(int semesterIndex, GameState state)
        {
            var seed = semesterIndex * 31 + state.StatIntelligence * 7 + state.StatPsychology * 3;
            var gradePool = new[] { "A", "A", "B", "B", "B", "C", "C" };
            return gradePool[Mathf.Abs(seed) % gradePool.Length];
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && finishSemesterButton != null && titleText != null && subtitleText != null && bodyText != null)
            {
                return;
            }

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
            var bgSprite = RuntimeArt.LoadBg("bg_highschool");
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
            header.anchorMin = new Vector2(0f, 0.74f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 74, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.36f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.88f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            subtitleText = CreateText("Subtitle", header, font, 38, FontStyle.Normal, UITheme.TextLight);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.06f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.42f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;

            backToHomeButton = CreateSmallButton("← 返回主界面", header, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backToHomeButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.72f);
            backRect.anchorMax = new Vector2(0.34f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backToHomeButton.gameObject.AddComponent<UiPressScale>();

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.74f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var infoCard = CreateUiObject("InfoCard", body);
            infoCard.anchorMin = new Vector2(0.06f, 0.18f);
            infoCard.anchorMax = new Vector2(0.94f, 0.98f);
            infoCard.offsetMin = Vector2.zero;
            infoCard.offsetMax = Vector2.zero;
            var infoImage = infoCard.gameObject.AddComponent<Image>();
            infoImage.color = Color.white;
            RuntimeArt.ApplyRounded(infoImage);
            var infoShadow = infoCard.gameObject.AddComponent<Shadow>();
            infoShadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            infoShadow.effectDistance = new Vector2(0f, -10f);

            bodyText = CreateText("BodyText", infoCard, font, 34, FontStyle.Normal, UITheme.TextSoft);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.rectTransform.anchorMin = new Vector2(0.06f, 0.14f);
            bodyText.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            bodyText.rectTransform.offsetMin = Vector2.zero;
            bodyText.rectTransform.offsetMax = Vector2.zero;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;

            progressText = CreateText("ProgressText", infoCard, font, 30, FontStyle.Bold, UITheme.FromHex("888888"));
            progressText.alignment = TextAnchor.UpperRight;
            progressText.rectTransform.anchorMin = new Vector2(0.06f, 0.0f);
            progressText.rectTransform.anchorMax = new Vector2(0.94f, 0.12f);
            progressText.rectTransform.offsetMin = Vector2.zero;
            progressText.rectTransform.offsetMax = Vector2.zero;

            finishSemesterButton = CreatePrimaryButton("结算本学期 → 进入假期", body, font, UITheme.Confirm, Color.white);
            var finishRect = (RectTransform)finishSemesterButton.transform;
            finishRect.anchorMin = new Vector2(0.10f, 0.04f);
            finishRect.anchorMax = new Vector2(0.90f, 0.14f);
            finishRect.offsetMin = Vector2.zero;
            finishRect.offsetMax = Vector2.zero;
            finishSemesterButton.gameObject.AddComponent<UiPressScale>();
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
                grad.SetColors(a, UITheme.ConfirmHover, UITheme.ConfirmHover, a);
                var shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(a.r / 255f, a.g / 255f, a.b / 255f, 0.35f);
                shadow.effectDistance = new Vector2(0f, -12f);
            }
            return button;
        }
    }
}