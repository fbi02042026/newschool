# 开发记录（续做用）

> 本文件用于记录每次改动点、涉及文件和下一步计划，方便跨设备继续开发。

## 2026-06-12

### 玩法方向

- 采用“混合式”结构：主线依旧线性推进，但在关键节点回到 Home 汇总与做选择
- Home 的定位：阶段结算/放假中转/承载小游戏与支线系统（如学期结束放假小游戏、大学阶段实习小游戏），避免“纯填志愿工具化”的枯燥感，体现“玩中学”

### 已完成改动（本次会话）

#### 家庭界面（Family）

- 修复：重置按钮需要点多次才触发的问题（原因是流程提示文字挡住射线）
  - 修改：[ScreenFlowHint.cs](file:///z:/gaokao/newschool/Assets/Scripts/UI/ScreenFlowHint.cs)
  - 要点：将提示 Text 的 `raycastTarget` 设为 `false`
- 修复：右上角问号出现两个
  - 修改：[FamilyScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Family/FamilyScreen.cs)
  - 要点：`GuideService.EnsureHelpButton` 统一复用预制体已有的 `BtnHelp`
- 修复：从省市选择返回家庭界面不再随机
  - 修改：[FamilyScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Family/FamilyScreen.cs)
  - 要点：若已选择过家庭且进度已到 Province 及以后，返回直接展示已选家庭，不再 `StartDraw()`

#### 省市界面（Province）

- UI：将“地狱难度”配色改为偏红（底色 + 文字 + 选中态）
  - 修改：[ProvinceScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Province/ProvinceScreen.cs)
  - 变更点：`GetDifficultyCardColor / GetDifficultyAccentColor / GetSelectedDifficultyAccentColor`

#### 主界面（Home）

- 新增：Home 界面脚本（运行时生成 UI）
  - 新增：[HomeScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Home/HomeScreen.cs)
  - 布局：中间人物形象卡（性别徽章 + 名字 + 关键摘要），下方功能入口卡（2 列网格）
  - 入口：按 `GameState.GetUnlockedButtons()` 动态生成入口按钮
- 路由接入：`ScreenType.Home` 不再走占位页，改为加载 `HomeScreen`
  - 修改：[ScreenRouter.cs](file:///z:/gaokao/newschool/Assets/Scripts/UI/ScreenRouter.cs)
- 混合式关键交互：Home 增加“继续主线”置顶按钮
  - 修改：[HomeScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Home/HomeScreen.cs)
  - 逻辑：根据 `GameState.CurrentProgress` 计算下一步 ScreenType（会优先推进到“学期”，学期完结后再进入“高考”）
- 学期阶段“放假入口”占位
  - 修改：[HomeScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Home/HomeScreen.cs)
  - 逻辑：当 `CurrentProgress == Semester` 且 `SemesterIndex` 处于 (0, TotalSemesters) 区间时，在入口区添加“放假活动”按钮，并弹出占位说明（后续接小游戏/支线任务）

#### 学期界面（Semester）

- 新增：学期推进临时版界面（用于打通“学期结束 → 回 Home 放假 → 继续下一学期”的混合式链路）
  - 新增：[SemesterScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Semester/SemesterScreen.cs)
  - 路由接入：[ScreenRouter.cs](file:///z:/gaokao/newschool/Assets/Scripts/UI/ScreenRouter.cs)
  - 交互：点击“结束本学期”会 `SemesterIndex++` 并回 Home；当 `SemesterIndex >= TotalSemesters` 时将进度推进到 `Gaokao`

#### 全局状态（GameState）

- 新增学期进度字段：
  - 修改：[GameState.cs](file:///z:/gaokao/newschool/Assets/Scripts/Core/GameState.cs)
  - 字段：`SemesterIndex`（已完成学期数）、`TotalSemesters`（默认 6）

#### 选科界面（Subject）

- 新增：选科正式界面（首选 1 + 再选 2），确认后落到 Home（混合式第一落点）
  - 新增：[SubjectScreen.cs](file:///z:/gaokao/newschool/Assets/Scripts/Features/Subject/SubjectScreen.cs)
  - 路由接入：[ScreenRouter.cs](file:///z:/gaokao/newschool/Assets/Scripts/UI/ScreenRouter.cs)
  - 数据落库：`GameState.FirstSubject`、`GameState.SecondSubjects`，并将 `CurrentProgress` 推进到 `Home`

#### 开发环境清理

- `.com-unity-codely.json` 是本地心跳文件，会持续变化导致仓库永远变脏
  - 修改：[.gitignore](file:///z:/gaokao/newschool/.gitignore)
  - 处理：已加入 ignore，并从仓库移除跟踪

### 相关提交（main）

- 7c0b518：实现主界面Home
- 24054b2：忽略本地心跳文件
- 3fa495a：更新UI与引导资源（包含家庭修复/省市配色等）

### 下一步计划（回家继续）

- Home 的“放假/实习小游戏”落地方式（建议）
  - 学期结束：在学期结束回 Home 后，通过“放假活动”按钮进入小游戏/任务链（当前已做入口与占位弹窗）
  - 大学阶段：在 University 阶段周期性回 Home，展示“实习任务/小游戏入口”
- 把 Subject（选科）从占位页做成正式界面，并在确认选科后落到 Home（混合式第一落点）
- 逐步补齐：Semester / Gaokao / Volunteer / University 的“阶段结束回 Home”机制与事件入口
