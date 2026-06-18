# 游戏流程全面审查 - 任务分解

## [ ] Task 1: 审查启动界面流程
- **Priority**: P0
- **Depends On**: None
- **Description**: 检查启动界面(LaunchScreen)的按钮逻辑、存档检测、路由跳转
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic`: 编译无错误
  - `human-judgment`: 按钮点击后正确跳转

## [ ] Task 2: 审查创建角色界面
- **Priority**: P0
- **Depends On**: Task 1
- **Description**: 检查ProfileScreen的男女立绘加载路径、性别选择、名字输入
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `programmatic`: Resources路径正确，无NullReference
  - `human-judgment`: 立绘正常显示，名字切换正常

## [ ] Task 3: 审查主界面学期卡片
- **Priority**: P0
- **Depends On**: Task 2
- **Description**: 检查HomeScreen的学期横向滑屏区域、卡片渲染逻辑
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic`: semesterScrollContent字段正确赋值
  - `human-judgment`: 学期卡片正常显示和滑动

## [ ] Task 4: 审查校园日常玩法
- **Priority**: P0
- **Depends On**: Task 3
- **Description**: 检查DailyGameScreen的开场、背景、属性、流水账、事件弹窗逻辑
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic`: 所有UI字段正确初始化，无NullReference
  - `human-judgment`: 界面显示完整，交互正常

## [ ] Task 5: 审查结算页面
- **Priority**: P0
- **Depends On**: Task 4
- **Description**: 检查DailySettlementScreen的属性变化显示、精力状态、评价展示
- **Acceptance Criteria Addressed**: AC-5
- **Test Requirements**:
  - `programmatic`: 编译无错误
  - `human-judgment`: 结算信息完整显示

## [ ] Task 6: 审查流程闭环
- **Priority**: P0
- **Depends On**: Task 5
- **Description**: 检查从结算页返回主界面的路由跳转、数据保存
- **Acceptance Criteria Addressed**: AC-6
- **Test Requirements**:
  - `programmatic`: 路由跳转正确，状态保存完整
  - `human-judgment`: 返回主界面后数据正确

## [ ] Task 7: 审查其他界面路由
- **Priority**: P1
- **Depends On**: Task 6
- **Description**: 检查性格、家庭、省市、选科、高考等界面的路由和逻辑
- **Acceptance Criteria Addressed**: FR-3, FR-4, FR-8
- **Test Requirements**:
  - `programmatic`: 编译无错误
  - `human-judgment`: 界面能正常显示和跳转