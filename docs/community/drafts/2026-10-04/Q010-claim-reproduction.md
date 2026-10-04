# Q010 认领者复现记录（fengzhenhong，2026-10-04）

对应批次：[Q010.md](Q010.md)（维护者本地审核稿）。本文件只记录认领者在当前源码上的实际复现结果，
不修改审核稿本身。分支 `perf/batch-q010`，基点 `4533f6bb`。

## 复现方式

五个主题统一从 `combat_start` 同根起搜：

```
tools/replay/run-checkpoint-batch.ps1 -ReplayMode SearchOnly -CheckpointSelector start
```

环境：游戏 v0.111.0、RitsuLib 0.111.0、.NET SDK 10.0.400，headless 全程无需打开游戏窗口。

## 结果与审核稿的差异

| 主题 | 遭遇 | 审核稿原值 | 审核稿改善值 | 本轮实测 | 边界 | 结束回合 | 用药 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| O041 | HUNTER_KILLER_NORMAL | 17 | 12 | **17** | None | 6 | 1 |
| O042 | THIEVING_HOPPER_WEAK | 5 | 1 | **5** | None | 7 | 0 |
| O043 | SPINY_TOAD_NORMAL | 12 | 8 | **18** | **TimeLimit** | 5 | 0 |
| O044 | SLUMBERING_BEETLE_NORMAL | 8 | 5 | **5** | None | 6 | 0 |
| O045 | LOUSE_PROGENITOR_NORMAL | 13 | 1 | **1** | None | 5 | 1 |

三点需要维护者确认口径：

1. **O044、O045 的改善值当前源码已达成**，不再是待优化缺口。O045 实测首段为
   `POMMEL_STRIKE / BURNING_PACT / IMPERVIOUS / BASH`，与审核稿改善路线同构（审核稿记
   `IMPERVIOUS / BURNING_PACT / POMMEL_STRIKE / STOKE`），终值同为 1 战损、endTurn=5；
   差别仅是搜索自行重排了同一组卡的首段顺序。O044 实测 projHP=5、0 瓶、endTurn=6，
   首段已含 GENESIS / PARTICLE_WALL / VENERATE。
2. **O041、O042 复现的是审核稿原值**（17 / 5），改善值（12 / 1）未达成，属当前源码上的真实缺口，
   需要重新定位根因，不能引用审核稿里的旧版归因。
3. **O043 实测 18 战损，劣于审核稿的原值 12 与改善值 8**，且以 `TimeLimit` 收束，
   搜索耗时 180109 ms 打满软预算；审核稿记录其旧边界为 `NodeLimit`。

## O041 现场

- 实测首段：`BLOOD_POTION / SHRUG_IT_OFF / UPPERCUT / INFERNAL_BLADE / UPPERCUT / MOLTEN_FIST / STRIKE_IRONCLAD`；
  审核稿改善首段 `DISTRACTION / STOKE / BATTLE_TRANCE / STOKE`，两者无交集。
- snapshot `cumulativePlayerHpLost=17`、`projectedPlayerHp=60`、`boundaryReason=None`。
- snapshot 带四条 `predictionGaps`，均为 `compensated=true`、`reason=MethodMirrorIncomplete`：
  `ANGER.OnPlay`、`MOLTEN_FIST.OnPlay`、`UNRELENTING.OnPlay`、`UPPERCUT.OnPlay`。

## O042 现场

- 实测首段：`EndTurn / FEEL_NO_PAIN / SHRUG_IT_OFF / ALCHEMIZE / TAUNT / EndTurn / EndTurn / BRAND`；
  审核稿改善首段 `EndTurn / TAUNT / SHRUG_IT_OFF / DEFEND_IRONCLAD`，endTurn=6。
- projHP=5、0 瓶、hpDeficit=-1。

## O043 线索（尚未定论）

循环 region 计数在该主题异常突出，而在 O041/O042/O044 上接近 0：

- `cycleRegionsDetected=3857`、`cycleRegionCandidatesConsidered=32104`、
  `cycleRegionCandidatesAdmitted=27701`、`cycleRegionCandidatesDropped=4403`、
  `cycleRegionProgressEpochs=3818`。
- 按 lane 拆分：probe 16942、normal 10759、progress 0。
- `CycleRegionGlobalProbeAdmissionBudget()` 为 `clamp(MaxExpandedNodes/128, 64, 256)`，
  在 `MaxExpandedNodes=120000` 下为 256/回合。

这条线索尚未定论，本轮未确认 probe lane 超额的成因——region 可能跨回合分布，
使 `searchedTurns=5` 不代表 region 的回合跨度。因此不作为结论提交。

## 本机 headless 环境记录

两处阻塞与绕过方式，供后续批次复用：

1. `run-checkpoint-batch.ps1` 默认把 RitsuLib 拼到 Steam 工坊路径
   `.../workshop/content/2868840/3747602295`。本机实际在 `E:\Slay the Spire 2\mods\STS2-RitsuLib`，
   省略 `-RitsuWorkshopRoot` 会得到 `invalid_archive`。
2. 启动器 `tools/testing/run-unattended-test.ps1:422` 硬校验 CombatSolver.dll / manifest /
   MemoryCleaner.exe 三件产物，缺 MemoryCleaner.exe 直接 throw，批处理表现为 `process_crash`。
   该 exe 是 `net48` 项目，需 .NET Framework 4.8 引用程序集；本机无 VS / SDK / winget，
   改用 NuGet `Microsoft.NETFramework.ReferenceAssemblies.net48` 解出引用程序集安装后构建通过
   （0 警告 0 错误），未改动仓库任何文件。

## 未验证项

- O041、O042 根因未定位；O043 的 `TimeLimit` 收束未做对照实验。
- 五个主题均未执行 `DeploySolver` 原生部署，未做成对耗时对照。
- 两个哨兵（相邻正确场景、未改目标）尚未选定与运行。
- 未运行可见 Steam 会话，无 FPS 与帧时间结论。