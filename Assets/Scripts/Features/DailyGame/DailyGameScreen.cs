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
        private Text openingSubtitle;
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
        private Text streamDayLabel;
        private Text dayTxt;

        // Stream
        private ScrollRect streamScroll;
        private Transform streamContainer;
        private List<StreamItem> streamItems;
        private int currentStreamIndex;
        private Coroutine typewriterRoutine;

        // Event popup
        private GameObject eventModal;
        private Text eventNarrator;
        private Text eventPortrait;
        private Text eventDialog;
        private Text eventTypeTag;
        private Text eventDialogLine;
        private Text eventTurn;
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

        // Pause overlay
        private GameObject pauseOverlay;
        private Button pauseBtn;
        private Text pauseBtnLabel;

        private List<GameEvent> dailyEvents;
        private int currentEventIndex;
        private bool isEventActive;
        private bool isEnding;
        private bool isPaused;

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
            isPaused = false;
            if (pauseOverlay != null) pauseOverlay.SetActive(false);
            StartNewDay();
        }

        protected override void OnScreenClose()
        {
            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
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
            isPaused = false;
            openingDone = false;

            UpdatePlayerStats();
            ClearStreamContainer();
            // GenerateStreamEntries 改为按需动态创建（最多 10 条）

            // Show opening first
            openingOverlay.SetActive(true);
            if (openingText != null) openingText.text = "美好的一天开启了";
            if (openingSubtitle != null)
            {
                openingSubtitle.text = "你将以「显眼包」的身份...";
            }
        }

        public void OnOpeningTap()
        {
            if (openingDone) return;
            openingDone = true;
            openingOverlay.SetActive(false);
            StartCoroutine(PlayStreamSequence());
        }

        private void TogglePause()
        {
            if (openingDone == false || isEventActive) return;
            if (isEnding) return;
            isPaused = !isPaused;
            if (pauseOverlay != null) pauseOverlay.SetActive(isPaused);
            if (isPaused)
            {
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            }
            else
            {
                // resume stream
                StartCoroutine(PlayStreamSequence());
            }
        }

        private void ResumeFromPause()
        {
            isPaused = false;
            if (pauseOverlay != null) pauseOverlay.SetActive(false);
            StartCoroutine(PlayStreamSequence());
        }

        private void PauseGoHome()
        {
            isPaused = false;
            if (pauseOverlay != null) pauseOverlay.SetActive(false);
            GameState.Instance.SaveGame();
            NavigateTo(ScreenType.Home, true);
        }

        private void UpdatePlayerStats()
        {
            var state = GameState.Instance;

            intellText.text = $"{state.StatIntelligence}";
            psychoText.text = $"{state.StatPsychology}";
            socialText.text = $"{state.StatSocial}";
            healthText.text = $"{state.StatHealth}";

            float pct = Mathf.Clamp01(state.Energy / 150f);
            energyBar.fillAmount = pct;
            energyBar.color = DailyGameData.GetEnergyColor(state.Energy);
            energyNum.text = $"{state.Energy}";
            energyZone.text = $"精力 {DailyGameData.GetEnergyZone(state.Energy)}";
            energyZone.color = DailyGameData.GetEnergyColor(state.Energy);
            energyLabel.color = DailyGameData.GetEnergyColor(state.Energy);

            if (streamDayLabel != null && currentStreamIndex < streamItems.Count)
                streamDayLabel.text = $"🕖 {streamItems[currentStreamIndex].Time}";
            if (dayTxt != null) dayTxt.text = $"第 {state.DayIndex} 天";
        }

        private void ClearStreamContainer()
        {
            foreach (Transform child in streamContainer)
                Destroy(child.gameObject);
        }

        private IEnumerator PlayStreamSequence()
        {
            while (currentStreamIndex < streamItems.Count && !isEnding && !isPaused)
            {
                var item = streamItems[currentStreamIndex];
                var textObj = GetOrCreateStreamEntry(currentStreamIndex, item.Time).Find("Text").GetComponent<Text>();

                yield return StartCoroutine(Typewriter(textObj, item.Text));

                if (isPaused) yield break;

                if (!string.IsNullOrEmpty(item.EventId))
                {
                    ShowEventButton(currentStreamIndex);
                    yield break;
                }

                currentStreamIndex++;
                yield return new WaitForSeconds(0.8f);
            }

            if (!isEnding && !isPaused)
                StartCoroutine(PlayEnding());
        }

        private Transform GetOrCreateStreamEntry(int idx, string time)
        {
            const int VISIBLE_MAX = 10;
            // 销毁溢出最早的一条
            while (streamContainer.childCount >= VISIBLE_MAX)
            {
                Destroy(streamContainer.GetChild(0).gameObject);
                // 同步更新 currentStreamIndex 不对（已经++ 了），但我们根据 streamContainer 子节点数判断
            }
            var entry = CreateUiObject($"Stream_{idx}_{time}", streamContainer);
            var layout = entry.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 70f;
            layout.minHeight = 60f;

            var timeLabel = CreateText("Time", entry, BuiltinFont(), 28, FontStyle.Bold, GetTimeColor(time));
            timeLabel.alignment = TextAnchor.MiddleCenter;
            timeLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            timeLabel.rectTransform.anchorMax = new Vector2(0.15f, 1f);
            timeLabel.text = time;

            var textObj = CreateText("Text", entry, BuiltinFont(), 28, FontStyle.Normal, UITheme.Text);
            textObj.alignment = TextAnchor.MiddleLeft;
            textObj.rectTransform.anchorMin = new Vector2(0.17f, 0f);
            textObj.rectTransform.anchorMax = new Vector2(0.75f, 1f);
            textObj.horizontalOverflow = HorizontalWrapMode.Wrap;
            textObj.text = "";

            return entry.transform;
        }

        private IEnumerator Typewriter(Text textObj, string fullText)
        {
            textObj.text = "";
            if (string.IsNullOrEmpty(fullText)) yield break;
            float totalTime = Mathf.Min(1.4f, fullText.Length * 0.035f);
            float delay = totalTime / fullText.Length;

            for (int i = 0; i < fullText.Length; i++)
            {
                if (isPaused || isEnding) yield break;
                textObj.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(delay);
            }
        }

        private void ShowEventButton(int idx)
        {
            // 找到当前流水对应的 entry
            var entry = streamContainer.Find($"Stream_{idx}_{streamItems[idx].Time}");
            if (entry == null) return;

            var btnGo = CreateUiObject("EventBtn", entry);
            btnGo.anchorMin = new Vector2(0.72f, 0.05f);
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

            var entry = streamContainer.Find($"Stream_{idx}_{streamItems[idx].Time}");
            if (entry != null)
            {
                var btnT = entry.Find("EventBtn");
                if (btnT != null) Destroy(btnT.gameObject);
            }

            if (currentEventIndex < dailyEvents.Count)
                ShowEvent(dailyEvents[currentEventIndex]);
        }

        private void ShowEvent(GameEvent gameEvent)
        {
            isEventActive = true;
            eventModal.SetActive(true);

            eventNarrator.text = "⚡ 突发事件";
            eventTypeTag.text = $"{gameEvent.Category} · 难度 {'★' * Mathf.Min(5, gameEvent.Difficulty)} · {gameEvent.Time}";
            eventPortrait.text = gameEvent.Narrator;
            eventDialog.text = gameEvent.Dialog;
            eventDialogLine.text = $"{gameEvent.Narrator}：{gameEvent.Dialog}";
            eventTurn.text = $"{currentEventIndex + 1}";

            foreach (Transform child in optionContainer)
                Destroy(child.gameObject);

            var font = BuiltinFont();
            for (int i = 0; i < gameEvent.Options.Count; i++)
            {
                var opt = gameEvent.Options[i];
                var optIdx = i;

                var btnGo = CreateUiObject("OptionBtn", optionContainer);
                var btnLayout = btnGo.gameObject.AddComponent<LayoutElement>();
                btnLayout.preferredHeight = 100f;

                var btnImg = btnGo.gameObject.AddComponent<Image>();
                Color bgColor = i == 0 ? new Color32(255, 107, 107, 255)
                    : i == 1 ? new Color32(33, 150, 243, 255)
                    : i == 2 ? new Color32(123, 192, 107, 255)
                    : new Color32(255, 152, 0, 255);
                btnImg.color = bgColor;
                RuntimeArt.ApplyRounded(btnImg);

                var btn = btnGo.gameObject.AddComponent<Button>();
                if (opt.EnergyCost > GameState.Instance.Energy && opt.EnergyCost > 0)
                {
                    btn.interactable = false;
                    btnImg.color = new Color(0.55f, 0.55f, 0.55f);
                }

                // 行动类型标签（破局/稳进/保守）
                var actionLabel = CreateText("Action", btnGo, font, 24, FontStyle.Bold, Color.white);
                actionLabel.alignment = TextAnchor.MiddleLeft;
                actionLabel.rectTransform.anchorMin = new Vector2(0.04f, 0.55f);
                actionLabel.rectTransform.anchorMax = new Vector2(0.55f, 0.92f);
                actionLabel.text = $"{opt.Emoji} {opt.Name}";

                // 收益行（绿/红）
                var eff = new System.Text.StringBuilder();
                if (opt.Effects != null)
                {
                    foreach (var kvp in opt.Effects)
                    {
                        string color = kvp.Value > 0 ? "思" : "⚡";
                        eff.Append($"{color}{kvp.Key.Substring(0, 1)} {(kvp.Value > 0 ? "+" : "")}{kvp.Value}({Mathf.Abs(kvp.Value * 9 / 10 + 9)}) ");
                    }
                }
                var effectLine = CreateText("Effect", btnGo, font, 22, FontStyle.Normal, Color.white);
                effectLine.alignment = TextAnchor.MiddleLeft;
                effectLine.rectTransform.anchorMin = new Vector2(0.04f, 0.05f);
                effectLine.rectTransform.anchorMax = new Vector2(0.98f, 0.55f);
                effectLine.text = eff.ToString();

                // 消耗徽章（右侧）
                if (opt.EnergyCost > 0)
                {
                    var cost = CreateText("Cost", btnGo, font, 22, FontStyle.Bold, new Color32(255, 200, 100, 255));
                    cost.alignment = TextAnchor.MiddleRight;
                    cost.rectTransform.anchorMin = new Vector2(0.65f, 0.10f);
                    cost.rectTransform.anchorMax = new Vector2(0.96f, 0.90f);
                    cost.text = $"⚡精力-{opt.EnergyCost}";
                }

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

            // 添加最后一条收尾
            var endStream = new StreamItem { Time = "23:30", Text = "今天就这样过去了...", EventId = "" };
            var finalEntry = GetOrCreateStreamEntry(streamItems.Count, endStream.Time);
            var finalText = finalEntry.Find("Text").GetComponent<Text>();
            yield return StartCoroutine(Typewriter(finalText, endStream.Text));
            yield return new WaitForSeconds(0.6f);

            // 收尾总结条目
            var daySummary = $"第 {GameState.Instance.DayIndex} 天，结束了。";
            var summaryEntry = GetOrCreateStreamEntry(streamItems.Count + 1, "总结");
            var summaryText = summaryEntry.Find("Text").GetComponent<Text>();
            yield return StartCoroutine(Typewriter(summaryText, daySummary));
            yield return new WaitForSeconds(1.0f);

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

        #region Init

        private void InitAll(Transform content)
        {
            // Opening
            openingOverlay = content.Find("Opening").gameObject;
            openingText = openingOverlay.transform.Find("Card/Text").GetComponent<Text>();
            openingSubtitle = openingOverlay.transform.Find("Card/Subtitle").GetComponent<Text>();
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
            // 点击 card 也算触发
            var cardBtn = openingOverlay.transform.Find("Card").GetComponent<Button>();
            if (cardBtn != null)
            {
                cardBtn.onClick.RemoveAllListeners();
                cardBtn.onClick.AddListener(OnOpeningTap);
            }

            // Stats (V2 - flat 4 cards)
            intellText = content.Find("HUD/智力/Value").GetComponent<Text>();
            psychoText = content.Find("HUD/纪律/Value").GetComponent<Text>();
            socialText = content.Find("HUD/社交/Value").GetComponent<Text>();
            healthText = content.Find("HUD/健康/Value").GetComponent<Text>();
            energyLabel = content.Find("EnergyBar/Emoji").GetComponent<Text>();
            energyBar = content.Find("EnergyBar/BarBg/Bar").GetComponent<Image>();
            energyNum = content.Find("EnergyBar/Value").GetComponent<Text>();
            energyZone = content.Find("EnergyBar/Zone").GetComponent<Text>();
            streamDayLabel = content.Find("InfoRow/TimeChip/Txt").GetComponent<Text>();
            dayTxt = content.Find("InfoRow/DayChip/Txt").GetComponent<Text>();

            // Stream
            var streamPanel = content.Find("StreamPanel");
            streamScroll = streamPanel.GetComponent<ScrollRect>();
            streamContainer = streamScroll.content;

            // Event modal (V2)
            eventModal = content.Find("EventModal").gameObject;
            eventNarrator = eventModal.transform.Find("EventCard/TitleBar/Narrator").GetComponent<Text>();
            eventPortrait = eventModal.transform.Find("EventCard/EventDescCard/Title").GetComponent<Text>();
            eventDialog = eventModal.transform.Find("EventCard/EventDescCard/Desc").GetComponent<Text>();
            optionContainer = eventModal.transform.Find("EventCard/Options");
            adButton = eventModal.transform.Find("EventCard/AdBtn").GetComponent<Button>();
            adButtonText = adButton.GetComponentInChildren<Text>();
            adButton.onClick.RemoveAllListeners();
            adButton.onClick.AddListener(OnAdButtonClick);

            // Result modal
            resultModal = content.Find("ResultModal").gameObject;
            resultText = resultModal.transform.Find("ResultCard/Text").GetComponent<Text>();
            resultContinueBtn = resultModal.transform.Find("ResultCard/ContinueBtn").GetComponent<Button>();
            resultContinueBtn.onClick.RemoveAllListeners();
            resultContinueBtn.onClick.AddListener(ContinueAfterResult);

            // Blackout
            blackoutOverlay = content.Find("Blackout").gameObject;
            moonText = blackoutOverlay.transform.Find("Moon").GetComponent<Text>();

            // Pause overlay
            pauseOverlay = content.Find("PauseOverlay").gameObject;
            var resumeBtn = pauseOverlay.transform.Find("Card/ResumeBtn").GetComponent<Button>();
            resumeBtn.onClick.RemoveAllListeners();
            resumeBtn.onClick.AddListener(ResumeFromPause);
            var homeBtn = pauseOverlay.transform.Find("Card/HomeBtn").GetComponent<Button>();
            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(PauseGoHome);

            // Pause button (top-right)
            pauseBtn = content.Find("PauseBtn").GetComponent<Button>();
            pauseBtnLabel = pauseBtn.GetComponentInChildren<Text>();
            pauseBtn.onClick.RemoveAllListeners();
            pauseBtn.onClick.AddListener(TogglePause);

            eventModal.SetActive(false);
            resultModal.SetActive(false);
            blackoutOverlay.SetActive(false);
            pauseOverlay.SetActive(false);
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
            bg.color = new Color32(255, 248, 240, 255); // 暖米色 #FFF8F0

            // ===== Top HUD: 4 属性 + 精力条 + 卡组/时间 (5 行) =====
            // 1) 属性格（4 个圆角卡）
            var hud = CreateUiObject("HUD", contentObj);
            hud.anchorMin = new Vector2(0.02f, 0.88f);
            hud.anchorMax = new Vector2(0.98f, 0.98f);

            var hudGrid = hud.gameObject.AddComponent<GridLayoutGroup>();
            hudGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            hudGrid.constraintCount = 4;
            hudGrid.cellSize = new Vector2(160f, 78f);
            hudGrid.spacing = new Vector2(6f, 0f);
            hudGrid.padding = new RectOffset(4, 4, 4, 4);

            intellText = CreateStatChipV2(hud, font, "智力", "学习能力", "📚");
            psychoText = CreateStatChipV2(hud, font, "纪律", "情绪管理", "🎯");
            socialText = CreateStatChipV2(hud, font, "社交", "人际关系", "💖");
            healthText = CreateStatChipV2(hud, font, "健康", "健康状态", "💪");

            // 2) 精力条（横向）
            var energyBar2 = CreateUiObject("EnergyBar", contentObj);
            energyBar2.anchorMin = new Vector2(0.02f, 0.81f);
            energyBar2.anchorMax = new Vector2(0.98f, 0.87f);

            var ehbBg = energyBar2.gameObject.AddComponent<Image>();
            ehbBg.color = new Color32(255, 255, 255, 235);
            RuntimeArt.ApplyRounded(ehbBg);
            var ehbShadow = energyBar2.gameObject.AddComponent<Shadow>();
            ehbShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            ehbShadow.effectDistance = new Vector2(0f, -2f);

            // 左侧 emoji
            energyLabel = CreateText("Emoji", energyBar2, font, 30, FontStyle.Normal, new Color32(255, 158, 92, 255));
            energyLabel.alignment = TextAnchor.MiddleCenter;
            energyLabel.rectTransform.anchorMin = new Vector2(0.01f, 0.0f);
            energyLabel.rectTransform.anchorMax = new Vector2(0.06f, 1f);
            energyLabel.text = "⚡";

            energyZone = CreateText("Zone", energyBar2, font, 24, FontStyle.Bold, new Color32(255, 158, 92, 255));
            energyZone.alignment = TextAnchor.MiddleLeft;
            energyZone.rectTransform.anchorMin = new Vector2(0.07f, 0.0f);
            energyZone.rectTransform.anchorMax = new Vector2(0.20f, 1f);
            energyZone.text = "精力 良好";

            // 进度条背景
            var barBg2 = CreateUiObject("BarBg", energyBar2);
            barBg2.anchorMin = new Vector2(0.22f, 0.30f);
            barBg2.anchorMax = new Vector2(0.78f, 0.70f);
            var barBg2Img = barBg2.gameObject.AddComponent<Image>();
            barBg2Img.color = new Color32(240, 224, 208, 255);

            energyBar = CreateUiObject("Bar", barBg2).gameObject.AddComponent<Image>();
            energyBar.rectTransform.anchorMin = new Vector2(0f, 0f);
            energyBar.rectTransform.anchorMax = new Vector2(1f, 1f);
            energyBar.fillMethod = Image.FillMethod.Horizontal;
            energyBar.color = new Color32(123, 192, 107, 255);

            // 右侧数值 + 乘数
            energyNum = CreateText("Value", energyBar2, font, 30, FontStyle.Bold, new Color32(74, 58, 46, 255));
            energyNum.alignment = TextAnchor.MiddleRight;
            energyNum.rectTransform.anchorMin = new Vector2(0.78f, 0f);
            energyNum.rectTransform.anchorMax = new Vector2(0.92f, 1f);
            energyNum.text = "99";

            var mult = CreateText("Mult", energyBar2, font, 20, FontStyle.Bold, new Color32(255, 158, 92, 255));
            mult.alignment = TextAnchor.MiddleCenter;
            mult.rectTransform.anchorMin = new Vector2(0.92f, 0.1f);
            mult.rectTransform.anchorMax = new Vector2(0.99f, 0.9f);
            mult.text = "×1.1";

            // 3) 卡组 / 时间行
            var infoRow = CreateUiObject("InfoRow", contentObj);
            infoRow.anchorMin = new Vector2(0.02f, 0.77f);
            infoRow.anchorMax = new Vector2(0.98f, 0.81f);

            var deckChip = CreateUiObject("DeckChip", infoRow);
            deckChip.anchorMin = new Vector2(0f, 0f);
            deckChip.anchorMax = new Vector2(0.18f, 1f);
            var deckBg = deckChip.gameObject.AddComponent<Image>();
            deckBg.color = new Color32(255, 255, 255, 255);
            RuntimeArt.ApplyRounded(deckBg);
            var deckTxt = CreateText("Txt", deckChip, font, 24, FontStyle.Bold, new Color32(107, 157, 247, 255));
            deckTxt.alignment = TextAnchor.MiddleCenter;
            deckTxt.rectTransform.anchorMin = Vector2.zero;
            deckTxt.rectTransform.anchorMax = Vector2.one;
            deckTxt.text = "🃏 卡组 20";

            var timeChip = CreateUiObject("TimeChip", infoRow);
            timeChip.anchorMin = new Vector2(0.20f, 0f);
            timeChip.anchorMax = new Vector2(0.40f, 1f);
            var timeBg = timeChip.gameObject.AddComponent<Image>();
            timeBg.color = new Color32(255, 255, 255, 255);
            RuntimeArt.ApplyRounded(timeBg);
            var timeTxt = CreateText("Txt", timeChip, font, 24, FontStyle.Bold, new Color32(155, 126, 110, 255));
            timeTxt.alignment = TextAnchor.MiddleCenter;
            timeTxt.rectTransform.anchorMin = Vector2.zero;
            timeTxt.rectTransform.anchorMax = Vector2.one;
            timeTxt.text = "🕖 07:30";
            streamDayLabel = timeTxt; // 用作"第N天"显示
            streamDayLabel.text = $"🕖 07:30";

            var dayChip = CreateUiObject("DayChip", infoRow);
            dayChip.anchorMin = new Vector2(0.70f, 0f);
            dayChip.anchorMax = new Vector2(0.98f, 1f);
            var dayBg = dayChip.gameObject.AddComponent<Image>();
            dayBg.color = new Color32(255, 255, 255, 255);
            RuntimeArt.ApplyRounded(dayBg);
            dayTxt = CreateText("Txt", dayChip, font, 24, FontStyle.Bold, new Color32(255, 107, 107, 255));
            dayTxt.alignment = TextAnchor.MiddleCenter;
            dayTxt.rectTransform.anchorMin = Vector2.zero;
            dayTxt.rectTransform.anchorMax = Vector2.one;
            dayTxt.text = "第 1 天";

            // ===== Stream Panel =====
            var streamPanel = CreateUiObject("StreamPanel", contentObj);
            streamPanel.anchorMin = new Vector2(0.02f, 0.02f);
            streamPanel.anchorMax = new Vector2(0.98f, 0.80f);

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

            // ===== Event Modal (V2 - 大圆角卡) =====
            var eventModalObj = CreateUiObject("EventModal", contentObj);
            Stretch(eventModalObj);
            eventModal = eventModalObj.gameObject;
            var mOverlay = eventModalObj.gameObject.AddComponent<Image>();
            mOverlay.color = new Color(0f, 0f, 0f, 0.5f);
            eventModalObj.gameObject.AddComponent<Button>();
            var eventCanvas = eventModalObj.gameObject.AddComponent<Canvas>();
            eventCanvas.sortingOrder = 100;

            // 大圆角主卡
            var eventCard = CreateUiObject("EventCard", eventModalObj);
            eventCard.anchorMin = new Vector2(0.05f, 0.08f);
            eventCard.anchorMax = new Vector2(0.95f, 0.92f);
            eventCard.offsetMin = Vector2.zero;
            eventCard.offsetMax = Vector2.zero;
            var eventCardImg = eventCard.gameObject.AddComponent<Image>();
            eventCardImg.color = new Color32(255, 248, 240, 255);
            RuntimeArt.ApplyRounded(eventCardImg);
            var eventCardShadow = eventCard.gameObject.AddComponent<Shadow>();
            eventCardShadow.effectColor = new Color(0f, 0f, 0f, 0.15f);
            eventCardShadow.effectDistance = new Vector2(0f, -8f);

            // 标题"突发事件"+ 右侧回合数字徽章
            var titleBar = CreateUiObject("TitleBar", eventCard);
            titleBar.anchorMin = new Vector2(0.04f, 0.92f);
            titleBar.anchorMax = new Vector2(0.96f, 0.99f);

            eventNarrator = CreateText("Narrator", titleBar, font, 36, FontStyle.Bold, new Color32(255, 107, 107, 255));
            eventNarrator.alignment = TextAnchor.MiddleLeft;
            eventNarrator.rectTransform.anchorMin = new Vector2(0f, 0f);
            eventNarrator.rectTransform.anchorMax = new Vector2(0.7f, 1f);
            eventNarrator.text = "⚡ 突发事件";

            eventTurn = CreateText("Turn", titleBar, font, 36, FontStyle.Bold, new Color32(255, 107, 107, 255));
            eventTurn.alignment = TextAnchor.MiddleCenter;
            eventTurn.rectTransform.anchorMin = new Vector2(0.85f, 0.05f);
            eventTurn.rectTransform.anchorMax = new Vector2(0.98f, 0.95f);
            eventTurn.text = "59";

            // 事件描述卡（浅蓝渐变 + 类型标签 + 标题 + 描述 + 对话）
            var eventDescCard = CreateUiObject("EventDescCard", eventCard);
            eventDescCard.anchorMin = new Vector2(0.04f, 0.55f);
            eventDescCard.anchorMax = new Vector2(0.96f, 0.90f);
            var edcImg = eventDescCard.gameObject.AddComponent<Image>();
            edcImg.color = new Color32(229, 240, 251, 255);
            RuntimeArt.ApplyRounded(edcImg);

            // 类型标签
            eventTypeTag = CreateText("TypeTag", eventDescCard, font, 24, FontStyle.Bold, new Color32(107, 157, 247, 255));
            eventTypeTag.alignment = TextAnchor.MiddleLeft;
            eventTypeTag.rectTransform.anchorMin = new Vector2(0.03f, 0.80f);
            eventTypeTag.rectTransform.anchorMax = new Vector2(0.97f, 0.96f);
            eventTypeTag.text = "思事件 · 难度 ★★★ · 10:00";

            // 事件名称
            eventPortrait = CreateText("Title", eventDescCard, font, 30, FontStyle.Bold, new Color32(74, 58, 46, 255));
            eventPortrait.alignment = TextAnchor.MiddleLeft;
            eventPortrait.rectTransform.anchorMin = new Vector2(0.03f, 0.55f);
            eventPortrait.rectTransform.anchorMax = new Vector2(0.97f, 0.80f);
            eventPortrait.text = "数学课压力脑";

            // 描述
            eventDialog = CreateText("Desc", eventDescCard, font, 24, FontStyle.Normal, new Color32(74, 58, 46, 255));
            eventDialog.alignment = TextAnchor.UpperLeft;
            eventDialog.rectTransform.anchorMin = new Vector2(0.03f, 0.22f);
            eventDialog.rectTransform.anchorMax = new Vector2(0.97f, 0.55f);
            eventDialog.horizontalOverflow = HorizontalWrapMode.Wrap;
            eventDialog.text = "数学老师在黑板上写了一道压轴题...";

            // 说话人/对话
            eventDialogLine = CreateText("DialogLine", eventDescCard, font, 24, FontStyle.Bold, new Color32(255, 107, 107, 255));
            eventDialogLine.alignment = TextAnchor.MiddleLeft;
            eventDialogLine.rectTransform.anchorMin = new Vector2(0.03f, 0.02f);
            eventDialogLine.rectTransform.anchorMax = new Vector2(0.97f, 0.22f);
            eventDialogLine.text = "数学老师：这道题谁能做出来？";

            // 3 个选项（垂直）
            var optionsRect = CreateUiObject("Options", eventCard);
            optionContainer = optionsRect;
            optionsRect.anchorMin = new Vector2(0.04f, 0.15f);
            optionsRect.anchorMax = new Vector2(0.96f, 0.53f);
            var optLayout = optionContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            optLayout.spacing = 8f;
            optLayout.childControlWidth = true;
            optLayout.childControlHeight = true;
            optLayout.childForceExpandWidth = true;
            optLayout.childForceExpandHeight = false;

            // 看广告按钮
            var adBtnGo = CreateUiObject("AdBtn", eventCard);
            adBtnGo.anchorMin = new Vector2(0.04f, 0.02f);
            adBtnGo.anchorMax = new Vector2(0.96f, 0.13f);
            adButton = adBtnGo.gameObject.AddComponent<Button>();
            var adImg = adBtnGo.gameObject.AddComponent<Image>();
            adImg.color = new Color32(255, 251, 240, 255);
            RuntimeArt.ApplyRounded(adImg);
            var adStroke = adBtnGo.gameObject.AddComponent<Outline>();
            adStroke.effectColor = new Color32(255, 213, 128, 255);
            adStroke.effectDistance = new Vector2(0, 0);
            adButtonText = CreateText("Label", adBtnGo, font, 26, FontStyle.Bold, new Color32(255, 158, 92, 255));
            adButtonText.alignment = TextAnchor.MiddleCenter;
            adButtonText.rectTransform.anchorMin = Vector2.zero;
            adButtonText.rectTransform.anchorMax = Vector2.one;
            adButtonText.text = "📺 看广告（50%概率回精力/补卡）";

            // ===== Result Modal =====
            var resultModalObj = CreateUiObject("ResultModal", contentObj);
            Stretch(resultModalObj);
            resultModal = resultModalObj.gameObject;
            var rOverlay = resultModalObj.gameObject.AddComponent<Image>();
            rOverlay.color = new Color(0f, 0f, 0f, 0.55f);
            var resultCanvas = resultModalObj.gameObject.AddComponent<Canvas>();
            resultCanvas.sortingOrder = 100;

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
            var blackoutCanvas = blackoutOverlay.AddComponent<Canvas>();
            blackoutCanvas.sortingOrder = 100;

            moonText = CreateText("Moon", blackoutOverlay.transform, font, 120, FontStyle.Normal, Color.white);
            moonText.alignment = TextAnchor.MiddleCenter;
            moonText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            moonText.rectTransform.anchorMax = new Vector2(1f, 0.7f);
            moonText.text = "🌙";

            // ===== Pause button (top-right) =====
            var pauseBtnObj = CreateUiObject("PauseBtn", contentObj);
            pauseBtnObj.anchorMin = new Vector2(0.88f, 0.93f);
            pauseBtnObj.anchorMax = new Vector2(0.98f, 0.99f);
            pauseBtnObj.offsetMin = Vector2.zero;
            pauseBtnObj.offsetMax = Vector2.zero;
            pauseBtn = pauseBtnObj.gameObject.AddComponent<Button>();
            var pauseImg = pauseBtnObj.gameObject.AddComponent<Image>();
            pauseImg.color = new Color(1f, 0.95f, 0.86f, 0.95f);
            RuntimeArt.ApplyRounded(pauseImg);
            var pauseShadow = pauseBtnObj.gameObject.AddComponent<Shadow>();
            pauseShadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            pauseShadow.effectDistance = new Vector2(0f, -2f);
            var pauseBtnCanvas = pauseBtnObj.gameObject.AddComponent<Canvas>();
            pauseBtnCanvas.sortingOrder = 50;
            pauseBtnLabel = CreateText("Label", pauseBtnObj, font, 38, FontStyle.Bold, new Color32(180, 90, 50, 255));
            pauseBtnLabel.alignment = TextAnchor.MiddleCenter;
            pauseBtnLabel.rectTransform.anchorMin = Vector2.zero;
            pauseBtnLabel.rectTransform.anchorMax = Vector2.one;
            pauseBtnLabel.text = "⏸";

            // ===== Pause Overlay (暂停中：继续 / 回家) =====
            var pauseOverlayObj = CreateUiObject("PauseOverlay", contentObj);
            Stretch(pauseOverlayObj);
            pauseOverlay = pauseOverlayObj.gameObject;
            var pauseBg = pauseOverlayObj.gameObject.AddComponent<Image>();
            pauseBg.color = new Color(0.06f, 0.06f, 0.10f, 0.78f);
            pauseOverlayObj.gameObject.SetActive(false);
            var pauseCanvas = pauseOverlayObj.gameObject.AddComponent<Canvas>();
            pauseCanvas.sortingOrder = 100;

            var pauseCard = CreateUiObject("Card", pauseOverlayObj);
            pauseCard.anchorMin = new Vector2(0.18f, 0.28f);
            pauseCard.anchorMax = new Vector2(0.82f, 0.72f);
            pauseCard.offsetMin = Vector2.zero;
            pauseCard.offsetMax = Vector2.zero;
            var pauseCardImg = pauseCard.gameObject.AddComponent<Image>();
            pauseCardImg.color = Color.white;
            RuntimeArt.ApplyRounded(pauseCardImg);
            var pauseCardShadow = pauseCard.gameObject.AddComponent<Shadow>();
            pauseCardShadow.effectColor = new Color(0f, 0f, 0f, 0.20f);
            pauseCardShadow.effectDistance = new Vector2(0f, -10f);

            var pauseEmoji = CreateText("Emoji", pauseCard, font, 100, FontStyle.Normal, UITheme.Accent);
            pauseEmoji.alignment = TextAnchor.MiddleCenter;
            pauseEmoji.rectTransform.anchorMin = new Vector2(0f, 0.70f);
            pauseEmoji.rectTransform.anchorMax = new Vector2(1f, 0.96f);
            pauseEmoji.rectTransform.offsetMin = Vector2.zero;
            pauseEmoji.rectTransform.offsetMax = Vector2.zero;
            pauseEmoji.text = "⏸";

            var pauseTitle = CreateText("Title", pauseCard, font, 48, FontStyle.Bold, UITheme.Text);
            pauseTitle.alignment = TextAnchor.MiddleCenter;
            pauseTitle.rectTransform.anchorMin = new Vector2(0f, 0.52f);
            pauseTitle.rectTransform.anchorMax = new Vector2(1f, 0.70f);
            pauseTitle.rectTransform.offsetMin = Vector2.zero;
            pauseTitle.rectTransform.offsetMax = Vector2.zero;
            pauseTitle.text = "已暂停";

            var pauseSub = CreateText("Sub", pauseCard, font, 28, FontStyle.Normal, UITheme.TextSoft);
            pauseSub.alignment = TextAnchor.MiddleCenter;
            pauseSub.rectTransform.anchorMin = new Vector2(0f, 0.42f);
            pauseSub.rectTransform.anchorMax = new Vector2(1f, 0.54f);
            pauseSub.rectTransform.offsetMin = Vector2.zero;
            pauseSub.rectTransform.offsetMax = Vector2.zero;
            pauseSub.text = "深呼吸，然后继续";

            // 继续按钮
            var resumeBtn = CreatePrimaryButton("▶ 继续", pauseCard, font, UITheme.Confirm, UITheme.Text);
            resumeBtn.name = "ResumeBtn";
            var resumeRect = (RectTransform)resumeBtn.transform;
            resumeRect.anchorMin = new Vector2(0.10f, 0.18f);
            resumeRect.anchorMax = new Vector2(0.90f, 0.36f);
            resumeRect.offsetMin = Vector2.zero;
            resumeRect.offsetMax = Vector2.zero;
            var resumeLayout = resumeBtn.GetComponent<LayoutElement>();
            if (resumeLayout != null) resumeLayout.preferredHeight = 90f;
            var resumeLabel = resumeBtn.GetComponentInChildren<Text>();
            if (resumeLabel != null) resumeLabel.fontSize = 38;

            // 回家按钮
            var homeBtn = CreatePrimaryButton("🏠 回家", pauseCard, font, new Color32(158, 158, 158, 255), Color.white);
            homeBtn.name = "HomeBtn";
            var homeRect = (RectTransform)homeBtn.transform;
            homeRect.anchorMin = new Vector2(0.10f, 0.04f);
            homeRect.anchorMax = new Vector2(0.90f, 0.16f);
            homeRect.offsetMin = Vector2.zero;
            homeRect.offsetMax = Vector2.zero;
            var homeLayout = homeBtn.GetComponent<LayoutElement>();
            if (homeLayout != null) homeLayout.preferredHeight = 70f;
            var homeLabel = homeBtn.GetComponentInChildren<Text>();
            if (homeLabel != null) homeLabel.fontSize = 32;

            // ===== Opening Overlay (粉色渐变开屏 - 放在最后保证层级最高) =====
            var opening = CreateUiObject("Opening", contentObj);
            openingOverlay = opening.gameObject;
            Stretch(opening);
            // 粉色背景渐变
            var openBg = opening.gameObject.AddComponent<Image>();
            openBg.color = new Color32(255, 200, 200, 255);
            var openBgLight = CreateUiObject("BgLight", opening);
            Stretch(openBgLight);
            var openBgLightImg = openBgLight.gameObject.AddComponent<Image>();
            openBgLightImg.color = new Color32(255, 235, 220, 255);
            openBgLight.transform.SetAsFirstSibling();

            var openBtn = opening.gameObject.AddComponent<Button>();
            openBtn.onClick.AddListener(OnOpeningTap);

            var openCard = CreateUiObject("Card", opening);
            openCard.anchorMin = new Vector2(0.08f, 0.05f);
            openCard.anchorMax = new Vector2(0.92f, 0.95f);
            openCard.offsetMin = Vector2.zero;
            openCard.offsetMax = Vector2.zero;
            var openCardImg = openCard.gameObject.AddComponent<Image>();
            openCardImg.color = new Color(1f, 1f, 1f, 0.01f); // 几乎透明但可点击
            RuntimeArt.ApplyRounded(openCardImg);
            var openCardBtn = openCard.gameObject.AddComponent<Button>();
            openCardBtn.onClick.AddListener(OnOpeningTap);

            var openEmoji = CreateText("Emoji", openCard, font, 140, FontStyle.Normal, new Color32(255, 158, 92, 255));
            openEmoji.alignment = TextAnchor.MiddleCenter;
            openEmoji.rectTransform.anchorMin = new Vector2(0.30f, 0.62f);
            openEmoji.rectTransform.anchorMax = new Vector2(0.70f, 0.92f);
            openEmoji.rectTransform.offsetMin = Vector2.zero;
            openEmoji.rectTransform.offsetMax = Vector2.zero;
            openEmoji.text = "☀️";

            openingText = CreateText("Text", openCard, font, 50, FontStyle.Bold, new Color32(74, 58, 46, 255));
            openingText.alignment = TextAnchor.MiddleCenter;
            openingText.rectTransform.anchorMin = new Vector2(0.04f, 0.42f);
            openingText.rectTransform.anchorMax = new Vector2(0.96f, 0.62f);
            openingText.rectTransform.offsetMin = Vector2.zero;
            openingText.rectTransform.offsetMax = Vector2.zero;
            openingText.text = "美好的一天开启了";

            openingSubtitle = CreateText("Subtitle", openCard, font, 28, FontStyle.Normal, new Color32(107, 78, 65, 255));
            openingSubtitle.alignment = TextAnchor.MiddleCenter;
            openingSubtitle.rectTransform.anchorMin = new Vector2(0.04f, 0.32f);
            openingSubtitle.rectTransform.anchorMax = new Vector2(0.96f, 0.42f);
            openingSubtitle.rectTransform.offsetMin = Vector2.zero;
            openingSubtitle.rectTransform.offsetMax = Vector2.zero;
            openingSubtitle.text = "你将以「显眼包」的身份...";

            // 开启今天按钮
            var openStartBtn = CreatePrimaryButton("🌞 开启今天", openCard, font, new Color32(255, 138, 128, 255), new Color32(255, 255, 255, 255));
            openStartBtn.name = "StartBtn";
            var startRect = (RectTransform)openStartBtn.transform;
            startRect.anchorMin = new Vector2(0.15f, 0.10f);
            startRect.anchorMax = new Vector2(0.85f, 0.25f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;
            openStartBtn.onClick.RemoveAllListeners();
            openStartBtn.onClick.AddListener(OnOpeningTap);
            var startLabel = openStartBtn.GetComponentInChildren<Text>();
            if (startLabel != null) startLabel.fontSize = 36;

            // Initial state
            openingOverlay.SetActive(true);
            eventModal.SetActive(false);
            resultModal.SetActive(false);
            blackoutOverlay.SetActive(false);
        }

        private Text CreateStatChipV2(Transform parent, Font font, string name, string iconName, string emoji)
        {
            var item = CreateUiObject(name, parent);

            // 圆角白色卡
            var img = item.gameObject.AddComponent<Image>();
            img.color = new Color32(255, 255, 255, 255);
            RuntimeArt.ApplyRounded(img);
            var shadow = item.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            shadow.effectDistance = new Vector2(0f, -2f);

            // 图标
            var iconGo = CreateUiObject("Icon", item);
            iconGo.anchorMin = new Vector2(0.05f, 0.50f);
            iconGo.anchorMax = new Vector2(0.30f, 0.92f);
            var iconImg = iconGo.gameObject.AddComponent<Image>();
            var iconSprite = RuntimeArt.LoadSprite($"UI/icon/{iconName}");
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
            }
            else
            {
                iconImg.color = new Color32(200, 200, 200, 100);
            }

            // 名称
            var label = CreateText("Label", item, font, 22, FontStyle.Normal, new Color32(155, 126, 110, 255));
            label.alignment = TextAnchor.UpperLeft;
            label.rectTransform.anchorMin = new Vector2(0.32f, 0.48f);
            label.rectTransform.anchorMax = new Vector2(0.98f, 0.95f);
            label.text = name;

            // 数值
            var value = CreateText("Value", item, font, 36, FontStyle.Bold, new Color32(74, 58, 46, 255));
            value.alignment = TextAnchor.MiddleLeft;
            value.rectTransform.anchorMin = new Vector2(0.32f, 0.05f);
            value.rectTransform.anchorMax = new Vector2(0.98f, 0.52f);
            value.text = "0";
            return value;
        }

        #endregion

        private Color GetTimeColor(string time)
        {
            int hour = int.Parse(time.Split(':')[0]);
            if (hour >= 6 && hour < 12) return new Color32(107, 157, 247, 255);
            if (hour >= 12 && hour < 18) return new Color32(255, 158, 92, 255);
            return new Color32(155, 126, 110, 255);
        }
    }
}