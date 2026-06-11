using System;
using System.Collections;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Family
{
    public class FamilyScreen : ScreenBase
    {
        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button keepButton;
        [SerializeField] private Button rerollButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text drawHintText;
        [SerializeField] private Text drawCardIconText;
        [SerializeField] private Text drawCardNameText;
        [SerializeField] private Text drawCardDescText;
        [SerializeField] private Image progressFillImage;
        [SerializeField] private RectTransform drawStageRoot;
        [SerializeField] private RectTransform resultStageRoot;
        [SerializeField] private Text resultIconText;
        [SerializeField] private Text resultNameText;
        [SerializeField] private Text resultDescText;
        [SerializeField] private Text resultMoneyText;
        [SerializeField] private Image statIntFill;
        [SerializeField] private Image statPsyFill;
        [SerializeField] private Image statSocFill;
        [SerializeField] private Image statHealthFill;
        [SerializeField] private Text statIntValue;
        [SerializeField] private Text statPsyValue;
        [SerializeField] private Text statSocValue;
        [SerializeField] private Text statHealthValue;

        [Header("重置对比")]
        [SerializeField] private RectTransform compareStageRoot;
        [SerializeField] private Button replaceButton;
        [SerializeField] private Button cancelReplaceButton;
        [SerializeField] private Text compareOldNameText;
        [SerializeField] private Text compareNewNameText;
        [SerializeField] private Text compareOldMoneyText;
        [SerializeField] private Text compareNewMoneyText;
        [SerializeField] private Text compareMoneyDeltaText;
        [SerializeField] private Text compareOldIconText;
        [SerializeField] private Text compareNewIconText;
        [SerializeField] private Image compareOldIntFill;
        [SerializeField] private Image compareOldPsyFill;
        [SerializeField] private Image compareOldSocFill;
        [SerializeField] private Image compareOldHealthFill;
        [SerializeField] private Image compareNewIntFill;
        [SerializeField] private Image compareNewPsyFill;
        [SerializeField] private Image compareNewSocFill;
        [SerializeField] private Image compareNewHealthFill;
        [SerializeField] private Text compareOldIntValueText;
        [SerializeField] private Text compareOldPsyValueText;
        [SerializeField] private Text compareOldSocValueText;
        [SerializeField] private Text compareOldHealthValueText;
        [SerializeField] private Text compareNewIntValueText;
        [SerializeField] private Text compareNewPsyValueText;
        [SerializeField] private Text compareNewSocValueText;
        [SerializeField] private Text compareNewHealthValueText;
        [SerializeField] private Text compareIntDeltaText;
        [SerializeField] private Text comparePsyDeltaText;
        [SerializeField] private Text compareSocDeltaText;
        [SerializeField] private Text compareHealthDeltaText;

        private Coroutine drawCoroutine;
        private int freeRerollLeft = 1;
        private FamilyCandidate currentCandidate;
        private FamilyCandidate previousCandidate;
        private FamilyCandidate pendingCandidate;

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            EnsureCompareLayout();
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.CurrentProgress = Core.GameProgress.Family;
                state.HasSaveData = true;
            }

            StartDraw();
        }

        protected override void OnScreenClose()
        {
            if (drawCoroutine != null)
            {
                StopCoroutine(drawCoroutine);
                drawCoroutine = null;
            }
        }

        public override void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "命运的起点";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "抽一张家庭背景卡，决定你的开局资源";
            }

            UpdateRerollButtonState();
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null && helpButton != null && keepButton != null && rerollButton != null && titleText != null && subtitleText != null && drawHintText != null && drawCardIconText != null && drawCardNameText != null && drawCardDescText != null && progressFillImage != null && drawStageRoot != null && resultStageRoot != null && resultIconText != null && resultNameText != null && resultDescText != null && resultMoneyText != null && statIntFill != null && statPsyFill != null && statSocFill != null && statHealthFill != null && statIntValue != null && statPsyValue != null && statSocValue != null && statHealthValue != null)
            {
                return;
            }

            BuildRuntimeLayout();
        }

        private void BindEvents()
        {
            if (backButton != null) backButton.onClick.AddListener(() => NavigateTo(ScreenType.Profile, false));
            if (helpButton != null) helpButton.onClick.AddListener(() => ShowToast("后续会补：属性说明弹窗"));
            if (keepButton != null) keepButton.onClick.AddListener(KeepCurrent);
            if (rerollButton != null) rerollButton.onClick.AddListener(Reroll);
            if (replaceButton != null) replaceButton.onClick.AddListener(ReplaceWithPending);
            if (cancelReplaceButton != null) cancelReplaceButton.onClick.AddListener(CancelReplace);
        }

        private void StartDraw()
        {
            if (drawCoroutine != null)
            {
                StopCoroutine(drawCoroutine);
            }

            drawStageRoot.gameObject.SetActive(true);
            resultStageRoot.gameObject.SetActive(false);
            if (compareStageRoot != null) compareStageRoot.gameObject.SetActive(false);
            keepButton.gameObject.SetActive(false);
            rerollButton.gameObject.SetActive(false);

            drawCoroutine = StartCoroutine(DrawRoutine());
        }

        private IEnumerator DrawRoutine()
        {
            var duration = 2.0f;
            var tick = 0.10f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += tick;
                currentCandidate = FamilyCandidate.Random();
                ApplyDrawCandidate(currentCandidate);
                SetProgress(Mathf.Clamp01(elapsed / duration));
                yield return new WaitForSeconds(tick);
            }

            currentCandidate = FamilyCandidate.Random();
            ApplyDrawCandidate(currentCandidate);
            SetProgress(1f);

            yield return new WaitForSeconds(0.15f);

            if (previousCandidate != null)
            {
                pendingCandidate = FamilyCandidate.RandomDifferent(previousCandidate.Type);
                ShowCompare(previousCandidate, pendingCandidate);
                yield break;
            }

            ShowResult(currentCandidate, true);
        }

        private void ApplyDrawCandidate(FamilyCandidate candidate)
        {
            if (candidate == null) return;
            if (drawCardIconText != null) drawCardIconText.text = candidate.Icon;
            if (drawCardNameText != null) drawCardNameText.text = candidate.Name;
            if (drawCardDescText != null) drawCardDescText.text = candidate.Desc;
            if (drawHintText != null) drawHintText.text = "抽卡中…";
        }

        private void ShowResult(FamilyCandidate candidate)
        {
            ShowResult(candidate, true);
        }

        private void ShowResult(FamilyCandidate candidate, bool showFooterButtons)
        {
            if (candidate == null) return;

            drawStageRoot.gameObject.SetActive(false);
            resultStageRoot.gameObject.SetActive(true);
            if (compareStageRoot != null) compareStageRoot.gameObject.SetActive(false);
            keepButton.gameObject.SetActive(showFooterButtons);
            rerollButton.gameObject.SetActive(showFooterButtons);

            if (resultIconText != null) resultIconText.text = candidate.Icon;
            if (resultNameText != null) resultNameText.text = candidate.Name;
            if (resultDescText != null) resultDescText.text = candidate.Desc;
            if (resultMoneyText != null) resultMoneyText.text = $"金币 {candidate.Money}";

            SetStat(statIntFill, statIntValue, candidate.Intelligence, 100, UITheme.Confirm, UITheme.ConfirmHover);
            SetStat(statPsyFill, statPsyValue, candidate.Psychology, 100, UITheme.FromHex("CE93D8"), UITheme.FromHex("AB47BC"));
            SetStat(statSocFill, statSocValue, candidate.Social, 100, UITheme.FromHex("FFCC80"), UITheme.FromHex("FF9800"));
            SetStat(statHealthFill, statHealthValue, candidate.Health, 100, UITheme.FromHex("A5D6A7"), UITheme.FromHex("66BB6A"));

            if (drawHintText != null) drawHintText.text = "抽卡完成";

            UpdateRerollButtonState();
        }

        private void ShowCompare(FamilyCandidate oldCandidate, FamilyCandidate newCandidate)
        {
            if (compareStageRoot == null)
            {
                ShowResult(newCandidate, true);
                return;
            }

            ShowResult(oldCandidate, false);
            compareStageRoot.gameObject.SetActive(true);

            if (drawHintText != null) drawHintText.text = "抽到新开局！对比后选择是否替换";

            if (compareOldIconText != null) compareOldIconText.text = oldCandidate.Icon;
            if (compareOldNameText != null) compareOldNameText.text = oldCandidate.Name;
            if (compareOldMoneyText != null) compareOldMoneyText.text = $"{oldCandidate.Money}";

            if (compareNewIconText != null) compareNewIconText.text = newCandidate.Icon;
            if (compareNewNameText != null) compareNewNameText.text = newCandidate.Name;
            if (compareNewMoneyText != null) compareNewMoneyText.text = $"{newCandidate.Money}";

            SetDelta(compareMoneyDeltaText, newCandidate.Money - oldCandidate.Money);

            SetStat(compareOldIntFill, null, oldCandidate.Intelligence, 100, UITheme.Confirm, UITheme.ConfirmHover);
            SetStat(compareOldPsyFill, null, oldCandidate.Psychology, 100, UITheme.FromHex("CE93D8"), UITheme.FromHex("AB47BC"));
            SetStat(compareOldSocFill, null, oldCandidate.Social, 100, UITheme.FromHex("FFCC80"), UITheme.FromHex("FF9800"));
            SetStat(compareOldHealthFill, null, oldCandidate.Health, 100, UITheme.FromHex("A5D6A7"), UITheme.FromHex("66BB6A"));
            SetStat(compareNewIntFill, null, newCandidate.Intelligence, 100, UITheme.Confirm, UITheme.ConfirmHover);
            SetStat(compareNewPsyFill, null, newCandidate.Psychology, 100, UITheme.FromHex("CE93D8"), UITheme.FromHex("AB47BC"));
            SetStat(compareNewSocFill, null, newCandidate.Social, 100, UITheme.FromHex("FFCC80"), UITheme.FromHex("FF9800"));
            SetStat(compareNewHealthFill, null, newCandidate.Health, 100, UITheme.FromHex("A5D6A7"), UITheme.FromHex("66BB6A"));

            SetValue(compareOldIntValueText, oldCandidate.Intelligence);
            SetValue(compareOldPsyValueText, oldCandidate.Psychology);
            SetValue(compareOldSocValueText, oldCandidate.Social);
            SetValue(compareOldHealthValueText, oldCandidate.Health);
            SetValue(compareNewIntValueText, newCandidate.Intelligence);
            SetValue(compareNewPsyValueText, newCandidate.Psychology);
            SetValue(compareNewSocValueText, newCandidate.Social);
            SetValue(compareNewHealthValueText, newCandidate.Health);

            SetDelta(compareIntDeltaText, newCandidate.Intelligence - oldCandidate.Intelligence);
            SetDelta(comparePsyDeltaText, newCandidate.Psychology - oldCandidate.Psychology);
            SetDelta(compareSocDeltaText, newCandidate.Social - oldCandidate.Social);
            SetDelta(compareHealthDeltaText, newCandidate.Health - oldCandidate.Health);
        }

        private static void SetValue(Text text, int value)
        {
            if (text == null) return;
            text.text = value.ToString();
        }

        private static void SetDelta(Text text, int delta)
        {
            if (text == null) return;
            if (delta > 0)
            {
                text.text = $"+{delta}";
                text.color = UITheme.FromHex("66BB6A");
            }
            else if (delta < 0)
            {
                text.text = $"{delta}";
                text.color = UITheme.FromHex("EF5350");
            }
            else
            {
                text.text = "0";
                text.color = UITheme.TextLight;
            }
        }

        private static void SetStat(Image fill, Text valueText, int value, int max, Color a, Color b)
        {
            if (valueText != null) valueText.text = $"{value}/{max}";
            if (fill == null) return;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01((float)value / max);
            var grad = fill.GetComponent<UiCornerGradient>();
            if (grad == null) grad = fill.gameObject.AddComponent<UiCornerGradient>();
            grad.SetColors(a, b, b, a);
        }

        private void SetProgress(float t)
        {
            if (progressFillImage == null) return;
            progressFillImage.fillAmount = Mathf.Clamp01(t);
        }

        private void UpdateRerollButtonState()
        {
            if (rerollButton == null) return;
            var text = rerollButton.GetComponentInChildren<Text>();

            if (freeRerollLeft > 0)
            {
                rerollButton.interactable = true;
                if (text != null) text.text = "🔄 免费重置（1次）";
            }
            else
            {
                rerollButton.interactable = false;
                if (text != null) text.text = "🔒 免费重置（已用完）";
            }
        }

        private void KeepCurrent()
        {
            if (currentCandidate == null) return;
            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.SelectedFamily = currentCandidate.Type;
                state.Money = currentCandidate.Money;
                state.StatIntelligence = currentCandidate.Intelligence;
                state.StatPsychology = currentCandidate.Psychology;
                state.StatSocial = currentCandidate.Social;
                state.StatHealth = currentCandidate.Health;
                state.CurrentProgress = Core.GameProgress.Province;
                state.HasSaveData = true;
            }

            NavigateTo(ScreenType.Province, true);
        }

        private void Reroll()
        {
            if (freeRerollLeft <= 0) return;
            freeRerollLeft--;
            previousCandidate = currentCandidate;
            StartDraw();
        }

        private void ReplaceWithPending()
        {
            if (pendingCandidate == null)
            {
                CancelReplace();
                return;
            }

            currentCandidate = pendingCandidate;
            pendingCandidate = null;
            previousCandidate = null;
            ShowResult(currentCandidate, true);
        }

        private void CancelReplace()
        {
            if (previousCandidate == null)
            {
                ShowResult(currentCandidate, true);
                return;
            }

            currentCandidate = previousCandidate;
            pendingCandidate = null;
            previousCandidate = null;
            ShowResult(currentCandidate, true);
        }

        private void BuildRuntimeLayout()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = (RectTransform)transform;
            Stretch(root);

            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgGradient = background.gameObject.AddComponent<UiCornerGradient>();
            bgGradient.SetColors(UITheme.CardPeach, UITheme.Bg, UITheme.CardSky, UITheme.CardLavender);

            var dotsLayer = CreateUiObject("BgDots", root);
            Stretch(dotsLayer);
            BuildBgDots(dotsLayer, 12);

            var panel = CreateUiObject("Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.78f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var stepBadge = CreateUiObject("StepBadge", header);
            stepBadge.anchorMin = new Vector2(0.36f, 0.70f);
            stepBadge.anchorMax = new Vector2(0.64f, 0.98f);
            stepBadge.offsetMin = Vector2.zero;
            stepBadge.offsetMax = Vector2.zero;
            var stepBadgeImage = stepBadge.gameObject.AddComponent<Image>();
            stepBadgeImage.color = UITheme.CardPeach;
            stepBadge.gameObject.AddComponent<UiAutoRounded>();
            var stepText = CreateText("StepText", stepBadge, font, 32, FontStyle.Bold, UITheme.Accent);
            Stretch(stepText.rectTransform);
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.text = "STEP 2 / 5";

            backButton = CreateSmallButton("← 返回", header, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.70f);
            backRect.anchorMax = new Vector2(0.24f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backButton.gameObject.AddComponent<UiPressScale>();

            helpButton = CreateSmallButton("?", header, font, UITheme.CardSky, UITheme.Confirm);
            var helpRect = (RectTransform)helpButton.transform;
            helpRect.anchorMin = new Vector2(0.90f, 0.70f);
            helpRect.anchorMax = new Vector2(1f, 0.98f);
            helpRect.offsetMin = Vector2.zero;
            helpRect.offsetMax = Vector2.zero;
            helpButton.gameObject.AddComponent<UiPressScale>();

            titleText = CreateText("Title", header, font, 84, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.26f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.72f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;
            titleText.text = "命运的起点";

            subtitleText = CreateText("Subtitle", header, font, 42, FontStyle.Normal, UITheme.TextLight);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.00f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.30f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;
            subtitleText.text = "抽一张家庭背景卡，决定你的开局资源";

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.20f);
            body.anchorMax = new Vector2(1f, 0.76f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            drawStageRoot = CreateUiObject("DrawStage", body);
            Stretch(drawStageRoot);

            var drawCardOuter = CreateUiObject("DrawCardOuter", drawStageRoot);
            drawCardOuter.anchorMin = new Vector2(0.18f, 0.20f);
            drawCardOuter.anchorMax = new Vector2(0.82f, 0.90f);
            drawCardOuter.offsetMin = Vector2.zero;
            drawCardOuter.offsetMax = Vector2.zero;

            var drawCard = CreateUiObject("DrawCard", drawCardOuter);
            Stretch(drawCard);
            var drawCardImage = drawCard.gameObject.AddComponent<Image>();
            drawCardImage.color = Color.white;
            drawCard.gameObject.AddComponent<UiAutoRounded>();
            var drawCardShadow = drawCard.gameObject.AddComponent<Shadow>();
            drawCardShadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            drawCardShadow.effectDistance = new Vector2(0f, -16f);

            var drawIconWrap = CreateUiObject("IconWrap", drawCard);
            drawIconWrap.anchorMin = new Vector2(0.24f, 0.62f);
            drawIconWrap.anchorMax = new Vector2(0.76f, 0.92f);
            drawIconWrap.offsetMin = Vector2.zero;
            drawIconWrap.offsetMax = Vector2.zero;
            var drawIconWrapImage = drawIconWrap.gameObject.AddComponent<Image>();
            drawIconWrapImage.color = UITheme.CardSky;
            drawIconWrap.gameObject.AddComponent<UiAutoRounded>();
            drawIconWrap.gameObject.AddComponent<UiFloatBob>().Configure(10f, 0.45f, 0f);
            drawCardIconText = CreateText("Icon", drawIconWrap, font, 96, FontStyle.Bold, UITheme.Text);
            Stretch(drawCardIconText.rectTransform);
            drawCardIconText.alignment = TextAnchor.MiddleCenter;
            drawCardIconText.text = "🏠";

            drawCardNameText = CreateText("Name", drawCard, font, 60, FontStyle.Bold, UITheme.Text);
            drawCardNameText.alignment = TextAnchor.MiddleCenter;
            drawCardNameText.rectTransform.anchorMin = new Vector2(0.08f, 0.44f);
            drawCardNameText.rectTransform.anchorMax = new Vector2(0.92f, 0.62f);
            drawCardNameText.rectTransform.offsetMin = Vector2.zero;
            drawCardNameText.rectTransform.offsetMax = Vector2.zero;
            drawCardNameText.text = "抽取中…";

            drawCardDescText = CreateText("Desc", drawCard, font, 36, FontStyle.Normal, UITheme.TextLight);
            drawCardDescText.alignment = TextAnchor.MiddleCenter;
            drawCardDescText.rectTransform.anchorMin = new Vector2(0.10f, 0.18f);
            drawCardDescText.rectTransform.anchorMax = new Vector2(0.90f, 0.44f);
            drawCardDescText.rectTransform.offsetMin = Vector2.zero;
            drawCardDescText.rectTransform.offsetMax = Vector2.zero;
            drawCardDescText.text = "正在抽取你的家庭背景…";

            var barBg = CreateUiObject("ProgressBarBg", drawStageRoot);
            barBg.anchorMin = new Vector2(0.18f, 0.12f);
            barBg.anchorMax = new Vector2(0.82f, 0.16f);
            barBg.offsetMin = Vector2.zero;
            barBg.offsetMax = Vector2.zero;
            var barBgImage = barBg.gameObject.AddComponent<Image>();
            barBgImage.color = new Color32(238, 238, 238, 255);
            barBg.gameObject.AddComponent<UiAutoRounded>();

            var barFill = CreateUiObject("ProgressFill", barBg);
            Stretch(barFill);
            var barFillImage = barFill.gameObject.AddComponent<Image>();
            barFillImage.color = Color.white;
            barFillImage.type = Image.Type.Filled;
            barFillImage.fillMethod = Image.FillMethod.Horizontal;
            barFillImage.fillOrigin = 0;
            barFillImage.fillAmount = 0f;
            barFill.gameObject.AddComponent<UiAutoRounded>();
            var barGrad = barFill.gameObject.AddComponent<UiCornerGradient>();
            barGrad.SetColors(UITheme.Confirm, UITheme.Accent, UITheme.Accent, UITheme.Confirm);
            progressFillImage = barFillImage;

            drawHintText = CreateText("DrawHint", drawStageRoot, font, 36, FontStyle.Normal, UITheme.TextLight);
            drawHintText.alignment = TextAnchor.MiddleCenter;
            drawHintText.rectTransform.anchorMin = new Vector2(0.08f, 0.02f);
            drawHintText.rectTransform.anchorMax = new Vector2(0.92f, 0.10f);
            drawHintText.rectTransform.offsetMin = Vector2.zero;
            drawHintText.rectTransform.offsetMax = Vector2.zero;
            drawHintText.text = "抽卡中…";

            resultStageRoot = CreateUiObject("ResultStage", body);
            Stretch(resultStageRoot);
            resultStageRoot.gameObject.SetActive(false);

            var resultCard = CreateUiObject("ResultCard", resultStageRoot);
            resultCard.anchorMin = new Vector2(0.08f, 0.18f);
            resultCard.anchorMax = new Vector2(0.92f, 0.96f);
            resultCard.offsetMin = Vector2.zero;
            resultCard.offsetMax = Vector2.zero;
            var resultCardImage = resultCard.gameObject.AddComponent<Image>();
            resultCardImage.color = Color.white;
            resultCard.gameObject.AddComponent<UiAutoRounded>();
            var resultShadow = resultCard.gameObject.AddComponent<Shadow>();
            resultShadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            resultShadow.effectDistance = new Vector2(0f, -18f);

            var ribbon = CreateUiObject("Ribbon", resultCard);
            ribbon.anchorMin = new Vector2(0.72f, 0.88f);
            ribbon.anchorMax = new Vector2(0.98f, 0.98f);
            ribbon.offsetMin = Vector2.zero;
            ribbon.offsetMax = Vector2.zero;
            var ribbonImage = ribbon.gameObject.AddComponent<Image>();
            ribbonImage.color = UITheme.Accent;
            ribbon.gameObject.AddComponent<UiAutoRounded>();
            var ribbonText = CreateText("RibbonText", ribbon, font, 28, FontStyle.Bold, Color.white);
            Stretch(ribbonText.rectTransform);
            ribbonText.alignment = TextAnchor.MiddleCenter;
            ribbonText.text = "NEW";

            var resultIconWrap = CreateUiObject("ResultIconWrap", resultCard);
            resultIconWrap.anchorMin = new Vector2(0.32f, 0.70f);
            resultIconWrap.anchorMax = new Vector2(0.68f, 0.90f);
            resultIconWrap.offsetMin = Vector2.zero;
            resultIconWrap.offsetMax = Vector2.zero;
            var resultIconWrapImage = resultIconWrap.gameObject.AddComponent<Image>();
            resultIconWrapImage.color = UITheme.CardSky;
            resultIconWrap.gameObject.AddComponent<UiAutoRounded>();
            resultIconWrap.gameObject.AddComponent<UiFloatBob>().Configure(8f, 0.5f, 0.2f);
            resultIconText = CreateText("ResultIcon", resultIconWrap, font, 92, FontStyle.Bold, UITheme.Text);
            Stretch(resultIconText.rectTransform);
            resultIconText.alignment = TextAnchor.MiddleCenter;
            resultIconText.text = "🏠";

            resultNameText = CreateText("ResultName", resultCard, font, 64, FontStyle.Bold, UITheme.Text);
            resultNameText.alignment = TextAnchor.MiddleCenter;
            resultNameText.rectTransform.anchorMin = new Vector2(0.08f, 0.58f);
            resultNameText.rectTransform.anchorMax = new Vector2(0.92f, 0.70f);
            resultNameText.rectTransform.offsetMin = Vector2.zero;
            resultNameText.rectTransform.offsetMax = Vector2.zero;
            resultNameText.text = "家庭背景";

            resultDescText = CreateText("ResultDesc", resultCard, font, 36, FontStyle.Normal, UITheme.TextLight);
            resultDescText.alignment = TextAnchor.MiddleCenter;
            resultDescText.rectTransform.anchorMin = new Vector2(0.10f, 0.44f);
            resultDescText.rectTransform.anchorMax = new Vector2(0.90f, 0.58f);
            resultDescText.rectTransform.offsetMin = Vector2.zero;
            resultDescText.rectTransform.offsetMax = Vector2.zero;
            resultDescText.text = "描述";

            var moneyChip = CreateUiObject("MoneyChip", resultCard);
            moneyChip.anchorMin = new Vector2(0.30f, 0.36f);
            moneyChip.anchorMax = new Vector2(0.70f, 0.44f);
            moneyChip.offsetMin = Vector2.zero;
            moneyChip.offsetMax = Vector2.zero;
            var moneyChipImage = moneyChip.gameObject.AddComponent<Image>();
            moneyChipImage.color = UITheme.GoldLight;
            moneyChip.gameObject.AddComponent<UiAutoRounded>();
            resultMoneyText = CreateText("MoneyText", moneyChip, font, 34, FontStyle.Bold, UITheme.FromHex("E65100"));
            Stretch(resultMoneyText.rectTransform);
            resultMoneyText.alignment = TextAnchor.MiddleCenter;
            resultMoneyText.text = "金币 0";

            var statGrid = CreateUiObject("StatGrid", resultCard);
            statGrid.anchorMin = new Vector2(0.08f, 0.06f);
            statGrid.anchorMax = new Vector2(0.92f, 0.34f);
            statGrid.offsetMin = Vector2.zero;
            statGrid.offsetMax = Vector2.zero;

            BuildStatItem(statGrid, font, "智力", "🧠", new Vector2(0f, 0.52f), new Vector2(0.48f, 1f), out statIntFill, out statIntValue);
            BuildStatItem(statGrid, font, "心理", "💜", new Vector2(0.52f, 0.52f), new Vector2(1f, 1f), out statPsyFill, out statPsyValue);
            BuildStatItem(statGrid, font, "社交", "💬", new Vector2(0f, 0f), new Vector2(0.48f, 0.48f), out statSocFill, out statSocValue);
            BuildStatItem(statGrid, font, "健康", "💪", new Vector2(0.52f, 0f), new Vector2(1f, 0.48f), out statHealthFill, out statHealthValue);

            EnsureCompareLayout();

            var footer = CreateUiObject("Footer", panel);
            footer.anchorMin = new Vector2(0f, 0.04f);
            footer.anchorMax = new Vector2(1f, 0.18f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            keepButton = CreatePrimaryButton("✅ 就这个了！", footer, font, UITheme.Confirm, Color.white);
            var keepRect = (RectTransform)keepButton.transform;
            keepRect.anchorMin = new Vector2(0.08f, 0.55f);
            keepRect.anchorMax = new Vector2(0.92f, 1f);
            keepRect.offsetMin = Vector2.zero;
            keepRect.offsetMax = Vector2.zero;
            keepButton.gameObject.AddComponent<UiPressScale>();
            keepButton.gameObject.SetActive(false);

            rerollButton = CreateButton("🔄 免费重置（1次）", footer, font, Color.white, UITheme.Text);
            var rerollRect = (RectTransform)rerollButton.transform;
            rerollRect.anchorMin = new Vector2(0.08f, 0.04f);
            rerollRect.anchorMax = new Vector2(0.92f, 0.49f);
            rerollRect.offsetMin = Vector2.zero;
            rerollRect.offsetMax = Vector2.zero;
            var rerollImage = rerollButton.GetComponent<Image>();
            if (rerollImage != null)
            {
                RuntimeArt.ApplyRounded(rerollImage);
                rerollImage.color = Color.white;
            }
            var rerollOutline = rerollButton.gameObject.AddComponent<Outline>();
            rerollOutline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            rerollOutline.effectDistance = new Vector2(4f, -4f);
            rerollButton.gameObject.AddComponent<UiPressScale>();
            rerollButton.gameObject.SetActive(false);
        }

        private void EnsureCompareLayout()
        {
            if (compareStageRoot != null) return;
            if (resultStageRoot == null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            compareStageRoot = CreateUiObject("CompareStage", transform);
            Stretch(compareStageRoot);
            compareStageRoot.gameObject.SetActive(false);

            var mask = CreateUiObject("Mask", compareStageRoot);
            Stretch(mask);
            var maskImage = mask.gameObject.AddComponent<Image>();
            maskImage.color = new Color(0f, 0f, 0f, 0.45f);

            var compareCard = CreateUiObject("CompareCard", compareStageRoot);
            compareCard.anchorMin = new Vector2(0.06f, 0.18f);
            compareCard.anchorMax = new Vector2(0.94f, 0.88f);
            compareCard.offsetMin = Vector2.zero;
            compareCard.offsetMax = Vector2.zero;
            var compareCardImage = compareCard.gameObject.AddComponent<Image>();
            compareCardImage.color = Color.white;
            compareCard.gameObject.AddComponent<UiAutoRounded>();
            var compareShadow = compareCard.gameObject.AddComponent<Shadow>();
            compareShadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            compareShadow.effectDistance = new Vector2(0f, -18f);

            var header = CreateText("CompareTitle", compareCard, font, 44, FontStyle.Bold, UITheme.Text);
            header.alignment = TextAnchor.MiddleCenter;
            header.rectTransform.anchorMin = new Vector2(0.06f, 0.90f);
            header.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            header.rectTransform.offsetMin = Vector2.zero;
            header.rectTransform.offsetMax = Vector2.zero;
            header.text = "重置对比：是否替换为新开局？";

            var left = CreateUiObject("Old", compareCard);
            left.anchorMin = new Vector2(0.04f, 0.62f);
            left.anchorMax = new Vector2(0.48f, 0.88f);
            left.offsetMin = Vector2.zero;
            left.offsetMax = Vector2.zero;
            BuildCompareFamilyCard(left, font, "原来", UITheme.CardButter, out compareOldIconText, out compareOldNameText);

            var right = CreateUiObject("New", compareCard);
            right.anchorMin = new Vector2(0.52f, 0.62f);
            right.anchorMax = new Vector2(0.96f, 0.88f);
            right.offsetMin = Vector2.zero;
            right.offsetMax = Vector2.zero;
            BuildCompareFamilyCard(right, font, "新抽到", UITheme.CardSky, out compareNewIconText, out compareNewNameText);

            var statsPanel = CreateUiObject("StatsPanel", compareCard);
            statsPanel.anchorMin = new Vector2(0.04f, 0.30f);
            statsPanel.anchorMax = new Vector2(0.96f, 0.54f);
            statsPanel.offsetMin = Vector2.zero;
            statsPanel.offsetMax = Vector2.zero;
            var statsPanelImage = statsPanel.gameObject.AddComponent<Image>();
            statsPanelImage.color = new Color32(250, 250, 250, 255);
            statsPanel.gameObject.AddComponent<UiAutoRounded>();

            var headRow = CreateUiObject("HeadRow", statsPanel);
            headRow.anchorMin = new Vector2(0.06f, 0.82f);
            headRow.anchorMax = new Vector2(0.94f, 1.0f);
            headRow.offsetMin = Vector2.zero;
            headRow.offsetMax = Vector2.zero;
            var headLeft = CreateText("OldLabel", headRow, font, 26, FontStyle.Bold, UITheme.TextLight);
            headLeft.alignment = TextAnchor.MiddleLeft;
            headLeft.rectTransform.anchorMin = new Vector2(0.26f, 0f);
            headLeft.rectTransform.anchorMax = new Vector2(0.44f, 1f);
            headLeft.rectTransform.offsetMin = Vector2.zero;
            headLeft.rectTransform.offsetMax = Vector2.zero;
            headLeft.text = "原来";

            var headMid = CreateText("DeltaLabel", headRow, font, 26, FontStyle.Bold, UITheme.TextLight);
            headMid.alignment = TextAnchor.MiddleCenter;
            headMid.rectTransform.anchorMin = new Vector2(0.44f, 0f);
            headMid.rectTransform.anchorMax = new Vector2(0.56f, 1f);
            headMid.rectTransform.offsetMin = Vector2.zero;
            headMid.rectTransform.offsetMax = Vector2.zero;
            headMid.text = "变化";

            var headRight = CreateText("NewLabel", headRow, font, 26, FontStyle.Bold, UITheme.TextLight);
            headRight.alignment = TextAnchor.MiddleRight;
            headRight.rectTransform.anchorMin = new Vector2(0.56f, 0f);
            headRight.rectTransform.anchorMax = new Vector2(0.74f, 1f);
            headRight.rectTransform.offsetMin = Vector2.zero;
            headRight.rectTransform.offsetMax = Vector2.zero;
            headRight.text = "新抽到";

            BuildMoneyCompareRow(statsPanel, font, new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.82f));
            BuildStatCompareRow(statsPanel, font, "智力", "🧠", new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.64f), out compareOldIntFill, out compareOldIntValueText, out compareNewIntFill, out compareNewIntValueText, out compareIntDeltaText);
            BuildStatCompareRow(statsPanel, font, "心理", "💜", new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.48f), out compareOldPsyFill, out compareOldPsyValueText, out compareNewPsyFill, out compareNewPsyValueText, out comparePsyDeltaText);
            BuildStatCompareRow(statsPanel, font, "社交", "💬", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.32f), out compareOldSocFill, out compareOldSocValueText, out compareNewSocFill, out compareNewSocValueText, out compareSocDeltaText);
            BuildStatCompareRow(statsPanel, font, "健康", "💪", new Vector2(0.06f, 0.0f), new Vector2(0.94f, 0.16f), out compareOldHealthFill, out compareOldHealthValueText, out compareNewHealthFill, out compareNewHealthValueText, out compareHealthDeltaText);

            replaceButton = CreatePrimaryButton("✅ 替换为新开局", compareCard, font, UITheme.Confirm, Color.white);
            var replaceRect = (RectTransform)replaceButton.transform;
            replaceRect.anchorMin = new Vector2(0.08f, 0.14f);
            replaceRect.anchorMax = new Vector2(0.92f, 0.26f);
            replaceRect.offsetMin = Vector2.zero;
            replaceRect.offsetMax = Vector2.zero;
            replaceButton.gameObject.AddComponent<UiPressScale>();

            cancelReplaceButton = CreateButton("↩ 保留原来的", compareCard, font, Color.white, UITheme.Text);
            var cancelRect = (RectTransform)cancelReplaceButton.transform;
            cancelRect.anchorMin = new Vector2(0.08f, 0.02f);
            cancelRect.anchorMax = new Vector2(0.92f, 0.12f);
            cancelRect.offsetMin = Vector2.zero;
            cancelRect.offsetMax = Vector2.zero;
            var cancelImage = cancelReplaceButton.GetComponent<Image>();
            if (cancelImage != null)
            {
                RuntimeArt.ApplyRounded(cancelImage);
                cancelImage.color = Color.white;
            }
            var cancelOutline = cancelReplaceButton.gameObject.AddComponent<Outline>();
            cancelOutline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            cancelOutline.effectDistance = new Vector2(4f, -4f);
            cancelReplaceButton.gameObject.AddComponent<UiPressScale>();
        }

        private static void BuildCompareFamilyCard(RectTransform parent, Font font, string header, Color headerBg, out Text iconText, out Text nameText)
        {
            var bg = parent.gameObject.AddComponent<Image>();
            bg.color = new Color32(250, 250, 250, 255);
            parent.gameObject.AddComponent<UiAutoRounded>();

            var head = CreateUiObject("Header", parent);
            head.anchorMin = new Vector2(0.06f, 0.84f);
            head.anchorMax = new Vector2(0.94f, 0.98f);
            head.offsetMin = Vector2.zero;
            head.offsetMax = Vector2.zero;
            var headImage = head.gameObject.AddComponent<Image>();
            headImage.color = headerBg;
            head.gameObject.AddComponent<UiAutoRounded>();
            var headText = CreateText("HeaderText", head, font, 28, FontStyle.Bold, UITheme.TextSoft);
            Stretch(headText.rectTransform);
            headText.alignment = TextAnchor.MiddleCenter;
            headText.text = header;

            var iconWrap = CreateUiObject("IconWrap", parent);
            iconWrap.anchorMin = new Vector2(0.26f, 0.64f);
            iconWrap.anchorMax = new Vector2(0.74f, 0.82f);
            iconWrap.offsetMin = Vector2.zero;
            iconWrap.offsetMax = Vector2.zero;
            var iconWrapImage = iconWrap.gameObject.AddComponent<Image>();
            iconWrapImage.color = Color.white;
            iconWrap.gameObject.AddComponent<UiAutoRounded>();
            iconText = CreateText("Icon", iconWrap, font, 58, FontStyle.Bold, UITheme.Text);
            Stretch(iconText.rectTransform);
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.text = "🏠";

            nameText = CreateText("Name", parent, font, 32, FontStyle.Bold, UITheme.Text);
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.rectTransform.anchorMin = new Vector2(0.06f, 0.54f);
            nameText.rectTransform.anchorMax = new Vector2(0.94f, 0.64f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;
            nameText.text = "家庭";
        }

        private void BuildMoneyCompareRow(RectTransform parent, Font font, Vector2 min, Vector2 max)
        {
            var row = CreateUiObject("Row_Money", parent);
            row.anchorMin = min;
            row.anchorMax = max;
            row.offsetMin = Vector2.zero;
            row.offsetMax = Vector2.zero;

            var iconText = CreateText("Icon", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            iconText.alignment = TextAnchor.MiddleLeft;
            iconText.rectTransform.anchorMin = new Vector2(0f, 0f);
            iconText.rectTransform.anchorMax = new Vector2(0.10f, 1f);
            iconText.rectTransform.offsetMin = Vector2.zero;
            iconText.rectTransform.offsetMax = Vector2.zero;
            iconText.text = "🪙";

            var labelText = CreateText("Label", row, font, 28, FontStyle.Bold, UITheme.TextSoft);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.rectTransform.anchorMin = new Vector2(0.10f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(0.24f, 1f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.text = "金币";

            compareOldMoneyText = CreateText("OldMoney", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            compareOldMoneyText.alignment = TextAnchor.MiddleRight;
            compareOldMoneyText.rectTransform.anchorMin = new Vector2(0.24f, 0f);
            compareOldMoneyText.rectTransform.anchorMax = new Vector2(0.44f, 1f);
            compareOldMoneyText.rectTransform.offsetMin = Vector2.zero;
            compareOldMoneyText.rectTransform.offsetMax = Vector2.zero;
            compareOldMoneyText.text = "0";

            compareMoneyDeltaText = CreateText("Delta", row, font, 26, FontStyle.Bold, UITheme.TextLight);
            compareMoneyDeltaText.alignment = TextAnchor.MiddleCenter;
            compareMoneyDeltaText.rectTransform.anchorMin = new Vector2(0.44f, 0f);
            compareMoneyDeltaText.rectTransform.anchorMax = new Vector2(0.56f, 1f);
            compareMoneyDeltaText.rectTransform.offsetMin = Vector2.zero;
            compareMoneyDeltaText.rectTransform.offsetMax = Vector2.zero;
            compareMoneyDeltaText.text = "0";

            compareNewMoneyText = CreateText("NewMoney", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            compareNewMoneyText.alignment = TextAnchor.MiddleRight;
            compareNewMoneyText.rectTransform.anchorMin = new Vector2(0.56f, 0f);
            compareNewMoneyText.rectTransform.anchorMax = new Vector2(0.76f, 1f);
            compareNewMoneyText.rectTransform.offsetMin = Vector2.zero;
            compareNewMoneyText.rectTransform.offsetMax = Vector2.zero;
            compareNewMoneyText.text = "0";
        }

        private static void BuildStatCompareRow(RectTransform parent, Font font, string label, string icon, Vector2 min, Vector2 max, out Image oldFill, out Text oldValue, out Image newFill, out Text newValue, out Text deltaText)
        {
            var row = CreateUiObject($"Row_{label}", parent);
            row.anchorMin = min;
            row.anchorMax = max;
            row.offsetMin = Vector2.zero;
            row.offsetMax = Vector2.zero;

            var iconText = CreateText("Icon", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            iconText.alignment = TextAnchor.MiddleLeft;
            iconText.rectTransform.anchorMin = new Vector2(0f, 0f);
            iconText.rectTransform.anchorMax = new Vector2(0.10f, 1f);
            iconText.rectTransform.offsetMin = Vector2.zero;
            iconText.rectTransform.offsetMax = Vector2.zero;
            iconText.text = icon;

            var labelText = CreateText("Label", row, font, 28, FontStyle.Bold, UITheme.TextSoft);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.rectTransform.anchorMin = new Vector2(0.10f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(0.24f, 1f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.text = label;

            var oldBarBg = CreateUiObject("OldBarBg", row);
            oldBarBg.anchorMin = new Vector2(0.24f, 0.22f);
            oldBarBg.anchorMax = new Vector2(0.44f, 0.78f);
            oldBarBg.offsetMin = Vector2.zero;
            oldBarBg.offsetMax = Vector2.zero;
            var oldBgImage = oldBarBg.gameObject.AddComponent<Image>();
            oldBgImage.color = new Color32(238, 238, 238, 255);
            oldBarBg.gameObject.AddComponent<UiAutoRounded>();

            var oldFillRect = CreateUiObject("OldFill", oldBarBg);
            Stretch(oldFillRect);
            oldFill = oldFillRect.gameObject.AddComponent<Image>();
            oldFill.color = Color.white;
            oldFillRect.gameObject.AddComponent<UiAutoRounded>();

            oldValue = CreateText("OldValue", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            oldValue.alignment = TextAnchor.MiddleRight;
            oldValue.rectTransform.anchorMin = new Vector2(0.44f, 0f);
            oldValue.rectTransform.anchorMax = new Vector2(0.48f, 1f);
            oldValue.rectTransform.offsetMin = Vector2.zero;
            oldValue.rectTransform.offsetMax = Vector2.zero;
            oldValue.text = "0";

            deltaText = CreateText("Delta", row, font, 26, FontStyle.Bold, UITheme.TextLight);
            deltaText.alignment = TextAnchor.MiddleCenter;
            deltaText.rectTransform.anchorMin = new Vector2(0.48f, 0f);
            deltaText.rectTransform.anchorMax = new Vector2(0.60f, 1f);
            deltaText.rectTransform.offsetMin = Vector2.zero;
            deltaText.rectTransform.offsetMax = Vector2.zero;
            deltaText.text = "0";

            var newBarBg = CreateUiObject("NewBarBg", row);
            newBarBg.anchorMin = new Vector2(0.60f, 0.22f);
            newBarBg.anchorMax = new Vector2(0.80f, 0.78f);
            newBarBg.offsetMin = Vector2.zero;
            newBarBg.offsetMax = Vector2.zero;
            var newBgImage = newBarBg.gameObject.AddComponent<Image>();
            newBgImage.color = new Color32(238, 238, 238, 255);
            newBarBg.gameObject.AddComponent<UiAutoRounded>();

            var newFillRect = CreateUiObject("NewFill", newBarBg);
            Stretch(newFillRect);
            newFill = newFillRect.gameObject.AddComponent<Image>();
            newFill.color = Color.white;
            newFillRect.gameObject.AddComponent<UiAutoRounded>();

            newValue = CreateText("NewValue", row, font, 26, FontStyle.Bold, UITheme.TextSoft);
            newValue.alignment = TextAnchor.MiddleRight;
            newValue.rectTransform.anchorMin = new Vector2(0.80f, 0f);
            newValue.rectTransform.anchorMax = new Vector2(1f, 1f);
            newValue.rectTransform.offsetMin = Vector2.zero;
            newValue.rectTransform.offsetMax = Vector2.zero;
            newValue.text = "0";
        }

        private static void BuildBgDots(RectTransform parent, int count)
        {
            var rand = new System.Random(1337);
            for (int i = 0; i < count; i++)
            {
                var dot = CreateUiObject($"Dot_{i}", parent);
                dot.anchorMin = new Vector2(0.5f, 0.5f);
                dot.anchorMax = new Vector2(0.5f, 0.5f);
                dot.pivot = new Vector2(0.5f, 0.5f);
                var size = (float)(rand.NextDouble() * 48 + 18);
                dot.sizeDelta = new Vector2(size, size);
                var px = (float)(rand.NextDouble() * 960 - 480);
                var py = (float)(rand.NextDouble() * 2080 - 1040);
                dot.anchoredPosition = new Vector2(px, py);
                var image = dot.gameObject.AddComponent<Image>();
                dot.gameObject.AddComponent<UiAutoRounded>();

                var tint = i % 3 == 0 ? UITheme.CardSky : (i % 3 == 1 ? UITheme.CardPeach : UITheme.CardLavender);
                image.color = new Color32(tint.r, tint.g, tint.b, 42);
                dot.gameObject.AddComponent<UiFloatBob>().Configure((float)(rand.NextDouble() * 10 + 6), (float)(rand.NextDouble() * 0.5 + 0.35), (float)rand.NextDouble());
            }
        }

        private static void BuildStatItem(RectTransform parent, Font font, string label, string icon, Vector2 min, Vector2 max, out Image fillImage, out Text valueText)
        {
            var item = CreateUiObject($"Stat_{label}", parent);
            item.anchorMin = min;
            item.anchorMax = max;
            item.offsetMin = Vector2.zero;
            item.offsetMax = Vector2.zero;
            var itemImage = item.gameObject.AddComponent<Image>();
            itemImage.color = new Color32(250, 250, 250, 255);
            item.gameObject.AddComponent<UiAutoRounded>();

            var iconText = CreateText("Icon", item, font, 34, FontStyle.Bold, UITheme.TextSoft);
            iconText.alignment = TextAnchor.MiddleLeft;
            iconText.rectTransform.anchorMin = new Vector2(0.06f, 0.60f);
            iconText.rectTransform.anchorMax = new Vector2(0.26f, 0.94f);
            iconText.rectTransform.offsetMin = Vector2.zero;
            iconText.rectTransform.offsetMax = Vector2.zero;
            iconText.text = icon;

            var labelText = CreateText("Label", item, font, 32, FontStyle.Bold, UITheme.TextSoft);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.rectTransform.anchorMin = new Vector2(0.24f, 0.60f);
            labelText.rectTransform.anchorMax = new Vector2(0.60f, 0.94f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.text = label;

            valueText = CreateText("Value", item, font, 30, FontStyle.Bold, UITheme.TextLight);
            valueText.alignment = TextAnchor.MiddleRight;
            valueText.rectTransform.anchorMin = new Vector2(0.60f, 0.60f);
            valueText.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            valueText.rectTransform.offsetMin = Vector2.zero;
            valueText.rectTransform.offsetMax = Vector2.zero;
            valueText.text = "0/100";

            var barBg = CreateUiObject("BarBg", item);
            barBg.anchorMin = new Vector2(0.06f, 0.16f);
            barBg.anchorMax = new Vector2(0.94f, 0.42f);
            barBg.offsetMin = Vector2.zero;
            barBg.offsetMax = Vector2.zero;
            var barBgImage = barBg.gameObject.AddComponent<Image>();
            barBgImage.color = new Color32(238, 238, 238, 255);
            barBg.gameObject.AddComponent<UiAutoRounded>();

            var barFill = CreateUiObject("BarFill", barBg);
            Stretch(barFill);
            fillImage = barFill.gameObject.AddComponent<Image>();
            fillImage.color = Color.white;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0f;
            barFill.gameObject.AddComponent<UiAutoRounded>();
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
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
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var go = new GameObject(label);
            var rect = go.AddComponent<RectTransform>();
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

            return button;
        }

        private static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var btn = CreateButton(label, parent, font, bgColor, textColor);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = 40;
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

        private void ShowToast(string message)
        {
            Debug.Log($"[FamilyScreen] {message}");
        }

        private sealed class FamilyCandidate
        {
            public Core.FamilyBackgroundType Type;
            public string Name;
            public string Desc;
            public string Icon;
            public int Money;
            public int Intelligence;
            public int Psychology;
            public int Social;
            public int Health;

            public static FamilyCandidate Random()
            {
                var values = new[]
                {
                    Create(Core.FamilyBackgroundType.Rural, "村民之子", "家里条件一般，但你很能吃苦。", "🌾", 500, 58, 52, 46, 62),
                    Create(Core.FamilyBackgroundType.Worker, "工薪之家", "父母踏实勤奋，给你稳定的支持。", "🧰", 900, 62, 58, 52, 60),
                    Create(Core.FamilyBackgroundType.CivilServant, "中产家庭", "生活体面，资源中等偏上。", "🏙", 1200, 66, 60, 58, 58),
                    Create(Core.FamilyBackgroundType.Business, "富裕家庭", "经济宽裕，试错成本更低。", "💎", 1800, 60, 56, 68, 55),
                    Create(Core.FamilyBackgroundType.Intellectual, "书香门第", "重视教育，你从小耳濡目染。", "📚", 1500, 78, 62, 52, 56)
                };

                return values[UnityEngine.Random.Range(0, values.Length)];
            }

            public static FamilyCandidate RandomDifferent(Core.FamilyBackgroundType excludeType)
            {
                FamilyCandidate candidate = null;
                for (int i = 0; i < 10; i++)
                {
                    candidate = Random();
                    if (candidate != null && candidate.Type != excludeType)
                    {
                        return candidate;
                    }
                }

                return candidate ?? Random();
            }

            private static FamilyCandidate Create(Core.FamilyBackgroundType type, string name, string desc, string icon, int money, int intelligence, int psychology, int social, int health)
            {
                return new FamilyCandidate
                {
                    Type = type,
                    Name = name,
                    Desc = desc,
                    Icon = icon,
                    Money = money,
                    Intelligence = intelligence,
                    Psychology = psychology,
                    Social = social,
                    Health = health
                };
            }
        }
    }
}

