# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

## 下一版本（开发中）

### Smart 药水层的准入不再被 strategic 净差否证（Q010 / W2，Refs #210）

`CombatSearchCoordinator.Audits.cs` 的梯度入口原先只用 `StrategicHpDeficit` 推 `MaximumSmartPotionUses`；净差为 0 时整层药水搜索直接跳过。净差在同一路线上还会扣掉与药水无关的既有治疗，因此恒 ≤ 必然受击（`SolverResult.UnavoidableHpLost`，口径 `src/Search/CombatBeamSolver.Phases.cs:541-545,833`），零药路线越好越容易把药水层否证。Smart 政策下改为同时按必然受击求一份配额并取两者最大值，并输出 `SMART_POTION_GRADIENT axis_widened` 诊断。

- 只放宽「跑不跑药水层」的上界：beam / 节点 / 时间 / No-GC 预算一律不变，结果之间的比较规则（`RouteQualityPolicy`、`IsBetterPotionPolicyResult`）未改。净差 ≤ 必然受击 ⇒ 新上界 ≥ 旧上界 ⇒ 只会多搜，旧行为是被包含的子集；层内是否被采纳仍由原比较器裁决，因此最终结果按生产比较器不劣于修复前。非 Smart 政策与其余 `MaximumSmartPotionUses` 调用点语义不变。
- 日志算例（同根、同政策、同预算；约定：`projected_battle_hp_lost` 为整场累计、`unavoidable_hp_lost` 为扣除卖血后的必然受击）：修复前宽档（`GC_SEARCH_ALLOCATION_LIMIT=2319212440`、No-GC 弃区 44%）零药最优由 20 改善到 14、strategic 净差 8 < 门槛 9（`src/Runtime/SolverWeights.cs:99` 默认值，`BLOOD_POTION` 不在 `src/Search/PotionValuationRegistry.cs:19-34`）→ `stop=no_potion_acceptable maximum=0` → 终值 14；同一路线那条 `RESULT` 行给 `unavoidable_hp_lost=13`，按必然受击轴第一瓶本可接受（13 ≥ 9）。窄档（1296864492、24%）净差 14 ≥ 9，修复前即得 1。
- 修复后实测（本机四条同根夹具全部 Passed，`score` 与修复前逐位相同：O041 10001064970、O042 10001354974、O044 10001829972、O045 10001989977）：O045 出现 `axis_widened hp_deficit=14 unavoidable_hp_lost=19 maximum_from_deficit=1 maximum_from_unavoidable=2`，第 2 层被真实搜索（`saved=20 required=27 selected=False`）后仍选第 1 层，终值不变；O041/O042 的 `axis_widened` 未触发（配额未被净差否证），终值、卖血、省血、长期资源、成长奖励全部与修复前相同。
- W2 同时补上第二处门槛链：`AuditOpeningPowerUse`（`Audits.cs:100-102`）此前仍只按净差推 `maximumSmartPotionUses`，与梯度入口不一致，现同样取两轴最大值。`unavoidable_hp_lost` 断言字段已全链接通（协议 `ExpectedInitialUnavoidableHpLost` → `UnattendedSolverMetrics.UnavoidableHpLost` → `Writer` 输出 → ps1/sh 参数 → `SolverPolicy` 断言体），O042 夹具加 `expectedInitialUnavoidableHpLost=1`；正反对照：断 1 时 Passed（runId 2de2962f540d4ab68fc11a6a9999f3d8）、断 2 时 Failed（fb393317a66149b9a9c97e23e41f9e15），证明确实执行到。
- 未观测：修复后的宽档终值。本轮主机 NoGC 达成比例降到 25–30%（`limit` 1.19–1.60GB），修复后 4 次 O045 全落窄档，「宽档不再退到 14」目前是机制推导，需 `limit` ≥ 2.2GB 的机器复跑闭合。
- W1 已实现：`BeamWidthPortfolioGate.cs` 新增 `AllocatedBytesFromOrigin`（基线结束时与 `memberAllocated` 同时刻取信号自其自身起点的累计量）与 `EstimateQuotaCost`，`RejectRefinement` 追加可选参数 `memberMemoryCostBytes`（默认 `null` 即旧口径）；调用方 `CombatSearchCoordinator.BeamPortfolio.cs` 采样同起点量、并对有界精炼成员按 `totalExpanded / 8` 的实际配额投影成本。**已观测效果为覆盖不变**：O042 修复前后两次运行的成员明细逐位相同（index=0/1/5 `ran=True`、index=2/3/4 `skipped=MemoryHeadroomInsufficient`、`ran=3 compared=3 selected_index=0`），因此「消除 portfolio 覆盖缺口」尚未被证明闭合，需 t14 在成对哨兵里按内存档核对。`EstimateMemberCost` 语义未改，checks 工具三断言未改即绿（隔离副本跑出 `BEAM_WIDTH_PORTFOLIO_OK checks=108`）。已知算术（约定：按宽度线性外推、`ceil(base×w×3/(baseW×2))`）：O042 有界成员 `index=5` 配额 `7026/8=878` 节点、实花 `allocated_delta=113047992` 字节，外推给 638031970 字节，比值 5.64；基线自报 1063386616 占墙 1283575704 的 82.9%。
