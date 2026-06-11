# 游戏状态机与交互逻辑文档

> 本文档描述 `我的高考志愿模拟器` 的完整状态结构、界面切换流程、存档/读档逻辑及各界面交互事件。
> 供 Unity 程序员实现游戏逻辑时参考。

---

## 1. gameState 完整结构

```csharp
[Serializable]
public class GameState
{
    // ===== 基础信息 =====
    public string gender;           // "male" | "female" | null
    public string name;             // 玩家名字，2-8字
    
    // ===== 家庭背景 =====
    public FamilyData family;       // 当前家庭
    public FamilyData oldFamily;    // 上一次抽卡的家庭（用于对比）
    public int freeReroll;          // 免费重置次数，初始 1
    public int adReroll;            // 广告重置次数，初始 1
    
    // ===== 省市选择 =====
    public string province;         // 省市名称
    
    // ===== 选科方向 =====
    public string subject;          // "physics" | "history" | null
    public string expandedSubject;  // 当前展开的科目 ID（UI 状态）
    
    // ===== 天赋树 =====
    public int energy;              // 剩余能量点，初始 6
    public Dictionary<string, int> talentNodes;  // 分支ID -> 已点亮节点数
    
    // ===== 流程控制 =====
    public string step;             // 当前界面标识
}

[Serializable]
public class FamilyData
{
    public string id;               // 家庭类型ID: "RURAL" | "WORKING" | "MIDDLE" | "WEALTHY" | "SCHOLAR"
    public string name;             // 显示名称
    public string icon;             // Emoji 图标
    public string desc;             // 描述
    public int money;               // 初始金币
    public Stats stats;             // 四维属性
}

[Serializable]
public class Stats
{
    public int int;                 // 智力 (10-100)
    public int psy;                 // 心理 (10-100)
    public int soc;                 // 社交 (10-100)
    public int health;              // 健康 (10-100)
}
```

### 初始状态（newGame）

```json
{
  "gender": null,
  "name": "",
  "family": null,
  "oldFamily": null,
  "freeReroll": 1,
  "adReroll": 1,
  "province": null,
  "subject": null,
  "expandedSubject": null,
  "step": "title",
  "energy": 6,
  "talentNodes": {}
}
```

---

## 2. 界面切换流程图

```
┌─────────────┐
│   title     │◄─────────────────────────────────────────────┐
│  启动画面    │                                              │
└──────┬──────┘                                              │
       │ newGame()                                           │
       ▼                                                     │
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     │
│   gender    │────►│   family    │────►│  province   │     │
│ 性别+起名   │     │ 家庭背景抽卡 │     │  省市选择   │     │
└─────────────┘     └─────────────┘     └──────┬──────┘     │
                                               │             │
                                               ▼             │
                                        ┌─────────────┐      │
                                        │   subject   │      │
                                        │  选科方向   │      │
                                        └──────┬──────┘      │
                                               │             │
                                               ▼             │
                                        ┌─────────────┐      │
                                        │   talent    │      │
                                        │  天赋树分配 │      │
                                        └──────┬──────┘      │
                                               │             │
                                               ▼             │
                                        ┌─────────────┐      │
                                        │  gameplay   │──────┘
                                        │  主游戏循环  │
                                        └─────────────┘

// Continue 分支
┌─────────────┐
│   title     │──► continueGame() 读取存档
│  启动画面    │      根据 step 跳转到对应界面
└─────────────┘
```

### Step 枚举值

| step 值 | 对应界面 | 说明 |
|---------|----------|------|
| `title` | Screen-Title | 启动画面 |
| `gender` | Screen-Gender | 性别+起名 |
| `family` | Screen-Family | 家庭背景 |
| `province` | Screen-Province | 省市选择 |
| `subject` | Screen-Subject | 选科方向 |
| `talent` | Screen-Talent | 天赋树 |

### 切换规则

1. **title -> gender**：点击"重启人生"按钮
2. **gender -> family**：性别已选 + 名字有效（2-8字，无敏感词），点击"下一步"
3. **family -> province**：点击"就这个了！"按钮
4. **province -> subject**：已选省市，点击"确认"
5. **subject -> talent**：已选科目，在确认对话框中点击"确定"
6. **talent -> gameplay**：点击"确认天赋"按钮，进入主游戏循环
7. **任意界面 -> title**：游戏结束或返回主菜单

---

## 3. 存档/读档逻辑

### 存档机制

```csharp
public void SaveGame()
{
    // Unity 中使用 PlayerPrefs 或 JSON 文件
    string json = JsonUtility.ToJson(gameState);
    PlayerPrefs.SetString("gaokao_sim_save", json);
    PlayerPrefs.Save();
}
```

**存档触发时机**：
- 确认性别后（gender -> family 切换时）
- 抽卡动画结束后（family 确定时）
- 保留旧家庭/新家庭后（对比选择后）
- 确认省市后（province -> subject 切换时）
- 确认选科后（subject -> talent 切换时）
- 点亮天赋节点后（每次 lightNode 成功后）
- 确认天赋后（talent -> gameplay 切换时）

### 读档机制

```csharp
public void ContinueGame()
{
    string saved = PlayerPrefs.GetString("gaokao_sim_save", "");
    if (!string.IsNullOrEmpty(saved))
    {
        gameState = JsonUtility.FromJson<GameState>(saved);
        
        // 根据 step 恢复界面
        switch (gameState.step)
        {
            case "gender":
                ShowScreen("Screen-Gender");
                break;
            case "family":
                ShowScreen("Screen-Family");
                RenderFamily();
                break;
            case "province":
                ShowScreen("Screen-Province");
                RenderProvinceList();
                break;
            case "subject":
                ShowScreen("Screen-Subject");
                RenderSubjectCards();
                break;
            case "talent":
                ShowScreen("Screen-Talent");
                RenderTalentTree();
                break;
            default:
                ShowScreen("Screen-Title");
                break;
        }
    }
}
```

### 新游戏逻辑

```csharp
public void NewGame()
{
    // 重置状态
    gameState = new GameState();
    
    // 清除存档
    PlayerPrefs.DeleteKey("gaokao_sim_save");
    
    // 重置 UI
    ClearInputField();
    ClearGenderSelection();
    DisableConfirmButton();
    
    // 切换到性别界面
    ShowScreen("Screen-Gender");
}
```

### 继续游戏按钮显示逻辑

```csharp
void Start()
{
    // 检查是否有存档
    if (PlayerPrefs.HasKey("gaokao_sim_save"))
    {
        btnContinue.gameObject.SetActive(true);
    }
    else
    {
        btnContinue.gameObject.SetActive(false);
    }
}
```

---

## 4. 各界面交互事件列表

### 4.1 启动画面（Screen-Title）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnNewGameClick` | 点击"重启人生" | 调用 `NewGame()` |
| `OnContinueClick` | 点击"继续游戏" | 调用 `ContinueGame()` |

### 4.2 性别+起名（Screen-Gender）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnSelectGender` | 点击性别卡片 | 设置 `gameState.gender`，更新选中态，调用 `CheckGenderReady()` |
| `OnNameInput` | 输入框内容变化 | 实时校验名字，更新 `gameState.name`，调用 `CheckGenderReady()` |
| `CheckGenderReady` | 性别或名字变化 | 校验：`gender != null && name.Length >= 2 && name.Length <= 8 && !CheckBadWords(name)` |
| `OnConfirmGender` | 点击"下一步" | 调用 `SaveGame()`，切换到 `Screen-Family`，播放抽卡动画 |

**名字校验规则**：
```csharp
bool CheckBadWords(string str)
{
    string s = str.ToLower();
    foreach (var word in GameData.BAD_WORDS)
    {
        if (s.Contains(word)) return true;
    }
    return false;
}
```

### 4.3 家庭背景（Screen-Family）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnPlayDrawAnimation` | 进入界面时自动触发 | 20步抽卡动画，每步100ms，最后确定家庭 |
| `OnRerollFamily` | 点击"免费重置" | `freeReroll--`，保存旧家庭，重新播放动画 |
| `OnAdRerollFamily` | 点击"看广告重置" | `adReroll--`，保存旧家庭，重新播放动画 |
| `OnShowCompare` | 重置次数用完时点击 | 显示对比弹窗，展示 oldFamily vs family |
| `OnKeepOldFamily` | 对比弹窗"保留旧的" | `family = oldFamily`，`oldFamily = null`，`SaveGame()` |
| `OnKeepNewFamily` | 对比弹窗"选择新的" | `oldFamily = null`，`SaveGame()` |
| `OnConfirmFamily` | 点击"就这个了！" | `SaveGame()`，切换到 `Screen-Province` |
| `OnOpenStatInfo` | 点击右上角"?" | 显示属性说明弹窗 |
| `OnCloseStatInfo` | 点击"知道了" | 关闭属性说明弹窗 |

**抽卡动画逻辑**：
```csharp
IEnumerator PlayDrawAnimation()
{
    // 显示动画阶段，隐藏结果
    drawStage.SetActive(true);
    familyResult.SetActive(false);
    familyFooter.SetActive(false);
    
    int totalSteps = 20;
    for (int step = 0; step < totalSteps; step++)
    {
        progressBar.value = (float)step / totalSteps;
        
        // 随机显示家庭
        var randomFamily = GetRandomFamilyTemplate();
        drawIcon.text = randomFamily.icon;
        drawName.text = randomFamily.name;
        drawDesc.text = randomFamily.desc;
        
        // Pop 动画
        drawCard.transform.DOScale(1.08f, 0.04f).SetLoops(2, LoopType.Yoyo);
        
        yield return new WaitForSeconds(0.1f);
    }
    
    // 确定最终家庭
    gameState.family = RollFamily();
    gameState.oldFamily = null;
    SaveGame();
    
    // 切换到结果展示
    drawStage.SetActive(false);
    RenderFamily();
    familyResult.SetActive(true);
    familyFooter.SetActive(true);
}

FamilyData RollFamily()
{
    var keys = GameData.FAMILY_RANGE.Keys.ToList();
    var key = keys[Random.Range(0, keys.Count)];
    var template = GameData.FAMILY_RANGE[key];
    
    return new FamilyData
    {
        id = key,
        name = template.name,
        icon = template.icon,
        desc = template.desc,
        money = template.money,
        stats = new Stats
        {
            int = Random.Range(template.int[0], template.int[1] + 1),
            psy = Random.Range(template.psy[0], template.psy[1] + 1),
            soc = Random.Range(template.soc[0], template.soc[1] + 1),
            health = Random.Range(template.health[0], template.health[1] + 1)
        }
    };
}
```

### 4.4 省市选择（Screen-Province）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnSelectProvince` | 点击省市卡片 | 设置 `gameState.province`，更新选中态，显示省市信息面板，启用确认按钮 |
| `OnConfirmProvince` | 点击"确认" | `SaveGame()`，切换到 `Screen-Subject` |

**省市列表渲染规则**：
- 热门城市（北京、上海、广州、深圳）显示在"🔥 热门城市"区域
- 其余省市显示在"📋 全部省市"区域
- 每个卡片显示：名称、高考模式标签、难度文字
- 卡片背景色根据难度等级变化（见 art-spec.md）

### 4.5 选科方向（Screen-Subject）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnToggleSubjectDetail` | 点击科目卡片 | 如果已展开则收起；如果未展开则展开并设为选中，启用确认按钮 |
| `OnConfirmSubject` | 点击"确认" | 显示确认对话框 |
| `OnFinalizeSubject` | 对话框点击"确定" | 重置能量为6，清空天赋节点，`SaveGame()`，切换到 `Screen-Talent` |
| `OnCancelSubject` | 对话框点击"再想想" | 关闭对话框 |

**展开/收起逻辑**：
```csharp
void ToggleSubjectDetail(string subjectId)
{
    if (gameState.expandedSubject == subjectId)
    {
        // 收起
        gameState.expandedSubject = null;
    }
    else
    {
        // 展开并选中
        gameState.subject = subjectId;
        gameState.expandedSubject = subjectId;
        btnSubjectConfirm.interactable = true;
    }
    RenderSubjectCards();
}
```

### 4.6 天赋树（Screen-Talent）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OnLightNode` | 点击可点亮节点 | 消耗1能量，点亮节点，`SaveGame()`，刷新天赋树 |
| `OnConfirmTalent` | 点击"确认天赋" | `SaveGame()`，提示"准备进入高一上学期"，进入主游戏 |

**节点状态判定**：
```csharp
NodeState GetNodeState(string branchId, int nodeIdx)
{
    var unlockedBranches = gameState.subject == "physics" 
        ? new[] { "math", "craft", "body" }
        : new[] { "word", "art", "body" };
    
    bool isUnlocked = unlockedBranches.Contains(branchId);
    if (!isUnlocked) return NodeState.Locked;
    
    int litCount = gameState.talentNodes.ContainsKey(branchId) 
        ? gameState.talentNodes[branchId] 
        : 0;
    
    if (nodeIdx < litCount) return NodeState.Lit;
    if (nodeIdx == litCount) return NodeState.Active;
    return NodeState.Locked;
}
```

**点亮规则**：
```csharp
void LightNode(string branchId, int nodeIdx)
{
    if (gameState.energy <= 0)
    {
        ShowAlert("能量点不足！");
        return;
    }
    
    int currentLit = gameState.talentNodes.ContainsKey(branchId) 
        ? gameState.talentNodes[branchId] 
        : 0;
    
    if (nodeIdx != currentLit)
    {
        ShowAlert("请按顺序点亮节点！");
        return;
    }
    
    // 点亮节点
    gameState.talentNodes[branchId] = currentLit + 1;
    gameState.energy--;
    
    SaveGame();
    RenderTalentTree();
}
```

---

## 5. 弹窗交互事件

### 5.1 确认对话框（ConfirmDialog）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `ShowConfirmDialog` | 需要二次确认时 | 设置标题和消息，显示弹窗 |
| `OnConfirm` | 点击"确定" | 执行确认回调，关闭弹窗 |
| `OnCancel` | 点击"取消/再想想" | 执行取消回调，关闭弹窗 |

### 5.2 家庭对比界面（CompareOverlay）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `ShowCompareOverlay` | 重置次数用完时点击重置 | 渲染旧家庭和新家庭对比数据，显示弹窗 |
| `OnKeepOld` | 点击"保留旧的" | `family = oldFamily`，`oldFamily = null`，关闭弹窗，刷新显示 |
| `OnKeepNew` | 点击"选择新的" | `oldFamily = null`，关闭弹窗，刷新显示 |

### 5.3 属性说明弹窗（StatInfoPopup）

| 事件 | 触发条件 | 执行逻辑 |
|------|----------|----------|
| `OpenStatInfo` | 点击"?"按钮 | 显示弹窗 |
| `CloseStatInfo` | 点击"知道了" | 关闭弹窗 |

---

## 6. 工具函数

### 6.1 随机属性波动

```csharp
int RandomVary(int baseValue, int range)
{
    return Mathf.Clamp(
        Mathf.RoundToInt(baseValue + Random.Range(-range, range)),
        10, 100
    );
}
```

### 6.2 界面切换

```csharp
void ShowScreen(string screenId)
{
    // 隐藏所有界面
    foreach (var screen in allScreens)
    {
        screen.SetActive(false);
    }
    
    // 显示目标界面
    var target = GetScreen(screenId);
    target.SetActive(true);
    
    // 更新状态
    gameState.step = screenId.Replace("Screen-", "").ToLower();
}
```

### 6.3 动画工具

```csharp
// 抖动动画（用于输入错误提示）
void PlayShakeAnimation(Transform target)
{
    target.DOShakePosition(0.4f, new Vector3(4, 0, 0), 10, 0);
}

// 渐显动画
void PlayFadeIn(CanvasGroup canvasGroup)
{
    canvasGroup.alpha = 0;
    canvasGroup.DOFade(1, 0.2f);
}

// 浮动动画
void PlayFloatAnimation(Transform target, float duration = 3f, float distance = 10f)
{
    target.DOMoveY(target.position.y + distance, duration / 2)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
}
```

---

## 7. 事件总线（可选设计）

建议 Unity 中实现简单的事件总线来解耦 UI 和逻辑：

```csharp
public static class GameEvents
{
    // 状态变化事件
    public static Action OnGameStateChanged;
    public static Action OnFamilyRolled;
    public static Action OnProvinceSelected;
    public static Action OnSubjectSelected;
    public static Action OnTalentNodeLit;
    
    // UI 事件
    public static Action<string> OnShowScreen;
    public static Action OnShowConfirmDialog;
    public static Action OnShowCompareOverlay;
    public static Action OnShowStatInfo;
    
    // 存档事件
    public static Action OnGameSaved;
    public static Action OnGameLoaded;
}
```

---

## 8. 数据流图

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Player     │────►│   UI Input  │────►│  GameLogic  │
│  Interaction│     │  (Buttons,  │     │  (Events,   │
│             │     │   Input)    │     │   State)    │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │  GameState  │
                                        │  (Data)     │
                                        └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │  SaveSystem │
                                        │ (PlayerPrefs│
                                        │  /JSON File)│
                                        └─────────────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │   UI Render │
                                        │  (Update    │
                                        │   Display)  │
                                        └─────────────┘
```
