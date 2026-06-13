using UnityEngine;
using UnityEngine.SceneManagement;

namespace GaokaoSimulator.Core
{
    /// <summary>
    /// 应用程序启动引导器
    /// 负责初始化所有核心系统并进入第一个界面
    /// </summary>
    public class AppBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (FindObjectOfType<AppBootstrap>() != null)
            {
                return;
            }

            var bootstrapGo = new GameObject("AppBootstrap");
            bootstrapGo.AddComponent<AppBootstrap>();
        }

        [Header("系统预制体")]
        [SerializeField] private GameObject gameStatePrefab;
        [SerializeField] private GameObject screenRouterPrefab;
        
        [Header("启动配置")]
        [SerializeField] private bool skipToHomeIfHasSave = false;
        
        private void Awake()
        {
            Debug.Log("[AppBootstrap] 开始初始化...");
            InitializeCoreSystems();
        }
        
        private void Start()
        {
            // 延迟一帧确保所有系统就绪
            Invoke(nameof(EnterFirstScreen), 0.1f);
        }
        
        /// <summary>
        /// 初始化核心系统
        /// </summary>
        private void InitializeCoreSystems()
        {
            // 1. 确保GameState单例存在
            if (GameState.Instance == null)
            {
                if (gameStatePrefab != null)
                {
                    Instantiate(gameStatePrefab);
                }
                else
                {
                    // 创建空的GameObject并添加组件
                    var go = new GameObject("GameState");
                    go.AddComponent<GameState>();
                }
            }
            
            // 2. 确保ScreenRouter单例存在
            if (UI.ScreenRouter.Instance == null)
            {
                if (screenRouterPrefab != null)
                {
                    Instantiate(screenRouterPrefab);
                }
                else
                {
                    var go = new GameObject("ScreenRouter");
                    go.AddComponent<UI.ScreenRouter>();
                }
            }
            
            // 3. 确保AudioManager单例存在
            if (AudioManager.Instance == null)
            {
                var audioGo = new GameObject("AudioManager");
                audioGo.AddComponent<AudioManager>();
            }
            
            Debug.Log("[AppBootstrap] 核心系统初始化完成");
        }
        
        /// <summary>
        /// 进入第一个界面
        /// </summary>
        private void EnterFirstScreen()
        {
            if (GameState.Instance == null)
            {
                Debug.LogError("[AppBootstrap] GameState未初始化！");
                return;
            }
            
            // 检查是否有存档且配置允许跳过
            if (skipToHomeIfHasSave && GameState.Instance.HasSaveData)
            {
                // 有存档，直接进入主界面
                UI.ScreenRouter.Instance?.NavigateTo(UI.ScreenType.Home);
            }
            else
            {
                // 新游戏或不允许跳过，进入启动画面
                UI.ScreenRouter.Instance?.NavigateTo(UI.ScreenType.Launch);
            }
            
            Debug.Log($"[AppBootstrap] 进入第一个界面: {(GameState.Instance.HasSaveData && skipToHomeIfHasSave ? "主界面" : "启动画面")}");
        }
        
        /// <summary>
        /// 手动触发重新初始化（调试用）
        /// </summary>
        [ContextMenu("重新初始化")]
        private void Reinitialize()
        {
            InitializeCoreSystems();
            EnterFirstScreen();
        }
    }
}
