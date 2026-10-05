# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

0.49.2 全平台发布定稿见 [历史卷 15](archive/development/volume-15.md)，内容性 Mod 提示初始记录见 [历史卷 14](archive/development/volume-14.md)，玩家说明见 [0.49.2 更新日志](releases/0.49.2-RELEASE_NOTES.md)。

0.49.3 发布定稿与 BaseLib 生成牌回归见 [历史卷 18](archive/development/volume-18.md)，框架与局外 Mod 兼容性验证见 [历史卷 17](archive/development/volume-17.md)，玩家说明见 [0.49.3 更新日志](releases/0.49.3-RELEASE_NOTES.md)。

## 下一版本（开发中）

### Smart 药水层的准入不再被 strategic 净差否证（Q010 / W2，Refs #210）

`CombatSearchCoordinator.Audits.cs` 的梯度入口原先只用 `StrategicHpDeficit` 推 `MaximumSmartPotionUses`；净差为 0 时整层药水搜索直接跳过。两条轴来自不同公式，之间没有大小关系：净差是 `ActEndingBossPolicy.StrategicHpDeficit`（`src/Search/ActEndingBossPolicy.cs:139-148`，调用点 `src/Search/CombatSearchCoordinator.cs:966-984`）算出的 `cumulativeHpLost + maxHpDeficit − 持久化回收治疗 + DeathSavePremium − StrategicHpCredit`，比必然受击（`SolverResult.UnavoidableHpLost`，口径 `src/Search/CombatBeamSolver.Phases.cs:541-545,833`）多出 `maxHpDeficit` 与 `DeathSavePremium` 两个加项，又再扣与药水无关的既有治疗（遗物 / `CURL_UP`）——本批产物里就有反例：O042 的 `potionFree` 净差 5 > 必然受击 1（`POLICY_BASELINE kind=potion_free hp_deficit=5` 对上同一次 `RESULT … unavoidable_hp_lost=1`，runId `430a16e9…`、`b973275b…`）。零药路线越好（或治疗越多），净差越小，越容易把整层否证。Smart 政策下改为同时按必然受击求一份配额并取两者最大值，并输出 `SMART_POTION_GRADIENT axis_widened` 诊断。

- 只放宽「跑不跑药水层」的上界：beam / 节点 / 时间 / No-GC 预算一律不变，结果之间的比较规则（`RouteQualityPolicy`、`IsBetterPotionPolicyResult`）未改。单调性由构造成立：新上界取两轴各自配额的最大值，`max(·) ≥ 旧上界`，故只会放宽搜索面，旧行为是被包含的子集（不依赖两轴之间的大小关系，见上条反例）；层内是否被采纳仍由原比较器裁决，因此最终结果按生产比较器不劣于修复前。非 Smart 政策与其余 `MaximumSmartPotionUses` 调用点语义不变。
- 日志算例（同根、同政策、同预算；约定：`projected_battle_hp_lost` 为整场累计、`unavoidable_hp_lost` 为扣除卖血后的必然受击）：修复前宽档（`GC_SEARCH_ALLOCATION_LIMIT=2319212440`、No-GC 弃区 44%）零药最优由 20 改善到 14、strategic 净差 8 < 门槛 9（`src/Search/SolverWeights.cs:99` 默认值，`BLOOD_POTION` 不在 `src/Search/PotionValuationRegistry.cs:19-34`）→ `stop=no_potion_acceptable maximum=0` → 终值 14；同一路线那条 `RESULT` 行给 `unavoidable_hp_lost=13`，按必然受击轴第一瓶本可接受（13 ≥ 9）。窄档（1296864492、24%）净差 14 ≥ 9，修复前即得 1。
- 修复后实测（本机四条同根夹具全部 Passed，`score` 与修复前逐位相同：O041 10001064970、O042 10001354974、O044 10001829972、O045 10001989977）：O045 出现 `axis_widened hp_deficit=14 unavoidable_hp_lost=19 maximum_from_deficit=1 maximum_from_unavoidable=2`，第 2 层被真实搜索（`saved=20 required=27 selected=False`）后仍选第 1 层，终值不变；O041/O042 的 `axis_widened` 未触发（配额未被净差否证），终值、卖血、省血、长期资源、成长奖励全部与修复前相同。注意 `score` 本身随内存档变化：同一条 O044 在本轮后续成对 A/B 里，修复前后两个二进制都给 `score=10001949972`、`projHP=3`（四次一致），而该值在 W2 轮次与认领者机器上是 `10001829972`、`projHP=5`——差异与修复无关，是两个二进制同档同根的实测（详见 TEST_MATRIX「夹具锁定质量界」段与 `coverage/evidence/test-evidence.json` 的 O044 条目）。
- W2 同时补上第二处门槛链：`AuditOpeningPowerUse`（`Audits.cs:100-102`）此前仍只按净差推 `maximumSmartPotionUses`，与梯度入口不一致，现同样取两轴最大值。`unavoidable_hp_lost` 断言字段已全链接通（协议 `ExpectedInitialUnavoidableHpLost` → `UnattendedSolverMetrics.UnavoidableHpLost` → `Writer` 输出 → ps1/sh 参数 → `SolverPolicy` 断言体），O042 夹具加 `expectedInitialUnavoidableHpLost=1`；正反对照：断 1 时 Passed（runId af3fdabed95b4ac6afa3c36162f66550）、断 9 时 Failed（1c332f4748be4c86a2cb104b0cbd4ba3）；两者同为修复后最终二进制 mainAssemblyHash BD84AE5C…，证明确实执行到。- 合并复跑（2026-10-05，merge `7cd38b55` onto origin/main `5d28a1cf`）：四份同根夹具全部 Passed，O041/O042 的 `score` 与修复前逐位相同；O045 零药轴自身达 `proj=0`、`score=10002069976`（runId `8aaa687f…`），据此移除其用药等值断言键（质量界原则，见 `docs/archive/testing/volume-17.md` 注记）；O042 的 `unavoidable=1` 由新协议字段机器断言通过（runId `09320b03…`）。构建二进制 `A29416DE3903…`（哈希仅作 run 身份）。

附带修正：`tools/testing/run-unattended-test.ps1` 的 UTF-8 BOM 在 b414b465 被未声明地剥掉（该文件 :798-799 有中文注释），本轮还原。准确表述：还原相对 b414b465 只有首行一个 BOM 字节序列（`git diff --numstat b414b465..HEAD` 实测 `1 1`）；相对 b414b465^ 还含 b414b465 自身新增的 unavoidable 参数与 JSON 键两行（`git diff --numstat b414b465^..HEAD` 实测 `2 0`），并非除 BOM 外完全一致。
- 同机可达最宽档成对 A/B（1888 vs 1904 MiB、同根 `rootContinuationStamp` 一致）：无修复二进制在 `limit=1979522968` 字节（1888 MiB）复现缺陷原形 `proj=14 / endTurn=7 / 0 瓶`、诊断行 `stop=no_potion_acceptable hp_deficit=8 maximum=0`（runId `b3b6d729552846a39c7d588b73df42ef`）；修复后在 `limit=1996151364` 字节（1904 MiB）同一根下给 `proj=0 / 1 瓶 / score=10002069975`（`8215083c…`），即不再产出该形状。弃区比在本机受物理内存限制最高到 38–41%，更宽档未取到（`limit` 由 `min(configured, headroom + reusableHeap)` 与 `HighMemoryLoadThresholdBytes/100×95` 推导，不是可调环境变量）。
- W1 已实现：`BeamWidthPortfolioGate.cs` 新增 `AllocatedBytesFromOrigin`（基线结束时与 `memberAllocated` 同时刻取信号自其自身起点的累计量）与 `EstimateQuotaCost`，`RejectRefinement` 追加可选参数 `memberMemoryCostBytes`（默认 `null` 即旧口径）；调用方 `CombatSearchCoordinator.BeamPortfolio.cs` 采样同起点量、并对有界精炼成员按 `totalExpanded / 8` 的实际配额投影成本。**决策翻转已被实测取到**（`-MeasureSearchPhases` 下的 `BEAM_WIDTH_PORTFOLIO_GATE` 原值代入）：同一根成对里，无修复件按宽度规则给有界成员估 644623680 字节 > 余量 555298448 而拦下，修复后按决策时累计配额估 203427236 字节 ≤ 余量 555298448 而放行（`quota=floor(11886/8)=1485`，`ceil(min(1074372800,641651968)×1485×3/(7026×2))`，成本降 3.17×），另一主题同型翻转（累计 8986 ⇒ `quota=1123`，693583056 → 223070094，余量 612649268，3.11×）；两侧最终 `score` 逐位相同，即覆盖上升未改变采纳。剩余被拦成员（203、135+band、135+base 等）都不是有界成员、本就不走配额投影，换同起点基数后仍高于余量，属真实余量不足。早前那次「明细逐位相同」的运行是因为 `MeasurePhasePerformance` 未开、且该档余量太小（有界成员成本 208599929 仍 > 205241764），其诊断缺席不构成证据。`EstimateMemberCost` 语义未改，checks 工具三断言未改即绿（隔离副本跑出 `BEAM_WIDTH_PORTFOLIO_OK checks=108`）。已知算术（约定：宽度外推 `ceil(base×w×3/(baseW×2))`；配额一律按**决策时累计** `floor(expandedByMembers/8)`，不是按基线单段节点数）：t5 批次那次 O042 只有基线跑过（累计 7026）⇒ 配额 878、实花 `allocated_delta=113047992` 字节、宽度外推 638031970 字节、比值 5.64；夹具串行那次累计 11886 ⇒ 配额 1485。两次数字都真，差别只在决策时累计量不同，旧文档把两种口径混写已统一为累计口径（GATE 实测 `memory_cost=208599929` 只由 1485 复现，按 878 得 123333830）。基线自报 1063386616 占墙 1283575704 的 82.9%。另按叶路径口径（递归展平、容器节点不单独计）登记同一次运行内 recorded→executed 的差异：O041 为 24 项 = 1 项真实值变化（`performancePreset` →Custom）+ 23 项 recorded 侧未写出的字段在 executed 侧显式默认化；五包该项数 24/23/24/2/25，只用于说明口径，不参与 W1 算术。
### 第三方额外回合来源登记

`ExtraTurnMirrors` 为 `ShouldTakeExtraTurn` / `AfterTakingExtraTurn` 开放第三方登记（泛型与按 `Type` 两种）。
原版龙涎香、帕尔之眼与第三方来源按原生监听顺序统一派发；`HookMirrors.ShouldTakeExtraTurn` 在首个 true 处结束判断，
`AfterTakingExtraTurn` 固定成员后依次结算并处理选择暂停。搜索回放与 `LiveEndTurnRiskEvaluator` 共用入口。重写了却没登记的
第三方类型按回合阶段表的口径停止搜索。监听者掩码用最后一位 `ExtraTurnCallbacks`，两个方法共用。
帕尔之眼的后置回调在所属玩家获得额外回合时标记已使用，与原生方法一致；分支计数仍由 `SimulatedCombatState` 持有。

### 第三方规范 Power 预热登记

0.49.2 起规范 Power 预热只访问原版来源，第三方 Power 在搜索里第一次被施加时会撞上
`PowerDynamicVarMaterializationGuardPatch`。`PowerDynamicVarWarmup.RegisterAdaptedCanonicalPower` 让适配层
显式担保某个第三方 Power 的规范实例可以在主线程物化，建根时随原版一起物化，每局一次；物化失败照常抛出。
默认范围不变，没有登记时行为与此前一致。

### 原版怪物出招表补丁的适配声明

`PredictionModPatchAudit.RegisterAdaptedMonsterMachine(Type, string modId)` 让适配层逐个声明某 mod 对某原版怪物
`GenerateMoveStateMachine` 的补丁已适配。`RejectForeignPatches` 只对出招表方法、且仅对登记的
（出招表的声明类型，mod id）组合放行，声明类型可以是被多个怪物继承的抽象基类；其他 mod、其他审计方法及第三方怪物的整体门禁不变。
没有登记时行为与此前一致。动机：平衡尖塔改写了 45 个原版怪物的出招表，0.49.2 起几乎每场都停在
「求解器暂未适配此内容性 Mod」，而其适配层已逐条核对招式效果与条件分支。

### 单人共享损血剪枝（2026-10-05）

完整合规胜利在串行提交处立即发布，各组合成员共享按失窃量、药水量、成长来源次数及遗物目标分桶的见证。同根同政策的会话仅携带完整零药路线，根戳、损血账本或政策变化时失效。未知回血、开放药水档及未知成长上界保留展开。

可执行零药胜利使用独立于纯 HP 上界的准入条件；成长和遗物路线同样参与下一次同根请求的完整路线选优。纯 HP 界继续使用原有资格限制。原生 `PRIMARY-INCUMBENT-REUSE` 连续三次搜索通过完整路线质量、成长次数、两回合增量回放、政策失效和 live 隔离检查。

未终局无风险节点可以等值截断，可能放弃同收益同损血但更早获胜的路线；不承诺原完整排序或所有有限 Beam 根均不退化。实现及复跑见[策略说明](strategy/hp-loss-pruning/README.md)，新基线结果见[测试入口](TEST_MATRIX.md)。
0.49.4 发布定稿、问题上传引导与 PR #215、#217 合并记录见 [历史卷 19](archive/development/volume-19.md)，玩家说明见 [0.49.4 更新日志](releases/0.49.4-RELEASE_NOTES.md)。
