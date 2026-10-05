# Q015 路线质量

[Issue #222](https://github.com/Torch1230/CombatSolver/issues/222)，O066～O070 同一批次。贡献分支从主线 `9a4489d8`（manifest 0.50.0）开始；PR #213 已被主线合入。本记录不代替全批验收，不关闭 Issue。原始任务预测见 [发布材料](../archive/community/2026-10-05-worldlines/Q015.md)。

## 状态（2026-10-06）

| 条目 | 当前证据 | 状态 |
| --- | --- | --- |
| O068 海洋混混／储君 | 同开战根，基线 7／0药／T4；候选自主搜索及原生执行 3／0药／T4、零意外重算 | 已追平人工合法路线；独立哨兵预测保持 |
| O066 异蛙寄生虫／储君 | 保存后缀原生执行 3／1异鱼之油／T9；开战短搜基线36／0／T12，攻击续路候选17／0／T9 | 正在核对真正 T4 比较根；未追平 |
| O067 感染棱柱／静默猎手 | 材料预检通过；发布预测 12→7，均 1 药 | 当前版本同根验证待做 |
| O069 灵魂枢纽／亡灵契约师 | 材料预检通过；发布预测 46→32，多 1 药 | 当前版本同根及药水成本验证待做 |
| O070 永世沙漏／故障机器人 | 材料预检通过；发布预测 25→20，均无药 | 当前版本同根验证待做 |

## O068：候选产生，但破盾续路被淘汰

代表报告 `1b49315484e242cf8604815953f82a6d`，session `37c2b7a8552141f1aeae8735dc84fc36`，原版 0.111.0、A10、SEAPUNK_WEAK。原报告 8→3 的比较发生在 T2→T3，不直接算作本轮整场收益。本轮从开战 60/75 HP 恢复；开战和首次可操作点的完整 ContinuationStamp、原生二进制状态通过。辅助 Mod 清单存在差异，状态对账通过不代表所有 Mod 完全相同。

先从原包 `:7` 原生部署保存的获胜后缀：实际 3 战损、0 药、T4，搜索 0 次、重算 0 次。再将原记录前缀和获胜后缀合成 15 动作测试见证，逐步核对增量／完整回放。该派生包只存 `.local/`，原 ZIP 不改；它不是原包保存的完整开战预测，也不是自主发现证据。

只读整池诊断共 2791 事件、零丢失。窄成员宽度 36 的局部边界中有 80 个原始候选；倒数第二步原始排名 37，未进入最终保留集。必保代表置换后，原本较靠前的攻击候选也落选。防守估值偏好已经出防御的分支，而 T4 第一击只是消耗敌方格挡，敌人 HP 仍为 4，第二击才取胜。

生产改动只有一个因素：既有按药水数量分组的保路入口，在原名额内保留一个刚减少敌方 HP＋格挡、仍可合法出攻击的非终局候选。合法攻击数量从现有手牌合法性循环冻结，保路不读模拟器；它是启发式提示，不是可达击杀证明，不进入指纹或 ContinuationStamp。沿用既有进攻代表比较器，不改终局政策，不按卡名或遭遇特化。

## O068 验证与失败试验

所有正常样本同一原包 `start`、原 Beam 90／250000 节点上限／DOP4，固定 10000 ms，保留 Smart、成长、遗物、奖励预测及组合政策。没有使用原报告 180000 ms 的档位预算。请求上限 120 s，串行隔离运行并清理实例。

| 实验 | 正常预测：战损／药／回合 | 总展开／转移 | 正常总搜索 ms／分配 B | 结论 |
| --- | --- | --- | --- | --- |
| 主线 9a4489d8 基线 | 7／0／T4 | 7010／18362 | 2549.4631／620607688 | 当前独立基线 |
| 仅 HP 伤害＋剩余手牌价值 | 7／0／T4 | 7649／19887 | 2926.3534／663000744 | 失败，替换 |
| 保路时读取模拟器内合法攻击 | 7／0／T4 | 7007／18360 | 2670.1695／615626176 | 失败，撤去该读取 |
| 冻结攻击数，但只算 HP 进展 | 7／0／T4 | 7007／18360 | 2796.6852／611821056 | 失败，被格挡进展条件替换 |
| HP＋格挡进展及冻结合法攻击 | **3／0／T4** | 1858／4935 | 1025.6851／174835576 | 原生完整执行 57/75 HP，零重算 |

基线 runId `2130baa7463c49028dbfc6a3084a0877`；最终自主部署 `488511f0cd7348c5afdbdb3f60920c3b`。每项时间均为正常搜索，不含只读观察器或逐转移验证；本轮每版本一个独立进程样本，不能外推普遍提速或可见 Steam 帧时间。冻结基线 DLL 仅带通用 Testing 整池观察改动，其生产搜索与 9a4489d8 相同。

## 固定独立哨兵

沿用 Q002 的 SILENT／BYRDONIS_ELITE 生成根，冻结种子、牌组、升级、遗物及药水；输入见下方维护的 JSON。两次独立进程使用相同 Custom／Beam60／120000节点／固定30000 ms／DOP2／Smart／普通GC／无详细日志，原生开局均已捕获并对账。

| 版本 | 战损／药／结束回合 | 总展开／转移／选择分支 | 搜索 ms／分配 B | GC 总／最大暂停 ms |
| --- | --- | --- | --- | --- |
| 9a4489d8 生产基线 | 5／1／T4 | 17480／68036／12470 | 8819.4325／2425680760 | 206.968／25.729 |
| 攻击续路候选 | 5／1／T4 | 17587／67878／12297 | 8988.7631／2431347336 | 129.782／18.132 |

runId 分别为 `61f63b7118a04bb79db8074288fefc61`、`37331b22dcf24a90b00b33ed3d4fb7f8`，均 Passed。时间增加约 1.9%，分配增加约 0.23%，单对样本未见明显增加，但不能据此证明统计上的普遍性能不退化。两者在首个结果后停止，本轮未进行该哨兵整场原生部署；O068 的完整原生部署已通过。没有原档位长搜或可见 Steam 卡顿结论。

## 可重跑入口

先从 [Q015 官方资料](https://github.com/Torch1230/CombatSolver/releases/download/community-tasks-2026-10-02/Q015.zip) 取得 O068 子包；`<O068.zip>` 表示原始子包，不是临时派生见证。按本机情况补充游戏／Ritsu 路径。以下命令不表示其他四包已运行。

```powershell
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O068-DEPLOY -CheckpointArchivePath <O068.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o068 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-DAMAGING-CONTINUATION-SENTINEL -GeneratedScenarioPath coverage/fixtures/search/damaging-continuation-sentinel.json -PerformancePresetForTest Custom -SearchBeamWidthForTest 60 -SearchMaxExpandedNodesForTest 120000 -SearchBudgetOverrideMilliseconds 30000 -FixedSearchBudget -SearchMaxDegreeOfParallelismForTest 2 -EnableNoGcRegionForTest 0 -EnableDetailedDiagnosticLogsForTest 0 -PotionPolicyForTest Smart -RuntimeProfile default -ExpectedInitialProjectedBattleHpLostAtMost 5 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -StopAfterInitialSolverResultAssertion -EvidenceDirectory .local/validation/q015/sentinel -TimeoutSeconds 120 -CleanupInstanceOnExit
```

Linux 使用 `tools/testing/run-unattended-test.sh` 的对应 kebab-case 参数。两个维护的 JSON 输入不含个人路径、原始存档或玩家动作。
