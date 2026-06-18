using System;
using System.Collections.Generic;
using GaokaoSimulator.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    /// <summary>
    /// 界面类型枚举
    /// </summary>
    public enum ScreenType
    {
        Launch,         // 启动画面
        Profile,        // 创建人物
        Personality,    // 性格选择
        Family,         // 家庭背景
        Province,       // 选择省份
        Subject,        // 选科
        Home,           // 主界面Hub
        TalentTree,     // 天赋树
        Semester,       // 学期主界面
        SemesterResult, // 学期结算（成绩单）
        Gaokao,         // 高考
        Volunteer,      // 志愿填报
        University,     // 大学
        Career,         // 毕业到30岁
        Summary,        // 人生总结
        PlayerInfo,     // 人物信息
        Shop,           // 商店
        DailyGame,      // 主玩法（流水账+事件）
        DailySettlement // 每日结算
    }
    
    /// <summary>
    /// 界面路由系统
    /// 负责所有界面的切换、导航栈管理、参数传递
    /// 单例模式，全局唯一
    /// </summary>
    public class ScreenRouter : MonoBehaviour
    {
        public static ScreenRouter Instance { get; private set; }
        
        [Header("屏幕根节点")]
        [SerializeField] private Transform screenRoot;
        
        [Header("界面预制体配置")]
        [SerializeField] private List<ScreenConfig> screenConfigs = new List<ScreenConfig>();
        
        [Header("过渡动画")]
        [SerializeField] private float transitionDuration = 0.3f;
        
        // 当前显示的界面
        private ScreenBase currentScreen;
        
        // 导航栈（用于返回）
        private Stack<ScreenType> navigationStack = new Stack<ScreenType>();
        
        // 界面实例缓存
        private Dictionary<ScreenType, ScreenBase> screenInstances = new Dictionary<ScreenType, ScreenBase>();
        
        [System.Serializable]
        public class ScreenConfig
        {
            public ScreenType screenType;
            public ScreenBase prefab;
            public bool cacheInMemory = true; // 是否常驻内存
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureUiInfrastructure();
            
            Debug.Log("[ScreenRouter] 初始化完成");
        }

        private void EnsureUiInfrastructure()
        {
            if (EventSystem.current == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(eventSystemGo);
            }

            if (screenRoot != null)
            {
                return;
            }

            var canvasGo = GameObject.Find("UICanvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("UICanvas");
                DontDestroyOnLoad(canvasGo);

                var rectTransform = canvasGo.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                // 确保 UI 图层存在
                int uiLayer = LayerMask.NameToLayer("UI");
                if (uiLayer == -1) uiLayer = 0;

                canvasGo.layer = uiLayer;

                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.pixelPerfect = false;

                var uiCameraGo = new GameObject("UICamera");
                DontDestroyOnLoad(uiCameraGo);
                var uiCamera = uiCameraGo.AddComponent<Camera>();
                uiCamera.clearFlags = CameraClearFlags.Depth;
                uiCamera.cullingMask = 1 << uiLayer;
                uiCamera.orthographic = true;
                uiCamera.orthographicSize = 13.8f;
                uiCamera.nearClipPlane = 0.1f;
                uiCamera.farClipPlane = 200;
                uiCamera.depth = 10;
                canvas.worldCamera = uiCamera;
                canvas.planeDistance = 100;

                // 相机放在 Canvas 后方，确保 Canvas 平面在视野中心
                uiCameraGo.transform.position = new Vector3(0, 0, -100);

                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
                canvasGo.AddComponent<AdaptiveCanvasController>();
            }

            var safeArea = canvasGo.transform.Find("SafeArea");
            screenRoot = safeArea != null ? safeArea : canvasGo.transform;

            if (screenRoot.GetComponent<RectTransform>() == null)
            {
                screenRoot.gameObject.AddComponent<RectTransform>();
            }
        }
        
        /// <summary>
        /// 导航到指定界面
        /// </summary>
        /// <param name="screenType">目标界面类型</param>
        /// <param name="pushToStack">是否加入导航栈（支持返回）</param>
        /// <param name="parameters">可选参数</param>
        /// <param name="onComplete">切换完成回调</param>
        public void NavigateTo(ScreenType screenType, bool pushToStack = false, 
            Dictionary<string, object> parameters = null, Action onComplete = null)
        {
            // 如果要加入导航栈，把当前界面入栈
            if (pushToStack && currentScreen != null)
            {
                navigationStack.Push(currentScreen.ScreenId);
            }
            
            // 执行界面切换
            StartCoroutine(TransitionToScreen(screenType, parameters, onComplete));
        }
        
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        public void GoBack(Action onComplete = null)
        {
            if (navigationStack.Count == 0)
            {
                Debug.LogWarning("[ScreenRouter] 导航栈为空，无法返回");
                onComplete?.Invoke();
                return;
            }
            
            var previousScreen = navigationStack.Pop();
            NavigateTo(previousScreen, false, null, onComplete);
        }
        
        /// <summary>
        /// 切换到指定界面的协程
        /// </summary>
        private System.Collections.IEnumerator TransitionToScreen(ScreenType screenType, 
            Dictionary<string, object> parameters, Action onComplete)
        {
            // 隐藏当前界面
            if (currentScreen != null)
            {
                yield return StartCoroutine(currentScreen.Hide(transitionDuration));
                currentScreen.gameObject.SetActive(false);
            }
            
            // 获取或创建目标界面
            var targetScreen = GetOrCreateScreen(screenType);
            
            if (targetScreen == null)
            {
                Debug.LogError($"[ScreenRouter] 无法创建界面: {screenType}");
                onComplete?.Invoke();
                yield break;
            }
            
            // 传递参数
            if (parameters != null)
            {
                targetScreen.SetParameters(parameters);
            }
            
            // 显示新界面
            targetScreen.gameObject.SetActive(true);
            currentScreen = targetScreen;
            
            yield return StartCoroutine(targetScreen.Show(transitionDuration));
            
            // 自动切换 BGM
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.GetBgmForScreen(screenType));
            }
            
            onComplete?.Invoke();
            
            Debug.Log($"[ScreenRouter] 切换完成: {screenType}");
        }
        
        /// <summary>
        /// 获取或创建指定界面实例
        /// </summary>
        private ScreenBase GetOrCreateScreen(ScreenType screenType)
        {
            var resourcesPrefab = Resources.Load<ScreenBase>($"UI/Screens/Screen_{screenType}");
            var resourcesGo = Resources.Load<GameObject>($"UI/Screens/Screen_{screenType}");

            bool hasResourcesPrefab = resourcesPrefab != null || resourcesGo != null;

            if (screenInstances.TryGetValue(screenType, out var cachedScreen))
            {
                if (hasResourcesPrefab)
                {
                    Destroy(cachedScreen.gameObject);
                    screenInstances.Remove(screenType);
                }
                else
                {
                    return cachedScreen;
                }
            }

            var config = screenConfigs.Find(c => c.screenType == screenType);

            if (resourcesPrefab != null)
            {
                var instanceFromResources = Instantiate(resourcesPrefab, screenRoot);
                instanceFromResources.name = $"Screen_{screenType}";
                instanceFromResources.ScreenId = screenType;
                screenInstances[screenType] = instanceFromResources;
                return instanceFromResources;
            }

            if (resourcesGo != null)
            {
                var instanceGo = Instantiate(resourcesGo, screenRoot);
                instanceGo.name = $"Screen_{screenType}";

                var screenBase = instanceGo.GetComponent<ScreenBase>();
                if (screenBase == null)
                {
                    screenBase = AddScreenComponent(instanceGo, screenType);
                }

                if (screenBase != null)
                {
                    screenBase.ScreenId = screenType;
                    screenInstances[screenType] = screenBase;
                    return screenBase;
                }
            }

            if (config != null && config.prefab != null)
            {
                var instance = Instantiate(config.prefab, screenRoot);
                instance.name = $"Screen_{screenType}";
                instance.ScreenId = screenType;

                if (config.cacheInMemory)
                {
                    screenInstances[screenType] = instance;
                }

                return instance;
            }

            return CreateRuntimeScreen(screenType);
        }

        private ScreenBase AddScreenComponent(GameObject gameObject, ScreenType screenType)
        {
            ScreenBase instance;
            if (screenType == ScreenType.Launch)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Launch.LaunchScreen>();
            }
            else if (screenType == ScreenType.Profile)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Profile.ProfileScreen>();
            }
            else if (screenType == ScreenType.Personality)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Personality.PersonalityScreen>();
            }
            else if (screenType == ScreenType.Family)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Family.FamilyScreen>();
            }
            else if (screenType == ScreenType.Province)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Province.ProvinceScreen>();
            }
            else if (screenType == ScreenType.Subject)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Subject.SubjectScreen>();
            }
            else if (screenType == ScreenType.Home)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Home.HomeScreen>();
            }
            else if (screenType == ScreenType.Semester)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Semester.SemesterScreen>();
            }
            else if (screenType == ScreenType.SemesterResult)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Semester.SemesterResultScreen>();
            }
            else if (screenType == ScreenType.PlayerInfo)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.PlayerInfo.PlayerInfoScreen>();
            }
            else if (screenType == ScreenType.Shop)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Shop.ShopScreen>();
            }
            else if (screenType == ScreenType.Gaokao)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Gaokao.GaokaoScreen>();
            }
            else if (screenType == ScreenType.Volunteer)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Volunteer.VolunteerScreen>();
            }
            else if (screenType == ScreenType.University)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.University.UniversityScreen>();
            }
            else if (screenType == ScreenType.Career)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Career.CareerScreen>();
            }
            else if (screenType == ScreenType.Summary)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.Summary.SummaryScreen>();
            }
            else if (screenType == ScreenType.DailyGame)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.DailyGame.DailyGameScreen>();
            }
            else if (screenType == ScreenType.DailySettlement)
            {
                instance = gameObject.AddComponent<GaokaoSimulator.Features.DailyGame.DailySettlementScreen>();
            }
            else
            {
                var placeholder = gameObject.AddComponent<RuntimePlaceholderScreen>();
                placeholder.Configure(screenType);
                instance = placeholder;
            }
            
            return instance;
        }

        private ScreenBase CreateRuntimeScreen(ScreenType screenType)
        {
            var screenGo = new GameObject($"Screen_{screenType}");
            var rectTransform = screenGo.AddComponent<RectTransform>();
            rectTransform.SetParent(screenRoot, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            ScreenBase instance;
            if (screenType == ScreenType.Launch)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Launch.LaunchScreen>();
            }
            else if (screenType == ScreenType.Profile)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Profile.ProfileScreen>();
            }
            else if (screenType == ScreenType.Personality)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Personality.PersonalityScreen>();
            }
            else if (screenType == ScreenType.Family)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Family.FamilyScreen>();
            }
            else if (screenType == ScreenType.Province)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Province.ProvinceScreen>();
            }
            else if (screenType == ScreenType.Subject)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Subject.SubjectScreen>();
            }
            else if (screenType == ScreenType.Home)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Home.HomeScreen>();
            }
            else if (screenType == ScreenType.Semester)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Semester.SemesterScreen>();
            }
            else if (screenType == ScreenType.SemesterResult)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Semester.SemesterResultScreen>();
            }
            else if (screenType == ScreenType.PlayerInfo)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.PlayerInfo.PlayerInfoScreen>();
            }
            else if (screenType == ScreenType.Shop)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Shop.ShopScreen>();
            }
            else if (screenType == ScreenType.Gaokao)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Gaokao.GaokaoScreen>();
            }
            else if (screenType == ScreenType.Volunteer)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Volunteer.VolunteerScreen>();
            }
            else if (screenType == ScreenType.University)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.University.UniversityScreen>();
            }
            else if (screenType == ScreenType.Career)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Career.CareerScreen>();
            }
            else if (screenType == ScreenType.Summary)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.Summary.SummaryScreen>();
            }
            else if (screenType == ScreenType.DailyGame)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.DailyGame.DailyGameScreen>();
            }
            else if (screenType == ScreenType.DailySettlement)
            {
                instance = screenGo.AddComponent<GaokaoSimulator.Features.DailyGame.DailySettlementScreen>();
            }
            else
            {
                var placeholder = screenGo.AddComponent<RuntimePlaceholderScreen>();
                placeholder.Configure(screenType);
                instance = placeholder;
            }

            instance.name = $"Screen_{screenType}";
            instance.ScreenId = screenType;
            screenInstances[screenType] = instance;

            Debug.Log($"[ScreenRouter] 运行时创建界面: {screenType}");
            return instance;
        }
        
        /// <summary>
        /// 获取当前界面
        /// </summary>
        public ScreenBase GetCurrentScreen()
        {
            return currentScreen;
        }
        
        /// <summary>
        /// 预加载指定界面（用于优化切换流畅度）
        /// </summary>
        public void PreloadScreen(ScreenType screenType)
        {
            if (!screenInstances.ContainsKey(screenType))
            {
                var screen = GetOrCreateScreen(screenType);
                if (screen != null)
                {
                    screen.gameObject.SetActive(false);
                    Debug.Log($"[ScreenRouter] 预加载完成: {screenType}");
                }
            }
        }
        
        /// <summary>
        /// 清理所有缓存的界面实例
        /// </summary>
        public void ClearCache()
        {
            foreach (var kvp in screenInstances)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            screenInstances.Clear();
            currentScreen = null;
            navigationStack.Clear();
            
            Debug.Log("[ScreenRouter] 缓存已清理");
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
