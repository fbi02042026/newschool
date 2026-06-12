using System;
using GaokaoSimulator.Features.Launch;
using GaokaoSimulator.Features.Profile;
using GaokaoSimulator.Features.Family;
using GaokaoSimulator.Features.Province;
using GaokaoSimulator.UI;
using GaokaoSimulator.UI.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.EditorTools
{
    public static class UIScreenPrefabGenerator
    {
        private const string LaunchPrefabPath = "Assets/Resources/UI/Screens/Screen_Launch.prefab";
        private const string ProfilePrefabPath = "Assets/Resources/UI/Screens/Screen_Profile.prefab";
        private const string FamilyPrefabPath = "Assets/Resources/UI/Screens/Screen_Family.prefab";
        private const string ProvincePrefabPath = "Assets/Resources/UI/Screens/Screen_Province.prefab";

        [MenuItem("Gaokao/UI/Generate Screen Prefabs (Safe)")]
        private static void GenerateSafe()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder("Assets/Resources/UI/Screens");

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GenerateLaunch(font, overwrite: false);
            GenerateProfile(font, overwrite: false);
            GenerateFamily(font, overwrite: false);
            GenerateProvince(font, overwrite: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Gaokao/UI/Regenerate Screen Prefabs (Overwrite)")]
        private static void GenerateOverwrite()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder("Assets/Resources/UI/Screens");

            var anyExists = AssetDatabase.LoadAssetAtPath<GameObject>(LaunchPrefabPath) != null
                            || AssetDatabase.LoadAssetAtPath<GameObject>(ProfilePrefabPath) != null;
            anyExists = anyExists || AssetDatabase.LoadAssetAtPath<GameObject>(FamilyPrefabPath) != null;
            anyExists = anyExists || AssetDatabase.LoadAssetAtPath<GameObject>(ProvincePrefabPath) != null;

            if (anyExists)
            {
                var choice = EditorUtility.DisplayDialogComplex(
                    "Regenerate Screen Prefabs",
                    "检测到已有 Screen Prefab。\n\nOverwrite：覆盖现有（会丢失你在 Prefab 里的手工调整）\nSkip：跳过已存在，仅生成缺失\nCancel：取消",
                    "Overwrite",
                    "Skip",
                    "Cancel"
                );

                if (choice == 2)
                {
                    return;
                }

                var overwrite = choice == 0;
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                GenerateLaunch(font, overwrite);
                GenerateProfile(font, overwrite);
                GenerateFamily(font, overwrite);
                GenerateProvince(font, overwrite);
            }
            else
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                GenerateLaunch(font, overwrite: true);
                GenerateProfile(font, overwrite: true);
                GenerateFamily(font, overwrite: true);
                GenerateProvince(font, overwrite: true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void GenerateLaunch(Font font, bool overwrite)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(LaunchPrefabPath) != null)
            {
                return;
            }

            var root = new GameObject("Screen_Launch", typeof(RectTransform));
            var screen = root.AddComponent<LaunchScreen>();
            root.GetComponent<RectTransform>().localScale = Vector3.one;
            Stretch(root.GetComponent<RectTransform>());

            var bg = CreateImage("Background", root.transform, Color.white);
            Stretch(bg.rectTransform);
            var bgGrad = bg.gameObject.AddComponent<UiCornerGradient>();
            bgGrad.SetColors(UITheme.FromHex("FFF0E6"), UITheme.FromHex("FFF0E6"), UITheme.FromHex("F3E5F5"), UITheme.FromHex("FFF8F0"));

            var dots = CreateRect("BgDots", root.transform);
            Stretch(dots);
            GenerateDots(dots, 12);

            var decorations = CreateRect("Decorations", root.transform);
            Stretch(decorations);
            CreateFloatingSticker(decorations, font, "📖", new Vector2(0.14f, 0.88f), UITheme.CardButter);
            CreateFloatingSticker(decorations, font, "✏", new Vector2(0.86f, 0.84f), UITheme.CardPeach);
            CreateFloatingSticker(decorations, font, "🎒", new Vector2(0.18f, 0.28f), UITheme.CardSky);
            CreateFloatingSticker(decorations, font, "🏫", new Vector2(0.84f, 0.34f), UITheme.CardMint);

            var content = CreateRect("Content", root.transform);
            content.anchorMin = new Vector2(0.08f, 0.18f);
            content.anchorMax = new Vector2(0.92f, 0.84f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var titleIcon = CreateImage("TitleIcon", content, Color.white);
            titleIcon.rectTransform.anchorMin = new Vector2(0.45f, 0.76f);
            titleIcon.rectTransform.anchorMax = new Vector2(0.55f, 0.90f);
            titleIcon.rectTransform.offsetMin = Vector2.zero;
            titleIcon.rectTransform.offsetMax = Vector2.zero;
            titleIcon.gameObject.AddComponent<UiAutoRounded>();
            titleIcon.color = UITheme.CardSky;
            titleIcon.gameObject.AddComponent<UiFloatBob>().Configure(UITheme.ScaleY(10f), 0.5f, 0f);
            var iconText = CreateText("IconText", titleIcon.transform, font, 80, FontStyle.Bold, UITheme.Text);
            Stretch(iconText.rectTransform);
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.text = "🎓";

            var titleText = CreateText("TitleText", content, font, 110, FontStyle.Bold, UITheme.Accent);
            titleText.rectTransform.anchorMin = new Vector2(0.06f, 0.56f);
            titleText.rectTransform.anchorMax = new Vector2(0.94f, 0.76f);
            titleText.rectTransform.offsetMin = Vector2.zero;
            titleText.rectTransform.offsetMax = Vector2.zero;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "我的高考志愿模拟器";
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(1f, 1f, 1f, 0.85f);
            titleText.GetComponent<Shadow>().effectDistance = new Vector2(0, 6);

            var subTitle = CreateText("SubTitle", content, font, 44, FontStyle.Normal, UITheme.TextLight);
            subTitle.rectTransform.anchorMin = new Vector2(0.08f, 0.48f);
            subTitle.rectTransform.anchorMax = new Vector2(0.92f, 0.58f);
            subTitle.rectTransform.offsetMin = Vector2.zero;
            subTitle.rectTransform.offsetMax = Vector2.zero;
            subTitle.alignment = TextAnchor.MiddleCenter;
            subTitle.text = "GAOKAO SIMULATOR · 可试玩版本";

            var buttonGroup = CreateRect("ButtonGroup", content);
            buttonGroup.anchorMin = new Vector2(0.12f, 0.10f);
            buttonGroup.anchorMax = new Vector2(0.88f, 0.42f);
            buttonGroup.offsetMin = Vector2.zero;
            buttonGroup.offsetMax = Vector2.zero;

            var newBtn = CreateButton("BtnNewGame", buttonGroup, font, "重启我的高中人生", true);
            newBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.58f);
            newBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.96f);
            newBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            newBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var contBtn = CreateButton("BtnContinue", buttonGroup, font, "继续", false);
            contBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.10f);
            contBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.48f);
            contBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            contBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var version = CreateText("VersionLabel", root.transform, font, 30, FontStyle.Bold, new Color(UITheme.TextLight.r / 255f, UITheme.TextLight.g / 255f, UITheme.TextLight.b / 255f, 0.55f));
            version.rectTransform.anchorMin = new Vector2(0.34f, 0.02f);
            version.rectTransform.anchorMax = new Vector2(0.66f, 0.06f);
            version.rectTransform.offsetMin = Vector2.zero;
            version.rectTransform.offsetMax = Vector2.zero;
            version.alignment = TextAnchor.MiddleCenter;
            version.text = "v0.0";

            var about = CreateIconButton("BtnAbout", root.transform, font, "i");
            about.GetComponent<RectTransform>().anchorMin = new Vector2(0.04f, 0.03f);
            about.GetComponent<RectTransform>().anchorMax = new Vector2(0.14f, 0.09f);
            about.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            about.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var settings = CreateIconButton("BtnSettings", root.transform, font, "⚙");
            settings.GetComponent<RectTransform>().anchorMin = new Vector2(0.86f, 0.03f);
            settings.GetComponent<RectTransform>().anchorMax = new Vector2(0.96f, 0.09f);
            settings.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            settings.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var so = new SerializedObject(screen);
            so.FindProperty("newGameButton").objectReferenceValue = newBtn;
            so.FindProperty("continueGameButton").objectReferenceValue = contBtn;
            so.FindProperty("settingsButton").objectReferenceValue = settings;
            so.FindProperty("aboutButton").objectReferenceValue = about;
            so.FindProperty("titleTransform").objectReferenceValue = content;
            so.FindProperty("versionText").objectReferenceValue = version;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, LaunchPrefabPath);
        }

        private static void GenerateProfile(Font font, bool overwrite)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(ProfilePrefabPath) != null)
            {
                return;
            }

            var root = new GameObject("Screen_Profile", typeof(RectTransform));
            var screen = root.AddComponent<ProfileScreen>();
            Stretch(root.GetComponent<RectTransform>());

            var bg = CreateImage("Background", root.transform, Color.white);
            Stretch(bg.rectTransform);
            var bgGrad = bg.gameObject.AddComponent<UiCornerGradient>();
            bgGrad.SetColors(UITheme.FromHex("FFF0E6"), UITheme.FromHex("FFF8F0"), UITheme.FromHex("E3F2FD"), UITheme.FromHex("F3E5F5"));

            var dots = CreateRect("BgDots", root.transform);
            Stretch(dots);
            GenerateDots(dots, 12);

            var header = CreateRect("ScreenHeader", root.transform);
            header.anchorMin = new Vector2(0.06f, 0.78f);
            header.anchorMax = new Vector2(0.94f, 0.96f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var stepBadge = CreateImage("StepBadge", header.transform, UITheme.CardPeach);
            stepBadge.gameObject.AddComponent<UiAutoRounded>();
            stepBadge.rectTransform.anchorMin = new Vector2(0.36f, 0.68f);
            stepBadge.rectTransform.anchorMax = new Vector2(0.64f, 0.98f);
            stepBadge.rectTransform.offsetMin = Vector2.zero;
            stepBadge.rectTransform.offsetMax = Vector2.zero;
            var stepText = CreateText("Text", stepBadge.transform, font, 32, FontStyle.Bold, UITheme.Accent);
            Stretch(stepText.rectTransform);
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.text = "STEP 1 / 5";

            var back = CreateButton("BtnBack", header.transform, font, "← 返回", false);
            back.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.68f);
            back.GetComponent<RectTransform>().anchorMax = new Vector2(0.24f, 0.98f);
            back.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            back.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var title = CreateText("Title", header.transform, font, 84, FontStyle.Bold, UITheme.Text);
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.26f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.72f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.alignment = TextAnchor.MiddleCenter;
            title.text = "创建你的角色";

            var subtitle = CreateText("Subtitle", header.transform, font, 42, FontStyle.Normal, UITheme.TextLight);
            subtitle.rectTransform.anchorMin = new Vector2(0.06f, 0f);
            subtitle.rectTransform.anchorMax = new Vector2(0.94f, 0.30f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.text = "先选性别，再告诉我你的名字";

            var body = CreateRect("ScreenBody", root.transform);
            body.anchorMin = new Vector2(0.06f, 0.22f);
            body.anchorMax = new Vector2(0.94f, 0.76f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var cards = CreateRect("GenderCards", body.transform);
            cards.anchorMin = new Vector2(0f, 0.52f);
            cards.anchorMax = new Vector2(1f, 1f);
            cards.offsetMin = Vector2.zero;
            cards.offsetMax = Vector2.zero;

            var male = CreateCardButton("MaleCard", cards, font, "男生", UITheme.CardSky);
            male.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.06f);
            male.GetComponent<RectTransform>().anchorMax = new Vector2(0.49f, 0.94f);
            male.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            male.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var female = CreateCardButton("FemaleCard", cards, font, "女生", UITheme.FromHex("FFEAF3"));
            female.GetComponent<RectTransform>().anchorMin = new Vector2(0.51f, 0.06f);
            female.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.94f);
            female.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            female.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var nameSection = CreateRect("NameSection", body.transform);
            nameSection.anchorMin = new Vector2(0f, 0f);
            nameSection.anchorMax = new Vector2(1f, 0.50f);
            nameSection.offsetMin = Vector2.zero;
            nameSection.offsetMax = Vector2.zero;

            var label = CreateText("NameLabel", nameSection.transform, font, 40, FontStyle.Bold, UITheme.Text);
            label.rectTransform.anchorMin = new Vector2(0.02f, 0.66f);
            label.rectTransform.anchorMax = new Vector2(0.98f, 0.92f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.alignment = TextAnchor.MiddleLeft;
            label.text = "你的姓名";

            var inputWrap = CreateImage("NameInputWrap", nameSection.transform, Color.white);
            inputWrap.gameObject.AddComponent<UiAutoRounded>();
            inputWrap.rectTransform.anchorMin = new Vector2(0f, 0.28f);
            inputWrap.rectTransform.anchorMax = new Vector2(1f, 0.62f);
            inputWrap.rectTransform.offsetMin = Vector2.zero;
            inputWrap.rectTransform.offsetMax = Vector2.zero;
            var inputOutline = inputWrap.gameObject.AddComponent<Outline>();
            inputOutline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
            inputOutline.effectDistance = new Vector2(4, -4);

            var placeholder = CreateText("Placeholder", inputWrap.transform, font, 42, FontStyle.Italic, new Color32(UITheme.TextLight.r, UITheme.TextLight.g, UITheme.TextLight.b, 180));
            placeholder.rectTransform.anchorMin = new Vector2(0.06f, 0f);
            placeholder.rectTransform.anchorMax = new Vector2(0.94f, 1f);
            placeholder.rectTransform.offsetMin = Vector2.zero;
            placeholder.rectTransform.offsetMax = Vector2.zero;
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.text = "请输入 2-8 个字";

            var inputText = CreateText("Text", inputWrap.transform, font, 46, FontStyle.Bold, UITheme.Text);
            inputText.rectTransform.anchorMin = new Vector2(0.06f, 0f);
            inputText.rectTransform.anchorMax = new Vector2(0.94f, 1f);
            inputText.rectTransform.offsetMin = Vector2.zero;
            inputText.rectTransform.offsetMax = Vector2.zero;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.text = string.Empty;

            var inputField = inputWrap.gameObject.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;
            inputField.characterLimit = 8;
            inputField.lineType = InputField.LineType.SingleLine;

            var random = CreateButton("BtnRandom", nameSection.transform, font, "随机", false);
            random.GetComponent<RectTransform>().anchorMin = new Vector2(0.64f, 0.06f);
            random.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.24f);
            random.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            random.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var preview = CreateText("Preview", nameSection.transform, font, 34, FontStyle.Bold, UITheme.TextSoft);
            preview.rectTransform.anchorMin = new Vector2(0f, 0.06f);
            preview.rectTransform.anchorMax = new Vector2(0.62f, 0.24f);
            preview.rectTransform.offsetMin = Vector2.zero;
            preview.rectTransform.offsetMax = Vector2.zero;
            preview.alignment = TextAnchor.MiddleLeft;
            preview.text = "当前选择：男生 · 林星河";

            var footer = CreateRect("ScreenFooter", root.transform);
            footer.anchorMin = new Vector2(0.06f, 0.06f);
            footer.anchorMax = new Vector2(0.94f, 0.18f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            var confirm = CreateButton("BtnConfirm", footer, font, "下一步 →", true);
            Stretch(confirm.GetComponent<RectTransform>());

            var so = new SerializedObject(screen);
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("maleButton").objectReferenceValue = male;
            so.FindProperty("femaleButton").objectReferenceValue = female;
            so.FindProperty("randomNameButton").objectReferenceValue = random;
            so.FindProperty("confirmButton").objectReferenceValue = confirm;
            so.FindProperty("nameInputField").objectReferenceValue = inputField;
            so.FindProperty("hintText").objectReferenceValue = subtitle;
            so.FindProperty("previewText").objectReferenceValue = preview;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ProfilePrefabPath);
        }

        private static void GenerateFamily(Font font, bool overwrite)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(FamilyPrefabPath) != null)
            {
                return;
            }

            var root = new GameObject("Screen_Family", typeof(RectTransform));
            var screen = root.AddComponent<FamilyScreen>();
            Stretch(root.GetComponent<RectTransform>());

            var bg = CreateImage("Background", root.transform, Color.white);
            Stretch(bg.rectTransform);
            var bgGrad = bg.gameObject.AddComponent<UiCornerGradient>();
            bgGrad.SetColors(UITheme.CardPeach, UITheme.Bg, UITheme.CardSky, UITheme.CardLavender);

            var dots = CreateRect("BgDots", root.transform);
            Stretch(dots);
            GenerateDots(dots, 12);

            var panel = CreateRect("Panel", root.transform);
            panel.anchorMin = new Vector2(0.06f, 0.04f);
            panel.anchorMax = new Vector2(0.94f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateRect("ScreenHeader", panel);
            header.anchorMin = new Vector2(0f, 0.78f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var stepBadge = CreateImage("StepBadge", header.transform, UITheme.CardPeach);
            stepBadge.gameObject.AddComponent<UiAutoRounded>();
            stepBadge.rectTransform.anchorMin = new Vector2(0.36f, 0.70f);
            stepBadge.rectTransform.anchorMax = new Vector2(0.64f, 0.98f);
            stepBadge.rectTransform.offsetMin = Vector2.zero;
            stepBadge.rectTransform.offsetMax = Vector2.zero;
            var stepText = CreateText("Text", stepBadge.transform, font, 32, FontStyle.Bold, UITheme.Accent);
            Stretch(stepText.rectTransform);
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.text = "STEP 2 / 5";

            var back = CreateButton("BtnBack", header.transform, font, "← 返回", false);
            back.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.70f);
            back.GetComponent<RectTransform>().anchorMax = new Vector2(0.24f, 0.98f);
            back.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            back.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var help = CreateButton("BtnHelp", header.transform, font, "?", false);
            help.GetComponent<RectTransform>().anchorMin = new Vector2(0.90f, 0.70f);
            help.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.98f);
            help.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            help.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var title = CreateText("Title", header.transform, font, 84, FontStyle.Bold, UITheme.Text);
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.26f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.72f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.alignment = TextAnchor.MiddleCenter;
            title.text = "命运的起点";

            var subtitle = CreateText("Subtitle", header.transform, font, 42, FontStyle.Normal, UITheme.TextLight);
            subtitle.rectTransform.anchorMin = new Vector2(0.06f, 0f);
            subtitle.rectTransform.anchorMax = new Vector2(0.94f, 0.30f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.text = "抽一张家庭背景卡，决定你的开局资源";

            var body = CreateRect("ScreenBody", panel);
            body.anchorMin = new Vector2(0f, 0.20f);
            body.anchorMax = new Vector2(1f, 0.76f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var drawStage = CreateRect("DrawStage", body);
            Stretch(drawStage);

            var drawCard = CreateImage("DrawCard", drawStage.transform, Color.white);
            drawCard.gameObject.AddComponent<UiAutoRounded>();
            drawCard.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.08f);
            drawCard.GetComponent<Shadow>().effectDistance = new Vector2(0, -16);
            drawCard.rectTransform.anchorMin = new Vector2(0.18f, 0.20f);
            drawCard.rectTransform.anchorMax = new Vector2(0.82f, 0.90f);
            drawCard.rectTransform.offsetMin = Vector2.zero;
            drawCard.rectTransform.offsetMax = Vector2.zero;

            var drawIconWrap = CreateImage("IconWrap", drawCard.transform, UITheme.CardSky);
            drawIconWrap.gameObject.AddComponent<UiAutoRounded>();
            drawIconWrap.gameObject.AddComponent<UiFloatBob>().Configure(10f, 0.45f, 0f);
            drawIconWrap.rectTransform.anchorMin = new Vector2(0.24f, 0.62f);
            drawIconWrap.rectTransform.anchorMax = new Vector2(0.76f, 0.92f);
            drawIconWrap.rectTransform.offsetMin = Vector2.zero;
            drawIconWrap.rectTransform.offsetMax = Vector2.zero;
            var drawIcon = CreateText("Icon", drawIconWrap.transform, font, 96, FontStyle.Bold, UITheme.Text);
            Stretch(drawIcon.rectTransform);
            drawIcon.alignment = TextAnchor.MiddleCenter;
            drawIcon.text = "🏠";

            var drawName = CreateText("Name", drawCard.transform, font, 60, FontStyle.Bold, UITheme.Text);
            drawName.rectTransform.anchorMin = new Vector2(0.08f, 0.44f);
            drawName.rectTransform.anchorMax = new Vector2(0.92f, 0.62f);
            drawName.rectTransform.offsetMin = Vector2.zero;
            drawName.rectTransform.offsetMax = Vector2.zero;
            drawName.alignment = TextAnchor.MiddleCenter;
            drawName.text = "抽取中…";

            var drawDesc = CreateText("Desc", drawCard.transform, font, 36, FontStyle.Normal, UITheme.TextLight);
            drawDesc.rectTransform.anchorMin = new Vector2(0.10f, 0.18f);
            drawDesc.rectTransform.anchorMax = new Vector2(0.90f, 0.44f);
            drawDesc.rectTransform.offsetMin = Vector2.zero;
            drawDesc.rectTransform.offsetMax = Vector2.zero;
            drawDesc.alignment = TextAnchor.MiddleCenter;
            drawDesc.text = "正在抽取你的家庭背景…";

            var barBg = CreateImage("ProgressBarBg", drawStage.transform, new Color32(238, 238, 238, 255));
            barBg.gameObject.AddComponent<UiAutoRounded>();
            barBg.rectTransform.anchorMin = new Vector2(0.18f, 0.12f);
            barBg.rectTransform.anchorMax = new Vector2(0.82f, 0.16f);
            barBg.rectTransform.offsetMin = Vector2.zero;
            barBg.rectTransform.offsetMax = Vector2.zero;
            var barFill = CreateImage("ProgressFill", barBg.transform, Color.white);
            barFill.gameObject.AddComponent<UiAutoRounded>();
            Stretch(barFill.rectTransform);
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillOrigin = 0;
            barFill.fillAmount = 0f;
            var barFillGrad = barFill.gameObject.AddComponent<UiCornerGradient>();
            barFillGrad.SetColors(UITheme.Confirm, UITheme.Accent, UITheme.Accent, UITheme.Confirm);

            var drawHint = CreateText("DrawHint", drawStage.transform, font, 36, FontStyle.Normal, UITheme.TextLight);
            drawHint.rectTransform.anchorMin = new Vector2(0.08f, 0.02f);
            drawHint.rectTransform.anchorMax = new Vector2(0.92f, 0.10f);
            drawHint.rectTransform.offsetMin = Vector2.zero;
            drawHint.rectTransform.offsetMax = Vector2.zero;
            drawHint.alignment = TextAnchor.MiddleCenter;
            drawHint.text = "抽卡中…";

            var resultStage = CreateRect("ResultStage", body);
            Stretch(resultStage);
            resultStage.gameObject.SetActive(false);

            var resultCard = CreateImage("ResultCard", resultStage.transform, Color.white);
            resultCard.gameObject.AddComponent<UiAutoRounded>();
            resultCard.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.08f);
            resultCard.GetComponent<Shadow>().effectDistance = new Vector2(0, -18);
            resultCard.rectTransform.anchorMin = new Vector2(0.08f, 0.18f);
            resultCard.rectTransform.anchorMax = new Vector2(0.92f, 0.96f);
            resultCard.rectTransform.offsetMin = Vector2.zero;
            resultCard.rectTransform.offsetMax = Vector2.zero;

            var resultIconWrap = CreateImage("ResultIconWrap", resultCard.transform, UITheme.CardSky);
            resultIconWrap.gameObject.AddComponent<UiAutoRounded>();
            resultIconWrap.gameObject.AddComponent<UiFloatBob>().Configure(8f, 0.5f, 0.2f);
            resultIconWrap.rectTransform.anchorMin = new Vector2(0.32f, 0.72f);
            resultIconWrap.rectTransform.anchorMax = new Vector2(0.68f, 0.92f);
            resultIconWrap.rectTransform.offsetMin = Vector2.zero;
            resultIconWrap.rectTransform.offsetMax = Vector2.zero;
            var resultIcon = CreateText("Icon", resultIconWrap.transform, font, 92, FontStyle.Bold, UITheme.Text);
            Stretch(resultIcon.rectTransform);
            resultIcon.alignment = TextAnchor.MiddleCenter;
            resultIcon.text = "🏠";

            var resultName = CreateText("ResultName", resultCard.transform, font, 64, FontStyle.Bold, UITheme.Text);
            resultName.rectTransform.anchorMin = new Vector2(0.08f, 0.60f);
            resultName.rectTransform.anchorMax = new Vector2(0.92f, 0.72f);
            resultName.rectTransform.offsetMin = Vector2.zero;
            resultName.rectTransform.offsetMax = Vector2.zero;
            resultName.alignment = TextAnchor.MiddleCenter;
            resultName.text = "家庭背景";

            var resultDesc = CreateText("ResultDesc", resultCard.transform, font, 36, FontStyle.Normal, UITheme.TextLight);
            resultDesc.rectTransform.anchorMin = new Vector2(0.10f, 0.46f);
            resultDesc.rectTransform.anchorMax = new Vector2(0.90f, 0.60f);
            resultDesc.rectTransform.offsetMin = Vector2.zero;
            resultDesc.rectTransform.offsetMax = Vector2.zero;
            resultDesc.alignment = TextAnchor.MiddleCenter;
            resultDesc.text = "描述";

            var moneyChip = CreateImage("MoneyChip", resultCard.transform, UITheme.GoldLight);
            moneyChip.gameObject.AddComponent<UiAutoRounded>();
            moneyChip.rectTransform.anchorMin = new Vector2(0.30f, 0.38f);
            moneyChip.rectTransform.anchorMax = new Vector2(0.70f, 0.46f);
            moneyChip.rectTransform.offsetMin = Vector2.zero;
            moneyChip.rectTransform.offsetMax = Vector2.zero;
            var moneyText = CreateText("MoneyText", moneyChip.transform, font, 34, FontStyle.Bold, UITheme.FromHex("E65100"));
            Stretch(moneyText.rectTransform);
            moneyText.alignment = TextAnchor.MiddleCenter;
            moneyText.text = "金币 0";

            var statGrid = CreateRect("StatGrid", resultCard.transform);
            statGrid.anchorMin = new Vector2(0.08f, 0.06f);
            statGrid.anchorMax = new Vector2(0.92f, 0.34f);
            statGrid.offsetMin = Vector2.zero;
            statGrid.offsetMax = Vector2.zero;

            CreateStatItem(statGrid, font, "智力", "🧠", new Vector2(0f, 0.52f), new Vector2(0.48f, 1f), out var statIntFill, out var statIntValue);
            CreateStatItem(statGrid, font, "心理", "💜", new Vector2(0.52f, 0.52f), new Vector2(1f, 1f), out var statPsyFill, out var statPsyValue);
            CreateStatItem(statGrid, font, "社交", "💬", new Vector2(0f, 0f), new Vector2(0.48f, 0.48f), out var statSocFill, out var statSocValue);
            CreateStatItem(statGrid, font, "健康", "💪", new Vector2(0.52f, 0f), new Vector2(1f, 0.48f), out var statHealthFill, out var statHealthValue);

            var footer = CreateRect("ScreenFooter", panel);
            footer.anchorMin = new Vector2(0f, 0.04f);
            footer.anchorMax = new Vector2(1f, 0.18f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            var keep = CreateButton("BtnKeep", footer, font, "✅ 就这个了！", true);
            keep.GetComponent<RectTransform>().anchorMin = new Vector2(0.08f, 0.55f);
            keep.GetComponent<RectTransform>().anchorMax = new Vector2(0.92f, 1f);
            keep.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            keep.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            keep.gameObject.SetActive(false);

            var reroll = CreateButton("BtnReroll", footer, font, "🔄 免费重置（1次）", false);
            reroll.GetComponent<RectTransform>().anchorMin = new Vector2(0.08f, 0.04f);
            reroll.GetComponent<RectTransform>().anchorMax = new Vector2(0.92f, 0.49f);
            reroll.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            reroll.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            reroll.gameObject.SetActive(false);

            var so = new SerializedObject(screen);
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("helpButton").objectReferenceValue = help;
            so.FindProperty("keepButton").objectReferenceValue = keep;
            so.FindProperty("rerollButton").objectReferenceValue = reroll;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("subtitleText").objectReferenceValue = subtitle;
            so.FindProperty("drawHintText").objectReferenceValue = drawHint;
            so.FindProperty("drawCardIconText").objectReferenceValue = drawIcon;
            so.FindProperty("drawCardNameText").objectReferenceValue = drawName;
            so.FindProperty("drawCardDescText").objectReferenceValue = drawDesc;
            so.FindProperty("progressFillImage").objectReferenceValue = barFill;
            so.FindProperty("drawStageRoot").objectReferenceValue = drawStage;
            so.FindProperty("resultStageRoot").objectReferenceValue = resultStage;
            so.FindProperty("resultIconText").objectReferenceValue = resultIcon;
            so.FindProperty("resultNameText").objectReferenceValue = resultName;
            so.FindProperty("resultDescText").objectReferenceValue = resultDesc;
            so.FindProperty("resultMoneyText").objectReferenceValue = moneyText;
            so.FindProperty("statIntFill").objectReferenceValue = statIntFill;
            so.FindProperty("statPsyFill").objectReferenceValue = statPsyFill;
            so.FindProperty("statSocFill").objectReferenceValue = statSocFill;
            so.FindProperty("statHealthFill").objectReferenceValue = statHealthFill;
            so.FindProperty("statIntValue").objectReferenceValue = statIntValue;
            so.FindProperty("statPsyValue").objectReferenceValue = statPsyValue;
            so.FindProperty("statSocValue").objectReferenceValue = statSocValue;
            so.FindProperty("statHealthValue").objectReferenceValue = statHealthValue;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, FamilyPrefabPath);
        }

        private static void GenerateProvince(Font font, bool overwrite)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(ProvincePrefabPath) != null)
            {
                return;
            }

            var root = new GameObject("Screen_Province", typeof(RectTransform));
            var screen = root.AddComponent<ProvinceScreen>();
            Stretch(root.GetComponent<RectTransform>());

            var bg = CreateImage("Background", root.transform, Color.white);
            Stretch(bg.rectTransform);
            var bgGrad = bg.gameObject.AddComponent<UiCornerGradient>();
            bgGrad.SetColors(UITheme.CardButter, UITheme.Bg, UITheme.CardSky, UITheme.CardLavender);

            var dots = CreateRect("BgDots", root.transform);
            Stretch(dots);
            GenerateDots(dots, 12);

            var panel = CreateRect("Panel", root.transform);
            panel.anchorMin = new Vector2(0.05f, 0.04f);
            panel.anchorMax = new Vector2(0.95f, 0.96f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            var header = CreateRect("ScreenHeader", panel);
            header.anchorMin = new Vector2(0f, 0.80f);
            header.anchorMax = new Vector2(1f, 1f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            var stepBadge = CreateImage("StepBadge", header.transform, UITheme.CardPeach);
            stepBadge.gameObject.AddComponent<UiAutoRounded>();
            stepBadge.rectTransform.anchorMin = new Vector2(0.36f, 0.68f);
            stepBadge.rectTransform.anchorMax = new Vector2(0.64f, 0.96f);
            stepBadge.rectTransform.offsetMin = Vector2.zero;
            stepBadge.rectTransform.offsetMax = Vector2.zero;
            var stepText = CreateText("Text", stepBadge.transform, font, 32, FontStyle.Bold, UITheme.Accent);
            Stretch(stepText.rectTransform);
            stepText.alignment = TextAnchor.MiddleCenter;
            stepText.text = "STEP 3 / 5";

            var back = CreateButton("BtnBack", header.transform, font, "← 返回", false);
            back.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.68f);
            back.GetComponent<RectTransform>().anchorMax = new Vector2(0.24f, 0.96f);
            back.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            back.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var title = CreateText("Title", header.transform, font, 82, FontStyle.Bold, UITheme.Text);
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.28f);
            title.rectTransform.anchorMax = new Vector2(0.94f, 0.68f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.alignment = TextAnchor.MiddleCenter;
            title.text = "选择你的省市";

            var subtitle = CreateText("Subtitle", header.transform, font, 40, FontStyle.Normal, UITheme.TextLight);
            subtitle.rectTransform.anchorMin = new Vector2(0.04f, 0f);
            subtitle.rectTransform.anchorMax = new Vector2(0.96f, 0.28f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.text = "不同地区采用不同高考模式和难度";

            var body = CreateRect("ScreenBody", panel);
            body.anchorMin = new Vector2(0f, 0.20f);
            body.anchorMax = new Vector2(1f, 0.78f);
            body.offsetMin = Vector2.zero;
            body.offsetMax = Vector2.zero;

            var scrollRoot = CreateRect("ScrollRoot", body);
            Stretch(scrollRoot);
            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            var viewport = CreateRect("Viewport", scrollRoot);
            Stretch(viewport);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.spacing = 24;
            vlg.padding = new RectOffset(0, 0, 0, 24);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = content;

            var hotTitleRow = CreateRect("HotTitleRow", content);
            hotTitleRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            var hotTitleLayout = hotTitleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hotTitleLayout.childAlignment = TextAnchor.MiddleLeft;
            hotTitleLayout.spacing = 12f;
            hotTitleLayout.childControlHeight = true;
            hotTitleLayout.childControlWidth = false;
            hotTitleLayout.childForceExpandHeight = true;
            hotTitleLayout.childForceExpandWidth = false;
            var hotTitle = CreateText("HotTitle", hotTitleRow, font, 30, FontStyle.Bold, UITheme.TextLight);
            hotTitle.alignment = TextAnchor.MiddleLeft;
            hotTitle.text = "热门城市";
            hotTitle.gameObject.AddComponent<LayoutElement>().preferredWidth = 220f;
            var hotTag = CreateImage("HotTag", hotTitleRow, UITheme.Accent);
            hotTag.gameObject.AddComponent<UiAutoRounded>();
            hotTag.gameObject.AddComponent<LayoutElement>().preferredWidth = 104f;
            var hotTagText = CreateText("Text", hotTag.transform, font, 20, FontStyle.Bold, Color.white);
            Stretch(hotTagText.rectTransform);
            hotTagText.alignment = TextAnchor.MiddleCenter;
            hotTagText.text = "推荐";

            var hotList = CreateRect("HotList", content);
            var hotGrid = hotList.gameObject.AddComponent<GridLayoutGroup>();
            hotGrid.cellSize = new Vector2(520f, 172f);
            hotGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            hotGrid.constraintCount = 2;
            hotGrid.spacing = new Vector2(24f, 24f);
            hotGrid.childAlignment = TextAnchor.UpperCenter;
            hotGrid.padding = new RectOffset(4, 4, 4, 4);
            hotList.gameObject.AddComponent<LayoutElement>().preferredHeight = 368f;

            var allTitleRow = CreateRect("AllTitleRow", content);
            allTitleRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            var allTitle = CreateText("AllTitle", allTitleRow, font, 30, FontStyle.Bold, UITheme.TextLight);
            Stretch(allTitle.rectTransform);
            allTitle.alignment = TextAnchor.MiddleLeft;
            allTitle.text = "全部省市";

            var allList = CreateRect("AllList", content);
            var allGrid = allList.gameObject.AddComponent<GridLayoutGroup>();
            allGrid.cellSize = new Vector2(520f, 172f);
            allGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            allGrid.constraintCount = 2;
            allGrid.spacing = new Vector2(24f, 24f);
            allGrid.childAlignment = TextAnchor.UpperCenter;
            allGrid.padding = new RectOffset(4, 4, 4, 4);
            allList.gameObject.AddComponent<LayoutElement>().preferredHeight = 1500f;

            var footer = CreateRect("ScreenFooter", panel);
            footer.anchorMin = new Vector2(0f, 0.04f);
            footer.anchorMax = new Vector2(1f, 0.18f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;

            var infoPanel = CreateImage("ProvinceInfoPanel", footer, UITheme.CardSky);
            infoPanel.gameObject.AddComponent<UiAutoRounded>();
            infoPanel.rectTransform.anchorMin = new Vector2(0.02f, 0.46f);
            infoPanel.rectTransform.anchorMax = new Vector2(0.98f, 1f);
            infoPanel.rectTransform.offsetMin = Vector2.zero;
            infoPanel.rectTransform.offsetMax = Vector2.zero;
            infoPanel.gameObject.SetActive(false);

            var infoName = CreateText("InfoName", infoPanel.transform, font, 34, FontStyle.Bold, UITheme.Text);
            infoName.rectTransform.anchorMin = new Vector2(0.04f, 0.56f);
            infoName.rectTransform.anchorMax = new Vector2(0.52f, 0.92f);
            infoName.rectTransform.offsetMin = Vector2.zero;
            infoName.rectTransform.offsetMax = Vector2.zero;
            infoName.alignment = TextAnchor.MiddleLeft;
            infoName.text = "北京 · 轻松开局";

            var infoModeChip = CreateImage("InfoModeChip", infoPanel.transform, UITheme.Confirm);
            infoModeChip.gameObject.AddComponent<UiAutoRounded>();
            infoModeChip.rectTransform.anchorMin = new Vector2(0.56f, 0.56f);
            infoModeChip.rectTransform.anchorMax = new Vector2(0.94f, 0.92f);
            infoModeChip.rectTransform.offsetMin = Vector2.zero;
            infoModeChip.rectTransform.offsetMax = Vector2.zero;
            var infoModeText = CreateText("InfoModeText", infoModeChip.transform, font, 24, FontStyle.Bold, Color.white);
            Stretch(infoModeText.rectTransform);
            infoModeText.alignment = TextAnchor.MiddleCenter;
            infoModeText.text = "新高考模式 3+1+2";

            var infoDiff = CreateText("InfoDiff", infoPanel.transform, font, 24, FontStyle.Bold, UITheme.TextSoft);
            infoDiff.rectTransform.anchorMin = new Vector2(0.04f, 0.24f);
            infoDiff.rectTransform.anchorMax = new Vector2(0.94f, 0.50f);
            infoDiff.rectTransform.offsetMin = Vector2.zero;
            infoDiff.rectTransform.offsetMax = Vector2.zero;
            infoDiff.alignment = TextAnchor.MiddleLeft;
            infoDiff.text = "难度系数 1.00 · 整体处于常规强度";

            var infoDesc = CreateText("InfoDesc", infoPanel.transform, font, 24, FontStyle.Normal, UITheme.TextLight);
            infoDesc.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
            infoDesc.rectTransform.anchorMax = new Vector2(0.94f, 0.24f);
            infoDesc.rectTransform.offsetMin = Vector2.zero;
            infoDesc.rectTransform.offsetMax = Vector2.zero;
            infoDesc.alignment = TextAnchor.UpperLeft;
            infoDesc.text = "语数外固定，首选1门，再选2门，更接近多数省份玩法。";

            var confirm = CreateButton("BtnConfirm", footer, font, "确认进入选科 →", true);
            confirm.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0f);
            confirm.GetComponent<RectTransform>().anchorMax = new Vector2(0.98f, 0.38f);
            confirm.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            confirm.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var so = new SerializedObject(screen);
            so.FindProperty("backButton").objectReferenceValue = back;
            so.FindProperty("confirmButton").objectReferenceValue = confirm;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("subtitleText").objectReferenceValue = subtitle;
            so.FindProperty("hotListRoot").objectReferenceValue = hotList;
            so.FindProperty("allListRoot").objectReferenceValue = allList;
            so.FindProperty("infoPanelRoot").objectReferenceValue = infoPanel.rectTransform;
            so.FindProperty("infoNameText").objectReferenceValue = infoName;
            so.FindProperty("infoModeChipImage").objectReferenceValue = infoModeChip;
            so.FindProperty("infoModeText").objectReferenceValue = infoModeText;
            so.FindProperty("infoDiffText").objectReferenceValue = infoDiff;
            so.FindProperty("infoDescText").objectReferenceValue = infoDesc;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ProvincePrefabPath);
        }

        private static void CreateStatItem(RectTransform parent, Font font, string label, string icon, Vector2 min, Vector2 max, out Image fillImage, out Text valueText)
        {
            var item = CreateImage($"Stat_{label}", parent.transform, new Color32(250, 250, 250, 255));
            item.gameObject.AddComponent<UiAutoRounded>();
            item.rectTransform.anchorMin = min;
            item.rectTransform.anchorMax = max;
            item.rectTransform.offsetMin = Vector2.zero;
            item.rectTransform.offsetMax = Vector2.zero;

            var iconText = CreateText("Icon", item.transform, font, 34, FontStyle.Bold, UITheme.TextSoft);
            iconText.rectTransform.anchorMin = new Vector2(0.06f, 0.60f);
            iconText.rectTransform.anchorMax = new Vector2(0.26f, 0.94f);
            iconText.rectTransform.offsetMin = Vector2.zero;
            iconText.rectTransform.offsetMax = Vector2.zero;
            iconText.alignment = TextAnchor.MiddleLeft;
            iconText.text = icon;

            var labelText = CreateText("Label", item.transform, font, 32, FontStyle.Bold, UITheme.TextSoft);
            labelText.rectTransform.anchorMin = new Vector2(0.24f, 0.60f);
            labelText.rectTransform.anchorMax = new Vector2(0.60f, 0.94f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.text = label;

            valueText = CreateText("Value", item.transform, font, 30, FontStyle.Bold, UITheme.TextLight);
            valueText.rectTransform.anchorMin = new Vector2(0.60f, 0.60f);
            valueText.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);
            valueText.rectTransform.offsetMin = Vector2.zero;
            valueText.rectTransform.offsetMax = Vector2.zero;
            valueText.alignment = TextAnchor.MiddleRight;
            valueText.text = "0/100";

            var barBg = CreateImage("BarBg", item.transform, new Color32(238, 238, 238, 255));
            barBg.gameObject.AddComponent<UiAutoRounded>();
            barBg.rectTransform.anchorMin = new Vector2(0.06f, 0.16f);
            barBg.rectTransform.anchorMax = new Vector2(0.94f, 0.42f);
            barBg.rectTransform.offsetMin = Vector2.zero;
            barBg.rectTransform.offsetMax = Vector2.zero;

            fillImage = CreateImage("BarFill", barBg.transform, Color.white);
            fillImage.gameObject.AddComponent<UiAutoRounded>();
            Stretch(fillImage.rectTransform);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0f;
        }

        private static void GenerateDots(RectTransform parent, int count)
        {
            var rand = new System.Random(1337);
            for (int i = 0; i < count; i++)
            {
                var dot = CreateImage($"Dot_{i}", parent, new Color32(255, 255, 255, 255));
                dot.gameObject.AddComponent<UiAutoRounded>();
                dot.color = new Color32(255, 255, 255, 255);
                dot.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                dot.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                dot.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                var size = (float)(rand.NextDouble() * 48 + 18);
                dot.rectTransform.sizeDelta = new Vector2(size, size);
                var px = (float)(rand.NextDouble() * 960 - 480);
                var py = (float)(rand.NextDouble() * 2080 - 1040);
                dot.rectTransform.anchoredPosition = new Vector2(px, py);

                var tint = i % 3 == 0 ? UITheme.CardSky : (i % 3 == 1 ? UITheme.CardPeach : UITheme.CardLavender);
                dot.color = new Color32(tint.r, tint.g, tint.b, 42);

                dot.gameObject.AddComponent<UiFloatBob>().Configure((float)(rand.NextDouble() * 10 + 6), (float)(rand.NextDouble() * 0.5 + 0.35), (float)rand.NextDouble());
            }
        }

        private static void CreateFloatingSticker(RectTransform parent, Font font, string icon, Vector2 anchor, Color bg)
        {
            var sticker = CreateImage($"Sticker_{icon}", parent, bg);
            sticker.gameObject.AddComponent<UiAutoRounded>();
            sticker.rectTransform.anchorMin = anchor;
            sticker.rectTransform.anchorMax = anchor;
            sticker.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            sticker.rectTransform.sizeDelta = new Vector2(92f, 92f);
            sticker.rectTransform.anchoredPosition = Vector2.zero;

            var text = CreateText("Text", sticker.transform, font, 56, FontStyle.Bold, UITheme.Text);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = icon;

            sticker.gameObject.AddComponent<UiFloatBob>().Configure(UITheme.ScaleY(8f), 0.45f, UnityEngine.Random.value);
        }

        private static Button CreateButton(string name, Transform parent, Font font, string label, bool primary)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.localScale = Vector3.one;
            var image = buttonGo.GetComponent<Image>();
            image.color = Color.white;
            image.gameObject.AddComponent<UiAutoRounded>();

            if (primary)
            {
                var grad = buttonGo.AddComponent<UiCornerGradient>();
                grad.SetColors(UITheme.Confirm, UITheme.ConfirmHover, UITheme.ConfirmHover, UITheme.Confirm);
                var shadow = buttonGo.AddComponent<Shadow>();
                shadow.effectColor = new Color(UITheme.Confirm.r / 255f, UITheme.Confirm.g / 255f, UITheme.Confirm.b / 255f, 0.35f);
                shadow.effectDistance = new Vector2(0, -12);
            }
            else
            {
                var outline = buttonGo.AddComponent<Outline>();
                outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
                outline.effectDistance = new Vector2(4, -4);
            }

            var text = CreateText("Text", buttonGo.transform, font, 54, FontStyle.Bold, primary ? Color.white : UITheme.Text);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            buttonGo.AddComponent<UiPressScale>();
            return buttonGo.GetComponent<Button>();
        }

        private static Button CreateCardButton(string name, Transform parent, Font font, string label, Color accentBg)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.localScale = Vector3.one;
            var image = buttonGo.GetComponent<Image>();
            image.color = Color.white;
            buttonGo.AddComponent<UiAutoRounded>();

            var iconWrap = CreateImage("IconWrap", buttonGo.transform, accentBg);
            iconWrap.gameObject.AddComponent<UiAutoRounded>();
            iconWrap.rectTransform.anchorMin = new Vector2(0.22f, 0.44f);
            iconWrap.rectTransform.anchorMax = new Vector2(0.78f, 0.88f);
            iconWrap.rectTransform.offsetMin = Vector2.zero;
            iconWrap.rectTransform.offsetMax = Vector2.zero;
            var iconText = CreateText("Text", iconWrap.transform, font, 82, FontStyle.Bold, UITheme.Text);
            Stretch(iconText.rectTransform);
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.text = label == "男生" ? "🙋" : "🙋";

            var labelText = CreateText("Label", buttonGo.transform, font, 46, FontStyle.Bold, UITheme.Text);
            labelText.rectTransform.anchorMin = new Vector2(0.10f, 0.12f);
            labelText.rectTransform.anchorMax = new Vector2(0.90f, 0.34f);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = label;

            buttonGo.AddComponent<UiPressScale>();
            buttonGo.AddComponent<UiFloatBob>().Configure(6f, 0.45f, UnityEngine.Random.value);
            return buttonGo.GetComponent<Button>();
        }

        private static Button CreateIconButton(string name, Transform parent, Font font, string label)
        {
            var btn = CreateButton(name, parent, font, label, false);
            var text = btn.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.fontSize = 56;
            }
            return btn;
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, Color color)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            root.SetActive(true);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}

