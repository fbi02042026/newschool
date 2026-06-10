using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GaokaoSimulator.UI
{
    /// <summary>
    /// 界面基类
    /// 所有具体界面都需要继承此类
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenBase : MonoBehaviour
    {
        [Header("界面配置")]
        [SerializeField] protected bool allowBackNavigation = true; // 是否允许返回
        [SerializeField] protected bool pauseGameWhenOpen = false;   // 打开时是否暂停游戏
        
        [Header("动画配置")]
        [SerializeField] protected float fadeInDuration = 0.3f;
        [SerializeField] protected float fadeOutDuration = 0.2f;
        [SerializeField] protected AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // 当前界面ID（由ScreenRouter在创建时设置）
        public ScreenType ScreenId { get; set; }
        
        // CanvasGroup组件
        protected CanvasGroup CanvasGroup { get; private set; }
        
        // 参数传递
        protected Dictionary<string, object> parameters;
        
        // 是否已初始化
        private bool isInitialized = false;
        
        #region Unity生命周期
        
        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            if (CanvasGroup == null)
            {
                CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 初始状态为隐藏
            CanvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
        
        protected virtual void OnEnable()
        {
            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }
            
            OnScreenOpen();
        }
        
        protected virtual void OnDisable()
        {
            OnScreenClose();
        }
        
        #endregion
        
        #region 抽象方法 - 子类必须实现
        
        /// <summary>
        /// 初始化界面（只执行一次）
        /// </summary>
        protected abstract void Initialize();
        
        /// <summary>
        /// 界面打开时的处理
        /// </summary>
        protected abstract void OnScreenOpen();
        
        /// <summary>
        /// 界面关闭时的处理
        /// </summary>
        protected abstract void OnScreenClose();
        
        /// <summary>
        /// 刷新界面显示（数据变化时调用）
        /// </summary>
        public abstract void Refresh();

        public virtual void OnScreenResize()
        {
        }
        
        #endregion
        
        #region 参数传递
        
        /// <summary>
        /// 设置界面参数
        /// </summary>
        public virtual void SetParameters(Dictionary<string, object> param)
        {
            parameters = param ?? new Dictionary<string, object>();
            OnParametersSet();
        }
        
        /// <summary>
        /// 参数设置后的回调
        /// </summary>
        protected virtual void OnParametersSet()
        {
            // 子类可重写此方法处理参数
        }
        
        /// <summary>
        /// 获取参数值
        /// </summary>
        protected T GetParameter<T>(string key, T defaultValue = default)
        {
            if (parameters != null && parameters.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            return defaultValue;
        }
        
        #endregion
        
        #region 显示/隐藏动画
        
        /// <summary>
        /// 显示动画（协程，由ScreenRouter调用）
        /// </summary>
        public IEnumerator Show(float duration)
        {
            gameObject.SetActive(true);
            
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                CanvasGroup.alpha = t;
                yield return null;
            }
            
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 隐藏动画（协程，由ScreenRouter调用）
        /// </summary>
        public IEnumerator Hide(float duration)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(1 - Mathf.Clamp01(elapsed / duration));
                CanvasGroup.alpha = t;
                yield return null;
            }
            
            CanvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 立即显示（无动画）
        /// </summary>
        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
        }
        
        /// <summary>
        /// 立即隐藏（无动画）
        /// </summary>
        public void HideImmediate()
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 返回上一页
        /// </summary>
        protected void GoBack()
        {
            if (allowBackNavigation && ScreenRouter.Instance != null)
            {
                ScreenRouter.Instance.GoBack();
            }
        }
        
        /// <summary>
        /// 跳转到指定界面
        /// </summary>
        protected void NavigateTo(ScreenType screenType, bool pushToStack = false)
        {
            ScreenRouter.Instance?.NavigateTo(screenType, pushToStack);
        }
        
        /// <summary>
        /// 打开弹窗
        /// </summary>
        protected void OpenPopup(string popupId, Dictionary<string, object> param = null)
        {
            // 通过PopupController打开弹窗
            // 这里简化处理，实际项目中应该有PopupController
            Debug.Log($"[ScreenBase] 打开弹窗: {popupId}");
        }
        
        /// <summary>
        /// 关闭弹窗
        /// </summary>
        protected void ClosePopup(string popupId)
        {
            Debug.Log($"[ScreenBase] 关闭弹窗: {popupId}");
        }
        
        /// <summary>
        /// 显示Toast提示
        /// </summary>
        protected void ShowToast(string message, float duration = 2f)
        {
            Debug.Log($"[Toast] {message}");
            // 实际项目中应该有Toast系统
        }
        
        #endregion
    }
}
