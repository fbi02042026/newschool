using System;
using UnityEngine;
using UnityEngine.UI;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;

namespace GaokaoSimulator.Features.Profile
{
    /// <summary>
    /// 创建人物界面
    /// 支持性别选择、昵称输入、随机名字，并进入下一步流程。
    /// </summary>
    public class ProfileScreen : UI.ScreenBase
    {
        [Header("UI引用")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button maleButton;
        [SerializeField] private Button femaleButton;
        [SerializeField] private Button randomNameButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private InputField nameInputField;
        [SerializeField] private Text hintText;
        [SerializeField] private Text previewText;

        private Core.PlayerGender selectedGender = Core.PlayerGender.Male;

        [SerializeField] private Image maleCardImage;
        [SerializeField] private Image femaleCardImage;
        [SerializeField] private Image maleAvatarImage;
        [SerializeField] private Image femaleAvatarImage;

        private static readonly string[] MaleNames =
        {
            "林星河", "顾言川", "沈一鸣", "周子墨", "许知远", "程景行"
        };

        private static readonly string[] FemaleNames =
        {
            "苏念安", "林知夏", "许星晚", "沈书瑶", "江映月", "温可晴"
        };

        protected override void Initialize()
        {
            EnsureRuntimeLayout();
            BindEvents();
            Refresh();
        }

        protected override void OnScreenOpen()
        {
            if (Core.GameState.Instance != null)
            {
                Core.GameState.Instance.CurrentProgress = Core.GameProgress.Profile;
                Core.GameState.Instance.HasSaveData = true;
            }

            Refresh();
        }

        protected override void OnScreenClose()
        {
        }

        public override void Refresh()
        {
            var gameState = Core.GameState.Instance;
            if (gameState != null)
            {
                selectedGender = gameState.Gender;

                if (string.IsNullOrWhiteSpace(gameState.PlayerName))
                {
                    gameState.PlayerName = GetDefaultName();
                }

                if (nameInputField != null)
                {
                    nameInputField.text = gameState.PlayerName;
                }
            }
            else if (nameInputField != null && string.IsNullOrWhiteSpace(nameInputField.text))
            {
                nameInputField.text = GetDefaultName();
            }

            UpdateGenderButtons();
            UpdatePreview();
        }

        private void EnsureRuntimeLayout()
        {
            if (backButton != null && maleButton != null && femaleButton != null && randomNameButton != null && confirmButton != null && nameInputField != null && hintText != null && previewText != null)
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
            var bgImage = background.gameObject.AddComponent<Image>();
            bgImage.color = Color.white;
            var bgGradient = background.gameObject.AddComponent<UiVerticalGradient>();
            bgGradient.SetColors(new Color32(255, 248, 233, 255), new Color32(236, 247, 255, 255));

            CreateDecorCircle(root, "CircleTopLeft", new Vector2(88f, -140f), 180f, new Color32(255, 216, 226, 95));
            CreateDecorCircle(root, "CircleTopRight", new Vector2(-82f, -220f), 250f, new Color32(207, 231, 255, 95), true);
            CreateDecorCircle(root, "CircleBottom", new Vector2(0f, 120f), 360f, new Color32(255, 223, 205, 80));

            var panel = CreateUiObject("Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var headerCard = CreateUiObject("HeaderCard", panel);
            headerCard.anchorMin = new Vector2(0f, 0.78f);
            headerCard.anchorMax = new Vector2(1f, 1f);
            headerCard.offsetMin = Vector2.zero;
            headerCard.offsetMax = Vector2.zero;
            var stepBadge = CreateUiObject("StepBadge", headerCard);
            stepBadge.anchorMin = new Vector2(0.34f, 0.74f);
            stepBadge.anchorMax = new Vector2(0.66f, 0.98f);
            stepBadge.offsetMin = Vector2.zero;
            stepBadge.offsetMax = Vector2.zero;
            var stepBadgeImage = stepBadge.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(stepBadgeImage);
            stepBadgeImage.color = new Color32(255, 239, 229, 255);
            var stepText = CreateText("StepText", stepBadge, font, 28, FontStyle.Bold, new Color32(248, 137, 125, 255));
            Stretch(stepText.rectTransform);
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.text = "STEP 1 / 5";

            backButton = CreateSmallButton("回到首页", headerCard, font, new Color32(255, 224, 202, 255), new Color32(131, 87, 63, 255));
            var backRect = (RectTransform)backButton.transform;
            backRect.anchorMin = new Vector2(0.02f, 0.72f);
            backRect.anchorMax = new Vector2(0.22f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            backButton.gameObject.AddComponent<UiPressScale>();

            var title = CreateText("Title", headerCard, font, 78, FontStyle.Bold, new Color32(112, 71, 103, 255));
            title.text = "先选一个形象";
            title.alignment = TextAnchor.MiddleCenter;
            title.rectTransform.anchorMin = new Vector2(0.12f, 0.34f);
            title.rectTransform.anchorMax = new Vector2(0.88f, 0.78f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            hintText = CreateText("Hint", headerCard, font, 34, FontStyle.Normal, new Color32(142, 119, 135, 255));
            hintText.text = "临时版先确认身份与输入流程，风格通过后再替换正式角色插画";
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.rectTransform.anchorMin = new Vector2(0.08f, 0.02f);
            hintText.rectTransform.anchorMax = new Vector2(0.92f, 0.30f);
            hintText.rectTransform.offsetMin = Vector2.zero;
            hintText.rectTransform.offsetMax = Vector2.zero;

            var contentCard = CreateUiObject("ContentCard", panel);
            contentCard.anchorMin = new Vector2(0f, 0.22f);
            contentCard.anchorMax = new Vector2(1f, 0.74f);
            contentCard.offsetMin = Vector2.zero;
            contentCard.offsetMax = Vector2.zero;
            var contentImage = contentCard.gameObject.AddComponent<Image>();
            contentImage.color = new Color32(255, 255, 255, 248);
            AddSoftShadow(contentCard.gameObject, new Color(0.43f, 0.28f, 0.42f, 0.12f), new Vector2(0f, -14f));

            var avatarPanel = CreateUiObject("AvatarPanel", contentCard);
            avatarPanel.anchorMin = new Vector2(0.08f, 0.52f);
            avatarPanel.anchorMax = new Vector2(0.92f, 0.94f);
            avatarPanel.offsetMin = Vector2.zero;
            avatarPanel.offsetMax = Vector2.zero;

            maleButton = CreateGenderButton("男生", avatarPanel, font, new Color32(255, 255, 255, 248), new Color32(88, 116, 156, 255));
            var maleRect = (RectTransform)maleButton.transform;
            maleRect.anchorMin = new Vector2(0f, 0.12f);
            maleRect.anchorMax = new Vector2(0.48f, 0.98f);
            maleRect.offsetMin = Vector2.zero;
            maleRect.offsetMax = Vector2.zero;
            maleCardImage = maleButton.GetComponent<Image>();
            RuntimeArt.ApplyRounded(maleCardImage);
            AddSoftShadow(maleButton.gameObject, new Color(0.25f, 0.25f, 0.3f, 0.12f), new Vector2(0f, -10f));
            maleAvatarImage = AttachAvatar(maleButton.transform, font, "男");
            maleAvatarImage.gameObject.AddComponent<UiFloatBob>().Configure(6f, 0.5f, 0.1f);
            maleButton.gameObject.AddComponent<UiPressScale>();

            femaleButton = CreateGenderButton("女生", avatarPanel, font, new Color32(255, 255, 255, 248), new Color32(156, 96, 120, 255));
            var femaleRect = (RectTransform)femaleButton.transform;
            femaleRect.anchorMin = new Vector2(0.52f, 0.12f);
            femaleRect.anchorMax = new Vector2(1f, 0.98f);
            femaleRect.offsetMin = Vector2.zero;
            femaleRect.offsetMax = Vector2.zero;
            femaleCardImage = femaleButton.GetComponent<Image>();
            RuntimeArt.ApplyRounded(femaleCardImage);
            AddSoftShadow(femaleButton.gameObject, new Color(0.25f, 0.25f, 0.3f, 0.12f), new Vector2(0f, -10f));
            femaleAvatarImage = AttachAvatar(femaleButton.transform, font, "女");
            femaleAvatarImage.gameObject.AddComponent<UiFloatBob>().Configure(6f, 0.53f, 0.4f);
            femaleButton.gameObject.AddComponent<UiPressScale>();

            previewText = CreateText("AvatarPreview", avatarPanel, font, 34, FontStyle.Bold, new Color32(112, 76, 106, 255));
            previewText.alignment = TextAnchor.MiddleCenter;
            previewText.rectTransform.anchorMin = new Vector2(0f, 0f);
            previewText.rectTransform.anchorMax = new Vector2(1f, 0.12f);
            previewText.rectTransform.offsetMin = Vector2.zero;
            previewText.rectTransform.offsetMax = Vector2.zero;

            var inputSection = CreateUiObject("InputSection", contentCard);
            inputSection.anchorMin = new Vector2(0.08f, 0.06f);
            inputSection.anchorMax = new Vector2(0.92f, 0.34f);
            inputSection.offsetMin = Vector2.zero;
            inputSection.offsetMax = Vector2.zero;

            var nameLabel = CreateText("NameLabel", inputSection, font, 40, FontStyle.Bold, new Color32(112, 76, 106, 255));
            nameLabel.text = "同学，怎么称呼你呀？";
            nameLabel.alignment = TextAnchor.MiddleLeft;
            nameLabel.rectTransform.anchorMin = new Vector2(0f, 0.66f);
            nameLabel.rectTransform.anchorMax = new Vector2(0.4f, 1f);
            nameLabel.rectTransform.offsetMin = Vector2.zero;
            nameLabel.rectTransform.offsetMax = Vector2.zero;

            var inputBg = CreateUiObject("InputBg", inputSection);
            inputBg.anchorMin = new Vector2(0f, 0.16f);
            inputBg.anchorMax = new Vector2(0.72f, 0.62f);
            inputBg.offsetMin = Vector2.zero;
            inputBg.offsetMax = Vector2.zero;
            var inputBgImage = inputBg.gameObject.AddComponent<Image>();
            inputBgImage.color = new Color32(255, 250, 252, 255);
            RuntimeArt.ApplyRounded(inputBgImage);
            AddOutline(inputBg.gameObject, new Color32(255, 199, 221, 255));

            nameInputField = inputBg.gameObject.AddComponent<InputField>();
            var textViewport = CreateUiObject("TextViewport", inputBg);
            Stretch(textViewport);
            textViewport.offsetMin = new Vector2(22f, 14f);
            textViewport.offsetMax = new Vector2(-22f, -14f);
            textViewport.gameObject.AddComponent<RectMask2D>();

            var placeholder = CreateText("Placeholder", textViewport, font, 40, FontStyle.Normal, new Color32(190, 169, 179, 255));
            placeholder.text = "输入你想使用的称呼";
            placeholder.alignment = TextAnchor.MiddleLeft;
            Stretch(placeholder.rectTransform);

            var inputText = CreateText("Text", textViewport, font, 40, FontStyle.Bold, new Color32(96, 78, 101, 255));
            inputText.alignment = TextAnchor.MiddleLeft;
            Stretch(inputText.rectTransform);

            nameInputField.textComponent = inputText;
            nameInputField.placeholder = placeholder;
            nameInputField.lineType = InputField.LineType.SingleLine;
            nameInputField.characterLimit = 8;

            randomNameButton = CreateSmallButton("换个称呼", inputSection, font, new Color32(255, 230, 190, 255), new Color32(129, 93, 45, 255));
            var randomRect = (RectTransform)randomNameButton.transform;
            randomRect.anchorMin = new Vector2(0.76f, 0.16f);
            randomRect.anchorMax = new Vector2(1f, 0.62f);
            randomRect.offsetMin = Vector2.zero;
            randomRect.offsetMax = Vector2.zero;
            randomNameButton.gameObject.AddComponent<UiPressScale>();

            confirmButton = CreatePrimaryButton("就这样决定了", panel, font, new Color32(126, 189, 255, 255), new Color32(255, 255, 255, 255));
            var confirmRect = (RectTransform)confirmButton.transform;
            confirmRect.anchorMin = new Vector2(0.12f, 0.10f);
            confirmRect.anchorMax = new Vector2(0.88f, 0.18f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;
            var confirmImage = confirmButton.GetComponent<Image>();
            if (confirmImage != null)
            {
                var confirmGradient = confirmButton.gameObject.AddComponent<UiVerticalGradient>();
                confirmGradient.SetColors(new Color32(141, 206, 255, 255), new Color32(92, 162, 255, 255));
            }
            confirmButton.gameObject.AddComponent<UiPressScale>();
        }

        private static Image AttachAvatar(Transform card, Font font, string roleText)
        {
            var rect = CreateUiObject("Avatar", card);
            rect.anchorMin = new Vector2(0.08f, 0.18f);
            rect.anchorMax = new Vector2(0.92f, 0.96f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color32(255, 251, 253, 255);
            RuntimeArt.ApplyRounded(image);

            var halo = CreateUiObject("Halo", rect);
            halo.anchorMin = new Vector2(0.16f, 0.44f);
            halo.anchorMax = new Vector2(0.84f, 0.94f);
            halo.offsetMin = Vector2.zero;
            halo.offsetMax = Vector2.zero;
            var haloImage = halo.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(haloImage);
            haloImage.color = roleText == "男" ? new Color32(227, 242, 255, 255) : new Color32(255, 235, 244, 255);

            var badge = CreateUiObject("RoleBadge", halo);
            badge.anchorMin = new Vector2(0.26f, 0.18f);
            badge.anchorMax = new Vector2(0.74f, 0.66f);
            badge.offsetMin = Vector2.zero;
            badge.offsetMax = Vector2.zero;
            var badgeImage = badge.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(badgeImage);
            badgeImage.color = Color.white;

            var roleMain = CreateText("RoleMain", badge, font, 54, FontStyle.Bold, roleText == "男" ? new Color32(87, 126, 171, 255) : new Color32(173, 92, 126, 255));
            Stretch(roleMain.rectTransform);
            roleMain.alignment = TextAnchor.MiddleCenter;
            roleMain.text = roleText;

            var titleChip = CreateUiObject("TitleChip", rect);
            titleChip.anchorMin = new Vector2(0.22f, 0.28f);
            titleChip.anchorMax = new Vector2(0.78f, 0.40f);
            titleChip.offsetMin = Vector2.zero;
            titleChip.offsetMax = Vector2.zero;
            var titleChipImage = titleChip.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(titleChipImage);
            titleChipImage.color = Color.white;
            var titleText = CreateText("TitleText", titleChip, font, 24, FontStyle.Bold, new Color32(118, 92, 110, 255));
            Stretch(titleText.rectTransform);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = roleText == "男" ? "理性稳稳型" : "元气温柔型";

            CreateStatChip(rect, font, roleText == "男" ? "学习 +2" : "亲和 +2", new Vector2(0.12f, 0.08f), new Vector2(0.46f, 0.20f), roleText == "男" ? new Color32(226, 241, 255, 255) : new Color32(255, 233, 242, 255));
            CreateStatChip(rect, font, roleText == "男" ? "执行 +1" : "表达 +1", new Vector2(0.54f, 0.08f), new Vector2(0.88f, 0.20f), roleText == "男" ? new Color32(255, 239, 217, 255) : new Color32(236, 242, 255, 255));

            return image;
        }

        private static void CreateStatChip(Transform parent, Font font, string label, Vector2 min, Vector2 max, Color bgColor)
        {
            var chip = CreateUiObject($"Chip_{label}", parent);
            chip.anchorMin = min;
            chip.anchorMax = max;
            chip.offsetMin = Vector2.zero;
            chip.offsetMax = Vector2.zero;
            var chipImage = chip.gameObject.AddComponent<Image>();
            RuntimeArt.ApplyRounded(chipImage);
            chipImage.color = bgColor;
            var chipText = CreateText("ChipText", chip, font, 20, FontStyle.Bold, new Color32(126, 101, 118, 255));
            Stretch(chipText.rectTransform);
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.text = label;
        }

        private void BindEvents()
        {
            backButton.onClick.AddListener(() => NavigateTo(UI.ScreenType.Launch, false));
            maleButton.onClick.AddListener(() => SelectGender(Core.PlayerGender.Male));
            femaleButton.onClick.AddListener(() => SelectGender(Core.PlayerGender.Female));
            randomNameButton.onClick.AddListener(ApplyRandomName);
            confirmButton.onClick.AddListener(ConfirmProfile);
            nameInputField.onValueChanged.AddListener(_ => UpdatePreview());
        }

        private void SelectGender(Core.PlayerGender gender)
        {
            selectedGender = gender;
            UpdateGenderButtons();
            UpdatePreview();
        }

        private void ApplyRandomName()
        {
            var pool = selectedGender == Core.PlayerGender.Male ? MaleNames : FemaleNames;
            nameInputField.text = pool[UnityEngine.Random.Range(0, pool.Length)];
            UpdatePreview();
        }

        private void ConfirmProfile()
        {
            var playerName = SanitizeName(nameInputField.text);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                ShowToast("还没告诉我怎么称呼你呢");
                return;
            }

            if (Core.GameState.Instance != null)
            {
                Core.GameState.Instance.PlayerName = playerName;
                Core.GameState.Instance.Gender = selectedGender;
                Core.GameState.Instance.CurrentProgress = Core.GameProgress.Family;
                Core.GameState.Instance.HasSaveData = true;
            }

            NavigateTo(UI.ScreenType.Family, true);
        }

        private void UpdateGenderButtons()
        {
            UpdateGenderButtonVisual(maleButton, selectedGender == Core.PlayerGender.Male, new Color32(126, 197, 255, 255));
            UpdateGenderButtonVisual(femaleButton, selectedGender == Core.PlayerGender.Female, new Color32(255, 170, 200, 255));
        }

        private void UpdateGenderButtonVisual(Button button, bool isSelected, Color accentColor)
        {
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color32(255, 255, 255, 248);
                RuntimeArt.ApplyRounded(image);
            }

            var outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(6f, -6f);
            }

            outline.effectColor = isSelected ? accentColor : new Color32(0, 0, 0, 0);

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color32(245, 245, 245, 255);
            colors.selectedColor = Color.white;
            button.colors = colors;
        }

        private void UpdatePreview()
        {
            var playerName = SanitizeName(nameInputField != null ? nameInputField.text : string.Empty);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = GetDefaultName();
            }

            if (previewText != null)
            {
                var genderText = selectedGender == Core.PlayerGender.Male ? "男生" : "女生";
                previewText.text = $"当前选择：{genderText} · {playerName}";
            }
        }

        private string GetDefaultName()
        {
            var deviceName = SystemInfo.deviceName;
            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                return SanitizeName(deviceName);
            }

            return selectedGender == Core.PlayerGender.Male ? MaleNames[0] : FemaleNames[0];
        }

        private static string SanitizeName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return string.Empty;
            }

            var name = rawName.Trim();
            return name.Length > 8 ? name.Substring(0, 8) : name;
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

        private static void CreateDecorCircle(RectTransform parent, string name, Vector2 anchoredPosition, float size, Color color, bool anchorRight = false)
        {
            var circle = CreateUiObject(name, parent);
            circle.anchorMin = anchorRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            circle.anchorMax = circle.anchorMin;
            circle.anchoredPosition = anchoredPosition;
            circle.sizeDelta = new Vector2(size, size);
            var image = circle.gameObject.AddComponent<Image>();
            image.color = color;
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

        private static Button CreateGenderButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var button = CreatePrimaryButton(label, parent, font, bgColor, textColor);
            ((RectTransform)button.transform).sizeDelta = Vector2.zero;

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.resizeTextForBestFit = false;
                text.fontSize = 38;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                var rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0.18f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            return button;
        }

        private static Button CreateSmallButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var button = CreatePrimaryButton(label, parent, font, bgColor, textColor);
            var rect = (RectTransform)button.transform;
            rect.sizeDelta = Vector2.zero;
            return button;
        }

        private static Button CreatePrimaryButton(string label, Transform parent, Font font, Color bgColor, Color textColor)
        {
            var rect = CreateUiObject($"Button_{label}", parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = bgColor;
            RuntimeArt.ApplyRounded(image);

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.03f;
            colors.pressedColor = bgColor * 0.92f;
            colors.selectedColor = bgColor;
            button.colors = colors;

            AddSoftShadow(rect.gameObject, new Color(0.35f, 0.24f, 0.34f, 0.16f), new Vector2(0f, -8f));

            var text = CreateText("Label", rect, font, 44, FontStyle.Bold, textColor);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.rectTransform);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 26;
            text.resizeTextMaxSize = 44;

            return button;
        }

        private static void AddSoftShadow(GameObject target, Color shadowColor, Vector2 offset)
        {
            var shadow = target.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = offset;
        }

        private static void AddOutline(GameObject target, Color outlineColor)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }
}
