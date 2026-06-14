using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Semester
{
    public class SemesterResultScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        private static readonly string[] SemesterLabels = { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };

        private static readonly string[] GradeAComments =
        {
            "表现优异！这个学期你真的很努力",
            "学霸就是你！继续保持这个状态",
            "老师都在夸你进步很大，真棒！"
        };

        private static readonly string[] GradeBComments =
        {
            "稳扎稳打，这个学期过得不错",
            "中规中矩，还有很多上升空间",
            "还不错哦，下个学期再冲一冲"
        };

        private static readonly string[] GradeCComments =
        {
            "有些波折，但还有机会追赶",
            "这个学期不太顺利，但没关系",
            "别灰心，高中还长着呢"
        };

        [Header("UI引用")]
        [SerializeField] private Button backToHomeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text gradeText;
        [SerializeField] private Text commentText;
        [SerializeField] private Text statsText;

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.SemesterResult));
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            var completedIndex = state != null ? Mathf.Max(0, state.SemesterIndex - 1) : 0;
            var semesterNum = completedIndex + 1;
            var grade = "B";
            if (state != null && state.SemesterGrades.Count > completedIndex)
            {
                grade = state.SemesterGrades[completedIndex];
            }

            if (titleText != null)
            {
                titleText.text = $"第 {semesterNum} 学期结束";
            }

            if (subtitleText != null)
            {
                var label = semesterNum <= SemesterLabels.Length ? SemesterLabels[semesterNum - 1] : $"第{semesterNum}学期";
                subtitleText.text = $"「{label}」";
            }

            if (gradeText != null)
            {
                gradeText.text = grade;
                gradeText.color = GetGradeColor(grade);
            }

            if (commentText != null)
            {
                commentText.text = GetComment(grade, completedIndex);
            }

            if (statsText != null)
            {
                statsText.text =
                    $"学习能力：{state?.StatIntelligence ?? 0}\n" +
                    $"情绪管理：{state?.StatPsychology ?? 0}\n" +
                    $"人际关系：{state?.StatSocial ?? 0}\n" +
                    $"健康状态：{state?.StatHealth ?? 0}\n" +
                    $"金币：{state?.Money ?? 0}";
            }
        }

        private void BindEvents()
        {
            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.RemoveAllListeners();
                backToHomeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && titleText != null && subtitleText != null &&
                gradeText != null && commentText != null && statsText != null)
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

            // 背景
            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            var bgSprite = RuntimeArt.LoadBg("bg_semester_result");
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

            // 主面板
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

            // 顶部标题区域
            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.78f);
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

            // 主体内容区域
            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.78f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            // 评级卡片（上半部分）
            var gradeCard = CreateUiObject("GradeCard", body);
            gradeCard.anchorMin = new Vector2(0.06f, 0.50f);
            gradeCard.anchorMax = new Vector2(0.94f, 0.98f);
            gradeCard.offsetMin = Vector2.zero;
            gradeCard.offsetMax = Vector2.zero;
            var cardImage = gradeCard.gameObject.AddComponent<Image>();
            cardImage.color = Color.white;
            RuntimeArt.ApplyRounded(cardImage);
            var cardShadow = gradeCard.gameObject.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            cardShadow.effectDistance = new Vector2(0f, -10f);

            var gradeLabel = CreateText("GradeLabel", gradeCard, font, 30, FontStyle.Normal, UITheme.TextLight);
            gradeLabel.text = "本学期评级";
            gradeLabel.alignment = TextAnchor.MiddleCenter;
            gradeLabel.rectTransform.anchorMin = new Vector2(0f, 0.58f);
            gradeLabel.rectTransform.anchorMax = new Vector2(1f, 0.78f);
            gradeLabel.rectTransform.offsetMin = Vector2.zero;
            gradeLabel.rectTransform.offsetMax = Vector2.zero;

            gradeText = CreateText("GradeText", gradeCard, font, 96, FontStyle.Bold, UITheme.Gold);
            gradeText.alignment = TextAnchor.MiddleCenter;
            gradeText.rectTransform.anchorMin = new Vector2(0f, 0.22f);
            gradeText.rectTransform.anchorMax = new Vector2(1f, 0.62f);
            gradeText.rectTransform.offsetMin = Vector2.zero;
            gradeText.rectTransform.offsetMax = Vector2.zero;

            commentText = CreateText("CommentText", gradeCard, font, 32, FontStyle.Normal, UITheme.TextSoft);
            commentText.alignment = TextAnchor.MiddleCenter;
            commentText.rectTransform.anchorMin = new Vector2(0.06f, 0.04f);
            commentText.rectTransform.anchorMax = new Vector2(0.94f, 0.26f);
            commentText.rectTransform.offsetMin = Vector2.zero;
            commentText.rectTransform.offsetMax = Vector2.zero;

            // 能力值卡片（下半部分左侧放stats，右侧留空或放按钮区域的上方）
            var statsCard = CreateUiObject("StatsCard", body);
            statsCard.anchorMin = new Vector2(0.06f, 0.14f);
            statsCard.anchorMax = new Vector2(0.94f, 0.46f);
            statsCard.offsetMin = Vector2.zero;
            statsCard.offsetMax = Vector2.zero;
            var statsImage = statsCard.gameObject.AddComponent<Image>();
            statsImage.color = Color.white;
            RuntimeArt.ApplyRounded(statsImage);
            var statsShadow = statsCard.gameObject.AddComponent<Shadow>();
            statsShadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            statsShadow.effectDistance = new Vector2(0f, -10f);

            statsText = CreateText("StatsText", statsCard, font, 34, FontStyle.Normal, UITheme.TextSoft);
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.rectTransform.anchorMin = new Vector2(0.08f, 0.06f);
            statsText.rectTransform.anchorMax = new Vector2(0.92f, 0.94f);
            statsText.rectTransform.offsetMin = Vector2.zero;
            statsText.rectTransform.offsetMax = Vector2.zero;
            statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statsText.verticalOverflow = VerticalWrapMode.Overflow;

            // 返回主界面按钮
            backToHomeButton = CreatePrimaryButton("返回主界面", body, font, UITheme.Confirm, Color.white);
            var backRect = (RectTransform)backToHomeButton.transform;
            backRect.anchorMin = new Vector2(0.10f, 0.02f);
            backRect.anchorMax = new Vector2(0.90f, 0.10f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backToHomeButton.gameObject.AddComponent<UiPressScale>();
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

        private static Color GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "A": return UITheme.Gold;
                case "B": return new Color(0.30f, 0.70f, 0.30f);
                case "C": return new Color(0.55f, 0.55f, 0.55f);
                default: return UITheme.Text;
            }
        }

        private static string GetComment(string grade, int semesterIndex)
        {
            string[] pool;
            switch (grade)
            {
                case "A": pool = GradeAComments; break;
                case "B": pool = GradeBComments; break;
                case "C": pool = GradeCComments; break;
                default: return "稳扎稳打，继续保持";
            }

            return pool[Mathf.Abs(semesterIndex) % pool.Length];
        }
    }
}