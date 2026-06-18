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
    /// 统一"开启人生"按钮，有存档则继续，无存档则新游戏
    /// </summary>
    public class LaunchScreen : UI.ScreenBase
    {
        [Header("主按钮")]
        [SerializeField] private Button startButton;

        [Header("旧版按钮（兼容）")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueGameButton;

        [Header("辅助按钮")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button logoutButton;

        [Header("用户协议")]
        [SerializeField] private Toggle agreeToggle;

        [Header("标题动画")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private float titleAnimationDuration = 1f;
        [SerializeField] private AnimationCurve titleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("版本信息")]
        [SerializeField] private Text versionText;

        private Coroutine titleAnimationCoroutine;
        private Coroutine backgroundAnimationCoroutine;
        private Coroutine buttonFadeCoroutine;
        private Coroutine introSequenceCoroutine;

        #region ScreenBase实现

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            ScreenFlowHint.Clear(transform.Find("SafePanel") ?? transform);

            // 自动接线：从预制体的 kuang/duigou 创建 Toggle
            if (agreeToggle == null)
            {
                agreeToggle = AutoWireAgreeToggle();
            }

            // 自动接线：BtnNewGame 作为统一 startButton
            if (startButton == null && newGameButton != null)
            {
                startButton = newGameButton;
            }

            // 自动接线：左边创建注销按钮
            if (logoutButton == null)
            {
                logoutButton = CreateLogoutButtonRuntime();
            }

            // 统一按钮模式：隐藏 BtnContinue
            if (startButton != null && continueGameButton != null)
            {
                continueGameButton.gameObject.SetActive(false);
            }

            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
            else
            {
                if (newGameButton != null)
                    newGameButton.onClick.AddListener(OnNewGameClicked);
                if (continueGameButton != null)
                    continueGameButton.onClick.AddListener(OnContinueGameClicked);
            }

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (aboutButton != null)
                aboutButton.onClick.AddListener(OnAboutClicked);

            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            if (versionText != null)
                versionText.text = $"v{Application.version}";

            // 提前设置背景初始缩放，避免第一帧跳变
            var bg = transform.Find("Background");
            if (bg != null)
            {
                bg.localScale = Vector3.one * 1.2f;
            }

            // 创建白色遮罩，覆盖整个界面
            CreateWhiteMask();

            // 提前隐藏内容，避免在白色遮罩出现前闪现
            if (titleTransform != null)
            {
                var titleCg = titleTransform.GetComponent<CanvasGroup>();
                if (titleCg == null) titleCg = titleTransform.gameObject.AddComponent<CanvasGroup>();
                titleCg.alpha = 0f;
            }

            if (startButton != null)
            {
                var btnCg = startButton.GetComponent<CanvasGroup>();
                if (btnCg == null) btnCg = startButton.gameObject.AddComponent<CanvasGroup>();
                btnCg.alpha = 0f;
            }

            Debug.Log("[LaunchScreen] 初始化完成");
        }

        /// <summary>
        /// 重写 Show：跳过默认渐显动画，直接显示。
        /// 白色遮罩在 Initialize() 已创建并覆盖全部内容，IntroSequence 负责渐消。
        /// </summary>
        public override System.Collections.IEnumerator Show(float duration)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            // 直接显示，不渐显。白遮罩会覆盖一切
            canvasGroup.alpha = 1f;

            // 等一帧确保布局完成
            yield return null;

            OnScreenOpen();
        }

        protected override void OnScreenOpen()
        {
            // 所有动画由开场序列统一控制
            introSequenceCoroutine = StartCoroutine(IntroSequence());

            UpdateButtonState();
        }

        protected override void OnScreenClose()
        {
            if (introSequenceCoroutine != null)
            {
                StopCoroutine(introSequenceCoroutine);
                introSequenceCoroutine = null;
            }

            if (titleAnimationCoroutine != null)
            {
                StopCoroutine(titleAnimationCoroutine);
                titleAnimationCoroutine = null;
            }

            if (backgroundAnimationCoroutine != null)
            {
                StopCoroutine(backgroundAnimationCoroutine);
                backgroundAnimationCoroutine = null;
            }

            if (buttonFadeCoroutine != null)
            {
                StopCoroutine(buttonFadeCoroutine);
                buttonFadeCoroutine = null;
            }
        }

        public override void Refresh()
        {
            UpdateButtonState();
        }

        public override void OnScreenResize() { }

        #endregion

        #region 运行时布局

        private void EnsureRuntimeLayout()
        {
            bool hasMainButton = startButton != null || (newGameButton != null && continueGameButton != null);
            if (hasMainButton && titleTransform != null && versionText != null)
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

            startButton = CreateButton("开启人生", safePanel, font, Color.white, Color.white);
            var startRect = (RectTransform)startButton.transform;
            startRect.anchorMin = new Vector2(0.12f, 0.16f);
            startRect.anchorMax = new Vector2(0.88f, 0.26f);
            startRect.offsetMin = Vector2.zero;
            startRect.offsetMax = Vector2.zero;
            StylePrimaryButton(startButton, new Color32(141, 206, 255, 255), new Color32(92, 162, 255, 255));
            startButton.gameObject.AddComponent<UiPressScale>();

            logoutButton = CreateSmallButton("注销", safePanel, font,
                new Color32(255, 255, 255, 200), new Color32(180, 180, 180, 255));
            var logoutRect = (RectTransform)logoutButton.transform;
            logoutRect.anchorMin = new Vector2(0.02f, 0.02f);
            logoutRect.anchorMax = new Vector2(0.12f, 0.08f);
            logoutRect.offsetMin = Vector2.zero;
            logoutRect.offsetMax = Vector2.zero;
            logoutButton.gameObject.AddComponent<UiPressScale>();

            var agreeGroup = CreateUiObject("AgreeGroup", safePanel);
            agreeGroup.anchorMin = new Vector2(0.12f, 0.06f);
            agreeGroup.anchorMax = new Vector2(0.88f, 0.12f);
            agreeGroup.offsetMin = Vector2.zero;
            agreeGroup.offsetMax = Vector2.zero;
            var agreeLayout = agreeGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            agreeLayout.spacing = 8f;
            agreeLayout.childAlignment = TextAnchor.MiddleLeft;

            var toggleGo = new GameObject("AgreeToggle", typeof(RectTransform));
            toggleGo.transform.SetParent(agreeGroup, false);
            var toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(40f, 40f);
            agreeToggle = toggleGo.AddComponent<Toggle>();
            agreeToggle.isOn = false;

            var toggleBg = toggleGo.AddComponent<Image>();
            toggleBg.color = Color.white;
            RuntimeArt.ApplyRounded(toggleBg);
            var toggleOutline = toggleGo.AddComponent<Outline>();
            toggleOutline.effectColor = new Color32(206, 233, 255, 255);
            toggleOutline.effectDistance = new Vector2(2f, -2f);
            agreeToggle.targetGraphic = toggleBg;

            var toggleCheck = CreateUiObject("Checkmark", toggleGo.transform);
            toggleCheck.anchorMin = new Vector2(0.15f, 0.15f);
            toggleCheck.anchorMax = new Vector2(0.85f, 0.85f);
            toggleCheck.offsetMin = Vector2.zero;
            toggleCheck.offsetMax = Vector2.zero;
            var toggleCheckImg = toggleCheck.gameObject.AddComponent<Image>();
            toggleCheckImg.color = new Color32(33, 150, 243, 255);
            RuntimeArt.ApplyRounded(toggleCheckImg);
            agreeToggle.graphic = toggleCheckImg;

            var agreeText = CreateText("AgreeText", agreeGroup, font, 28, FontStyle.Normal, new Color32(133, 111, 139, 255));
            agreeText.alignment = TextAnchor.MiddleLeft;
            agreeText.text = "我已阅读并同意《用户协议》";

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

        #endregion

        #region 按钮事件处理

        /// <summary>
        /// 统一"开启人生"按钮 —— 有存档就继续，没存档就新游戏
        /// </summary>
        private void OnStartClicked()
        {
            Debug.Log("[LaunchScreen] 点击开启人生");

            if (agreeToggle != null && !agreeToggle.isOn)
            {
                ShowToastPopup("请先勾选用户协议");
                return;
            }

            bool hasSave = Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData;
            if (hasSave)
            {
                ContinueGame();
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[LaunchScreen] 点击新游戏");

            if (agreeToggle != null && !agreeToggle.isOn)
            {
                ShowToastPopup("请先勾选用户协议");
                return;
            }

            if (Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData)
            {
                ShowNewGameConfirmDialog();
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnContinueGameClicked()
        {
            Debug.Log("[LaunchScreen] 点击继续游戏");
            ContinueGame();
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[LaunchScreen] 点击设置");
            ShowToastPopup("偏好设置还在整理中，之后会开放给你");
        }

        private void OnAboutClicked()
        {
            Debug.Log("[LaunchScreen] 点击关于");
            ShowAboutDialog();
        }

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
            ShowToastPopup("已清除存档，可以重新开始");
        }

        #endregion

        #region 游戏流程控制

        private void StartNewGame()
        {
            Core.GameState.Instance?.ResetState();
            NavigateTo(UI.ScreenType.Profile, true);
        }

        private void ContinueGame()
        {
            if (Core.GameState.Instance == null) return;

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
                    NavigateTo(UI.ScreenType.Home, false);
                    break;
            }
        }

        #endregion

        #region UI更新

        private void UpdateButtonState()
        {
            // 统一按钮模式下不需要切换显隐
            if (startButton != null)
                return;

            if (newGameButton == null || continueGameButton == null)
                return;

            bool hasSave = Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData;
            newGameButton.gameObject.SetActive(!hasSave);
            continueGameButton.gameObject.SetActive(hasSave);
        }

        /// <summary>
        /// 创建全屏白色遮罩
        /// </summary>
        private void CreateWhiteMask()
        {
            var maskGo = new GameObject("WhiteMask", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var maskRect = maskGo.GetComponent<RectTransform>();
            maskRect.SetParent(transform, false);
            Stretch(maskRect);
            maskRect.SetAsLastSibling();

            var maskImg = maskGo.GetComponent<Image>();
            maskImg.color = Color.white;
            maskImg.raycastTarget = true; // 遮罩期间拦截点击

            var maskCg = maskGo.GetComponent<CanvasGroup>();
            maskCg.alpha = 1f;
            maskCg.blocksRaycasts = true;
        }

        /// <summary>
        /// 开场序列：白色遮罩渐消 → 标题动画 + 背景缩放 + 按钮渐显
        /// </summary>
        private IEnumerator IntroSequence()
        {
            Debug.Log("[LaunchScreen] IntroSequence 开始");

            // 初始状态：标题和按钮先隐藏
            if (titleTransform != null)
            {
                var titleCg = titleTransform.GetComponent<CanvasGroup>();
                if (titleCg == null) titleCg = titleTransform.gameObject.AddComponent<CanvasGroup>();
                titleCg.alpha = 0f;
            }

            if (startButton != null)
            {
                var btnCg = startButton.GetComponent<CanvasGroup>();
                if (btnCg == null) btnCg = startButton.gameObject.AddComponent<CanvasGroup>();
                btnCg.alpha = 0f;
            }

            // 背景缩放动画立即启动（不等待遮罩，遮罩覆盖在上面所以看不到）
            backgroundAnimationCoroutine = StartCoroutine(AnimateBackground());
            Debug.Log("[LaunchScreen] 背景缩放动画已启动");

            // 阶段1：白色遮罩停留0.5秒，然后2秒渐消
            var mask = transform.Find("WhiteMask");
            Debug.Log($"[LaunchScreen] WhiteMask: {(mask != null ? "找到" : "未找到")}");
            if (mask != null)
            {
                yield return new WaitForSeconds(0.5f);

                var maskCg = mask.GetComponent<CanvasGroup>();
                float maskFadeDuration = 2f;
                float maskElapsed = 0f;
                while (maskElapsed < maskFadeDuration)
                {
                    maskElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(maskElapsed / maskFadeDuration);
                    t = 1f - (1f - t) * (1f - t); // ease out
                    maskCg.alpha = 1f - t;
                    yield return null;
                }
                maskCg.alpha = 0f;
                maskCg.blocksRaycasts = false;
                Debug.Log("[LaunchScreen] 白色遮罩渐消完成");
            }

            // 阶段2：遮罩消失后，启动标题和按钮动画
            if (titleTransform != null)
            {
                titleAnimationCoroutine = StartCoroutine(AnimateTitle());
            }

            if (startButton != null)
            {
                buttonFadeCoroutine = StartCoroutine(FadeInButton(startButton));
            }

            Debug.Log("[LaunchScreen] IntroSequence 完成");
        }

        private IEnumerator AnimateTitle()
        {
            if (titleTransform == null) yield break;

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

                float scale = Mathf.Lerp(0.8f, 1f, t);
                titleTransform.localScale = originalScale * scale;

                titleCanvasGroup.alpha = t;

                yield return null;
            }

            titleTransform.localScale = originalScale;
            titleCanvasGroup.alpha = 1;
        }

        private IEnumerator AnimateBackground()
        {
            var bg = transform.Find("Background");
            if (bg == null)
            {
                Debug.LogWarning("[LaunchScreen] AnimateBackground: Background 未找到！");
                yield break;
            }

            Debug.Log($"[LaunchScreen] AnimateBackground 开始, 初始scale={bg.localScale}");

            float duration = 10f; // 10秒
            Vector3 targetScale = Vector3.one;
            Vector3 startScale = targetScale * 1.2f;

            bg.localScale = startScale;

            float elapsed = 0;
            float lastLogTime = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 线性过渡，匀速变化，让用户能感知到
                bg.localScale = Vector3.Lerp(startScale, targetScale, t);

                // 每秒打印一次日志，确认动画在跑
                if (elapsed - lastLogTime >= 1f)
                {
                    lastLogTime = elapsed;
                    Debug.Log($"[LaunchScreen] 背景缩放: t={t:F4}, scale={bg.localScale.x:F4}, elapsed={elapsed:F1}s");
                }

                yield return null;
            }

            bg.localScale = targetScale;
            Debug.Log("[LaunchScreen] AnimateBackground 完成");
        }

        private IEnumerator FadeInButton(Button button)
        {
            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;

            float fadeDuration = 1.5f;
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                // ease out
                t = 1f - (1f - t) * (1f - t);
                canvasGroup.alpha = t;
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        #endregion

        #region 弹窗对话框

        private void ShowNewGameConfirmDialog()
        {
            Debug.Log("[LaunchScreen] 显示新游戏确认对话框");
            StartNewGame();
        }

        private void ShowAboutDialog()
        {
            Debug.Log("[LaunchScreen] 显示关于对话框");
            ShowToastPopup($"我的高考志愿模拟器\n版本：{Application.version}\n这是一段从高考到人生选择的模拟旅程");
        }

        /// <summary>
        /// 在屏幕中央显示一个临时提示弹窗，1.5秒后自动消失
        /// </summary>
        private void ShowToastPopup(string message)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 半透明遮罩
            var overlay = CreateUiObject("ToastOverlay", transform);
            Stretch(overlay);
            overlay.SetAsLastSibling();
            var overlayBg = overlay.gameObject.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.3f);
            overlayBg.raycastTarget = false;

            // 提示框
            var card = CreateUiObject("ToastCard", overlay);
            card.anchorMin = new Vector2(0.15f, 0.4f);
            card.anchorMax = new Vector2(0.85f, 0.6f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardBg = card.gameObject.AddComponent<Image>();
            cardBg.color = new Color32(40, 40, 40, 235);
            RuntimeArt.ApplyRounded(cardBg);

            var text = CreateText("ToastText", card, font, 36, FontStyle.Normal, Color.white);
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin = new Vector2(0.08f, 0.1f);
            text.rectTransform.anchorMax = new Vector2(0.92f, 0.9f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = message;

            var canvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = false;

            StartCoroutine(FadeToast(canvasGroup));
        }

        private IEnumerator FadeToast(CanvasGroup canvasGroup)
        {
            yield return new WaitForSeconds(1.5f);

            float elapsed = 0;
            float fadeDuration = 0.3f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            if (canvasGroup != null)
                Destroy(canvasGroup.gameObject);
        }

        #endregion

        #region 自动接线

        /// <summary>
        /// 从预制体的 kuang/duigou 子对象自动创建 Toggle 组件
        /// </summary>
        private Toggle AutoWireAgreeToggle()
        {
            var agreeParent = transform.Find("用户协议");
            if (agreeParent == null) return null;

            var kuang = agreeParent.Find("kuang");
            var duigou = agreeParent.Find("duigou");

            var toggle = agreeParent.gameObject.AddComponent<Toggle>();
            toggle.isOn = false;

            if (kuang != null)
            {
                var kuangImg = kuang.GetComponent<Image>();
                if (kuangImg != null) toggle.targetGraphic = kuangImg;
            }

            if (duigou != null)
            {
                var duigouImg = duigou.GetComponent<Image>();
                if (duigouImg != null) toggle.graphic = duigouImg;
            }

            Debug.Log("[LaunchScreen] 自动接线用户协议 Toggle");
            return toggle;
        }

        /// <summary>
        /// 运行时在左下角创建注销按钮
        /// </summary>
        private Button CreateLogoutButtonRuntime()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var button = CreateSmallButton("注销", transform, font,
                new Color32(255, 255, 255, 200), new Color32(180, 180, 180, 255));

            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(0.02f, 0.02f);
            rect.anchorMax = new Vector2(0.10f, 0.06f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            button.gameObject.AddComponent<UiPressScale>();

            Debug.Log("[LaunchScreen] 自动创建注销按钮");
            return button;
        }

        #endregion
    }
}