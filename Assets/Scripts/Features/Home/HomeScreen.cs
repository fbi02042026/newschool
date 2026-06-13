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

            // 高考完成后 → 志愿抉择
            if (state.CurrentProgress == GameProgress.Gaokao)
            {
                continueButton.interactable = true;
                var label = continueButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = "志愿抉择";
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

            // 高考完成后 → 志愿
            if (state.CurrentProgress == GameProgress.Gaokao)
            {
                NavigateTo(ScreenType.Volunteer, true);
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

            summaryText.text = $"省份：{province}    家庭：{family}\n" +
                $"选科：{subjects}    金币：{state.Money}\n" +
                $"学习能力 {state.StatIntelligence}    情绪管理 {state.StatPsychology}    人际关系 {state.StatSocial}    健康状态 {state.StatHealth}";
            RefreshAvatar();
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
                    || unlocked[i] == HomeButtonType.Gaokao || unlocked[i] == HomeButtonType.Volunteer)
                {
                    unlocked.RemoveAt(i);
                }
            }

            // 假期时显示放假活动入口（非 Activity 按钮，是一个独立的弹出入口）
            if (state != null && state.CurrentProgress == GameProgress.Semester)
            {
                var total = Mathf.Max(1, state.TotalSemesters);
                var inVacation = state.SemesterIndex > 0 && state.SemesterIndex < total;
                if (inVacation)
                {
                    var holidayButton = CreatePrimaryButton("放假活动", buttonGroupRoot, BuiltinFont(), UITheme.Gold, UITheme.Text);
                    holidayButton.gameObject.AddComponent<UiPressScale>();
                    holidayButton.onClick.AddListener(ShowHolidayPopup);
                    dynamicButtons.Add(holidayButton);
                }
            }

            for (int i = 0; i < unlocked.Count; i++)
            {
                var buttonType = unlocked[i];
                var label = GetButtonLabel(buttonType);
                var button = CreatePrimaryButton(label, buttonGroupRoot, BuiltinFont(), UITheme.Confirm, UITheme.Text);
                button.gameObject.AddComponent<UiPressScale>();

                // Activity 按钮：非假期时灰显
                if (buttonType == HomeButtonType.Activity)
                {
                    var isVacation = state != null && state.SemesterIndex > 0 && state.SemesterIndex < Mathf.Max(1, state.TotalSemesters);
                    button.interactable = isVacation;
                    if (!isVacation)
                    {
                        button.onClick.AddListener(() => ShowToast("学期中暂不开放"));
                    }
                    else
                    {
                        button.onClick.AddListener(() => OnButtonClicked(buttonType));
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
                    ShowToast("活动中心开发中，后续开放");
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
            if (continueButton != null && titleText != null && subtitleText != null && summaryText != null && avatarImage != null && avatarBadgeText != null && avatarNameText != null && buttonGroupRoot != null)
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

            titleText = CreateText("Title", header, font, 84, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.36f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.88f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            subtitleText = CreateText("Subtitle", header, font, 42, FontStyle.Normal, UITheme.TextLight);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.06f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.42f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;

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

            var characterCard = CreateUiObject("CharacterCard", body);
            characterCard.anchorMin = new Vector2(0.06f, 0.44f);
            characterCard.anchorMax = new Vector2(0.94f, 0.98f);
            characterCard.offsetMin = Vector2.zero;
            characterCard.offsetMax = Vector2.zero;
            var characterBg = characterCard.gameObject.AddComponent<Image>();
            characterBg.color = Color.white;
            RuntimeArt.ApplyRounded(characterBg);
            var characterGradient = characterCard.gameObject.AddComponent<UiCornerGradient>();
            characterGradient.SetColors(UITheme.CardPeach, UITheme.CardSky, UITheme.CardLavender, UITheme.CardMint);
            var characterShadow = characterCard.gameObject.AddComponent<Shadow>();
            characterShadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            characterShadow.effectDistance = new Vector2(0f, -12f);

            var characterCardButton = characterCard.gameObject.AddComponent<Button>();
            characterCardButton.transition = Selectable.Transition.None;
            var characterCardButtonImage = characterCardButton.GetComponent<Image>();
            if (characterCardButtonImage != null)
            {
                characterCardButtonImage.color = new Color(1f, 1f, 1f, 0f);
            }
            characterCardButton.onClick.AddListener(() => NavigateTo(ScreenType.PlayerInfo, false));

            var avatarRoot = CreateUiObject("Avatar", characterCard);
            avatarRoot.anchorMin = new Vector2(0.20f, 0.18f);
            avatarRoot.anchorMax = new Vector2(0.80f, 0.88f);
            avatarRoot.offsetMin = Vector2.zero;
            avatarRoot.offsetMax = Vector2.zero;

            avatarImage = avatarRoot.gameObject.AddComponent<Image>();
            avatarImage.color = UITheme.FromHex("F1F8FF");
            RuntimeArt.ApplyRounded(avatarImage);
            avatarRoot.gameObject.AddComponent<UiFloatBob>().Configure(8f, 0.45f, 0.15f);

            var badge = CreateUiObject("Badge", avatarRoot);
            badge.anchorMin = new Vector2(0.34f, 0.36f);
            badge.anchorMax = new Vector2(0.66f, 0.64f);
            badge.offsetMin = Vector2.zero;
            badge.offsetMax = Vector2.zero;
            var badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.color = Color.white;
            RuntimeArt.ApplyRounded(badgeImage);

            avatarBadgeText = CreateText("BadgeText", badge, font, 72, FontStyle.Bold, UITheme.FromHex("577EAB"));
            Stretch(avatarBadgeText.rectTransform);
            avatarBadgeText.alignment = TextAnchor.MiddleCenter;
            avatarBadgeText.text = "男";

            avatarNameText = CreateText("PlayerName", characterCard, font, 46, FontStyle.Bold, UITheme.Text);
            avatarNameText.alignment = TextAnchor.MiddleCenter;
            avatarNameText.rectTransform.anchorMin = new Vector2(0.10f, 0.04f);
            avatarNameText.rectTransform.anchorMax = new Vector2(0.90f, 0.18f);
            avatarNameText.rectTransform.offsetMin = Vector2.zero;
            avatarNameText.rectTransform.offsetMax = Vector2.zero;
            avatarNameText.text = "未命名";

            summaryText = CreateText("Summary", characterCard, font, 28, FontStyle.Normal, UITheme.TextSoft);
            summaryText.alignment = TextAnchor.MiddleCenter;
            summaryText.rectTransform.anchorMin = new Vector2(0.06f, 0.04f);
            summaryText.rectTransform.anchorMax = new Vector2(0.94f, 0.20f);
            summaryText.rectTransform.offsetMin = Vector2.zero;
            summaryText.rectTransform.offsetMax = Vector2.zero;

            var entryCard = CreateUiObject("EntryCard", body);
            entryCard.anchorMin = new Vector2(0.06f, 0.06f);
            entryCard.anchorMax = new Vector2(0.94f, 0.40f);
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

            if (state.CurrentProgress < GameProgress.Gaokao)
            {
                return ScreenType.Gaokao;
            }

            if (state.CurrentProgress < GameProgress.Volunteer)
            {
                return ScreenType.Volunteer;
            }

            if (state.CurrentProgress < GameProgress.University)
            {
                return ScreenType.University;
            }

            if (state.CurrentProgress < GameProgress.Career)
            {
                return ScreenType.Career;
            }

            if (state.CurrentProgress < GameProgress.Summary)
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

