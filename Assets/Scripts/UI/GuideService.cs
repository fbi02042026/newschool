using GaokaoSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    public static class GuideService
    {
        public static void EnsureHelpButtonUnderTitle(Transform screenRoot, string titlePath, string buttonName, System.Action onClick)
        {
            if (screenRoot == null)
            {
                return;
            }

            var title = screenRoot.Find(titlePath) as RectTransform;
            if (title == null)
            {
                EnsureHelpButton(screenRoot, buttonName, onClick);
                return;
            }

            EnsureHelpButtonUnderTitle(title, buttonName, onClick);
        }

        public static void EnsureHelpButtonUnderTitle(RectTransform titleRect, string buttonName, System.Action onClick)
        {
            if (titleRect == null)
            {
                return;
            }

            var existing = titleRect.Find(buttonName) as RectTransform;
            Button button;

            if (existing == null)
            {
                var go = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
                existing = go.GetComponent<RectTransform>();
                existing.SetParent(titleRect, false);
                existing.localScale = Vector3.one;
                existing.anchorMin = new Vector2(0.5f, 0f);
                existing.anchorMax = new Vector2(0.5f, 0f);
                existing.pivot = new Vector2(0.5f, 1f);
                existing.sizeDelta = new Vector2(66f, 66f);
                existing.anchoredPosition = new Vector2(0f, -16f);

                var image = go.GetComponent<Image>();
                image.color = new Color32(255, 255, 255, 240);
                go.AddComponent<UI.Effects.UiAutoRounded>();
                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
                outline.effectDistance = new Vector2(3f, -3f);
                go.AddComponent<UI.Effects.UiPressScale>();

                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var labelGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                var labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.SetParent(go.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var text = labelGo.GetComponent<Text>();
                text.font = font;
                text.fontSize = 34;
                text.fontStyle = FontStyle.Bold;
                text.color = UITheme.Text;
                text.alignment = TextAnchor.MiddleCenter;
                text.supportRichText = false;
                text.text = "!";

                button = go.GetComponent<Button>();
            }
            else
            {
                button = existing.GetComponent<Button>();
                if (button == null)
                {
                    button = existing.gameObject.AddComponent<Button>();
                }
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        public static void EnsureHelpButton(Transform parent, string buttonName, System.Action onClick)
        {
            if (parent == null)
            {
                return;
            }

            var existing = parent.Find(buttonName) as RectTransform;
            Button button;

            if (existing == null)
            {
                var go = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
                existing = go.GetComponent<RectTransform>();
                existing.SetParent(parent, false);
                existing.localScale = Vector3.one;

                var image = go.GetComponent<Image>();
                image.color = new Color32(255, 255, 255, 240);
                go.AddComponent<UI.Effects.UiAutoRounded>();
                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color32(UITheme.Border.r, UITheme.Border.g, UITheme.Border.b, 255);
                outline.effectDistance = new Vector2(3f, -3f);
                go.AddComponent<UI.Effects.UiPressScale>();

                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var labelGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                var labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.SetParent(go.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var text = labelGo.GetComponent<Text>();
                text.font = font;
                text.fontSize = 42;
                text.fontStyle = FontStyle.Bold;
                text.color = UITheme.Text;
                text.alignment = TextAnchor.MiddleCenter;
                text.supportRichText = false;
                text.text = "?";

                button = go.GetComponent<Button>();
            }
            else
            {
                button = existing.GetComponent<Button>();
                if (button == null)
                {
                    button = existing.gameObject.AddComponent<Button>();
                }
            }

            var scopeRoot = parent.GetComponentInParent<ScreenBase>() != null
                ? parent.GetComponentInParent<ScreenBase>().transform
                : parent.root;
            RemoveDuplicateButtons(scopeRoot, buttonName, existing);

            existing.anchorMin = new Vector2(0.88f, 0.72f);
            existing.anchorMax = new Vector2(0.98f, 0.96f);
            existing.offsetMin = Vector2.zero;
            existing.offsetMax = Vector2.zero;

            var label = existing.Find("Text")?.GetComponent<Text>();
            if (label != null)
            {
                label.fontSize = 42;
                label.text = "?";
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        private static void RemoveDuplicateButtons(Transform root, string buttonName, RectTransform keep)
        {
            if (root == null || string.IsNullOrWhiteSpace(buttonName))
            {
                return;
            }

            var children = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null || child == keep)
                {
                    continue;
                }

                if (child.name == buttonName)
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        public static void TryShowOnce(ScreenType screenType, Transform screenRoot)
        {
            var state = GameState.Instance;
            if (state == null || state.CurrentPlaythrough != 1)
            {
                return;
            }

            var key = $"guide:{screenType}";
            if (state.HasSeenGuide(key))
            {
                return;
            }

            var steps = BuildSteps(screenType);
            if (steps == null || steps.Length == 0)
            {
                return;
            }

            state.MarkGuideSeen(key);
            GuideDialog.Show(screenRoot, key, steps);
        }

        public static void Open(ScreenType screenType, Transform screenRoot)
        {
            var steps = BuildSteps(screenType);
            if (steps == null || steps.Length == 0)
            {
                return;
            }

            GuideDialog.Show(screenRoot, $"guide:{screenType}", steps);
        }

        private static GuideStep[] BuildSteps(ScreenType screenType)
        {
            var state = GameState.Instance;
            var tone = state != null && state.GuideToneVariant >= 0 ? state.GuideToneVariant % 3 : 0;
            var playerAddress = GetPlayerAddress(state);

            switch (screenType)
            {
                case ScreenType.Launch:
                    return null;
                case ScreenType.Profile:
                    return BuildProfileSteps(tone);
                case ScreenType.Family:
                    return BuildFamilySteps(tone, playerAddress);
                case ScreenType.Province:
                    return BuildProvinceSteps(tone, playerAddress);
                case ScreenType.Subject:
                    return BuildSubjectSteps(tone, playerAddress);
                default:
                    return null;
            }
        }

        private static GuideStep[] BuildProfileSteps(int tone)
        {
            switch (tone)
            {
                case 1:
                    return new[]
                    {
                        new GuideStep("初次见面", "嗨，同学好，第一次见面，我是你的重生小助手小灯。\n先让我认识一下你，请问同学怎么称呼？"),
                        new GuideStep("先做这一步", "这一屏只用选形象和名字。\n想偷个懒的话，也可以点随机名字。")
                    };
                case 2:
                    return new[]
                    {
                        new GuideStep("小灯报到", "哼，我可不是特意来等你的。\n既然已经开始重生了，就先把名字和形象定下来吧。"),
                        new GuideStep("别紧张", "这一步不会影响难度，只是先把你的身份立住。\n准备好后就继续。")
                    };
                default:
                    return new[]
                    {
                        new GuideStep("初次见面", "嗨，同学好，我是你的重生小助手小灯。\n先告诉我，你想让我怎么称呼你呀？"),
                        new GuideStep("很快就好", "选好形象、定好名字，我们就进入下一步。\n后面的家庭背景会更影响开局。")
                    };
            }
        }

        private static GuideStep[] BuildFamilySteps(int tone, string playerAddress)
        {
            switch (tone)
            {
                case 1:
                    return new[]
                    {
                        new GuideStep("继续出发", $"{playerAddress}，太好了，真是个好名字。\n接下来会随机出你的家庭背景。"),
                        new GuideStep("抽卡小提示", "如果你不满意这次结果，可以免费重置一次。\n先看看新旧对比，再决定要不要换。")
                    };
                case 2:
                    return new[]
                    {
                        new GuideStep("下一站", $"{playerAddress}，名字还挺不错的嘛。\n接下来轮到家庭背景出场了，这会影响你的开局资源。"),
                        new GuideStep("别急着纠结", "你还有一次免费重置机会。\n不顺眼就刷一次，顺眼就直接出发。")
                    };
                default:
                    return new[]
                    {
                        new GuideStep("继续出发", $"{playerAddress}，太好了，真是个好名字。\n接下来会随机出你的家庭背景哦。"),
                        new GuideStep("抽卡小提示", "如果你不满意，可以免费重置一次。\n看完左右对比，再决定保留哪一个就好。")
                    };
            }
        }

        private static GuideStep[] BuildProvinceSteps(int tone, string playerAddress)
        {
            switch (tone)
            {
                case 1:
                    return new[]
                    {
                        new GuideStep("下一步是省份", $"{playerAddress}，这一页我们先决定开局省份。\n它会影响考试规则和整体难度。"),
                        new GuideStep("怎么选更清楚", "上面是热门 4 个城市，下面是全部省市。\n点考试模式标签还能看对应规则说明。")
                    };
                case 2:
                    return new[]
                    {
                        new GuideStep("省份要认真选", $"{playerAddress}，这一步可别乱点。\n省份一变，考试模式和难度都会跟着变。"),
                        new GuideStep("记住这两个点", "热门城市固定在上面，全省列表在下面。\n选科细节别急，我们下一屏再聊。")
                    };
                default:
                    return new[]
                    {
                        new GuideStep("下一步是省份", $"{playerAddress}，接下来先决定你的开局战场。\n不同省份会对应不同考试模式和难度。"),
                        new GuideStep("怎么选更清楚", "上面是热门 4 个城市，下面是全部省市。\n点模式标签可以看规则说明，选科我们下一屏再聊。")
                    };
            }
        }

        private static GuideStep[] BuildSubjectSteps(int tone, string playerAddress)
        {
            switch (tone)
            {
                case 1:
                    return new[]
                    {
                        new GuideStep("选科怎么想", $"{playerAddress}，这一步决定你后续会遇到哪些任务线，也会影响能选的专业范围。\n先选 1 门首选，再选 2 门再选就好。"),
                        new GuideStep("不确定就问专家", "如果你不知道怎么选，可以点右上角的“专家”图标。\n按你喜欢的方向点一条，它会给你一组更适合的推荐。")
                    };
                case 2:
                    return new[]
                    {
                        new GuideStep("别乱点", $"{playerAddress}，选科不是玄学，但也别硬选。\n选你更擅长、也更愿意长期投入的方向。"),
                        new GuideStep("偷懒办法", "不想纠结就点右上角专家。\n按你喜欢的方向选一条，它会直接给你推荐组合。")
                    };
                default:
                    return new[]
                    {
                        new GuideStep("选科怎么想", $"{playerAddress}，这一步会影响你的学习节奏和后续任务线。\n先选首选（物理/历史），再选两门再选科。"),
                        new GuideStep("不会选也没关系", "点右上角专家图标。\n选你喜欢的方向，它会给你更合适的科目组合。")
                    };
            }
        }

        private static string GetPlayerAddress(GameState state)
        {
            var name = state != null ? state.PlayerName : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                return "同学";
            }

            return $"{name}同学";
        }
    }
}

