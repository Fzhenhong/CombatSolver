# CombatSolver 测试入口

按改动选择最小验证层，方法见 [无人测试](HEADLESS_TESTING.md) 与 [社区验收](community/testing-guide.md)。以下记录保留取得证据时的源码和范围，不能视作本轮重新通过。

历史记录见 [归档索引](archive/testing/README.md)。

## 倾泻与手空效果边界（2026-10-03）

`CASCADE-EMPTY-HAND-NATIVE` 使用报告 d9c106 的倾泻前牌堆顺序及 Shuffle 完整内部状态，单独保留倾泻+和无尽陀螺；无需恢复原包中的重生个体及历史 Power 施加者。未改行为源码上的 `cc37414e00274b6baaf0d877a60e3ac9` 出现原生/模拟手牌偏差；选择痛击的 `e3bac4a083574686b1e9d018ccc23f80` 复现 `NativeChoicePlanMismatchException`，计划 BASH+1、原生仅 STRIKE_IRONCLAD。修复后 `53ba69afa54544f3a1322b42367d5e90` Passed：嵌套坚毅原生页面完成，完整 continuation（有序牌堆、逐实例状态、Power、怪物和九条 RNG）一致；完整动作回放与执行检查点恢复、完成后 Fork、live 不变对账通过。该场景不运行 Solve，不带增量搜索开关，不代表原包整场部署通过。

复跑：`tools/run-unattended-test.ps1 -ScenarioId CASCADE-EMPTY-HAND-NATIVE -EnemyCurrentHp 1000 -HeadlessFastModeForTest Instant -DeploymentFastModeForTest Instant -DeploymentInterActionDelaySecondsForTest 0 -TimeoutSeconds 120 -CleanupInstanceOnExit`；Linux 入口使用同名 scenario 的 GNU 风格参数。详细基线和局限见 [报告记录](issues/axebot-reports-20261003.md)。

`EFFECT-SCOPE-ADJACENT-CONTRACT` 的 `260511f79ef34d9393ef2d5f6b07f672` Passed（37.4秒）。同请求完成无尽陀螺配合Havoc、普通牌、重放牌的三项原生完整状态/RNG差分和效果内普通 Fork 拒绝；五种嵌套执行续接（Havoc/Cascade/DrawPrefix/Repeat/Decisions）的重捕获、全候选、DOP2、原生状态；手动自身选牌的兄弟修改/取消/异常与原生差分；九种药水的原生完整 continuation 和正式候选续接合同。复跑使用相同参数，仅替换 ScenarioId。未运行完整 Solve 或整场自动部署。

## 巨斧机器人近期报告（2026-10-03）

当前主线 `7df1f048` / 0.48.0 上建立三项失败基线。最终通过五项原生完整状态/RNG差分及一项根/Fork合同：金纸+音乐盒在回合末生成虚无复制牌；压缩+两张间隔状态牌的燃料顺序；子弹时间后的 FOLLY 临时星能清理；原力+逐张变牌；SEANCE 抽牌堆选牌变换；金纸延迟计数在根、live推进、父子/兄弟、指纹和续用中的隔离。

| 场景 | 最终 runId | 范围 |
|---|---|---|
| AXEBOT-JOSS-FINAL | `26b7755643d241f091c62f57c9f75a3a` | 原生回合末完整状态/RNG |
| AXEBOT-COMPACT-FINAL | `b45e21f166d042f1ae59079d001e375d` | 原生有序手牌/生成牌/状态/RNG |
| AXEBOT-FOLLY-FINAL | `896d94be19b9473e988aa997fc30c2c7` | 原生回合末完整费用层/状态/RNG |
| AXEBOT-TRANSFORM-FINAL | `8512e36aecd74d93a15f3adb34502190` | 原力与SEANCE两项原生完整差分 |
| AXEBOT-JOSS-DEFERRED-FORK | `e7c0363924e74211b043ebfaca50ac0c` | 根冻结、live推进、指纹/续用、父子/兄弟及逐分支消费 |

复跑在仓库根目录使用 `tools/run-unattended-test.ps1`：前三项分别传 `coverage/unattended/axebot-joss-late-ethereal.json`（IRONCLAD）、`axebot-compact-order.json`（DEFECT）、`axebot-folly-star-cleanup.json`（SILENT）至 `-MonsterMoveChecksPath`；变牌哨兵使用 `axebot-transform-sentinel.json`（IRONCLAD）。均为 `-EncounterId MockMonsterEncounter -TimeoutSeconds 120 -ExitOnComplete -CleanupInstanceOnExit`。Fork 合同使用精确 `-ScenarioId AXEBOT-JOSS-DEFERRED-FORK -RelicsPath coverage/unattended/axebot-joss-deferred-relics.json`，其他参数相同。没有运行搜索，因此不带增量搜索开关。

Release 编译零警告/错误，`REFACTOR_BOUNDARIES_OK search_files=246`。该战斗修复阶段的覆盖工具曾拒绝既有 `PassedWithDocumentedBoundaries` / `PassedWithDocumentedPerformanceRegression` 状态，临时补足解析后又遇 `InfusedCore` 重复构造；当时临时修改已撤回。后续覆盖工具修复结果见下节。完整问题包部署、全场零重算和性能未验证；首轮倾泻包中途缺历史 applier ID 1、原生录制恢复停在输入26，最小重建请求120秒超时，未扩大时间帽；后续单动作修复见上节。详细失败基线及材料边界见 [报告记录](issues/axebot-reports-20261003.md)。本轮创建的无头实例均按清理开关删除。

## 文档维护验证（2026-10-03）

主线 419 份 Markdown、1573 个本地链接和锚点通过；多人分支 400 份、1545 个链接通过；独立日志服务 14 份、30 个链接通过。当前指南与活动记录的长度门槛通过。主线与多人分支 Release 构建均为零警告/错误，主线 Windows 结构门禁通过（246 个 Search 文件）。

两条分支 CoverageCatalog 实际生成，`--verify-state-fields --verify-branch-state-reads` 通过。主线仍有注能核心一个 active exact Hook 缺运行证据；多人分支该目录未报运行证据缺口。目录生成消费历史证据，本次没有运行原生战斗、性能或全量 verify，不扩大旧结果的适用范围。

金币回调负基线 `88843d10d5b94471ad61d36d0efd20f4` 保持 Failed；合并后修复的独立原生证据见下节。

## 0.48.0合入研究分支与前置计划搜索回归（2026-10-03）

合入上游 `a789aad2`，Release构建及Bash结构门禁通过。五角色 `NATIVE-HEALING-ALL-ENCOUNTERS` 各一次 Passed，开局原生合同另一次 Passed。四根 VeryHigh/DOP16 完整筛查的根续用戳及目录指纹与原始基线一致；储君女王423.57秒未获胜，质量门槛拒绝，宿主Passed不等于质量通过。隔离单变量对照恢复300.61秒/67战损/0瓶完整胜利。

最终修复候选恢复原认证根的提前计划资格，保留通用胜利界消费。`KNOWN-HEALING-OPENING` / `7241445571bb49179e4d56b071892aa6` 和储君 `NATIVE-HEALING-ALL-ENCOUNTERS` / `e15b0024740e403a922963721bf705f8` 严格原生合同 Passed；Bash/PowerShell门禁均 `search_files=246`。最终29根回归完成：26个原始获胜根均保留胜利、3个原始未获胜根保持未获胜，战损没有超过原始基线。受试体单次内存超限记录保留，最终ABBA对照提速3.06倍、峰值+6.95%、战损26→24；其原生全程部署等待第一张Automation后超时，未列为原生验收完成。猎手首领47/62战损波动另有四次对照记录，结果与限制见[合并检查](performance/upstream-0480-merge-check-20261003.md)。回复入口扫描工具构建及已知直接/间接来源清单断言通过，不替代原生语义差分；其余审计未完成项见[第一阶段来源审计](performance/native-health-source-audit-20261003.md)。

## 金币、最大生命与 HP 回调（2026-10-03）

合入 `56b6d6ee` 后，`GOLD-HEALING-MECHANISMS`、`MAX-HP-HEALING-CALLBACKS`、`FEED-MAX-HP-CAP` 分别以 `c68a0c923fa84f48b9f4ce1ee9f68831`、`1e611f0aa1624d908d7c4f07d893fde4`、`de2062aa4fc847f98b7293a8d848d22b` Passed。覆盖 24 项检查，包含完整状态/RNG、父子与 live 隔离、未知回调拒绝、金币修正、真实致命出牌/用药、最大生命封顶和 HP 阈值回调。Release 构建零警告/错误。具体原生前提、复跑方法、失败夹具记录与性能待验收项见[金币与最大生命回复链](performance/gold-max-hp-healing-20261003.md)。

`RELIC-MAX-HP-HEALING-BOUNDS` 的 `35f61a69acf74cc3b6cc56f6e5269f8d` 原生回调负基线保持 Failed（已知界 0、实际回复 1）。修复后 `b16bb21caa4441bda3f931705fc0dddd` Passed：ChosenCheese / DarkstonePeriapt 的原生回调、完整状态/RNG、父分支/live 隔离及各自熔化排除，共四项。IRONCLAD、50/80、120 秒帽，不运行 Solve；不代表完整战斗结束派发或永久牌组变更已模拟。复跑使用同名 scenario 与上述初始生命参数，Linux/PowerShell 各自原生无人测试入口，必须带实例清理。
