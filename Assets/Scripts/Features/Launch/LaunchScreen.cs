using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            backgroundImage.color = new Color32(255, 244, 250, 255);

            CreateDecorBubble(root, "BubbleTopLeft", new Vector2(110f, -160f), 220f, new Color32(255, 210, 228, 120));
            CreateDecorBubble(root, "BubbleTopRight", new Vector2(-120f, -250f), 300f, new Color32(198, 235, 255, 110), true);
            CreateDecorBubble(root, "BubbleBottom", new Vector2(0f, 180f), 420f, new Color32(227, 219, 255, 90));

            var safePanel = CreateUiObject("SafePanel", root);
            safePanel.anchorMin = new Vector2(0.08f, 0.05f);
            safePanel.anchorMax = new Vector2(0.92f, 0.95f);
            safePanel.offsetMin = Vector2.zero;
            safePanel.offsetMax = Vector2.zero;

            var titleGroup = CreateUiObject("TitleGroup", safePanel);
            titleGroup.anchorMin = new Vector2(0f, 0.63f);
            titleGroup.anchorMax = new Vector2(1f, 0.95f);
            titleGroup.offsetMin = Vector2.zero;
            titleGroup.offsetMax = Vector2.zero;
            titleTransform = titleGroup;

            var titleText = CreateText("MainTitle", titleGroup, font, 84, FontStyle.Bold, new Color32(117, 76, 115, 255));
            Stretch(titleText.rectTransform);
            titleText.alignment = TextAnchor.UpperCenter;
            titleText.text = "我的高考志愿模拟器";

            var subtitleRect = CreateUiObject("Subtitle", titleGroup);
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0.36f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;
            var subtitleText = subtitleRect.gameObject.AddComponent<Text>();
            subtitleText.font = font;
            subtitleText.fontSize = 34;
            subtitleText.fontStyle = FontStyle.Normal;
            subtitleText.alignment = TextAnchor.UpperCenter;
            subtitleText.color = new Color32(130, 103, 136, 255);
            subtitleText.text = "2026 新高考基线版 · 轻松上手的小游戏体验";

            var card = CreateUiObject("ButtonCard", safePanel);
            card.anchorMin = new Vector2(0.04f, 0.18f);
            card.anchorMax = new Vector2(0.96f, 0.62f);
            card.offsetMin = Vector2.zero;
            card.offsetMax = Vector2.zero;
            var cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color32(255, 255, 255, 245);
            var cardShadow = card.gameObject.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0.47f, 0.34f, 0.47f, 0.16f);
            cardShadow.effectDistance = new Vector2(0f, -14f);

            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 54, 54);
            layout.spacing = 26;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            newGameButton = CreateButton("新游戏", card, font, new Color32(255, 171, 197, 255), new Color32(116, 45, 86, 255));
            continueGameButton = CreateButton("继续游戏", card, font, new Color32(191, 225, 255, 255), new Color32(52, 88, 128, 255));
            settingsButton = CreateButton("设置", card, font, new Color32(229, 214, 255, 255), new Color32(90, 67, 121, 255));
            aboutButton = CreateButton("关于", card, font, new Color32(255, 227, 185, 255), new Color32(121, 82, 45, 255));

            var tipRect = CreateUiObject("TipText", safePanel);
            tipRect.anchorMin = new Vector2(0.08f, 0.08f);
            tipRect.anchorMax = new Vector2(0.92f, 0.15f);
            tipRect.offsetMin = Vector2.zero;
            tipRect.offsetMax = Vector2.zero;
            var tipText = tipRect.gameObject.AddComponent<Text>();
            tipText.font = font;
            tipText.fontSize = 28;
            tipText.alignment = TextAnchor.MiddleCenter;
            tipText.color = new Color32(133, 111, 139, 255);
            tipText.text = "首次进入建议从“新游戏”开始体验完整引导流程";

            var versionRect = CreateUiObject("Version", safePanel);
            versionRect.anchorMin = new Vector2(0.25f, 0.0f);
            versionRect.anchorMax = new Vector2(0.75f, 0.06f);
            versionRect.offsetMin = Vector2.zero;
            versionRect.offsetMax = Vector2.zero;
            versionText = versionRect.gameObject.AddComponent<Text>();
            versionText.font = font;
            versionText.fontSize = 26;
            versionText.alignment = TextAnchor.MiddleCenter;
            versionText.color = new Color32(154, 134, 160, 255);
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
            rect.sizeDelta = new Vector2(0f, 150f);

            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;

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

            var labelText = CreateText("Label", rect, font, 46, FontStyle.Bold, textColor);
            Stretch(labelText.rectTransform);
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = label;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 24;
            labelText.resizeTextMaxSize = 46;

            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 150f;

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
            ShowToast("设置功能开发中，后续开放");
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
            ShowToast($"我的高考志愿模拟器\n版本: {Application.version}\n2026新高考基线版");
        }
        
        #endregion
    }
}
