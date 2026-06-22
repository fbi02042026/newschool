using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI.Effects;

namespace GaokaoSimulator.UI
{
    public abstract class ScreenBase : MonoBehaviour
    {
        public ScreenType ScreenId { get; set; }

        protected virtual void Awake()
        {
            Initialize();
        }

        protected abstract void Initialize();

        protected virtual void OnScreenOpen() { }
        protected virtual void OnScreenClose() { }
        public virtual void Refresh() { }
        public virtual void OnScreenResize() { }

        public virtual void SetParameters(Dictionary<string, object> parameters) { }

        public virtual IEnumerator Show(float duration)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            OnScreenOpen();
        }

        public virtual IEnumerator Hide(float duration)
        {
            OnScreenClose();

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        protected void NavigateTo(ScreenType screenType, bool pushToStack = false)
        {
            if (ScreenRouter.Instance != null)
            {
                ScreenRouter.Instance.NavigateTo(screenType, pushToStack);
            }
            else
            {
                Debug.LogWarning($"[ScreenBase] ScreenRouter.Instance is null, cannot navigate to {screenType}");
            }
        }

        protected void GoBack()
        {
            if (ScreenRouter.Instance != null)
            {
                ScreenRouter.Instance.GoBack();
            }
        }

        protected void ShowToast(string message)
        {
            Debug.Log($"[{GetType().Name}] {message}");
        }

        protected virtual void ShowToastPopup(string message)
        {
            var font = Resources.Load<Font>("text/AlimamaFangYuanTiVF-Thin-2");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var toastGo = new GameObject("ToastPopup", typeof(RectTransform));
            var toastRect = toastGo.GetComponent<RectTransform>();
            toastRect.SetParent(transform, false);
            toastRect.anchorMin = new Vector2(0.1f, 0.42f);
            toastRect.anchorMax = new Vector2(0.9f, 0.52f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;
            toastRect.SetAsLastSibling();

            var toastText = toastGo.AddComponent<Text>();
            toastText.font = font;
            toastText.fontSize = 55;
            toastText.fontStyle = FontStyle.Normal;
            toastText.color = new Color32(255, 255, 150, 255);
            toastText.alignment = TextAnchor.MiddleCenter;
            toastText.raycastTarget = false;
            toastText.text = message;

            var shadow = toastGo.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(3, -2);

            var outline = toastGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -1);

            var canvasGroup = toastGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;

            StartCoroutine(FadeToastPopup(toastRect, canvasGroup));
        }

        private IEnumerator FadeToastPopup(RectTransform card, CanvasGroup canvasGroup)
        {
            yield return new WaitForSeconds(2f);

            float elapsed = 0;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = 1f - t;
                card.anchoredPosition += new Vector2(0, 60f * Time.deltaTime);
                yield return null;
            }

            Destroy(canvasGroup.gameObject);
        }

        protected static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        protected static RectTransform CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        protected static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        protected static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, Color color)
        {
            var rect = CreateUiObject(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.resizeTextForBestFit = false;
            return text;
        }

        protected static Button CreateButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var go = new GameObject(label, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            var image = go.AddComponent<Image>();
            image.color = bgColor;
            var button = go.AddComponent<Button>();
            RuntimeArt.ApplyRounded(image);

            var text = CreateText("Text", rect, font, 54, FontStyle.Bold, textColor);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 160f;

            return button;
        }

        protected static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var btn = CreateButton(label, parent, font, bgColor, textColor);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null) text.fontSize = 40;
            var layout = btn.GetComponent<LayoutElement>();
            if (layout != null) layout.preferredHeight = 120f;
            return btn;
        }

        protected static Button CreatePrimaryButton(string label, Transform parent, Font font, Color a, Color textColor)
        {
            var button = CreateButton(label, parent, font, Color.white, textColor);
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
                var grad = button.gameObject.AddComponent<UiCornerGradient>();
                grad.SetColors(a, UITheme.ConfirmHover, UITheme.ConfirmHover, a);
                var shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(a.r / 255f, a.g / 255f, a.b / 255f, 0.35f);
                shadow.effectDistance = new Vector2(0f, -12f);
            }
            return button;
        }
    }
}