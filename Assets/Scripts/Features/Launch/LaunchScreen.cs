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
        [Header("UI引用")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button aboutButton;
        
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
            if (newGameButton != null && continueGameButton != null && titleTransform != null && versionText != null)
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
            var bgGradient = background.gameObject.AddComponent<UiVerticalGradient>();
            bgGradient.SetColors(new Color32(255, 248, 233, 255), new Color32(255, 234, 246, 255));

            CreateDecorBubble(root, "BubbleTopLeft", new Vector2(110f, -160f), 220f, new Color32(255, 210, 228, 120));
            CreateDecorBubble(root, "BubbleTopRight", new Vector2(-120f, -250f), 300f, new Color32(198, 235, 255, 110), true);
            CreateDecorBubble(root, "BubbleBottom", new Vector2(0f, 180f), 420f, new Color32(227, 219, 255, 90));

            var safePanel = CreateUiObject("SafePanel", root);
            safePanel.anchorMin = new Vector2(0.08f, 0.05f);
            safePanel.anchorMax = new Vector2(0.92f, 0.95f);
            safePanel.offsetMin = Vector2.zero;
            safePanel.offsetMax = Vector2.zero;

            var titleGroup = CreateUiObject("TitleGroup", safePanel);
            titleGroup.anchorMin = new Vector2(0f, 0.70f);
            titleGroup.anchorMax = new Vector2(1f, 0.95f);
            titleGroup.offsetMin = Vector2.zero;
            titleGroup.offsetMax = Vector2.zero;
            titleTransform = titleGroup;

            var titleBubble = CreateUiObject("TitleBubble", titleGroup);
            titleBubble.anchorMin = new Vector2(0.06f, 0.18f);
            titleBubble.anchorMax = new Vector2(0.94f, 0.96f);
            titleBubble.offsetMin = Vector2.zero;
            titleBubble.offsetMax = Vector2.zero;
            var titleBubbleImage = titleBubble.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(titleBubbleImage);
            titleBubbleImage.color = new Color32(255, 255, 255, 242);
            var titleBubbleOutline = titleBubble.gameObject.AddComponent<Outline>();
            titleBubbleOutline.effectColor = new Color32(255, 207, 223, 255);
            titleBubbleOutline.effectDistance = new Vector2(5f, -5f);

            var titleText = CreateText("MainTitle", titleBubble, font, 96, FontStyle.Bold, new Color32(252, 132, 141, 255));
            Stretch(titleText.rectTransform);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "我的高考\n志愿模拟器";
            var titleShadow = titleText.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0.25f, 0.2f, 0.25f, 0.25f);
            titleShadow.effectDistance = new Vector2(0f, -6f);

            var hero = CreateUiObject("Hero", safePanel);
            hero.anchorMin = new Vector2(0.08f, 0.34f);
            hero.anchorMax = new Vector2(0.92f, 0.70f);
            hero.offsetMin = Vector2.zero;
            hero.offsetMax = Vector2.zero;
            var heroImage = hero.gameObject.AddComponent<Image>();
            heroImage.color = Color.white;
            heroImage.preserveAspect = true;
            var heroSprite = RuntimeArt.LoadSprite("UI/Launch/hero_boy_books");
            if (heroSprite != null)
            {
                heroImage.sprite = heroSprite;
            }
            else
            {
                heroImage.enabled = false;
            }

            newGameButton = CreateButton("开始新游戏", safePanel, font, Color.white, Color.white);
            var newGameRect = (RectTransform)newGameButton.transform;
            newGameRect.anchorMin = new Vector2(0.12f, 0.16f);
            newGameRect.anchorMax = new Vector2(0.88f, 0.26f);
            newGameRect.offsetMin = Vector2.zero;
            newGameRect.offsetMax = Vector2.zero;
            StylePrimaryButton(newGameButton, new Color32(141, 206, 255, 255), new Color32(92, 162, 255, 255));

            continueGameButton = CreateButton("继续", safePanel, font, Color.white, new Color32(255, 104, 126, 255));
            var continueRect = (RectTransform)continueGameButton.transform;
            continueRect.anchorMin = new Vector2(0.32f, 0.125f);
            continueRect.anchorMax = new Vector2(0.68f, 0.165f);
            continueRect.offsetMin = Vector2.zero;
            continueRect.offsetMax = Vector2.zero;
            StyleOutlineButton(continueGameButton, new Color32(255, 104, 126, 255));

            aboutButton = CreateIconButton("?", safePanel, font);
            var aboutRect = (RectTransform)aboutButton.transform;
            aboutRect.anchorMin = new Vector2(0.02f, 0.06f);
            aboutRect.anchorMax = new Vector2(0.10f, 0.12f);
            aboutRect.offsetMin = Vector2.zero;
            aboutRect.offsetMax = Vector2.zero;

            settingsButton = CreateIconButton("⚙", safePanel, font);
            var settingsRect = (RectTransform)settingsButton.transform;
            settingsRect.anchorMin = new Vector2(0.90f, 0.06f);
            settingsRect.anchorMax = new Vector2(0.98f, 0.12f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;

            var tipRect = CreateUiObject("TipText", safePanel);
            tipRect.anchorMin = new Vector2(0.08f, 0.27f);
            tipRect.anchorMax = new Vector2(0.92f, 0.34f);
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
                text.fontSize = 58;
            }

            return button;
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
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
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
            if (continueGameButton == null) return;
            
            bool hasSave = Core.GameState.Instance != null && Core.GameState.Instance.HasSaveData;
            continueGameButton.interactable = hasSave;
            
            // 可以在这里改变按钮的视觉状态（如变灰）
            var buttonImage = continueGameButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = hasSave ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            }
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
