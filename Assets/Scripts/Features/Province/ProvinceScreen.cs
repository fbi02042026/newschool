using System.Collections.Generic;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Province
{
    public class ProvinceScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private RectTransform hotListRoot;
        [SerializeField] private RectTransform allListRoot;
        [SerializeField] private RectTransform infoPanelRoot;
        [SerializeField] private Text infoNameText;
        [SerializeField] private Image infoModeChipImage;
        [SerializeField] private Button infoModeChipButton;
        [SerializeField] private Text infoModeText;
        [SerializeField] private Text infoDiffText;
        [SerializeField] private Text infoDescText;
        [SerializeField] private ScrollRect allListScrollRect;
        [SerializeField] private RectTransform modeTipRoot;
        [SerializeField] private Text modeTipTitleText;
        [SerializeField] private Text modeTipBodyText;
        [SerializeField] private Button modeTipCloseButton;

        private readonly Dictionary<string, List<ProvinceCardView>> cardViewsByProvince = new Dictionary<string, List<ProvinceCardView>>();
        private LayoutElement hotListLayoutElement;
        private LayoutElement allListLayoutElement;
        private string selectedProvinceName;
        private bool cardsBuilt;
        private RectTransform modeExplainPopupRoot;

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            EnsureListContainers();
            BindEvents();
            BuildProvinceCards();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.CurrentProgress = Core.GameProgress.Province;
                state.HasSaveData = true;
                if (!string.IsNullOrWhiteSpace(state.SelectedProvince))
                {
                    selectedProvinceName = state.SelectedProvince;
                }
            }

            GuideService.EnsureHelpButton(transform.Find("Panel/ScreenHeader") ?? transform, "GuideHelpButton", () => GuideService.Open(ScreenType.Province, transform));
            GuideService.TryShowOnce(ScreenType.Province, transform);
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = "选择你的省市";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "每个地区的考试命题方式不同，选择你想参加高考的省份";
            }

            UpdateListLayoutHeights();
            UpdateCardStates();
            UpdateInfoPanel();
            UpdateConfirmState();
        }

        private void EnsureListContainers()
        {
            hotListLayoutElement = EnsureGridContainerStyle(hotListRoot);
            allListLayoutElement = EnsureGridContainerStyle(allListRoot);

            if (allListScrollRect != null && allListLayoutElement != null)
            {
                Destroy(allListLayoutElement);
                allListLayoutElement = null;
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null &&
                confirmButton != null &&
                titleText != null &&
                subtitleText != null &&
                hotListRoot != null &&
                allListRoot != null &&
                infoPanelRoot != null &&
                infoNameText != null &&
                infoModeChipImage != null &&
                infoModeChipButton != null &&
                infoModeText != null &&
                infoDiffText != null &&
                infoDescText != null &&
                allListScrollRect != null &&
                modeTipRoot != null &&
                modeTipTitleText != null &&
                modeTipBodyText != null &&
                modeTipCloseButton != null)
            {
                return;
            }

            BuildRuntimeLayout();
        }

        private void BindEvents()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(ConfirmProvince);
            }

            if (infoModeChipButton != null)
            {
                infoModeChipButton.onClick.RemoveAllListeners();
                infoModeChipButton.onClick.AddListener(OpenModeTip);
            }

            if (modeTipCloseButton != null)
            {
                modeTipCloseButton.onClick.RemoveAllListeners();
                modeTipCloseButton.onClick.AddListener(CloseModeTip);
            }
        }

        private void BuildProvinceCards()
        {
            if (cardsBuilt)
            {
                return;
            }

            cardsBuilt = true;
            cardViewsByProvince.Clear();
            ClearChildren(hotListRoot);
            ClearChildren(allListRoot);

            var hot = new List<ProvinceOption>(ProvinceCatalog.HotOptions);
            hot.Sort(CompareOptionsByDifficulty);
            foreach (var option in hot)
            {
                CreateProvinceCard(hotListRoot, option);
            }

            var all = new List<ProvinceOption>(ProvinceCatalog.AllOptions);
            all.Sort(CompareOptionsByDifficulty);
            foreach (var option in all)
            {
                CreateProvinceCard(allListRoot, option);
            }
        }

        private void CreateProvinceCard(RectTransform parent, ProvinceOption option)
        {
            if (parent == null || option == null)
            {
                return;
            }

            var card = new GameObject($"Province_{option.Name}", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)card.transform;
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;

            var bg = card.GetComponent<Image>();
            bg.color = GetDifficultyCardColor(option.DifficultyScore);
            card.AddComponent<UiAutoRounded>();

            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            outline.effectDistance = new Vector2(4f, -4f);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.05f);
            shadow.effectDistance = new Vector2(0f, -10f);

            var button = card.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => SelectProvince(option.Name));
            card.AddComponent<UiPressScale>();

            var nameText = CreateText("Name", rect, BuiltinFont(), 34, FontStyle.Bold, UITheme.Text);
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.rectTransform.anchorMin = new Vector2(0.08f, 0.60f);
            nameText.rectTransform.anchorMax = new Vector2(0.92f, 0.90f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;
            nameText.text = option.Name;

            var modeText = CreateText("ModeText", rect, BuiltinFont(), 24, FontStyle.Normal, UITheme.TextLight);
            modeText.alignment = TextAnchor.MiddleLeft;
            modeText.rectTransform.anchorMin = new Vector2(0.08f, 0.18f);
            modeText.rectTransform.anchorMax = new Vector2(0.92f, 0.46f);
            modeText.rectTransform.offsetMin = Vector2.zero;
            modeText.rectTransform.offsetMax = Vector2.zero;
            modeText.text = option.Mode;
            modeText.color = GetModeAccentColor(option.Mode);

            var view = new ProvinceCardView
            {
                Option = option,
                Button = button,
                Background = bg,
                Outline = outline,
                NameText = nameText,
                ModeText = modeText
            };

            if (!cardViewsByProvince.TryGetValue(option.Name, out var list))
            {
                list = new List<ProvinceCardView>();
                cardViewsByProvince[option.Name] = list;
            }

            list.Add(view);
        }

        private void SelectProvince(string provinceName)
        {
            selectedProvinceName = provinceName;
            Refresh();
        }

        private void UpdateCardStates()
        {
            foreach (var pair in cardViewsByProvince)
            {
                var isSelected = pair.Key == selectedProvinceName;
                foreach (var view in pair.Value)
                {
                    ApplyCardState(view, isSelected);
                }
            }
        }

        private void ApplyCardState(ProvinceCardView view, bool isSelected)
        {
            if (view == null || view.Option == null)
            {
                return;
            }

            if (view.Background != null)
            {
                view.Background.color = GetDifficultyCardColor(view.Option.DifficultyScore);
            }

            if (view.Outline != null)
            {
                view.Outline.effectColor = isSelected
                    ? new Color32(UITheme.Confirm.r, UITheme.Confirm.g, UITheme.Confirm.b, 255)
                    : new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
                view.Outline.effectDistance = isSelected ? new Vector2(5f, -5f) : new Vector2(4f, -4f);
            }

            if (view.NameText != null)
            {
                view.NameText.color = isSelected ? UITheme.ConfirmHover : UITheme.Text;
            }

            if (view.ModeText != null)
            {
                view.ModeText.color = isSelected ? GetSelectedModeAccentColor(view.Option.Mode) : GetModeAccentColor(view.Option.Mode);
            }
        }

        private void UpdateInfoPanel()
        {
            var option = ProvinceCatalog.Find(selectedProvinceName);
            if (infoPanelRoot != null)
            {
                infoPanelRoot.gameObject.SetActive(option != null);
            }

            if (option == null)
            {
                if (infoModeChipButton != null)
                {
                    infoModeChipButton.interactable = false;
                }
                return;
            }

            if (infoNameText != null)
            {
                infoNameText.text = string.Empty;
                infoNameText.gameObject.SetActive(false);
            }

            if (infoModeChipImage != null)
            {
                infoModeChipImage.color = GetModeChipColor(option.Mode);
            }

            if (infoModeText != null)
            {
                infoModeText.text = GetConfirmModeLabel(option);
            }

            if (infoModeChipButton != null)
            {
                infoModeChipButton.interactable = true;
            }

            if (infoDescText != null)
            {
                infoDescText.text = option.Description ?? string.Empty;
                infoDescText.gameObject.SetActive(true);
            }
        }

        private void OpenModeTip()
        {
            var option = ProvinceCatalog.Find(selectedProvinceName);
            if (option == null)
            {
                ShowToast("先选一个省市，再看模式说明");
                return;
            }

            if (modeTipTitleText != null)
            {
                modeTipTitleText.text = $"模式说明 · {GetConfirmModeLabel(option)}";
            }

            if (modeTipBodyText != null)
            {
                modeTipBodyText.text = GetModeTipBody(option.Mode);
            }

            if (modeTipRoot != null)
            {
                modeTipRoot.gameObject.SetActive(true);
            }
        }

        private void CloseModeTip()
        {
            if (modeTipRoot != null)
            {
                modeTipRoot.gameObject.SetActive(false);
            }
        }

        private void ShowModeExplainPopup()
        {
            if (modeExplainPopupRoot != null)
            {
                modeExplainPopupRoot.gameObject.SetActive(true);
            }
        }

        private void CloseModeExplainPopup()
        {
            if (modeExplainPopupRoot != null)
            {
                modeExplainPopupRoot.gameObject.SetActive(false);
            }
        }

        private void UpdateConfirmState()
        {
            var hasSelection = !string.IsNullOrWhiteSpace(selectedProvinceName);
            if (confirmButton == null)
            {
                return;
            }

            confirmButton.interactable = hasSelection;
            var image = confirmButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = hasSelection ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }

            var label = confirmButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = hasSelection
                    ? $"确认进入选科 · {GetConfirmModeLabel(ProvinceCatalog.Find(selectedProvinceName))} →"
                    : "先选择城市 / 省市";
                label.color = hasSelection ? Color.white : new Color(1f, 1f, 1f, 0.88f);
            }
        }

        private void ConfirmProvince()
        {
            var option = ProvinceCatalog.Find(selectedProvinceName);
            if (option == null)
            {
                ShowToast("先选一个省市，再继续下一步");
                return;
            }

            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.SelectedProvince = option.Name;
                state.SelectedProvinceMode = option.Mode;
                state.CurrentProgress = Core.GameProgress.Subject;
                state.HasSaveData = true;
            }

            NavigateTo(ScreenType.Subject, true);
        }

        private void UpdateListLayoutHeights()
        {
            if (hotListLayoutElement != null)
            {
                var grid = hotListRoot != null ? hotListRoot.GetComponent<GridLayoutGroup>() : null;
                if (grid != null && grid.constraintCount > 0)
                {
                    var rows = Mathf.CeilToInt(ProvinceCatalog.HotOptions.Length / (float)grid.constraintCount);
                    hotListLayoutElement.preferredHeight = rows * grid.cellSize.y + Mathf.Max(0f, rows - 1) * grid.spacing.y + grid.padding.top + grid.padding.bottom;
                }
            }

            if (allListLayoutElement != null)
            {
                var grid = allListRoot != null ? allListRoot.GetComponent<GridLayoutGroup>() : null;
                if (grid != null && grid.constraintCount > 0)
                {
                    var rows = Mathf.CeilToInt(ProvinceCatalog.AllOptions.Length / (float)grid.constraintCount);
                    allListLayoutElement.preferredHeight = rows * grid.cellSize.y + Mathf.Max(0f, rows - 1) * grid.spacing.y + grid.padding.top + grid.padding.bottom;
                }
            }
        }

        private static int CompareOptionsByDifficulty(ProvinceOption a, ProvinceOption b)
        {
            var diffCompare = a.DifficultyScore.CompareTo(b.DifficultyScore);
            if (diffCompare != 0) return diffCompare;
            return string.CompareOrdinal(a?.Name ?? string.Empty, b?.Name ?? string.Empty);
        }

        private static Color32 GetModeChipColor(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return UITheme.Confirm;
            if (mode.Contains("自主") || mode.Contains("命题")) return UITheme.FromHex("4DB38A");
            if (mode.Contains("民族") || mode.Contains("专项")) return UITheme.FromHex("E8A53A");
            return UITheme.FromHex("4F7BD9");
        }

        private static string GetConfirmModeLabel(ProvinceOption option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.Mode))
            {
                return "查看模式";
            }
            if (option.Mode.Contains("自主") || option.Mode.Contains("命题")) return "自主命题模式";
            if (option.Mode.Contains("民族") || option.Mode.Contains("专项")) return "全国卷(含专项计划)";
            return "统一卷模式";
        }

        private static string GetModeTipBody(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return "请先选择一个地区查看详情。";
            if (mode.Contains("自主") || mode.Contains("命题"))
            {
                return "自主命题由本省自行组织命题。\n试题风格和方向更贴近当地学习特点。";
            }
            if (mode.Contains("民族") || mode.Contains("专项"))
            {
                return "全国统一命题框架下，有专项计划支持。\n部分地区有独立的录取安排和加分政策。";
            }
            return "统一卷模式采用全国统一命题。\n标准更统一，全国范围内可比较性更强。";
        }

        private void BuildRuntimeLayout()
        {
            var font = BuiltinFont();

            var background = CreateUiObject("Background", transform);
            Stretch(background);
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgGradient = background.gameObject.AddComponent<UiCornerGradient>();
            bgGradient.SetColors(UITheme.CardButter, UITheme.Bg, UITheme.CardSky, UITheme.CardLavender);

            var dots = CreateUiObject("BgDots", transform);
            Stretch(dots);
            GenerateDots(dots, 12);

            var panel = CreateUiObject("Panel", transform);
            panel.anchorMin = new Vector2(0.05f, 0.04f);
            panel.anchorMax = new Vector2(0.95f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateUiObject("ScreenHeader", panel);
            header.anchorMin = new Vector2(0f, 0.81f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var badge = CreateUiObject("StepBadge", header);
            badge.anchorMin = new Vector2(0.36f, 0.68f);
            badge.anchorMax = new Vector2(0.64f, 0.96f);
            badge.offsetMin = Vector2.zero;
            badge.offsetMax = Vector2.zero;
            var badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.color = UITheme.CardPeach;
            badge.gameObject.AddComponent<UiAutoRounded>();
            var badgeText = CreateText("Text", badge, font, 32, FontStyle.Bold, UITheme.Accent);
            Stretch(badgeText.rectTransform);
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.text = "STEP 3 / 5";

            backButton = CreateOutlineButton("← 返回", header, font);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.68f);
            backRect.anchorMax = new Vector2(0.24f, 0.96f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 82, FontStyle.Bold, UITheme.Text);
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.28f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.68f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;
            titleText.alignment = TextAnchor.MiddleCenter;

            subtitleText = CreateText("Subtitle", header, font, 40, FontStyle.Normal, UITheme.TextLight);
            subtitleText.rectTransform.anchorMin = new Vector2(0.04f, 0f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.96f, 0.28f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;
            subtitleText.alignment = TextAnchor.MiddleCenter;

            var modeHelpButton = CreateSmallButton("?", header, font, UITheme.CardSky, UITheme.Text);
            var modeHelpRect = (RectTransform)modeHelpButton.transform;
            modeHelpRect.anchorMin = new Vector2(0.82f, 0.68f);
            modeHelpRect.anchorMax = new Vector2(0.98f, 0.96f);
            modeHelpRect.offsetMin = Vector2.zero;
            modeHelpRect.offsetMax = Vector2.zero;
            modeHelpButton.gameObject.AddComponent<UiPressScale>();
            modeHelpButton.onClick.AddListener(ShowModeExplainPopup);

            var body = CreateUiObject("ScreenBody", panel);
            body.anchorMin = new Vector2(0f, 0.26f);
            body.anchorMax = new Vector2(1f, 0.79f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var hotPanel = CreateUiObject("HotPanel", body);
            hotPanel.anchorMin = new Vector2(0f, 0.60f);
            hotPanel.anchorMax = new Vector2(1f, 1f);
            hotPanel.offsetMin = Vector2.zero;
            hotPanel.offsetMax = Vector2.zero;
            var hotPanelImage = hotPanel.gameObject.AddComponent<Image>();
            hotPanelImage.color = new Color32(255, 255, 255, 246);
            hotPanel.gameObject.AddComponent<UiAutoRounded>();

            var hotTitleRow = CreateSectionHeader(hotPanel, font, "热门城市", "推荐");
            hotTitleRow.anchorMin = new Vector2(0.04f, 0.74f);
            hotTitleRow.anchorMax = new Vector2(0.96f, 0.96f);
            hotTitleRow.offsetMin = Vector2.zero;
            hotTitleRow.offsetMax = Vector2.zero;

            hotListRoot = CreateGridContainer("HotList", hotPanel, out hotListLayoutElement);
            hotListRoot.anchorMin = new Vector2(0.04f, 0.08f);
            hotListRoot.anchorMax = new Vector2(0.96f, 0.70f);
            hotListRoot.offsetMin = Vector2.zero;
            hotListRoot.offsetMax = Vector2.zero;
            var hotGrid = hotListRoot.GetComponent<GridLayoutGroup>();
            if (hotGrid != null)
            {
                hotGrid.constraintCount = 4;
                hotGrid.cellSize = new Vector2(0f, 160f);
                hotGrid.spacing = new Vector2(18f, 18f);
                hotGrid.padding = new RectOffset(6, 6, 6, 6);
            }

            var divider = CreateUiObject("SectionDivider", body);
            divider.anchorMin = new Vector2(0.03f, 0.54f);
            divider.anchorMax = new Vector2(0.97f, 0.57f);
            divider.offsetMin = Vector2.zero;
            divider.offsetMax = Vector2.zero;
            var dividerImage = divider.gameObject.AddComponent<Image>();
            dividerImage.color = new Color32(255, 217, 224, 255);
            divider.gameObject.AddComponent<UiAutoRounded>();

            var allPanel = CreateUiObject("AllPanel", body);
            allPanel.anchorMin = new Vector2(0f, 0f);
            allPanel.anchorMax = new Vector2(1f, 0.52f);
            allPanel.offsetMin = Vector2.zero;
            allPanel.offsetMax = Vector2.zero;
            var allPanelImage = allPanel.gameObject.AddComponent<Image>();
            allPanelImage.color = new Color32(255, 255, 255, 246);
            allPanel.gameObject.AddComponent<UiAutoRounded>();

            var allTitleRow = CreateSectionHeader(allPanel, font, "全部省市", null);
            allTitleRow.anchorMin = new Vector2(0.04f, 0.82f);
            allTitleRow.anchorMax = new Vector2(0.96f, 0.98f);
            allTitleRow.offsetMin = Vector2.zero;
            allTitleRow.offsetMax = Vector2.zero;

            var scrollRoot = CreateUiObject("AllScrollRoot", allPanel);
            scrollRoot.anchorMin = new Vector2(0.04f, 0.05f);
            scrollRoot.anchorMax = new Vector2(0.96f, 0.78f);
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = Vector2.zero;
            allListScrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            allListScrollRect.horizontal = false;
            allListScrollRect.vertical = true;
            allListScrollRect.movementType = ScrollRect.MovementType.Clamped;
            allListScrollRect.scrollSensitivity = 36f;

            var viewport = CreateUiObject("Viewport", scrollRoot);
            Stretch(viewport);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUiObject("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.spacing = 0f;
            contentLayout.padding = new RectOffset(0, 0, 0, 0);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            allListScrollRect.content = content;
            allListScrollRect.viewport = viewport;

            allListRoot = CreateGridContainer("AllList", content, out allListLayoutElement);
            allListRoot.anchorMin = new Vector2(0f, 1f);
            allListRoot.anchorMax = new Vector2(1f, 1f);
            allListRoot.pivot = new Vector2(0.5f, 1f);
            allListRoot.offsetMin = Vector2.zero;
            allListRoot.offsetMax = Vector2.zero;
            var allGrid = allListRoot.GetComponent<GridLayoutGroup>();
            if (allGrid != null)
            {
                allGrid.constraintCount = 2;
                allGrid.cellSize = new Vector2(0f, 160f);
                allGrid.spacing = new Vector2(20f, 20f);
                allGrid.padding = new RectOffset(6, 6, 6, 6);
            }

            var footer = CreateUiObject("ScreenFooter", panel);
            footer.anchorMin = new Vector2(0f, 0.03f);
            footer.anchorMax = new Vector2(1f, 0.24f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            infoPanelRoot = CreateUiObject("ProvinceInfoPanel", footer);
            infoPanelRoot.anchorMin = new Vector2(0.02f, 0.46f);
            infoPanelRoot.anchorMax = new Vector2(0.98f, 1f);
            infoPanelRoot.offsetMin = Vector2.zero;
            infoPanelRoot.offsetMax = Vector2.zero;
            var infoBg = infoPanelRoot.gameObject.AddComponent<Image>();
            infoBg.color = UITheme.CardSky;
            infoPanelRoot.gameObject.AddComponent<UiAutoRounded>();

            infoNameText = CreateText("InfoName", infoPanelRoot, font, 34, FontStyle.Bold, UITheme.Text);
            infoNameText.alignment = TextAnchor.MiddleLeft;
            infoNameText.rectTransform.anchorMin = new Vector2(0.04f, 0.56f);
            infoNameText.rectTransform.anchorMax = new Vector2(0.52f, 0.92f);
            infoNameText.rectTransform.offsetMin = Vector2.zero;
            infoNameText.rectTransform.offsetMax = Vector2.zero;

            var modeChip = CreateUiObject("InfoModeChip", infoPanelRoot);
            modeChip.anchorMin = new Vector2(0.56f, 0.56f);
            modeChip.anchorMax = new Vector2(0.94f, 0.92f);
            modeChip.offsetMin = Vector2.zero;
            modeChip.offsetMax = Vector2.zero;
            infoModeChipImage = modeChip.gameObject.AddComponent<Image>();
            infoModeChipButton = modeChip.gameObject.AddComponent<Button>();
            modeChip.gameObject.AddComponent<UiAutoRounded>();
            infoModeText = CreateText("InfoModeText", modeChip, font, 24, FontStyle.Bold, Color.white);
            Stretch(infoModeText.rectTransform);
            infoModeText.alignment = TextAnchor.MiddleCenter;

            infoDiffText = CreateText("InfoDiff", infoPanelRoot, font, 24, FontStyle.Bold, UITheme.TextSoft);
            infoDiffText.alignment = TextAnchor.MiddleLeft;
            infoDiffText.rectTransform.anchorMin = new Vector2(0.04f, 0.24f);
            infoDiffText.rectTransform.anchorMax = new Vector2(0.94f, 0.50f);
            infoDiffText.rectTransform.offsetMin = Vector2.zero;
            infoDiffText.rectTransform.offsetMax = Vector2.zero;

            infoDescText = CreateText("InfoDesc", infoPanelRoot, font, 24, FontStyle.Normal, UITheme.TextLight);
            infoDescText.alignment = TextAnchor.UpperLeft;
            infoDescText.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
            infoDescText.rectTransform.anchorMax = new Vector2(0.94f, 0.24f);
            infoDescText.rectTransform.offsetMin = Vector2.zero;
            infoDescText.rectTransform.offsetMax = Vector2.zero;

            confirmButton = CreatePrimaryButton("确认进入选科 →", footer, font, UITheme.Confirm, Color.white);
            var confirmRect = (RectTransform)confirmButton.transform;
            confirmRect.anchorMin = new Vector2(0.02f, 0f);
            confirmRect.anchorMax = new Vector2(0.98f, 0.38f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;

            modeTipRoot = CreateUiObject("ModeTip", transform);
            Stretch(modeTipRoot);
            modeTipRoot.gameObject.SetActive(false);
            var tipMaskImage = modeTipRoot.gameObject.AddComponent<Image>();
            tipMaskImage.color = new Color(0f, 0f, 0f, 0.55f);
            var tipMaskButton = modeTipRoot.gameObject.AddComponent<Button>();
            tipMaskButton.onClick.AddListener(CloseModeTip);

            var tipCard = CreateUiObject("TipCard", modeTipRoot);
            tipCard.anchorMin = new Vector2(0.12f, 0.32f);
            tipCard.anchorMax = new Vector2(0.88f, 0.68f);
            tipCard.offsetMin = Vector2.zero;
            tipCard.offsetMax = Vector2.zero;
            var tipCardImage = tipCard.gameObject.AddComponent<Image>();
            tipCardImage.color = Color.white;
            tipCard.gameObject.AddComponent<UiAutoRounded>();
            var tipCardShadow = tipCard.gameObject.AddComponent<Shadow>();
            tipCardShadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
            tipCardShadow.effectDistance = new Vector2(0f, -16f);

            modeTipTitleText = CreateText("Title", tipCard, font, 38, FontStyle.Bold, UITheme.Text);
            modeTipTitleText.alignment = TextAnchor.MiddleCenter;
            modeTipTitleText.rectTransform.anchorMin = new Vector2(0.08f, 0.66f);
            modeTipTitleText.rectTransform.anchorMax = new Vector2(0.92f, 0.90f);
            modeTipTitleText.rectTransform.offsetMin = Vector2.zero;
            modeTipTitleText.rectTransform.offsetMax = Vector2.zero;

            modeTipBodyText = CreateText("Body", tipCard, font, 28, FontStyle.Normal, UITheme.TextSoft);
            modeTipBodyText.alignment = TextAnchor.UpperLeft;
            modeTipBodyText.rectTransform.anchorMin = new Vector2(0.08f, 0.20f);
            modeTipBodyText.rectTransform.anchorMax = new Vector2(0.92f, 0.62f);
            modeTipBodyText.rectTransform.offsetMin = Vector2.zero;
            modeTipBodyText.rectTransform.offsetMax = Vector2.zero;

            var tipCloseRect = CreateUiObject("CloseButton", tipCard);
            tipCloseRect.anchorMin = new Vector2(0.26f, 0.04f);
            tipCloseRect.anchorMax = new Vector2(0.74f, 0.16f);
            tipCloseRect.offsetMin = Vector2.zero;
            tipCloseRect.offsetMax = Vector2.zero;
            var closeImage = tipCloseRect.gameObject.AddComponent<Image>();
            closeImage.color = Color.white;
            tipCloseRect.gameObject.AddComponent<UiAutoRounded>();
            var closeGradient = tipCloseRect.gameObject.AddComponent<UiCornerGradient>();
            closeGradient.SetColors(UITheme.Confirm, UITheme.ConfirmHover, UITheme.ConfirmHover, UITheme.Confirm);
            modeTipCloseButton = tipCloseRect.gameObject.AddComponent<Button>();
            tipCloseRect.gameObject.AddComponent<UiPressScale>();
            var tipCloseText = CreateText("Text", tipCloseRect, font, 28, FontStyle.Bold, Color.white);
            Stretch(tipCloseText.rectTransform);
            tipCloseText.alignment = TextAnchor.MiddleCenter;
            tipCloseText.text = "我知道啦";

            infoPanelRoot.gameObject.SetActive(false);

            modeExplainPopupRoot = CreateUiObject("ModeExplainPopup", transform);
            Stretch(modeExplainPopupRoot);
            modeExplainPopupRoot.gameObject.SetActive(false);
            var explainMaskImage = modeExplainPopupRoot.gameObject.AddComponent<Image>();
            explainMaskImage.color = new Color(0f, 0f, 0f, 0.45f);
            var explainMaskButton = modeExplainPopupRoot.gameObject.AddComponent<Button>();
            explainMaskButton.onClick.AddListener(CloseModeExplainPopup);

            var explainCard = CreateUiObject("Card", modeExplainPopupRoot);
            explainCard.anchorMin = new Vector2(0.08f, 0.18f);
            explainCard.anchorMax = new Vector2(0.92f, 0.82f);
            explainCard.offsetMin = Vector2.zero;
            explainCard.offsetMax = Vector2.zero;
            var explainCardImage = explainCard.gameObject.AddComponent<Image>();
            explainCardImage.color = Color.white;
            explainCard.gameObject.AddComponent<UiAutoRounded>();
            var explainCardShadow = explainCard.gameObject.AddComponent<Shadow>();
            explainCardShadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            explainCardShadow.effectDistance = new Vector2(0f, -16f);

            var explainTitle = CreateText("Title", explainCard, font, 56, FontStyle.Bold, UITheme.Text);
            explainTitle.alignment = TextAnchor.UpperCenter;
            explainTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.74f);
            explainTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            explainTitle.rectTransform.offsetMin = Vector2.zero;
            explainTitle.rectTransform.offsetMax = Vector2.zero;
            explainTitle.text = "考试模式说明";

            var blueCard = CreateUiObject("BlueCard", explainCard);
            blueCard.anchorMin = new Vector2(0.06f, 0.56f);
            blueCard.anchorMax = new Vector2(0.94f, 0.72f);
            blueCard.offsetMin = Vector2.zero;
            blueCard.offsetMax = Vector2.zero;
            var blueBg = blueCard.gameObject.AddComponent<Image>();
            blueBg.color = UITheme.CardSky;
            blueCard.gameObject.AddComponent<UiAutoRounded>();
            var blueText = CreateText("BlueText", blueCard, font, 28, FontStyle.Normal, UITheme.Text);
            Stretch(blueText.rectTransform);
            blueText.alignment = TextAnchor.MiddleCenter;
            blueText.text = "🔵 蓝色卡片 = 统一卷模式（大多数省份采用全国统一命题）";

            var greenCard = CreateUiObject("GreenCard", explainCard);
            greenCard.anchorMin = new Vector2(0.06f, 0.40f);
            greenCard.anchorMax = new Vector2(0.94f, 0.56f);
            greenCard.offsetMin = Vector2.zero;
            greenCard.offsetMax = Vector2.zero;
            var greenBg = greenCard.gameObject.AddComponent<Image>();
            greenBg.color = UITheme.CardMint;
            greenCard.gameObject.AddComponent<UiAutoRounded>();
            var greenText = CreateText("GreenText", greenCard, font, 28, FontStyle.Normal, UITheme.Text);
            Stretch(greenText.rectTransform);
            greenText.alignment = TextAnchor.MiddleCenter;
            greenText.text = "🟢 绿色卡片 = 自主命题模式（本省自行命题，风格各有特点）";

            var orangeCard = CreateUiObject("OrangeCard", explainCard);
            orangeCard.anchorMin = new Vector2(0.06f, 0.24f);
            orangeCard.anchorMax = new Vector2(0.94f, 0.40f);
            orangeCard.offsetMin = Vector2.zero;
            orangeCard.offsetMax = Vector2.zero;
            var orangeBg = orangeCard.gameObject.AddComponent<Image>();
            orangeBg.color = UITheme.CardButter;
            orangeCard.gameObject.AddComponent<UiAutoRounded>();
            var orangeText = CreateText("OrangeText", orangeCard, font, 28, FontStyle.Normal, UITheme.Text);
            Stretch(orangeText.rectTransform);
            orangeText.alignment = TextAnchor.MiddleCenter;
            orangeText.text = "🟠 橙色卡片 = 全国卷含专项计划（统一命题框架下有专门安排）";

            var note = CreateText("Note", explainCard, font, 26, FontStyle.Normal, UITheme.TextSoft);
            note.alignment = TextAnchor.UpperLeft;
            note.rectTransform.anchorMin = new Vector2(0.06f, 0.12f);
            note.rectTransform.anchorMax = new Vector2(0.94f, 0.24f);
            note.rectTransform.offsetMin = Vector2.zero;
            note.rectTransform.offsetMax = Vector2.zero;
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            note.text = "不同模式考试流程和计分方式有差异，但不影响你的选择本身";

            var explainClose = CreatePrimaryButton("知道了", explainCard, font, UITheme.Confirm, Color.white);
            explainClose.gameObject.AddComponent<UiPressScale>();
            var explainCloseRect = (RectTransform)explainClose.transform;
            explainCloseRect.anchorMin = new Vector2(0.12f, 0.02f);
            explainCloseRect.anchorMax = new Vector2(0.88f, 0.10f);
            explainCloseRect.offsetMin = Vector2.zero;
            explainCloseRect.offsetMax = Vector2.zero;
            explainClose.onClick.AddListener(CloseModeExplainPopup);
        }

        private static RectTransform CreateSectionHeader(Transform parent, Font font, string title, string tag)
        {
            var row = CreateUiObject($"Header_{title}", parent);
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 12;
            layout.childControlHeight = true;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            var titleText = CreateText("Title", row, font, 34, FontStyle.Bold, UITheme.TextLight);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.text = title;
            titleText.gameObject.AddComponent<LayoutElement>().preferredWidth = 320f;

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var tagWrap = CreateUiObject("Tag", row);
                tagWrap.gameObject.AddComponent<LayoutElement>().preferredWidth = 140f;
                var tagImage = tagWrap.gameObject.AddComponent<Image>();
                tagImage.color = UITheme.Accent;
                tagWrap.gameObject.AddComponent<UiAutoRounded>();
                var tagText = CreateText("Text", tagWrap, font, 24, FontStyle.Bold, Color.white);
                Stretch(tagText.rectTransform);
                tagText.alignment = TextAnchor.MiddleCenter;
                tagText.text = tag;
            }

            return row;
        }

        private static RectTransform CreateGridContainer(string name, Transform parent, out LayoutElement layoutElement)
        {
            var root = CreateUiObject(name, parent);
            var grid = root.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(0f, 160f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.spacing = new Vector2(20f, 20f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(6, 6, 6, 6);
            root.gameObject.AddComponent<ProvinceGridSizer>();
            layoutElement = root.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 360f;
            return root;
        }

        private static void GenerateDots(RectTransform parent, int count)
        {
            var random = new System.Random(2026);
            for (int i = 0; i < count; i++)
            {
                var dot = CreateUiObject($"Dot_{i}", parent);
                dot.anchorMin = new Vector2(0.5f, 0.5f);
                dot.anchorMax = new Vector2(0.5f, 0.5f);
                dot.pivot = new Vector2(0.5f, 0.5f);
                dot.sizeDelta = Vector2.one * (float)(random.NextDouble() * 44 + 16);
                dot.anchoredPosition = new Vector2((float)(random.NextDouble() * 940 - 470), (float)(random.NextDouble() * 2240 - 1120));
                var image = dot.gameObject.AddComponent<Image>();
                image.color = i % 3 == 0 ? new Color32(UITheme.CardSky.r, UITheme.CardSky.g, UITheme.CardSky.b, 42)
                    : i % 3 == 1 ? new Color32(UITheme.CardPeach.r, UITheme.CardPeach.g, UITheme.CardPeach.b, 42)
                    : new Color32(UITheme.CardLavender.r, UITheme.CardLavender.g, UITheme.CardLavender.b, 42);
                dot.gameObject.AddComponent<UiAutoRounded>();
                dot.gameObject.AddComponent<UiFloatBob>().Configure((float)(random.NextDouble() * 10 + 6), (float)(random.NextDouble() * 0.45 + 0.35), (float)random.NextDouble());
            }
        }

        private static Button CreateOutlineButton(string label, Transform parent, Font font)
        {
            return CreateButton(label, parent, font, false);
        }

        private static Button CreatePrimaryButton(string label, Transform parent, Font font, Color a, Color textColor)
        {
            var button = CreateButton(label, parent, font, true);
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = textColor;
            }

            var gradient = button.gameObject.GetComponent<UiCornerGradient>();
            if (gradient != null)
            {
                gradient.SetColors(a, UITheme.ConfirmHover, UITheme.ConfirmHover, a);
            }

            return button;
        }

        private static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var btn = CreateButton(label, parent, font, true);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = Mathf.RoundToInt(36 * UiTextScale);
            var image = btn.GetComponent<Image>();
            if (image != null) image.color = bgColor;
            return btn;
        }

        private static Button CreateButton(string label, Transform parent, Font font, bool primary)
        {
            var buttonGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var image = buttonGo.GetComponent<Image>();
            image.color = Color.white;
            buttonGo.AddComponent<UiAutoRounded>();

            if (primary)
            {
                var gradient = buttonGo.AddComponent<UiCornerGradient>();
                gradient.SetColors(UITheme.Confirm, UITheme.ConfirmHover, UITheme.ConfirmHover, UITheme.Confirm);
                var shadow = buttonGo.AddComponent<Shadow>();
                shadow.effectColor = new Color(UITheme.Confirm.r / 255f, UITheme.Confirm.g / 255f, UITheme.Confirm.b / 255f, 0.35f);
                shadow.effectDistance = new Vector2(0f, -12f);
            }
            else
            {
                var outline = buttonGo.AddComponent<Outline>();
                outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
                outline.effectDistance = new Vector2(4f, -4f);
            }

            var text = CreateText("Text", buttonGo.transform, font, 46, FontStyle.Bold, primary ? Color.white : UITheme.Text);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            buttonGo.AddComponent<UiPressScale>();
            return buttonGo.GetComponent<Button>();
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
            text.lineSpacing = 1.12f;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static LayoutElement EnsureGridContainerStyle(RectTransform root)
        {
            if (root == null)
            {
                return null;
            }

            var grid = root.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = root.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.cellSize = new Vector2(0f, 160f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.spacing = new Vector2(20f, 20f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(6, 6, 6, 6);

            if (root.GetComponent<ProvinceGridSizer>() == null)
            {
                root.gameObject.AddComponent<ProvinceGridSizer>();
            }

            var fitter = root.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = root.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layoutElement = root.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = root.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredHeight = 360f;
            return layoutElement;
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

        private static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void ClearChildren(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static Color32 GetDifficultyCardColor(int difficultyScore)
        {
            if (difficultyScore <= 2) return UITheme.CardMint;       // 绿色 - 轻松
            if (difficultyScore <= 3) return UITheme.CardSky;         // 蓝色 - 正常
            return new Color32(255, 215, 215, 255);                   // 红色 - 困难
        }

        private static Color32 GetModeCardColor(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return UITheme.BgCard;
            if (mode.Contains("自主") || mode.Contains("命题")) return UITheme.CardMint;       // 绿色 - 自主命题
            if (mode.Contains("民族") || mode.Contains("专项")) return UITheme.CardButter;       // 橙色 - 专项计划
            return UITheme.CardSky;                                                             // 蓝色 - 统一卷
        }

        private static Color32 GetModeAccentColor(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return UITheme.TextLight;
            if (mode.Contains("自主") || mode.Contains("命题")) return UITheme.FromHex("4DB38A");    // 绿色
            if (mode.Contains("民族") || mode.Contains("专项")) return UITheme.FromHex("E8A53A");    // 橙色
            return UITheme.FromHex("4F7BD9");                                                       // 蓝色
        }

        private static Color32 GetSelectedModeAccentColor(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return UITheme.Text;
            if (mode.Contains("自主") || mode.Contains("命题")) return UITheme.FromHex("2D8A5A");
            if (mode.Contains("民族") || mode.Contains("专项")) return UITheme.FromHex("C87828");
            return UITheme.FromHex("2F5BB9");
        }

        private sealed class ProvinceCardView
        {
            public ProvinceOption Option;
            public Button Button;
            public Image Background;
            public Outline Outline;
            public Text NameText;
            public Text ModeText;
        }

        private sealed class ProvinceOption
        {
            public string Name;
            public string Mode;
            public string Description;
            public int DifficultyScore;

            public ProvinceOption(string name, string mode, string description, int difficultyScore = 3)
            {
                Name = name;
                Mode = mode;
                Description = description;
                DifficultyScore = difficultyScore;
            }
        }

        private static class ProvinceCatalog
        {
            public static readonly ProvinceOption[] HotOptions =
            {
                new ProvinceOption("北京", "自主命题", "京派试题风格灵活，综合素养导向明显", 1),
                new ProvinceOption("上海", "自主命题", "海派命题侧重实际应用和跨学科思维", 1),
                new ProvinceOption("浙江", "自主命题", "率先试行选考赋分，组合空间更大", 2),
                new ProvinceOption("江苏", "统一卷(新高考Ⅰ卷)", "学科竞赛氛围浓厚，考试竞争有挑战", 5)
            };

            public static readonly ProvinceOption[] AllOptions =
            {
                new ProvinceOption("海南", "自主命题", "旅游资源丰富，考试模式有一定特色", 1),
                new ProvinceOption("天津", "自主命题", "邻近首都，考查方向偏实用", 1),
                new ProvinceOption("台湾", "自主命题", "海岛风情，教育体系独具特色", 1),
                new ProvinceOption("香港", "自主命题", "中西融合，国际视野开阔", 1),
                new ProvinceOption("澳门", "自主命题", "中西融合，文化底蕴深厚", 1),
                new ProvinceOption("新疆", "全国卷(民族类考生有专项计划)", "广袤土地上自有独特学习路径", 1),
                new ProvinceOption("西藏", "全国卷(民族类考生有专项计划)", "广袤土地上自有独特学习路径", 1),
                new ProvinceOption("安徽", "统一卷(全国甲卷)", "中规中矩，学习氛围踏实", 2),
                new ProvinceOption("江西", "统一卷(全国甲卷)", "中规中矩，学习氛围踏实", 2),
                new ProvinceOption("山西", "统一卷(全国甲卷)", "中规中矩，学习氛围踏实", 2),
                new ProvinceOption("陕西", "统一卷(全国甲卷)", "中规中矩，学习氛围踏实", 2),
                new ProvinceOption("吉林", "统一卷(全国甲卷)", "四季分明，备考节奏分明", 2),
                new ProvinceOption("内蒙古", "统一卷(全国甲卷)", "地域广阔，视野开阔", 2),
                new ProvinceOption("甘肃", "统一卷(全国甲卷)", "积淀深厚，学习风气质朴", 2),
                new ProvinceOption("青海", "统一卷(全国甲卷)", "环境单纯，专注力较易保持", 2),
                new ProvinceOption("宁夏", "统一卷(全国甲卷)", "环境单纯，专注力较易保持", 2),
                new ProvinceOption("四川", "统一卷(全国甲卷)", "天府之国，生活气息浓厚", 2),
                new ProvinceOption("贵州", "统一卷(全国甲卷)", "地貌多样，节奏相对平稳", 2),
                new ProvinceOption("云南", "统一卷(全国甲卷)", "地貌多样，节奏相对平稳", 2),
                new ProvinceOption("广西", "统一卷(全国甲卷)", "气候温和，备考环境舒适", 2),
                new ProvinceOption("辽宁", "统一卷(新高考Ⅱ卷)", "工业底蕴深厚，理科思维活跃", 3),
                new ProvinceOption("黑龙江", "统一卷(新高考Ⅱ卷)", "辽阔大气，学习环境安稳", 3),
                new ProvinceOption("重庆", "统一卷(新高考Ⅱ卷)", "山城风貌，教材版本较新", 3),
                new ProvinceOption("广东", "统一卷(新高考Ⅰ卷)", "人口基数大，考试组织成熟", 4),
                new ProvinceOption("山东", "统一卷(新高考Ⅰ卷)", "人口基数大，考试组织成熟", 5),
                new ProvinceOption("河北", "统一卷(新高考Ⅰ卷)", "考试组织规范，积累深厚", 5),
                new ProvinceOption("湖北", "统一卷(新高考Ⅰ卷)", "教育资源集中，学习节奏紧凑", 4),
                new ProvinceOption("湖南", "统一卷(新高考Ⅰ卷)", "文理交流传统，风格兼具灵活", 4),
                new ProvinceOption("福建", "统一卷(新高考Ⅰ卷)", "气候宜人，考试环境较为轻松", 4),
                new ProvinceOption("河南", "统一卷(新高考Ⅰ卷)", "人口基数大，考试组织成熟", 5),
            };

            public static ProvinceOption Find(string provinceName)
            {
                if (string.IsNullOrWhiteSpace(provinceName))
                {
                    return null;
                }

                for (int i = 0; i < HotOptions.Length; i++)
                {
                    if (HotOptions[i].Name == provinceName)
                    {
                        return HotOptions[i];
                    }
                }

                for (int i = 0; i < AllOptions.Length; i++)
                {
                    if (AllOptions[i].Name == provinceName)
                    {
                        return AllOptions[i];
                    }
                }

                return null;
            }
        }

        private sealed class ProvinceGridSizer : UIBehaviour
        {
            protected override void OnRectTransformDimensionsChange()
            {
                base.OnRectTransformDimensionsChange();
                Apply();
            }

            protected override void Start()
            {
                base.Start();
                Apply();
            }

            private void Apply()
            {
                var rect = transform as RectTransform;
                if (rect == null)
                {
                    return;
                }

                var grid = GetComponent<GridLayoutGroup>();
                if (grid == null || grid.constraintCount <= 0)
                {
                    return;
                }

                var totalSpacing = grid.spacing.x * (grid.constraintCount - 1);
                var width = rect.rect.width - grid.padding.left - grid.padding.right - totalSpacing;
                var cellWidth = width / grid.constraintCount;
                if (cellWidth > 0f)
                {
                    grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
                }
            }
        }
    }
}
