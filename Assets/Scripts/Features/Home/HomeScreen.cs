using System.Collections.Generic;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Home
{
    public class HomeScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button helpButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text avatarBadgeText;
        [SerializeField] private Text avatarNameText;
        [SerializeField] private RectTransform buttonGroupRoot;
        [SerializeField] private Text semesterIndicator;
        [SerializeField] private RectTransform semesterReviewRoot;
        private RectTransform semesterScrollContent;

        private readonly List<Button> dynamicButtons = new List<Button>();
        private RectTransform holidayPopupRoot;

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
                if (state.CurrentProgress < GameProgress.Home)
                {
                    state.CurrentProgress = GameProgress.Home;
                }
                state.SaveGame();

                // 一周目引导提示
                if (state.CurrentPlaythrough == 1 && !state.HasSeenGuide("home_first"))
                {
                    state.MarkGuideSeen("home_first");
                    ShowToast("欢迎来到主界面！点击「继续主线」推进学期，学期完成后可查看成绩评级～");
                }
            }

            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            RefreshSummary();
            RefreshContinueButton();
            RebuildButtons();
        }

        private void RefreshAvatar()
        {
            var state = GameState.Instance;
            if (state == null)
            {
                return;
            }

            if (avatarBadgeText != null)
            {
                avatarBadgeText.text = state.Gender == PlayerGender.Female ? "女" : "男";
                avatarBadgeText.color = state.Gender == PlayerGender.Female
                    ? UITheme.FromHex("AD5C7E")
                    : UITheme.FromHex("577EAB");
            }

            if (avatarNameText != null)
            {
                avatarNameText.text = string.IsNullOrWhiteSpace(state.PlayerName) ? "未命名" : state.PlayerName;
            }

            if (avatarImage != null)
            {
                avatarImage.color = state.Gender == PlayerGender.Female
                    ? UITheme.FromHex("FFF1F7")
                    : UITheme.FromHex("F1F8FF");
            }
        }

        private void BindEvents()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ContinueMainline);
            }
        }

        private void RefreshContinueButton()
        {
            if (continueButton == null)
            {
                return;
            }

            var state = GameState.Instance;
            if (state == null)
            {
                continueButton.interactable = false;
                return;
            }

            // 学期全部完成后 → 决胜高考
            if (state.CurrentProgress == GameProgress.Semester && state.SemesterIndex >= Mathf.Max(1, state.TotalSemesters))
            {
                continueButton.interactable = true;
                var label = continueButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "决胜高考";
                }
                return;
            }

            var next = GetMainlineNext(state);
            continueButton.interactable = next != ScreenType.Home;

            var lbl = continueButton.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.text = $"继续主线 · {GetMainlineLabel(next)}";
            }
        }

        private void ContinueMainline()
        {
            var state = GameState.Instance;
            if (state == null)
            {
                return;
            }

            // 学期全部完成后 → 高考
            if (state.CurrentProgress == GameProgress.Semester && state.SemesterIndex >= Mathf.Max(1, state.TotalSemesters))
            {
                NavigateTo(ScreenType.Gaokao, true);
                return;
            }

            var next = GetMainlineNext(state);
            if (next == ScreenType.Home)
            {
                return;
            }

            NavigateTo(next, true);
        }

        private void RefreshSummary()
        {
            if (summaryText == null)
            {
                return;
            }

            var state = GameState.Instance;
            if (state == null)
            {
                summaryText.text = "未找到游戏状态";
                return;
            }

            var province = string.IsNullOrWhiteSpace(state.SelectedProvince) ? "未选择省份" : state.SelectedProvince;
            var family = state.SelectedFamily == FamilyBackgroundType.None ? "未选择家庭" : GetFamilyLabel(state.SelectedFamily);
            var subjectsReady = state.FirstSubject != FirstSubject.None && state.SecondSubjects != null && state.SecondSubjects.Count == 2;
            var subjects = subjectsReady ? $"{state.FirstSubject} + {state.SecondSubjects[0]}/{state.SecondSubjects[1]}" : "未完成选科";

            summaryText.text = "";
            RefreshSemesterInfo();
            RefreshAtmosphere();
        }

        private void RefreshAtmosphere()
        {
            if (subtitleText == null) return;
            var state = GameState.Instance;
            if (state == null) return;

            var playerName = !string.IsNullOrWhiteSpace(state.PlayerName) ? state.PlayerName : "同学";

            if (state.CurrentProgress >= GameProgress.Gaokao)
            {
                subtitleText.text = "所有的努力，都将在这一刻绽放";
            }
            else if (state.CurrentProgress >= GameProgress.Semester)
            {
                var labels = new[] { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };
                var idx = Mathf.Clamp(state.SemesterIndex, 0, labels.Length - 1);
                var stageQuotes = new[]
                {
                    "新的篇章，从第一个学期开始书写",
                    "适应了节奏，你变得更加从容",
                    "分科后的挑战，正是成长的契机",
                    "高二的关键时刻，每一步都算数",
                    "高三的冲刺，梦想触手可及",
                    "最后一学期，全力以赴不留遗憾",
                };
                subtitleText.text = stageQuotes[idx];
            }
            else
            {
                subtitleText.text = $"欢迎回来，{playerName}";
            }
        }

        private void RefreshSemesterInfo()
        {
            var state = GameState.Instance;
            if (state == null) return;

            if (semesterIndicator != null)
            {
                var labels = new[] { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };
                var emojis = new[] { "📖", "🔬", "📐", "🌍", "📝", "🎯" };
                var idx = state.SemesterIndex;
                var name = idx < labels.Length ? labels[idx] : $"第{idx + 1}学期";
                var emoji = idx < emojis.Length ? emojis[idx] : "📚";

                if (state.CurrentProgress >= GameProgress.Gaokao)
                {
                    semesterIndicator.text = "🎓 学期完成，准备高考";
                }
                else if (state.CurrentProgress >= GameProgress.Semester)
                {
                    semesterIndicator.text = $"{emoji} {name}";
                }
                else
                {
                    semesterIndicator.text = $"{emojis[0]} {labels[0]} · 即将开始";
                }
            }

            RebuildSemesterCards();
        }

        private void RebuildSemesterCards()
        {
            if (semesterScrollContent == null) return;

            for (int i = semesterScrollContent.childCount - 1; i >= 0; i--)
            {
                Destroy(semesterScrollContent.GetChild(i).gameObject);
            }

            var state = GameState.Instance;
            if (state == null) return;

            var total = Mathf.Max(1, state.TotalSemesters);
            var currentIdx = state.SemesterIndex;
            var font = BuiltinFont();
            var labels = new[] { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };
            var emojis = new[] { "📖", "🔬", "📐", "🌍", "📝", "🎯" };
            var genderPrefix = state.Gender == PlayerGender.Female ? "女" : "男";

            // 显示所有学期（已完成+当前），高考后显示全部
            var showCount = state.CurrentProgress >= GameProgress.Gaokao ? total : Mathf.Min(currentIdx + 1, total);
            if (showCount <= 0) showCount = 1;

            for (int i = 0; i < showCount; i++)
            {
                var isCompleted = i < currentIdx;
                var isCurrent = i == currentIdx;
                var label = i < labels.Length ? labels[i] : $"第{i + 1}学期";
                var emoji = i < emojis.Length ? emojis[i] : "📚";
                var grade = isCompleted && i < state.SemesterGrades.Count ? state.SemesterGrades[i] : "";

                BuildSemesterCard(semesterScrollContent, font, i, label, emoji, grade, isCurrent, isCompleted, genderPrefix);
            }
        }

        private void BuildSemesterCard(Transform parent, Font font, int index, string label, string emoji, string grade, bool isCurrent, bool isCompleted, string genderPrefix)
        {
            var card = CreateUiObject($"SemCard_{index}", parent);
            var cardLayout = card.gameObject.AddComponent<LayoutElement>();
            cardLayout.preferredWidth = 280f;

            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = isCurrent ? Color.white : new Color(1f, 1f, 1f, 0.7f);
            RuntimeArt.ApplyRounded(cardBg);
            var cardShadow = card.gameObject.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            cardShadow.effectDistance = new Vector2(0f, -6f);

            // 插画区域
            var illObj = CreateUiObject("Illustration", card);
            illObj.anchorMin = new Vector2(0.06f, 0.22f);
            illObj.anchorMax = new Vector2(0.94f, 0.96f);
            illObj.offsetMin = Vector2.zero;
            illObj.offsetMax = Vector2.zero;
            var illImage = illObj.gameObject.AddComponent<Image>();
            illImage.color = new Color(1f, 1f, 1f, 0f);
            illImage.preserveAspect = true;

            var illLabelNames = new[] { "高一上", "高一下", "高二上", "高二下", "高三上", "高三下" };
            var illName = index < illLabelNames.Length ? illLabelNames[index] : $"学期{index + 1}";
            var sprite = RuntimeArt.LoadSprite($"UI/学期/{genderPrefix}/{illName}");
            if (sprite == null) sprite = RuntimeArt.LoadSprite($"UI/学期/{illName}");
            if (sprite != null)
            {
                illImage.sprite = sprite;
                illImage.color = Color.white;
            }

            var illEmoji = CreateText("IllEmoji", card, font, 72, FontStyle.Normal, UITheme.TextLight);
            illEmoji.alignment = TextAnchor.MiddleCenter;
            illEmoji.rectTransform.anchorMin = new Vector2(0.06f, 0.22f);
            illEmoji.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
            illEmoji.rectTransform.offsetMin = Vector2.zero;
            illEmoji.rectTransform.offsetMax = Vector2.zero;
            illEmoji.text = sprite != null ? "" : emoji;

            // 学期名称
            var nameText = CreateText("SemName", card, font, 28, isCurrent ? FontStyle.Bold : FontStyle.Normal, isCurrent ? UITheme.Text : UITheme.TextLight);
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.rectTransform.anchorMin = new Vector2(0.04f, 0.06f);
            nameText.rectTransform.anchorMax = new Vector2(0.96f, 0.22f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;
            nameText.text = label;

            // 成绩扣章（已完成学期）
            if (isCompleted && !string.IsNullOrEmpty(grade))
            {
                var stampObj = CreateUiObject("GradeStamp", card);
                stampObj.anchorMin = new Vector2(0.68f, 0.72f);
                stampObj.anchorMax = new Vector2(0.96f, 0.96f);
                stampObj.offsetMin = Vector2.zero;
                stampObj.offsetMax = Vector2.zero;
                var stampBg = stampObj.gameObject.AddComponent<Image>();
                RuntimeArt.ApplyRounded(stampBg);
                stampBg.color = GetGradeStampColor(grade);

                var stampText = CreateText("StampText", stampObj, font, 40, FontStyle.Bold, Color.white);
                Stretch(stampText.rectTransform);
                stampText.alignment = TextAnchor.MiddleCenter;
                stampText.text = grade;

                // 旋转效果，模拟扣章
                stampObj.localRotation = Quaternion.Euler(0f, 0f, -12f);
            }

            // 当前学期标签
            if (isCurrent)
            {
                var currentTag = CreateUiObject("CurrentTag", card);
                currentTag.anchorMin = new Vector2(0.06f, 0.72f);
                currentTag.anchorMax = new Vector2(0.34f, 0.96f);
                currentTag.offsetMin = Vector2.zero;
                currentTag.offsetMax = Vector2.zero;
                var tagBg = currentTag.gameObject.AddComponent<Image>();
                tagBg.color = UITheme.Confirm;
                tagBg.gameObject.AddComponent<UiAutoRounded>();
                var tagText = CreateText("TagText", currentTag, font, 22, FontStyle.Bold, Color.white);
                Stretch(tagText.rectTransform);
                tagText.alignment = TextAnchor.MiddleCenter;
                tagText.text = "当前";
            }
        }

        private static Color GetGradeStampColor(string grade)
        {
            switch (grade)
            {
                case "A": return UITheme.Accent;
                case "B": return new Color32(76, 175, 80, 255);
                default: return new Color32(158, 158, 158, 255);
            }
        }

        private string GetSemesterGrade(int semesterIndex)
        {
            var state = GameState.Instance;
            if (state == null) return "C";

            var grades = state.SemesterGrades;
            if (grades != null && semesterIndex < grades.Count)
            {
                return grades[semesterIndex];
            }

            var seed = semesterIndex * 31 + (int)(state.StatIntelligence * 7) + (int)(state.StatPsychology * 3);
            var gradePool = new[] { "A", "A", "B", "B", "B", "C", "C" };
            var grade = gradePool[Mathf.Abs(seed) % gradePool.Length];
            return grade;
        }

        private static Color GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "A": return new Color32(255, 183, 77, 255);
                case "B": return new Color32(129, 199, 132, 255);
                case "C": return new Color32(144, 164, 174, 255);
                default: return UITheme.TextSoft;
            }
        }

        private void RebuildButtons()
        {
            if (buttonGroupRoot == null)
            {
                return;
            }

            for (int i = 0; i < dynamicButtons.Count; i++)
            {
                var btn = dynamicButtons[i];
                if (btn != null)
                {
                    Destroy(btn.gameObject);
                }
            }

            dynamicButtons.Clear();

            var state = GameState.Instance;
            var unlocked = state != null ? state.GetUnlockedButtons() : new List<HomeButtonType>();

            // 设置/规则/成就/高考/志愿改为右上角或主线入口，不在底部按钮区显示
            for (int i = unlocked.Count - 1; i >= 0; i--)
            {
                if (unlocked[i] == HomeButtonType.Settings || unlocked[i] == HomeButtonType.Rules || unlocked[i] == HomeButtonType.Achievements
                    || unlocked[i] == HomeButtonType.Gaokao || unlocked[i] == HomeButtonType.Volunteer
                    || unlocked[i] == HomeButtonType.Semester)
                {
                    unlocked.RemoveAt(i);
                }
            }

            for (int i = 0; i < unlocked.Count; i++)
            {
                var buttonType = unlocked[i];
                var label = GetButtonLabel(buttonType);
                var button = CreatePrimaryButton(label, buttonGroupRoot, BuiltinFont(), UITheme.Confirm, UITheme.Text);
                button.gameObject.AddComponent<UiPressScale>();

                // Activity 按钮：放假时可进入活动中心，学期中灰显
                if (buttonType == HomeButtonType.Activity)
                {
                    var total = state != null ? Mathf.Max(1, state.TotalSemesters) : 6;
                    var isVacation = state != null && state.CurrentProgress == GameProgress.Semester && state.SemesterIndex > 0 && state.SemesterIndex < total;
                    button.interactable = isVacation;
                    if (!isVacation)
                    {
                        if (state != null && state.CurrentProgress >= GameProgress.Gaokao)
                        {
                            button.onClick.AddListener(() => ShowToast("高考已过，假期活动不再开放"));
                        }
                        else
                        {
                            button.onClick.AddListener(() => ShowToast("学期中暂不开放，放假后可进入"));
                        }
                    }
                    else
                    {
                        button.onClick.AddListener(() => ShowHolidayPopup());
                    }
                    dynamicButtons.Add(button);
                    continue;
                }

                // University / Career 按钮：未完成志愿时灰显
                if (buttonType == HomeButtonType.University)
                {
                    if (state != null && state.CurrentProgress < GameProgress.Volunteer)
                    {
                        button.interactable = false;
                        button.onClick.AddListener(() => ShowToast("完成志愿抉择后开启大学生活"));
                        dynamicButtons.Add(button);
                        continue;
                    }
                }
                if (buttonType == HomeButtonType.Career)
                {
                    // 人生启程：大学毕业后开启
                    if (state != null && state.CurrentProgress < GameProgress.University)
                    {
                        button.interactable = false;
                        button.onClick.AddListener(() => ShowToast("大学毕业后开启人生篇章"));
                        dynamicButtons.Add(button);
                        continue;
                    }
                }

                button.onClick.AddListener(() => OnButtonClicked(buttonType));
                dynamicButtons.Add(button);
            }
        }

        private void OnButtonClicked(HomeButtonType buttonType)
        {
            switch (buttonType)
            {
                case HomeButtonType.TalentTree:
                    NavigateTo(ScreenType.TalentTree, true);
                    return;
                case HomeButtonType.Semester:
                    NavigateTo(ScreenType.Semester, true);
                    return;
                case HomeButtonType.Gaokao:
                    NavigateTo(ScreenType.Gaokao, true);
                    return;
                case HomeButtonType.Volunteer:
                    NavigateTo(ScreenType.Volunteer, true);
                    return;
                case HomeButtonType.University:
                    NavigateTo(ScreenType.University, true);
                    return;
                case HomeButtonType.Equipment:
                    NavigateTo(ScreenType.Shop, false);
                    return;
                case HomeButtonType.Career:
                    NavigateTo(ScreenType.Career, true);
                    break;
                case HomeButtonType.StudentProfile:
                    NavigateTo(ScreenType.PlayerInfo, false);
                    break;
                case HomeButtonType.Activity:
                    ShowHolidayPopup();
                    return;
                default:
                    ShowToast("该功能暂未接入界面");
                    return;
            }
        }

        private void ShowHolidayPopup()
        {
            if (holidayPopupRoot == null)
            {
                holidayPopupRoot = BuildHolidayPopup();
            }

            if (holidayPopupRoot != null)
            {
                holidayPopupRoot.gameObject.SetActive(true);
            }
        }

        private RectTransform BuildHolidayPopup()
        {
            var parent = transform.Find("Panel") as RectTransform;
            if (parent == null)
            {
                parent = transform as RectTransform;
            }

            if (parent == null)
            {
                return null;
            }

            var font = BuiltinFont();
            var overlay = CreateUiObject("HolidayPopup", parent);
            Stretch(overlay);
            var overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.35f);

            var card = CreateUiObject("Card", overlay);
            card.anchorMin = new Vector2(0.08f, 0.24f);
            card.anchorMax = new Vector2(0.92f, 0.76f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = Color.white;
            RuntimeArt.ApplyRounded(cardImage);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            shadow.effectDistance = new Vector2(0f, -16f);

            var title = CreateText("Title", card, font, 62, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.UpperCenter;
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.70f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.text = "放假活动（开发中）";

            var body = CreateText("Body", card, font, 40, FontStyle.Normal, UITheme.TextSoft);
            body.alignment = TextAnchor.UpperLeft;
            body.rectTransform.anchorMin = new Vector2(0.08f, 0.24f);
            body.rectTransform.anchorMax = new Vector2(0.92f, 0.70f);
            body.rectTransform.offsetMin = Vector2.zero;
            body.rectTransform.offsetMax = Vector2.zero;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.text = "这里用于接小游戏或线性支线任务，让升学过程更有“玩中学”的节奏：\n\n- 打工攒钱 / 购物\n- 社交事件（同学/老师）\n- 短测挑战（知识点+小奖励）\n- 实践任务（项目/竞赛）\n\n完成后继续下一学期。";

            var close = CreatePrimaryButton("返回主界面", card, font, UITheme.Confirm, Color.white);
            close.gameObject.AddComponent<UiPressScale>();
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = new Vector2(0.12f, 0.08f);
            closeRect.anchorMax = new Vector2(0.88f, 0.20f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            close.onClick.AddListener(() => overlay.gameObject.SetActive(false));

            overlay.gameObject.SetActive(false);
            return overlay;
        }

        private void ShowRulesHelp()
        {
            ShowToast("玩法百科开发中，后续开放");
        }

        private static string GetButtonLabel(HomeButtonType buttonType)
        {
            switch (buttonType)
            {
                case HomeButtonType.TalentTree: return "成长赋能";
                case HomeButtonType.Semester: return "校园日常";
                case HomeButtonType.Equipment: return "商城";
                case HomeButtonType.Gaokao: return "决胜高考";
                case HomeButtonType.Volunteer: return "志愿抉择";
                case HomeButtonType.University: return "大学时光";
                case HomeButtonType.Career: return "人生启程";
                case HomeButtonType.Achievements: return "成就殿堂";
                case HomeButtonType.Settings: return "设置";
                case HomeButtonType.Rules: return "玩法百科";
                case HomeButtonType.Activity: return "活动中心";
                case HomeButtonType.StudentProfile: return "学生档案";
                default: return "功能";
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (continueButton != null && titleText != null && subtitleText != null && summaryText != null && buttonGroupRoot != null && semesterIndicator != null && semesterReviewRoot != null)
            {
                return;
            }

            BuildRuntimeLayout();
        }

        private void BuildRuntimeLayout()
        {
            var font = BuiltinFont();
            var root = (RectTransform)transform;
            Stretch(root);

            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            var bgSprite = RuntimeArt.LoadBg("bg_home");
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

            var panelBg = panel.gameObject.AddComponent<Image>();
            panelBg.color = Color.white;
            panel.gameObject.AddComponent<UiAutoRounded>();

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.74f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 72, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.44f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.90f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            subtitleText = CreateText("Subtitle", header, font, 34, FontStyle.Normal, UITheme.TextLight);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.18f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.48f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;

            summaryText = CreateText("Summary", header, font, 28, FontStyle.Normal, UITheme.TextSoft);
            summaryText.alignment = TextAnchor.MiddleCenter;
            summaryText.rectTransform.anchorMin = new Vector2(0.08f, -0.02f);
            summaryText.rectTransform.anchorMax = new Vector2(0.92f, 0.10f);
            summaryText.rectTransform.offsetMin = Vector2.zero;
            summaryText.rectTransform.offsetMax = Vector2.zero;
            summaryText.text = "";

            var topSettings = CreateSmallButton("?", header, font, UITheme.CardSky, UITheme.Text);
            var settingsRect = (RectTransform)topSettings.transform;
            settingsRect.anchorMin = new Vector2(0.84f, 0.72f);
            settingsRect.anchorMax = new Vector2(1f, 0.98f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;
            topSettings.gameObject.AddComponent<UiPressScale>();
            topSettings.onClick.AddListener(() => ShowRulesHelp());

            var topAchievements = CreateSmallButton("🏆", header, font, UITheme.FromHex("FFF8E1"), UITheme.Text);
            var achievementsRect = (RectTransform)topAchievements.transform;
            achievementsRect.anchorMin = new Vector2(0.56f, 0.72f);
            achievementsRect.anchorMax = new Vector2(0.69f, 0.98f);
            achievementsRect.offsetMin = Vector2.zero;
            achievementsRect.offsetMax = Vector2.zero;
            topAchievements.gameObject.AddComponent<UiPressScale>();
            topAchievements.onClick.AddListener(() => ShowToast("成就殿堂开发中，后续开放"));

            var topRules = CreateSmallButton("⚙", header, font, UITheme.FromHex("F0F4FF"), UITheme.Text);
            var rulesRect = (RectTransform)topRules.transform;
            rulesRect.anchorMin = new Vector2(0.70f, 0.72f);
            rulesRect.anchorMax = new Vector2(0.83f, 0.98f);
            rulesRect.offsetMin = Vector2.zero;
            rulesRect.offsetMax = Vector2.zero;
            topRules.gameObject.AddComponent<UiPressScale>();
            topRules.onClick.AddListener(() => ShowToast("设置功能开发中，后续开放"));

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.74f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            // 当前学期标题
            semesterIndicator = CreateText("SemesterTitle", body, font, 64, FontStyle.Bold, UITheme.FromHex("5C8BCF"));
            semesterIndicator.alignment = TextAnchor.MiddleCenter;
            semesterIndicator.rectTransform.anchorMin = new Vector2(0.06f, 0.72f);
            semesterIndicator.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            semesterIndicator.rectTransform.offsetMin = Vector2.zero;
            semesterIndicator.rectTransform.offsetMax = Vector2.zero;

            // 学期横向滑屏区域
            var scrollView = CreateUiObject("SemesterScrollView", body);
            scrollView.anchorMin = new Vector2(0f, 0.42f);
            scrollView.anchorMax = new Vector2(1f, 0.70f);
            scrollView.offsetMin = Vector2.zero;
            scrollView.offsetMax = Vector2.zero;

            var viewport = CreateUiObject("Viewport", scrollView);
            Stretch(viewport);
            var viewportMask = viewport.gameObject.AddComponent<Image>();
            viewportMask.color = new Color(0f, 0f, 0f, 0.01f);
            var viewportMask2D = viewport.gameObject.AddComponent<Mask>();
            viewportMask2D.showMaskGraphic = false;

            semesterScrollContent = CreateUiObject("SemesterContent", viewport);
            semesterScrollContent.anchorMin = new Vector2(0f, 0f);
            semesterScrollContent.anchorMax = new Vector2(0f, 1f);
            semesterScrollContent.pivot = new Vector2(0f, 0.5f);
            semesterScrollContent.anchoredPosition = new Vector2(20f, 0f);
            semesterScrollContent.sizeDelta = new Vector2(0f, 0f);
            var contentLayout = semesterScrollContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.MiddleLeft;
            contentLayout.spacing = 16f;
            contentLayout.padding = new RectOffset(0, 20, 0, 0);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;
            var contentFitter = semesterScrollContent.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.content = semesterScrollContent;
            scrollRect.viewport = viewport;
            scrollRect.scrollSensitivity = 20f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;

            // 已结束学期成绩标签
            semesterReviewRoot = CreateUiObject("SemesterReviewLabel", body);
            semesterReviewRoot.anchorMin = new Vector2(0.06f, 0.37f);
            semesterReviewRoot.anchorMax = new Vector2(0.94f, 0.41f);
            semesterReviewRoot.offsetMin = Vector2.zero;
            semesterReviewRoot.offsetMax = Vector2.zero;

            var reviewLabel = CreateText("ReviewLabel", semesterReviewRoot, font, 24, FontStyle.Normal, UITheme.FromHex("999999"));
            reviewLabel.alignment = TextAnchor.MiddleLeft;
            reviewLabel.text = "← 左右滑动查看已结束学期 →";

            var entryCard = CreateUiObject("EntryCard", body);
            entryCard.anchorMin = new Vector2(0.06f, 0.06f);
            entryCard.anchorMax = new Vector2(0.94f, 0.36f);
            entryCard.offsetMin = Vector2.zero;
            entryCard.offsetMax = Vector2.zero;
            var entryBg = entryCard.gameObject.AddComponent<Image>();
            entryBg.color = Color.white;
            RuntimeArt.ApplyRounded(entryBg);
            var entryShadow = entryCard.gameObject.AddComponent<Shadow>();
            entryShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            entryShadow.effectDistance = new Vector2(0f, -10f);

            continueButton = CreatePrimaryButton("继续主线", entryCard, font, UITheme.Confirm, UITheme.Text);
            continueButton.gameObject.AddComponent<UiPressScale>();
            var continueRect = (RectTransform)continueButton.transform;
            continueRect.anchorMin = new Vector2(0.04f, 0.62f);
            continueRect.anchorMax = new Vector2(0.96f, 0.96f);
            continueRect.offsetMin = Vector2.zero;
            continueRect.offsetMax = Vector2.zero;
            var continueLayout = continueButton.GetComponent<LayoutElement>();
            if (continueLayout != null)
            {
                continueLayout.preferredHeight = 160f;
            }

            buttonGroupRoot = CreateUiObject("Buttons", entryCard);
            buttonGroupRoot.anchorMin = new Vector2(0f, 0f);
            buttonGroupRoot.anchorMax = new Vector2(1f, 0.60f);
            buttonGroupRoot.offsetMin = Vector2.zero;
            buttonGroupRoot.offsetMax = Vector2.zero;

            var grid = buttonGroupRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(520f, 150f);
            grid.spacing = new Vector2(18f, 18f);
            grid.padding = new RectOffset(18, 18, 18, 18);
            grid.childAlignment = TextAnchor.UpperCenter;
        }

        private static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static string GetFamilyLabel(FamilyBackgroundType type)
        {
            switch (type)
            {
                case FamilyBackgroundType.Intellectual:
                    return "书香";
                case FamilyBackgroundType.Business:
                    return "经商";
                case FamilyBackgroundType.Worker:
                    return "工薪";
                case FamilyBackgroundType.Rural:
                    return "田园人家";
                case FamilyBackgroundType.CivilServant:
                    return "公务员";
                default:
                    return "未知";
            }
        }

        private static ScreenType GetMainlineNext(GameState state)
        {
            if (state == null)
            {
                return ScreenType.Home;
            }

            if (state.CurrentProgress < GameProgress.Profile)
            {
                return ScreenType.Profile;
            }

            if (state.CurrentProgress == GameProgress.Profile)
            {
                return ScreenType.Family;
            }

            if (state.CurrentProgress == GameProgress.Family)
            {
                return ScreenType.Province;
            }

            if (state.CurrentProgress == GameProgress.Province)
            {
                return ScreenType.Subject;
            }

            if (state.CurrentProgress == GameProgress.Subject)
            {
                return ScreenType.Home;
            }

            if (state.CurrentProgress < GameProgress.Semester)
            {
                return ScreenType.Semester;
            }

            if (state.CurrentProgress == GameProgress.Semester)
            {
                var total = Mathf.Max(1, state.TotalSemesters);
                if (state.SemesterIndex < total)
                {
                    return ScreenType.Semester;
                }

                return ScreenType.Gaokao;
            }

            if (state.CurrentProgress <= GameProgress.Gaokao)
            {
                return ScreenType.Gaokao;
            }

            if (state.CurrentProgress <= GameProgress.Volunteer)
            {
                return ScreenType.Volunteer;
            }

            if (state.CurrentProgress <= GameProgress.University)
            {
                return ScreenType.University;
            }

            if (state.CurrentProgress <= GameProgress.Career)
            {
                return ScreenType.Career;
            }

            if (state.CurrentProgress <= GameProgress.Summary)
            {
                return ScreenType.Summary;
            }

            return ScreenType.Summary;
        }

        private static string GetMainlineLabel(ScreenType next)
        {
            switch (next)
            {
                case ScreenType.Profile:
                    return "创建人物";
                case ScreenType.Family:
                    return "家庭背景";
                case ScreenType.Province:
                    return "选择省市";
                case ScreenType.Subject:
                    return "选科";
                case ScreenType.Semester:
                    return "推进学期";
                case ScreenType.Gaokao:
                    return "进入高考";
                case ScreenType.Volunteer:
                    return "填报志愿";
                case ScreenType.University:
                    return "进入大学";
                case ScreenType.Career:
                    return "毕业去向";
                case ScreenType.Summary:
                    return "人生总结";
                default:
                    return "继续";
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
            if (text != null) text.fontSize = Mathf.RoundToInt(40 * UiTextScale);
            var layout = btn.GetComponent<LayoutElement>();
            if (layout != null) layout.preferredHeight = 120f;
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

