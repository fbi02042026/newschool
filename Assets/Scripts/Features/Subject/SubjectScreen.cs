using System.Collections.Generic;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GaokaoSimulator.Features.Subject
{
    public class SubjectScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button expertButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text selectionText;

        [SerializeField] private Button physicsButton;
        [SerializeField] private Button historyButton;

        [SerializeField] private Button politicsButton;
        [SerializeField] private Button geographyButton;
        [SerializeField] private Button chemistryButton;
        [SerializeField] private Button biologyButton;

        private FirstSubject selectedFirst = FirstSubject.None;
        private readonly List<SecondSubject> selectedSeconds = new List<SecondSubject>();
        private RectTransform expertPopupRoot;
        private Text expertQuestionText;
        private Text expertProgressText;
        private Text expertResultText;
        private Button expertOptionAButton;
        private Button expertOptionBButton;
        private Button expertOptionCButton;
        private Button expertApplyButton;
        private Button expertResetButton;
        private int expertQuestionIndex;
        private int expertPhysicsScore;
        private int expertHistoryScore;
        private int expertPoliticsScore;
        private int expertGeographyScore;
        private int expertChemistryScore;
        private int expertBiologyScore;
        private FirstSubject expertRecommendedFirst = FirstSubject.None;
        private SecondSubject expertRecommendedSecondA = SecondSubject.None;
        private SecondSubject expertRecommendedSecondB = SecondSubject.None;

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            ScreenFlowHint.Ensure(transform.Find("Panel") ?? transform, ScreenFlowHint.GetNextLabel(ScreenType.Subject));
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            var state = GameState.Instance;
            if (state != null)
            {
                state.HasSaveData = true;
                if (state.CurrentProgress < GameProgress.Subject)
                {
                    state.CurrentProgress = GameProgress.Subject;
                }

                selectedFirst = state.FirstSubject;
                selectedSeconds.Clear();
                if (state.SecondSubjects != null)
                {
                    for (int i = 0; i < state.SecondSubjects.Count; i++)
                    {
                        var s = state.SecondSubjects[i];
                        if (s != SecondSubject.None && !selectedSeconds.Contains(s))
                        {
                            selectedSeconds.Add(s);
                        }
                    }
                }
            }

            GuideService.EnsureHelpButton(transform.Find("Panel/Header") ?? transform, "BtnHelp", () => GuideService.Open(ScreenType.Subject, transform));
            GuideService.TryShowOnce(ScreenType.Subject, transform);
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "选科";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "先选 1 门首选，再从 4 门里选 2 门";
            }

            if (hintText != null)
            {
                hintText.text = BuildHintText();
            }

            UpdateCardState(physicsButton, selectedFirst == FirstSubject.Physics);
            UpdateCardState(historyButton, selectedFirst == FirstSubject.History);

            UpdateCardState(politicsButton, selectedSeconds.Contains(SecondSubject.Politics));
            UpdateCardState(geographyButton, selectedSeconds.Contains(SecondSubject.Geography));
            UpdateCardState(chemistryButton, selectedSeconds.Contains(SecondSubject.Chemistry));
            UpdateCardState(biologyButton, selectedSeconds.Contains(SecondSubject.Biology));

            UpdateSelectionText();
            UpdateConfirmState();
        }

        private string BuildHintText()
        {
            return "首选科目决定大方向，再从 4 门里选 2 门补充能力。\n拿不准就点右上角专家，我会先问你几个问题，再推荐更适合的组合。";
        }

        private void BindEvents()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => GoBack());
            }

            if (physicsButton != null)
            {
                physicsButton.onClick.RemoveAllListeners();
                physicsButton.onClick.AddListener(() => SelectFirst(FirstSubject.Physics));
            }

            if (historyButton != null)
            {
                historyButton.onClick.RemoveAllListeners();
                historyButton.onClick.AddListener(() => SelectFirst(FirstSubject.History));
            }

            if (politicsButton != null)
            {
                politicsButton.onClick.RemoveAllListeners();
                politicsButton.onClick.AddListener(() => ToggleSecond(SecondSubject.Politics));
            }

            if (geographyButton != null)
            {
                geographyButton.onClick.RemoveAllListeners();
                geographyButton.onClick.AddListener(() => ToggleSecond(SecondSubject.Geography));
            }

            if (chemistryButton != null)
            {
                chemistryButton.onClick.RemoveAllListeners();
                chemistryButton.onClick.AddListener(() => ToggleSecond(SecondSubject.Chemistry));
            }

            if (biologyButton != null)
            {
                biologyButton.onClick.RemoveAllListeners();
                biologyButton.onClick.AddListener(() => ToggleSecond(SecondSubject.Biology));
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(ConfirmSubjects);
            }

            if (expertButton != null)
            {
                expertButton.onClick.RemoveAllListeners();
                expertButton.onClick.AddListener(OpenExpertPopup);
            }
        }

        private void OpenExpertPopup()
        {
            if (expertPopupRoot == null)
            {
                expertPopupRoot = BuildExpertPopup();
            }

            if (expertPopupRoot != null)
            {
                ResetExpertQuestionnaire();
                expertPopupRoot.gameObject.SetActive(true);
            }
        }

        private RectTransform BuildExpertPopup()
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

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var overlay = CreateUiObject("ExpertPopup", parent);
            Stretch(overlay);
            var overlayImage = overlay.gameObject.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.35f);

            var card = CreateUiObject("Card", overlay);
            card.anchorMin = new Vector2(0.08f, 0.18f);
            card.anchorMax = new Vector2(0.92f, 0.82f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = Color.white;
            RuntimeArt.ApplyRounded(cardImage);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            shadow.effectDistance = new Vector2(0f, -16f);

            var title = CreateText("Title", card, font, 58, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.UpperCenter;
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.86f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.text = "如果不知道选什么";

            expertProgressText = CreateText("Progress", card, font, 28, FontStyle.Bold, UITheme.TextLight);
            expertProgressText.alignment = TextAnchor.MiddleCenter;
            expertProgressText.rectTransform.anchorMin = new Vector2(0.08f, 0.76f);
            expertProgressText.rectTransform.anchorMax = new Vector2(0.92f, 0.84f);
            expertProgressText.rectTransform.offsetMin = Vector2.zero;
            expertProgressText.rectTransform.offsetMax = Vector2.zero;

            expertQuestionText = CreateText("Question", card, font, 42, FontStyle.Bold, UITheme.Text);
            expertQuestionText.alignment = TextAnchor.UpperLeft;
            expertQuestionText.rectTransform.anchorMin = new Vector2(0.08f, 0.58f);
            expertQuestionText.rectTransform.anchorMax = new Vector2(0.92f, 0.74f);
            expertQuestionText.rectTransform.offsetMin = Vector2.zero;
            expertQuestionText.rectTransform.offsetMax = Vector2.zero;

            expertOptionAButton = CreateExpertOptionButton(card, font);
            var optionARect = (RectTransform)expertOptionAButton.transform;
            optionARect.anchorMin = new Vector2(0.08f, 0.40f);
            optionARect.anchorMax = new Vector2(0.92f, 0.54f);
            optionARect.offsetMin = Vector2.zero;
            optionARect.offsetMax = Vector2.zero;

            expertOptionBButton = CreateExpertOptionButton(card, font);
            var optionBRect = (RectTransform)expertOptionBButton.transform;
            optionBRect.anchorMin = new Vector2(0.08f, 0.24f);
            optionBRect.anchorMax = new Vector2(0.92f, 0.38f);
            optionBRect.offsetMin = Vector2.zero;
            optionBRect.offsetMax = Vector2.zero;

            expertOptionCButton = CreateExpertOptionButton(card, font);
            var optionCRect = (RectTransform)expertOptionCButton.transform;
            optionCRect.anchorMin = new Vector2(0.08f, 0.08f);
            optionCRect.anchorMax = new Vector2(0.92f, 0.22f);
            optionCRect.offsetMin = Vector2.zero;
            optionCRect.offsetMax = Vector2.zero;

            expertOptionAButton.onClick.AddListener(() => OnExpertOptionSelected(0));
            expertOptionBButton.onClick.AddListener(() => OnExpertOptionSelected(1));
            expertOptionCButton.onClick.AddListener(() => OnExpertOptionSelected(2));

            expertResultText = CreateText("Result", card, font, 34, FontStyle.Normal, UITheme.TextSoft);
            expertResultText.alignment = TextAnchor.UpperLeft;
            expertResultText.rectTransform.anchorMin = new Vector2(0.08f, 0.34f);
            expertResultText.rectTransform.anchorMax = new Vector2(0.92f, 0.74f);
            expertResultText.rectTransform.offsetMin = Vector2.zero;
            expertResultText.rectTransform.offsetMax = Vector2.zero;
            expertResultText.resizeTextForBestFit = true;
            expertResultText.resizeTextMinSize = 24;
            expertResultText.resizeTextMaxSize = Mathf.RoundToInt(34 * UiTextScale);
            expertResultText.gameObject.SetActive(false);

            expertApplyButton = CreatePrimaryButton("采用这套推荐", card, font, UITheme.Confirm, Color.white);
            expertApplyButton.gameObject.AddComponent<UiPressScale>();
            var applyRect = (RectTransform)expertApplyButton.transform;
            applyRect.anchorMin = new Vector2(0.08f, 0.08f);
            applyRect.anchorMax = new Vector2(0.92f, 0.20f);
            applyRect.offsetMin = Vector2.zero;
            applyRect.offsetMax = Vector2.zero;
            expertApplyButton.onClick.AddListener(ApplyExpertRecommendation);
            expertApplyButton.gameObject.SetActive(false);

            expertResetButton = CreatePrimaryButton("重新回答 / 先自己选", card, font, UITheme.Gold, UITheme.Text);
            expertResetButton.gameObject.AddComponent<UiPressScale>();
            var resetRect = (RectTransform)expertResetButton.transform;
            resetRect.anchorMin = new Vector2(0.08f, 0.20f);
            resetRect.anchorMax = new Vector2(0.92f, 0.32f);
            resetRect.offsetMin = Vector2.zero;
            resetRect.offsetMax = Vector2.zero;
            expertResetButton.onClick.AddListener(() => ResetExpertQuestionnaire());
            expertResetButton.gameObject.SetActive(false);

            var closeButton = CreateSmallButton("关闭", card, font, UITheme.CardPeach, UITheme.Text);
            closeButton.gameObject.AddComponent<UiPressScale>();
            var closeRect = (RectTransform)closeButton.transform;
            closeRect.anchorMin = new Vector2(0.68f, 0.86f);
            closeRect.anchorMax = new Vector2(0.90f, 0.96f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            closeButton.onClick.AddListener(() => overlay.gameObject.SetActive(false));

            overlay.gameObject.SetActive(false);
            return overlay;
        }

        private Button CreateExpertOptionButton(Transform parent, Font font)
        {
            var button = CreatePrimaryButton("选项", parent, font, UITheme.Gold, UITheme.Text);
            button.gameObject.AddComponent<UiPressScale>();
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.alignment = TextAnchor.MiddleLeft;
                text.fontSize = Mathf.RoundToInt(34 * UiTextScale);
            }

            return button;
        }

        private void ResetExpertQuestionnaire()
        {
            expertQuestionIndex = 0;
            expertPhysicsScore = 0;
            expertHistoryScore = 0;
            expertPoliticsScore = 0;
            expertGeographyScore = 0;
            expertChemistryScore = 0;
            expertBiologyScore = 0;
            expertRecommendedFirst = FirstSubject.None;
            expertRecommendedSecondA = SecondSubject.None;
            expertRecommendedSecondB = SecondSubject.None;
            UpdateExpertQuestionUi();
        }

        private void OnExpertOptionSelected(int optionIndex)
        {
            ApplyExpertAnswerScore(expertQuestionIndex, optionIndex);
            expertQuestionIndex++;

            if (expertQuestionIndex >= 3)
            {
                BuildExpertRecommendation();
                return;
            }

            UpdateExpertQuestionUi();
        }

        private void ApplyExpertAnswerScore(int questionIndex, int optionIndex)
        {
            switch (questionIndex)
            {
                case 0:
                    if (optionIndex == 0)
                    {
                        expertPhysicsScore += 2;
                        expertChemistryScore += 1;
                    }
                    else if (optionIndex == 1)
                    {
                        expertHistoryScore += 2;
                        expertPoliticsScore += 1;
                    }
                    else
                    {
                        expertGeographyScore += 2;
                        expertBiologyScore += 1;
                    }
                    return;
                case 1:
                    if (optionIndex == 0)
                    {
                        expertPhysicsScore += 2;
                        expertChemistryScore += 1;
                        expertGeographyScore += 1;
                    }
                    else if (optionIndex == 1)
                    {
                        expertHistoryScore += 2;
                        expertPoliticsScore += 2;
                    }
                    else
                    {
                        expertBiologyScore += 2;
                        expertChemistryScore += 1;
                        expertPhysicsScore += 1;
                    }
                    return;
                default:
                    if (optionIndex == 0)
                    {
                        expertPhysicsScore += 1;
                        expertChemistryScore += 1;
                    }
                    else if (optionIndex == 1)
                    {
                        expertHistoryScore += 1;
                        expertPoliticsScore += 1;
                    }
                    else
                    {
                        expertGeographyScore += 2;
                        expertBiologyScore += 1;
                        expertPoliticsScore += 1;
                    }
                    return;
            }
        }

        private void UpdateExpertQuestionUi()
        {
            if (expertQuestionText == null || expertProgressText == null || expertOptionAButton == null || expertOptionBButton == null || expertOptionCButton == null)
            {
                return;
            }

            if (expertResultText != null) expertResultText.gameObject.SetActive(false);
            if (expertApplyButton != null) expertApplyButton.gameObject.SetActive(false);
            if (expertResetButton != null) expertResetButton.gameObject.SetActive(false);

            expertQuestionText.gameObject.SetActive(true);
            expertOptionAButton.gameObject.SetActive(true);
            expertOptionBButton.gameObject.SetActive(true);
            expertOptionCButton.gameObject.SetActive(true);

            expertProgressText.text = $"问题 {expertQuestionIndex + 1} / 3";

            switch (expertQuestionIndex)
            {
                case 0:
                    expertQuestionText.text = "1. 你平时更容易进入状态的是哪种学习方式？";
                    SetExpertButtonLabel(expertOptionAButton, "做题、推理、实验更有感觉");
                    SetExpertButtonLabel(expertOptionBButton, "阅读、表达、讨论更有感觉");
                    SetExpertButtonLabel(expertOptionCButton, "联系生活、看现实案例更有感觉");
                    return;
                case 1:
                    expertQuestionText.text = "2. 你现在更想靠近哪类未来方向？";
                    SetExpertButtonLabel(expertOptionAButton, "工程、计算机、理工类");
                    SetExpertButtonLabel(expertOptionBButton, "法学、管理、传播、考公");
                    SetExpertButtonLabel(expertOptionCButton, "医学、生命、健康、自然方向");
                    return;
                default:
                    expertQuestionText.text = "3. 你更适合哪种节奏？";
                    SetExpertButtonLabel(expertOptionAButton, "公式清晰、题感强，越刷越顺");
                    SetExpertButtonLabel(expertOptionBButton, "理解记忆、分析表达更稳定");
                    SetExpertButtonLabel(expertOptionCButton, "观察现实、联系生活会更好学");
                    return;
            }
        }

        private void BuildExpertRecommendation()
        {
            expertRecommendedFirst = expertPhysicsScore >= expertHistoryScore ? FirstSubject.Physics : FirstSubject.History;

            var secondScores = new List<KeyValuePair<SecondSubject, int>>
            {
                new KeyValuePair<SecondSubject, int>(SecondSubject.Politics, expertPoliticsScore),
                new KeyValuePair<SecondSubject, int>(SecondSubject.Geography, expertGeographyScore),
                new KeyValuePair<SecondSubject, int>(SecondSubject.Chemistry, expertChemistryScore),
                new KeyValuePair<SecondSubject, int>(SecondSubject.Biology, expertBiologyScore)
            };

            secondScores.Sort((a, b) => b.Value.CompareTo(a.Value));
            expertRecommendedSecondA = secondScores[0].Key;
            expertRecommendedSecondB = secondScores[1].Key;

            expertProgressText.text = "推荐已生成";
            expertQuestionText.gameObject.SetActive(false);

            if (expertResultText != null)
            {
                expertResultText.gameObject.SetActive(true);
                expertResultText.text =
                    $"{GetFirstLabel(expertRecommendedFirst)} + {GetSecondLabel(expertRecommendedSecondA)} + {GetSecondLabel(expertRecommendedSecondB)}\n\n" +
                    GetExpertReasonText(expertRecommendedFirst, expertRecommendedSecondA, expertRecommendedSecondB) +
                    "\n\n未来方向：\n" +
                    GetExpertFutureText(expertRecommendedFirst, expertRecommendedSecondA, expertRecommendedSecondB);
            }

            expertOptionAButton.gameObject.SetActive(false);
            expertOptionBButton.gameObject.SetActive(false);
            expertOptionCButton.gameObject.SetActive(false);

            if (expertApplyButton != null) expertApplyButton.gameObject.SetActive(true);
            if (expertResetButton != null) expertResetButton.gameObject.SetActive(true);
        }

        private string GetExpertReasonText(FirstSubject first, SecondSubject a, SecondSubject b)
        {
            if (first == FirstSubject.Physics)
            {
                return $"这套更偏理工路线，适合想保留工程/计算机/理科空间的人。\n再选 {GetSecondLabel(a)}、{GetSecondLabel(b)} 能把相关方向再补强。";
            }

            return $"这套更偏人文与管理路线，适合表达、分析、阅读型同学。\n再选 {GetSecondLabel(a)}、{GetSecondLabel(b)} 会让方向更稳。";
        }

        private string GetExpertFutureText(FirstSubject first, SecondSubject a, SecondSubject b)
        {
            var lines = new List<string>();
            if (first == FirstSubject.Physics)
            {
                lines.Add("主线：工程 / 计算机 / 理科");
            }
            else
            {
                lines.Add("主线：法学 / 文史 / 管理 / 教育");
            }

            AppendSecondFuture(lines, a);
            AppendSecondFuture(lines, b);

            return string.Join("\n", lines.ToArray());
        }

        private void AppendSecondFuture(List<string> lines, SecondSubject subject)
        {
            switch (subject)
            {
                case SecondSubject.Chemistry:
                    lines.Add("化学：材料 / 化工 / 医药实验相关");
                    return;
                case SecondSubject.Biology:
                    lines.Add("生物：生命科学 / 医学健康相关");
                    return;
                case SecondSubject.Geography:
                    lines.Add("地理：环境 / 城市规划 / 地理信息相关");
                    return;
                case SecondSubject.Politics:
                    lines.Add("政治：考公 / 公管 / 社科相关");
                    return;
                default:
                    return;
            }
        }

        private void ApplyExpertRecommendation()
        {
            if (expertRecommendedFirst == FirstSubject.None || expertRecommendedSecondA == SecondSubject.None || expertRecommendedSecondB == SecondSubject.None)
            {
                return;
            }

            ApplyRecommendation(expertRecommendedFirst, expertRecommendedSecondA, expertRecommendedSecondB);

            if (expertPopupRoot != null)
            {
                expertPopupRoot.gameObject.SetActive(false);
            }
        }

        private static void SetExpertButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        private void ApplyRecommendation(FirstSubject first, SecondSubject a, SecondSubject b)
        {
            selectedFirst = first;
            selectedSeconds.Clear();
            selectedSeconds.Add(a);
            selectedSeconds.Add(b);
            ShowToast($"已为你勾选：{GetFirstLabel(first)} + {GetSecondLabel(a)} + {GetSecondLabel(b)}");
            Refresh();
        }

        private void SelectFirst(FirstSubject subject)
        {
            selectedFirst = subject;
            Refresh();
        }

        private void ToggleSecond(SecondSubject subject)
        {
            if (subject == SecondSubject.None)
            {
                return;
            }

            if (selectedFirst == FirstSubject.None)
            {
                ShowToast("先选首选科目（物理/历史）");
                return;
            }

            if (selectedSeconds.Contains(subject))
            {
                selectedSeconds.Remove(subject);
                Refresh();
                return;
            }

            if (selectedSeconds.Count >= 2)
            {
                ShowToast("再选科只能选 2 门");
                return;
            }

            selectedSeconds.Add(subject);
            Refresh();
        }

        private void ConfirmSubjects()
        {
            if (selectedFirst == FirstSubject.None || selectedSeconds.Count != 2)
            {
                ShowToast("先选 1 门首选 + 2 门再选科");
                return;
            }

            var state = GameState.Instance;
            if (state != null)
            {
                state.FirstSubject = selectedFirst;
                state.SecondSubjects.Clear();
                state.SecondSubjects.Add(selectedSeconds[0]);
                state.SecondSubjects.Add(selectedSeconds[1]);
                state.CurrentProgress = GameProgress.Home;
                state.HasSaveData = true;
            }

            NavigateTo(ScreenType.Home, true);
        }

        private void UpdateSelectionText()
        {
            if (selectionText == null)
            {
                return;
            }

            var first = selectedFirst == FirstSubject.None ? "未选择" : GetFirstLabel(selectedFirst);
            var second = selectedSeconds.Count == 0
                ? "未选择"
                : selectedSeconds.Count == 1
                    ? $"{GetSecondLabel(selectedSeconds[0])}（还差 1 门）"
                    : $"{GetSecondLabel(selectedSeconds[0])} + {GetSecondLabel(selectedSeconds[1])}";

            selectionText.text = $"当前选择：{first} + {second}";
        }

        private void UpdateConfirmState()
        {
            if (confirmButton == null)
            {
                return;
            }

            var ok = selectedFirst != FirstSubject.None && selectedSeconds.Count == 2;
            confirmButton.interactable = ok;

            var label = confirmButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = ok ? "确认选科 → 进入主界面" : "先完成选科";
            }
        }

        private static void UpdateCardState(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = selected ? Color.white : UITheme.BgCard;
            }

            var outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = selected
                ? new Color32(UITheme.Confirm.r, UITheme.Confirm.g, UITheme.Confirm.b, 255)
                : new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            outline.effectDistance = selected ? new Vector2(5f, -5f) : new Vector2(4f, -4f);

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = selected ? UITheme.ConfirmHover : UITheme.Text;
            }

            var descText = button.transform.Find("DescText")?.GetComponent<Text>();
            if (descText != null)
            {
                descText.color = selected ? UITheme.ConfirmHover : UITheme.TextSoft;
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null && confirmButton != null && expertButton != null && titleText != null && subtitleText != null && hintText != null && selectionText != null
                && physicsButton != null && historyButton != null
                && politicsButton != null && geographyButton != null && chemistryButton != null && biologyButton != null)
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

            var background = CreateUiObject("Background", root);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgGradient = background.gameObject.AddComponent<UiCornerGradient>();
            bgGradient.SetColors(UITheme.CardPeach, UITheme.Bg, UITheme.CardSky, UITheme.CardLavender);

            var panel = CreateUiObject("Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.76f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            backButton = CreateSmallButton("← 返回", header, font, UITheme.CardPeach, UITheme.Text);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.70f);
            backRect.anchorMax = new Vector2(0.24f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backButton.gameObject.AddComponent<UiPressScale>();

            titleText = CreateText("Title", header, font, 84, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.22f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.70f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            subtitleText = CreateText("Subtitle", header, font, 42, FontStyle.Normal, UITheme.TextLight);
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.06f, 0.00f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.94f, 0.28f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;

            expertButton = CreateExpertButton(header);
            var expertRect = (RectTransform)expertButton.transform;
            expertRect.anchorMin = new Vector2(0.68f, 0.72f);
            expertRect.anchorMax = new Vector2(0.78f, 0.98f);
            expertRect.offsetMin = Vector2.zero;
            expertRect.offsetMax = Vector2.zero;
            expertButton.gameObject.AddComponent<UiPressScale>();

            var body = CreateUiObject("Body", panel);
            body.anchorMin = new Vector2(0f, 0.16f);
            body.anchorMax = new Vector2(1f, 0.76f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var tip = CreateUiObject("Tip", body);
            tip.anchorMin = new Vector2(0.06f, 0.84f);
            tip.anchorMax = new Vector2(0.94f, 0.98f);
            tip.offsetMin = Vector2.zero;
            tip.offsetMax = Vector2.zero;
            var tipBg = tip.gameObject.AddComponent<Image>();
            tipBg.color = Color.white;
            RuntimeArt.ApplyRounded(tipBg);
            var tipShadow = tip.gameObject.AddComponent<Shadow>();
            tipShadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            tipShadow.effectDistance = new Vector2(0f, -10f);
            hintText = CreateText("HintText", tip, font, 34, FontStyle.Normal, UITheme.TextSoft);
            hintText.alignment = TextAnchor.UpperLeft;
            hintText.rectTransform.anchorMin = new Vector2(0.05f, 0.10f);
            hintText.rectTransform.anchorMax = new Vector2(0.95f, 0.90f);
            hintText.rectTransform.offsetMin = Vector2.zero;
            hintText.rectTransform.offsetMax = Vector2.zero;

            var firstTitle = CreateText("FirstTitle", body, font, 40, FontStyle.Bold, UITheme.Text);
            firstTitle.alignment = TextAnchor.MiddleLeft;
            firstTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.72f);
            firstTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.82f);
            firstTitle.rectTransform.offsetMin = Vector2.zero;
            firstTitle.rectTransform.offsetMax = Vector2.zero;
            firstTitle.text = "首选科目（选 1）";

            var firstRow = CreateUiObject("FirstRow", body);
            firstRow.anchorMin = new Vector2(0.06f, 0.50f);
            firstRow.anchorMax = new Vector2(0.94f, 0.72f);
            firstRow.offsetMin = Vector2.zero;
            firstRow.offsetMax = Vector2.zero;

            physicsButton = CreateSubjectCard("物理", "工科 / 理科 / 计算机 / 部分医学", firstRow, font, UITheme.FromHex("4F7BD9"));
            var phyRect = (RectTransform)physicsButton.transform;
            phyRect.anchorMin = new Vector2(0f, 0f);
            phyRect.anchorMax = new Vector2(0.49f, 1f);
            phyRect.offsetMin = Vector2.zero;
            phyRect.offsetMax = Vector2.zero;
            physicsButton.gameObject.AddComponent<UiPressScale>();

            historyButton = CreateSubjectCard("历史", "法学 / 文史 / 新闻 / 教育 / 管理", firstRow, font, UITheme.FromHex("FFB74D"));
            var hisRect = (RectTransform)historyButton.transform;
            hisRect.anchorMin = new Vector2(0.51f, 0f);
            hisRect.anchorMax = new Vector2(1f, 1f);
            hisRect.offsetMin = Vector2.zero;
            hisRect.offsetMax = Vector2.zero;
            historyButton.gameObject.AddComponent<UiPressScale>();

            var secondTitle = CreateText("SecondTitle", body, font, 40, FontStyle.Bold, UITheme.Text);
            secondTitle.alignment = TextAnchor.MiddleLeft;
            secondTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.40f);
            secondTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.50f);
            secondTitle.rectTransform.offsetMin = Vector2.zero;
            secondTitle.rectTransform.offsetMax = Vector2.zero;
            secondTitle.text = "再选科目（选 2）";

            var secondGrid = CreateUiObject("SecondGrid", body);
            secondGrid.anchorMin = new Vector2(0.06f, 0.10f);
            secondGrid.anchorMax = new Vector2(0.94f, 0.40f);
            secondGrid.offsetMin = Vector2.zero;
            secondGrid.offsetMax = Vector2.zero;
            var grid = secondGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(520f, 170f);
            grid.spacing = new Vector2(18f, 18f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.childAlignment = TextAnchor.UpperCenter;

            politicsButton = CreateSubjectCard("政治", "考公 / 法学 / 公管 / 社科", secondGrid, font, UITheme.FromHex("E57373"));
            geographyButton = CreateSubjectCard("地理", "地理 / 环境 / 城规 / 旅游", secondGrid, font, UITheme.FromHex("81C784"));
            chemistryButton = CreateSubjectCard("化学", "化工 / 材料 / 医药 / 实验", secondGrid, font, UITheme.FromHex("64B5F6"));
            biologyButton = CreateSubjectCard("生物", "生命科学 / 医学 / 健康", secondGrid, font, UITheme.FromHex("BA68C8"));

            selectionText = CreateText("SelectionText", body, font, 36, FontStyle.Bold, UITheme.TextSoft);
            selectionText.alignment = TextAnchor.MiddleCenter;
            selectionText.rectTransform.anchorMin = new Vector2(0.06f, 0.00f);
            selectionText.rectTransform.anchorMax = new Vector2(0.94f, 0.08f);
            selectionText.rectTransform.offsetMin = Vector2.zero;
            selectionText.rectTransform.offsetMax = Vector2.zero;

            var footer = CreateUiObject("Footer", panel);
            footer.anchorMin = new Vector2(0f, 0.02f);
            footer.anchorMax = new Vector2(1f, 0.14f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            confirmButton = CreatePrimaryButton("确认选科 → 进入主界面", footer, font, UITheme.Confirm, Color.white);
            var confirmRect = (RectTransform)confirmButton.transform;
            confirmRect.anchorMin = new Vector2(0.12f, 0.10f);
            confirmRect.anchorMax = new Vector2(0.88f, 0.90f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;
            confirmButton.gameObject.AddComponent<UiPressScale>();
        }

        private static Button CreateSubjectCard(string title, string desc, Transform parent, Font font, Color accent)
        {
            var button = CreateButton(title, parent, font, Color.white, UITheme.Text);
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
            }

            var titleText = button.GetComponentInChildren<Text>();
            if (titleText != null)
            {
                titleText.name = "TitleText";
                titleText.alignment = TextAnchor.UpperCenter;
                titleText.rectTransform.anchorMin = new Vector2(0.08f, 0.44f);
                titleText.rectTransform.anchorMax = new Vector2(0.92f, 0.78f);
                titleText.rectTransform.offsetMin = Vector2.zero;
                titleText.rectTransform.offsetMax = Vector2.zero;
                titleText.color = UITheme.Text;
            }

            var accentBar = CreateUiObject("AccentBar", button.transform);
            accentBar.anchorMin = new Vector2(0.06f, 0.80f);
            accentBar.anchorMax = new Vector2(0.94f, 0.88f);
            accentBar.offsetMin = Vector2.zero;
            accentBar.offsetMax = Vector2.zero;
            var accentImage = accentBar.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(accentImage);
            accentImage.color = accent;

            var descText = CreateText("DescText", button.transform, font, 24, FontStyle.Normal, UITheme.TextSoft);
            descText.alignment = TextAnchor.MiddleCenter;
            descText.rectTransform.anchorMin = new Vector2(0.08f, 0.10f);
            descText.rectTransform.anchorMax = new Vector2(0.92f, 0.42f);
            descText.rectTransform.offsetMin = Vector2.zero;
            descText.rectTransform.offsetMax = Vector2.zero;
            descText.text = desc;
            descText.resizeTextForBestFit = true;
            descText.resizeTextMinSize = 20;
            descText.resizeTextMaxSize = 28;

            var shadow = button.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            shadow.effectDistance = new Vector2(0f, -8f);

            return button;
        }

        private static Button CreateExpertButton(Transform parent)
        {
            var go = new GameObject("BtnExpert", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            RuntimeArt.ApplyRounded(image);
            image.sprite = NpcPortraits.Load(NpcPortraitId.Expert);
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            outline.effectDistance = new Vector2(3f, -3f);
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            shadow.effectDistance = new Vector2(0f, -8f);
            return go.GetComponent<Button>();
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
                default: return "未选择";
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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
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
            return button;
        }

        private static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var btn = CreateButton(label, parent, font, bgColor, textColor);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = Mathf.RoundToInt(40 * UiTextScale);
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

