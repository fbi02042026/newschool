# 我的高考志愿模拟器 - Unity 移植说明

> 本文档为 Unity 开发团队提供项目概述、文件结构说明、接入建议及各界面实现优先级。

---

## 项目概述

`我的高考志愿模拟器` 是一款基于真实中国高考制度的竖屏模拟游戏。玩家通过选择性别、抽取家庭背景、选择省市、确定选科方向、分配天赋点等步骤，体验从高中到高考的人生历程。

### 核心玩法

1. **角色创建**：选择性别，输入名字（2-8字，过滤敏感词）
2. **家庭抽卡**：随机抽取家庭背景（村民之子、工薪之家、中产家庭、富裕家庭、书香门第），获得初始属性和金币
3. **省市选择**：选择高考省份，不同省份有不同的高考模式和难度系数
4. **选科方向**：选择物理方向或历史方向，解锁不同的天赋分支和专业
5. **天赋分配**：使用能量点点亮天赋树节点，提升角色属性

### 技术栈建议

- **引擎**：Unity 2022.3 LTS 或更高版本
- **UI 框架**：UGUI（Unity GUI）
- **动画**：DOTween（推荐）或 Unity 原生 Animation
- **数据持久化**：PlayerPrefs（简单存档）或 JSON 文件
- **JSON 解析**：Unity 内置 JsonUtility 或 Newtonsoft.Json

---

## 文件结构说明

```
unity-export/
├── README.md              # 本文件：移植说明与接入建议
├── game-data.json         # 游戏配置数据（Unity 可直接读取）
├── ui-layout.md           # UI 布局说明文档
├── state-machine.md       # 状态机与交互逻辑文档
└── art-spec.md            # 美术资源规格文档
```

### 各文件用途

| 文件 | 目标读者 | 内容 |
|------|----------|------|
| `game-data.json` | 程序员 | 所有游戏配置数据，包括家庭、省市、科目、天赋、属性标签、敏感词等 |
| `ui-layout.md` | UI 设计师 | 每个界面的元素列表、位置、尺寸、动画规格 |
| `state-machine.md` | 程序员 | 游戏状态结构、界面切换流程、存档读档逻辑、交互事件列表 |
| `art-spec.md` | 美术 + 程序员 | 配色方案、字体要求、圆角阴影、动画参数、图标映射 |
| `README.md` | 全团队 | 项目概述、文件结构、接入建议、实现优先级 |

---

## Unity 接入建议

### 1. 数据层（Data Layer）

#### 1.1 读取 game-data.json

```csharp
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;
    public GameConfig Config;
    
    void Awake()
    {
        Instance = this;
        LoadConfig();
    }
    
    void LoadConfig()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("game-data");
        if (jsonFile != null)
        {
            Config = JsonUtility.FromJson<GameConfig>(jsonFile.text);
        }
    }
}

[Serializable]
public class GameConfig
{
    public string version;
    public int year;
    public string[] BAD_WORDS;
    public FamilyRange[] FAMILY_RANGE;
    public ProvinceData[] PROVINCE_DATA;
    public SubjectData[] SUBJECT_DATA;
    public StatLabels STAT_LABELS;
    public TalentData[] TALENT_DATA;
    // ... 其他字段
}
```

#### 1.2 数据模型

参考 `state-machine.md` 中的 C# 数据结构定义，创建对应的 `GameState`、`FamilyData`、`Stats` 等类。

### 2. UI 层（UI Layer）

#### 2.1 使用 UGUI 搭建界面

- 每个界面对应一个 `Canvas` 或一个 `Panel`
- 使用 `Canvas Scaler` 适配不同分辨率（参考 `art-spec.md` 第10章）
- 建议使用 `VerticalLayoutGroup` 和 `HorizontalLayoutGroup` 实现弹性布局
- 弹窗使用独立的 `Canvas` 并设置 `Sort Order` 控制层级

#### 2.2 界面切换管理器

```csharp
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] screens;
    
    public void ShowScreen(string screenName)
    {
        foreach (var screen in screens)
        {
            screen.SetActive(screen.name == screenName);
        }
        GameStateManager.Instance.CurrentStep = screenName;
    }
}
```

### 3. 存档系统

```csharp
public class SaveManager
{
    private const string SAVE_KEY = "gaokao_sim_save";
    
    public static void Save(GameState state)
    {
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }
    
    public static GameState Load()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<GameState>(json);
        }
        return null;
    }
    
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}
```

### 4. 动画建议

推荐使用 **DOTween** 插件实现动画：

```csharp
// 浮动动画
transform.DOMoveY(transform.position.y + 10, 1.5f)
    .SetEase(Ease.InOutSine)
    .SetLoops(-1, LoopType.Yoyo);

// 缩放弹跳
transform.DOScale(1.08f, 0.08f).SetLoops(2, LoopType.Yoyo);

// 渐显
canvasGroup.DOFade(1, 0.2f);

// 抖动
transform.DOShakePosition(0.4f, new Vector3(4, 0, 0), 10, 0);
```

### 5. 粒子特效

- **启动画面背景**：使用 Particle System 创建 12 个浮动圆点
- **抽卡完成**：短暂的粒子爆发效果
- **天赋点亮**：节点位置的小范围粒子效果

---

## 各界面实现优先级建议

### P0 - 必须优先实现（核心流程）

| 优先级 | 界面 | 说明 | 预估工作量 |
|--------|------|------|-----------|
| P0 | 启动画面 | 游戏入口，包含新游戏/继续游戏按钮 | 1天 |
| P0 | 性别+起名 | 角色创建第一步，基础输入校验 | 1天 |
| P0 | 家庭背景 | 核心抽卡玩法，含动画和重置逻辑 | 2天 |
| P0 | 省市选择 | 网格列表，选中态切换 | 1天 |
| P0 | 选科方向 | 展开/收起卡片，确认对话框 | 1天 |
| P0 | 天赋树 | 5分支x5节点，点亮逻辑 | 2天 |

**P0 合计：约 8 个工作日**

### P1 - 重要功能（体验完善）

| 优先级 | 界面/功能 | 说明 | 预估工作量 |
|--------|----------|------|-----------|
| P1 | 家庭对比弹窗 | 重置次数用完后的对比选择 | 0.5天 |
| P1 | 属性说明弹窗 | 右上角"?"按钮触发的帮助 | 0.5天 |
| P1 | 存档/读档系统 | PlayerPrefs 实现 | 0.5天 |
| P1 | 动画系统 | DOTween 集成，所有界面动画 | 1天 |
| P1 | 粒子背景 | 启动画面浮动圆点 | 0.5天 |

**P1 合计：约 3 个工作日**

### P2 - 优化项（锦上添花）

| 优先级 | 界面/功能 | 说明 | 预估工作量 |
|--------|----------|------|-----------|
| P2 | 后处理效果 | Bloom、Color Grading、Vignette | 0.5天 |
| P2 | 音效系统 | 按钮点击、抽卡音效、成功音效 | 0.5天 |
| P2 | 本地化支持 | 多语言框架预留 | 0.5天 |
| P2 | 成就系统 | 预留数据结构 | 0.5天 |

**P2 合计：约 2 个工作日**

### 推荐开发顺序

```
Week 1:
  Day 1-2: 项目搭建 + 数据层 + 存档系统 + 启动画面
  Day 3-4: 性别+起名 + 家庭背景（含抽卡动画）
  Day 5:   省市选择 + 选科方向

Week 2:
  Day 1-2: 天赋树 + 弹窗系统
  Day 3:   动画系统 + 粒子特效
  Day 4:   整合测试 + Bug 修复
  Day 5:   优化 + 后处理 + 音效
```

---

## 关键注意事项

### 1. 敏感词过滤

- 名字输入必须过滤 `game-data.json` 中的 `BAD_WORDS` 列表
- 校验不通过时显示红色提示文字，禁用确认按钮
- 建议使用 `ToLower()` 后进行包含匹配

### 2. 随机数种子

- 家庭属性随机生成时，注意 `Random.Range(min, max + 1)` 的边界
- 属性值需要限制在 10-100 范围内

### 3. 天赋解锁规则

- 物理方向解锁：数理(math)、匠心(craft)、韧骨(body)
- 历史方向解锁：辞海(word)、灵感(art)、韧骨(body)
- 节点必须按顺序点亮（0 -> 1 -> 2 -> 3 -> 4）
- 每点亮一个节点消耗 1 点能量

### 4. 省市难度系数

- 难度系数影响后续游戏的高考分数要求
- 轻松模式（0.88-0.95）：北京、上海、天津等
- 正常模式（0.96-1.05）：大部分省份
- 困难模式（1.06-1.12）：湖北、湖南、广东等
- 地狱模式（1.15-1.22）：河南、河北、山东、江苏、浙江

### 5. 响应式适配

- 设计基准：430px x 764px（9:16 竖屏）
- 使用 `Canvas Scaler` 的 `Scale With Screen Size` 模式
- 适配刘海屏/挖孔屏的安全区域

---

## 参考资源

### 文档索引

| 需求 | 参考文档 |
|------|----------|
| 需要游戏数据 | `game-data.json` |
| 需要 UI 布局 | `ui-layout.md` |
| 需要状态机逻辑 | `state-machine.md` |
| 需要美术规格 | `art-spec.md` |

### 外部工具推荐

| 工具 | 用途 |
|------|------|
| DOTween | UI 动画 |
| TextMeshPro | 高质量文字渲染（支持渐变文字） |
| Newtonsoft.Json | 更强大的 JSON 解析 |
| Unity Particle Pack | 粒子特效参考 |

---

## 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0 | 2026-06-11 | 初始 Unity 移植包 |

---

## 联系方式

如有疑问，请参考原始 HTML 游戏源码：`/workspace/gaokao-simulator/index.html`
