using System.Collections;
using System.Collections.Generic;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Summary
{
    public class SummaryScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;
        private static readonly string[] SemesterNames = { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };

        private Text titleText;
        private Text subtitleText;
        private Text infoCardText;
        private Text gradeListTitle;
        private RectTransform gradeListContainer;
        private Image semesterImage;
        private Text semesterImageComment;
        private Image universityImage;
        private Text universityImageComment;
        private Image endingImage;
        private Text endingTitle;
        private Text endingDesc;
        private Text statIntelligenceText;
        private Text statPsychologyText;
        private Text statSocialText;
        private Text statHealthText;
        private Text lifeSummaryText;
        private Button backToHomeButton;
        private Button restartButton;
        private ScrollRect summaryScrollRect;
        private Coroutine autoScrollCoroutine;
        private const float ScrollDelay = 2.5f;
        private const float ScrollDuration = 15f;

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
                if (state.CurrentProgress < GameProgress.Summary)
                {
                    state.CurrentProgress = GameProgress.Summary;
                }
            }

            Refresh();

            if (autoScrollCoroutine != null) StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = StartCoroutine(AutoScrollRoutine());
        }

        protected override void OnScreenClose()
        {
            if (autoScrollCoroutine != null)
            {
                StopCoroutine(autoScrollCoroutine);
                autoScrollCoroutine = null;
            }
        }

        private IEnumerator AutoScrollRoutine()
        {
            yield return new WaitForSeconds(ScrollDelay);
            // 等待布局完成
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (summaryScrollRect == null) yield break;

            var content = summaryScrollRect.content;
            if (content == null) yield break;

            // 确保内容高度已计算
            var contentHeight = content.rect.height;
            var viewportHeight = ((RectTransform)summaryScrollRect.viewport).rect.height;
            if (contentHeight <= viewportHeight + 10f) yield break; // 内容不够长，不需要滚动

            float elapsed = 0f;
            while (elapsed < ScrollDuration)
            {
                if (summaryScrollRect == null || summaryScrollRect.content == null) yield break;

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / ScrollDuration);
                // 使用 easeInOutQuad 让滚动更平滑
                var eased = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                summaryScrollRect.verticalNormalizedPosition = Mathf.Lerp(1f, 0f, eased);
                yield return null;
            }

            // 确保滚到底
            if (summaryScrollRect != null)
            {
                summaryScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            if (state == null) return;

            var name = !string.IsNullOrWhiteSpace(state.PlayerName) ? state.PlayerName : "我";
            var province = !string.IsNullOrWhiteSpace(state.SelectedProvince) ? state.SelectedProvince : "某省";
            var gender = state.Gender == PlayerGender.Female ? "她" : "他";
            var genderSelf = state.Gender == PlayerGender.Female ? "我" : "我";
            var familyLabel = GetFamilyLabel(state.SelectedFamily);

            if (titleText != null) titleText.text = "我的人生，独一无二";
            if (subtitleText != null) subtitleText.text = $"「{name}」的人生旅程";

            var subjectsReady = state.FirstSubject != FirstSubject.None && state.SecondSubjects != null && state.SecondSubjects.Count == 2;
            var subjects = subjectsReady
                ? $"{GetFirstLabel(state.FirstSubject)} + {GetSecondLabel(state.SecondSubjects[0])}/{GetSecondLabel(state.SecondSubjects[1])}"
                : "未完成选科";
            if (infoCardText != null)
            {
                infoCardText.text = $"姓名：{name}    省份：{province}    家庭：{familyLabel}    选科：{subjects}";
            }

            // 高中成绩
            if (gradeListTitle != null)
            {
                gradeListTitle.text = "高中成绩";
            }
            if (gradeListContainer != null)
            {
                for (int i = gradeListContainer.childCount - 1; i >= 0; i--)
                {
                    var child = gradeListContainer.GetChild(i);
                    if (child != null) Destroy(child.gameObject);
                }

                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var grades = state.SemesterGrades;
                if (grades != null && grades.Count > 0)
                {
                    for (int i = 0; i < grades.Count; i++)
                    {
                        var grade = grades[i];
                        if (string.IsNullOrWhiteSpace(grade)) continue;

                        var semesterName = i < SemesterNames.Length ? SemesterNames[i] : $"第{i + 1}学期";
                        var gradeColor = GetGradeColor(grade);

                        var row = CreateUiObject($"GradeRow_{i}", gradeListContainer);
                        var rowLayout = row.gameObject.AddComponent<LayoutElement>();
                        rowLayout.preferredHeight = 36f;

                        var nameText = CreateText("Name", row, font, 26, FontStyle.Normal, UITheme.Text);
                        nameText.alignment = TextAnchor.MiddleLeft;
                        nameText.rectTransform.anchorMin = new Vector2(0f, 0f);
                        nameText.rectTransform.anchorMax = new Vector2(0.6f, 1f);
                        nameText.rectTransform.offsetMin = Vector2.zero;
                        nameText.rectTransform.offsetMax = Vector2.zero;
                        nameText.text = semesterName;

                        var gradeText = CreateText("Grade", row, font, 26, FontStyle.Bold, gradeColor);
                        gradeText.alignment = TextAnchor.MiddleRight;
                        gradeText.rectTransform.anchorMin = new Vector2(0.6f, 0f);
                        gradeText.rectTransform.anchorMax = new Vector2(1f, 1f);
                        gradeText.rectTransform.offsetMin = Vector2.zero;
                        gradeText.rectTransform.offsetMax = Vector2.zero;
                        gradeText.text = grade;
                    }
                }
            }

            // 随机高中学期图片
            var (semSprite, semComment) = LoadRandomSemesterImage(state);
            if (semesterImage != null)
            {
                if (semSprite != null)
                {
                    semesterImage.sprite = semSprite;
                    semesterImage.color = Color.white;
                    semesterImage.gameObject.SetActive(true);
                }
                else
                {
                    semesterImage.gameObject.SetActive(false);
                }
            }
            if (semesterImageComment != null)
            {
                semesterImageComment.text = semComment;
            }

            // 随机大学图片
            var (uniSprite, uniComment) = LoadRandomUniversityImage(state);
            if (universityImage != null)
            {
                if (uniSprite != null)
                {
                    universityImage.sprite = uniSprite;
                    universityImage.color = Color.white;
                    universityImage.gameObject.SetActive(true);
                }
                else
                {
                    universityImage.gameObject.SetActive(false);
                }
            }
            if (universityImageComment != null)
            {
                universityImageComment.text = uniComment;
            }

            // 结局
            var endingType = DetermineEnding(state);
            var endingSprite = LoadEndingSprite(state);
            var (endingTitleStr, endingDescStr, isGoodEnding) = GetEndingInfo(endingType, state);

            if (endingImage != null)
            {
                if (endingSprite != null)
                {
                    endingImage.sprite = endingSprite;
                    endingImage.color = Color.white;
                    endingImage.gameObject.SetActive(true);
                }
                else
                {
                    endingImage.gameObject.SetActive(false);
                }
            }
            if (endingTitle != null)
            {
                endingTitle.text = endingTitleStr;
                endingTitle.color = isGoodEnding ? UITheme.Gold : UITheme.FromHex("CC6666");
            }
            if (endingDesc != null)
            {
                endingDesc.text = endingDescStr;
            }

            // 最终能力
            if (statIntelligenceText != null) statIntelligenceText.text = $"学习能力：{BuildMiniBar(state.StatIntelligence)}";
            if (statPsychologyText != null) statPsychologyText.text = $"情绪管理：{BuildMiniBar(state.StatPsychology)}";
            if (statSocialText != null) statSocialText.text = $"人际关系：{BuildMiniBar(state.StatSocial)}";
            if (statHealthText != null) statHealthText.text = $"健康状态：{BuildMiniBar(state.StatHealth)}";

            // 人生总结（第一人称）
            if (lifeSummaryText != null)
            {
                var intDesc = state.StatIntelligence >= 80 ? "学习能力出众" :
                    state.StatIntelligence >= 50 ? "学习能力尚可" : "学习之路虽不轻松，但我从未放弃";
                var psyDesc = state.StatPsychology >= 80 ? "心态始终稳如磐石" :
                    state.StatPsychology >= 50 ? "情绪管理得当" : "在情绪的起伏中学会了与自己和解";
                var socDesc = state.StatSocial >= 80 ? "身边总是围着朋友" :
                    state.StatSocial >= 50 ? "有着不错的人缘" : "在人际交往中学会了独处与陪伴的平衡";
                var healthDesc = state.StatHealth >= 80 ? "身体一直倍儿棒" :
                    state.StatHealth >= 50 ? "身体还算不错" : "虽然身体底子不算好，但一直在坚持";

                var firstSubject = GetFirstLabel(state.FirstSubject);

                lifeSummaryText.text =
                    $"2026年，我从{province}的{familyLabel}家庭出发，开始了高中生活。\n\n" +
                    $"三年的时光里，我在{firstSubject}的道路上一步步前行。\n" +
                    $"从高一上的青涩到高三下的坚定，我用汗水浇灌出了属于自己的答案。\n\n" +
                    $"{intDesc}，{psyDesc}，{socDesc}，{healthDesc}。\n\n" +
                    $"人生没有标准答案，但每一步都算数。\n" +
                    $"这是我的故事，独一无二。";
            }
        }

        private static string BuildMiniBar(int value)
        {
            var max = 10;
            var filled = Mathf.Clamp(value / 10, 0, max);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < max; i++)
                sb.Append(i < filled ? "■" : "□");
            sb.Append($" {value}");
            return sb.ToString();
        }

        private void BindEvents()
        {
            if (backToHomeButton != null)
            {
                backToHomeButton.onClick.RemoveAllListeners();
                backToHomeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        private void RestartGame()
        {
            var state = GameState.Instance;
            if (state != null)
            {
                state.ResetState();
            }

            NavigateTo(ScreenType.Launch, false);
        }

        private void EnsureRuntimeLayout()
        {
            if (backToHomeButton != null && restartButton != null && titleText != null && subtitleText != null)
                return;

            BuildRuntimeLayout();
        }

        private void BuildRuntimeLayout()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = (RectTransform)transform;
            Stretch(root);

            // Background
            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            var bgSprite = RuntimeArt.LoadBg("bg_ending");
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.type = Image.Type.Simple;
                bgImage.color = Color.white;
            }
            else
            {
                bgImage.color = new Color32(40, 30, 50, 255);
            }

            // Panel
            var panel = CreateUiObject("Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(1f, 1f, 1f, 0.55f);
            panel.gameObject.AddComponent<UiAutoRounded>();
            var panelShadow = panel.gameObject.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            panelShadow.effectDistance = new Vector2(0f, -10f);

            // ScrollRect
            var scrollRoot = CreateUiObject("ScrollRoot", panel);
            Stretch(scrollRoot);

            summaryScrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            summaryScrollRect.horizontal = false;
            summaryScrollRect.vertical = true;
            summaryScrollRect.movementType = ScrollRect.MovementType.Clamped;
            summaryScrollRect.scrollSensitivity = 30f;

            var viewport = CreateUiObject("Viewport", scrollRoot);
            Stretch(viewport);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUiObject("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.spacing = 14f;
            contentLayout.padding = new RectOffset(16, 16, 16, 16);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            summaryScrollRect.content = content;
            summaryScrollRect.viewport = viewport;

            // Block 1: Title
            BuildBlockTitle(content, font);

            // Block 2: Player Info
            BuildBlockInfo(content, font);

            // Block 3: High School Grades + Image
            BuildBlockHighSchool(content, font);

            // Block 4: University Image
            BuildBlockUniversity(content, font);

            // Block 5: Ending
            BuildBlockEnding(content, font);

            // Block 6: Final Stats
            BuildBlockStats(content, font);

            // Block 7: Life Summary
            BuildBlockLifeSummary(content, font);

            // Block 8: Buttons
            BuildBlockButtons(content, font);
        }

        private void BuildBlockTitle(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Title", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 100f;

            titleText = CreateText("Title", block, font, 60, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.50f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            subtitleText = CreateText("Subtitle", block, font, 32, FontStyle.Normal, UITheme.TextSoft);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.06f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.48f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockInfo(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Info", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 80f;

            var card = CreateUiObject("Card", block);
            card.anchorMin = new Vector2(0.04f, 0f);
            card.anchorMax = new Vector2(0.96f, 1f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();

            infoCardText = CreateText("InfoText", card, font, 26, FontStyle.Normal, UITheme.Text);
            infoCardText.alignment = TextAnchor.MiddleCenter;
            infoCardText.rectTransform.anchorMin = new Vector2(0.06f, 0f);
            infoCardText.rectTransform.anchorMax = new Vector2(0.94f, 1f);
            infoCardText.rectTransform.offsetMin = Vector2.zero;
            infoCardText.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockHighSchool(Transform parent, Font font)
        {
            // 高中成绩 + 随机图片
            var block = CreateUiObject("Block_HighSchool", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 340f;

            // 左侧：成绩列表
            var leftPanel = CreateUiObject("LeftPanel", block);
            leftPanel.anchorMin = new Vector2(0f, 0f);
            leftPanel.anchorMax = new Vector2(0.48f, 1f);
            leftPanel.offsetMin = Vector2.zero;
            leftPanel.offsetMax = Vector2.zero;

            var leftCard = CreateUiObject("Card", leftPanel);
            leftCard.anchorMin = new Vector2(0f, 0f);
            leftCard.anchorMax = new Vector2(1f, 1f);
            leftCard.offsetMin = Vector2.zero;
            leftCard.offsetMax = Vector2.zero;
            var leftCardBg = leftCard.gameObject.AddComponent<Image>();
            leftCardBg.color = Color.white;
            leftCard.gameObject.AddComponent<UiAutoRounded>();

            gradeListTitle = CreateText("GradeTitle", leftCard, font, 32, FontStyle.Bold, UITheme.Text);
            gradeListTitle.alignment = TextAnchor.MiddleLeft;
            gradeListTitle.rectTransform.anchorMin = new Vector2(0.08f, 0.86f);
            gradeListTitle.rectTransform.anchorMax = new Vector2(0.92f, 0.98f);
            gradeListTitle.rectTransform.offsetMin = Vector2.zero;
            gradeListTitle.rectTransform.offsetMax = Vector2.zero;

            gradeListContainer = CreateUiObject("GradeList", leftCard);
            gradeListContainer.anchorMin = new Vector2(0.08f, 0.06f);
            gradeListContainer.anchorMax = new Vector2(0.92f, 0.84f);
            gradeListContainer.offsetMin = Vector2.zero;
            gradeListContainer.offsetMax = Vector2.zero;
            var gradesVlg = gradeListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            gradesVlg.childControlHeight = true;
            gradesVlg.childControlWidth = true;
            gradesVlg.childForceExpandHeight = false;
            gradesVlg.childForceExpandWidth = true;
            gradesVlg.spacing = 2f;

            // 右侧：随机高中图片
            var rightPanel = CreateUiObject("RightPanel", block);
            rightPanel.anchorMin = new Vector2(0.52f, 0f);
            rightPanel.anchorMax = new Vector2(1f, 1f);
            rightPanel.offsetMin = Vector2.zero;
            rightPanel.offsetMax = Vector2.zero;

            var rightCard = CreateUiObject("Card", rightPanel);
            rightCard.anchorMin = new Vector2(0f, 0f);
            rightCard.anchorMax = new Vector2(1f, 1f);
            rightCard.offsetMin = Vector2.zero;
            rightCard.offsetMax = Vector2.zero;
            var rightCardBg = rightCard.gameObject.AddComponent<Image>();
            rightCardBg.color = Color.white;
            rightCard.gameObject.AddComponent<UiAutoRounded>();

            var imgObj = CreateUiObject("SemesterImage", rightCard);
            imgObj.anchorMin = new Vector2(0.06f, 0.22f);
            imgObj.anchorMax = new Vector2(0.94f, 0.94f);
            imgObj.offsetMin = Vector2.zero;
            imgObj.offsetMax = Vector2.zero;
            semesterImage = imgObj.gameObject.AddComponent<Image>();
            semesterImage.preserveAspect = true;

            semesterImageComment = CreateText("Comment", rightCard, font, 22, FontStyle.Italic, UITheme.TextSoft);
            semesterImageComment.alignment = TextAnchor.MiddleCenter;
            semesterImageComment.rectTransform.anchorMin = new Vector2(0.06f, 0.04f);
            semesterImageComment.rectTransform.anchorMax = new Vector2(0.94f, 0.20f);
            semesterImageComment.rectTransform.offsetMin = Vector2.zero;
            semesterImageComment.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockUniversity(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_University", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 340f;

            var card = CreateUiObject("Card", block);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(1f, 1f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();

            var title = CreateText("Title", card, font, 32, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.MiddleLeft;
            title.text = "大学时光";
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.88f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            var imgObj = CreateUiObject("UniversityImage", card);
            imgObj.anchorMin = new Vector2(0.06f, 0.22f);
            imgObj.anchorMax = new Vector2(0.94f, 0.86f);
            imgObj.offsetMin = Vector2.zero;
            imgObj.offsetMax = Vector2.zero;
            universityImage = imgObj.gameObject.AddComponent<Image>();
            universityImage.preserveAspect = true;

            universityImageComment = CreateText("Comment", card, font, 22, FontStyle.Italic, UITheme.TextSoft);
            universityImageComment.alignment = TextAnchor.MiddleCenter;
            universityImageComment.rectTransform.anchorMin = new Vector2(0.06f, 0.04f);
            universityImageComment.rectTransform.anchorMax = new Vector2(0.94f, 0.20f);
            universityImageComment.rectTransform.offsetMin = Vector2.zero;
            universityImageComment.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockEnding(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Ending", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 380f;

            var card = CreateUiObject("Card", block);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(1f, 1f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();

            endingTitle = CreateText("EndingTitle", card, font, 36, FontStyle.Bold, UITheme.Gold);
            endingTitle.alignment = TextAnchor.MiddleCenter;
            endingTitle.text = "人生结局";
            endingTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.88f);
            endingTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            endingTitle.rectTransform.offsetMin = Vector2.zero;
            endingTitle.rectTransform.offsetMax = Vector2.zero;

            var imgObj = CreateUiObject("EndingImage", card);
            imgObj.anchorMin = new Vector2(0.06f, 0.38f);
            imgObj.anchorMax = new Vector2(0.94f, 0.86f);
            imgObj.offsetMin = Vector2.zero;
            imgObj.offsetMax = Vector2.zero;
            endingImage = imgObj.gameObject.AddComponent<Image>();
            endingImage.preserveAspect = true;

            endingDesc = CreateText("EndingDesc", card, font, 24, FontStyle.Normal, UITheme.TextSoft);
            endingDesc.alignment = TextAnchor.MiddleCenter;
            endingDesc.rectTransform.anchorMin = new Vector2(0.06f, 0.04f);
            endingDesc.rectTransform.anchorMax = new Vector2(0.94f, 0.36f);
            endingDesc.rectTransform.offsetMin = Vector2.zero;
            endingDesc.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockStats(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Stats", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 160f;

            var card = CreateUiObject("Card", block);
            card.anchorMin = new Vector2(0f, 0f);
            card.anchorMax = new Vector2(1f, 1f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();

            var title = CreateText("Title", card, font, 32, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.MiddleLeft;
            title.text = "最终能力";
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.82f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            statIntelligenceText = CreateText("S1", card, font, 24, FontStyle.Normal, UITheme.Text);
            statIntelligenceText.alignment = TextAnchor.MiddleLeft;
            statIntelligenceText.rectTransform.anchorMin = new Vector2(0.06f, 0.62f);
            statIntelligenceText.rectTransform.anchorMax = new Vector2(0.94f, 0.80f);
            statIntelligenceText.rectTransform.offsetMin = Vector2.zero;
            statIntelligenceText.rectTransform.offsetMax = Vector2.zero;

            statPsychologyText = CreateText("S2", card, font, 24, FontStyle.Normal, UITheme.Text);
            statPsychologyText.alignment = TextAnchor.MiddleLeft;
            statPsychologyText.rectTransform.anchorMin = new Vector2(0.06f, 0.42f);
            statPsychologyText.rectTransform.anchorMax = new Vector2(0.94f, 0.60f);
            statPsychologyText.rectTransform.offsetMin = Vector2.zero;
            statPsychologyText.rectTransform.offsetMax = Vector2.zero;

            statSocialText = CreateText("S3", card, font, 24, FontStyle.Normal, UITheme.Text);
            statSocialText.alignment = TextAnchor.MiddleLeft;
            statSocialText.rectTransform.anchorMin = new Vector2(0.06f, 0.22f);
            statSocialText.rectTransform.anchorMax = new Vector2(0.94f, 0.40f);
            statSocialText.rectTransform.offsetMin = Vector2.zero;
            statSocialText.rectTransform.offsetMax = Vector2.zero;

            statHealthText = CreateText("S4", card, font, 24, FontStyle.Normal, UITheme.Text);
            statHealthText.alignment = TextAnchor.MiddleLeft;
            statHealthText.rectTransform.anchorMin = new Vector2(0.06f, 0.02f);
            statHealthText.rectTransform.anchorMax = new Vector2(0.94f, 0.20f);
            statHealthText.rectTransform.offsetMin = Vector2.zero;
            statHealthText.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildBlockLifeSummary(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Summary", parent);
            var vlg = block.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            block.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = CreateText("SectionTitle", block, font, 32, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.MiddleLeft;
            title.text = "人生总结";
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 40f;

            lifeSummaryText = CreateText("SummaryText", block, font, 26, FontStyle.Normal, UITheme.TextSoft);
            lifeSummaryText.alignment = TextAnchor.UpperLeft;
            lifeSummaryText.horizontalOverflow = HorizontalWrapMode.Wrap;
            lifeSummaryText.verticalOverflow = VerticalWrapMode.Overflow;
            lifeSummaryText.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void BuildBlockButtons(Transform parent, Font font)
        {
            var block = CreateUiObject("Block_Buttons", parent);
            var blockLayout = block.gameObject.AddComponent<LayoutElement>();
            blockLayout.preferredHeight = 90f;

            backToHomeButton = CreateSmallButton("← 返回主界面", block, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backToHomeButton.transform;
            backRect.anchorMin = new Vector2(0.06f, 0.1f);
            backRect.anchorMax = new Vector2(0.48f, 0.9f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backToHomeButton.gameObject.AddComponent<UiPressScale>();

            restartButton = CreatePrimaryButton("重新开始", block, font, new Color32(255, 138, 128, 255), Color.white);
            var restartRect = (RectTransform)restartButton.transform;
            restartRect.anchorMin = new Vector2(0.52f, 0.1f);
            restartRect.anchorMax = new Vector2(0.94f, 0.9f);
            restartRect.offsetMin = Vector2.zero;
            restartRect.offsetMax = Vector2.zero;
            restartButton.gameObject.AddComponent<UiPressScale>();
        }

        private static string GetFamilyLabel(FamilyBackgroundType type)
        {
            switch (type)
            {
                case FamilyBackgroundType.Intellectual: return "书香门第";
                case FamilyBackgroundType.Business: return "经商家庭";
                case FamilyBackgroundType.Worker: return "工薪家庭";
                case FamilyBackgroundType.Rural: return "田园人家";
                case FamilyBackgroundType.CivilServant: return "公务员家庭";
                default: return "普通家庭";
            }
        }

        private static string GetFirstLabel(FirstSubject subject)
        {
            switch (subject)
            {
                case FirstSubject.Physics: return "物理";
                case FirstSubject.History: return "历史";
                default: return "未选择";
            }
        }

        private static string GetSecondLabel(SecondSubject subject)
        {
            switch (subject)
            {
                case SecondSubject.Politics: return "政治";
                case SecondSubject.Geography: return "地理";
                case SecondSubject.Chemistry: return "化学";
                case SecondSubject.Biology: return "生物";
                default: return "";
            }
        }

        private static (Sprite sprite, string comment) LoadRandomSemesterImage(GameState state)
        {
            if (state == null) return (null, "");

            var genderPrefix = state.Gender == PlayerGender.Female ? "女" : "男";
            var comments = new[]
            {
                "高一那年，第一次踏入高中校园，青涩而充满期待",
                "高一下，逐渐适应了节奏，开始找到自己的步调",
                "高二分科后，选择了方向，每一天都在为梦想努力",
                "高二下，越来越清楚自己想要什么",
                "高三的冲刺，书桌上堆满了试卷",
                "最后一个学期，拼尽全力，不留遗憾",
            };

            var count = Mathf.Min(state.SemesterGrades.Count, SemesterNames.Length);
            if (count <= 0) return (null, "");

            var idx = Random.Range(0, count);
            var path = $"UI/学期/{genderPrefix}/{SemesterNames[idx]}";
            var sprite = RuntimeArt.LoadSprite(path);
            return (sprite, comments[idx]);
        }

        private static (Sprite sprite, string comment) LoadRandomUniversityImage(GameState state)
        {
            if (state == null) return (null, "");

            var genderPrefix = state.Gender == PlayerGender.Female ? "女" : "男";
            var yearNames = new[] { "大一", "大二", "大三", "大四" };
            var comments = new[]
            {
                "大学第一年，遇见更广阔的世界",
                "大二，在社团和学业间找到平衡",
                "大三，开始为未来认真打算",
                "大四，站在十字路口，回望来路",
            };

            var idx = Random.Range(0, yearNames.Length);
            var path = $"UI/大学学期/{genderPrefix}/{yearNames[idx]}";
            var sprite = RuntimeArt.LoadSprite(path);
            return (sprite, comments[idx]);
        }

        private static Sprite LoadEndingSprite(GameState state)
        {
            if (state == null) return null;
            var genderPrefix = state.Gender == PlayerGender.Female ? "女" : "男";
            var endingType = DetermineEnding(state);
            return RuntimeArt.LoadSprite($"UI/结局/{genderPrefix}end_{endingType}");
        }

        private static string DetermineEnding(GameState state)
        {
            var intelligence = state.StatIntelligence;
            var psychology = state.StatPsychology;
            var social = state.StatSocial;
            var health = state.StatHealth;

            var maxStat = Mathf.Max(intelligence, psychology, social, health);
            var minStat = Mathf.Min(intelligence, psychology, social, health);

            // 均衡发展 → freedom 或 hardship
            if (maxStat - minStat <= 5)
            {
                return maxStat >= 60 ? "freedom" : "hardship";
            }

            if (intelligence >= maxStat) return "scholar";
            if (psychology >= maxStat) return "artist";
            if (social >= maxStat) return "civil_servant";
            if (health >= maxStat) return "doctor";

            return "programmer";
        }

        /// <summary>
        /// 返回 (结局标题, 结局描述, 是否好结局)
        /// </summary>
        private static (string title, string desc, bool isGood) GetEndingInfo(string endingType, GameState state)
        {
            var name = !string.IsNullOrWhiteSpace(state?.PlayerName) ? state.PlayerName : "我";

            switch (endingType)
            {
                case "scholar":
                    return ("学术之星", $"凭借优异的成绩和不懈的努力，{name}考入了顶尖学府，走上了学术研究的道路。\n学海无涯，但探索的脚步永不停歇。", true);
                case "artist":
                    return ("艺术人生", $"丰富的内心世界让{name}在艺术领域找到了归属。\n用画笔/音符表达自己，活出了独特的色彩。", true);
                case "civil_servant":
                    return ("人民公仆", $"出色的社交能力和责任感让{name}进入了公共服务领域。\n为人民服务，虽然辛苦但意义非凡。", true);
                case "doctor":
                    return ("白衣天使", $"健康的体魄和坚韧的意志，让{name}成为了一名医生。\n救死扶伤，是这世界上最伟大的职业之一。", true);
                case "freedom":
                    return ("自由之路", $"均衡发展的{name}选择了一条不被定义的道路。\n自由职业、旅行、探索——人生有无限可能。", true);
                case "business":
                    return ("商海沉浮", $"凭借敏锐的商业嗅觉，{name}在商界闯出了一片天地。\n但商场如战场，成功背后也有不为人知的艰辛。", true);
                case "hardship":
                    return ("艰难前行", $"高中三年并不轻松，{name}的每一步都走得格外艰难。\n但人生不止一种活法，跌倒了再爬起来，也是一种勇气。", false);
                case "programmer":
                    return ("代码人生", $"{name}成为了一名程序员，在0和1的世界里构建未来。\n加班是常态，但写出优雅代码的成就感无可替代。", false);
                default:
                    return ("未完待续", $"{name}的人生故事还在继续。每一次选择，都是新的篇章。", true);
            }
        }

        private static Color GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "A": return UITheme.Gold;
                case "B": return new Color32(76, 175, 80, 255);
                default: return UITheme.TextLight;
            }
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