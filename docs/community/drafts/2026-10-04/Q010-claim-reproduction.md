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

## 未验证项

- 尚无任何代码改动，因此没有「修改前失败 / 修改后通过」对照。
- O041 已定位到判据链与基准口径（见 `.local/tool-tasks/q010/O041-analysis.md`），本轮判定为
  口径差异而非选择错误，未改码；O042 的根因与 `MemoryHeadroomInsufficient` 是否判定过严仍未确认。
- 五个主题均未执行 `DeploySolver` 原生部署，未做成对耗时对照。
- 两个哨兵（相邻正确场景、未改目标）尚未选定与运行。
- 未运行可见 Steam 会话，无 FPS 与帧时间结论。