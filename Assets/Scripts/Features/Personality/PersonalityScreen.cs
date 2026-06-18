using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using System.Collections.Generic;

namespace GaokaoSimulator.Features.Personality
{
    public class PersonalityScreen : ScreenBase
    {
        private struct PersonalityData
        {
            public string Name;
            public string Emoji;
            public string Tags;
            public string[] Effects;
            public string Quote;
            public string QuoteSource;
            public Color CardColor;
            public Color TextColor;
            public Color SelectedColor;
        }

        private readonly List<PersonalityData> personalities = new List<PersonalityData>
        {
            new PersonalityData
            {
                Name = "学业大牛",
                Emoji = "🎓",
                Tags = "主号手 · 20张卡组",
                Effects = new[] { "智+3", "思+3", "行+1", "纪+3" },
                Quote = "从小就聪明，但妈知道你心里也累。去吧，用你的脑子证明自己。",
                QuoteSource = "妈妈",
                CardColor = new Color32(229, 240, 251, 255),
                TextColor = new Color32(74, 58, 46, 255),
                SelectedColor = new Color32(33, 150, 243, 255)
            },
            new PersonalityData
            {
                Name = "显眼包",
                Emoji = "☀️",
                Tags = "行主号 · 20张卡组",
                Effects = new[] { "行+5", "纪-2" },
                Quote = "天生活跃气，人群焦点。",
                QuoteSource = "班主任",
                CardColor = new Color32(255, 243, 224, 255),
                TextColor = new Color32(74, 58, 46, 255),
                SelectedColor = new Color32(255, 152, 0, 255)
            },
            new PersonalityData
            {
                Name = "段子手",
                Emoji = "😆",
                Tags = "幽默爆表，人际润滑",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "兄弟，你那嘴皮子是真能处，但关键时刻别光耍嘴皮子啊！走起！",
                QuoteSource = "铁哥们",
                CardColor = new Color32(255, 238, 238, 255),
                TextColor = new Color32(74, 58, 46, 255),
                SelectedColor = new Color32(244, 67, 54, 255)
            },
            new PersonalityData
            {
                Name = "文青",
                Emoji = "🎨",
                Tags = "内心细腻，追求精神",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "有灵气的孩子，但生活不只是诗和远方。去吧，记得你还要高考。",
                QuoteSource = "语文老师",
                CardColor = new Color32(243, 243, 255, 255),
                TextColor = new Color32(74, 58, 46, 255),
                SelectedColor = new Color32(156, 39, 176, 255)
            },
            new PersonalityData
            {
                Name = "众人",
                Emoji = "🧑",
                Tags = "社恐但专注，内心戏多",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "",
                QuoteSource = "",
                CardColor = new Color32(255, 250, 240, 255),
                TextColor = new Color32(74, 58, 46, 255),
                SelectedColor = new Color32(96, 125, 139, 255)
            }
        };

        private int selectedIndex = -1;
        private Button confirmBtn;
        private List<GameObject> dropdownPanels;
        private List<Button> cardButtons;
        private List<Image> cardImages;

        private GameObject guideOverlay;
        private Image guideCharacter;
        private Text guideText;
        private Button guideNextBtn;
        private int guideStep;
        private int guideTargetIndex;

        protected override void Initialize()
        {
            BuildLayout();
        }

        protected override void OnScreenOpen()
        {
            if (Core.GameState.Instance != null)
            {
                Core.GameState.Instance.CurrentProgress = Core.GameProgress.Personality;
            }

            if (Core.GameState.Instance != null && !Core.GameState.Instance.HasPlayedTutorial)
            {
                ShowGuide();
            }
        }

        private void BuildLayout()
        {
            var font = BuiltinFont();
            var root = (RectTransform)transform;
            Stretch(root);

            var content = CreateUiObject("Content", root);
            Stretch(content);
            var bgImg = content.gameObject.AddComponent<Image>();
            bgImg.color = new Color32(255, 248, 240, 255);

            var header = CreateUiObject("Header", content);
            header.anchorMin = new Vector2(0f, 0.88f);
            header.anchorMax = new Vector2(1f, 1f);

            var title = CreateText("Title", header, font, 44, FontStyle.Bold, new Color32(74, 58, 46, 255));
            title.alignment = TextAnchor.MiddleCenter;
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.text = "选个性格，开启你的高中人生";

            var cardArea = CreateUiObject("CardArea", content);
            cardArea.anchorMin = new Vector2(0.06f, 0.12f);
            cardArea.anchorMax = new Vector2(0.94f, 0.88f);

            var vLayout = cardArea.gameObject.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 8f;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childAlignment = TextAnchor.MiddleCenter;

            dropdownPanels = new List<GameObject>();
            cardButtons = new List<Button>();
            cardImages = new List<Image>();

            for (int i = 0; i < personalities.Count; i++)
            {
                var p = personalities[i];
                CreatePersonalityCard(cardArea, font, p, i);
            }

            confirmBtn = CreatePrimaryButton("确认选择", content, font, new Color32(158, 158, 158, 255), new Color32(180, 180, 180, 255));
            var btnRect = (RectTransform)confirmBtn.transform;
            btnRect.anchorMin = new Vector2(0.08f, 0.02f);
            btnRect.anchorMax = new Vector2(0.92f, 0.10f);
            var btnLabel = confirmBtn.GetComponentInChildren<Text>();
            if (btnLabel != null) btnLabel.fontSize = 32;
            confirmBtn.onClick.AddListener(OnConfirmSelection);
            confirmBtn.interactable = false;

            BuildGuideOverlay(content, font);
        }

        private void CreatePersonalityCard(Transform parent, Font font, PersonalityData p, int index)
        {
            var cardRoot = CreateUiObject($"CardRoot_{index}", parent);
            var cardRootRect = cardRoot.GetComponent<RectTransform>();
            cardRootRect.sizeDelta = new Vector2(0f, 80f);

            var cardBg = cardRoot.gameObject.AddComponent<Image>();
            cardBg.color = p.CardColor;
            RuntimeArt.ApplyRounded(cardBg);
            cardImages.Add(cardBg);

            var cardBtn = cardRoot.gameObject.AddComponent<Button>();
            cardBtn.onClick.AddListener(() => OnCardClicked(index));
            cardButtons.Add(cardBtn);

            var iconText = CreateText("Icon", cardRoot, font, 48, FontStyle.Normal, p.TextColor);
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.rectTransform.anchorMin = new Vector2(0.02f, 0f);
            iconText.rectTransform.anchorMax = new Vector2(0.12f, 1f);
            iconText.text = p.Emoji;

            var nameText = CreateText("Name", cardRoot, font, 32, FontStyle.Bold, p.TextColor);
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.rectTransform.anchorMin = new Vector2(0.14f, 0.35f);
            nameText.rectTransform.anchorMax = new Vector2(0.40f, 0.95f);
            nameText.text = p.Name;

            var tagsText = CreateText("Tags", cardRoot, font, 22, FontStyle.Normal, new Color32(107, 78, 65, 255));
            tagsText.alignment = TextAnchor.MiddleLeft;
            tagsText.rectTransform.anchorMin = new Vector2(0.14f, 0f);
            tagsText.rectTransform.anchorMax = new Vector2(0.50f, 0.40f);
            tagsText.text = p.Tags;

            var arrowText = CreateText("Arrow", cardRoot, font, 36, FontStyle.Normal, p.TextColor);
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.rectTransform.anchorMin = new Vector2(0.90f, 0f);
            arrowText.rectTransform.anchorMax = new Vector2(0.98f, 1f);
            arrowText.text = "▼";
            arrowText.name = "Arrow_" + index;

            var dropdown = CreateUiObject($"Dropdown_{index}", parent);
            var dropdownRect = dropdown.GetComponent<RectTransform>();
            dropdownRect.sizeDelta = new Vector2(0f, 160f);
            dropdown.gameObject.SetActive(false);
            dropdownPanels.Add(dropdown.gameObject);

            var dropdownBg = dropdown.gameObject.AddComponent<Image>();
            dropdownBg.color = new Color32(255, 255, 255, 245);
            RuntimeArt.ApplyRounded(dropdownBg);

            var effectsArea = CreateUiObject("Effects", dropdown);
            effectsArea.anchorMin = new Vector2(0.04f, 0.45f);
            effectsArea.anchorMax = new Vector2(0.96f, 0.65f);
            var effectsLayout = effectsArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            effectsLayout.spacing = 10f;
            effectsLayout.childControlWidth = true;
            effectsLayout.childControlHeight = true;
            effectsLayout.childForceExpandWidth = false;
            effectsLayout.childForceExpandHeight = false;
            effectsLayout.childAlignment = TextAnchor.MiddleCenter;

            foreach (var eff in p.Effects)
            {
                var effChip = CreateUiObject("Chip", effectsArea);
                var effLayoutEl = effChip.gameObject.AddComponent<LayoutElement>();
                effLayoutEl.preferredWidth = 70f;
                effLayoutEl.preferredHeight = 36f;

                var effBg = effChip.gameObject.AddComponent<Image>();
                bool isPositive = eff.Contains("+");
                effBg.color = isPositive ? new Color32(123, 192, 107, 255) : new Color32(255, 107, 107, 255);
                RuntimeArt.ApplyRounded(effBg);

                var effTxt = CreateText("Txt", effChip, font, 22, FontStyle.Bold, Color.white);
                effTxt.alignment = TextAnchor.MiddleCenter;
                effTxt.rectTransform.anchorMin = Vector2.zero;
                effTxt.rectTransform.anchorMax = Vector2.one;
                effTxt.text = eff;
            }

            if (!string.IsNullOrEmpty(p.Quote))
            {
                var quoteBg = CreateUiObject("QuoteBg", dropdown);
                quoteBg.anchorMin = new Vector2(0.04f, 0.08f);
                quoteBg.anchorMax = new Vector2(0.96f, 0.45f);
                var qBgImg = quoteBg.gameObject.AddComponent<Image>();
                qBgImg.color = new Color(0.95f, 0.95f, 0.98f, 1f);
                RuntimeArt.ApplyRounded(qBgImg);

                var quoteSource = CreateText("Source", quoteBg, font, 24, FontStyle.Bold, new Color32(255, 107, 107, 255));
                quoteSource.alignment = TextAnchor.UpperLeft;
                quoteSource.rectTransform.anchorMin = new Vector2(0.04f, 0.65f);
                quoteSource.rectTransform.anchorMax = new Vector2(0.96f, 0.95f);
                quoteSource.text = $"{p.QuoteSource}：";

                var quoteText = CreateText("Text", quoteBg, font, 22, FontStyle.Normal, new Color32(74, 58, 46, 255));
                quoteText.alignment = TextAnchor.UpperLeft;
                quoteText.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
                quoteText.rectTransform.anchorMax = new Vector2(0.96f, 0.70f);
                quoteText.horizontalOverflow = HorizontalWrapMode.Wrap;
                quoteText.text = p.Quote;
            }
        }

        private void OnCardClicked(int index)
        {
            for (int i = 0; i < dropdownPanels.Count; i++)
            {
                if (i == index)
                {
                    dropdownPanels[i].SetActive(selectedIndex != index);
                }
                else
                {
                    dropdownPanels[i].SetActive(false);
                }

                var arrow = cardButtons[i].transform.Find($"Arrow_{i}")?.GetComponent<Text>();
                if (arrow != null)
                {
                    arrow.text = (i == index && selectedIndex != index) ? "▲" : "▼";
                }
            }

            if (selectedIndex == index)
            {
                selectedIndex = -1;
                cardImages[index].color = personalities[index].CardColor;
                confirmBtn.interactable = false;
                var btnImg = confirmBtn.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color32(158, 158, 158, 255);
                var btnLabel = confirmBtn.GetComponentInChildren<Text>();
                if (btnLabel != null) btnLabel.color = new Color32(180, 180, 180, 255);
            }
            else
            {
                for (int i = 0; i < cardImages.Count; i++)
                {
                    cardImages[i].color = personalities[i].CardColor;
                }
                cardImages[index].color = personalities[index].SelectedColor;
                selectedIndex = index;
                confirmBtn.interactable = true;
                var btnImg = confirmBtn.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color32(33, 150, 243, 255);
                var btnLabel = confirmBtn.GetComponentInChildren<Text>();
                if (btnLabel != null) btnLabel.color = Color.white;
            }
        }

        private void OnConfirmSelection()
        {
            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.PersonalityName = personalities[selectedIndex].Name;
                state.HasPlayedTutorial = true;
                state.SaveGame();
            }
            NavigateTo(ScreenType.Family, true);
        }

        private void BuildGuideOverlay(Transform parent, Font font)
        {
            guideOverlay = CreateUiObject("GuideOverlay", parent).gameObject;
            Stretch((RectTransform)guideOverlay.transform);

            var overlayImg = guideOverlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

            var guideCanvas = guideOverlay.AddComponent<Canvas>();
            guideCanvas.sortingOrder = 200;

            var guidePanel = CreateUiObject("GuidePanel", guideOverlay.transform);
            guidePanel.anchorMin = new Vector2(0.06f, 0.12f);
            guidePanel.anchorMax = new Vector2(0.94f, 0.88f);

            var panelBg = guidePanel.gameObject.AddComponent<Image>();
            panelBg.color = new Color32(255, 255, 255, 255);
            RuntimeArt.ApplyRounded(panelBg);

            var charContainer = CreateUiObject("CharContainer", guidePanel);
            charContainer.anchorMin = new Vector2(0.04f, 0.65f);
            charContainer.anchorMax = new Vector2(0.24f, 0.96f);

            guideCharacter = charContainer.gameObject.AddComponent<Image>();
            var charSprite = Resources.Load<Sprite>("UI/Guide/guide_character");
            if (charSprite != null) guideCharacter.sprite = charSprite;
            guideCharacter.type = Image.Type.Simple;
            guideCharacter.preserveAspect = true;

            guideText = CreateText("GuideText", guidePanel, font, 32, FontStyle.Normal, new Color32(74, 58, 46, 255));
            guideText.alignment = TextAnchor.UpperLeft;
            guideText.rectTransform.anchorMin = new Vector2(0.28f, 0.40f);
            guideText.rectTransform.anchorMax = new Vector2(0.96f, 0.96f);
            guideText.horizontalOverflow = HorizontalWrapMode.Wrap;

            guideNextBtn = CreatePrimaryButton("下一步", guidePanel, font, new Color32(33, 150, 243, 255), Color.white);
            var nextRect = (RectTransform)guideNextBtn.transform;
            nextRect.anchorMin = new Vector2(0.50f, 0.04f);
            nextRect.anchorMax = new Vector2(0.96f, 0.14f);
            var nextLabel = guideNextBtn.GetComponentInChildren<Text>();
            if (nextLabel != null) nextLabel.fontSize = 28;
            guideNextBtn.onClick.AddListener(OnGuideNext);

            guideOverlay.SetActive(false);
        }

        private void ShowGuide()
        {
            guideStep = 0;
            guideTargetIndex = Random.Range(0, personalities.Count);
            guideOverlay.SetActive(true);
            UpdateGuideText();
        }

        private void UpdateGuideText()
        {
            switch (guideStep)
            {
                case 0:
                    guideText.text = $"你好呀！我是你的游戏向导～\n\n在开始高中生活之前，先选一个性格吧！这会影响你整个高中的发展哦～\n\n{personalities[guideTargetIndex].Emoji} {personalities[guideTargetIndex].Name} 看起来很适合你呢，点击看看吧！";
                    guideNextBtn.GetComponentInChildren<Text>().text = "我来试试";
                    break;
                case 1:
                    guideText.text = $"太棒了！你选择了「{personalities[guideTargetIndex].Name}」！\n\n{personalities[guideTargetIndex].Tags}\n\n看看属性变化：" + string.Join(" ", personalities[guideTargetIndex].Effects) + "\n\n准备好了吗？点击确认开始你的高中人生！";
                    guideNextBtn.GetComponentInChildren<Text>().text = "确认选择";
                    break;
            }
        }

        private void OnGuideNext()
        {
            if (guideStep == 0)
            {
                OnCardClicked(guideTargetIndex);
                guideStep = 1;
                UpdateGuideText();
            }
            else
            {
                guideOverlay.SetActive(false);
                OnConfirmSelection();
            }
        }
    }
}