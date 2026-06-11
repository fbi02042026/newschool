# Unity UI 布局说明文档

> 本文档为 Unity UI 设计师提供每个界面的元素列表、位置、尺寸及动画参考。
> 所有尺寸基于原设计 430px x ~764px（9:16）竖屏比例，Unity 中建议使用 Canvas Scaler 适配。

---

## 通用规范

- **画布尺寸**：宽 430，高按 9:16 比例（约 764）
- **安全边距**：左右 24px，底部 24px，顶部状态栏预留 44px
- **字体层级**：
  - 标题：28-32px，Weight 900
  - 副标题：20-24px，Weight 800
  - 正文：14-16px，Weight 400/600
  - 辅助说明：12-13px，Weight 400
  - 标签/徽章：11-12px，Weight 700
- **圆角规范**：大卡片 20px，中卡片 14px，小标签 10px，按钮 50px（胶囊形）
- **阴影**：卡片使用 `0 4px 24px rgba(0,0,0,0.06)`，弹窗使用 `0 8px 40px rgba(0,0,0,0.10)`

---

## 界面1：启动画面（Title Screen）

### 层级结构
```
Screen-Title (Full Screen)
├── BgDots (Particle Layer, z=0)
├── Decorations (4 floating emojis, z=1)
├── Content (Centered, z=2)
│   ├── TitleIcon (Emoji)
│   ├── TitleText
│   ├── SubTitle
│   └── ButtonGroup
│       ├── BtnNewGame (Primary)
│       └── BtnContinue (Outline, 条件显示)
└── VersionLabel (Bottom Center, z=2)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 | 动画 |
|------|------|------|-----------|------|
| BgDots | Particle System / 12个Image | 全屏随机分布 | 圆点 6-20px，opacity 0.15 | dotFloat: Y轴浮动 -6px，scale 1.0->1.1，周期 3-6s，随机延迟 |
| Decor_1 (📖) | Text/Image | top:12%, left:12% | 1.2rem | float: Y轴 -10px，周期 4s |
| Decor_2 (✏️) | Text/Image | top:18%, right:14% | 1.2rem | float: Y轴 -10px，周期 4s，延迟 1s |
| Decor_3 (🎒) | Text/Image | bottom:25%, left:16% | 1.2rem | float: Y轴 -10px，周期 4s，延迟 2s |
| Decor_4 (🏫) | Text/Image | bottom:30%, right:12% | 1.2rem | float: Y轴 -10px，周期 4s，延迟 3s |
| TitleIcon (🎓) | Text/Image | Center X, Y偏移 -120 | 3.5rem | float: Y轴 -10px，周期 3s |
| TitleText | Text | Center X, Y偏移 -60 | 1.7rem, Weight 900 | 渐变色文字（#FF8A80 -> #FF9800 -> #64B5F6） |
| SubTitle | Text | Center X, Y偏移 -20 | 0.78rem, #8D8D8D | 无 |
| BtnNewGame | Button | Center X, Y偏移 +40 | 宽 260, 高 50, 胶囊形 | hover: scale 1.02, pressed: scale 0.96 |
| BtnContinue | Button | Center X, Y偏移 +100 | 宽 260, 高 50, 胶囊形 | 同上新，默认隐藏（有存档时显示） |
| VersionLabel | Text | Bottom Center, bottom:1.2rem | 0.65rem, #8D8D8D, opacity 0.5 | 无 |

### 背景
- 渐变背景：`linear-gradient(180deg, #FFF0E6 0%, #FFF8F0 40%, #F3E5F5 100%)`

---

## 界面2：性别 + 起名（Gender & Name）

### 层级结构
```
Screen-Gender (Flex Column)
├── ScreenHeader
│   ├── StepBadge ("Step 1/5")
│   ├── Title ("👤 创建你的角色")
│   └── Subtitle ("请告诉我你的信息吧")
├── ScreenBody (Scrollable)
│   ├── GenderCards (Horizontal Flex)
│   │   ├── GenderCard-Male
│   │   └── GenderCard-Female
│   └── NameSection
│       ├── NameLabel
│       ├── NameInputWrap
│       │   ├── N-Icon (✍️)
│       │   └── InputField
│       └── NameHint (Error/Hint Text)
└── ScreenFooter
    └── BtnConfirm ("下一步 →", 默认禁用)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 | 交互 |
|------|------|------|-----------|------|
| StepBadge | Label | Header Center Top | 0.7rem, #FF8A80 bg, 胶囊形 | 无 |
| Title | Text | Header Center | 1.25rem, Weight 800 | 无 |
| Subtitle | Text | Header Center | 0.78rem, #8D8D8D | 无 |
| GenderCard-Male | Button | Body Top, Flex Left | Flex:1, 圆角 20px, 边框 2.5px #E8E8E8 | 点击选中：边框变 #64B5F6，背景变 #E3F2FD，阴影 glow |
| GenderCard-Female | Button | Body Top, Flex Right | 同上 | 同上 |
| G-Icon (🙋‍♂️/🙋‍♀️) | Text | Card Center Top | 2.8rem | 无 |
| G-Label | Text | Card Center Bottom | 0.95rem, Weight 700 | 无 |
| NameLabel | Text | Body Middle | 0.85rem, Weight 600, #3D3D3D | 无 |
| NameInputWrap | Panel | Body Middle | 圆角 20px, 边框 2.5px #E8E8E8, padding 1rem 1.2rem | Focus: 边框变 #64B5F6 |
| N-Icon (✍️) | Text | InputWrap Left | 1.5rem | 无 |
| InputField | InputField | InputWrap Center | 1.05rem, 无边框, 透明背景 | 输入时实时校验 |
| NameHint | Text | InputWrap Below | 0.7rem, #8D8D8D | 错误时变红色 #EF5350 |
| BtnConfirm | Button | Footer Center | 全宽, 胶囊形, 禁用态 opacity 0.4 | 性别+名字有效时启用 |

### 交互状态
- **GenderCard 选中态**：border-color = `#64B5F6`, background = `#E3F2FD`, box-shadow = `0 0 0 3px rgba(100,181,246,0.15)`
- **Input 焦点态**：border-color = `#64B5F6`
- **BtnConfirm 禁用态**：opacity 0.4, cursor = not-allowed
- **校验规则**：名字 2-8 字，过滤敏感词（参考 game-data.json BAD_WORDS）

---

## 界面3：家庭背景（Family Draw）

### 层级结构
```
Screen-Family (Flex Column)
├── ScreenHeader
│   ├── StepBadge ("Step 2/5")
│   ├── Title ("🏠 命运的起点")
│   ├── Subtitle
│   └── HelpBtn ("?", 右上角)
├── ScreenBody
│   ├── DrawStage (动画阶段)
│   │   ├── DrawCardOuter (Perspective 600px)
│   │   │   └── DrawCard
│   │   │       ├── D-Icon
│   │   │       ├── D-Name
│   │   │       └── D-Desc
│   │   ├── DrawProgressBar
│   │   └── DrawHint
│   └── FamilyResult (结果阶段, 默认隐藏)
│       └── FamilyHighlightCard
│           ├── CardRibbon ("NEW" 角标)
│           ├── F-Icon
│           ├── F-Name
│           ├── F-Desc
│           ├── F-Money (金币显示)
│           └── StatGrid (2x2)
│               ├── StatItem x4
└── ScreenFooter (默认隐藏)
    ├── BtnKeep ("✅ 就这个了！")
    └── BtnReroll ("🔄 免费重置（1次）")
```

### 元素详情

#### 抽卡动画阶段

| 元素 | 类型 | 位置 | 尺寸/样式 | 动画 |
|------|------|------|-----------|------|
| DrawCardOuter | Panel | Body Center | 宽 280, perspective 600 | 3D翻转容器 |
| DrawCard | Panel | Outer Center | 圆角 20px, 阴影 lg, 边框 3px #E8E8E8 | pop: scale 1.08, 边框变 #64B5F6, 发光阴影, 持续 80ms |
| D-Icon | Text | Card Top | 3.5rem | 每 100ms 切换随机家庭 icon |
| D-Name | Text | Card Middle | 1.3rem, Weight 800 | 同上，切换随机家庭名 |
| D-Desc | Text | Card Bottom | 0.75rem, #8D8D8D | 同上，切换随机描述 |
| DrawProgressBar | Slider/Image | Card Below | 宽 200, 高 6px, 圆角 3px | 0% -> 100%, 20步, 每步 100ms |
| DrawProgressFill | Image | Bar内部 | 渐变 #64B5F6 -> #FF8A80 | 宽度线性增长 |
| DrawHint | Text | Bar Below | 0.8rem, #8D8D8D | pulse: opacity 0.5->1, 周期 1.5s |

#### 结果展示阶段

| 元素 | 类型 | 位置 | 尺寸/样式 | 动画 |
|------|------|------|-----------|------|
| FamilyHighlightCard | Panel | Body Center | 圆角 20px, 阴影, padding 1.4rem | finalReveal: scale 0.8->1.05->1, rotateY -10deg->0, opacity 0->1, 0.6s |
| CardRibbon | Label | 右上角 | 背景 #FF8A80, 白色文字, 旋转 45deg | 无 |
| F-Icon | Text | Card Top | 3rem | 无 |
| F-Name | Text | Card Top | 1.2rem, Weight 800 | 无 |
| F-Desc | Text | Card Middle | 0.75rem, #8D8D8D | 无 |
| F-Money | Label | Card Middle | 背景 #FFF8E1, 圆角 20px, 文字 #E65100 | 金币图标 + 数值 |
| StatGrid | GridLayout | Card Bottom | 2列, 间距 0.5rem | 无 |
| StatItem | Panel | Grid Cell | 背景 #FAFAFA, 圆角 10px, padding 0.55rem 0.7rem | 进度条 width 0->value%, 0.5s ease |
| S-Bar | Image | Item Right | 宽 44, 高 6, 圆角 3px, 背景 #EEEEEE | 填充动画 |
| S-Bar-Fill | Image | Bar内部 | 高度 100%, 圆角 3px, 渐变色 | 宽度动画 |

### 交互
- **BtnReroll**：点击后保存旧家庭到 `oldFamily`，重新播放抽卡动画
- **免费重置次数**：初始 1 次，用完后显示"看广告重置"
- **HelpBtn (?)**：点击打开属性说明弹窗（见弹窗章节）

---

## 界面4：省市选择（Province Selection）

### 层级结构
```
Screen-Province (Flex Column)
├── ScreenHeader
│   ├── StepBadge ("Step 3/5")
│   ├── Title ("📍 选择你的省市")
│   └── Subtitle
├── ScreenBody (Scrollable)
│   ├── SectionTitle-Hot ("🔥 热门城市 [推荐]")
│   ├── ProvinceList-Hot (Grid 2列)
│   │   └── ProvinceCard x4
│   ├── SectionTitle-All ("📋 全部省市")
│   └── ProvinceList-All (Grid 2列)
│       └── ProvinceCard xN
└── ScreenFooter
    ├── ProvinceInfoPanel (选中后显示, 默认隐藏)
    └── BtnConfirm ("确认 →", 默认禁用)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 | 交互 |
|------|------|------|-----------|------|
| SectionTitle | Text | Body Top | 0.8rem, Weight 700, #8D8D8D | 无 |
| HotTag | Label | Title右侧 | 背景 #FF8A80, 白色, 0.6rem | 无 |
| ProvinceList | GridLayout | Body | 2列, 间距 0.6rem | 无 |
| ProvinceCard | Button | Grid Cell | 圆角 14px, 边框 2.5px #E8E8E8, padding 0.7rem 0.5rem | 点击选中 |
| P-Name | Text | Card Top | 0.85rem, Weight 700 | 无 |
| P-Mode | Label | Card Middle | 0.6rem, Weight 600, 圆角 4px | 3+3=紫色系, 3+1+2=蓝色系 |
| P-Diff | Text | Card Bottom | 0.6rem, #8D8D8D | 无 |
| ProvinceInfo | Panel | Footer Top | 背景 #E3F2FD, 圆角 14px, 边框 1.5px rgba(100,181,246,0.25) | 选中后显示，包含模式和难度说明 |
| BtnConfirm | Button | Footer Bottom | 全宽, 胶囊形 | 选中省市后启用 |

### 难度颜色映射

| 难度 | diffClass | 卡片背景 |
|------|-----------|----------|
| 轻松 | diff-easy | #E8F5E9 (薄荷绿) |
| 正常 | diff-normal | #FFFFFF (白色) |
| 困难 | diff-hard | #FFF0E6 (蜜桃色) |
| 地狱 | diff-hell | #F3E5F5 (薰衣草色) |

### 选中态
- 所有难度卡片选中后统一变为：背景 `#E3F2FD`, 边框 `#64B5F6`

---

## 界面5：选科方向（Subject Selection）

### 层级结构
```
Screen-Subject (Flex Column)
├── ScreenHeader
│   ├── StepBadge ("Step 4/5")
│   ├── Title ("📐 选择你的方向")
│   └── Subtitle ("🎯 基于真实「3+1+2」高考选科制度")
├── ScreenBody (Scrollable)
│   ├── SubjectCards (Vertical Stack)
│   │   └── SubjectCard x2
│   │       ├── S-Icon
│   │       ├── S-Name
│   │       ├── S-Desc
│   │       └── SubjectDetail (展开/收起)
│   │           ├── STags (解锁科目标签)
│   │           └── SubjectMajors (可选专业)
│   └── SubjectNote (提示信息)
└── ScreenFooter
    └── BtnConfirm ("确认 →", 默认禁用)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 | 交互 |
|------|------|------|-----------|------|
| SubjectCard | Button | Body | 圆角 20px, 边框 2.5px #E8E8E8, padding 1.2rem | 点击展开详情并选中 |
| S-Icon | Text | Card Top | 2.6rem | 无 |
| S-Name | Text | Card Top | 1.1rem, Weight 800 | 无 |
| S-Desc | Text | Card Middle | 0.75rem, #8D8D8D | 无 |
| SubjectDetail | Panel | Card Bottom | 默认隐藏 | 展开时显示 |
| STags | HorizontalLayout | Detail Top | 标签 0.65rem, 背景 rgba(102,187,106,0.12), 文字 #66BB6A | 无 |
| SubjectMajors | Panel | Detail Bottom | 背景 #F5F5F5, 圆角 10px | 无 |
| M-Title | Text | Majors Top | 0.7rem, Weight 700, #8D8D8D | 无 |
| M-Chip | Label | Majors Flow | 0.65rem, 背景 #FFFFFF, 圆角 4px, #6D6D6D | 无 |
| SubjectNote | Panel | Body Bottom | 背景 #FFF0E6, 圆角 14px, 文字 #FF8A80 | 无 |
| BtnConfirm | Button | Footer | 全宽, 胶囊形 | 选中科目后启用，点击弹出确认对话框 |

### 状态说明
- **Compact 态**：padding 0.8rem 1rem，icon 2rem，无详情显示
- **Expanded 态**：padding 1.2rem，显示完整详情，边框变 `#64B5F6`，背景变 `#E3F2FD`
- **选中逻辑**：点击卡片 -> 如果已展开则收起；如果未展开则展开并设为选中 -> 启用确认按钮

---

## 界面6：天赋树（Talent Tree）

### 层级结构
```
Screen-Talent (Flex Column)
├── ScreenHeader
│   ├── StepBadge ("Step 5/5")
│   ├── Title ("🌳 天赋觉醒")
│   ├── Subtitle
│   └── EnergyDisplay ("⚡ 剩余能量点：6", 左上角)
├── ScreenBody
│   └── TalentTree (Horizontal Flex, 5分支)
│       └── TalentBranch x5
│           ├── BranchHeader (分支名称)
│           └── TalentNode x5 (从下到上)
│               └── NodeTooltip (Hover提示)
└── ScreenFooter
    └── BtnConfirm ("确认天赋 →")
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 | 交互 |
|------|------|------|-----------|------|
| EnergyDisplay | Label | Header Left | 0.8rem, Weight 700, #64B5F6, 背景 #E3F2FD, 圆角 12px | 实时更新剩余点数 |
| TalentTree | HorizontalLayout | Body Center | 5分支, 间距 1.5rem, 底部对齐, 最小高度 400 | 无 |
| TalentBranch | VerticalLayout | Tree Cell | 垂直排列, 间距 0.8rem | 无 |
| BranchConnector | Image | Branch背景 | 宽 3px, 渐变 #E8E8E8->#DDD, 贯穿分支 | z-index 0 |
| BranchHeader | Text | Branch Top | 0.75rem, Weight 700, 背景 #FFF8F0, 圆角 8px | z-index 1 |
| TalentNode | Button | Branch垂直排列 | 36x36 圆形, 边框 3px | 点击点亮（条件满足时） |
| NodeTooltip | Panel | Node上方 | 背景 #FFFFFF, 圆角 10px, 阴影, 0.65rem | Hover 显示, opacity 0->1, 0.2s |

### 节点状态

| 状态 | 样式 | 交互 |
|------|------|------|
| Locked (未解锁分支) | 背景 #f5f5f5, 边框 #e0e0e0, 文字 #bbb | 不可点击, cursor = not-allowed |
| Active (可点亮) | 背景渐变 #64B5F6->#42A5F5, 边框 #1976D2, 白色文字, 发光阴影 | 可点击, nodePulse: scale 1->1.1, 周期 1.5s |
| Lit (已点亮) | 背景渐变 #FFD54F->#FF8F00, 边框 #E65100, 白色文字, 发光阴影 | 不可点击 |

### 解锁规则
- 物理方向解锁：数理(math)、匠心(craft)、韧骨(body)
- 历史方向解锁：辞海(word)、灵感(art)、韧骨(body)
- 点亮顺序：必须按顺序从下到上点亮（节点 0 -> 1 -> 2 -> 3 -> 4）
- 能量消耗：每点亮一个节点消耗 1 点能量，初始 6 点

---

## 弹窗1：确认对话框（Confirm Dialog）

### 层级结构
```
ConfirmDialog (Overlay, z=100)
├── DarkOverlay (背景遮罩, rgba(0,0,0,0.5))
└── DialogBox (Center)
    ├── Title
    ├── Message
    └── ButtonRow (Horizontal)
        ├── BtnCancel (Outline)
        └── BtnConfirm (Primary)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 |
|------|------|------|-----------|
| DarkOverlay | Panel | 全屏 | 背景 rgba(0,0,0,0.5) |
| DialogBox | Panel | 屏幕中心 | 背景 #FFFFFF, 圆角 20px, 宽 90% max 320px, padding 1.5rem, 阴影 lg |
| Title | Text | Box Top | 1.05rem, Weight 700, 居中 |
| Message | Text | Title Below | 0.8rem, #6D6D6D, 居中, 行高 1.6 |
| ButtonRow | HorizontalLayout | Box Bottom | 间距 0.6rem, 两按钮均分 |
| BtnCancel | Button | Row Left | Flex:1, Outline 样式 |
| BtnConfirm | Button | Row Right | Flex:1, Primary 样式 |

### 使用场景
- 选科确认："确定选择 [物理方向] 吗？选科后将锁定你的专业方向..."
- 通用确认：任何需要二次确认的操作

---

## 弹窗2：家庭对比界面（Compare Overlay）

### 层级结构
```
CompareOverlay (Overlay, z=100)
├── DarkOverlay
└── CompareBox (Center)
    ├── Title ("🔄 家庭对比")
    ├── CompareCols (Horizontal)
    │   ├── CompareCol-Old (背景 #FFF5F5)
    │   │   ├── C-Label ("旧家庭")
    │   │   ├── C-Icon
    │   │   ├── C-Name
    │   │   └── CompareRows
    │   └── CompareCol-New (背景 #F1F8E9)
    │       ├── C-Label ("新家庭")
    │       ├── C-Icon
    │       ├── C-Name
    │       └── CompareRows
    └── ButtonRow
        ├── BtnKeepOld ("保留旧的", Outline)
        └── BtnKeepNew ("选择新的", Primary)
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 |
|------|------|------|-----------|
| CompareBox | Panel | 屏幕中心 | 背景 #FFFFFF, 圆角 20px, 宽 90% max 380px, padding 1.2rem |
| Title | Text | Box Top | 1.05rem, Weight 700, 居中 |
| CompareCols | HorizontalLayout | Title Below | 间距 0.6rem |
| CompareCol | Panel | Col Cell | Flex:1, 圆角 14px, padding 0.8rem, 居中 |
| Old Col | - | - | 背景 #FFF5F5 |
| New Col | - | - | 背景 #F1F8E9 |
| C-Label | Text | Col Top | 0.65rem, Weight 700, #8D8D8D, 大写 |
| C-Icon | Text | Col Middle | 2rem |
| C-Name | Text | Col Middle | 0.85rem, Weight 800 |
| CompareRow | HorizontalLayout | Col Bottom | 两端对齐, padding 0.25rem 0, 0.78rem |
| CR-Label | Text | Row Left | #8D8D8D |
| CR-Value | Text | Row Center | Weight 700 |
| CR-Diff | Text | Row Right | 0.7rem, Weight 700, 最小宽 30px, 右对齐 |

### Diff 颜色
- 上升：#66BB6A (cr-up)，显示 `+N`
- 下降：#EF5350 (cr-down)，显示 `-N`
- 持平：#9E9E9E (cr-same)，显示 `=`

### 使用场景
- 免费重置次数用完后再次点击重置，展示新旧家庭对比供玩家选择

---

## 弹窗3：属性说明（Stat Info Popup）

### 层级结构
```
InfoPopup (Overlay, z=200)
├── DarkOverlay
└── InfoBox (Center)
    ├── Title ("💡 属性说明")
    ├── InfoList (Vertical)
    │   └── InfoItem x4
    │       ├── I-Label (属性名)
    │       └── I-Desc (属性说明)
    └── CloseBtn
```

### 元素详情

| 元素 | 类型 | 位置 | 尺寸/样式 |
|------|------|------|-----------|
| InfoBox | Panel | 屏幕中心 | 背景 #FFFFFF, 圆角 20px, 宽 90% max 340px, padding 1.2rem 1.5rem |
| Title | Text | Box Top | 0.95rem, Weight 700, #3D3D3D |
| InfoList | VerticalLayout | Title Below | 间距 0.4rem, margin-bottom 0.8rem |
| InfoItem | HorizontalLayout | List Cell | 0.8rem, 行高 1.5 |
| I-Label | Text | Item Left | Weight 700, #3D3D3D |
| I-Desc | Text | Item Right | #6D6D6D, margin-left 0.2rem |
| CloseBtn | Button | Box Bottom | 居中, btn-sm 样式 (padding 0.6rem 1.2rem, 0.85rem) |

### 属性列表
1. 📐 智力 - 影响考试得分、学习效率、天赋解锁
2. 🧠 心理 - 影响抗压能力、考场发挥、逆境翻盘
3. 💬 社交 - 影响人脉建立、大学活动、就业机会
4. 💪 健康 - 影响体力恢复、减少生病、体育课加分

### 使用场景
- 家庭背景界面的 HelpBtn (?) 点击后弹出
- 任何需要解释四大属性含义的地方

---

## 附录：界面切换关系图

```
[Title] --newGame--> [Gender] --confirmGender--> [Family] --confirmFamily--> [Province]
   ^                                                                    |
   |                                                                    v
[Continue]                                                          [Subject]
   |                                                                    |
   |                                                                    v
   └────────────────────────────────────────────────────────────── [Talent]
```

- 每个界面确认后自动存档
- Continue 根据存档的 step 字段恢复到对应界面
