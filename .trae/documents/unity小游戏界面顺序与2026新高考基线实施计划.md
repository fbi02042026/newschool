# 《我的高考志愿模拟器》Unity 小游戏界面顺序与 2026 新高考基线实施计划

## 一、Summary

- 目标：基于 `Assets/策划` 下的需求文档，在当前 Unity 2022.3 URP 2D 项目中，按界面先后顺序逐个实现可试玩的原生小游戏界面；每完成一个界面即停下供你确认，确认后再继续下一个。
- 载体：不走网页版，直接使用 Unity 原生 UI（UGUI）实现。
- 视觉方向：1242×2760 竖屏参考分辨率，自适应布局，2D Q 版软萌马卡龙风格；在“软萌”基础上增加“轻拟物纸片层次 + 高对比文字可读性”，避免纯可爱风导致信息界面难读。
- 政策范围：先落地“全国通用 2026 新高考基线”，不在第一阶段做省份细则全量分叉，但数据结构必须为后续按省份扩展留口。
- 执行原则（✅ 已确认采用「线性新手流程 + 主界面Hub」混合方案，玩家体验优先）：
  - **第一次进入游戏（新手期）**：强制线性流程 `启动画面 → 创建人物(性别+昵称) → 家庭背景 → 选择省份 → 选科`，确保玩家理解2026新高考"选科决定专业池"的核心机制
  - **完成新手流程后**：进入**主界面Hub**，所有系统（天赋树/学期/高考/志愿/大学/30岁）通过Hub按钮进入，按钮按进度解锁
  - 先搭建全局UI框架（含Hub、路由、适配），再按新手流程顺序逐个实现界面，每完成一个即停下确认

## 二、Current State Analysis

### 2.1 当前项目事实

- 当前工程为 Unity/Tuanjie 2022.3.62t8，URP 2D 已配置。
- `Packages/manifest.json` 已包含 `com.unity.ugui`、`com.unity.textmeshpro`、`com.unity.test-framework`，满足原生 UI 开发基础条件。
- 当前 `Assets` 基本为空项目，仅有：
  - `Assets/Scenes/SampleScene.scene`
  - `Assets/策划/*.md|*.docx`
  - URP 相关设置资源
- 当前未发现任何业务脚本、UI 预制体、配置资源、动画控制器或数据层代码，属于“从零搭建业务层”的状态。

### 2.2 本地策划文档已确认的信息

- `Assets/策划/网页版开发计划.md`
  - 明确了 8 个界面的顺序与逐屏验收流程。
- `Assets/策划/大学数据库与志愿填报系统_V6.5_最终同步版.md`
  - 明确了大学分档、志愿策略、录取概率、专业系统、3+1+2 选科硬锁死规则。
- `Assets/策划/肉鸽化改造方案_V6.5.md`
  - 明确了 5 槽装备、标签共鸣、学期挂机 + 微决策流。
- `Assets/策划/毕业到30岁系统与成就系统_V6.5.md`
  - 明确了毕业选择、工作阶段、成就系统、30 岁总结页。

### 2.3 已核实的 2026 高考公开信息基线

- 教育部 2026 招生工作通知延续“稳中求进、规范招生、优化专业结构、扩大优质本科供给”的总方向，适合转译为游戏中的“专业导向更强、按专业填报更重要”的设计基线。
- 2026 年全国新高考主流仍以 `3+1+2` 为核心模式之一，通用结构可抽象为：
  - 语数外 3 门统考
  - 物理/历史 2 选 1 作为首选科目
  - 政/地/化/生 4 选 2 作为再选科目
  - 满分 750
  - 首选科目按原始分计入，再选科目采用等级赋分
- 多省 2026 细则继续强调：
  - 选科决定专业池
  - 专业组/专业要求校验重要
  - 平行志愿或专业组填报的规则理解比单纯“看学校名气”更重要
- 结论：第一阶段游戏需要把“选科锁专业、专业要求可校验、志愿存在风险梯度”做对；省份差异、批次差异、听力一年两考等作为后续扩展，不在第一轮界面实现中压满复杂度。

## 三、Assumptions & Decisions

### 3.1 产品决策

- 先做 Unity 原生小游戏，不做 HTML 原型。
- 先做单人离线可试玩版本，不接联网、不接实名、不接真实志愿服务。
- 优先完成“可玩 + 可验证 + 可迭代”的竖屏原型，不在首轮追求最终美术资源完整度。
- 按界面逐个推进，每个界面完成后保留返回入口，方便回归测试。
- 起名策略（第一阶段）：
  - 默认提供“平台昵称自动带入”的占位实现：读取本机 Unity 的 `SystemInfo.deviceName` 作为默认展示名（仅本地使用），并提供“随机名字候选 + 手动输入”两种改名方式。
  - 后续如要接入各小游戏平台（抖音/快手/微信等）玩家昵称，再新增平台桥接层，不影响现有 UI/数据结构。

### 3.2 UI/视觉决策

- 基准分辨率固定为 `1242 x 2760` 竖屏。
- `CanvasScaler` 使用 `Scale With Screen Size`，以竖屏为主，自适应刘海与长屏。
- 风格采用“软萌马卡龙”主基调，但 UI 信息层采用：
  - 浅底色块
  - 清晰深色字
  - 轻描边按钮
  - 圆角卡片
  - 轻微弹性动画
  这样兼顾可爱和大量信息阅读。
- 动效先使用 Unity 原生 `CanvasGroup / DOTween 替代方案暂不引入 / Animator` 的轻量过渡；如后续需要更顺手的补间，再单独评估是否加库。

### 3.3 系统决策

- 采用“单主场景 + 多 Screen Root 面板切换”作为第一阶段方案，而不是一屏一个场景。
  - 原因：界面连续、状态共享多、逐屏调试更快、回退测试更容易。
- 新增“主界面 Hub（Home）”作为所有系统入口，但不替代新手流程：
  - 新玩家：必须完成创建人物/家庭/省份/选科后，才进入 Hub。
  - 老玩家（后续有存档时）：可直接进入 Hub。
  - Hub 上的按钮按进度解锁（例如未选科不能进入天赋树/学期主界面）。
- 数据层采用“代码状态对象 + ScriptableObject 配置资源”混合方案。
  - 固定表：学校、专业、规则、事件池、天赋节点、装备模板用 ScriptableObject。
  - 运行态：玩家状态、当前周目、已选科目、已解锁志愿数、当前界面等用运行时 `GameState`。
- 第一阶段不接存档文件；先预留接口，至少支持“本次运行继续”。
- 志愿系统在游戏中采用“现实抽象化”而非全省真实还原：
  - 保留“冲/稳/保”
  - 保留“选科要求校验”
  - 保留“服从调剂”
  - 保留“录取概率与位次/分差相关”
  - 不在第一阶段引入各省复杂批次表

## 四、Proposed Changes

### 4.1 目录与资源骨架

后续执行阶段统一新增如下结构：

- `Assets/Scenes/`
  - `MainGame.unity`
- `Assets/Scripts/Core/`
  - `AppBootstrap.cs`
  - `GameState.cs`
  - `GameConstants.cs`
- `Assets/Scripts/UI/`
  - `ScreenRouter.cs`
  - `ScreenBase.cs`
  - `AdaptiveCanvasController.cs`
  - `PopupController.cs`
- `Assets/Scripts/UI/Common/`
  - 通用按钮、卡片、页签、进度条、Toast、弹窗组件
- `Assets/Scripts/Features/Launch/`
- `Assets/Scripts/Features/Profile/`（创建人物：性别/昵称）
- `Assets/Scripts/Features/Family/`
- `Assets/Scripts/Features/Province/`
- `Assets/Scripts/Features/SubjectSelection/`
- `Assets/Scripts/Features/Home/`（主界面 Hub）
- `Assets/Scripts/Features/TalentTree/`
- `Assets/Scripts/Features/Semester/`
- `Assets/Scripts/Features/Gaokao/`
- `Assets/Scripts/Features/Volunteer/`
- `Assets/Scripts/Features/UniversityCareer/`
- `Assets/Scripts/Data/`
  - 运行态和配置读取脚本
- `Assets/Data/Configs/`
  - 学校、专业、规则、事件、天赋、装备、家庭背景等配置资源
- `Assets/Prefabs/UI/Common/`
  - 通用 UI 预制体
- `Assets/Prefabs/UI/Screens/`
  - 各界面根预制体
- `Assets/Art/UI/`
  - 临时底图、按钮、角标、背景装饰
- `Assets/TMP/`
  - TMP 字体资源

### 4.2 全局基础层

第一批实现不直接做具体业务，而是先搭基础框架，避免后面每屏返工。

#### 需要创建的内容

- `MainGame.unity`
  - 主场景，包含唯一主 Canvas、EventSystem、UI Camera、全局管理器
- `AppBootstrap.cs`
  - 初始化游戏状态、装载基础配置、进入启动界面
- `GameState.cs`
  - 保存玩家基础属性、当前流程阶段、当前周目、已选家庭背景、已选科目、天赋、志愿、录取结果等
- `ScreenRouter.cs`
  - 负责 8 个主界面的切换、返回与参数传递
- `AdaptiveCanvasController.cs`
  - 处理参考分辨率、锚点、安全区域、超长屏适配
- 通用样式预制体
  - 顶部标题栏
  - 主按钮/次按钮
  - 卡片选择项
  - 弹窗
  - 信息提示条
  - 属性条

#### 这样做的原因

- 用户要求“一个界面没问题再做下一个”，但所有界面都会复用相同框架。
- 若不先统一适配和样式，第 2 屏以后返工概率会非常高。

### 4.3 2026 新高考基线数据层

第一轮要先把“规则骨架”做对，后续界面只读配置。

#### 需要创建的内容

- `GaokaoRuleConfig`
  - 全国通用 2026 基线规则
  - 包含 `3+1+2`、总分 750、首选科目/再选科目描述、再选科目为等级赋分的提示信息
- `SubjectRequirementConfig`
  - 专业/专业类对科目要求的抽象：
    - 首选物理
    - 首选历史
    - 物理+化学
    - 历史+政治
    - 不限
- `UniversityTierConfig`
  - S/A/B/C/D/E 六档大学体系
- `MajorCategoryConfig`
  - 计算机、医学、金融、法学、教育、文史哲、设计艺术、农林生物、体育军警等大类
- `VolunteerRuleConfig`
  - 冲/稳/保
  - 志愿顺序修正
  - 热门专业修正
  - 调剂修正
- `ProvinceConfig`（第一阶段轻量实现）
  - 省份列表与显示名、模式标签（3+1+2 / 3+3 / 传统）
  - 第一阶段只用于 UI 展示与后续扩展预留，不强绑定具体批次表

#### 第一阶段要做的政策映射

- 必做：
  - 3+1+2 选科表达
  - 选科锁专业
  - 专业/院校要求校验
  - 志愿风险梯度
- 只做文案占位，不做复杂模拟：
  - 各省批次差异
  - 听力一年两考
  - 少数民族语言加考
  - 综合评价批次

### 4.4 逐屏实施顺序

#### 界面 1：启动画面

目标：

- 建立首屏观感和整体基调。

内容：

- 标题、副标题、版本号
- `新游戏`
- `继续游戏`（若暂无真实存档，则先置灰或显示“即将开放”）
- `策划来源/2026规则基线说明` 小入口

实现文件：

- `Assets/Prefabs/UI/Screens/LaunchScreen.prefab`
- `Assets/Scripts/Features/Launch/LaunchScreenController.cs`

验收标准：

- 1242×2760 竖屏布局稳定
- 至少在常见长屏/短屏比例下不穿帮
- 点击 `新游戏` 能进入家庭背景页

修正：

- 点击 `新游戏` 进入“创建人物”页（性别+起名），不是直接进入家庭背景页

#### 界面 2：创建人物（性别 + 起名）

目标：

- 建立玩家身份与代入感，并保证后续所有界面可统一显示昵称/头像占位。

内容：

- 性别选择：男 / 女（Q 版头像占位，可后续替换更精致的立绘）
- 昵称输入：
  - 默认昵称：设备名占位 + 提示“可修改”
  - `随机一个`：从名字库抽取
  - `确认`：写入 `GameState`

实现文件：

- `Assets/Prefabs/UI/Screens/ProfileCreateScreen.prefab`
- `Assets/Scripts/Features/Profile/ProfileCreateController.cs`
- `Assets/Data/Configs/NamePoolConfig.asset`

验收标准：

- 性别/昵称写入 `GameState`，并能在后续页面展示
- 昵称输入限制：长度、非法字符提示

#### 界面 3：家庭背景选择

目标：

- 让玩家完成开局身份选择，并影响初始属性。

内容：

- 5 张背景卡
- 每张卡展示初始属性变化、描述、推荐玩法
- 确认按钮与二次确认弹窗

实现文件：

- `Assets/Prefabs/UI/Screens/FamilyBackgroundScreen.prefab`
- `Assets/Scripts/Features/Family/FamilyBackgroundController.cs`
- `Assets/Data/Configs/FamilyBackgroundConfig.asset`

验收标准：

- 单选逻辑正确
- 卡片选中态与取消态清晰
- 确认后 `GameState` 正确记录家庭背景

#### 界面 3：选科

目标：

- 正确传达 2026 新高考“选科决定专业池”的核心理念。

内容：

- 首选科目：物理 / 历史 二选一
- 再选科目：政/地/化/生 四选二
- 动态展示“可报专业方向池”
- 错误组合提示、确认弹窗、硬锁死说明

实现文件：

- `Assets/Prefabs/UI/Screens/SubjectSelectionScreen.prefab`
- `Assets/Scripts/Features/SubjectSelection/SubjectSelectionController.cs`
- `Assets/Scripts/Features/SubjectSelection/SubjectRequirementPreview.cs`

验收标准：

- 首选只能二选一，再选只能四选二
- UI 能实时刷新可报方向提示
- 确认后不可无提示覆盖

#### 界面 4：选择省份

目标：

- 让玩家明确“省份不同，规则可能不同”，但第一阶段仍按全国通用基线推进，避免复杂化。

内容：

- 省份列表（支持搜索/常用省份置顶）
- 每个省份显示“考试模式标签”：3+1+2 / 3+3 / 传统（仅做提示）
- 选择后写入 `GameState`
- 提示文案：后续会逐步补齐各省细则，目前先按全国基线模拟

实现文件：

- `Assets/Prefabs/UI/Screens/ProvinceSelectScreen.prefab`
- `Assets/Scripts/Features/Province/ProvinceSelectController.cs`
- `Assets/Data/Configs/ProvinceConfig.asset`

验收标准：

- 省份选择可保存并在主界面显示
- 不影响后续流程稳定性

#### 界面 5：选科

（原“界面 3：选科”的内容整体顺延到此处，逻辑不变）

#### 界面 6：主界面 Hub（Home）

目标：

- 把所有系统入口集中在一个“主界面”，符合你希望的“其它界面都在主界面的按钮里”的交互习惯，同时保留流程锁定与引导。

内容：

- 顶部：玩家昵称 + 性别头像占位 + 当前省份
- 属性摘要卡：智力/社交/健康/心理/压力/金钱
- 入口按钮（按进度解锁）：
  - `天赋树`
  - `学期推进`
  - `高考`（锁定，直到学期达到高三末）
  - `志愿填报`（锁定，直到高考出分）
  - `大学/毕业/30岁`（锁定，直到录取完成）
  - `设置/帮助`

实现文件：

- `Assets/Prefabs/UI/Screens/HomeHubScreen.prefab`
- `Assets/Scripts/Features/Home/HomeHubController.cs`

验收标准：

- 从选科确认后自动进入 Hub
- Hub 的按钮锁定逻辑正确，未解锁不可进入对应界面
- 进入/返回各系统界面后能回到 Hub

#### 界面 7：天赋树

目标：

- 把“选科 -> 天赋 -> 未来路径”关联起来。

内容：

- 多分支天赋树
- 节点消耗与点亮效果
- 与选科/专业方向联动说明
- 重置或取消按钮

实现文件：

- `Assets/Prefabs/UI/Screens/TalentTreeScreen.prefab`
- `Assets/Scripts/Features/TalentTree/TalentTreeController.cs`
- `Assets/Data/Configs/TalentTreeConfig.asset`

验收标准：

- 节点连线与解锁前置逻辑正确
- 点数消耗正确
- 已点亮状态可视化明确

#### 界面 8：学期主界面

目标：

- 这是核心玩法页，后续所有数值循环都在这里展开。

内容：

- 5 个装备槽
- 当前属性摘要
- 共鸣标签与等级
- 学期进度条
- 微决策弹窗入口
- 下一阶段推进按钮

实现文件：

- `Assets/Prefabs/UI/Screens/SemesterMainScreen.prefab`
- `Assets/Scripts/Features/Semester/SemesterMainController.cs`
- `Assets/Scripts/Features/Semester/EquipmentLoadoutController.cs`
- `Assets/Scripts/Features/Semester/SynergyPreviewController.cs`
- `Assets/Data/Configs/EquipmentConfig.asset`
- `Assets/Data/Configs/EventPoolConfig.asset`

验收标准：

- 装备槽 UI 清晰
- 共鸣预览和属性变化可见
- 微决策弹窗能够触发并影响状态

#### 界面 9：高考

目标：

- 制造阶段性高潮，完成成绩揭晓。

内容：

- 高考倒计时结束
- 成绩结算动画
- 分数、位次/档次、科目结果摘要
- 解锁下一步志愿填报

实现文件：

- `Assets/Prefabs/UI/Screens/GaokaoResultScreen.prefab`
- `Assets/Scripts/Features/Gaokao/GaokaoResultController.cs`
- `Assets/Scripts/Features/Gaokao/ScoreCalculator.cs`

验收标准：

- 分数计算流程可复现
- 动画结束后结果稳定显示
- 能携带结果进入志愿填报页

#### 界面 10：志愿填报

目标：

- 这是最关键的“玩中学”界面，要把真实规则抽象成可理解玩法。

内容：

- 志愿槽位（按周目解锁）
- 院校卡 / 专业卡选择
- 冲/稳/保标签
- 调剂开关
- 专业要求校验
- 录取风险提示
- 最终投档结算

实现文件：

- `Assets/Prefabs/UI/Screens/VolunteerScreen.prefab`
- `Assets/Scripts/Features/Volunteer/VolunteerController.cs`
- `Assets/Scripts/Features/Volunteer/VolunteerSlotView.cs`
- `Assets/Scripts/Features/Volunteer/AdmissionResolver.cs`
- `Assets/Data/Configs/UniversityDatabase.asset`
- `Assets/Data/Configs/MajorDatabase.asset`

验收标准：

- 不符合选科要求的专业不可提交
- 冲/稳/保风险提示直观
- 录取结果可解释，不是纯黑盒

#### 界面 11：大学 / 毕业 / 30岁

目标：

- 先做“简化闭环版”，保证第一轮能从高中玩到人生总结。

内容：

- 大学阶段简化页
- 毕业抉择页
- 职业/深造结果摘要
- 30 岁人生报告页

实现文件：

- `Assets/Prefabs/UI/Screens/UniversityCareerScreen.prefab`
- `Assets/Scripts/Features/UniversityCareer/UniversityCareerController.cs`
- `Assets/Scripts/Features/UniversityCareer/LifeSummaryGenerator.cs`
- `Assets/Data/Configs/CareerRouteConfig.asset`
- `Assets/Data/Configs/AchievementConfig.asset`

验收标准：

- 能形成完整一轮闭环
- 总结页能读取前序关键决策并生成结果

## 五、界面制作节奏与停点

执行阶段严格按以下节奏推进：

1. 先完成全局基础层 + 界面 1 所需最小骨架。
2. 提供界面 1 预览与说明，等待你确认。
3. 你确认无问题后，再实现界面 2。
4. 后续每个界面都重复“实现 -> 自测 -> 交付确认 -> 再继续”的节奏。

额外规则：

- 每完成一个新界面，都要保证前面界面仍然可以回退测试。
- 第 5 屏开始，若发现数据结构不足，优先补公共层，不直接在单屏里硬编码。
- 第 7 屏志愿填报前，若发现 2026 规则需要补充，再补一次官方信息核对，但不改“全国通用基线”的范围定义。

## 六、Verification Steps

### 6.1 每屏通用验证

- 分辨率验证：
  - 1242×2760 基准
  - 1080×2400
  - 1179×2556
  - 1440×3200
- 交互验证：
  - 所有主按钮可点
  - 返回路径存在
  - 弹窗关闭不丢状态
- 文案验证：
  - 术语统一使用“新高考 / 选科 / 专业要求 / 志愿 / 调剂 / 录取”
  - 2026 规则说明不写成具体省份独占规则

### 6.2 关键逻辑验证

- 选科页面：
  - 首选与再选数量限制正确
  - 物理/历史切换会刷新专业池提示
- 天赋页：
  - 前置节点、点数消耗、重置逻辑正确
- 学期主界面：
  - 装备替换、共鸣等级、属性预览正确
- 高考页：
  - 结算结果受前置状态影响
- 志愿页：
  - 不符合要求的志愿不能提交
  - 调剂、热度、志愿顺序参与结果计算
- 总结页：
  - 读取完整流程状态并生成合理结局

### 6.3 建议的补充验证

- 若后续引入临时美术图集，检查低端机 UI overdraw 和字体清晰度。
- 若后续加存档，再补充“退出重进恢复当前界面”验证。

## 七、Risks

- 风险 1：当前项目没有任何业务骨架，若跳过公共层直接做单屏，后续返工量会很大。
- 风险 2：策划文档信息量大，若第一轮就追求完整还原大学/职业系统，会拖慢前 7 个关键界面的交付。
- 风险 3：2026 高考省份规则差异真实存在，若首轮就全量兼容，会显著增加复杂度，影响逐屏验证节奏。

对应策略：

- 第一轮只固化全国基线。
- 第 8 屏先做简化闭环版。
- 先统一数据模型，再逐屏扩功能。

## 八、First Execution Slice

计划获批后，立即执行以下最小切片，不再重新讨论：

1. 创建主场景与全局 Canvas/适配框架。
2. 建立全局 UI 样式与 `ScreenRouter`。
3. 实现 `启动画面` 完整可点版，并打通到 `创建人物`。
4. 给出预览，等你确认。
