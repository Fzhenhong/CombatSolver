# Q015 路线质量

[Issue #222](https://github.com/Torch1230/CombatSolver/issues/222)，O066～O070 同一批次。贡献分支从主线 `9a4489d8`（manifest 0.50.0）开始；PR #213 已被主线合入。本记录保存本地验收证据，不自动关闭 Issue。原始任务预测见 [发布材料](../archive/community/2026-10-05-worldlines/Q015.md)。

## 状态（2026-10-06）

| 条目 | 当前证据 | 状态 |
| --- | --- | --- |
| O068 海洋混混／储君 | 同开战根主线7／0；最终自主原生3／0／T4、零重算 | 折算追平；详细比较限制见下文 |
| O066 异蛙寄生虫／储君 | 同开战根主线36／0／T12；最终自主原生0／1异鱼之油／T6，实际41/75HP、零重算 | 折算9优于已原生验证的人工12；原T4根兼容验证通过 |
| O067 感染棱柱／静默猎手 | 同开战根主线12／1；最终自主原生7／1／T7、零重算 | 折算追平；详细比较限制见下文 |
| O069 灵魂枢纽／亡灵契约师 | 同开战根主线62／0；最终自主原生40／0／T13、零重算 | 折算少1；详细比较限制见下文 |
| O070 永世沙漏／故障机器人 | 同开战根主线36／0；最终自主原生20／0／T11、零重算 | 折算追平；详细比较限制见下文 |

## 机制与比较口径

- O068：既有药水数量分组名额内保留刚减少敌方HP＋格挡、仍有合法攻击的续路；合法攻击数沿既有手牌遍历冻结。
- O067：仅在Discard／DiscardAndDraw上下文保留预计安全且直接／延迟进展较好的续路；Control沿原8个结束回合探测名额保留一个延迟伤害可能收尾的代表，由实际回放决定胜负。
- O066：冻结AliveEnemyMask中旧敌退出、新敌入场的变化进入既有RevivalWindow通道。O069与O070验证相邻资源、选牌及长战斗路径。
- 以上均为中途保留启发式，不改战斗语义、状态等价、终局排序或预算，不增加Beam／通道／探测名额，不按报告、卡牌或遭遇特化。通用Testing观察步数参数同时维护PowerShell和Bash入口。

| 条目 | 同开战根主线战损／药／回合 | 人工完整参照战损／药／回合 | 最终折算成本（每药9HP） |
| --- | --- | --- | --- |
| O066 | 36／0／T12 | 3／1／T9 | 9，较人工12少3 |
| O067 | 12／1／T7 | 7／1／T7 | 16，与人工一致 |
| O068 | 7／0／T4 | 3／0／T4 | 3，与人工一致 |
| O069 | 62／0／T11 | 32／1／T7 | 40，较人工41少1；裸战损多8、晚6回合 |
| O070 | 36／0／T12 | 20／0／T10 | 20，与人工一致；晚1回合 |

原Issue数字为旧版本且部分来自不同中途比较根，不能当成本轮同开战根基线。开战恢复核对完整ContinuationStamp、原生二进制状态、牌序与RNG；保留原药水／成长／遗物／奖励／组合政策，仅profile固定10000ms。原有组合成员可使请求总搜索超过10秒；请求总超时120秒。普通性能样本未开启路径观察器或逐转移回放。分阶段原生人工参照、增量／完整等价、政策字段核对和失败试验保留在[排查历史](../archive/strategy/q015-investigation-20261006.md)。

## 固定独立哨兵

固定输入[BYRDONIS_ELITE／SILENT](../../coverage/fixtures/search/damaging-continuation-sentinel.json)来自已正确执行的Q002生成根，覆盖相邻攻击、弃牌、药水路径。种子、牌组、RNG、药水与成长政策固定；独立进程、Custom／Beam60／120000节点／固定30000ms／DOP2／Smart／普通GC／无详细日志。两端原生开局已捕获并对账，验证范围是首个完整预测，不是整场原生执行。

| 版本／runId | 战损／药／回合 | 展开／转移 | 总搜索ms／分配B |
| --- | --- | --- | --- |
| 主线9a4489d8／`61f63b7118a04bb79db8074288fefc61` | 5／1／T4 | 17480／68036 | 8819.4325／2425680760 |
| 最终Q015／`f1ecad1682dd49b397489a5f4214fad2` | 5／1／T4 | 17589／67841 | 7475.2308／2435604896 |

两端Passed、预测质量未退化，最终耗时未明显增加；分配增加约0.41%。每版仅一个独立正常样本，未建立统计波动范围，不能证明普遍性能不退化或提速；详细中间样本与GC记录见排查历史。没有Linux运行、原档位长搜、完整哨兵原生部署或可见Steam帧时间结论。

## 最终版本：死亡增援窗口与同源验收

O066在约06:04UTC开始额外开战排查，06:31UTC取得自主原生达标结果。失败的累计伤害同分试验已撤回；仅新增一个独立因素：冻结的AliveEnemyMask显示旧敌退出且新敌入场时，沿用既有RevivalWindow通道。原先仅判断复活数量或总HP变化，会漏掉击杀旧敌后增援使总HP上升的阶段。与此前7／1版本逐项核对，完整开战根ContinuationStamp及全部executedPolicy字段一致，政策差异为{}。识别不读模拟器，不新增通道名额，不预计未知增援HP，不改实际死亡／召唤语义、状态等价、预算或终局排序，不按报告、角色、卡名或遭遇特化。

同一最终Release程序集（SHA256 `7AB45F25ADCF5C5A02B4160A57E8EBCBB0E16252C3EDB033636ACBCFFF143CB7`）完成下表验收。五个主题均已从原始开战根自主搜索并原生执行完胜，零计划外重算；另保留O066原T4根兼容结果。固定哨兵仍为首结果验证，不宣称完整原生部署。

| 项目 | runId | 战损／药／回合 | 实际HP | 展开／转移 | 搜索ms／分配B |
| --- | --- | --- | --- | --- | --- |
| O066 开战 | `7e3d9c9ff0df468fada865f1aa33e5b4` | 0／1／T6 | 41/75 | 28791／103540 | 8387.9231／4767277264 |
| O066 T4 | `47029c7049664b59b8e81a50b6a945cb` | 3／1／T9 | 38/75 | 6610／22888 | 3591.7788／1054352448 |
| O067 开战 | `de73b24602e146858527361186b28789` | 7／1／T7 | 49/77 | 68008／342121 | 21164.1224／15145573792 |
| O068 开战 | `bda4c827af34449e94a680d16a4acc02` | 3／0／T4 | 57/75 | 1858／4935 | 869.8396／175002488 |
| O069 开战 | `08b7b882dcb64ed48640f4e5964cf4e5` | 40／0／T13 | 33/84 | 40584／201764 | 9942.3624／8587474376 |
| O070 开战 | `e8ebefd702364afabdf763bb7b3c5dd2` | 20／0／T11 | 54/75 | 81957／333181 | 19042.4303／19742803040 |
| 独立哨兵 | `f1ecad1682dd49b397489a5f4214fad2` | 5／1／T4 | 首结果；未完整部署 | 17589／67841 | 7475.2308／2435604896 |

O066开战自主路线28动作，T1使用1瓶异鱼之油，实际初始／最终均41/75HP，损血／回血／自伤／未归因损血均0。折算0＋9＝9，优于已原生验证的人工3＋9＝12，少3；旧7／1路线折算16的差距已解决。其他主题按药水机会成本比较；O070的晚一回合限制保留。全部使用原政策的固定短搜，没有原档位120／180秒或可见Steam帧时间结论。每版本单样本及时间切片工作量不构成普遍性能收益证明。

## 可重跑入口

### 命令

先从 [Q015 官方资料](https://github.com/Torch1230/CombatSolver/releases/download/community-tasks-2026-10-02/Q015.zip) 取得对应子包；`<O068.zip>` 等表示原始子包，不是临时派生见证。按本机情况补充游戏／Ritsu 路径。各命令对应下方已记录的实际验证范围。

```powershell
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O066-START-DEPLOY -CheckpointArchivePath <O066.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-medium-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o066-start -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O068-DEPLOY -CheckpointArchivePath <O068.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o068 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O066-T4-DEPLOY -CheckpointArchivePath <O066.zip> -CheckpointSelector 03b561868c6d4ceea599655def8da459:9 -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-medium-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 3 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o066 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O067-DEPLOY -CheckpointArchivePath <O067.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-forced-dexterity-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 7 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o067 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O069-POTION-COST-DEPLOY -CheckpointArchivePath <O069.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 41 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o069 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-O070-DEPLOY -CheckpointArchivePath <O070.zip> -CheckpointSelector start -ReplayMode DeploySolver -ReplayPolicyOverridePath coverage/fixtures/search/damaging-continuation-medium-replay-policy.json -ExpectedInitialProjectedBattleHpLostAtMost 20 -ExpectedInitialPotionCount 0 -ExpectedInitialFinalEnemyHpAtMost 0 -EvidenceDirectory .local/validation/q015/o070 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools/testing/run-unattended-test.ps1 -ScenarioId Q015-DAMAGING-CONTINUATION-SENTINEL -GeneratedScenarioPath coverage/fixtures/search/damaging-continuation-sentinel.json -PerformancePresetForTest Custom -SearchBeamWidthForTest 60 -SearchMaxExpandedNodesForTest 120000 -SearchBudgetOverrideMilliseconds 30000 -FixedSearchBudget -SearchMaxDegreeOfParallelismForTest 2 -EnableNoGcRegionForTest 0 -EnableDetailedDiagnosticLogsForTest 0 -PotionPolicyForTest Smart -RuntimeProfile default -ExpectedInitialProjectedBattleHpLostAtMost 5 -ExpectedInitialPotionCount 1 -ExpectedInitialFinalEnemyHpAtMost 0 -StopAfterInitialSolverResultAssertion -EvidenceDirectory .local/validation/q015/sentinel -TimeoutSeconds 120 -CleanupInstanceOnExit
```

Linux 使用 `tools/testing/run-unattended-test.sh` 的对应 kebab-case 参数。维护的 JSON 输入不含个人路径、原始存档或玩家动作。
