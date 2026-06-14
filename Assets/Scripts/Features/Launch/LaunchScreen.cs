using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;

namespace GaokaoSimulator.Features.Launch
{
    /// <summary>
    /// 启动画面
    /// 游戏入口界面，提供新游戏、继续游戏等选项
    /// </summary>
    public class LaunchScreen : UI.ScreenBase
    {
        private const float UiTextScale = 1.25f;

        [Header("UI引用")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button logoutButton;
        
        [Header("标题动画")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private float titleAnimationDuration = 1f;
        [SerializeField] private AnimationCurve titleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("版本信息")]
        [SerializeField] private Text versionText;
        
        // 动画协程引用
        private Coroutine titleAnimationCoroutine;
        
        #region ScreenBase实现
        
        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            ScreenFlowHint.Clear(transform.Find("SafePanel") ?? transform);

            // 绑定按钮事件
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }
            
            if (continueGameButton != null)
            {
                continueGameButton.onClick.AddListener(OnContinueGameClicked);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            if (aboutButton != null)
            {
                aboutButton.onClick.AddListener(OnAboutClicked);
            }
            
            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(OnLogoutClicked);
            }
            
            // 设置版本号
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
            
            Debug.Log("[LaunchScreen] 初始化完成");
        }
        
        protected override void OnScreenOpen()
        {
            // 播放标题动画
            if (titleTransform != null)
            {
                titleAnimationCoroutine = StartCoroutine(AnimateTitle());
            }
            
            // 检查是否有存档，控制继续游戏按钮状态
            UpdateContinueButtonState();

        }
        
        protected override void OnScreenClose()
        {
            // 停止动画
            if (titleAnimationCoroutine != null)
            {
                StopCoroutine(titleAnimationCoroutine);
                titleAnimationCoroutine = null;
            }
        }
        
        public override void Refresh()
        {
            // 刷新界面显示
            UpdateContinueButtonState();
        }
        
        public override void OnScreenResize()
        {
            // 屏幕尺寸变化时的处理
            // 可以在这里重新布局
        }
        
        #endregion

        #region 运行时布局

        private void EnsureRuntimeLayout()
        {
            if (newGameButton != null && continueGameButton != null && titleTransform != null && versionText != null && logoutButton != null)
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
            var backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = Color.white;
            var bgSprite = RuntimeArt.LoadSprite("UI/Launch/bg_launch_full");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.type = Image.Type.Simple;
                backgroundImage.preserveAspect = true;
            }
            else
            {
                var bgGradient = background.gameObject.AddComponent<UiVerticalGradient>();
                bgGradient.SetColors(new Color32(255, 248, 233, 255), new Color32(255, 234, 246, 255));
            }

            CreateDecorBubble(root, "BubbleTopLeft", new Vector2(110f, -160f), 220f, new Color32(255, 210, 228, 120));
            CreateDecorBubble(root, "BubbleTopRight", new Vector2(-120f, -250f), 300f, new Color32(198, 235, 255, 110), true);
            CreateDecorBubble(root, "BubbleBottom", new Vector2(0f, 180f), 420f, new Color32(227, 219, 255, 90));

            var safePanel = CreateUiObject("SafePanel", root);
            safePanel.anchorMin = new Vector2(0.08f, 0.05f);
            safePanel.anchorMax = new Vector2(0.92f, 0.95f);
            safePanel.offsetMin = Vector2.zero;
            safePanel.offsetMax = Vector2.zero;

            var topBadge = CreateUiObject("TopBadge", safePanel);
            topBadge.anchorMin = new Vector2(0.28f, 0.92f);
            topBadge.anchorMax = new Vector2(0.72f, 0.97f);
            topBadge.offsetMin = Vector2.zero;
            topBadge.offsetMax = Vector2.zero;
            var topBadgeImage = topBadge.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(topBadgeImage);
            topBadgeImage.color = new Color32(255, 255, 255, 220);
            var topBadgeText = CreateText("TopBadgeText", topBadge, font, 28, FontStyle.Bold, new Color32(153, 129, 147, 255));
            Stretch(topBadgeText.rectTransform);
            topBadgeText.alignment = TextAnchor.MiddleCenter;
            topBadgeText.text = "2026 可试玩版";

            var titleGroup = CreateUiObject("TitleGroup", safePanel);
            titleGroup.anchorMin = new Vector2(0f, 0.70f);
            titleGroup.anchorMax = new Vector2(1f, 0.90f);
            titleGroup.offsetMin = Vector2.zero;
            titleGroup.offsetMax = Vector2.zero;
            titleTransform = titleGroup;

            var eyebrowText = CreateText("Eyebrow", titleGroup, font, 26, FontStyle.Bold, new Color32(167, 144, 160, 255));
            eyebrowText.text = "从高中到人生选择，这一次重新开始";
            eyebrowText.alignment = TextAnchor.MiddleCenter;
            eyebrowText.rectTransform.anchorMin = new Vector2(0.06f, 0.70f);
            eyebrowText.rectTransform.anchorMax = new Vector2(0.94f, 0.98f);
            eyebrowText.rectTransform.offsetMin = Vector2.zero;
            eyebrowText.rectTransform.offsetMax = Vector2.zero;

            var titleText = CreateText("MainTitle", titleGroup, font, 92, FontStyle.Bold, new Color32(248, 133, 142, 255));
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.rectTransform.anchorMin = new Vector2(0.04f, 0.22f);
            titleText.rectTransform.anchorMax = new Vector2(0.96f, 0.80f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;
            titleText.text = "重启我的\n高中人生";
            var titleShadow = titleText.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(1f, 1f, 1f, 0.85f);
            titleShadow.effectDistance = new Vector2(0f, 6f);

            var subtitleText = CreateText("SubTitle", titleGroup, font, 28, FontStyle.Normal, new Color32(132, 112, 133, 255));
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.rectTransform.anchorMin = new Vector2(0.08f, 0.00f);
            subtitleText.rectTransform.anchorMax = new Vector2(0.92f, 0.30f);
            subtitleText.rectTransform.offsetMin = Vector2.zero;
            subtitleText.rectTransform.offsetMax = Vector2.zero;
            subtitleText.text = "先从高中重新开始，试试这次能走向哪里";

            var heroCard = CreateUiObject("HeroCard", safePanel);
            heroCard.anchorMin = new Vector2(0.06f, 0.36f);
            heroCard.anchorMax = new Vector2(0.94f, 0.67f);
            heroCard.offsetMin = Vector2.zero;
            heroCard.offsetMax = Vector2.zero;
            var heroCardImage = heroCard.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(heroCardImage);
            heroCardImage.color = new Color32(255, 255, 255, 238);
            var heroCardShadow = heroCard.gameObject.AddComponent<Shadow>();
            heroCardShadow.effectColor = new Color(0.43f, 0.31f, 0.42f, 0.14f);
            heroCardShadow.effectDistance = new Vector2(0f, -14f);

            var hero = CreateUiObject("Hero", heroCard);
            hero.anchorMin = new Vector2(0.18f, 0.18f);
            hero.anchorMax = new Vector2(0.82f, 0.92f);
            hero.offsetMin = Vector2.zero;
            hero.offsetMax = Vector2.zero;
            var heroSprite = RuntimeArt.LoadSprite("UI/Launch/hero_chibi_macaron");
            if (heroSprite != null)
            {
                var heroImage = hero.gameObject.AddComponent<Image>();
                heroImage.sprite = heroSprite;
                heroImage.type = Image.Type.Simple;
                heroImage.preserveAspect = true;
                heroImage.color = Color.white;
            }
            else
            {
                CreateHeroPlaceholder(hero, font);
            }
            hero.gameObject.AddComponent<UiFloatBob>().Configure(7f, 0.42f, 0f);

            CreateFeatureChip(heroCard, font, "选科", new Vector2(0.10f, 0.10f), new Vector2(0.30f, 0.26f), new Color32(255, 231, 190, 255), new Color32(159, 113, 64, 255));
            CreateFeatureChip(heroCard, font, "志愿", new Vector2(0.40f, 0.10f), new Vector2(0.60f, 0.26f), new Color32(216, 240, 255, 255), new Color32(77, 113, 153, 255));
            CreateFeatureChip(heroCard, font, "大学", new Vector2(0.70f, 0.10f), new Vector2(0.90f, 0.26f), new Color32(255, 226, 236, 255), new Color32(153, 91, 118, 255));

            var heroHint = CreateText("HeroHint", heroCard, font, 26, FontStyle.Bold, new Color32(141, 119, 138, 255));
            heroHint.alignment = TextAnchor.MiddleCenter;
            heroHint.rectTransform.anchorMin = new Vector2(0.10f, 0.00f);
            heroHint.rectTransform.anchorMax = new Vector2(0.90f, 0.12f);
            heroHint.rectTransform.offsetMin = Vector2.zero;
            heroHint.rectTransform.offsetMax = Vector2.zero;
            heroHint.text = "一屏一屏试玩，先把人生主流程跑通";

            newGameButton = CreateButton("重启我的高中人生", safePanel, font, Color.white, Color.white);
            var newGameRect = (RectTransform)newGameButton.transform;
            newGameRect.anchorMin = new Vector2(0.12f, 0.16f);
            newGameRect.anchorMax = new Vector2(0.88f, 0.26f);
            newGameRect.offsetMin = Vector2.zero;
            newGameRect.offsetMax = Vector2.zero;
            StylePrimaryButton(newGameButton, new Color32(141, 206, 255, 255), new Color32(92, 162, 255, 255));
            newGameButton.gameObject.AddComponent<UiPressScale>();

            continueGameButton = CreateButton("返回校园", safePanel, font, Color.white, new Color32(255, 104, 126, 255));
            var continueRect = (RectTransform)continueGameButton.transform;
            continueRect.anchorMin = new Vector2(0.12f, 0.16f);
            continueRect.anchorMax = new Vector2(0.88f, 0.26f);
            continueRect.offsetMin = Vector2.zero;
            continueRect.offsetMax = Vector2.zero;
            StylePrimaryButton(continueGameButton, new Color32(255, 140, 160, 255), new Color32(255, 104, 126, 255));
            continueGameButton.gameObject.AddComponent<UiPressScale>();

            aboutButton = CreateIconButton("?", safePanel, font);
            var aboutRect = (RectTransform)aboutButton.transform;
            aboutRect.anchorMin = new Vector2(0.02f, 0.08f);
            aboutRect.anchorMax = new Vector2(0.10f, 0.14f);
            aboutRect.offsetMin = Vector2.zero;
            aboutRect.offsetMax = Vector2.zero;
            aboutButton.gameObject.AddComponent<UiPressScale>();

            settingsButton = CreateIconButton("⚙", safePanel, font);
            var settingsRect = (RectTransform)settingsButton.transform;
            settingsRect.anchorMin = new Vector2(0.90f, 0.08f);
            settingsRect.anchorMax = new Vector2(0.98f, 0.14f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;
            settingsButton.gameObject.AddComponent<UiPressScale>();

            logoutButton = CreateIconButton("↺", safePanel, font);
            var logoutRect = (RectTransform)logoutButton.transform;
            logoutRect.anchorMin = new Vector2(0.12f, 0.08f);
            logoutRect.anchorMax = new Vector2(0.20f, 0.14f);
            logoutRect.offsetMin = Vector2.zero;
            logoutRect.offsetMax = Vector2.zero;
            logoutButton.gameObject.AddComponent<UiPressScale>();

            var tipRect = CreateUiObject("TipText", safePanel);
            tipRect.anchorMin = new Vector2(0.08f, 0.30f);
            tipRect.anchorMax = new Vector2(0.92f, 0.35f);
            tipRect.offsetMin = Vector2.zero;
            tipRect.offsetMax = Vector2.zero;
            var tipText = tipRect.gameObject.AddComponent<Text>();
            tipText.font = font;
            tipText.fontSize = 34;
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.color = new Color32(133, 111, 139, 255);
            tipText.text = "第一次来没关系，我们会陪你一步一步熟悉规则";

            var versionRect = CreateUiObject("Version", safePanel);
            versionRect.anchorMin = new Vector2(0.25f, 0.0f);
            versionRect.anchorMax = new Vector2(0.75f, 0.06f);
            versionRect.offsetMin = Vector2.zero;
            versionRect.offsetMax = Vector2.zero;
            versionText = versionRect.gameObject.AddComponent<Text>();
            versionText.font = font;
            versionText.fontSize = 30;
            versionText.alignment = TextAnchor.MiddleCenter;
            versionText.color = new Color32(154, 134, 160, 255);
        }

        private static void StylePrimaryButton(Button button, Color top, Color bottom)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                RuntimeArt.ApplyRounded(image);
                image.color = Color.white;
                var g = image.gameObject.AddComponent<UiVerticalGradient>();
                g.SetColors(top, bottom);
            }

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = Color.white;
                label.fontStyle = FontStyle.Bold;
            }
        }

        private static void StyleOutlineButton(Button button, Color outlineColor)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                RuntimeArt.ApplyRounded(image);
                image.color = new Color32(255, 255, 255, 242);
                var outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = outlineColor;
                outline.effectDistance = new Vector2(4f, -4f);
            }

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = outlineColor;
                label.fontStyle = FontStyle.Bold;
            }
        }

        private static Button CreateIconButton(string label, Transform parent, Font font)
        {
            var button = CreateButton(label, parent, font, Color.white, new Color32(120, 120, 120, 255));
            var rect = (RectTransform)button.transform;
            rect.sizeDelta = Vector2.zero;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                RuntimeArt.ApplyRounded(image);
                image.color = new Color32(255, 255, 255, 240);
                var outline = image.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(206, 233, 255, 255);
                outline.effectDistance = new Vector2(3f, -3f);
            }

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.resizeTextForBestFit = false;
                text.fontSize = 40;
            }

            return button;
        }

        private static void CreateFeatureChip(Transform parent, Font font, string label, Vector2 min, Vector2 max, Color bgColor, Color textColor)
        {
            var chip = CreateUiObject($"Chip_{label}", parent);
            chip.anchorMin = min;
            chip.anchorMax = max;
            chip.offsetMin = Vector2.zero;
            chip.offsetMax = Vector2.zero;
            var chipImage = chip.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(chipImage);
            chipImage.color = bgColor;
            var chipText = CreateText("ChipText", chip, font, 28, FontStyle.Bold, textColor);
            Stretch(chipText.rectTransform);
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.text = label;
        }

        private static void CreateDecorBubble(RectTransform parent, string name, Vector2 anchoredPosition, float size, Color color, bool anchorRight = false)
        {
            var bubble = CreateUiObject(name, parent);
            bubble.anchorMin = anchorRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            bubble.anchorMax = bubble.anchorMin;
            bubble.pivot = new Vector2(0.5f, 0.5f);
            bubble.anchoredPosition = anchoredPosition;
            bubble.sizeDelta = new Vector2(size, size);
            var image = bubble.gameObject.AddComponent<Image>();
            image.color = color;
        }

        private static void CreateHeroPlaceholder(RectTransform parent, Font font)
        {
            var halo = CreateUiObject("HeroHalo", parent);
            halo.anchorMin = new Vector2(0.18f, 0.06f);
            halo.anchorMax = new Vector2(0.82f, 0.78f);
            halo.offsetMin = Vector2.zero;
            halo.offsetMax = Vector2.zero;
            var haloImage = halo.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(haloImage);
            haloImage.color = new Color32(255, 240, 222, 255);
            var haloGradient = halo.gameObject.AddComponent<UiVerticalGradient>();
            haloGradient.SetColors(new Color32(255, 246, 228, 255), new Color32(255, 229, 240, 255));

            var stickerA = CreateUiObject("StickerA", parent);
            stickerA.anchorMin = new Vector2(0.02f, 0.66f);
            stickerA.anchorMax = new Vector2(0.22f, 0.84f);
            stickerA.offsetMin = Vector2.zero;
            stickerA.offsetMax = Vector2.zero;
            var stickerAImage = stickerA.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(stickerAImage);
            stickerAImage.color = new Color32(255, 235, 193, 255);
            var stickerAText = CreateText("StickerAText", stickerA, font, 24, FontStyle.Bold, new Color32(155, 108, 61, 255));
            Stretch(stickerAText.rectTransform);
            stickerAText.alignment = TextAnchor.MiddleCenter;
            stickerAText.text = "选科";

            var stickerB = CreateUiObject("StickerB", parent);
            stickerB.anchorMin = new Vector2(0.78f, 0.22f);
            stickerB.anchorMax = new Vector2(0.98f, 0.40f);
            stickerB.offsetMin = Vector2.zero;
            stickerB.offsetMax = Vector2.zero;
            var stickerBImage = stickerB.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(stickerBImage);
            stickerBImage.color = new Color32(221, 239, 255, 255);
            var stickerBText = CreateText("StickerBText", stickerB, font, 24, FontStyle.Bold, new Color32(82, 118, 153, 255));
            Stretch(stickerBText.rectTransform);
            stickerBText.alignment = TextAnchor.MiddleCenter;
            stickerBText.text = "志愿";

            var books = CreateUiObject("Books", parent);
            books.anchorMin = new Vector2(0.24f, 0.10f);
            books.anchorMax = new Vector2(0.76f, 0.34f);
            books.offsetMin = Vector2.zero;
            books.offsetMax = Vector2.zero;

            for (int i = 0; i < 4; i++)
            {
                var book = CreateUiObject($"Book_{i}", books);
                book.anchorMin = new Vector2(0.10f + i * 0.04f, 0.06f + i * 0.16f);
                book.anchorMax = new Vector2(0.90f - i * 0.04f, 0.26f + i * 0.16f);
                book.offsetMin = Vector2.zero;
                book.offsetMax = Vector2.zero;
                var bookImage = book.gameObject.AddComponent<Image>();
                RuntimeArt.ApplyRounded(bookImage);
                bookImage.color = i switch
                {
                    0 => new Color32(255, 219, 188, 255),
                    1 => new Color32(255, 205, 220, 255),
                    2 => new Color32(196, 231, 255, 255),
                    _ => new Color32(219, 239, 197, 255),
                };
                var outline = book.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(255, 255, 255, 150);
                outline.effectDistance = new Vector2(2f, -2f);
            }

            var mascot = CreateUiObject("Mascot", parent);
            mascot.anchorMin = new Vector2(0.28f, 0.18f);
            mascot.anchorMax = new Vector2(0.72f, 0.84f);
            mascot.offsetMin = Vector2.zero;
            mascot.offsetMax = Vector2.zero;

            var body = CreateUiObject("Body", mascot);
            body.anchorMin = new Vector2(0.32f, 0.02f);
            body.anchorMax = new Vector2(0.68f, 0.48f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;
            var bodyImage = body.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(bodyImage);
            bodyImage.color = new Color32(255, 251, 252, 255);

            var uniform = CreateUiObject("Uniform", body);
            uniform.anchorMin = new Vector2(0.16f, 0.34f);
            uniform.anchorMax = new Vector2(0.84f, 0.78f);
            uniform.offsetMin = Vector2.zero;
            uniform.offsetMax = Vector2.zero;
            var uniformImage = uniform.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(uniformImage);
            uniformImage.color = new Color32(173, 214, 255, 255);

            var head = CreateUiObject("Head", mascot);
            head.anchorMin = new Vector2(0.24f, 0.44f);
            head.anchorMax = new Vector2(0.76f, 0.86f);
            head.offsetMin = Vector2.zero;
            head.offsetMax = Vector2.zero;
            var headImage = head.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(headImage);
            headImage.color = new Color32(255, 233, 214, 255);

            var hair = CreateUiObject("Hair", head);
            hair.anchorMin = new Vector2(0.02f, 0.54f);
            hair.anchorMax = new Vector2(0.96f, 0.98f);
            hair.offsetMin = Vector2.zero;
            hair.offsetMax = Vector2.zero;
            var hairImage = hair.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(hairImage);
            hairImage.color = new Color32(106, 76, 69, 255);

            var face = CreateText("Face", head, font, 38, FontStyle.Bold, new Color32(120, 93, 90, 255));
            Stretch(face.rectTransform);
            face.alignment = TextAnchor.MiddleCenter;
            face.text = "·  ·\n  u";

            var badge = CreateUiObject("HeroBadge", parent);
            badge.anchorMin = new Vector2(0.24f, 0.80f);
            badge.anchorMax = new Vector2(0.76f, 0.96f);
            badge.offsetMin = Vector2.zero;
            badge.offsetMax = Vector2.zero;
            var badgeImage = badge.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(badgeImage);
            badgeImage.color = new Color32(255, 255, 255, 226);
            var badgeText = CreateText("BadgeText", badge, font, 30, FontStyle.Bold, new Color32(136, 112, 144, 255));
            Stretch(badgeText.rectTransform);
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.text = "软萌临时主视觉";
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
            text.fontSize = Mathf.RoundToInt(size * UiTextScale);
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.lineSpacing = 1.1f;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string label, Transform parent, Font font, Color backgroundColor, Color textColor)
        {
            var rect = CreateUiObject($"Button_{label}", parent);
            rect.sizeDelta = new Vector2(0f, 168f);

            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            RuntimeArt.ApplyRounded(image);

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.03f;
            colors.pressedColor = backgroundColor * 0.9f;
            colors.selectedColor = backgroundColor;
            colors.disabledColor = new Color32(220, 220, 220, 255);
            button.colors = colors;

            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.35f, 0.25f, 0.32f, 0.14f);
            shadow.effectDistance = new Vector2(0f, -7f);

            var labelText = CreateText("Label", rect, font, 54, FontStyle.Bold, textColor);
            Stretch(labelText.rectTransform);
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = label;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 28;
            labelText.resizeTextMaxSize = 54;

            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 168f;

            return button;
        }

        #endregion
        
        #region 按钮事件处理
        
        /// <summary>
        /// 新游戏按钮点击
        /// </summary>
        private void OnNewGameClicked()
        {
            Debug.Log("[LaunchScreen] 点击新游戏");
            
            // 显示确认对话框（如果有存档）
            if (Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData)
            {
                // 有存档，提示会覆盖
                ShowNewGameConfirmDialog();
            }
            else
            {
                // 直接开始新游戏
                StartNewGame();
            }
        }
        
        /// <summary>
        /// 继续游戏按钮点击
        /// </summary>
        private void OnContinueGameClicked()
        {
            Debug.Log("[LaunchScreen] 点击继续游戏");
            ContinueGame();
        }
        
        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[LaunchScreen] 点击设置");
            // 设置界面暂未实现，先给玩家明确反馈，避免跳转到空界面
            ShowToast("偏好设置还在整理中，之后会开放给你");
        }
        
        /// <summary>
        /// 关于按钮点击
        /// </summary>
        private void OnAboutClicked()
        {
            Debug.Log("[LaunchScreen] 点击关于");
            // 显示关于信息弹窗
            ShowAboutDialog();
        }

        /// <summary>
        /// 注销按钮点击
        /// </summary>
        private void OnLogoutClicked()
        {
            Debug.Log("[LaunchScreen] 点击注销");

            if (Core.GameState.Instance != null)
            {
                Core.GameState.Instance.ResetState();
                Core.GameState.Instance.HasSaveData = false;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            UpdateContinueButtonState();
            ShowToast("已清除存档，可以重新开始");
        }
        
        #endregion
        
        #region 游戏流程控制
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        private void StartNewGame()
        {
            // 重置游戏状态
            Core.GameState.Instance?.ResetState();
            
            // 进入创建人物界面
            NavigateTo(UI.ScreenType.Profile, true);
        }
        
        /// <summary>
        /// 继续游戏
        /// </summary>
        private void ContinueGame()
        {
            if (Core.GameState.Instance == null) return;
            
            // 根据当前进度进入对应界面
            var progress = Core.GameState.Instance.CurrentProgress;
            
            switch (progress)
            {
                case Core.GameProgress.Profile:
                    NavigateTo(UI.ScreenType.Profile, false);
                    break;
                case Core.GameProgress.Family:
                    NavigateTo(UI.ScreenType.Family, false);
                    break;
                case Core.GameProgress.Province:
                    NavigateTo(UI.ScreenType.Province, false);
                    break;
                case Core.GameProgress.Subject:
                    NavigateTo(UI.ScreenType.Subject, false);
                    break;
                default:
                    // 已完成新手流程，进入主界面
                    NavigateTo(UI.ScreenType.Home, false);
                    break;
            }
        }
        
        #endregion
        
        #region UI更新
        
        /// <summary>
        /// 更新继续游戏按钮状态
        /// </summary>
        private void UpdateContinueButtonState()
        {
            if (newGameButton == null || continueGameButton == null) return;
            
            bool hasSave = Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData;
            newGameButton.gameObject.SetActive(!hasSave);
            continueGameButton.gameObject.SetActive(hasSave);
        }
        
        /// <summary>
        /// 标题动画
        /// </summary>
        private IEnumerator AnimateTitle()
        {
            if (titleTransform == null) yield break;
            
            // 初始状态：稍微缩小且透明
            Vector3 originalScale = titleTransform.localScale;
            titleTransform.localScale = originalScale * 0.8f;
            
            CanvasGroup titleCanvasGroup = titleTransform.GetComponent<CanvasGroup>();
            if (titleCanvasGroup == null)
            {
                titleCanvasGroup = titleTransform.gameObject.AddComponent<CanvasGroup>();
            }
            titleCanvasGroup.alpha = 0;
            
            float elapsed = 0;
            while (elapsed < titleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = titleAnimationCurve.Evaluate(Mathf.Clamp01(elapsed / titleAnimationDuration));
                
                // 缩放动画：0.8 -> 1.0
                float scale = Mathf.Lerp(0.8f, 1f, t);
                titleTransform.localScale = originalScale * scale;
                
                // 透明度动画：0 -> 1
                titleCanvasGroup.alpha = t;
                
                yield return null;
            }
            
            // 确保最终状态正确
            titleTransform.localScale = originalScale;
            titleCanvasGroup.alpha = 1;
        }
        
        #endregion
        
        #region 弹窗对话框
        
        /// <summary>
        /// 显示新游戏确认对话框
        /// </summary>
        private void ShowNewGameConfirmDialog()
        {
            // 这里简化处理，实际应该有专门的弹窗系统
            Debug.Log("[LaunchScreen] 显示新游戏确认对话框");
            
            // 模拟用户确认后直接开始新游戏
            // 实际应该等待用户点击确认按钮
            StartNewGame();
        }
        
        /// <summary>
        /// 显示关于对话框
        /// </summary>
        private void ShowAboutDialog()
        {
            Debug.Log("[LaunchScreen] 显示关于对话框");
            // 显示游戏信息、版本号、版权信息等
            ShowToast($"我的高考志愿模拟器\n版本：{Application.version}\n这是一段从高考到人生选择的模拟旅程");
        }
        
        #endregion
    }
}
