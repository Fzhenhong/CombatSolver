# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)，0.48.1 批次见 [历史卷 10](archive/development/volume-10.md)。

## 下一版本（开发中）

2026-10-03 社区批次 B013（Refs #172，PR #204）五个主题：

- T010 格挡 Decimal 边界：升级后 0 费 SHADOWMELD+ 配 UNCEASING_TOP 与空抽牌堆形成合法无限循环，`ShadowmeldPower.Amount` 无界增长，回合末 Orichalcum 6 格挡 × 2^Amount 在 `HookMirrors.ModifyBlock` 溢出。乘法修正链（含附魔乘算）改为在 `SimulateCreatureState.BlockSettlementCeiling`（999_999_999）饱和，`ShadowmeldPower` 因子 ≥97 层饱和以避免原生 pow 转换先抛；普通区间算术不变。夹具 `B013-BLOCK-DECIMAL-BOUNDARY`（修改前 `dec22b8f` Failed，栈同报告；修改后 `e2757311`/`a23dcf21` Passed），代表包 DOP1 哨兵路线与展开/转移逐字段一致、耗时无稳定增加。
- T008 搜索节点生命周期：case A（能力牌探针从边界快照分叉）由上游 `ef17f75b` 修复，既有 `OPENING-POWER-BOUNDARY` 复验；case B（回合边界救援的固定前缀越过锁定终局）改为在玩家或全部敌人死亡后停止应用剩余前缀。夹具 `B013-FIXED-PREFIX-TERMINAL-BOUNDARY`（`971ccdd1` Failed → `0b3f96a6` Passed），代表包哨兵 22809/129762、终局一致。
- T007 固定前缀与结束回合动作：路线冻结的 `EndsPlayerTurn=true` 卡动作（VOID_FORM 打出即结束回合）被 `ApplyFixedPrefix` 拒绝，现允许并结算回合结束，仅当标记与实际结算不符时判无效；`FIXED-PREFIX-TURN-OUTCOMES` 合同同步（终局后动作改为截断）。夹具 `B013-FIXED-PREFIX-TURN-END-CARD`（`eb64242d` Failed → `899ef881` Passed）。第二样本（0.47.0，前缀与全部回合准备分支不相容）在当前版本从记录根不复现，保留诊断待更近复现。
- T006 跨回合续用对账：RadiantPearl 首回合抽牌前生成 Luminesce 未建模，计划续用戳少一张手牌，被赌博筹码选牌放大为 H[0] 失配；补齐 `BeforeHandDraw` 生成后代表包 `TURN_SETUP_STATE_MISMATCH` → `TURN_SETUP_STATE_MATCH`。夹具 `B013-RADIANT-PEARL-HAND-DRAW`（`cd227ff6` Failed → `15c80923` Passed）。第二样本（DEFECT 首回合 3 闪电球）由上游 `72e0f799` 修复并复验。
- T009 默认 GC 请求分配边界：No-GC 关闭路径原先整体 `Disable()`，`search_limit=long.MaxValue`，低内存宿主在部署漂移重算的请求建立期才 OOM（与 #145 同源）。按 #145 选项 3，默认 GC 路径建立系统余量派生的分配上限、系统内存上限与普通 Gen2 回收续搜检查点（不启动区域），作用域退出清理信号；受守护的 disabled-scope 契约断言同步更新。夹具 `B013-DEFAULT-GC-LIMIT`（`6ac0f466` Failed → `7c6f3b83` Passed）。哨兵在余量足够时结局不变并实际触发回收续搜；外部负载接近系统上限时按既有 `MemoryHeadroomInsufficient` 跳过能力成员，属低余量保护。

本批未提升版本、未发包、未启动可见 Steam；可见游戏内表现、原生整场部署、T007 第二样本与 T009 报告宿主复现列为未验证。

变基到 0.48.1（`2ead87d9`）后五夹具复跑 Passed，T010 代表包 DOP1 哨兵与旧基线逐字段一致（7426/77129、4 回合胜、finalHp 71），T009/T006 代表包 7 回合胜、finalHp 88；路线随上游搜索改动微调，质量保持。

巨斧机器人报告中未解决的内存耗尽、重生目标丢失及完整原包部署边界见 [排查记录](issues/axebot-reports-20261003.md)；旧更优世界线报告中无新增用药的搜索缺口仍待独立定位，证据保留在历史卷。
