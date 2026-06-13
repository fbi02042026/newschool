using UnityEngine;

namespace GaokaoSimulator.Core
{
    /// <summary>
    /// BGM 音频管理器
    /// 单例自举，负责背景音乐播放、切换、淡入淡出
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音频源")]
        [SerializeField] private AudioSource bgmSource;

        [Header("淡入淡出")]
        [SerializeField] private float crossfadeDuration = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.45f;

        private string currentClipName;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSource();
        }

        private void EnsureAudioSource()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = bgmVolume;
            }
        }

        /// <summary>
        /// 播放指定 BGM（自动跳过已在播放的同名曲目）
        /// </summary>
        public void PlayBGM(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;

            if (currentClipName == clipName && bgmSource.isPlaying) return;

            var clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 未找到音频资源: Audio/{clipName}");
                return;
            }

            currentClipName = clipName;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(CrossfadeTo(clip));
        }

        private System.Collections.IEnumerator CrossfadeTo(AudioClip newClip)
        {
            float elapsed = 0;
            float startVolume = bgmSource.isPlaying ? bgmSource.volume : 0f;

            if (bgmSource.isPlaying)
            {
                while (elapsed < crossfadeDuration)
                {
                    elapsed += Time.deltaTime;
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossfadeDuration);
                    yield return null;
                }
                bgmSource.Stop();
                bgmSource.volume = 0f;
            }

            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            elapsed = 0;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / crossfadeDuration);
                yield return null;
            }

            bgmSource.volume = bgmVolume;
            fadeCoroutine = null;
        }

        public void StopBGM()
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            bgmSource.Stop();
            currentClipName = null;
        }

        public void SetVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null) bgmSource.volume = bgmVolume;
        }

        /// <summary>
        /// 根据 ScreenType 获取对应 BGM 资源名称
        /// </summary>
        public static string GetBgmForScreen(UI.ScreenType screenType)
        {
            return screenType switch
            {
                UI.ScreenType.Launch      => "bgm_home",
                UI.ScreenType.Profile     => "bgm_daily",
                UI.ScreenType.Family      => "bgm_daily",
                UI.ScreenType.Province    => "bgm_daily",
                UI.ScreenType.Subject     => "bgm_highschool_01",
                UI.ScreenType.Home         => "bgm_home",
                UI.ScreenType.Semester     => "bgm_highschool_02",
                UI.ScreenType.Gaokao       => "bgm_exam",
                UI.ScreenType.Volunteer    => "bgm_zhiyuan",
                UI.ScreenType.University   => "bgm_college_01",
                UI.ScreenType.Career       => "bgm_life_01",
                UI.ScreenType.Summary      => "bgm_result_happy",
                _ => "bgm_daily"
            };
        }
    }
}
