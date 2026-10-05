# Q015 路线质量

[Issue #222](https://github.com/Torch1230/CombatSolver/issues/222)，O066～O070 同一批次。贡献分支从主线 `9a4489d8`（manifest 0.50.0）开始；PR #213 已被主线合入。本记录不代替全批验收，不关闭 Issue。原始任务预测见 [发布材料](../archive/community/2026-10-05-worldlines/Q015.md)。

## 状态（2026-10-06）

| 条目 | 当前证据 | 状态 |
| --- | --- | --- |
| O068 海洋混混／储君 | 同开战根，基线 7／0药／T4；候选自主搜索及原生执行 3／0药／T4、零意外重算 | 已追平人工合法路线；独立哨兵预测保持 |
| O066 异蛙寄生虫／储君 | 原 T4 比较根：主线3／1／T9，候选自主搜索及原生部署3／1异鱼之油／T9、零重算 | 原比较根已由主线解决；开战10秒短搜仍有差距 |
| O067 感染棱柱／静默猎手 | 保存后缀原生7／1敏捷药／T7；同开战根主线12／1，攻击续路候选9／1 | 约40分钟未追平，跳过；仍差2，未自主部署 |
| O069 灵魂枢纽／亡灵契约师 | 保存后缀原生32／1格挡药／T7；同开战根主线62／0，候选自主搜索和原生40／0／T13、零重算 | 折算优于人工1；裸战损未追平32 |
| O070 永世沙漏／故障机器人 | 同开战根主线36／0／T12；候选自主搜索和原生20／0／T11、零重算 | 战损追平人工20／0；比人工晚1回合 |

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

## O066：原比较根已在主线追平

报告 `488b952ad66f4c5dbed59264ae7361f2`，session `03b561868c6d4ceea599655def8da459`。原包 `:9`／cursor19／T4 的保存预测严格回放21动作并原生部署 Passed `3fc3be099c5d4db09cbb2de7a93a2896`：3战损、1异鱼之油、T9，原生38/75 HP，无额外搜索和重算。

同一 T4 根正常协调器对照，原 Medium profile60／120000节点／DOP4，仅固定为10000ms，Smart、原药水指令、零成长额度、奖励预测关闭均保持：

| 源码 | 预测战损／药／结束回合 | 展开／转移 | 搜索 ms／分配 B | 实际执行 |
| --- | --- | --- | --- | --- |
| 9a4489d8 主线 | 3／1／T9 | 6634／22903 | 4240.3057／1050745512 | 本次首个结果后停止；保存同根3损后缀已原生验证 |
| 攻击续路候选 | 3／1／T9 | 6610／22888 | 3911.7524／1049674136 | 38/75 HP，零计划外重算 |

正常主线 runId `fb08654c2348401ab21d83fcd8c27660`；候选自主部署 `feccc1a2cb774c3eb19e5038469a65ec`。本项按原报告的实际比较根已追平，不把该已有结果计为本次修复收益。

另从开战 `start` 比较固定10秒：主线36损／0药／T12（`42f90c670ce94611bf0393bdd7d32f53`，20591展开／79619转移／9905.6302ms／3697811576B）；候选17／0／T9（`de66e079e272430d867fd46a920391b5`，26249／96098／9921.7557ms／4394040680B）。候选因预期1药的断言 Failed，尚未原生部署，也未追平3损＋1药的折算12损目标。原日志还混有 startTurn1 的37动作旧计划与T4复用结果，不能拿旧17预测当成本轮开战短搜基线或混算区间；原档位120秒未验证，不扩大短搜预算继续调参。

## O067：自主路线仍差2

报告 `cc243a922beb4965995c911f8ff9fb60`，session `3e88aff1627f423e8a26c4fb2fd9b652`。原包 `:5`／cursor36／T6 的6动作保存后缀已逐步核对增量／完整回放并原生执行（`42820d9838434ca9ab71b2d971cb2c71`）：整场7战损，原生49/77 HP，T7获胜。敏捷药水在录制的T1已经喝过，后缀0药不等于整场0药；整场共1瓶、零额外搜索和重算。

正式开战比较采用 `:3`／cursor0 的 Force 敏捷药指令，Swift 保持 Smart，使用[维护的政策输入](../../coverage/fixtures/search/damaging-continuation-forced-dexterity-policy.json)；保留新颖性搜索开启、宽度组合开启、原成长额度和遗物目标，关闭奖励预测和提前探索，DOP4。14个非预算政策字段与原比较政策逐项一致。原 profile90／250000节点仅固定为10000ms；总搜索统计约26.47秒，包含既有能力组合成员，不能把 profile10000ms 宣称为整个请求10秒。

| 源码 | 正常预测：战损／整场药／回合 | 展开／转移 | 总搜索 ms／分配 B | 状态 |
| --- | --- | --- | --- | --- |
| 9a4489d8 主线 | 12／1／T7 | 68662／342674 | 26471.8835／15064986112 | SearchOnly Passed |
| 攻击续路候选 | 9／1／T7 | 70376／351837 | 26474.4649／15416907976 | 预期≤7断言 Failed；未原生部署 |

正常 runId `df5ba6553ad34f7ebe91c97000b26e39`、`aaf1ca3ec06d4a0fb8b8efb20ce338ff`。此前对新颖性开关不一致的判断经原包原文核对已撤销，没有因该误判重复正常搜索。

派生见证保留原始状态键、费用和选牌，只存 `.local/`：候选9损路线仅提前T2能力，严格结果仍9（30动作、T7、0展开，7损断言失败）；原保存12损路线仅把敏捷药从T4移至T1，严格结果8（29动作、T7、0展开，同样未达7）。按完整原生动作顺序及保存后缀重建的第三条见证严格回放31动作，7损、T7，增量／完整一致，303条观察事件无丢失（`37e46c7aae4c4c5ebc50e15e775e8823`）。派生见证不是自主发现或原生部署证据，不注入生产候选或评分。通用失败信息补充实际胜负、回合、战损、动作数和展开数，避免仅看到笼统的结局不匹配。

只读观察中，人工第17步（T4抽牌后防御）的等价状态进入候选和剪枝输入，未进入最终保留／展开；第18步未出现。此前部分具体动作被转置合并，后续等价状态仍存活，不能把首次具体动作丢失直接当作整条路线消失。尚无第17步完整候选池排序证据，不能断言某一比较器为根因。

两次单因素尝试均未追平，已撤回：保留增加格挡、减少预计受击且仍有合法攻击的代表（`603a0f4283fc438aae5acc7fb504c3bc`，9／1／T7，65119展开／329536转移，29195.6575ms／14501949056B）；将合法攻击条件替换为现有可达手牌价值（`d00f854b7ea44cd1bf6322cc321c18fb`，9／1／T7，66524／333532，26367.6819ms／14653435712B）。两者均因≤7断言 Failed，未自主原生部署。约40分钟内未追平，保留差2的待处理项，继续下一包；最终生产仍只有O068已验证的攻击续路因素。

## O069：省药后的整场成本

报告 `c1185416b35446cfaa685e41f0876e50`，session `44abdd695aca454dab8065a93b82e0f0`。原包 `:13`／cursor33／T5 保存后缀严格回放13动作并原生部署 Passed（`45061c6bffb4483d954d8406098b72c1`）：整场32战损、1格挡药、T7，实际41/84 HP，零额外搜索和重算。与原无药预测46相比，省14血但多1药，净优势5。

本轮自主比较从 `start`，原High profile90／250000节点／DOP8，仅固定为10000ms；原Smart、零成长、奖励预测关闭、新颖性关闭、宽度组合开启均保持，非预算政策与原`:3`比较政策无差异。原比较`:3`发生在4条开局动作之后，不把原46预测当作本轮开战短搜基线。

| 源码 | 正常预测：战损／药／回合 | 展开／转移 | 总搜索 ms／分配 B | 状态 |
| --- | --- | --- | --- | --- |
| 9a4489d8 主线 | 62／0／T11 | 26770／123355 | 9917.1062／5398837040 | SearchOnly Passed |
| 攻击续路候选首次搜索 | 40／0／T13 | 25342／130359 | 9920.1305／5551688736 | 仅按≤32战损断言 Failed，未部署；按药水成本应比较41／0 |
| 攻击续路候选原生验收 | 40／0／T13 | 23052／118877 | 9956.2275／5096935872 | Passed；实际33/84HP、零重算 |

正常 runId `d9ae076c078c4bdcb6ed6f4ae9e03263`、`4a94a13cf8d14970aefcd0d62b292d01`；自主部署 `65e41f3c883847c0a2f245161b0c8534`。人工32＋1×9＝41，候选40＋0×9＝40，折算少1；不宣称裸战损追平32。原生验收以≤41战损、零药、完胜条件完成此前未执行的部署层，Instant／0秒、零计划外重算。无新增生产因素，仍是O068攻击续路改动。时间切片导致两次搜索工作量不同，不能宣称确定性工作量或普遍性能收益。原档位180秒与可见Steam帧时间未验证。

## O070：整场战损追平，晚一回合

报告 `acdd30c74a154131abb6c01e68340674`，session `745949047c544bb28d0edb785f42e0b5`。原包`:8`／cursor18／T3保存后缀严格回放63动作并原生执行 Passed（`6754c133daee4df6a2b701f90e27ffaa`）：整场20战损、零药、T10，实际54/75HP，零额外搜索和重算。战损含13自伤，不能仅比较怪物受击。

同开战根对照使用原Medium profile60／120000节点／DOP8，仅固定为10000ms；原Smart、零成长、新颖性关闭、宽度组合及首领HP政策保持。

| 源码 | 正常预测：战损／药／回合 | 展开／转移 | 总搜索 ms／分配 B | 状态 |
| --- | --- | --- | --- | --- |
| 9a4489d8 主线 | 36／0／T12 | 93538／378049 | 27870.08／22895372144 | SearchOnly Passed |
| 攻击续路候选 | 20／0／T11 | 80957／328267 | 26336.7511／19381488128 | 自主原生执行54/75HP，零重算 |

正常 runId `5fdc455e4b16491ebdddbc190d13fb23`、`7357a242a1f54baeb26b38f3aed590c7`。没有新增生产因素；O068攻击续路改动使本根少16战损，药水数量不变，整场战损追平人工20，但比人工T10晚一回合。总搜索包含既有组合成员，不能宣称请求总预算10秒；每版本单样本，不外推普遍性能收益。原档位120秒和可见Steam帧时间未验证。

## 可重跑入口

### 命令

先从 [Q015 官方资料](https://github.com/Torch1230/CombatSolver/releases/download/community-tasks-2026-10-02/Q015.zip) 取得对应子包；`<O068.zip>` 等表示原始子包，不是临时派生见证。按本机情况补充游戏／Ritsu 路径。O067 的目标断言仍失败；其他命令对应已记录的实际验证范围。

```powershell
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O068-DEPLOY -CheckpointArchivePath <O068.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o068 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O066-T4-DEPLOY -CheckpointArchivePath <O066.zip> -CheckpointSelector 03b561868c6d4ceea599655def8da459:9 -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-medium-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o066 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O067-OPEN-TARGET -CheckpointArchivePath <O067.zip> -CheckpointSelector start -ReplayMode SearchOnly -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-forced-dexterity-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 7 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o067 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O069-POTION-COST-DEPLOY -CheckpointArchivePath <O069.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 41 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o069 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O070-DEPLOY -CheckpointArchivePath <O070.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-medium-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 20 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o070 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-DAMAGING-CONTINUATION-SENTINEL -GeneratedScenarioPath coverage/fixtures/search/damaging-continuation-sentinel.json -PerformancePresetForTest Custom -SearchBeamWidthForTest 60 -SearchMaxExpandedNodesForTest 120000 -SearchBudgetOverrideMilliseconds 30000 -FixedSearchBudget -SearchMaxDegreeOfParallelismForTest 2 -EnableNoGcRegionForTest 0 -EnableDetailedDiagnosticLogsForTest 0 -PotionPolicyForTest Smart -RuntimeProfile default -ExpectedInitialProjectedBattleHpLostAtMost 5 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -StopAfterInitialSolverResultAssertion -EvidenceDirectory .local/validation/q015/sentinel -TimeoutSeconds 120 -CleanupInstanceOnExit
```

Linux 使用 `tools/testing/run-unattended-test.sh` 的对应 kebab-case 参数。维护的 JSON 输入不含个人路径、原始存档或玩家动作。
