# 计划：省份合规改造 + 主界面功能补全 + 文案中性化

## 摘要

本次计划完成 10 项改动：
1. 省份界面删除"难度/地狱"等风险词汇，改用合规的中性分类（考试模式类型）
2. 右上角 `?` 按钮改为提示弹窗（解释各颜色含义）
3. 全局属性名中性化（智力→学习能力、心理→情绪管理、社交→人际关系、健康→健康状态）
4. 新增人物信息界面（PlayerInfoScreen）
5. 新增商店界面（ShopScreen）
6. 大学时光按钮灰显逻辑（未完成志愿时灰色，点击提示）
7. 成就殿堂入口补全
8. 活动入口常驻化
9. 决胜高考、志愿选择进入线性主线流程
10. 人生启程含义说明

---

## 阶段分析

### 1. 省份界面合规改造

**现状**（ProvinceScreen.cs）：
- 每个省份标记为 "轻松 / 正常 / 地狱" 三级
- 展示 `难度系数 0.88` 此类数值
- 引导文案和副标题多次提到"难度"、"战场"
- 使用 diff-easy / diff-normal / diff-hell 分类

**合规风险**：
- "地狱" 标签 + 具体地级市绑定 → 地域歧视/地域偏见风险
- "难度系数"让人直接联想到省份高下之分

**修改方案**：
- 删除 `Difficulty` 数值、`DiffText` 标签、`DiffClass` 分类
- ProvinceOption 精简为 4 个字段：`Name`、`Mode`（考试模式）、`Color`（卡片颜色）、`Description`（简要说明）
- 考试模式（Mode）使用合规描述，例如：
  - `"统一卷"` / `"自主命题"` / `"试点模式"`
- 卡片颜色按考试模式分类：统一卷 = 蓝色、自主命题 = 绿色、试点 = 橙色
- `?` 按钮点击时弹出说明框，解释各颜色含义

**涉及文件**：
| 文件 | 改动 |
|------|------|
| `ProvinceScreen.cs` | ProvinceCatalog 30 个省份数据重写；删除所有难度函数；重写卡片渲染 |
| `GameState.cs` | 删除 `SelectedProvinceDifficulty` 和 `SelectedProvinceDifficultyText` 字段 |
| `GuideService.cs` | 涉及 Province 的引导文案修改 |
| `HomeScreen.cs` | `?` 按钮 onClick 改为弹出合规说明 |

---

### 2. 右上角 ? 图标 → 提示弹窗

**现状**：`?` 按钮点击后只弹出 Toast "玩法百科开发中"。

**改为**：HomeScreen 中 `?` 按钮 onClick 调用 `ShowRulesHelpDialog()`：

```
📋 考试模式说明
━━━━━━━━━━━━━━━
蓝色卡片  统一卷模式
  采用全国统一命题，标准统一
绿色卡片  自主命题模式
  由本省自行命题，风格各有特点
橙色卡片  试点模式
  率先尝试新高考方案，方向较新

不同模式的考试流程和计分方式有差异，
但不会影响你的选择本身。
```

省份界面的 `?` 按钮也同步为此弹窗。

---

### 3. 全局属性中性化

| 原名称 | 新名称 | 涉及位置 |
|--------|--------|---------|
| 智力 (Intelligence) | 学习能力 (Learning) | GameState 字段名、HomeScreen 展示、FamilyScreen UI 标签、FamilyScreen 初始化数据 |
| 心理 (Psychology) | 情绪管理 (Emotion) | 同上 |
| 社交 (Social) | 人际关系 (Social) | 同上 |
| 健康 (Health) | 健康状态 (Health) | 同上 |

**注意**：
- 字段名不改（保持 `StatIntelligence` 等），只改中文展示标签和注释
- FamilyScreen 的家庭背景初始化数据中，数值不变，标签改
- 所有图标 emoji 同步更新：🧠→📖、💜→😊、💬→🤝、💪→🫀

---

### 4. 新增人物信息界面（PlayerInfoScreen）

**原因**：主界面 summaryText 只有一行属性文字，展示不完整。

**方案**：
- 新增 `ScreenType.PlayerInfo` 到 ScreenRouter 枚举
- 新增 `HomeButtonType.PlayerInfo` 到 GameState（或者直接用顶部头像区域点击进入）
- PlayerInfoScreen 展示：
  - 头像 + 名字 + 性别
  - 四维属性（带进度条）
  - 金币余额
  - 当前省份 / 家庭背景 / 选科
  - 已装备物品列表
  - 关闭按钮返回主界面

**入口**：主界面顶部人物卡（头像+名字区域）点击进入

---

### 5. 新增商店界面（ShopScreen）

**原因**：主界面有"背包物品"按钮但无 Screen 实现；学期前需要购买物品影响属性的逻辑。

**方案**：
- 新增 `ScreenType.Shop` 到枚举
- 现有 `HomeButtonType.Equipment` 重命名为 `Shop`（中文：`"商城"`），指向新 ShopScreen
- ShopScreen 内容：
  - 金币余额
  - 物品列表（文具类、营养品、辅导书等）
  - 物品效果描述
  - 购买确认
  - 已购物品存入 GameState 背包
- 购买后回到主界面，属性自动更新

**解锁条件**：选科完成后解锁（同现有 TalentTree/Semester）

---

### 6. 大学时光灰显

**现状**：University 按钮在志愿完成后直接解锁并创建。

**改为**：
- "大学时光" 在未完成志愿前**创建但灰显**（`interactable = false`）
- 点击灰显按钮弹出 Toast："完成志愿抉择后开启大学生活"
- 志愿完成后 `interactable = true`

---

### 7. 成就殿堂入口

**现状**：Achievements 按钮被 RebuildButtons 过滤掉，右上角也没有入口。

**方案**：
- 右上角新增 `🏆` 图标按钮（与 ?、⚙ 并列）
- 点击后 Toast："成就殿堂功能开发中，后续开放"
- 成就系统本身暂不实现

---

### 8. 活动入口

**现状**："放假活动"只在学期间隙动态插入，平时不可见。

**改为**：
- 新增 `HomeButtonType.Activity` 枚举值
- 主界面底部按钮新增"活动中心"按钮（始终可见）
- 学期间隙时按钮亮显可点；非假期时灰显，提示"学期中暂不开放"
- 点击进入活动页（先做 RuntimePlaceholderScreen，后续开发）

**活动规划**：
- 假期打工（赚金币）
- 社交事件
- 短测挑战
- 后续支线任务

---

### 9. 决胜高考、志愿选择 → 线性主线

**现状**：高考(Gaokao)和志愿(Volunteer)在 HomeScreen 底部按钮中，但用户认为它们是一次性流程，不该常驻。

**方案**：
- 高考和志愿按钮不在 Home 底部按钮中直接显示
- 学期完成后，"继续主线"按钮（continueButton）文字变为"决胜高考"
- 点击进入高考 → 考完回到主界面 → continueButton 变为"志愿选择"
- 完成志愿后 continueButton 回到普通状态
- 如想回顾已考完的高考/志愿结果，从右上角 ? 百科中查看

**主界面按钮过滤**：RebuildButtons 中过滤掉 `HomeButtonType.Gaokao` 和 `HomeButtonType.Volunteer`

---

### 10. 人生启程说明

**"人生启程"是什么**：对应 GameProgress.Career（毕业到 30 岁）

- 大学毕业后，玩家进入社会
- 根据学科、大学表现、属性，进入不同职业路径
- 毕业后到 30 岁的模拟：找工作、升职、创业等
- 该阶段结束后进入人生总结（Summary）

命名保持"人生启程"，按钮点击后进入 CareerScreen（目前占位，后续实现）。

---

## 实施清单

| # | 子任务 | 优先级 |
|---|--------|:------:|
| 1 | ProvinceScreen 删除难度系统，改用考试模式+颜色分类 | P0 |
| 2 | ProvinceScreen 新增 30 个省份的新数据 | P0 |
| 3 | GameState 删除难度相关字段 | P0 |
| 4 | HomeScreen `?` 按钮 → 弹窗说明考试模式颜色 | P0 |
| 5 | ProvinceScreen `?` 按钮 → 同步弹窗 | P0 |
| 6 | 全局属性标签改名（中文 4 个 + emoji 4 个） | P0 |
| 7 | 新增 ScreenType.PlayerInfo + PlayerInfoScreen | P0 |
| 8 | 主界面顶部人物卡点击进入人物信息 | P0 |
| 9 | HomeButtonType.Equipment → Shop，新增 ScreenType.Shop + ShopScreen | P0 |
| 10 | 大学时光灰显逻辑 + "完成志愿后开启"提示 | P0 |
| 11 | 右上角新增 🏆 按钮（成就） | P1 |
| 12 | 新增 HomeButtonType.Activity + "活动中心"按钮 | P1 |
| 13 | 高考/志愿从主界面底部移除，改为 continueButton 驱动 | P1 |
| 14 | GuideService 中 Province 引导文案修改 | P1 |
| 15 | 删除 ProvinceScreen 中所有难度排序/颜色函数 | P0 |

---

## 验证步骤

1. 跑 Play → 启动 → 创建人物 → 家庭 → 选省份
2. 省份页应看到**蓝色/绿色/橙色**卡片，无"难度"字样
3. 点击 ? 按钮 → 看到考试模式颜色说明弹窗
4. 家庭背景页面：属性标签为"学习能力/情绪管理/人际关系/健康状态"
5. 选科完成 → 进入主界面 → 顶部人物卡可点击 → 进入人物信息界面
6. 主界面底部应有"商城"、"成长赋能"、"校园日常"按钮
7. "大学时光"按钮灰显，点击提示"完成志愿后开启"
8. 右上角有三图标：? (百科) · 🏆 (成就) · ⚙ (设置)
9. 底部"活动中心"按钮 → 非假期灰显

---

## 假设与决策

- 省份数据变更只影响 ProvinceScreen 和 GameState，学期/高考逻辑暂不依赖难度值
- 属性字段名不改（StatIntelligence 等），只改中文标签和 emoji
- 商店物品数据先做简单硬编码列表
- PlayerInfoScreen 和 ShopScreen 先用 RuntimePlaceholderScreen 复用模式（运行时自建 UI）
- 高考/志愿移入 continueButton 流程后，原来的 Home 按钮不再显示，不删除枚举
