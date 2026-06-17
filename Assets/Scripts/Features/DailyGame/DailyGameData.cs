using System;
using System.Collections.Generic;
using UnityEngine;

namespace GaokaoSimulator.Features.DailyGame
{
    public static class DailyGameData
    {
        public const int TOTAL_EVENTS = 5;
        public const int TOTAL_STREAMS = 31;

        private static List<GameEvent> allEvents;
        private static List<StreamItem> allStreams;
        private static List<StreamItem> endStreams;

        static DailyGameData()
        {
            InitializeDefaultEvents();
            InitializeDefaultStreams();
            InitializeEndStreams();
        }

        #region Opening / Closing

        public static string GetOpeningText()
        {
            return "🌅 新的一天开始了...";
        }

        public static string GetNightClosing(int dayIndex)
        {
            return $"第 {dayIndex} 天，结束了。";
        }

        public static Sprite GetDayBackground(int dayIndex, int energy)
        {
            // 白天背景用 default（project expects sprite by path)
            return null;
        }

        #endregion

        #region Events (V6.9.3: 5 fixed events, with dialog + portrait + option name + best choice)

        private static void InitializeDefaultEvents()
        {
            allEvents = new List<GameEvent>
            {
                new GameEvent
                {
                    Id = "ev_run",
                    Name = "早操",
                    Time = "07:30",
                    Difficulty = 2,
                    Category = "行",
                    Narrator = "旁白",
                    PortraitPath = "",
                    Dialog = "体育委员突然喊道：\n'今天要测800米！'\n全班一阵哀嚎。",
                    Options = new List<EventOption>
                    {
                        new EventOption {
                            Label = "全力冲刺",
                            Name = "全力冲刺",
                            Emoji = "💨",
                            IsBest = true,
                            EnergyCost = 15,
                            Effects = new Dictionary<string, int> { { "Health", 6 }, { "Psychology", 2 } },
                            ResultText = "💪 你跟紧队伍节奏跑完了全程，虽然喘着粗气，但腿脚利索了不少。\n\n体质+6 心态+2",
                            ResultComment = "体育委员拍了拍你的肩膀：'好样的！'"
                        },
                        new EventOption {
                            Label = "保持匀速",
                            Name = "保持匀速",
                            Emoji = "🚶",
                            IsBest = false,
                            EnergyCost = 8,
                            Effects = new Dictionary<string, int> { { "Health", 3 }, { "Psychology", 1 } },
                            ResultText = "👌 你匀速完成了全程，虽然不快，但是坚持了下来。\n\n体质+3 心态+1",
                            ResultComment = "你深呼吸调整节奏，完成就是胜利。"
                        },
                        new EventOption {
                            Label = "偷工减料",
                            Name = "偷工减料",
                            Emoji = "😶",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Psychology", -1 }, { "Health", 0 } },
                            ResultText = "😅 你躲在队伍后面放慢脚步，结果还是被发现。\n\n心态-1",
                            ResultComment = "体育老师批评了两句，你脸有点发烫。"
                        },
                        new EventOption {
                            Label = "请假休息",
                            Name = "请假休息",
                            Emoji = "🤒",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Energy", -10 } },
                            ResultText = "😴 你跟老师说不舒服申请休息，老师同意了。\n\n今日精力上限-10",
                            ResultComment = "你坐在看台上吹着风，今天少了许多锻炼。"
                        }
                    }
                },
                new GameEvent
                {
                    Id = "ev_math",
                    Name = "数学课",
                    Time = "10:00",
                    Difficulty = 2,
                    Category = "思",
                    Narrator = "数学老师",
                    PortraitPath = "NPC/teacher",
                    Dialog = "谁来试试？\n这题不难，但挺考思路。",
                    Options = new List<EventOption>
                    {
                        new EventOption {
                            Label = "勇敢举手",
                            Name = "勇敢举手",
                            Emoji = "✋",
                            IsBest = true,
                            EnergyCost = 15,
                            Effects = new Dictionary<string, int> { { "Intelligence", 6 }, { "Psychology", 2 } },
                            ResultText = "🎯 你站起来走上讲台，三两下解出了题目。\n\n智力+6 心态+2",
                            ResultComment = "数学老师难得露出微笑：'思路清晰，好！'"
                        },
                        new EventOption {
                            Label = "试试半解",
                            Name = "试试半解",
                            Emoji = "🤔",
                            IsBest = false,
                            EnergyCost = 8,
                            Effects = new Dictionary<string, int> { { "Intelligence", 3 }, { "Psychology", 1 } },
                            ResultText = "📝 你上台只说出一半思路，老师说'方向对了，继续。'\n\n智力+3 心态+1",
                            ResultComment = "虽然没写完，但至少尝试过了。"
                        },
                        new EventOption {
                            Label = "低头不答",
                            Name = "低头不答",
                            Emoji = "🙈",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Psychology", -1 } },
                            ResultText = "🙅 你假装低头翻书，希望别被点名。\n\n心态-1",
                            ResultComment = "老师点了别人，你松了口气但也有点失落。"
                        },
                        new EventOption {
                            Label = "抄同桌思路",
                            Name = "抄同桌思路",
                            Emoji = "👀",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Intelligence", 1 }, { "Psychology", -1 } },
                            ResultText = "👓 你抄了同桌答案，站起来说了出来。\n\n智力+1 心态-1",
                            ResultComment = "对了，但你其实没搞懂是怎么回事。"
                        }
                    }
                },
                new GameEvent
                {
                    Id = "ev_lunch",
                    Name = "午饭",
                    Time = "12:30",
                    Difficulty = 1,
                    Category = "行",
                    Narrator = "同班同学",
                    PortraitPath = "NPC/classmate",
                    Dialog = "这里有人吗？\n我们可以坐这里吗？",
                    Options = new List<EventOption>
                    {
                        new EventOption {
                            Label = "热情让座",
                            Name = "热情让座",
                            Emoji = "🤝",
                            IsBest = true,
                            EnergyCost = 15,
                            Effects = new Dictionary<string, int> { { "Social", 6 }, { "Psychology", 2 } },
                            ResultText = "😊 你大方招呼他们坐下，还一起聊了聊课程。\n\n社交+6 心态+2",
                            ResultComment = "他们很高兴，以后经常会找你一起吃饭。"
                        },
                        new EventOption {
                            Label = "挤一挤一起坐",
                            Name = "挤一挤一起坐",
                            Emoji = "👍",
                            IsBest = false,
                            EnergyCost = 8,
                            Effects = new Dictionary<string, int> { { "Social", 3 }, { "Psychology", 1 } },
                            ResultText = "👥 你往里挪了挪，挤着坐下，大家一起聊天。\n\n社交+3 心态+1",
                            ResultComment = "虽然挤了点，气氛还算融洽。"
                        },
                        new EventOption {
                            Label = "说已经有人了",
                            Name = "说已经有人了",
                            Emoji = "🚫",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Social", -1 }, { "Psychology", -1 } },
                            ResultText = "😐 你说这个位置有人了，他们失望地走开。\n\n社交-1 心态-1",
                            ResultComment = "你继续一个人吃饭，但心里有点不舒服。"
                        },
                        new EventOption {
                            Label = "叫他们请奶茶",
                            Name = "叫他们请奶茶",
                            Emoji = "🧋",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Social", 2 }, { "Psychology", -1 } },
                            ResultText = "😏 你开玩笑说下次请奶茶就让坐，他们笑了。\n\n社交+2 心态-1",
                            ResultComment = "气氛轻松，但人情欠了一点点。"
                        }
                    }
                },
                new GameEvent
                {
                    Id = "ev_pe",
                    Name = "体育课",
                    Time = "16:00",
                    Difficulty = 1,
                    Category = "行",
                    Narrator = "队友",
                    PortraitPath = "NPC/teammate",
                    Dialog = "三对三缺一个，来不来？\n打半场就行。",
                    Options = new List<EventOption>
                    {
                        new EventOption {
                            Label = "直接上场",
                            Name = "直接上场",
                            Emoji = "🏀",
                            IsBest = true,
                            EnergyCost = 15,
                            Effects = new Dictionary<string, int> { { "Health", 6 }, { "Social", 3 } },
                            ResultText = "🏃 你加入比赛，配合默契，投进好几个球。\n\n体质+6 社交+3",
                            ResultComment = "队友们对你击掌：'传得好！'"
                        },
                        new EventOption {
                            Label = "替补帮忙",
                            Name = "替补帮忙",
                            Emoji = "🥅",
                            IsBest = false,
                            EnergyCost = 8,
                            Effects = new Dictionary<string, int> { { "Health", 3 }, { "Social", 1 } },
                            ResultText = "👟 你在场边替补上场，打了十几分钟。\n\n体质+3 社交+1",
                            ResultComment = "轻松活动一下，不耽误事儿。"
                        },
                        new EventOption {
                            Label = "树下歇着",
                            Name = "树下歇着",
                            Emoji = "🌳",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { },
                            ResultText = "🍃 你坐在树荫下看大家打球，吹着风很舒服。\n\n无属性变化",
                            ResultComment = "难得的放松，今天就这样吧。"
                        },
                        new EventOption {
                            Label = "说不舒服",
                            Name = "说不舒服",
                            Emoji = "😥",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Energy", -5 } },
                            ResultText = "😮‍💨 你说有点不舒服不打了，队友理解点头。\n\n今日精力上限-5",
                            ResultComment = "你保存了体力，但少了一场锻炼。"
                        }
                    }
                },
                new GameEvent
                {
                    Id = "ev_night",
                    Name = "晚自习",
                    Time = "20:30",
                    Difficulty = 2,
                    Category = "思",
                    Narrator = "班主任",
                    PortraitPath = "NPC/headteacher",
                    Dialog = "我明天检查作业。\n没做完的今晚必须完成！",
                    Options = new List<EventOption>
                    {
                        new EventOption {
                            Label = "早就完成",
                            Name = "早就完成",
                            Emoji = "✅",
                            IsBest = true,
                            EnergyCost = 15,
                            Effects = new Dictionary<string, int> { { "Intelligence", 6 }, { "Psychology", 3 } },
                            ResultText = "📖 你翻开作业，早就写完了，可以安心刷题。\n\n智力+6 心态+3",
                            ResultComment = "班主任经过你座位，满意点点头。"
                        },
                        new EventOption {
                            Label = "连夜赶完",
                            Name = "连夜赶完",
                            Emoji = "✍️",
                            IsBest = false,
                            EnergyCost = 8,
                            Effects = new Dictionary<string, int> { { "Intelligence", 3 }, { "Energy", -5 } },
                            ResultText = "⌛ 你埋头赶了半小时，把重要的补完了。\n\n智力+3 今日精力上限-5",
                            ResultComment = "累但踏实，明天不会被批评。"
                        },
                        new EventOption {
                            Label = "抄同桌答案",
                            Name = "抄同桌答案",
                            Emoji = "📋",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Psychology", -2 }, { "Intelligence", 1 } },
                            ResultText = "📃 你借了同桌作业飞快抄完，心里有点虚。\n\n智力+1 心态-2",
                            ResultComment = "应付过去了，但你其实什么也没学会。"
                        },
                        new EventOption {
                            Label = "算了明天再说",
                            Name = "算了明天再说",
                            Emoji = "😴",
                            IsBest = false,
                            EnergyCost = 0,
                            Effects = new Dictionary<string, int> { { "Psychology", -3 }, { "Intelligence", 0 } },
                            ResultText = "💤 你心里打算明天早自习抄，越想越烦。\n\n心态-3",
                            ResultComment = "一晚上都没学进去，一直在焦虑。"
                        }
                    }
                }
            };
        }

        #endregion

        #region Streams

        private static void InitializeDefaultStreams()
        {
            allStreams = new List<StreamItem>
            {
                new StreamItem { Time = "07:00", Text = "闹钟响了3遍你才起床，匆匆忙忙塞了两口面包。", EventId = "" },
                new StreamItem { Time = "07:20", Text = "早自习的铃声响了，你抱着课本跑进教室，差点迟到。", EventId = "" },
                new StreamItem { Time = "07:30", Text = "早操集合哨响了，操场上一片脚步声。", EventId = "ev_run" },
                new StreamItem { Time = "08:00", Text = "早读是英语，你跟着录音跟读了一会儿，舌头有点打结。", EventId = "" },
                new StreamItem { Time = "08:30", Text = "第一节课开始了，老师在讲台上滔滔不绝地讲着。", EventId = "" },
                new StreamItem { Time = "09:00", Text = "窗外的阳光透过树叶洒进来，几个同学开始打哈欠。", EventId = "" },
                new StreamItem { Time = "09:10", Text = "课间休息，同学们三三两两聚在一起聊天吃零食。", EventId = "" },
                new StreamItem { Time = "09:40", Text = "第二节课继续，老师的声音像催眠曲一样。", EventId = "" },
                new StreamItem { Time = "10:00", Text = "数学老师走进教室，推了推眼镜，气氛突然紧张起来。", EventId = "ev_math" },
                new StreamItem { Time = "10:40", Text = "下课后你长舒一口气，同学们议论纷纷。", EventId = "" },
                new StreamItem { Time = "11:00", Text = "第三节课结束，你伸了个大大的懒腰，肚子开始咕咕叫。", EventId = "" },
                new StreamItem { Time = "11:30", Text = "放学铃响了，大家争先恐后涌向食堂。", EventId = "" },
                new StreamItem { Time = "12:00", Text = "你排了好长的队才买到饭，端着餐盘四处找位置。", EventId = "" },
                new StreamItem { Time = "12:30", Text = "食堂里人声鼎沸，热闹非凡。你刚坐下，就看到几个同学朝你走来。", EventId = "ev_lunch" },
                new StreamItem { Time = "13:00", Text = "午休时间，教室里安静下来，有人在趴桌睡觉。", EventId = "" },
                new StreamItem { Time = "13:30", Text = "下午第一节课开始了，你勉强打起精神。", EventId = "" },
                new StreamItem { Time = "14:15", Text = "窗外下起了小雨，雨声让人昏昏欲睡。", EventId = "" },
                new StreamItem { Time = "14:30", Text = "老师讲到一个有趣的话题，教室里气氛活跃起来。", EventId = "" },
                new StreamItem { Time = "15:10", Text = "课间操时间，大家在走廊上活动筋骨。", EventId = "" },
                new StreamItem { Time = "15:40", Text = "下午第三节课，老师布置了大量作业，教室里一片哀嚎。", EventId = "" },
                new StreamItem { Time = "16:00", Text = "体育课的时间到了！同学们欢呼着冲向操场。", EventId = "ev_pe" },
                new StreamItem { Time = "17:00", Text = "放学后，有些同学留在教室自习，有些直接回家了。", EventId = "" },
                new StreamItem { Time = "17:30", Text = "你回到家，放下书包，瘫在沙发上休息了一会儿。", EventId = "" },
                new StreamItem { Time = "18:00", Text = "晚饭时间，妈妈做了你爱吃的菜，一家人围在一起。", EventId = "" },
                new StreamItem { Time = "18:30", Text = "短暂的休息后，你开始写作业。", EventId = "" },
                new StreamItem { Time = "19:30", Text = "作业写了一半，眼睛有点酸，你揉了揉眼睛。", EventId = "" },
                new StreamItem { Time = "20:00", Text = "晚自习开始了，教室里灯火通明，大家都在埋头苦读。", EventId = "" },
                new StreamItem { Time = "20:30", Text = "班主任突然推门进来，教室里瞬间安静下来。", EventId = "ev_night" },
                new StreamItem { Time = "21:30", Text = "晚自习结束，你收拾书包，和同学一起走出教室。", EventId = "" },
                new StreamItem { Time = "22:00", Text = "回到家，你洗了个热水澡，感觉一天的疲惫都洗掉了。", EventId = "" },
                new StreamItem { Time = "22:30", Text = "躺在床上，你回想今天发生的事情，嘴角微微上扬。", EventId = "" }
            };
        }

        private static void InitializeEndStreams()
        {
            endStreams = new List<StreamItem>
            {
                new StreamItem { Time = "23:30", Text = "宿舍里安静下来，窗外的月光洒在地板上。", EventId = "" },
                new StreamItem { Time = "23:50", Text = "今天最后一缕光熄灭，枕头边传来室友均匀的呼吸声。", EventId = "" }
            };
        }

        #endregion

        #region Public API

        public static List<GameEvent> GetDailyEvents()
        {
            return new List<GameEvent>(allEvents);
        }

        public static GameEvent GetEventById(string id)
        {
            return allEvents.Find(e => e.Id == id);
        }

        public static List<StreamItem> GetDailyStreams()
        {
            return new List<StreamItem>(allStreams);
        }

        public static List<StreamItem> GetEndStreams()
        {
            return new List<StreamItem>(endStreams);
        }

        public static string GetEndQuote()
        {
            return "今天最后一缕光熄灭，枕头边传来室友的呼噜声...";
        }

        public static int GetEnergyMultiplier(int energy)
        {
            if (energy >= 100) return 120;
            if (energy >= 80) return 110;
            if (energy >= 50) return 100;
            return 80;
        }

        public static string GetEnergyZone(int energy)
        {
            if (energy >= 100) return "充沛";
            if (energy >= 80) return "良好";
            if (energy >= 50) return "一般";
            return "疲惫";
        }

        public static string GetEnergyEmoji(int energy)
        {
            if (energy >= 100) return "💪";
            if (energy >= 80) return "😊";
            if (energy >= 50) return "😐";
            return "😩";
        }

        public static Color GetEnergyColor(int energy)
        {
            if (energy >= 100) return new Color32(76, 175, 80, 255);
            if (energy >= 80) return new Color32(33, 150, 243, 255);
            if (energy >= 50) return new Color32(255, 152, 0, 255);
            return new Color32(244, 67, 54, 255);
        }

        public static string GetSettlementComment(int eventsCompleted, int energy)
        {
            if (energy >= 100) return "惊心动魄的一天";
            if (energy >= 80) return "充实的一天";
            if (energy >= 50) return "平常的一天";
            return "疲惫的一天";
        }

        public static string GetCharacterMonologue(int energy)
        {
            if (energy >= 100) return "今天真是收获满满！明天也要加油！";
            if (energy >= 80) return "还不错的一天，明天继续努力。";
            if (energy >= 50) return "有点累了，好好休息一下...";
            return "今天好累啊，希望明天会好一点。";
        }

        public static string GetBestEventName(string eventId)
        {
            switch (eventId)
            {
                case "ev_run": return "早操 800 米";
                case "ev_math": return "数学压轴题";
                case "ev_lunch": return "食堂拼桌风波";
                case "ev_pe": return "体育课三对三";
                case "ev_night": return "晚自习突击检查";
                default: return "精彩瞬间";
            }
        }

        #endregion
    }

    [Serializable]
    public class GameEvent
    {
        public string Id;
        public string Name;
        public string Time;
        public int Difficulty;
        public string Category;

        // Dialog with portrait
        public string Narrator;
        public string PortraitPath;
        public string Dialog;

        // Options
        public List<EventOption> Options;
    }

    [Serializable]
    public class EventOption
    {
        public string Label;      // Button label
        public string Name;       // Full option name
        public string Emoji;
        public int EnergyCost;
        public bool IsBest;       // This is the recommended/best choice
        public Dictionary<string, int> Effects;

        // Result
        public string ResultText; // Shows after selection (with +attr)
        public string ResultComment; // NPC/comment text
    }

    [Serializable]
    public class StreamItem
    {
        public string Time;
        public string Text;
        public string EventId;
    }

    // Color for attribute effects
    public static class DailyColors
    {
        public static readonly Color Gain = new Color32(76, 175, 80, 255);
        public static readonly Color Loss = new Color32(244, 67, 54, 255);
        public static readonly Color Neutral = new Color32(117, 117, 117, 255);
        public static readonly Color BestOptionBorder = new Color32(255, 193, 7, 255);
    }

    public interface IGameDataProvider
    {
        List<GameEvent> GetEvents();
        List<StreamItem> GetStreams();
        GameEvent GetEvent(string id);
    }

    public static class GameDataProviderRegistry
    {
        private static IGameDataProvider currentProvider;

        public static void RegisterProvider(IGameDataProvider provider)
        {
            currentProvider = provider;
        }

        public static List<GameEvent> GetEvents()
        {
            if (currentProvider != null)
                return currentProvider.GetEvents();
            return DailyGameData.GetDailyEvents();
        }

        public static List<StreamItem> GetStreams()
        {
            if (currentProvider != null)
                return currentProvider.GetStreams();
            return DailyGameData.GetDailyStreams();
        }

        public static GameEvent GetEvent(string id)
        {
            if (currentProvider != null)
                return currentProvider.GetEvent(id);
            return DailyGameData.GetEventById(id);
        }
    }
}