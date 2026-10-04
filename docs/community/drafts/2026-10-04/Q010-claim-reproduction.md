# Q010 认领者复现记录（fengzhenhong，2026-10-04）

对应批次：[Q010.md](Q010.md)（维护者本地审核稿）。本文件只记录认领者在当前源码上的实际复现与定位结果，
不修改审核稿本身。分支 `perf/batch-q010`，基点 `4533f6bb`。

## 复现方式

五个主题统一从 `combat_start` 同根起搜：

```
tools/replay/run-checkpoint-batch.ps1 -ReplayMode SearchOnly -CheckpointSelector start
```

环境：游戏 v0.111.0、RitsuLib 程序集 0.6.2.0、.NET SDK 10.0.400，headless 全程无需打开游戏窗口
（详见文末「本机 headless 环境记录」）。

## 结果

「本轮实测」列一律取入选结果的 `comparisonQuality.projectedBattleHpLost`（整场累计口径，含本请求起算前已发生的战损）。
「搜索耗时」取 `RESULT` 的 `total_elapsed_ms`（本请求所有 solver 会话之和），「入选解展开」取 `RESULT` 的 `expanded`
（产出该实测值的那个 solver），「墙钟」取 `timings.json` 各阶段之和（含约 19 秒回放启动）。三个口径不再混用：

| 主题 | 遭遇 | 审核稿原值 | 审核稿改善值 | 本轮实测 | boundary | 搜索耗时 | 入选解展开 | 墙钟 | 性质 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| O041 | HUNTER_KILLER_NORMAL | 17 | 12 | **17** | None | 20923ms | 36177 | 40799.1ms | 已收敛；12 与本行不同根 |
| O042 | THIEVING_HOPPER_WEAK | 5 | 1 | **5** | None | 14703ms | 6261 | 34766.7ms | 已收敛，策略缺口 |
| O043 | SPINY_TOAD_NORMAL | 12 | 8 | **18** | TimeLimit | 180170ms | 130228 | 199836.9ms | 未收敛，预算受限 |
| O044 | SLUMBERING_BEETLE_NORMAL | 8 | 5 | **5** | None | 12079ms | 4760 | 31358.5ms | 已达目标 |
| O045 | LOUSE_PROGENITOR_NORMAL | 13 | 1 | **1** | None | 9743ms | 5142 | 30124.6ms | 已达目标 |

O041 旧版记录的 5.3s / 3062 是零药基线成员（`BEAM_WIDTH_PORTFOLIO_MEMBER index=0`，
`elapsed_ms=5272`、`expanded=3062`）的值，不是产出实测 17 的那个 solver；该请求的 `total_expanded=70196`。

本批因此分成三类，处置方式不同：

- **A 类，目标已达成**：O044、O045。当前源码同根即达改善值，只需回归夹具与前后对照。
- **B 类，已收敛但次优**：O042 与（按整场战损口径的）O041。搜索跑完（`boundary=None`）
  仍得不到改善值。O041 的对照基准另有问题，见其小节，不能按「同根跑输 12」表述。
- **C 类，未收敛**：O043。搜索把 profile 软预算耗尽仍未收敛，当前预算下无法判定是否存在更优路线。

## A 类：O044、O045

O045 实测首段 `POMMEL_STRIKE / BURNING_PACT / IMPERVIOUS / BASH`，审核稿改善路线记
`IMPERVIOUS / BURNING_PACT / POMMEL_STRIKE / STOKE`。两者**不是同一组卡的重排**：实测 T1
第 4 张是 `BASH`，包内改善路线第 4 张是 `STOKE`，卡集不同，能对照的只有终值
（同为 1 战损、endTurn=5、`won=True`；实测计划内另有 1 瓶 `ENERGY_POTION`）。
O044 实测 projHP=5、0 瓶、endTurn=6，与审核稿改善值一致。

这两项的改善值与本项实测在同一口径上可比（包内记录的 `battleHpLostSoFar=0`、
`startTurnNumber=1`，整场预测即 1 与 5），因此按回归夹具交付，不含新的生产补丁。

## B 类之一：O041

运行时序（相对首事件毫秒，`.local/checkpoint-batch/Q010-O041-so/.../runtime-events.json`）：

```
 7296  POLICY_BASELINE          kind=potion_free won=True  hp_deficit=17 enemy_hp=0 boundary=None   (入选零药 primary)
 7384  SEARCH_INTERIM_RESULT    potions=0  projected_battle_hp_lost=17
 7600  POLICY_BASELINE          kind=potion_free won=False hp_deficit=12 enemy_hp=70 boundary=None (portfolio index=4 半成品)
16318  POLICY_BASELINE          kind=potion_free won=True  hp_deficit=26 enemy_hp=0                 (带药层自身候选里的另一条零药)
16318  POLICY_BASELINE_OVERRIDE kind=potion_free won=True  hp_deficit=11
16341  SEARCH_INTERIM_RESULT    potions=1  projected_battle_hp_lost=17
16341  SMART_POTION_GRADIENT layer=1 won=True hp_deficit=-5 saved=16 required=9 acceptable=True selected=True expanded=36177
16343  SMART_POTION_GRADIENT result stop=threshold_met maximum=1 selected_potions=1
```

原记录「同根已找到零药 11 HP 路线、不劣于审核稿 12 HP 目标」**不成立**，是跨口径比较：
`hp_deficit=11` 是 `POLICY_BASELINE_OVERRIDE` 打的 strategic 净值（构造时传入 `StrategicHpDeficit(...)`，
`src/Search/CombatSearchCoordinator.Audits.cs:867-871`），而审核稿的 12 是原始战损口径。同名字段
`hp_deficit` 在 `POLICY_BASELINE` 行打的是 `features.CumulativePlayerHpLost`
（`src/Search/CombatBeamSolver.FinalPlanOrdering.cs:66,199`）。

入选零药 primary 的**两个口径同时闭合**：原始战损 17（`rel 7296`，动作前缀
`UPPERCUT / SHRUG_IT_OFF / INFERNAL_BLADE`，即 `BEAM_WIDTH_PORTFOLIO_MEMBER index=0`、
`expanded=3062`、`battle_hp_lost=17`），strategic 11（原始 17 扣掉治疗后 6）。
`rel 16318` 的 26 前缀是 `UPPERCUT / STRIKE_IRONCLAD / SECOND_WIND`，属于带药层自身候选集里的
**另一条**零药路线，因 `minimumPotionUses=1` 被 `FinalPlanOrdering` 过滤，与同毫秒的 11 不同源。
incumbent 链 `13 → 11(source=no_explicit_potion, turn=6) → 9 → 0 → -5` 印证 11 归属于入选 primary。

因此带药层的**原始战损同样是 17**：`saved=16` 全部来自 `BLOOD_POTION` 的一次治疗
（`ROUTE_HEALTH ActionIndex=0 Kind=heal Requested=16 Before=61 After=77`），没有减少任何一次受击。
原始战损轴上 17 = 17，带药层既无收益也无退化。

放行带药层的判据也要更正归属：跨结果选择由 `Audits.cs:969-974` 的
`IsSmartPotionGradientCandidateAcceptable`（定义在 `:1014-1021`，
`candidateWon && (!potionFreeWon || hpSaved >= hpRequired || protectsLoot)`）与
`IsBetterPotionPolicyResult`（经 `RouteQualityProjection.PotionPolicy`，`src/Search/RouteQualityPolicy.cs:78-95`）
两步决定，两者都在 strategic 轴上；`src/Search/PotionUsePolicy.cs:111-114` 的 Smart 资格是 solver
内部的候选过滤，不在这次跨结果选择的判据链上。

**更要紧的是基准本身不同根**：审核稿的 `17 → 12` 取自包内两个不同检查点（本包
`preflight.json` 的 `index.searchResults` 与 `report.manualProjectionComparison`）——17 记录于
`checkpointId=…:1`（`startTurnNumber=1`、`battleHpLostSoFar=0`、`potionCount=1`），12 记录于
`…:4`（`startTurnNumber=3`、`battleHpLostSoFar=6`、`potionCount=0`），而
`manualProjectionComparison` 的 `stateDifference="field=hp expected={77} actual={61}"` 说明玩家在
T1-T2 已偏离原路线。`projectedBattleHpLost` 的口径是「起算前已损 + 后续预测」
（`src/Search/CombatBeamSolver.Phases.cs:838`），所以 12 是**从已经打成这样的第三回合局面**重算出的
整场预测，不是从 `combat_start` 同根可比的更优解。

本轮从 `combat_start` 同根跑出的 30 个动作与该包 `:1` 记录的 17 路线逐 token 相同
（仅两枚 `ANGER` 的 occurrence 编号不同），即当前源码**没有复现出退化**，也没有一个同根可达的
改善目标可判失败。根因定位与两种口径的取舍见 `.local/tool-tasks/q010/O041-analysis.md`。

## B 类之二：O042

`boundary=None`、墙钟 34766.7 毫秒（profile 软预算 300000 ms）、请求累计展开 50512 节点
（上限 500000，入选解自身 6261），**搜索已收敛但未能找到优于 5 战损的路线**。

portfolio 6 个成员中 4 个以 `MemoryHeadroomInsufficient` 跳过，
实际参与比较的是 index=0（beam=135）与 index=5（beam=54）。

实测首段：`EndTurn / FEEL_NO_PAIN / SHRUG_IT_OFF / ALCHEMIZE / TAUNT / EndTurn / EndTurn / BRAND`；
审核稿改善首段 `EndTurn / TAUNT / SHRUG_IT_OFF / DEFEND_IRONCLAD`，endTurn=6。

`MemoryHeadroomInsufficient` 让大部分 portfolio 成员无法运行，是 O042 与 O041 共同的可疑点
（O041 侧为 `members=5 / ran=2 / compared=1`：index=1/2/3 以 `MemoryHeadroomInsufficient` 跳过，
index=4 跑了 382 节点后 `NodeLimitNotTerminal`、不参与比较）。本轮尚未确认该记忆门限是否在
本机环境（NoGC 区域、进程驻留）下判定过严。

## C 类：O043

对照实验（同根、同政策，只改外层超时）：

| 运行 | TimeoutSeconds | 入选解搜索耗时 | 请求搜索总量 | 墙钟 | expanded | boundary | projHP |
| --- | --- | --- | --- | --- | --- | --- | --- |
| run 1（`Q010-O043-so2`） | 300 | 180109ms | 180170ms | 199836.9ms | 130228 | TimeLimit | 18 |
| run 2（`Q010-O043-big`） | 900 | 180080ms | 180142ms | 200597.5ms | 127399 | TimeLimit | 18 |

旧版对照表把 run 1 的搜索耗时（180109）与 run 2 的墙钟（200597）并排，混了两个口径；
两行的搜索耗时其实几乎相同。

把外层超时放大到 900 秒并不改变结果，瓶颈是该包 profile 自身的
`softTimeBudgetMilliseconds = 180000`（beam 90、maxExpandedNodes 250000、preset High）。
节点预算远未触顶（约 13 万 / 25 万），**先耗尽的是时间预算**。

已排除的两处嫌疑（负结果）：

1. **循环 region 计数**：`BEAM_WIDTH_PORTFOLIO result members=6 ran=1 compared=1`，
   本轮只有 index=0 运行，计数不跨成员累加；`CycleRegionGlobalAdmissionBudget` 的作者自检
   覆盖停滞上限与 512 硬上限，`CycleRegionRetentionTransaction` 按 turn 复制预算字典。
   计数高只因该遭遇的 region 数量本身大。
2. **portfolio 跳过 5 个成员**：`src/Search/BeamWidthPortfolioGate.cs` 第 7-9 行明确
   `FrontierExhausted` 口径为「基线没有被任何上限截断（`SearchBoundaryReason.None`）」。
   O043 基线以 `TimeLimit` 收束，跳过精炼成员、把预算留给基线是**设计行为**。

结论：O043 的 18 战损是**时间预算耗尽导致的未收敛**，不是候选被错误丢弃。
要论证能否恢复 8 战损，需在提高该包 profile 软预算的同根条件下对照；
但直接调大预算会使本次质量对照失去可比性（验收要求同政策、同预算），需与维护者确认是否在批次范围内。

## 本机 headless 环境记录

1. 工坊默认路径不在 `run-checkpoint-batch.ps1`：该脚本第 11 行的 `-RitsuWorkshopRoot` **没有默认值**，
   只在非空时才拼进 `--ritsu-root`。真正的默认值在
   `tools/replay/CheckpointTool/BatchRunner.cs:324`，拼成
   `<game-root>/../../workshop/content/2868840/3747602295`。本机 RitsuLib 实际在
   `E:\Slay the Spire 2\mods\STS2-RitsuLib`，省略 `-RitsuWorkshopRoot` 会得到 `invalid_archive`。
   （`tools/testing/run-unattended-test.ps1:9` 另有硬编码 `D:\Steam` 默认，是第二处入口。）
   本机实际加载的 RitsuLib 程序集版本是 **0.6.2.0**（`mod_manifest.json` 的 `version: 0.6.2`）；
   此前写的「RitsuLib 0.111.0」是 `lib` 目录按游戏版本匹配的路径名，游戏本体为 v0.111.0。
   该目录与包内记录的 RitsuLib 各程序集版本一致，但 `moduleId` 全部不同
   （`policy.json` 的 `modEnvironmentComparison` 因此报 `build_changed`）。
2. 三处首跑 `process_crash` 的原因**不同**，不都是 MemoryCleaner：
   `Q010-O043-so` 与 `Q010-O044-so` 的 `launcher-error.log` 指向
   `run-unattended-test.ps1:466`，即 launcher.lock 争用（同一 headless 实例被并发占用），
   与 MemoryCleaner 无关；只有 `Q010-O045-searchonly` 首跑是 `run-unattended-test.ps1:422`
   硬校验 CombatSolver.dll / manifest / MemoryCleaner.exe 三件产物失败。该 exe 是 `net48` 项目，
   需 .NET Framework 4.8 引用程序集；本机无 VS / SDK / winget，改用 NuGet
   `Microsoft.NETFramework.ReferenceAssemblies.net48` 解出引用程序集安装后构建通过
   （0 警告 0 错误），未改动仓库任何文件。

## 五包改善值录制检查点与同根可比性

本批五个主题的「改善值」并非都从 `combat_start` 起算。逐包读 `preflight.json` 的 `index.searchResults`
（该表是原报告录制时的求解产物，含 `startTurnNumber`、`battleHpLostSoFar`、`projectedBattleHpLost`、
`boundaryReason`），口径为 `projectedBattleHpLost = 起算前已损 + 后续预测`
（`src/Search/CombatBeamSolver.Phases.cs:838`，`futureHpLost` 取自 `:541`）：

| 主题 | 原值录制点 | 改善值录制点 | 起算回合 / 已损 | 同根可比 |
| --- | --- | --- | --- | --- |
| O041 | 17 @ `…:1` | 12 @ `…:4` | T3 / 已损 6 | **否**，中途残局 |
| O042 | 5 @ `…:1` | 1 @ `…:5` | T3 / 已损 1 | **否**，中途残局（整场等价即 1，其「后续」为 0） |
| O043 | 12 @ `…:7`、`…:9`（`boundary=NodeLimit`） | 8 @ `…:9`、`…:11` | T2 / 已损 0 | **否**，第一回合之后的状态 |
| O044 | 8 @ `…:1` | 5 @ `…:3` | T2 / 已损 5 | **否**，中途残局 |
| O045 | 13 @ `…:1` | 1 @ `…:3` | T1 / 已损 0 | **是**，同根可比 |

五包的**原值**一律录于 `startTurnNumber=1`、`battleHpLostSoFar=0`，与本轮 `-CheckpointSelector start`
同根，可直接对照；**改善值**只有 O045 同根。O041 与 O042 另有玩家偏离的直接证据：
`report.manualProjectionComparison` 的 `stateDifference` 分别是 `field=hp expected={77} actual={61}`
（O041：计划第一回合喝 `BLOOD_POTION` 到 77，实机只到 61）与 `field=hp expected={55} actual={56}`
（O042：实机比计划多 1 点血）。也就是说，除 O045 外的四个「改善值」描述的是**已经打成那样的局面之后**
还能省多少血，不是从开战起可达的更优世界线。

据此本批的夹具目标调整为：

- **A 类**：O044、O045 锁同根实测终值（5 与 1），只断言战损、结束回合、边界与用药数，
  不断言路线同构（O045 实测第一回合第 4 张是 `BASH`，包内改善记录是 `STOKE`，卡集不同）。
- **B 类**：O041、O042 改为**同根非退化**夹具——锁当前源码在同根上的实际产出
  （O041 为 17 战损 / 1 瓶 / `boundary=None`；O042 为 5 战损 / 0 瓶 / `boundary=None` /
  其中主动卖血 4），不断言 12 或 1。O041 的同根产出与包内 `…:1` 记录的原路线逐 token 相同，
  O042 与包内 `…:1` 的 26 个动作多重集完全相同（仅三处相邻顺序差异），因此这两条夹具的含义是
  「未复现退化」，不是「未达目标」。
- **C 类**：O043 维持「时间预算耗尽导致未收敛」的边界记录，不设修复，也不设达标夹具。

夹具输入为 `coverage/fixtures/regressions/community/q010-o04{1,2,4,5}-same-root-*.json`，
复跑命令见 [测试矩阵](../../../TEST_MATRIX.md)。

## 同根夹具首次执行结果（2026-10-04）

四条夹具串行执行（单实例锁，逐条完成再下一条），均带 `-CleanupInstanceOnExit`、
`-CheckpointSelector start`、`-ReplayMode SearchOnly`、`Instant`、300 秒上限。

| 主题 | runId | 判定 | 战损 | endTurn | boundary | 瓶 | 省血/卖血 | score | 墙钟 ms | total_expanded / total_transitions / total_elapsed_ms |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| O041 | `e1ef0c5f460d4128a824ba43adf2ce8f` | Passed | 17 | 6 | None | 1 | saved 16 / required 9 | 10001064970 | 39990.4 | 77995 / 322339 / 20496 |
| O042 | `430a16e96916498b9908ef5af1fd67ff` | Passed | 5 | 7 | None | 0 | sold 4、unavoidable 1 | 10001354974 | 33805.4 | 51366 / 211256 / 13866 |
| O044 | `c1c11f6de94b41808b0f1cfbddbc5be5` | Passed | 5 | 6 | None | 0 | unavoidable 5、sold 0 | 10001829972 | 30367.9 | 11851 / 79116 / 11296 |
| O045 | `d67f8317705a4a9b9d355b8ce3996756`（同参数复跑 `e3632e589f2b4ef4939c49a661a2c931`） | **Failed** | 14 | 7 | None | 0 | saved 0 / required 9 | 10001209968 | 28663.1 | 10109 / 46116 / 9342 |

三条 Passed 的 `score` 与认领者 `SearchOnly` 产物逐位相同（O041 10001064970、O042 10001354974、
O044 10001829972），计划路线也与认领者产物**逐 token 相同**：O041 30 个动作、O042 26 个、O044 28 个，
按 `turn:kind:cardId+升级位`（药水取 `potionId`）做序列比较与多重集比较都相等。因此这三条既是首次
通过证据，也是「同一源码在同根上可重复」的证据。

O044 同时是一次跨环境对照：本轮分配墙 2325295000（认领者 1283048684，No-GC 弃区比例 44% vs 24%），
成员覆盖同为 `ran=1 compared=1`，终值、`score` 与路线全同，展开 11851 vs 11913、搜索耗时 11296ms vs
12079ms（−6.5%）。质量无退化、耗时无明显增加，符合 AGENTS.md 第 1 节对同条件哨兵的口径。

### O045 失败：覆盖率升高暴露的判据缺口，不是退化

失败信息「首轮路线使用药水 0 瓶，预期为 1 瓶」。两侧输入与政策可证完全相同：`rootContinuationStamp`
长 3776 字符、SHA256 均为 `F95E3DA5A9F50626FE74A5FF1302BB12B97690FD4924BBD3E31722E9252AD006`；
`policy.json` 的 `executedPolicy` 逐字段无差异；`request.json` 的 seed/ascension/encounter/act/
`enemyCurrentHp`/`checkpointSelector`/`replayMode`/`headlessFastModeForTest`/`stopFlag` 全同；`src` 与
`tools` 相对认领者运行时零改动。同参数复跑结果一致（同 `score`、同终值）。

差异只在内存余量，并沿这条链改变结果：

1. 本轮墙 2319212440（认领者 1296864492），beam 60 的有界精炼成员不再被 `SkippedMemoryHeadroom`
   拦下，于是真的跑了：`ran=3 compared=2`（认领者 `ran=1 compared=1`）。
2. 多跑成员把**零药最优**从 20 改善到 14（`POLICY_BASELINE` 序列本轮 `20, 14, wonFalse:0`，认领者
   `20, 0`），选中成员由 index 0（beam 90、战损 20）换成 index 1（beam 60、战损 14）。
3. 战损 14 的 strategic 净差为 8，低于门槛 `firstPaidPotionHpRequired=9`（`BLOOD_POTION` 不在
   `src/Search/PotionValuationRegistry.cs:19-34` 表内，取 `src/Runtime/SolverWeights.cs:99` 默认值 9；
   本轮 `potion_reward=Unknown/-/credit=0`，无替代治疗抵免）。于是 `MaximumSmartPotionUses`
   （`src/Search/CombatSearchCoordinator.cs:1009-1052`）返回 0，Smart 梯度在
   `src/Search/CombatSearchCoordinator.Audits.cs:859-865` 以 `stop=no_potion_acceptable maximum=0`
   直接返回，**整层药水搜索被跳过**。
4. 认领者侧零药只有 20，门槛通过，梯度搜出 1 瓶省血 19、整场战损 1 的路线并选中。

即：同一输入、同一政策、同一预算下，更宽的内存覆盖反而产出更差结果（14 vs 1）。缺口出在「零药更优
→ 判定药水不可能划算 → 不搜药水层」这一步：门槛只看 strategic 净差，被更好的零药结果压低后就否证了
整个药水层，而该层里存在终值更优的路线。这是判据口径问题，不属于「源码相对包内路线退化」，也不在本批
修复范围。按 AGENTS.md 第 1 节，本批不改这段判据：它影响所有 Smart 政策场景，需要独立的「同根质量无
退化 + 耗时无明显增加」哨兵，且应与门限口径偏差一起交维护者定口径。

### O042 的 unavoidable 分量按 diagnostics 闭合

同一次执行给出 `unavoidable_hp_lost=1` 与 `sold_hp=4`，相加即断言的整场战损 5，因此 t3 留待对账的
「被迫受击 = 1」分量在本次得到 diagnostics 层面的确认（仍无协议断言字段，见下节工作项）。

本轮 O042 同样是天然的覆盖率 A/B：分配墙 2298331032（认领者 1283575704），成员覆盖从 `ran=2` 升到
`ran=3 compared=3`（多跑 beam 90 与 beam 54，均得 16，beam 54 因 NodeLimit 结束），`DEFERRED_OPENING_POWER
power=FEEL_NO_PAIN hp_lost=5` 照旧触发，终值 5、`score` 与路线都不变。这正面回答 t3 的未验证项 1：
覆盖度提高后零药层没有搜出比 16 更好的结果，5 仍来自不经门限的 deferred 补查，因此门限口径不是 O042
终值的原因，放宽门限的收益也未在此体现。

### 退出码注意

带 `-CleanupInstanceOnExit` 时，实例目录 `.local/headless-instances/<实例>` 偶尔删不掉
（`game\data_sts2_windows_x86_64\0Harmony.dll` 句柄未释放），启动器因此返回退出码 1；这不影响
`result.json` 的 `Passed` 判定，本次四条的退出码均为 1（O045 同时是断言失败）。残留实例目录需手动
删除后再跑下一条，本次已确认最终 `headless-instances` 为空。

## 门限口径偏差：待维护者决策的工作项

O041 与 O042 的共同线索是 beam 宽度组合的精炼成员几乎全部被内存门限拦下
（O041 `members=5 ran=2 compared=1`，O042 `members=6 ran=2 compared=2`）。
逐位复算后，`src/Search/BeamWidthPortfolioGate.cs:86-118` 的算术与它拿到的输入一致，
**判定为环境保守而非逻辑过严**；但输入侧有两处口径问题值得维护者决定是否单独立项：

1. **成本侧与余量侧用了不同起点的分配量。** 门控用基线成员**自己那一段**的分配增量外推每个成员
   的成本（`src/Search/CombatSearchCoordinator.BeamPortfolio.cs:120,139-140`），
   却拿 `SearchMemoryPressureSignal.RemainingBytes`（`src/Runtime/SearchMemoryPressureSignal.cs:113-117,157-177`，
   **自区域配置时刻起**的累计分配）当余量。O042 基线自报 `allocated_delta=1063386616`，
   是整面分配墙 1283575704 的 82.9%，而反推出的判拒时刻余量只有约 0.64–1.06 GB；
   结果凡宽度 ≥ 基线宽度的成员必然被拒。O041 同构：60 宽成员外推成本 1229477958
   为其墙 1241302252 的 99.05%，差 11824294 字节被拒。
2. **有界精炼成员被显著高估。** `src/Search/BeamWidthPortfolio.cs:284-285` 把它的节点预算压到
   `totalExpanded / 8`（O042 为 7026/8 = 878，与日志 `nodes=878` 一致），实际只分配 113047992 字节，
   而门控按宽度线性外推给它的预算是 638031970 字节，高估约 56 倍。

3. **`unavoidable_hp_lost` 缺协议断言字段。** 无人测试的 `ExpectedInitial*` 只有 `soldHp`/
   `soldHpAtMost`/`hpLostAtMost` 一类，被迫受击分量只出现在 `RESULT` 诊断行
   （`src/Runtime/SolverDiagnostics.cs:209`），因此「战损构成」这类断言只能靠人工对账；若要机器化，
   需要新增 `ExpectedInitialUnavoidableHpLost`，属 `src/Testing` 生产协议改动，不在本批范围。

两者的直接环境诱因是 NoGC 区域被系统内存压力拒开：`GC_NO_GC_REGION_DECLINED
percent_of_configured=24`，配置 16 GB 只能保留 3.95 GB，中途区域重建后墙降到 1.28 GB
（`physical_load` 8.6→10.6 GB、`system_limit` 12.56 GB）。

本轮**不改生产代码**，理由：AGENTS.md 第 1 节禁止用扩大 Beam、节点、时间或 No-GC 预算掩盖问题，
而门限校准属口径改动，需要独立的「同根质量无退化 + 耗时无明显增加」哨兵；
且 O042 的最终结果并不出自被拦的精炼成员，而出自不经该门限的 `DEFERRED_OPENING_POWER` 补查
（`src/Search/CombatSearchCoordinator.cs:564-601`），放宽门限对本批两个主题的收益未经证实。

## 未验证项

- 本批无生产代码改动，因此没有「修改前失败 / 修改后通过」同输入对照；质量无退化的直接证据是
  `git diff` 在 `src` 与 `tools` 为空，加上 O044 的同根同政策跨环境重跑终值、`score` 与路线全同。
- 四个同根夹具已于 2026-10-04 首次执行完毕（见「同根夹具首次执行结果」）：O041/O042/O044 Passed，
  O045 Failed；结构化证据已按实际状态登记（三条 Passed、一条 Failed）。
- O045 的失败只在这台机器的内存余量下复现（分配墙约为认领者的 1.8 倍）。未在认领者那档余量下重跑
  以证明「余量窄即 Passed」——该侧证据来自认领者产物，非本轮执行。
- O042 的「被迫受击 = 1」分量仍**没有协议断言字段**（`UnavoidableHpLost` 只在
  `RESULT … unavoidable_hp_lost=` 诊断行输出，`src/Runtime/SolverDiagnostics.cs:209`）；
  本轮按 diagnostics 对账成立（1 + sold 4 = 5），但夹具断不到它，字段缺失作为工作项登记在门限一节。
- O041 已定位到判据链与基准口径（见 `.local/tool-tasks/q010/O041-analysis.md`），判定为
  口径差异而非选择错误，未改码；O042 见 `.local/tool-tasks/q010/O042-analysis.md`。
- 五个主题均未执行 `DeploySolver` 原生部署，未做成对耗时对照；PR 保持 Draft、等维护者对口径的答复，
  部署对照不属于本批收口条件。
- 质量无退化哨兵只覆盖到 O044（同根同政策同预算、跨内存档位重跑，终值/`score`/路线全同，
  搜索耗时 11296ms vs 12079ms）。「未改目标哨兵」在本批没有对应物：没有任何搜索策略代码改动，
  `src`/`tools` 的 diff 为空就是该哨兵的定义性证据。成对耗时只到 SearchOnly 搜索与 launcher 墙钟
  两个口径（见上表），未做同一进程内的严格交替计时。
- 未运行可见 Steam 会话，无 FPS 与帧时间结论。
