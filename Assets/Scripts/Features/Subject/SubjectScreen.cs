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
                subtitleText.text = "先选 1 门首选，再选 2 门再选科";
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
            var baseTip = "首选决定主方向（物理/历史），再选决定更具体的路线。\n它会影响后续能接到的任务线，以及部分专业的可选范围。";
            var directions = GetDirectionPreview();
            if (string.IsNullOrWhiteSpace(directions))
            {
                return baseTip;
            }

            return $"{baseTip}\n\n未来方向参考：\n{directions}";
        }

        private string GetDirectionPreview()
        {
            if (selectedFirst == FirstSubject.None)
            {
                return "物理：工科/理科/计算机/部分医学\n历史：法学/中文/新闻/教育/管理/考公";
            }

            if (selectedFirst == FirstSubject.Physics)
            {
                var list = new List<string> { "工科/理科/计算机" };
                if (selectedSeconds.Contains(SecondSubject.Chemistry)) list.Add("化工/材料/医药相关");
                if (selectedSeconds.Contains(SecondSubject.Biology)) list.Add("生命科学/医学相关");
                if (selectedSeconds.Contains(SecondSubject.Geography)) list.Add("环境/城规/地理信息");
                if (selectedSeconds.Contains(SecondSubject.Politics)) list.Add("公管/部分社科");
                return string.Join("\n", list.ToArray());
            }

            if (selectedFirst == FirstSubject.History)
            {
                var list = new List<string> { "法学/文史/教育/管理" };
                if (selectedSeconds.Contains(SecondSubject.Politics)) list.Add("考公/公管/法学方向更稳");
                if (selectedSeconds.Contains(SecondSubject.Geography)) list.Add("地理/环境/城规/旅游");
                if (selectedSeconds.Contains(SecondSubject.Biology)) list.Add("护理/运动康复/部分应用方向");
                if (selectedSeconds.Contains(SecondSubject.Chemistry)) list.Add("少量跨学科机会（看省份规则）");
                return string.Join("\n", list.ToArray());
            }

            return string.Empty;
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

            var title = CreateText("Title", card, font, 60, FontStyle.Bold, UITheme.Text);
            title.alignment = TextAnchor.UpperCenter;
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.82f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.text = "如果不知道选什么";

            var intro = CreateText("Intro", card, font, 38, FontStyle.Normal, UITheme.TextSoft);
            intro.alignment = TextAnchor.UpperLeft;
            intro.rectTransform.anchorMin = new Vector2(0.08f, 0.66f);
            intro.rectTransform.anchorMax = new Vector2(0.92f, 0.82f);
            intro.rectTransform.offsetMin = Vector2.zero;
            intro.rectTransform.offsetMax = Vector2.zero;
            intro.text = "点你更喜欢的方向，我给你一套更适合的选科组合。\n你仍然可以再手动调整。";

            var optionsRoot = CreateUiObject("Options", card);
            optionsRoot.anchorMin = new Vector2(0.08f, 0.18f);
            optionsRoot.anchorMax = new Vector2(0.92f, 0.66f);
            optionsRoot.offsetMin = Vector2.zero;
            optionsRoot.offsetMax = Vector2.zero;
            var layout = optionsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 18f;

            CreateExpertOption("喜欢做题/实验/推理", () => ApplyRecommendation(FirstSubject.Physics, SecondSubject.Chemistry, SecondSubject.Biology), optionsRoot, font);
            CreateExpertOption("想走计算机/工程", () => ApplyRecommendation(FirstSubject.Physics, SecondSubject.Chemistry, SecondSubject.Geography), optionsRoot, font);
            CreateExpertOption("喜欢人文/阅读/表达", () => ApplyRecommendation(FirstSubject.History, SecondSubject.Politics, SecondSubject.Biology), optionsRoot, font);
            CreateExpertOption("更倾向考公/法学/管理", () => ApplyRecommendation(FirstSubject.History, SecondSubject.Politics, SecondSubject.Geography), optionsRoot, font);

            var close = CreatePrimaryButton("先自己选 →", card, font, UITheme.Confirm, Color.white);
            close.gameObject.AddComponent<UiPressScale>();
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = new Vector2(0.12f, 0.06f);
            closeRect.anchorMax = new Vector2(0.88f, 0.16f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            close.onClick.AddListener(() => overlay.gameObject.SetActive(false));

            overlay.gameObject.SetActive(false);
            return overlay;
        }

        private void CreateExpertOption(string label, UnityAction onClick, Transform parent, Font font)
        {
            var button = CreatePrimaryButton(label, parent, font, UITheme.Gold, UITheme.Text);
            button.gameObject.AddComponent<UiPressScale>();
            button.onClick.AddListener(() =>
            {
                if (expertPopupRoot != null)
                {
                    expertPopupRoot.gameObject.SetActive(false);
                }
                onClick?.Invoke();
            });
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
            tip.anchorMin = new Vector2(0.06f, 0.78f);
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
            firstTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.70f);
            firstTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.82f);
            firstTitle.rectTransform.offsetMin = Vector2.zero;
            firstTitle.rectTransform.offsetMax = Vector2.zero;
            firstTitle.text = "首选科目（选 1）";

            var firstRow = CreateUiObject("FirstRow", body);
            firstRow.anchorMin = new Vector2(0.06f, 0.54f);
            firstRow.anchorMax = new Vector2(0.94f, 0.70f);
            firstRow.offsetMin = Vector2.zero;
            firstRow.offsetMax = Vector2.zero;

            physicsButton = CreatePrimaryButton("物理", firstRow, font, UITheme.FromHex("4F7BD9"), UITheme.Text);
            var phyRect = (RectTransform)physicsButton.transform;
            phyRect.anchorMin = new Vector2(0f, 0f);
            phyRect.anchorMax = new Vector2(0.49f, 1f);
            phyRect.offsetMin = Vector2.zero;
            phyRect.offsetMax = Vector2.zero;
            physicsButton.gameObject.AddComponent<UiPressScale>();

            historyButton = CreatePrimaryButton("历史", firstRow, font, UITheme.FromHex("FFB74D"), UITheme.Text);
            var hisRect = (RectTransform)historyButton.transform;
            hisRect.anchorMin = new Vector2(0.51f, 0f);
            hisRect.anchorMax = new Vector2(1f, 1f);
            hisRect.offsetMin = Vector2.zero;
            hisRect.offsetMax = Vector2.zero;
            historyButton.gameObject.AddComponent<UiPressScale>();

            var secondTitle = CreateText("SecondTitle", body, font, 40, FontStyle.Bold, UITheme.Text);
            secondTitle.alignment = TextAnchor.MiddleLeft;
            secondTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.44f);
            secondTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.54f);
            secondTitle.rectTransform.offsetMin = Vector2.zero;
            secondTitle.rectTransform.offsetMax = Vector2.zero;
            secondTitle.text = "再选科目（选 2）";

            var secondGrid = CreateUiObject("SecondGrid", body);
            secondGrid.anchorMin = new Vector2(0.06f, 0.14f);
            secondGrid.anchorMax = new Vector2(0.94f, 0.44f);
            secondGrid.offsetMin = Vector2.zero;
            secondGrid.offsetMax = Vector2.zero;
            var grid = secondGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(520f, 140f);
            grid.spacing = new Vector2(18f, 18f);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.childAlignment = TextAnchor.UpperCenter;

            politicsButton = CreateSecondaryCard("政治", secondGrid, font);
            geographyButton = CreateSecondaryCard("地理", secondGrid, font);
            chemistryButton = CreateSecondaryCard("化学", secondGrid, font);
            biologyButton = CreateSecondaryCard("生物", secondGrid, font);

            selectionText = CreateText("SelectionText", body, font, 36, FontStyle.Bold, UITheme.TextSoft);
            selectionText.alignment = TextAnchor.MiddleCenter;
            selectionText.rectTransform.anchorMin = new Vector2(0.06f, 0.00f);
            selectionText.rectTransform.anchorMax = new Vector2(0.94f, 0.14f);
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

        private static Button CreateSecondaryCard(string label, Transform parent, Font font)
        {
            var btn = CreateButton(label, parent, font, UITheme.BgCard, UITheme.Text);
            btn.gameObject.AddComponent<UiPressScale>();
            return btn;
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

