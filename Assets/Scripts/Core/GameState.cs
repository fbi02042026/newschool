using System;
using System.Collections.Generic;
using UnityEngine;

namespace GaokaoSimulator.Core
{
    /// <summary>
    /// 游戏全局状态 - 保存玩家所有进度数据
    /// 单例模式，跨场景持久化
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }
        
        // ===== 玩家基础信息 =====
        public string PlayerName { get; set; } = "";
        public PlayerGender Gender { get; set; } = PlayerGender.Male;
        public FamilyBackgroundType SelectedFamily { get; set; } = FamilyBackgroundType.None;
        public string SelectedProvince { get; set; } = "";
        public string SelectedProvinceMode { get; set; } = "";
        public int Money { get; set; } = 0;
        public int GuideToneVariant { get; set; } = -1;
        public string FamilyExamEventTitle { get; set; } = "";
        public string FamilyExamEventDesc { get; set; } = "";
        public string FamilyVolunteerEventTitle { get; set; } = "";
        public string FamilyVolunteerEventDesc { get; set; } = "";

        public int StatIntelligence { get; set; } = 0;
        public int StatPsychology { get; set; } = 0;
        public int StatSocial { get; set; } = 0;
        public int StatHealth { get; set; } = 0;
        public List<string> OwnedItems { get; set; } = new List<string>();
        
        // ===== 选科信息 =====
        public FirstSubject FirstSubject { get; set; } = FirstSubject.None;
        public List<SecondSubject> SecondSubjects { get; set; } = new List<SecondSubject>();
        
        // ===== 游戏进度 =====
        public GameProgress CurrentProgress { get; set; } = GameProgress.Launch;
        public int SemesterIndex { get; set; } = 0;
        public int TotalSemesters { get; set; } = 6;
        public List<string> SemesterGrades { get; set; } = new List<string>();
        public int CurrentPlaythrough { get; set; } = 1; // 第几周目
        public int UniversityYearIndex { get; set; } = 0; // 大学当前学年 0=大一 1=大二 2=大三 3=大四
        public bool HasSaveData { get; set; } = false;

        private readonly HashSet<string> seenGuideKeys = new HashSet<string>();

        public bool HasSeenGuide(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return true;
            }

            return seenGuideKeys.Contains(key);
        }

        public void MarkGuideSeen(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            seenGuideKeys.Add(key);
        }
        
        // ===== 属性系统（后续扩展）=====
        // public PlayerAttributes Attributes { get; set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (GuideToneVariant < 0)
            {
                GuideToneVariant = UnityEngine.Random.Range(0, 3);
            }
            
            Debug.Log("[GameState] 初始化完成");
        }
        
        /// <summary>
        /// 重置所有状态（新游戏）
        /// </summary>
        public void ResetState()
        {
            PlayerName = "";
            Gender = PlayerGender.Male;
            SelectedFamily = FamilyBackgroundType.None;
            SelectedProvince = "";
            SelectedProvinceMode = "";
            Money = 0;
            StatIntelligence = 0;
            StatPsychology = 0;
            StatSocial = 0;
            StatHealth = 0;
            GuideToneVariant = UnityEngine.Random.Range(0, 3);
            FamilyExamEventTitle = "";
            FamilyExamEventDesc = "";
            FamilyVolunteerEventTitle = "";
            FamilyVolunteerEventDesc = "";
            FirstSubject = FirstSubject.None;
            SecondSubjects.Clear();
            CurrentProgress = GameProgress.Launch;
            SemesterIndex = 0;
            SemesterGrades.Clear();
            TotalSemesters = 6;
            UniversityYearIndex = 0;
            seenGuideKeys.Clear();
            OwnedItems.Clear();
            // 保留 CurrentPlaythrough 和 HasSaveData
            
            Debug.Log("[GameState] 状态已重置");
        }

        public void SaveGame()
        {
            PlayerPrefs.SetInt("HasSaveData", HasSaveData ? 1 : 0);
            PlayerPrefs.SetInt("CurrentProgress", (int)CurrentProgress);
            PlayerPrefs.SetInt("SemesterIndex", SemesterIndex);
            PlayerPrefs.SetInt("Money", Money);
            PlayerPrefs.SetInt("StatIntelligence", StatIntelligence);
            PlayerPrefs.SetInt("StatPsychology", StatPsychology);
            PlayerPrefs.SetInt("StatSocial", StatSocial);
            PlayerPrefs.SetInt("StatHealth", StatHealth);
            PlayerPrefs.SetString("PlayerName", PlayerName ?? "");
            PlayerPrefs.SetString("SelectedProvince", SelectedProvince ?? "");
            PlayerPrefs.SetInt("SelectedFamily", (int)SelectedFamily);
            PlayerPrefs.SetInt("FirstSubject", (int)FirstSubject);
            PlayerPrefs.SetInt("UniversityYearIndex", UniversityYearIndex);

            // 持久化学期成绩列表
            var gradesStr = SemesterGrades != null && SemesterGrades.Count > 0
                ? string.Join(",", SemesterGrades)
                : "";
            PlayerPrefs.SetString("SemesterGrades", gradesStr);

            PlayerPrefs.Save();
            Debug.Log("[GameState] 存档已保存");
        }

        public void LoadGame()
        {
            if (PlayerPrefs.GetInt("HasSaveData", 0) == 0) return;

            HasSaveData = true;
            CurrentProgress = (GameProgress)PlayerPrefs.GetInt("CurrentProgress", 0);
            SemesterIndex = PlayerPrefs.GetInt("SemesterIndex", 0);
            Money = PlayerPrefs.GetInt("Money", 0);
            StatIntelligence = PlayerPrefs.GetInt("StatIntelligence", 0);
            StatPsychology = PlayerPrefs.GetInt("StatPsychology", 0);
            StatSocial = PlayerPrefs.GetInt("StatSocial", 0);
            StatHealth = PlayerPrefs.GetInt("StatHealth", 0);
            PlayerName = PlayerPrefs.GetString("PlayerName", "");
            SelectedProvince = PlayerPrefs.GetString("SelectedProvince", "");
            SelectedFamily = (FamilyBackgroundType)PlayerPrefs.GetInt("SelectedFamily", 0);
            FirstSubject = (FirstSubject)PlayerPrefs.GetInt("FirstSubject", 0);
            UniversityYearIndex = PlayerPrefs.GetInt("UniversityYearIndex", 0);

            // 加载学期成绩列表
            var gradesStr = PlayerPrefs.GetString("SemesterGrades", "");
            SemesterGrades.Clear();
            if (!string.IsNullOrEmpty(gradesStr))
            {
                var parts = gradesStr.Split(',');
                foreach (var p in parts)
                {
                    if (!string.IsNullOrEmpty(p))
                        SemesterGrades.Add(p);
                }
            }

            Debug.Log("[GameState] 存档已加载");
        }

        public void OnApplicationQuit()
        {
            if (HasSaveData)
            {
                SaveGame();
            }
        }
        
        /// <summary>
        /// 检查是否可以进入指定进度
        /// </summary>
        public bool CanEnterProgress(GameProgress targetProgress)
        {
            switch (targetProgress)
            {
                case GameProgress.Home:
                    // 必须完成选科才能进入主界面
                    return FirstSubject != FirstSubject.None && SecondSubjects.Count == 2;
                    
                case GameProgress.TalentTree:
                case GameProgress.Semester:
                    // 需要完成选科
                    return FirstSubject != FirstSubject.None && SecondSubjects.Count == 2;
                    
                case GameProgress.Gaokao:
                    // 需要完成学期学习
                    return CurrentProgress >= GameProgress.Semester;
                    
                case GameProgress.Volunteer:
                    // 需要完成高考
                    return CurrentProgress >= GameProgress.Gaokao;
                    
                case GameProgress.University:
                    // 需要完成志愿填报
                    return CurrentProgress >= GameProgress.Volunteer;
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 获取当前可解锁的按钮列表
        /// </summary>
        public List<HomeButtonType> GetUnlockedButtons()
        {
            var unlocked = new List<HomeButtonType>();
            
            // 基础按钮 + 商城(独立入口) + 活动中心(始终可见) + 学生档案
            unlocked.Add(HomeButtonType.Equipment);      // 商城 - 独立入口
            unlocked.Add(HomeButtonType.Activity);       // 活动中心 - 始终可见
            unlocked.Add(HomeButtonType.StudentProfile); // 学生档案 - 独立入口
            unlocked.Add(HomeButtonType.Settings);        // 右上角
            unlocked.Add(HomeButtonType.Achievements);    // 右上角
            unlocked.Add(HomeButtonType.Rules);           // 右上角
            
            // 选科完成后解锁（校园日常已合并到主线流程，不再作为独立按钮）
            if (FirstSubject != FirstSubject.None && SecondSubjects.Count == 2)
            {
                unlocked.Add(HomeButtonType.TalentTree);
            }
            
            // 大学时光和人生启程始终显示，灰显由 HomeScreen 控制
            unlocked.Add(HomeButtonType.University);
            unlocked.Add(HomeButtonType.Career);
            
            return unlocked;
        }
    }
    
    #region 枚举定义
    
    public enum PlayerGender { Male, Female }
    
    public enum FamilyBackgroundType 
    { 
        None, 
        Intellectual,   // 知识分子
        Business,       // 经商家庭
        Worker,         // 工薪阶层
        Rural,          // 农村家庭
        CivilServant    // 公务员家庭
    }
    
    public enum FirstSubject { None, Physics, History }
    
    public enum SecondSubject 
    { 
        None, 
        Politics, 
        Geography, 
        Chemistry, 
        Biology 
    }
    
    public enum GameProgress 
    { 
        Launch,         // 启动
        Profile,        // 创建人物
        Family,         // 家庭背景
        Province,       // 选择省份
        Subject,        // 选科
        Home,           // 主界面Hub
        TalentTree,     // 天赋树
        Semester,       // 学期主界面
        Gaokao,         // 高考
        Volunteer,      // 志愿填报
        University,     // 大学
        Career,         // 毕业到30岁
        Summary         // 人生总结
    }
    
    public enum HomeButtonType
    {
        TalentTree,
        Semester,
        Equipment,
        Gaokao,
        Volunteer,
        University,
        Career,
        Achievements,
        Settings,
        Rules,
        Activity,
        StudentProfile
    }
    
    #endregion
}
