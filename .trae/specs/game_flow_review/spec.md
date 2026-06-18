# 重启我的高中人生 - 游戏流程全面审查

## Overview
- **Summary**: 全面审查游戏主流程，从启动到结局的完整链路，识别代码问题和逻辑漏洞
- **Purpose**: 确保游戏流程闭环、数据正确传递、界面正常显示
- **Target Users**: 开发团队

## Goals
- 验证完整游戏流程的代码正确性
- 识别空引用、路径错误、逻辑漏洞等问题
- 确保界面跳转和数据传递正常

## Non-Goals (Out of Scope)
- 美术资源优化
- 性能优化
- 音效系统完善

## Background & Context
游戏流程：启动 → 创建角色 → 性格 → 家庭 → 省市 → 选科 → 主界面 → 校园日常 → 结算 → 循环 → 高考 → 志愿 → 大学 → 人生启程 → 总结

## Functional Requirements
- **FR-1**: 启动界面能正确进入创建角色流程
- **FR-2**: 创建角色界面能显示男女形象、选择性别、输入名字
- **FR-3**: 性格选择界面能正常工作
- **FR-4**: 家庭/省市/选科流程能正常推进
- **FR-5**: 主界面能显示学期卡片、按钮、当前进度
- **FR-6**: 校园日常玩法能正常进入，显示流水账和事件
- **FR-7**: 结算页面能显示属性变化和评价
- **FR-8**: 高考/志愿/大学/人生启程流程能正常工作

## Constraints
- **Technical**: Unity C# 项目，Runtime 动态创建 UI
- **Dependencies**: ScreenRouter 负责界面跳转，GameState 负责状态管理

## Acceptance Criteria

### AC-1: 启动界面
- **Given**: 游戏刚启动，无存档
- **When**: 点击"重启我的高中人生"
- **Then**: 进入创建角色界面
- **Verification**: `programmatic`

### AC-2: 创建角色界面
- **Given**: 在创建角色界面
- **When**: 选择男生/女生
- **Then**: 显示对应性别立绘，名字池切换
- **Verification**: `human-judgment`

### AC-3: 主界面学期卡片
- **Given**: 进入主界面，有学期进度
- **When**: 查看中间区域
- **Then**: 显示学期横向滑屏卡片
- **Verification**: `human-judgment`

### AC-4: 校园日常玩法
- **Given**: 在主界面，点击"校园日常"
- **When**: 进入玩法界面
- **Then**: 显示开场文字、背景、属性、流水账、事件弹窗
- **Verification**: `human-judgment`

### AC-5: 结算页面
- **Given**: 完成一天玩法
- **When**: 进入结算页
- **Then**: 显示属性变化、精力状态、评价
- **Verification**: `human-judgment`

### AC-6: 流程闭环
- **Given**: 在结算页
- **When**: 点击"回家"
- **Then**: 返回主界面
- **Verification**: `programmatic`

## Open Questions
- [ ] 各界面的路由跳转是否完整
- [ ] GameState 的进度状态是否正确更新
- [ ] 资源路径是否正确