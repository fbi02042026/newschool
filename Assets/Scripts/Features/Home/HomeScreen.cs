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
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text avatarBadgeText;
        [SerializeField] private Text avatarNameText;
        [SerializeField] private RectTransform buttonGroupRoot;

        private readonly List<Button> dynamicButtons = new List<Button>();

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.Home));
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

            GuideService.EnsureHelpButton(transform.Find("Panel/Header") ?? transform, "BtnHelp", () => GuideService.Open(ScreenType.Home, transform));
            GuideService.TryShowOnce(ScreenType.Home, transform);
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "主界面";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "准备开始你的高中生活";
            }

            RefreshSummary();
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
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => NavigateTo(ScreenType.Launch, false));
            }
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

            summaryText.text = $"省份：{province}    家庭：{family}\n选科：{subjects}    金币：{state.Money}";
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

            for (int i = 0; i < unlocked.Count; i++)
            {
                var buttonType = unlocked[i];
                var button = CreatePrimaryButton(GetButtonLabel(buttonType), buttonGroupRoot, BuiltinFont(), UITheme.Confirm, UITheme.Text);
                button.gameObject.AddComponent<UiPressScale>();
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
                case HomeButtonType.Career:
                    NavigateTo(ScreenType.Career, true);
                    return;
                default:
                    ShowToast("该功能暂未接入界面");
                    return;
            }
        }

        private static string GetButtonLabel(HomeButtonType buttonType)
        {
            switch (buttonType)
            {
                case HomeButtonType.TalentTree: return "天赋树";
                case HomeButtonType.Semester: return "学期推进";
                case HomeButtonType.Equipment: return "装备";
                case HomeButtonType.Gaokao: return "高考";
                case HomeButtonType.Volunteer: return "志愿填报";
                case HomeButtonType.University: return "大学";
                case HomeButtonType.Career: return "毕业到30岁";
                case HomeButtonType.Achievements: return "成就";
                case HomeButtonType.Settings: return "设置";
                case HomeButtonType.Rules: return "规则";
                default: return "功能";
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (restartButton != null && titleText != null && subtitleText != null && summaryText != null && avatarImage != null && avatarBadgeText != null && avatarNameText != null && buttonGroupRoot != null)
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
            bgImage.color = UITheme.Bg;

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

            restartButton = CreateSmallButton("← 返回启动页", header, font, UITheme.CardPeach, UITheme.Text);
            var restartRect = (RectTransform)restartButton.transform;
            restartRect.anchorMin = new Vector2(0f, 0.72f);
            restartRect.anchorMax = new Vector2(0.30f, 0.98f);
            restartRect.offsetMin = Vector2.zero;
            restartRect.offsetMax = Vector2.zero;
            restartButton.gameObject.AddComponent<UiPressScale>();

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.06f);
            body.anchorMax = new Vector2(1f, 0.74f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var characterCard = CreateUiObject("CharacterCard", body);
            characterCard.anchorMin = new Vector2(0.06f, 0.38f);
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

            summaryText = CreateText("Summary", characterCard, font, 34, FontStyle.Normal, UITheme.TextSoft);
            summaryText.alignment = TextAnchor.MiddleCenter;
            summaryText.rectTransform.anchorMin = new Vector2(0.08f, 0.18f);
            summaryText.rectTransform.anchorMax = new Vector2(0.92f, 0.30f);
            summaryText.rectTransform.offsetMin = Vector2.zero;
            summaryText.rectTransform.offsetMax = Vector2.zero;

            var entryCard = CreateUiObject("EntryCard", body);
            entryCard.anchorMin = new Vector2(0.06f, 0.06f);
            entryCard.anchorMax = new Vector2(0.94f, 0.34f);
            entryCard.offsetMin = Vector2.zero;
            entryCard.offsetMax = Vector2.zero;
            var entryBg = entryCard.gameObject.AddComponent<Image>();
            entryBg.color = Color.white;
            RuntimeArt.ApplyRounded(entryBg);
            var entryShadow = entryCard.gameObject.AddComponent<Shadow>();
            entryShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            entryShadow.effectDistance = new Vector2(0f, -10f);

            buttonGroupRoot = CreateUiObject("Buttons", entryCard);
            Stretch(buttonGroupRoot);
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
                    return "农村";
                case FamilyBackgroundType.CivilServant:
                    return "公务员";
                default:
                    return "未知";
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

