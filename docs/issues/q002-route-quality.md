# Q002 路线质量排查（2026-10-03）

任务 [#150](https://github.com/Torch1230/CombatSolver/issues/150) 的 O001–O005 整批由 `shun-tong` 认领。当前是材料核验与基线阶段，未修复搜索、未完成整场质量验收。后续按主题组织提交，使用 `Refs #150`；全批验收前不使用 `Closes #150`。

基线源码 `9d21f5fc`，manifest 0.48.0；Windows 游戏 0.111.0、RitsuLib 0.6.5。已合入上游 `556e7299`，本轮未修改 C#。原始 ZIP、完整日志和诊断政策保存在 `.local/issue-bundles/Q002/`，逐请求证据保存在 `.local/checkpoint-batch/`，不提交玩家存档。

## 材料与严格恢复

五包安全盘点通过，原生事件索引均声明完整；声明完整不代表已回放至战斗结束。`start` 验证开战及首个可操作状态；`latest` 只诊断录制后的检查点，不证明求解器能从开局找到玩家路线。

| 主题 / 报告 | 已取得证据 | 当前限制 |
|---|---|---|
| O001 无厌沙虫 / `be3cf3410d4444188ca98ef79b2052f6` | 开战完整 continuation 与原生状态通过，run `20de6853260b40d3aaf59902df212534` | 后续恢复在事件 30 的选牌状态格式处失败，见下文；未证明完整人工路线可比较。 |
| O002 女王 / `8fbb4f7f7d1b4e30832296d73e7a114d` | 开战失败 run `facf75d4184d42b18affd343bc461218`：遗物槽 1 原生导入为 `DEPRECATED_RELIC` | 存档实际持有第三方模型 `ANCIENT_AFFECTION_RELIC_DEVOTED_BOOMING_CONCH`；需要原版内容代表，不能删除遗物伪造同根。 |
| O003 永世沙漏 / `c5e305b95ff444fea1949a58c0b17b04` | 开战通过 run `729dd829931e4489acd8c3059777663a`；最新检查点 T4、29 事件的完整 continuation 与原生状态通过 run `ffb487504c644112a8f349b0d18ec56d` | 原报告 55→2 是跨 T1/T4 的预测变化，人工实机通关尚未验证。 |
| O004 乐加维林族母 / `b76f10234d8b435286083ae1e614646d` | 最新检查点 T1、4 事件通过 run `3d9e776c288440d0833a07fedbb9cab2`，包含喝能力药水、选牌、打出 Tools of the Trade | 51→5 是预测变化；额外药水按每瓶 9 HP 计机会成本，需区分已经使用与未来计划使用。 |
| O005 实验体 / `93133dcfdb114695b08aec08c36716c4` | 最新检查点 T2、12 事件通过 run `9d69774307834aa5ac38b1ecab3ec10f`，完整 continuation 与原生状态一致 | 55→10 是预测变化；尚无同预算质量或整场部署结论。 |

最新检查点三包证据目录为 `Q002-remaining-recorded-prefix-restore`。开战三包证据目录为 `Q002-round1-opening-restore`。批次启动器均记录退出及实例删除。

## O001 首个失败原因

`Q002-O001-recorded-prefix-restore` / run `912ff150d3f44cda83964a7e31312608` 在事件 30 返回 `recorded_action_mismatch`。事件的 expected/actual 原生选牌 payload 完全相同；31 个候选的身份、顺序、升级和选牌上下文也相同，只有以下三张牌的状态文本多出新版 `cost-state` 后缀：

| 候选索引 / NativeId | 卡牌 | 当前费用 | 新版补充的费用状态 |
|---|---|---|---|
| 2 / 37 | `MODDED` | 1 | `0:1[1:2:0:False,]/stars=-1:0[]` |
| 6 / 5 | `MOMENTUM_STRIKE` | 0 | `1:1[0:1:0:False,]/stars=-1:0[]` |
| 17 / 40 | `FRANTIC_ESCAPE` | 2 | `1:1[1:2:0:False,]/stars=-1:0[]` |

提交 `e6dc57ae` 在 `CardChoiceSupport` 和 `ContinuationStamp` 中加入费用层身份，记录修正类型、持续时间与顺序，避免当前费用相同但未来费用不同的牌被视为相同。这解释了旧包在该选牌点的文本不相等；不是此事件选错了牌的证据。`MODDED` 是原版卡牌模型 ID，不能仅凭名称判定第三方内容。

保留严格比较及原始包，未剥离费用层、修改事件或绕过校验。旧录制缺失的信息不能据此补造；之后是否存在模拟偏差仍未验证。人工在 T6 打出 Subroutine、Overclock、Hologram，再回收并打出 Frantic Escape，是后续排查沙坑生存保路的线索，不是已确认根因。

## 搜索基线与限制

诊断仅覆盖原始 `combat_start`，保留原包 Beam、节点、候选容量、药水、成长与遗物政策，显式修改 profile 时间预算并设置 `fixedBudget=true`。profile 时间是成员预算，不能当作整个协调器墙钟时间；短搜不能用于宣称同原报告预算的改善。

| 主题 | 本轮诊断 | 结果 |
|---|---|---|
| O001 | profile 5,000 ms；run `7d7fdcf4e11744409ef680327f183d98` | 协调器 18,616.64 ms；展开 58,352、转移 434,195；`onlyDeathRoutes=true`，最终玩家 HP 0、敌方 HP 79。`projectedBattleHpLost=6` 不能解读为损血 6 的获胜路线。 |
| O003 | profile 10,000 ms；run `cd349ecd871e491bae73b04234907af3` | 请求 120 s 超时，无完成结果或可归因进度快照；记录未验证，继续其他样例，不提高请求超时。 |
| O004 | profile 5,000 ms；run `fce35b90c7c547b09650443b06245eff` | 协调器 5,170.59 ms；搜索 8 回合，最终玩家 HP 48、敌方 HP 148、无药水。`onlyDeathRoutes=false`、预测损血 3，但 `combatEndedTurn=null`，只能称存活的未完成路线，不能与完整胜利的损血 5 比较。 |
| O005 | profile 5,000 ms；run `53af01da51bc487b9bb0c94d470afd2e` | 协调器 3,691.69 ms；第 7 回合预测获胜，最终玩家 HP 18、敌方 HP 0、损血 22、无药水。不与原报告 600,000 ms 的损血 10 宣称同预算差距。 |

O005 从原始开战根使用相同诊断政策执行 `DeploySolver`，run `dad1abb4070145fdad350ae71a609595`，44.15 s，状态 `deployment_completed` / 请求 Passed。实机战斗 outcome 为初始 HP 38、治疗 2、战中最终 HP 18、损血 22、自伤 20、敌方 HP 0、无药水，实际存活。终局断言另记录 `NativeOutcome:combatEnded=True:turn=8:hp=24:maxHp=89`；不能把终局 HP 24 和战斗 outcome HP 18 混用来算战损。执行采用 Instant/0 秒，实例删除成功。

执行断言 `UnexpectedReplans:0` 只统计 `StateMismatch` 与 `DeploymentDrift`；导出的 `unexpectedReplans=1` 还加上 `ContinuationMissing` 与 `PlanExhausted`，当前结果未细分后两项，不能宣称全部类型重算为零。原生录制导入路径仍将 `comparisonScope` 标为 `checkpoint`，批次工具没有据此自动产出人工优劣结论；完整实机胜负依据是本次 `actualOutcome`，不是该范围标签。上述记录口径问题留待测试入口修正，不通过手改结果掩盖。

O005 是本轮取得完整原生获胜基线的主题，可优先研究自伤换能量的兑现及终局 HP 排序。旧包玩家在 T1 使用 Dark Embrace、Burning Pact、Armaments、Bloodletting 和 Fiend Fire 等牌，T2 检查点已原生恢复；其后损血 10 的记录仍是预测，尚未实机验证。自伤占本轮战损 20/22 是排查线索，不足以断言 Bloodletting、保路或排序存在错误。

O001 首次诊断覆盖文件包含空的 `brightestFlameMaxHpLossLimit`，被 `invalid_policy_override` 拒绝，搜索并未执行；不能将批次等待超时计入搜索性能。修正为只覆盖 `profile` 与 `fixedBudget` 后才取得上述基线。

目前没有可提交的搜索优化结论。有效缺口必须继续证明同根、同政策、同预算、同一比较区间的合法实际存活路线，并扣除额外药水成本；目标和受影响哨兵验收完成后才报告修复。
