using System.Collections.Generic;
using GaokaoSimulator.Core;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.Features.Shop
{
    public class ShopScreen : ScreenBase
    {
        private const float UiTextScale = 1.45f;

        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text moneyText;
        [SerializeField] private RectTransform itemListRoot;

        private readonly List<Button> itemButtons = new List<Button>();
        private readonly List<ShopItem> items = new List<ShopItem>
        {
            new ShopItem { Id = "item_stationery", Name = "文具套装", Price = 100, Effect = "学习能力 +8", StatType = StatType.Intelligence, StatDelta = 8 },
            new ShopItem { Id = "item_psych_book", Name = "心理辅导书", Price = 120, Effect = "情绪管理 +8", StatType = StatType.Psychology, StatDelta = 8 },
            new ShopItem { Id = "item_social_guide", Name = "社交指南", Price = 90, Effect = "人际关系 +6", StatType = StatType.Social, StatDelta = 6 },
            new ShopItem { Id = "item_nutrition", Name = "营养补剂", Price = 150, Effect = "健康状态 +10", StatType = StatType.Health, StatDelta = 10 },
            new ShopItem { Id = "item_exam_sprint", Name = "真题冲刺", Price = 200, Effect = "学习能力 +15", StatType = StatType.Intelligence, StatDelta = 15 },
            new ShopItem { Id = "item_full_tutor", Name = "全科辅导", Price = 300, Effect = "四维各 +5", StatType = StatType.All, StatDelta = 5 },
        };

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var state = GameState.Instance;
            if (state == null) return;

            if (titleText != null) titleText.text = "商店";
            if (moneyText != null) moneyText.text = $"💰 金币余额：{state.Money}";

            UpdateItemButtons();
        }

        private void BindEvents()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);
            }
        }

        private void UpdateItemButtons()
        {
            var state = GameState.Instance;
            if (state == null) return;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (i >= itemButtons.Count) break;

                var button = itemButtons[i];
                if (button == null) continue;

                var isOwned = state.OwnedItems.Contains(item.Id);
                button.interactable = !isOwned;

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = isOwned ? "已购" : "购买";
                }

                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isOwned ? new Color(0.75f, 0.75f, 0.75f) : Color.white;
                }
            }
        }

        private void TryPurchase(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= items.Count)
            {
                return;
            }

            var state = GameState.Instance;
            if (state == null) return;

            var item = items[itemIndex];

            if (state.OwnedItems.Contains(item.Id))
            {
                return;
            }

            if (state.Money < item.Price)
            {
                ShowToast("金币不足！");
                return;
            }

            state.Money -= item.Price;
            state.OwnedItems.Add(item.Id);

            switch (item.StatType)
            {
                case StatType.Intelligence:
                    state.StatIntelligence += item.StatDelta;
                    break;
                case StatType.Psychology:
                    state.StatPsychology += item.StatDelta;
                    break;
                case StatType.Social:
                    state.StatSocial += item.StatDelta;
                    break;
                case StatType.Health:
                    state.StatHealth += item.StatDelta;
                    break;
                case StatType.All:
                    state.StatIntelligence += item.StatDelta;
                    state.StatPsychology += item.StatDelta;
                    state.StatSocial += item.StatDelta;
                    state.StatHealth += item.StatDelta;
                    break;
            }

            state.HasSaveData = true;
            Refresh();
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null && titleText != null && moneyText != null && itemListRoot != null)
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
            bgGradient.SetColors(UITheme.CardButter, UITheme.Bg, UITheme.CardPeach, UITheme.CardSky);

            var panel = CreateUiObject("Panel", transform);
            panel.anchorMin = new Vector2(0.05f, 0.04f);
            panel.anchorMax = new Vector2(0.95f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateUiObject("Header", panel);
            header.anchorMin = new Vector2(0f, 0.86f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            backButton = CreateOutlineButton("← 返回", header, font);
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0f, 0.1f);
            backRect.anchorMax = new Vector2(0.22f, 0.9f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            titleText = CreateText("Title", header, font, 64, FontStyle.Bold, UITheme.Text);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.24f, 0.1f);
            titleText.rectTransform.anchorMax = new Vector2(0.76f, 0.9f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;

            moneyText = CreateText("Money", header, font, 36, FontStyle.Bold, UITheme.Gold);
            moneyText.alignment = TextAnchor.MiddleCenter;
            moneyText.rectTransform.anchorMin = new Vector2(0.6f, 0.1f);
            moneyText.rectTransform.anchorMax = new Vector2(1f, 0.9f);
            moneyText.rectTransform.offsetMin = Vector2.zero;
            moneyText.rectTransform.offsetMax = Vector2.zero;

            var scrollRoot = CreateUiObject("ScrollRoot", panel);
            scrollRoot.anchorMin = new Vector2(0f, 0f);
            scrollRoot.anchorMax = new Vector2(1f, 0.86f);
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = Vector2.zero;

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;

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
            contentLayout.spacing = 18f;
            contentLayout.padding = new RectOffset(12, 12, 12, 12);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = content;
            scrollRect.viewport = viewport;

            itemListRoot = content;

            for (int i = 0; i < items.Count; i++)
            {
                BuildItemCard(itemListRoot, font, i);
            }
        }

        private void BuildItemCard(Transform parent, Font font, int index)
        {
            var item = items[index];

            var card = CreateUiObject($"Item_{item.Id}", parent);
            var cardLayout = card.gameObject.AddComponent<LayoutElement>();
            cardLayout.preferredHeight = 160f;

            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = Color.white;
            card.gameObject.AddComponent<UiAutoRounded>();
            var cardShadow = card.gameObject.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0f, 0f, 0f, 0.06f);
            cardShadow.effectDistance = new Vector2(0f, -6f);

            var nameText = CreateText("Name", card, font, 34, FontStyle.Bold, UITheme.Text);
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.rectTransform.anchorMin = new Vector2(0.04f, 0.62f);
            nameText.rectTransform.anchorMax = new Vector2(0.50f, 0.96f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;
            nameText.text = item.Name;

            var priceText = CreateText("Price", card, font, 28, FontStyle.Bold, UITheme.Gold);
            priceText.alignment = TextAnchor.MiddleLeft;
            priceText.rectTransform.anchorMin = new Vector2(0.04f, 0.28f);
            priceText.rectTransform.anchorMax = new Vector2(0.50f, 0.58f);
            priceText.rectTransform.offsetMin = Vector2.zero;
            priceText.rectTransform.offsetMax = Vector2.zero;
            priceText.text = $"{item.Price} 金币";

            var effectText = CreateText("Effect", card, font, 24, FontStyle.Normal, UITheme.TextSoft);
            effectText.alignment = TextAnchor.MiddleLeft;
            effectText.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
            effectText.rectTransform.anchorMax = new Vector2(0.50f, 0.28f);
            effectText.rectTransform.offsetMin = Vector2.zero;
            effectText.rectTransform.offsetMax = Vector2.zero;
            effectText.text = item.Effect;

            var buyBtn = CreatePrimaryButton("购买", card, font, UITheme.Confirm, Color.white);
            buyBtn.gameObject.AddComponent<UiPressScale>();
            var buyRect = (RectTransform)buyBtn.transform;
            buyRect.anchorMin = new Vector2(0.56f, 0.12f);
            buyRect.anchorMax = new Vector2(0.94f, 0.88f);
            buyRect.offsetMin = Vector2.zero;
            buyRect.offsetMax = Vector2.zero;

            var capturedIndex = index;
            buyBtn.onClick.AddListener(() => TryPurchase(capturedIndex));

            itemButtons.Add(buyBtn);
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

            var text = CreateText("Text", buttonGo.transform, font, 42, FontStyle.Bold, textColor);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            buttonGo.AddComponent<UiPressScale>();
            return buttonGo.GetComponent<Button>();
        }

        private sealed class ShopItem
        {
            public string Id;
            public string Name;
            public int Price;
            public string Effect;
            public StatType StatType;
            public int StatDelta;
        }

        private enum StatType
        {
            Intelligence,
            Psychology,
            Social,
            Health,
            All
        }
    }
}
