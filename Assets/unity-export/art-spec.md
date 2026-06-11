# 美术资源规格文档

> 本文档包含 `我的高考志愿模拟器` 的所有视觉规格，供 Unity 美术和程序参考。
> 包括配色方案、字体要求、圆角/阴影参数、动画时长与缓动函数、以及完整的图标映射表。

---

## 1. 配色方案

### 1.1 主色调（CSS 变量对应表）

| 变量名 | 色值 | Unity Color | 用途 |
|--------|------|-------------|------|
| `--bg` | `#FFF8F0` | (1.0, 0.973, 0.941, 1.0) | 主背景色 |
| `--bg-card` | `#FFFFFF` | (1.0, 1.0, 1.0, 1.0) | 卡片背景 |
| `--text` | `#3D3D3D` | (0.239, 0.239, 0.239, 1.0) | 主文字 |
| `--text-light` | `#8D8D8D` | (0.553, 0.553, 0.553, 1.0) | 次要文字 |
| `--text-soft` | `#6D6D6D` | (0.427, 0.427, 0.427, 1.0) | 辅助文字 |
| `--border` | `#E8E8E8` | (0.91, 0.91, 0.91, 1.0) | 边框/分割线 |

### 1.2 强调色

| 变量名 | 色值 | Unity Color | 用途 |
|--------|------|-------------|------|
| `--accent` | `#FF8A80` | (1.0, 0.541, 0.502, 1.0) | 主强调色、按钮、角标 |
| `--accent-hover` | `#FF5252` | (1.0, 0.322, 0.322, 1.0) | 强调色悬停态 |
| `--confirm` | `#64B5F6` | (0.392, 0.71, 0.965, 1.0) | 确认按钮、选中态 |
| `--confirm-hover` | `#42A5F5` | (0.259, 0.647, 0.961, 1.0) | 确认色悬停态 |
| `--gold` | `#FFB300` | (1.0, 0.702, 0.0, 1.0) | 金币、奖励 |
| `--gold-light` | `#FFF8E1` | (1.0, 0.973, 0.882, 1.0) | 金币背景 |

### 1.3 状态色

| 变量名 | 色值 | Unity Color | 用途 |
|--------|------|-------------|------|
| `--up` | `#66BB6A` | (0.4, 0.733, 0.416, 1.0) | 上升/增益/正确 |
| `--down` | `#EF5350` | (0.937, 0.325, 0.314, 1.0) | 下降/减益/错误 |
| `--same` | `#9E9E9E` | (0.62, 0.62, 0.62, 1.0) | 持平/中性 |

### 1.4 卡片主题色

| 变量名 | 色值 | Unity Color | 用途 |
|--------|------|-------------|------|
| `--card-mint` | `#E8F5E9` | (0.91, 0.961, 0.914, 1.0) | 轻松难度卡片背景 |
| `--card-lavender` | `#F3E5F5` | (0.953, 0.898, 0.961, 1.0) | 地狱难度卡片背景 |
| `--card-peach` | `#FFF0E6` | (1.0, 0.941, 0.902, 1.0) | 困难难度卡片背景、提示框 |
| `--card-butter` | `#FFFDE7` | (1.0, 0.992, 0.906, 1.0) | 备用背景 |
| `--card-sky` | `#E3F2FD` | (0.89, 0.949, 0.992, 1.0) | 选中态背景、信息面板 |

### 1.5 渐变定义

| 渐变名称 | 色值 | 用途 |
|----------|------|------|
| `btn-primary` | `linear-gradient(135deg, #FF8A80, #FF6B6B)` | 主按钮背景 |
| `btn-confirm` | `linear-gradient(135deg, #64B5F6, #5C9CE6)` | 确认按钮背景 |
| `title-gradient` | `linear-gradient(135deg, #FF8A80, #FF9800, #64B5F6)` | 标题文字渐变色 |
| `title-bg` | `linear-gradient(180deg, #FFF0E6 0%, #FFF8F0 40%, #F3E5F5 100%)` | 启动画面背景 |
| `draw-progress` | `linear-gradient(90deg, #64B5F6, #FF8A80)` | 抽卡进度条 |
| `bar-int` | `linear-gradient(90deg, #64B5F6, #42A5F5)` | 智力属性条 |
| `bar-psy` | `linear-gradient(90deg, #CE93D8, #AB47BC)` | 心理属性条 |
| `bar-soc` | `linear-gradient(90deg, #FFCC80, #FF9800)` | 社交属性条 |
| `bar-health` | `linear-gradient(90deg, #A5D6A7, #66BB6A)` | 健康属性条 |
| `node-lit` | `linear-gradient(135deg, #FFD54F, #FF8F00)` | 天赋已点亮节点 |
| `node-active` | `linear-gradient(135deg, #64B5F6, #42A5F5)` | 天赋可点亮节点 |
| `branch-connector` | `linear-gradient(to bottom, #E8E8E8, #DDD)` | 天赋分支连接线 |

### 1.6 难度颜色映射

| 难度 | diffClass | 卡片背景 | 模式标签背景 |
|------|-----------|----------|-------------|
| 轻松 | diff-easy | `#E8F5E9` | - |
| 正常 | diff-normal | `#FFFFFF` | - |
| 困难 | diff-hard | `#FFF0E6` | - |
| 地狱 | diff-hell | `#F3E5F5` | - |

**选中态统一覆盖**：所有难度卡片选中后变为背景 `#E3F2FD`，边框 `#64B5F6`。

### 1.7 模式标签颜色

| 模式 | modeClass | 背景 | 文字 |
|------|-----------|------|------|
| 3+3 | mode-33 | `rgba(156,39,176,0.15)` | `#9C27B0` |
| 3+1+2 | mode-312 | `rgba(100,181,246,0.15)` | `#64B5F6` |
| 旧高考 | mode-old | `rgba(158,158,158,0.15)` | `#9E9E9E` |

---

## 2. 字体要求

### 2.1 字体栈

```
"PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", system-ui, -apple-system, sans-serif
```

### 2.2 Unity 字体建议

| 平台 | 推荐字体 | 备选 |
|------|----------|------|
| iOS | PingFang SC (系统自带) | Noto Sans CJK SC |
| Android | Noto Sans CJK SC | Source Han Sans |
| 通用 | 思源黑体 (Source Han Sans) | 微软雅黑 (Windows) |

### 2.3 字号层级

| 层级 | 字号 (px) | 字重 | 行高 | 用途 |
|------|-----------|------|------|------|
| 超大标题 | 56px | 900 | 1.4 | 启动画面主标题 |
| 大标题 | 28-32px | 900 | 1.4 | 界面标题 |
| 中标题 | 20-24px | 800 | 1.3 | 卡片名称、步骤标题 |
| 正文 | 14-16px | 400/600 | 1.5 | 描述文字 |
| 辅助文字 | 12-13px | 400 | 1.5 | 提示、说明 |
| 标签/徽章 | 11-12px | 700 | 1.2 | 标签、徽章 |
| 微文字 | 10-11px | 700 | 1.2 | 角标、版本号 |

### 2.4 字重映射

| CSS font-weight | Unity FontStyle / 字重 |
|-----------------|------------------------|
| 400 (Normal) | Regular |
| 600 (SemiBold) | Medium |
| 700 (Bold) | Bold |
| 800 (ExtraBold) | Bold + 适当放大 |
| 900 (Black) | Bold + 放大 1.05x |

---

## 3. 圆角参数

| 元素类型 | 圆角值 | 说明 |
|----------|--------|------|
| 大卡片 | 20px | 家庭结果卡、选科卡片、弹窗 |
| 中卡片 | 14px | 省市卡片、对比列、提示面板 |
| 小标签 | 10px | 属性条、专业标签、统计项 |
| 按钮（胶囊） | 50px | 所有主按钮、确认按钮 |
| 按钮（小） | 12px | 小尺寸按钮 |
| 输入框 | 20px | 名字输入框 |
| 徽章/标签 | 20px | Step 徽章、金币显示 |
| 圆形节点 | 50% | 天赋节点（36x36 圆形） |
| 帮助按钮 | 50% | 右上角 "?" 按钮（22x22） |
| 进度条 | 3px | 属性条、抽卡进度条 |
| 小标签 | 4-6px | 模式标签、专业芯片 |

---

## 4. 阴影参数

### 4.1 阴影层级

| 层级 | 阴影值 | 用途 |
|------|--------|------|
| 卡片阴影 | `0 4px 24px rgba(0,0,0,0.06)` | 普通卡片 |
| 大阴影 | `0 8px 40px rgba(0,0,0,0.10)` | 弹窗、抽卡卡片、选中态 |
| 按钮阴影（主） | `0 4px 20px rgba(255,138,128,0.35)` | 主按钮 |
| 按钮阴影（确认） | `0 4px 20px rgba(100,181,246,0.35)` | 确认按钮 |
| 选中 Glow | `0 0 0 3px rgba(100,181,246,0.15)` | 选中卡片外发光 |
| 节点发光（点亮） | `0 0 12px rgba(255,152,0,0.4)` | 天赋已点亮节点 |
| 节点发光（可点） | `0 0 12px rgba(66,165,246,0.4)` | 天赋可点亮节点 |
| 抽卡发光 | `0 0 30px rgba(100,181,246,0.3)` | 抽卡 Pop 效果 |
| App 外阴影 | `0 0 80px rgba(0,0,0,0.08)` | 整个应用容器 |

### 4.2 Unity 阴影实现建议

```csharp
// 卡片阴影
shadowEffect.color = new Color(0, 0, 0, 0.06f);
shadowEffect.distance = new Vector2(0, 4);
shadowEffect.blurRadius = 24;

// 弹窗阴影
shadowEffect.color = new Color(0, 0, 0, 0.10f);
shadowEffect.distance = new Vector2(0, 8);
shadowEffect.blurRadius = 40;
```

---

## 5. 动画规格

### 5.1 动画时长表

| 动画名称 | 时长 | 缓动函数 | 用途 |
|----------|------|----------|------|
| `fadeIn` | 0.2s | ease | 弹窗渐显 |
| `float` | 3s | ease-in-out | 标题图标浮动 |
| `dotFloat` | 3-6s | ease-in-out | 背景圆点浮动 |
| `pulse` | 1.5s | ease-in-out | 提示文字呼吸 |
| `nodePulse` | 1.5s | ease-in-out | 天赋节点呼吸 |
| `pop` | 0.08s | ease | 抽卡卡片弹跳 |
| `finalReveal` | 0.6s | ease | 家庭结果揭示 |
| `shake` | 0.4s | ease | 输入错误抖动 |
| `statBarFill` | 0.5s | ease | 属性条填充 |
| `btnPress` | 0.2s | ease | 按钮按下 |
| `tooltipFade` | 0.2s | ease | 提示框显隐 |

### 5.2 缓动函数映射

| CSS 缓动 | Unity Easing | 说明 |
|----------|-------------|------|
| `ease` | `Ease.OutQuad` | 通用缓动 |
| `ease-in-out` | `Ease.InOutSine` | 对称缓动（浮动、呼吸） |
| `linear` | `Ease.Linear` | 线性（进度条） |
| `ease-out` | `Ease.OutCubic` | 快速开始缓慢结束 |

### 5.3 关键动画详解

#### 5.3.1 浮动动画（float）
```
周期: 3s
缓动: ease-in-out
变换: translateY(0) -> translateY(-10px) -> translateY(0)
循环: infinite
```

#### 5.3.2 背景圆点浮动（dotFloat）
```
周期: 3-6s（每个圆点随机）
缓动: ease-in-out
变换: translateY(0) scale(1) -> translateY(-6px) scale(1.1) -> translateY(0) scale(1)
循环: infinite
延迟: 0-2s 随机
```

#### 5.3.3 抽卡 Pop 动画
```
时长: 80ms
缓动: ease
变换: scale(1) -> scale(1.08)
附加: 边框颜色变为 #64B5F6，添加发光阴影
恢复: 80ms 后恢复 scale(1)
```

#### 5.3.4 家庭结果揭示（finalReveal）
```
时长: 600ms
缓动: ease
关键帧:
  0%:   scale(0.8) rotateY(-10deg) opacity(0)
  50%:  scale(1.05) rotateY(5deg)
  100%: scale(1) rotateY(0) opacity(1)
```

#### 5.3.5 抖动动画（shake）
```
时长: 400ms
缓动: ease
关键帧:
  0%, 100%: translateX(0)
  25%:      translateX(-4px)
  75%:      translateX(4px)
```

#### 5.3.6 呼吸动画（pulse / nodePulse）
```
周期: 1.5s
缓动: ease-in-out
变换: opacity(0.5) -> opacity(1) -> opacity(0.5)  // pulse
      scale(1) -> scale(1.1) -> scale(1)            // nodePulse
```

#### 5.3.7 属性条填充
```
时长: 500ms
缓动: ease
变换: width(0%) -> width(value%)
```

#### 5.3.8 按钮交互
```
悬停: scale(1.02), 200ms
按下: scale(0.96), 200ms
禁用: opacity(0.4), 无变换
```

---

## 6. 图标列表（Emoji 对应关系）

### 6.1 界面图标

| 用途 | Emoji | Unity 建议 |
|------|-------|-----------|
| 应用图标/标题 | 🎓 | 毕业帽图标 |
| 装饰-书 | 📖 | 书本图标 |
| 装饰-笔 | ✏️ | 铅笔图标 |
| 装饰-书包 | 🎒 | 书包图标 |
| 装饰-学校 | 🏫 | 学校建筑图标 |
| 性别-男 | 🙋‍♂️ | 男生头像 |
| 性别-女 | 🙋‍♀️ | 女生头像 |
| 输入图标 | ✍️ | 手写笔图标 |
| 金币 | 💰 / G | 金币图标 + "G" 文字 |
| 能量 | ⚡ | 闪电图标 |
| 步骤徽章 | - | 带数字的胶囊标签 |
| 帮助 | ? | 圆形问号按钮 |
| 热门标签 | 🔥 | 火焰图标 |
| 列表标签 | 📋 | 列表图标 |
| 提示 | 💡 | 灯泡图标 |
| 警告 | ⚠️ | 警告三角图标 |
| 刷新 | 🔄 | 刷新/循环箭头图标 |
| 确认 | ✅ | 对勾图标 |
| 文件夹 | 📂 | 文件夹图标 |

### 6.2 家庭图标

| 家庭类型 | Emoji | 名称 | Unity 建议 |
|----------|-------|------|-----------|
| RURAL | 🌾 | 村民之子 | 麦穗/农田图标 |
| WORKING | 🔧 | 工薪之家 | 扳手/工具图标 |
| MIDDLE | 🏡 | 中产家庭 | 房子图标 |
| WEALTHY | 💎 | 富裕家庭 | 钻石图标 |
| SCHOLAR | 📚 | 书香门第 | 书本堆叠图标 |

### 6.3 属性图标

| 属性 | Emoji | 名称 | 颜色 |
|------|-------|------|------|
| int | 📐 | 智力 | #64B5F6 (蓝) |
| psy | 🧠 | 心理 | #AB47BC (紫) |
| soc | 💬 | 社交 | #FF9800 (橙) |
| health | 💪 | 健康 | #66BB6A (绿) |

### 6.4 科目图标

| 科目 | Emoji | 名称 | Unity 建议 |
|------|-------|------|-----------|
| physics | ⚛️ | 物理方向 | 原子/物理符号图标 |
| history | 📜 | 历史方向 | 卷轴/历史书图标 |

### 6.5 天赋分支图标

| 分支ID | Emoji | 名称 | Unity 建议 |
|--------|-------|------|-----------|
| math | 📐 | 数理 | 三角尺/数学符号 |
| craft | 🔧 | 匠心 | 扳手/工匠工具 |
| word | 📜 | 辞海 | 卷轴/文字符号 |
| art | 🎨 | 灵感 | 调色板/画笔 |
| body | 💪 | 韧骨 | 肌肉/力量符号 |

### 6.6 天赋节点编号

天赋节点使用数字 1-5 表示，无独立图标，通过节点状态（颜色）区分。

### 6.7 图标尺寸规范

| 图标位置 | 尺寸 | 说明 |
|----------|------|------|
| 标题图标 | 56px (3.5rem) | 启动画面主图标 |
| 装饰图标 | 19px (1.2rem) | 启动画面浮动装饰 |
| 性别图标 | 45px (2.8rem) | 性别选择卡片 |
| 输入图标 | 24px (1.5rem) | 名字输入框前缀 |
| 家庭图标（抽卡） | 56px (3.5rem) | 抽卡动画中 |
| 家庭图标（结果） | 48px (3rem) | 结果展示卡片 |
| 科目图标 | 42px (2.6rem) | 选科卡片 |
| 科目图标（紧凑） | 32px (2rem) | 收起状态 |
| 对比图标 | 32px (2rem) | 对比弹窗 |
| 金币图标 | 18x18px | SVG 圆形带 G 字 |
| 帮助按钮 | 22x22px | 圆形问号 |
| 天赋节点 | 36x36px | 圆形数字 |

---

## 7. 布局间距规范

### 7.1 间距层级

| 名称 | 值 | 用途 |
|------|-----|------|
| xs | 4px (0.25rem) | 最小间距、行内元素 |
| sm | 8px (0.5rem) | 小间距、卡片内边距 |
| md | 12-16px (0.75-1rem) | 标准间距、按钮内边距 |
| lg | 20-24px (1.25-1.5rem) | 大间距、屏幕边距 |
| xl | 32-40px (2-2.5rem) | 标题间距、区块分隔 |

### 7.2 屏幕边距

| 位置 | 值 | 说明 |
|------|-----|------|
| 左右边距 | 24px (1.5rem) | 内容区安全边距 |
| 顶部边距 | 12-20px | Header 区域 |
| 底部边距 | 12-24px | Footer 区域 |
| 卡片间距 | 8-12px | 卡片/元素之间 |
| 按钮间距 | 8-10px | 按钮之间 |

---

## 8. 材质与特效建议

### 8.1 UI 材质

| 元素 | 材质类型 | 说明 |
|------|----------|------|
| 普通卡片 | UI/Default + 阴影 | 标准 UI 材质 |
| 渐变按钮 | UI/Default + 渐变纹理 | 或使用 Shader 实现渐变 |
| 渐变文字 | 自定义 Shader | 实现文字渐变效果（如标题） |
| 发光效果 | UI/Default + Bloom | 后处理 Bloom 效果 |
| 粒子背景 | Particle System | 启动画面浮动圆点 |

### 8.2 后处理建议

| 效果 | 强度 | 用途 |
|------|------|------|
| Bloom | 低 | 选中态发光、节点发光 |
| Color Grading | 轻微暖色 | 整体画面温馨感 |
| Vignette | 轻微 | 聚焦中心内容 |

---

## 9. 资源清单

### 9.1 需要制作的 Sprite/Texture

| 资源名称 | 类型 | 尺寸建议 | 说明 |
|----------|------|----------|------|
| bg_gradient_title | Texture | 512x512 | 启动画面渐变背景 |
| btn_primary_bg | Sprite | 256x64 | 主按钮渐变背景（可9切片） |
| btn_confirm_bg | Sprite | 256x64 | 确认按钮渐变背景 |
| card_bg | Sprite | 256x256 | 卡片背景（圆角20，可9切片） |
| card_bg_selected | Sprite | 256x256 | 选中卡片背景 |
| progress_bar_bg | Sprite | 128x8 | 进度条背景 |
| progress_bar_fill | Sprite | 128x8 | 进度条填充 |
| stat_bar_int | Sprite | 64x6 | 智力属性条 |
| stat_bar_psy | Sprite | 64x6 | 心理属性条 |
| stat_bar_soc | Sprite | 64x6 | 社交属性条 |
| stat_bar_health | Sprite | 64x6 | 健康属性条 |
| node_locked | Sprite | 64x64 | 锁定节点 |
| node_active | Sprite | 64x64 | 可点亮节点 |
| node_lit | Sprite | 64x64 | 已点亮节点 |
| coin_icon | Sprite | 32x32 | 金币图标 |
| ribbon_new | Sprite | 64x64 | "NEW" 角标 |

### 9.2 粒子特效

| 特效名称 | 用途 | 参数 |
|----------|------|------|
| bg_dots | 启动画面背景 | 12个圆点，随机大小 6-20px，随机颜色，缓慢浮动 |
| draw_sparkle | 抽卡完成 | 短暂粒子爆发，金色/蓝色 |
| node_lightup | 天赋点亮 | 节点位置粒子爆发，对应分支颜色 |

---

## 10. 响应式适配建议

### 10.1 参考分辨率

- **设计基准**：430px x 764px（9:16 竖屏）
- **最小宽度**：320px
- **最大宽度**：480px（平板竖屏）

### 10.2 Unity Canvas Scaler 设置

```
Canvas Scaler:
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 430 x 764
  Screen Match Mode: Match Width Or Height
  Match: 0.5 (Width 和 Height 之间平衡)
  Reference Pixels Per Unit: 100
```

### 10.3 安全区域

```csharp
// 适配刘海屏/挖孔屏
Rect safeArea = Screen.safeArea;
// 根据 safeArea 调整顶部和底部边距
```
