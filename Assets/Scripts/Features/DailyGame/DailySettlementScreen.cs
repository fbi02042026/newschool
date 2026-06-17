using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;

namespace GaokaoSimulator.Features.DailyGame
{
    public class DailySettlementScreen : ScreenBase
    {
        private Transform content;

        // Section 1: Comment
        private Text commentTitle;
        private Text commentText;

        // Section 2: Attributes + Energy
        private Text intellValue;
        private Text psychoValue;
        private Text socialValue;
        private Text healthValue;
        private Image energyBar;
        private Text energyValue;
        private Text energyZone;

        // Section 3: Best Moment
        private Text bestEventTitle;
        private Text bestEventDesc;

        // Section 4: Character Monologue
        private Text monologueText;

        // Section 5: Buttons
        private Button homeBtn;
        private Button continueBtn;

        protected override void Initialize()
        {
            content = transform.Find("Content");
            if (content == null)
            {
                BuildRuntimeLayout();
                content = transform.Find("Content");
            }

            InitAll();
        }

        protected override void OnScreenOpen()
        {
            UpdateContent();
        }

        private void InitAll()
        {
            commentTitle = content.Find("CommentSection/Title").GetComponent<Text>();
            commentText = content.Find("CommentSection/Text").GetComponent<Text>();

            intellValue = content.Find("AttrSection/Intelligence/Value").GetComponent<Text>();
            psychoValue = content.Find("AttrSection/Psychology/Value").GetComponent<Text>();
            socialValue = content.Find("AttrSection/Social/Value").GetComponent<Text>();
            healthValue = content.Find("AttrSection/Health/Value").GetComponent<Text>();

            energyBar = content.Find("EnergySection/Bar").GetComponent<Image>();
            energyValue = content.Find("EnergySection/Value").GetComponent<Text>();
            energyZone = content.Find("EnergySection/Zone").GetComponent<Text>();

            bestEventTitle = content.Find("BestSection/Title").GetComponent<Text>();
            bestEventDesc = content.Find("BestSection/Desc").GetComponent<Text>();

            monologueText = content.Find("MonologueSection/Text").GetComponent<Text>();

            homeBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(GoHome);
            continueBtn.onClick.AddListener(ContinueTomorrow);
        }

        private void UpdateContent()
        {
            var state = GameState.Instance;
            UpdateComment(state);
            UpdateAttributes(state);
            UpdateEnergy(state);
            UpdateBestMoment(state);
            UpdateMonologue(state);
        }

        private void UpdateComment(GameState state)
        {
            commentTitle.text = "今日评语";
            commentText.text = DailyGameData.GetSettlementComment(state.EventsCompleted, state.Energy);
        }

        private void UpdateAttributes(GameState state)
        {
            intellValue.text = state.StatIntelligence.ToString();
            psychoValue.text = state.StatPsychology.ToString();
            socialValue.text = state.StatSocial.ToString();
            healthValue.text = state.StatHealth.ToString();

            SetAttrColor(intellValue, state.StatIntelligence);
            SetAttrColor(psychoValue, state.StatPsychology);
            SetAttrColor(socialValue, state.StatSocial);
            SetAttrColor(healthValue, state.StatHealth);
        }

        private void SetAttrColor(Text text, int value)
        {
            if (value >= 80) text.color = new Color32(76, 175, 80, 255);
            else if (value >= 50) text.color = new Color32(33, 150, 243, 255);
            else if (value >= 30) text.color = new Color32(255, 152, 0, 255);
            else text.color = new Color32(244, 67, 54, 255);
        }

        private void UpdateEnergy(GameState state)
        {
            float pct = Mathf.Clamp01(state.Energy / 150f);
            energyBar.fillAmount = pct;
            energyBar.color = DailyGameData.GetEnergyColor(state.Energy);
            energyValue.text = $"{state.Energy}";
            energyZone.text = $"{DailyGameData.GetEnergyEmoji(state.Energy)} {DailyGameData.GetEnergyZone(state.Energy)}";
            energyZone.color = DailyGameData.GetEnergyColor(state.Energy);
        }

        private void UpdateBestMoment(GameState state)
        {
            bestEventTitle.text = "精彩瞬间";
            bestEventDesc.text = $"今日完成了 {state.EventsCompleted} 个事件，属性全面提升！";
        }

        private void UpdateMonologue(GameState state)
        {
            monologueText.text = DailyGameData.GetCharacterMonologue(state.Energy);
        }

        private void GoHome()
        {
            NavigateTo(ScreenType.Home, true);
        }

        private void ContinueTomorrow()
        {
            NavigateTo(ScreenType.DailyGame, true);
        }

        private void BuildRuntimeLayout()
        {
            var font = BuiltinFont();
            var root = (RectTransform)transform;
            Stretch(root);

            var contentObj = CreateUiObject("Content", root);
            Stretch(contentObj);
            var bg = contentObj.gameObject.AddComponent<Image>();
            bg.color = UITheme.Bg;

            // ===== Section 1: Comment =====
            var commentSection = CreateUiObject("CommentSection", contentObj);
            commentSection.anchorMin = new Vector2(0.06f, 0.78f);
            commentSection.anchorMax = new Vector2(0.94f, 0.96f);

            commentTitle = CreateText("Title", commentSection, font, 28, FontStyle.Bold, UITheme.Text);
            commentTitle.alignment = TextAnchor.UpperCenter;
            commentTitle.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            commentTitle.rectTransform.anchorMax = new Vector2(1f, 1f);

            commentText = CreateText("Text", commentSection, font, 36, FontStyle.Bold, UITheme.Text);
            commentText.alignment = TextAnchor.MiddleCenter;
            commentText.rectTransform.anchorMin = new Vector2(0f, 0f);
            commentText.rectTransform.anchorMax = new Vector2(1f, 0.45f);

            // ===== Section 2: Attributes =====
            var attrSection = CreateUiObject("AttrSection", contentObj);
            attrSection.anchorMin = new Vector2(0.06f, 0.58f);
            attrSection.anchorMax = new Vector2(0.94f, 0.75f);

            var attrGrid = attrSection.gameObject.AddComponent<GridLayoutGroup>();
            attrGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            attrGrid.constraintCount = 4;
            attrGrid.cellSize = new Vector2(160f, 100f);
            attrGrid.spacing = new Vector2(12f, 0f);
            attrGrid.padding = new RectOffset(8, 8, 0, 0);

            CreateAttrChip(attrSection, font, "智力", ref intellValue);
            CreateAttrChip(attrSection, font, "心理", ref psychoValue);
            CreateAttrChip(attrSection, font, "社交", ref socialValue);
            CreateAttrChip(attrSection, font, "健康", ref healthValue);

            // ===== Section 3: Energy =====
            var energySection = CreateUiObject("EnergySection", contentObj);
            energySection.anchorMin = new Vector2(0.06f, 0.48f);
            energySection.anchorMax = new Vector2(0.94f, 0.55f);

            var energyLabel = CreateText("Label", energySection, font, 24, FontStyle.Bold, UITheme.Text);
            energyLabel.alignment = TextAnchor.MiddleLeft;
            energyLabel.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            energyLabel.rectTransform.anchorMax = new Vector2(0.18f, 1f);
            energyLabel.text = "精力";

            var barBg = CreateUiObject("BarBg", energySection);
            barBg.anchorMin = new Vector2(0.2f, 0.35f);
            barBg.anchorMax = new Vector2(0.7f, 0.65f);
            var bgImg = barBg.gameObject.AddComponent<Image>();
            bgImg.color = new Color32(224, 224, 224, 255);

            energyBar = CreateUiObject("Bar", energySection).gameObject.AddComponent<Image>();
            energyBar.rectTransform.anchorMin = new Vector2(0.2f, 0.35f);
            energyBar.rectTransform.anchorMax = new Vector2(0.7f, 0.65f);
            energyBar.fillMethod = Image.FillMethod.Horizontal;

            energyValue = CreateText("Value", energySection, font, 22, FontStyle.Bold, UITheme.Text);
            energyValue.alignment = TextAnchor.MiddleCenter;
            energyValue.rectTransform.anchorMin = new Vector2(0.72f, 0f);
            energyValue.rectTransform.anchorMax = new Vector2(0.85f, 1f);

            energyZone = CreateText("Zone", energySection, font, 22, FontStyle.Normal, UITheme.Text);
            energyZone.alignment = TextAnchor.MiddleLeft;
            energyZone.rectTransform.anchorMin = new Vector2(0.86f, 0f);
            energyZone.rectTransform.anchorMax = new Vector2(1f, 1f);

            // ===== Section 4: Best Moment =====
            var bestSection = CreateUiObject("BestSection", contentObj);
            bestSection.anchorMin = new Vector2(0.06f, 0.35f);
            bestSection.anchorMax = new Vector2(0.94f, 0.45f);

            bestEventTitle = CreateText("Title", bestSection, font, 26, FontStyle.Bold, UITheme.Text);
            bestEventTitle.alignment = TextAnchor.UpperLeft;
            bestEventTitle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            bestEventTitle.rectTransform.anchorMax = new Vector2(1f, 1f);

            bestEventDesc = CreateText("Desc", bestSection, font, 24, FontStyle.Normal, UITheme.TextSoft);
            bestEventDesc.alignment = TextAnchor.MiddleLeft;
            bestEventDesc.rectTransform.anchorMin = new Vector2(0f, 0f);
            bestEventDesc.rectTransform.anchorMax = new Vector2(1f, 0.55f);

            // ===== Section 5: Monologue =====
            var monologueSection = CreateUiObject("MonologueSection", contentObj);
            monologueSection.anchorMin = new Vector2(0.06f, 0.22f);
            monologueSection.anchorMax = new Vector2(0.94f, 0.32f);

            monologueText = CreateText("Text", monologueSection, font, 26, FontStyle.Italic, UITheme.TextSoft);
            monologueText.alignment = TextAnchor.MiddleCenter;
            monologueText.rectTransform.anchorMin = Vector2.zero;
            monologueText.rectTransform.anchorMax = Vector2.one;

            // ===== Buttons =====
            homeBtn = CreatePrimaryButton("回家", contentObj, font, UITheme.FromHex("9E9E9E"), UITheme.Text);
            homeBtn.name = "HomeBtn";
            var homeRect = (RectTransform)homeBtn.transform;
            homeRect.anchorMin = new Vector2(0.06f, 0.04f);
            homeRect.anchorMax = new Vector2(0.47f, 0.18f);
            var homeLabel = homeBtn.GetComponentInChildren<Text>();
            if (homeLabel != null) homeLabel.fontSize = 28;

            continueBtn = CreatePrimaryButton("继续明天", contentObj, font, UITheme.Confirm, UITheme.Text);
            continueBtn.name = "ContinueBtn";
            var continueRect = (RectTransform)continueBtn.transform;
            continueRect.anchorMin = new Vector2(0.53f, 0.04f);
            continueRect.anchorMax = new Vector2(0.94f, 0.18f);
            var continueLabel = continueBtn.GetComponentInChildren<Text>();
            if (continueLabel != null) continueLabel.fontSize = 28;
        }

        private void CreateAttrChip(Transform parent, Font font, string name, ref Text valueText)
        {
            var item = CreateUiObject(name, parent);

            var nameText = CreateText("Name", item, font, 22, FontStyle.Normal, UITheme.TextSoft);
            nameText.alignment = TextAnchor.UpperCenter;
            nameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            nameText.rectTransform.anchorMax = new Vector2(1f, 1f);
            nameText.text = name;

            valueText = CreateText("Value", item, font, 36, FontStyle.Bold, UITheme.Text);
            valueText.alignment = TextAnchor.MiddleCenter;
            valueText.rectTransform.anchorMin = new Vector2(0f, 0f);
            valueText.rectTransform.anchorMax = new Vector2(1f, 0.55f);
            valueText.text = "0";
        }
    }
}