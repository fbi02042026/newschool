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
        public int Money { get; set; } = 0;

        public int StatIntelligence { get; set; } = 0;
        public int StatPsychology { get; set; } = 0;
        public int StatSocial { get; set; } = 0;
        public int StatHealth { get; set; } = 0;
        
        // ===== 选科信息 =====
        public FirstSubject FirstSubject { get; set; } = FirstSubject.None;
        public List<SecondSubject> SecondSubjects { get; set; } = new List<SecondSubject>();
        
        // ===== 游戏进度 =====
        public GameProgress CurrentProgress { get; set; } = GameProgress.Launch;
        public int CurrentPlaythrough { get; set; } = 1; // 第几周目
        public bool HasSaveData { get; set; } = false;
        
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
            Money = 0;
            StatIntelligence = 0;
            StatPsychology = 0;
            StatSocial = 0;
            StatHealth = 0;
            FirstSubject = FirstSubject.None;
            SecondSubjects.Clear();
            CurrentProgress = GameProgress.Launch;
            // 保留 CurrentPlaythrough 和 HasSaveData
            
            Debug.Log("[GameState] 状态已重置");
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
            
            // 基础按钮总是解锁
            unlocked.Add(HomeButtonType.Settings);
            unlocked.Add(HomeButtonType.Achievements);
            unlocked.Add(HomeButtonType.Rules);
            
            // 选科完成后解锁
            if (FirstSubject != FirstSubject.None && SecondSubjects.Count == 2)
            {
                unlocked.Add(HomeButtonType.TalentTree);
                unlocked.Add(HomeButtonType.Semester);
                unlocked.Add(HomeButtonType.Equipment);
            }
            
            // 学期完成后解锁高考
            if (CurrentProgress >= GameProgress.Semester)
            {
                unlocked.Add(HomeButtonType.Gaokao);
            }
            
            // 高考完成后解锁志愿
            if (CurrentProgress >= GameProgress.Gaokao)
            {
                unlocked.Add(HomeButtonType.Volunteer);
            }
            
            // 志愿完成后解锁大学
            if (CurrentProgress >= GameProgress.Volunteer)
            {
                unlocked.Add(HomeButtonType.University);
                unlocked.Add(HomeButtonType.Career);
            }
            
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
        Rules
    }
    
    #endregion
}
