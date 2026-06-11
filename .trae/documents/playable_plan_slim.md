# 可试玩版本（省图方案）计划

目标：在尽量少生成图片的前提下，把主流程做成可完整跑通、可反复试玩的版本；每屏“功能确认 → 少量美术替换 → Git 提交存档”，再进入下一屏。

## 1. 当前状态

已完成并做过美术更新（样例屏）：
- Launch（启动页）
- Profile（创建人物）

待实现（11 屏）：
- Family（家庭背景）
- Province（选择省份）
- Subject（选科）
- Home（主界面 Hub）
- TalentTree（天赋树）
- Semester（学期主界面）
- Gaokao（高考）
- Volunteer（志愿填报）
- University（大学）
- Career（毕业到30岁）
- Summary（人生总结）

## 2. 省图方案的图片策略（最省）

原则：
- 能用代码画（渐变/圆角/阴影/描边/分割线）就不出图。
- 面板类统一走 9-slice（九宫格）通用底图，所有界面复用。
- 只给每个界面出 1 张“主插画”用于区分主题，其余用通用图标表达。

### 2.1 通用资产（一次生成，全项目复用）

9-slice（九宫格）通用底图（3 张）：
- button_primary_9s
- card_9s
- panel_popup_9s

通用图标包（先做 24 张，后续按需补）：
- 导航/通用：back / close / info / settings / help / confirm / cancel / random
- 状态/提示：lock / warning / tip / check
- 资源/系统：coin / badge / book / location / calendar / heart / lightning
- 学期核心：study / rest / training / social / exam / event

通用装饰（6 张，可选）：
- cloud / sparkle / bubble / divider / dot / ribbon

通用资产小计：约 33 张

### 2.2 每屏主插画（11 张）

每个未实现界面 1 张主插画，放在页中部或页头作为主题识别。

主插画小计：11 张

### 2.3 选科学科图标（8 张）

- 物理、历史（2）
- 6 个二选科目（6）

学科图标小计：8 张

### 2.4 总计（省图方案）

33（通用） + 11（主插画） + 8（学科） ≈ **52 张**

其中真正“每屏独有”的约 **11 张**，其余全复用。

## 3. 开发顺序（可试玩优先）

按“当前链路最自然的断点”推进：
1) Family → 2) Province → 3) Subject → 4) Home
5) TalentTree → 6) Semester → 7) Gaokao → 8) Volunteer
9) University → 10) Career → 11) Summary

## 4. 每屏固定节奏（必须执行）

对每个界面都按下面节奏循环：
1. 临时美术功能版：先把流程/交互/状态写入/跳转打通（允许无图或占位图）。
2. 你试玩确认：你说“没问题/可以继续”后才进入下一步。
3. 省图美术替换：只补“该屏主插画 + 必要通用图标接入”，不做过度细化。
4. 最终检查：不破坏已确认功能。
5. Git 提交存档：提交信息包含“界面名 + 临时版/美术替换”。

## 5. 里程碑提交规则

提交粒度：
- 每个界面至少 2 个提交：
  - `feat(ui): <Screen> temp functional`
  - `art(ui): <Screen> slim art swap`
- 若你希望更细，也可以拆成 3 个（功能/修复/美术）。

## 6. 下一步（马上执行）

先做一份 UI Kit（省图通用资产）的最小集合（9-slice 3 张 + icon 24 张 + decor 6 张），提交为一次里程碑后：
- 开始实现 `Family` 的临时功能版。

