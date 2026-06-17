using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using System.Collections.Generic;

namespace GaokaoSimulator.Features.Personality
{
    public class PersonalityScreen : ScreenBase
    {
        private const float CardWidth = 580f;
        private const float CardHeight = 520f;

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
        }

        private readonly List<PersonalityData> personalities = new List<PersonalityData>
        {
            new PersonalityData
            {
                Name = "学业大牛",
                Emoji = "🎓",
                Tags = "主号手 · 20张卡组 (忠9/9/减压2)",
                Effects = new[] { "智+3", "思+3", "行+1", "纪+3" },
                Quote = "从小就聪明，但妈知道你心里也累。去吧，用你的脑子证明自己。",
                QuoteSource = "妈妈",
                CardColor = new Color32(229, 240, 251, 255),
                TextColor = new Color32(74, 58, 46, 255)
            },
            new PersonalityData
            {
                Name = "显眼包",
                Emoji = "☀️",
                Tags = "行主号 · 20张卡组 (忠9/9/减压2)",
                Effects = new[] { "行+5", "纪-2" },
                Quote = "天生活跃气，人群焦点。",
                QuoteSource = "班主任",
                CardColor = new Color32(255, 243, 224, 255),
                TextColor = new Color32(74, 58, 46, 255)
            },
            new PersonalityData
            {
                Name = "段子手",
                Emoji = "😆",
                Tags = "幽默忍爆表，人际润滑师。",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "兄弟，你那嘴皮子是真能处，但关键时刻别光耍嘴皮子啊！走起！",
                QuoteSource = "铁哥们",
                CardColor = new Color32(255, 238, 238, 255),
                TextColor = new Color32(74, 58, 46, 255)
            },
            new PersonalityData
            {
                Name = "文青",
                Emoji = "🎨",
                Tags = "内心细腻，追求精神世界。",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "有灵气的孩子，但生活不只是诗和远方。去吧，记得你还要高考。",
                QuoteSource = "语文老师",
                CardColor = new Color32(243, 243, 255, 255),
                TextColor = new Color32(74, 58, 46, 255)
            },
            new PersonalityData
            {
                Name = "众人",
                Emoji = "🧑",
                Tags = "社恐但专注，内心戏多。",
                Effects = new[] { "思+3", "心+3", "行+3", "纪+3" },
                Quote = "",
                QuoteSource = "",
                CardColor = new Color32(255, 250, 240, 255),
                TextColor = new Color32(74, 58, 46, 255)
            }
        };

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
            header.anchorMin = new Vector2(0f, 0.92f);
            header.anchorMax = new Vector2(1f, 1f);

            var title = CreateText("Title", header, font, 48, FontStyle.Bold, new Color32(74, 58, 46, 255));
            title.alignment = TextAnchor.MiddleCenter;
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.text = "选择你的性格";

            var scrollArea = CreateUiObject("ScrollArea", content);
            scrollArea.anchorMin = new Vector2(0f, 0.02f);
            scrollArea.anchorMax = new Vector2(1f, 0.92f);

            var scrollRect = scrollArea.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1f;

            var viewport = CreateUiObject("Viewport", scrollArea);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var viewportMask = viewport.gameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var contentRect = CreateUiObject("Cards", viewport);
            contentRect.anchorMin = new Vector2(0f, 0.05f);
            contentRect.anchorMax = new Vector2(0f, 0.95f);
            contentRect.pivot = new Vector2(0f, 0.5f);
            scrollRect.content = contentRect.GetComponent<RectTransform>();

            var hLayout = contentRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20f;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;
            hLayout.padding = new RectOffset(30, 30, 0, 0);

            float totalWidth = personalities.Count * (CardWidth + hLayout.spacing) + hLayout.padding.left + hLayout.padding.right;
            contentRect.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, 0f);

            for (int i = 0; i < personalities.Count; i++)
            {
                var p = personalities[i];
                CreatePersonalityCard(contentRect, font, p, i);
            }
        }

        private void CreatePersonalityCard(Transform parent, Font font, PersonalityData p, int index)
        {
            var card = CreateUiObject($"Card_{index}", parent);
            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(CardWidth, CardHeight);

            var bg = card.gameObject.AddComponent<Image>();
            bg.color = p.CardColor;
            RuntimeArt.ApplyRounded(bg);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.08f);
            shadow.effectDistance = new Vector2(0f, -4f);

            var btn = card.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => OnPersonalitySelected(index));

            var iconText = CreateText("Icon", card, font, 100, FontStyle.Normal, p.TextColor);
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.rectTransform.anchorMin = new Vector2(0.08f, 0.78f);
            iconText.rectTransform.anchorMax = new Vector2(0.92f, 0.98f);
            iconText.text = p.Emoji;

            var nameText = CreateText("Name", card, font, 46, FontStyle.Bold, p.TextColor);
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.rectTransform.anchorMin = new Vector2(0.04f, 0.65f);
            nameText.rectTransform.anchorMax = new Vector2(0.96f, 0.78f);
            nameText.text = p.Name;

            var tagsText = CreateText("Tags", card, font, 26, FontStyle.Normal, new Color32(107, 78, 65, 255));
            tagsText.alignment = TextAnchor.MiddleCenter;
            tagsText.rectTransform.anchorMin = new Vector2(0.04f, 0.56f);
            tagsText.rectTransform.anchorMax = new Vector2(0.96f, 0.65f);
            tagsText.text = p.Tags;

            var effectsRow = CreateUiObject("Effects", card);
            effectsRow.anchorMin = new Vector2(0.04f, 0.42f);
            effectsRow.anchorMax = new Vector2(0.96f, 0.56f);

            var effectsLayout = effectsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            effectsLayout.spacing = 8f;
            effectsLayout.childControlWidth = true;
            effectsLayout.childControlHeight = true;
            effectsLayout.childForceExpandWidth = false;
            effectsLayout.childForceExpandHeight = false;
            effectsLayout.childAlignment = TextAnchor.MiddleCenter;

            foreach (var eff in p.Effects)
            {
                var effChip = CreateUiObject("Chip", effectsRow);
                var effLayoutEl = effChip.gameObject.AddComponent<LayoutElement>();
                effLayoutEl.preferredWidth = 80f;
                effLayoutEl.preferredHeight = 40f;

                var effBg = effChip.gameObject.AddComponent<Image>();
                bool isPositive = eff.Contains("+");
                effBg.color = isPositive ? new Color32(123, 192, 107, 255) : new Color32(255, 107, 107, 255);

                var effTxt = CreateText("Txt", effChip, font, 24, FontStyle.Bold, Color.white);
                effTxt.alignment = TextAnchor.MiddleCenter;
                effTxt.rectTransform.anchorMin = Vector2.zero;
                effTxt.rectTransform.anchorMax = Vector2.one;
                effTxt.text = eff;
            }

            if (!string.IsNullOrEmpty(p.Quote))
            {
                var quoteBg = CreateUiObject("QuoteBg", card);
                quoteBg.anchorMin = new Vector2(0.04f, 0.04f);
                quoteBg.anchorMax = new Vector2(0.96f, 0.42f);

                var qBgImg = quoteBg.gameObject.AddComponent<Image>();
                qBgImg.color = new Color(1f, 1f, 1f, 0.65f);
                RuntimeArt.ApplyRounded(qBgImg);

                var quoteSource = CreateText("Source", quoteBg, font, 28, FontStyle.Bold, new Color32(255, 107, 107, 255));
                quoteSource.alignment = TextAnchor.UpperLeft;
                quoteSource.rectTransform.anchorMin = new Vector2(0.03f, 0.70f);
                quoteSource.rectTransform.anchorMax = new Vector2(0.97f, 0.97f);
                quoteSource.text = $"{p.QuoteSource}：";

                var quoteText = CreateText("Text", quoteBg, font, 26, FontStyle.Normal, p.TextColor);
                quoteText.alignment = TextAnchor.UpperLeft;
                quoteText.rectTransform.anchorMin = new Vector2(0.03f, 0.03f);
                quoteText.rectTransform.anchorMax = new Vector2(0.97f, 0.70f);
                quoteText.horizontalOverflow = HorizontalWrapMode.Wrap;
                quoteText.text = p.Quote;
            }
        }

        private void OnPersonalitySelected(int index)
        {
            var state = Core.GameState.Instance;
            if (state != null)
            {
                state.PersonalityName = personalities[index].Name;
                state.SaveGame();
            }
            NavigateTo(ScreenType.Family, true);
        }
    }
}
