using System;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.PlayerInfo
{
    public class PlayerInfoScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text genderText;
        [SerializeField] private Text moneyText;
        [SerializeField] private Text infoText;
        [SerializeField] private Text statIntelligenceLabel;
        [SerializeField] private Text statIntelligenceBar;
        [SerializeField] private Text statPsychologyLabel;
        [SerializeField] private Text statPsychologyBar;
        [SerializeField] private Text statSocialLabel;
        [SerializeField] private Text statSocialBar;
        [SerializeField] private Text statHealthLabel;
        [SerializeField] private Text statHealthBar;

        private RectTransform statCardRoot;
        private RectTransform infoRowRoot;

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
            }

            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            if (state == null) return;

            if (titleText != null) titleText.text = "人物信息";

            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(state.PlayerName) ? "未命名" : state.PlayerName;
            }

            if (genderText != null)
            {
                genderText.text = state.Gender == PlayerGender.Female ? "女" : "男";
                genderText.color = state.Gender == PlayerGender.Female
                    ? UITheme.FromHex("AD5C7E")
                    : UITheme.FromHex("577EAB");
            }

            if (avatarImage != null)
            {
                var avatarPath = state.Gender == PlayerGender.Female
                    ? "UI/Profile/avatar_girl"
                    : "UI/Profile/avatar_boy";
                var sprite = Resources.Load<Sprite>(avatarPath);
                if (sprite != null)
                {
                    avatarImage.sprite = sprite;
                    avatarImage.color = Color.white;
                }
                else
                {
                    avatarImage.sprite = null;
                    avatarImage.color = state.Gender == PlayerGender.Female
                        ? UITheme.FromHex("FFF1F7")
                        : UITheme.FromHex("F1F8FF");
                }
            }

            if (statIntelligenceBar != null) statIntelligenceBar.text = BuildStatBar(state.StatIntelligence);
            if (statPsychologyBar != null) statPsychologyBar.text = BuildStatBar(state.StatPsychology);
            if (statSocialBar != null) statSocialBar.text = BuildStatBar(state.StatSocial);
            if (statHealthBar != null) statHealthBar.text = BuildStatBar(state.StatHealth);

            if (statIntelligenceLabel != null) statIntelligenceLabel.text = $"📖 学习能力：{state.StatIntelligence}";
            if (statPsychologyLabel != null) statPsychologyLabel.text = $"😊 情绪管理：{state.StatPsychology}";
            if (statSocialLabel != null) statSocialLabel.text = $"🤝 人际关系：{state.StatSocial}";
            if (statHealthLabel != null) statHealthLabel.text = $"🫀 健康状态：{state.StatHealth}";

            if (moneyText != null) moneyText.text = $"💰 金币余额：{state.Money}";

            if (infoText != null)
            {
                var province = string.IsNullOrWhiteSpace(state.SelectedProvince) ? "未选择" : state.SelectedProvince;
                var family = state.SelectedFamily == FamilyBackgroundType.None ? "未选择" : GetFamilyLabel(state.SelectedFamily);
                var subjectsReady = state.FirstSubject != FirstSubject.None && state.SecondSubjects != null && state.SecondSubjects.Count == 2;
                var firstSubjectCn = GetFirstSubjectChinese(state.FirstSubject);
                var secondSubjectsCn = subjectsReady
                    ? $"{GetSecondSubjectChinese(state.SecondSubjects[0])}/{GetSecondSubjectChinese(state.SecondSubjects[1])}"
                    : "";
                var subjects = subjectsReady ? $"{firstSubjectCn} + {secondSubjectsCn}" : "未完成选科";

                var semester = state.SemesterIndex > 0 ? $"{state.SemesterIndex}/{state.TotalSemesters} 学期" : "尚未开始";
                var mode = string.IsNullOrWhiteSpace(state.SelectedProvinceMode) ? "未选择" : state.SelectedProvinceMode;

                var items = (state.OwnedItems != null && state.OwnedItems.Count > 0)
                    ? string.Join("、", state.OwnedItems)
                    : "暂无";

                infoText.text = $"📋 省份：{province}    家庭：{family}    选科：{subjects}\n" +
                    $"📅 学期进度：{semester}    考试模式：{mode}\n" +
                    $"🎒 已有物品：{items}";
            }
        }

        private static string BuildStatBar(int statValue)
        {
            var maxBlocks = 20;
            var filled = Mathf.Clamp(statValue, 0, maxBlocks);
            var bar = new System.Text.StringBuilder();
            for (int i = 0; i < maxBlocks; i++)
            {
                bar.Append(i < filled ? "▮" : "▯");
            }
            return bar.ToString();
        }

        private void BindEvents()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => NavigateTo(ScreenType.Home, false));
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null && closeButton != null && titleText != null &&
                avatarImage != null && nameText != null && genderText != null &&
                moneyText != null && infoText != null &&
                statIntelligenceLabel != null && statIntelligenceBar != null &&
                statPsychologyLabel != null && statPsychologyBar != null &&
                statSocialLabel != null && statSocialBar != null &&
                statHealthLabel != null && statHealthBar != null)
            {
                return;
            }

            BuildRuntimeLayout();
        }

        private void BuildRuntimeLayout()
        {
            var font = BuiltinFont();

            var background = CreateUiObject("Background", transform);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgGradient = background.gameObject.AddComponent<UiCornerGradient>();
            bgGradient.SetColors(UITheme.CardSky, UITheme.Bg, UITheme.CardLavender, UITheme.CardButter);

            var panel = CreateUiObject("Panel", transform);
            panel.anchorMin = new Vector2(0.05f, 0.04f);
            panel.anchorMax = new Vector2(0.95f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.88f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            backButton = CreateOutlineButton("← 返回", header, font);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.2f);
            backRect.anchorMax = new Vector2(0.22f, 0.8f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 64, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.24f, 0.2f);
            titleText.rectTransform.anchorMax = new Vector2(0.76f, 0.8f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.14f);
            body.anchorMax = new Vector2(1f, 0.88f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var avatarRoot = CreateUiObject("Avatar", body);
            avatarRoot.anchorMin = new Vector2(0.34f, 0.78f);
            avatarRoot.anchorMax = new Vector2(0.66f, 1f);
            avatarRoot.offsetMin = Vector2.zero;
            avatarRoot.offsetMax = Vector2.zero;

            avatarImage = avatarRoot.gameObject.AddComponent<Image>();
            avatarImage.color = UITheme.FromHex("F1F8FF");
            avatarRoot.gameObject.AddComponent<UiAutoRounded>();

            nameText = CreateText("Name", body, font, 48, FontStyle.Bold, UITheme.Text);
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.rectTransform.anchorMin = new Vector2(0.1f, 0.70f);
            nameText.rectTransform.anchorMax = new Vector2(0.9f, 0.78f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;

            genderText = CreateText("Gender", body, font, 36, FontStyle.Bold, UITheme.TextSoft);
            genderText.alignment = TextAnchor.MiddleCenter;
            genderText.rectTransform.anchorMin = new Vector2(0.1f, 0.66f);
            genderText.rectTransform.anchorMax = new Vector2(0.9f, 0.70f);
            genderText.rectTransform.offsetMin = Vector2.zero;
            genderText.rectTransform.offsetMax = Vector2.zero;

            moneyText = CreateText("Money", body, font, 36, FontStyle.Bold, UITheme.Gold);
            moneyText.alignment = TextAnchor.MiddleCenter;
            moneyText.rectTransform.anchorMin = new Vector2(0.1f, 0.60f);
            moneyText.rectTransform.anchorMax = new Vector2(0.9f, 0.64f);
            moneyText.rectTransform.offsetMin = Vector2.zero;
            moneyText.rectTransform.offsetMax = Vector2.zero;

            infoText = CreateText("Info", body, font, 28, FontStyle.Normal, UITheme.TextLight);
            infoText.alignment = TextAnchor.MiddleCenter;
            infoText.rectTransform.anchorMin = new Vector2(0.04f, 0.52f);
            infoText.rectTransform.anchorMax = new Vector2(0.96f, 0.58f);
            infoText.rectTransform.offsetMin = Vector2.zero;
            infoText.rectTransform.offsetMax = Vector2.zero;

            statCardRoot = CreateUiObject("StatCards", body);
            statCardRoot.anchorMin = new Vector2(0f, 0f);
            statCardRoot.anchorMax = new Vector2(1f, 0.52f);
            statCardRoot.offsetMin = Vector2.zero;
            statCardRoot.offsetMax = Vector2.zero;

            BuildStatCardRow(0, statCardRoot, font, out statIntelligenceLabel, out statIntelligenceBar);
            BuildStatCardRow(1, statCardRoot, font, out statPsychologyLabel, out statPsychologyBar);
            BuildStatCardRow(2, statCardRoot, font, out statSocialLabel, out statSocialBar);
            BuildStatCardRow(3, statCardRoot, font, out statHealthLabel, out statHealthBar);

            var footer = CreateUiObject("Footer", panel);
            footer.anchorMin = new Vector2(0f, 0f);
            footer.anchorMax = new Vector2(1f, 0.14f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            closeButton = CreatePrimaryButton("关闭", footer, font, UITheme.Confirm, Color.white);
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.15f, 0.1f);
            closeRect.anchorMax = new Vector2(0.85f, 0.9f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
        }

        private void BuildStatCardRow(int index, RectTransform parent, Font font, out Text label, out Text bar)
        {
            var card = CreateUiObject($"StatRow_{index}", parent);
            card.anchorMin = new Vector2(0.04f, 0.73f - index * 0.25f);
            card.anchorMax = new Vector2(0.96f, 0.93f - index * 0.25f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;

            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();

            label = CreateText("Label", card, font, 34, FontStyle.Bold, UITheme.Text);
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.anchorMin = new Vector2(0.04f, 0.48f);
            label.rectTransform.anchorMax = new Vector2(0.96f, 0.96f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            bar = CreateText("Bar", card, font, 24, FontStyle.Normal, UITheme.Accent);
            bar.alignment = TextAnchor.MiddleLeft;
            bar.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
            bar.rectTransform.anchorMax = new Vector2(0.96f, 0.46f);
            bar.rectTransform.offsetMin = Vector2.zero;
            bar.rectTransform.offsetMax = Vector2.zero;
        }

        private static string GetFamilyLabel(FamilyBackgroundType type)
        {
            switch (type)
            {
                case FamilyBackgroundType.Intellectual: return "书香";
                case FamilyBackgroundType.Business: return "经商";
                case FamilyBackgroundType.Worker: return "工薪";
                case FamilyBackgroundType.Rural: return "田园人家";
                case FamilyBackgroundType.CivilServant: return "公务员";
                default: return "未知";
            }
        }

        private static string GetFirstSubjectChinese(FirstSubject subject)
        {
            switch (subject)
            {
                case FirstSubject.Physics: return "物理";
                case FirstSubject.History: return "历史";
                default: return "未选择";
            }
        }

        private static string GetSecondSubjectChinese(SecondSubject subject)
        {
            switch (subject)
            {
                case SecondSubject.Chemistry: return "化学";
                case SecondSubject.Biology: return "生物";
                case SecondSubject.Geography: return "地理";
                case SecondSubject.Politics: return "政治";
                default: return "";
            }
        }

        private static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        private static Button CreateOutlineButton(string label, Transform parent, Font font)
        {
            var buttonGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var image = buttonGo.GetComponent<Image>();
            image.color = Color.white;
            buttonGo.AddComponent<UiAutoRounded>();

            var outline = buttonGo.AddComponent<Outline>();
            outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            outline.effectDistance = new Vector2(4f, -4f);

            var text = CreateText("Text", buttonGo.transform, font, 42, FontStyle.Bold, UITheme.Text);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            buttonGo.AddComponent<UiPressScale>();
            return buttonGo.GetComponent<Button>();
        }

        private static Button CreatePrimaryButton(string label, Transform parent, Font font, Color a, Color textColor)
        {
            var buttonGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var image = buttonGo.GetComponent<Image>();
            image.color = Color.white;
            buttonGo.AddComponent<UiAutoRounded>();

            var gradient = buttonGo.AddComponent<UiCornerGradient>();
            gradient.SetColors(a, UITheme.ConfirmHover, UITheme.ConfirmHover, a);
            var shadow = buttonGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(a.r / 255f, a.g / 255f, a.b / 255f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -12f);

            var text = CreateText("Text", buttonGo.transform, font, 46, FontStyle.Bold, textColor);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            buttonGo.AddComponent<UiPressScale>();
            return buttonGo.GetComponent<Button>();
        }
    }
}
