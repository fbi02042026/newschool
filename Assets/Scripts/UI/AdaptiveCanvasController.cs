using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    /// <summary>
    /// 自适应画布控制器
    /// 处理1242x2760基准分辨率的自适应布局
    /// 适配各种刘海屏、全面屏、长屏设备
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasController : MonoBehaviour
    {
        [Header("基准分辨率")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1242, 2760);
        
        [Header("适配模式")]
        [SerializeField] private CanvasScaler.ScreenMatchMode matchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] private float matchWidthOrHeight = 0.5f; // 0=宽适配, 1=高适配, 0.5=均衡
        
        [Header("安全区域")]
        [SerializeField] private bool useSafeArea = true;
        [SerializeField] private RectTransform safeAreaContainer; // 安全区域容器
        
        [Header("刘海屏适配")]
        [SerializeField] private float notchTopPadding = 80f;    // 顶部刘海预留
        [SerializeField] private float notchBottomPadding = 40f; // 底部手势条预留
        
        // 组件引用
        private Canvas canvas;
        private CanvasScaler canvasScaler;
        
        // 当前设备信息
        private Vector2 currentResolution;
        private float currentAspectRatio;
        private bool hasNotch = false;
        
        private void Awake()
        {
            InitializeComponents();
            ApplySettings();
        }
        
        private void Start()
        {
            ApplySafeArea();
        }
        
        private void Update()
        {
            // 检测分辨率变化（窗口模式或旋转）
            if (Screen.width != (int)currentResolution.x || Screen.height != (int)currentResolution.y)
            {
                OnResolutionChanged();
            }
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            canvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            
            if (safeAreaContainer == null)
            {
                // 尝试查找名为"SafeArea"的子物体
                var safeAreaObj = transform.Find("SafeArea");
                if (safeAreaObj != null)
                {
                    safeAreaContainer = safeAreaObj.GetComponent<RectTransform>();
                }
                else
                {
                    // 如果没有找到，创建一个
                    var go = new GameObject("SafeArea");
                    go.transform.SetParent(transform, false);
                    safeAreaContainer = go.AddComponent<RectTransform>();
                    
                    // 设置为全屏
                    safeAreaContainer.anchorMin = Vector2.zero;
                    safeAreaContainer.anchorMax = Vector2.one;
                    safeAreaContainer.offsetMin = Vector2.zero;
                    safeAreaContainer.offsetMax = Vector2.zero;
                }
            }
            
            // 设置Canvas为最高层级
            canvas.sortingOrder = 0;
        }
        
        /// <summary>
        /// 应用基础设置
        /// </summary>
        private void ApplySettings()
        {
            // 配置CanvasScaler
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.screenMatchMode = matchMode;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            
            // 记录当前分辨率
            currentResolution = new Vector2(Screen.width, Screen.height);
            currentAspectRatio = (float)Screen.width / Screen.height;
            
            // 检测是否有刘海（宽高比大于标准全面屏）
            hasNotch = currentAspectRatio > 2.1f; // 19.5:9 约等于 2.17
            
            Debug.Log($"[AdaptiveCanvas] 应用设置完成 | 分辨率: {currentResolution} | 宽高比: {currentAspectRatio:F2} | 刘海屏: {hasNotch}");
        }
        
        /// <summary>
        /// 应用安全区域
        /// </summary>
        private void ApplySafeArea()
        {
            if (!useSafeArea || safeAreaContainer == null)
            {
                return;
            }
            
            // 计算安全区域（考虑刘海和手势条）
            float topPadding = hasNotch ? notchTopPadding : 0;
            float bottomPadding = hasNotch ? notchBottomPadding : 0;
            
            // 转换为锚点坐标（0-1范围）
            float topAnchor = 1 - (topPadding / referenceResolution.y);
            float bottomAnchor = bottomPadding / referenceResolution.y;
            
            // 应用安全区域
            safeAreaContainer.anchorMin = new Vector2(0, bottomAnchor);
            safeAreaContainer.anchorMax = new Vector2(1, topAnchor);
            safeAreaContainer.offsetMin = Vector2.zero;
            safeAreaContainer.offsetMax = Vector2.zero;
            
            Debug.Log($"[AdaptiveCanvas] 应用安全区域 | 上内边距: {topPadding}px | 下内边距: {bottomPadding}px");
        }
        
        /// <summary>
        /// 分辨率变化时处理
        /// </summary>
        private void OnResolutionChanged()
        {
            currentResolution = new Vector2(Screen.width, Screen.height);
            currentAspectRatio = (float)Screen.width / Screen.height;
            hasNotch = currentAspectRatio > 2.1f;
            
            ApplySettings();
            ApplySafeArea();
            
            // 通知当前显示的界面刷新
            if (ScreenRouter.Instance?.GetCurrentScreen() != null)
            {
                ScreenRouter.Instance.GetCurrentScreen().OnScreenResize();
            }
            
            Debug.Log($"[AdaptiveCanvas] 分辨率变化 | 新分辨率: {currentResolution}");
        }
        
        /// <summary>
        /// 手动刷新安全区域（用于设备旋转后）
        /// </summary>
        public void RefreshSafeArea()
        {
            ApplySafeArea();
        }
        
        /// <summary>
        /// 获取安全区域容器
        /// </summary>
        public RectTransform GetSafeAreaContainer()
        {
            return safeAreaContainer;
        }
        
        /// <summary>
        /// 获取当前设备信息
        /// </summary>
        public DeviceInfo GetDeviceInfo()
        {
            return new DeviceInfo
            {
                Resolution = currentResolution,
                AspectRatio = currentAspectRatio,
                HasNotch = hasNotch,
                ReferenceResolution = referenceResolution,
                SafeAreaTop = hasNotch ? notchTopPadding : 0,
                SafeAreaBottom = hasNotch ? notchBottomPadding : 0
            };
        }
        
        /// <summary>
        /// 设备信息结构
        /// </summary>
        public struct DeviceInfo
        {
            public Vector2 Resolution;
            public float AspectRatio;
            public bool HasNotch;
            public Vector2 ReferenceResolution;
            public float SafeAreaTop;
            public float SafeAreaBottom;
            
            public override string ToString()
            {
                return $"分辨率: {Resolution}, 宽高比: {AspectRatio:F2}, 刘海屏: {HasNotch}";
            }
        }
    }
}
