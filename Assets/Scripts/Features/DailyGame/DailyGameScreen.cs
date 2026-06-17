using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;
using GaokaoSimulator.Core;

namespace GaokaoSimulator.Features.DailyGame
{
    public class DailyGameScreen : ScreenBase
    {
        // Opening
        private GameObject openingOverlay;
        private Text openingText;
        private bool openingDone;

        // Background area
        private Image bgImage;
        private Text energyLabel;
        private Image energyBar;
        private Text energyNum;
        private Text energyZone;
        private Text intellText;
        private Text psychoText;
        private Text socialText;
        private Text healthText;

        // Stream
        private ScrollRect streamScroll;
        private Transform streamContainer;
        private List<StreamItem> streamItems;
        private int currentStreamIndex;
        private Coroutine typewriterRoutine;

        // Event popup
        private GameObject eventModal;
        private Image eventPortrait;
        private Text eventNarrator;
        private Text eventDialog;
        private Transform optionContainer;
        private Button adButton;
        private Text adButtonText;

        // Result popup
        private GameObject resultModal;
        private Text resultText;
        private Button resultContinueBtn;

        // Ending
        private GameObject blackoutOverlay;
        private Text moonText;

        private List<GameEvent> dailyEvents;
        private int currentEventIndex;
        private bool isEventActive;
        private bool isEnding;

        private Button backBtn;

        protected override void Initialize()
        {
            var content = transform.Find("Content");
            if (content == null)
            {
                BuildRuntimeLayout();
                content = transform.Find("Content");
            }
            InitAll(content);
        }

        protected override void OnScreenOpen()
        {
            StartNewDay();
        }

        private void StartNewDay()
        {
            var gameState = GameState.Instance;
            gameState.DayIndex++;
            gameState.EventsCompleted = 0;
            gameState.DayEnergy = gameState.Energy;
            gameState.DayAdCount = 0;
            gameState.EnergyHistory.Clear();
            gameState.EnergyHistory.Add(gameState.Energy);

            dailyEvents = GameDataProviderRegistry.GetEvents();
            streamItems = GameDataProviderRegistry.GetStreams();
            currentEventIndex = 0;
            currentStreamIndex = 0;
            isEnding = false;
            openingDone = false;

            UpdatePlayerStats();
            ClearStreamContainer();
            GenerateStreamEntries();

            // Show opening first
            openingOverlay.SetActive(true);
            openingText.text = DailyGameData.GetOpeningText();
        }

        public void OnOpeningTap()
        {
            if (openingDone) return;
            openingDone = true;
            openingOverlay.SetActive(false);
            StartCoroutine(PlayStreamSequence());
        }

        private void UpdatePlayerStats()
        {
            var state = GameState.Instance;

            intellText.text = $"智力 {state.StatIntelligence}";
            psychoText.text = $"心理 {state.StatPsychology}";
            socialText.text = $"社交 {state.StatSocial}";
            healthText.text = $"健康 {state.StatHealth}";

            float pct = Mathf.Clamp01(state.Energy / 150f);
            energyBar.fillAmount = pct;
            energyBar.color = DailyGameData.GetEnergyColor(state.Energy);
            energyNum.text = $"{state.Energy}";
            energyZone.text = $"{DailyGameData.GetEnergyEmoji(state.Energy)} {DailyGameData.GetEnergyZone(state.Energy)}";
            energyZone.color = DailyGameData.GetEnergyColor(state.Energy);
        }

        private void ClearStreamContainer()
        {
            foreach (Transform child in streamContainer)
                Destroy(child.gameObject);
        }

        private void GenerateStreamEntries()
        {
            var font = BuiltinFont();
            for (int i = 0; i < streamItems.Count; i++)
            {
                var item = streamItems[i];
                var entry = CreateUiObject($"Stream_{i}", streamContainer);
                var layout = entry.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 120f;
                layout.minHeight = 100f;

                // Time
                var timeLabel = CreateText("Time", entry, font, 48, FontStyle.Bold, UITheme.TextSoft);
                timeLabel.alignment = TextAnchor.MiddleCenter;
                timeLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                timeLabel.rectTransform.anchorMax = new Vector2(0.14f, 1f);
                timeLabel.text = item.Time;

                // Text
                var textObj = CreateText("Text", entry, font, 48, FontStyle.Normal, UITheme.Text);
                textObj.alignment = TextAnchor.MiddleLeft;
                textObj.rectTransform.anchorMin = new Vector2(0.16f, 0f);
                textObj.rectTransform.anchorMax = new Vector2(1f, 1f);
                textObj.horizontalOverflow = HorizontalWrapMode.Wrap;
                textObj.text = "";

                // Event marker
                if (!string.IsNullOrEmpty(item.EventId))
                {
                    var marker = CreateUiObject("EventMarker", entry);
                    marker.anchorMin = new Vector2(0f, 0f);
                    marker.anchorMax = new Vector2(0.008f, 1f);
                    marker.offsetMin = Vector2.zero;
                    marker.offsetMax = Vector2.zero;
                    var markerImg = marker.gameObject.AddComponent<Image>();
                    markerImg.color = new Color32(244, 67, 54, 255);
                }
            }
        }

        private IEnumerator PlayStreamSequence()
        {
            while (currentStreamIndex < streamItems.Count && !isEnding)
            {
                var item = streamItems[currentStreamIndex];
                var entry = streamContainer.GetChild(currentStreamIndex);
                var textObj = entry.Find("Text").GetComponent<Text>();

                yield return StartCoroutine(Typewriter(textObj, item.Text));

                if (!string.IsNullOrEmpty(item.EventId))
                {
                    ShowEventButton(currentStreamIndex);
                    yield break;
                }

                currentStreamIndex++;
                yield return new WaitForSeconds(0.8f);
            }

            if (!isEnding)
                StartCoroutine(PlayEnding());
        }

        private IEnumerator Typewriter(Text textObj, string fullText)
        {
            textObj.text = "";
            float totalTime = Mathf.Min(2.0f, fullText.Length * 0.04f);
            float delay = totalTime / fullText.Length;

            for (int i = 0; i < fullText.Length; i++)
            {
                textObj.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(delay);
            }
        }

        private void ShowEventButton(int idx)
        {
            var entry = streamContainer.GetChild(idx);
            var btnGo = CreateUiObject("EventBtn", entry);
            btnGo.anchorMin = new Vector2(0.55f, 0.05f);
            btnGo.anchorMax = new Vector2(0.98f, 0.95f);

            var btnImg = btnGo.gameObject.AddComponent<Image>();
            btnImg.color = new Color32(244, 67, 54, 255);
            var btn = btnGo.gameObject.AddComponent<Button>();

            var btnLabel = CreateText("Label", btnGo, BuiltinFont(), 44, FontStyle.Bold, Color.white);
            btnLabel.alignment = TextAnchor.MiddleCenter;
            btnLabel.rectTransform.anchorMin = Vector2.zero;
            btnLabel.rectTransform.anchorMax = Vector2.one;
            btnLabel.text = "⚡ 进入事件";

            int captured = idx;
            btn.onClick.AddListener(() => OnEnterEvent(captured));
        }

        private void OnEnterEvent(int idx)
        {
            if (isEventActive || isEnding) return;

            var entry = streamContainer.GetChild(idx);
            var btnT = entry.Find("EventBtn");
            if (btnT != null) Destroy(btnT.gameObject);

            if (currentEventIndex < dailyEvents.Count)
                ShowEvent(dailyEvents[currentEventIndex]);
        }

        private void ShowEvent(GameEvent gameEvent)
        {
            isEventActive = true;
            eventModal.SetActive(true);

            eventNarrator.text = gameEvent.Narrator;
            eventDialog.text = gameEvent.Dialog;

            var state = GameState.Instance;
            string genderPath = state.Gender == Core.Gender.Male ? "boy" : "girl";
            var portraitSprite = Resources.Load<Sprite>($"UI/{genderPath}/普通");
            if (portraitSprite != null)
            {
                eventPortrait.sprite = portraitSprite;
                eventPortrait.color = Color.white;
            }

            foreach (Transform child in optionContainer)
                Destroy(child.gameObject);

            var font = BuiltinFont();
            for (int i = 0; i < gameEvent.Options.Count; i++)
            {
                var opt = gameEvent.Options[i];
                var optIdx = i;

                var btnGo = CreateUiObject("OptionBtn", optionContainer);
                var btnLayout = btnGo.gameObject.AddComponent<LayoutElement>();
                btnLayout.preferredHeight = 120f;

                var btnImg = btnGo.gameObject.AddComponent<Image>();
                Color bgColor = i == 0 ? new Color32(76, 175, 80, 255)
                    : i == 1 ? new Color32(33, 150, 243, 255)
                    : i == 2 ? new Color32(97, 97, 97, 255)
                    : new Color32(255, 152, 0, 255);
                btnImg.color = bgColor;

                var btn = btnGo.gameObject.AddComponent<Button>();
                if (opt.EnergyCost > GameState.Instance.Energy && opt.EnergyCost > 0)
                {
                    btn.interactable = false;
                    btnImg.color = new Color(0.35f, 0.35f, 0.35f);
                }

                // Option name + cost
                var label = CreateText("Label", btnGo, font, 44, FontStyle.Bold, Color.white);
                label.alignment = TextAnchor.MiddleCenter;
                label.rectTransform.anchorMin = new Vector2(0f, 0.3f);
                label.rectTransform.anchorMax = new Vector2(1f, 1f);
                label.text = $"{opt.Emoji} {opt.Name}";

                // Cost line
                var cost = CreateText("Cost", btnGo, font, 32, FontStyle.Normal, Color.white);
                cost.alignment = TextAnchor.MiddleCenter;
                cost.rectTransform.anchorMin = new Vector2(0f, 0f);
                cost.rectTransform.anchorMax = new Vector2(1f, 0.35f);
                cost.text = opt.EnergyCost > 0 ? $"精力-{opt.EnergyCost}" : "免费";

                btn.onClick.AddListener(() => OnOptionSelected(gameEvent, opt, optIdx));
            }

            // Ad button
            UpdateAdButton();
        }

        private void UpdateAdButton()
        {
            var state = GameState.Instance;
            if (state.DayAdCount >= 1)
            {
                adButton.interactable = false;
                adButtonText.text = "明天再用";
            }
            else
            {
                adButton.interactable = true;
                adButtonText.text = "📺 看广告（50%概率回精力）";
            }
        }

        private void OnAdButtonClick()
        {
            var state = GameState.Instance;
            if (state.DayAdCount >= 1) return;
            state.DayAdCount++;
            if (Random.value < 0.5f)
            {
                state.Energy = Mathf.Min(150, state.Energy + 30);
                UpdatePlayerStats();
            }
            UpdateAdButton();
        }

        private void OnOptionSelected(GameEvent gameEvent, EventOption option, int optionIndex)
        {
            if (!isEventActive) return;

            var state = GameState.Instance;
            state.Energy = Mathf.Max(0, state.Energy - option.EnergyCost);
            state.EventsCompleted++;
            state.EnergyHistory.Add(state.Energy);

            ApplyEffects(option.Effects);

            eventModal.SetActive(false);
            isEventActive = false;

            UpdatePlayerStats();

            ShowResult(option);
        }

        private void ApplyEffects(Dictionary<string, int> effects)
        {
            var state = GameState.Instance;
            foreach (var kvp in effects)
            {
                switch (kvp.Key)
                {
                    case "Intelligence": state.StatIntelligence += kvp.Value; break;
                    case "Psychology": state.StatPsychology += kvp.Value; break;
                    case "Social": state.StatSocial += kvp.Value; break;
                    case "Health": state.StatHealth += kvp.Value; break;
                }
            }
        }

        private void ShowResult(EventOption option)
        {
            resultModal.SetActive(true);
            resultText.text = option.ResultText + "\n\n" + option.ResultComment;
        }

        private void ContinueAfterResult()
        {
            resultModal.SetActive(false);
            currentEventIndex++;
            currentStreamIndex++;

            if (currentStreamIndex >= streamItems.Count)
                StartCoroutine(PlayEnding());
            else
                StartCoroutine(PlayStreamSequence());
        }

        private IEnumerator PlayEnding()
        {
            isEnding = true;

            var endStreams = DailyGameData.GetEndStreams();
            var font = BuiltinFont();

            foreach (var item in endStreams)
            {
                var entry = CreateUiObject($"End_{item.Time}", streamContainer);
                var layout = entry.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 120f;

                var timeLabel = CreateText("Time", entry, font, 48, FontStyle.Bold, UITheme.TextSoft);
                timeLabel.alignment = TextAnchor.MiddleCenter;
                timeLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
                timeLabel.rectTransform.anchorMax = new Vector2(0.14f, 1f);
                timeLabel.text = item.Time;

                var textObj = CreateText("Text", entry, font, 48, FontStyle.Normal, UITheme.Text);
                textObj.alignment = TextAnchor.MiddleLeft;
                textObj.rectTransform.anchorMin = new Vector2(0.16f, 0f);
                textObj.rectTransform.anchorMax = new Vector2(1f, 1f);
                textObj.horizontalOverflow = HorizontalWrapMode.Wrap;

                yield return StartCoroutine(Typewriter(textObj, item.Text));
                yield return new WaitForSeconds(0.8f);
            }

            // End quote
            {
                var entry = CreateUiObject("EndQuote", streamContainer);
                var layout = entry.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 120f;

                var textObj = CreateText("Text", entry, font, 50, FontStyle.Bold, UITheme.TextSoft);
                textObj.alignment = TextAnchor.MiddleCenter;
                textObj.rectTransform.anchorMin = Vector2.zero;
                textObj.rectTransform.anchorMax = Vector2.one;

                yield return StartCoroutine(Typewriter(textObj, DailyGameData.GetEndQuote()));
                yield return new WaitForSeconds(1.2f);
            }

            // Blackout
            blackoutOverlay.SetActive(true);
            StartCoroutine(MoonFloat());
            yield return new WaitForSeconds(1.8f);

            var state = GameState.Instance;
            state.Energy = Mathf.Min(150, state.Energy + 30);
            state.SaveGame();
            NavigateTo(ScreenType.DailySettlement, true);
        }

        private IEnumerator MoonFloat()
        {
            float elapsed = 0f;
            var moonRect = moonText.rectTransform;
            float startY = moonRect.anchoredPosition.y;
            while (elapsed < 1.8f)
            {
                elapsed += Time.deltaTime;
                moonRect.anchoredPosition = new Vector2(moonRect.anchoredPosition.x, startY + Mathf.Sin(elapsed * 3f) * 20f);
                yield return null;
            }
        }

        private void GoHome()
        {
            GameState.Instance.SaveGame();
            NavigateTo(ScreenType.Home, true);
        }

        #region Init

        private void InitAll(Transform content)
        {
            // Opening
            openingOverlay = content.Find("Opening").gameObject;
            openingText = openingOverlay.transform.Find("Text").GetComponent<Text>();
            var openBtn = openingOverlay.GetComponent<Button>();
            if (openBtn == null)
            {
                openBtn = openingOverlay.AddComponent<Button>();
                openBtn.onClick.AddListener(OnOpeningTap);
            }
            else
            {
                openBtn.onClick.RemoveAllListeners();
                openBtn.onClick.AddListener(OnOpeningTap);
            }

            // Stats
            intellText = content.Find("BgArea/Stats/Intelligence").GetComponent<Text>();
            psychoText = content.Find("BgArea/Stats/Psychology").GetComponent<Text>();
            socialText = content.Find("BgArea/Stats/Social").GetComponent<Text>();
            healthText = content.Find("BgArea/Stats/Health").GetComponent<Text>();
            energyBar = content.Find("BgArea/EnergyRow/Bar").GetComponent<Image>();
            energyNum = content.Find("BgArea/EnergyRow/Value").GetComponent<Text>();
            energyZone = content.Find("BgArea/EnergyRow/Zone").GetComponent<Text>();

            // Stream
            var streamPanel = content.Find("StreamPanel");
            streamScroll = streamPanel.GetComponent<ScrollRect>();
            streamContainer = streamScroll.content;

            // Event modal
            eventModal = content.Find("EventModal").gameObject;
            eventPortrait = eventModal.transform.Find("Portrait").GetComponent<Image>();
            eventNarrator = eventModal.transform.Find("Narrator").GetComponent<Text>();
            eventDialog = eventModal.transform.Find("Dialog").GetComponent<Text>();
            optionContainer = eventModal.transform.Find("Options");
            adButton = eventModal.transform.Find("AdBtn").GetComponent<Button>();
            adButtonText = adButton.GetComponentInChildren<Text>();
            adButton.onClick.RemoveAllListeners();
            adButton.onClick.AddListener(OnAdButtonClick);

            // Result modal
            resultModal = content.Find("ResultModal").gameObject;
            resultText = resultModal.transform.Find("Text").GetComponent<Text>();
            resultContinueBtn = resultModal.transform.Find("ContinueBtn").GetComponent<Button>();
            resultContinueBtn.onClick.RemoveAllListeners();
            resultContinueBtn.onClick.AddListener(ContinueAfterResult);

            // Blackout
            blackoutOverlay = content.Find("Blackout").gameObject;
            moonText = blackoutOverlay.transform.Find("Moon").GetComponent<Text>();

            // Back button
            var backBtnObj = content.Find("BackBtn");
            if (backBtnObj != null)
            {
                backBtn = backBtnObj.GetComponent<Button>();
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(GoHome);
            }

            eventModal.SetActive(false);
            resultModal.SetActive(false);
            blackoutOverlay.SetActive(false);
        }

        #endregion

        #region Build Runtime Layout

        private void BuildRuntimeLayout()
        {
            var font = BuiltinFont();
            var root = (RectTransform)transform;
            Stretch(root);

            var contentObj = CreateUiObject("Content", root);
            Stretch(contentObj);
            var bg = contentObj.gameObject.AddComponent<Image>();
            bg.color = UITheme.Bg;

            // ===== Opening Overlay =====
            var opening = CreateUiObject("Opening", contentObj);
            openingOverlay = opening.gameObject;
            Stretch(opening);
            opening.gameObject.AddComponent<Button>();
            var openBg = opening.gameObject.AddComponent<Image>();
            openBg.color = new Color(0.05f, 0.05f, 0.1f, 1f);
            openingText = CreateText("Text", opening, font, 80, FontStyle.Bold, Color.white);
            openingText.alignment = TextAnchor.MiddleCenter;
            openingText.rectTransform.anchorMin = new Vector2(0.1f, 0.35f);
            openingText.rectTransform.anchorMax = new Vector2(0.9f, 0.65f);
            openingText.text = DailyGameData.GetOpeningText();

            // ===== Background Area (top 38%) =====
            var bgArea = CreateUiObject("BgArea", contentObj);
            bgArea.anchorMin = new Vector2(0f, 0.62f);
            bgArea.anchorMax = new Vector2(1f, 1f);
            bgArea.offsetMin = Vector2.zero;
            bgArea.offsetMax = Vector2.zero;

            bgImage = bgArea.gameObject.AddComponent<Image>();
            bgImage.color = new Color32(33, 150, 243, 80);

            // Stats overlay on background
            var statsObj = CreateUiObject("Stats", bgArea);
            statsObj.anchorMin = new Vector2(0.04f, 0.15f);
            statsObj.anchorMax = new Vector2(0.96f, 0.55f);

            var statsGrid = statsObj.gameObject.AddComponent<GridLayoutGroup>();
            statsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            statsGrid.constraintCount = 4;
            statsGrid.cellSize = new Vector2(320f, 80f);
            statsGrid.spacing = new Vector2(16f, 0f);

            intellText = CreateStatChip(statsObj, font, "智力");
            psychoText = CreateStatChip(statsObj, font, "心理");
            socialText = CreateStatChip(statsObj, font, "社交");
            healthText = CreateStatChip(statsObj, font, "健康");

            // Energy row
            var energyRow = CreateUiObject("EnergyRow", bgArea);
            energyRow.anchorMin = new Vector2(0.04f, 0f);
            energyRow.anchorMax = new Vector2(0.96f, 0.12f);

            var energyBg = CreateUiObject("BarBg", energyRow);
            energyBg.anchorMin = new Vector2(0f, 0.25f);
            energyBg.anchorMax = new Vector2(0.7f, 0.75f);
            var barBgImg = energyBg.gameObject.AddComponent<Image>();
            barBgImg.color = new Color32(224, 224, 224, 255);

            energyBar = CreateUiObject("Bar", energyRow).gameObject.AddComponent<Image>();
            energyBar.rectTransform.anchorMin = new Vector2(0f, 0.25f);
            energyBar.rectTransform.anchorMax = new Vector2(0.7f, 0.75f);
            energyBar.fillMethod = Image.FillMethod.Horizontal;

            energyNum = CreateText("Value", energyRow, font, 40, FontStyle.Bold, UITheme.Text);
            energyNum.alignment = TextAnchor.MiddleCenter;
            energyNum.rectTransform.anchorMin = new Vector2(0.72f, 0f);
            energyNum.rectTransform.anchorMax = new Vector2(0.82f, 1f);

            energyZone = CreateText("Zone", energyRow, font, 36, FontStyle.Normal, UITheme.Text);
            energyZone.alignment = TextAnchor.MiddleLeft;
            energyZone.rectTransform.anchorMin = new Vector2(0.83f, 0f);
            energyZone.rectTransform.anchorMax = new Vector2(1f, 1f);

            // ===== Stream Panel =====
            var streamPanel = CreateUiObject("StreamPanel", contentObj);
            streamPanel.anchorMin = new Vector2(0.02f, 0.02f);
            streamPanel.anchorMax = new Vector2(0.98f, 0.60f);

            var viewport = CreateUiObject("Viewport", streamPanel);
            Stretch(viewport);
            viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.005f);
            viewport.gameObject.AddComponent<Mask>();

            var scrollContent = CreateUiObject("Content", viewport);
            scrollContent.anchorMin = new Vector2(0f, 0f);
            scrollContent.anchorMax = new Vector2(1f, 0f);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            scrollContent.sizeDelta = new Vector2(0f, 0f);
            streamContainer = scrollContent;

            var contentLayout = streamContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.spacing = 4f;
            contentLayout.padding = new RectOffset(12, 12, 12, 12);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            streamContainer.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = streamPanel.gameObject.AddComponent<ScrollRect>();
            scrollRect.content = scrollContent;
            scrollRect.viewport = viewport;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            streamScroll = scrollRect;

            // ===== Event Modal =====
            var eventModalObj = CreateUiObject("EventModal", contentObj);
            Stretch(eventModalObj);
            eventModal = eventModalObj.gameObject;
            var mOverlay = eventModalObj.gameObject.AddComponent<Image>();
            mOverlay.color = new Color(0f, 0f, 0f, 0.6f);
            eventModalObj.gameObject.AddComponent<Button>();

            // Portrait placeholder
            var portrait = CreateUiObject("Portrait", eventModalObj);
            portrait.anchorMin = new Vector2(0.04f, 0.65f);
            portrait.anchorMax = new Vector2(0.36f, 0.95f);
            eventPortrait = portrait.gameObject.AddComponent<Image>();
            eventPortrait.color = new Color32(200, 200, 200, 255);

            // Narrator name
            eventNarrator = CreateText("Narrator", eventModalObj, font, 52, FontStyle.Bold, Color.white);
            eventNarrator.alignment = TextAnchor.MiddleLeft;
            eventNarrator.rectTransform.anchorMin = new Vector2(0.40f, 0.82f);
            eventNarrator.rectTransform.anchorMax = new Vector2(0.94f, 0.95f);

            // Dialog text
            eventDialog = CreateText("Dialog", eventModalObj, font, 48, FontStyle.Normal, Color.white);
            eventDialog.alignment = TextAnchor.MiddleLeft;
            eventDialog.rectTransform.anchorMin = new Vector2(0.40f, 0.58f);
            eventDialog.rectTransform.anchorMax = new Vector2(0.94f, 0.80f);
            eventDialog.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Options
            var optionsRect = CreateUiObject("Options", eventModalObj);
            optionContainer = optionsRect;
            optionsRect.anchorMin = new Vector2(0.04f, 0.08f);
            optionsRect.anchorMax = new Vector2(0.70f, 0.55f);
            var optLayout = optionContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            optLayout.spacing = 8f;
            optLayout.childControlWidth = true;
            optLayout.childControlHeight = true;
            optLayout.childForceExpandWidth = true;
            optLayout.childForceExpandHeight = false;

            // Ad button
            var adBtnGo = CreateUiObject("AdBtn", eventModalObj);
            adBtnGo.anchorMin = new Vector2(0.74f, 0.08f);
            adBtnGo.anchorMax = new Vector2(0.94f, 0.20f);
            adButton = adBtnGo.gameObject.AddComponent<Button>();
            var adImg = adBtnGo.gameObject.AddComponent<Image>();
            adImg.color = new Color32(255, 152, 0, 255);
            adButtonText = CreateText("Label", adBtnGo, font, 32, FontStyle.Normal, Color.white);
            adButtonText.alignment = TextAnchor.MiddleCenter;
            adButtonText.rectTransform.anchorMin = Vector2.zero;
            adButtonText.rectTransform.anchorMax = Vector2.one;

            // ===== Result Modal =====
            var resultModalObj = CreateUiObject("ResultModal", contentObj);
            Stretch(resultModalObj);
            resultModal = resultModalObj.gameObject;
            var rOverlay = resultModalObj.gameObject.AddComponent<Image>();
            rOverlay.color = new Color(0f, 0f, 0f, 0.55f);

            var resultCard = CreateUiObject("ResultCard", resultModalObj);
            resultCard.anchorMin = new Vector2(0.08f, 0.20f);
            resultCard.anchorMax = new Vector2(0.92f, 0.80f);
            var rCardBg = resultCard.gameObject.AddComponent<Image>();
            rCardBg.color = Color.white;
            RuntimeArt.ApplyRounded(rCardBg);

            resultText = CreateText("Text", resultCard, font, 44, FontStyle.Normal, UITheme.Text);
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.rectTransform.anchorMin = new Vector2(0.06f, 0.20f);
            resultText.rectTransform.anchorMax = new Vector2(0.94f, 0.95f);
            resultText.horizontalOverflow = HorizontalWrapMode.Wrap;

            resultContinueBtn = CreatePrimaryButton("继续", resultCard, font, UITheme.Confirm, UITheme.Text);
            resultContinueBtn.name = "ContinueBtn";
            var rcRect = (RectTransform)resultContinueBtn.transform;
            rcRect.anchorMin = new Vector2(0.2f, 0.04f);
            rcRect.anchorMax = new Vector2(0.8f, 0.16f);
            var rcLayout = resultContinueBtn.GetComponent<LayoutElement>();
            if (rcLayout != null) rcLayout.preferredHeight = 80f;
            var rcLabel = resultContinueBtn.GetComponentInChildren<Text>();
            if (rcLabel != null) rcLabel.fontSize = 40;

            // ===== Blackout =====
            blackoutOverlay = CreateUiObject("Blackout", contentObj).gameObject;
            Stretch((RectTransform)blackoutOverlay.transform);
            blackoutOverlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

            moonText = CreateText("Moon", blackoutOverlay.transform, font, 120, FontStyle.Normal, Color.white);
            moonText.alignment = TextAnchor.MiddleCenter;
            moonText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            moonText.rectTransform.anchorMax = new Vector2(1f, 0.7f);
            moonText.text = "🌙";

            // ===== Back button (top-right) =====
            var backBtnObj = CreateUiObject("BackBtn", contentObj);
            backBtnObj.anchorMin = new Vector2(0.88f, 0.94f);
            backBtnObj.anchorMax = new Vector2(0.98f, 0.99f);
            backBtn = backBtnObj.gameObject.AddComponent<Button>();
            var backImg = backBtnObj.gameObject.AddComponent<Image>();
            backImg.color = new Color32(158, 158, 158, 255);
            var backLabel = CreateText("Label", backBtnObj, font, 36, FontStyle.Bold, Color.white);
            backLabel.alignment = TextAnchor.MiddleCenter;
            backLabel.rectTransform.anchorMin = Vector2.zero;
            backLabel.rectTransform.anchorMax = Vector2.one;
            backLabel.text = "回家";

            // Initial state
            openingOverlay.SetActive(true);
            eventModal.SetActive(false);
            resultModal.SetActive(false);
            blackoutOverlay.SetActive(false);
        }

        private Text CreateStatChip(Transform parent, Font font, string name)
        {
            var item = CreateUiObject(name, parent);
            var label = CreateText("Label", item, font, 38, FontStyle.Normal, UITheme.TextSoft);
            label.alignment = TextAnchor.UpperCenter;
            label.rectTransform.anchorMin = new Vector2(0f, 0.45f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.text = name;

            var value = CreateText("Value", item, font, 48, FontStyle.Bold, UITheme.Text);
            value.alignment = TextAnchor.MiddleCenter;
            value.rectTransform.anchorMin = new Vector2(0f, 0f);
            value.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            value.text = "0";
            return value;
        }

        #endregion
    }
}