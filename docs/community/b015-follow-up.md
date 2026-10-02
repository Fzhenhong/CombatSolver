# B015 后续：统计饱和隔离与其余主题的证据边界

关联 Refs #174，延续[阶段一](b015-stage-one.md)。源码基线仍为 `88298ae5`；只在 `fix/batch-b015` 本地工作。初始调查时认领获得维护者积极回复、Assignee仍空；收尾刷新[任务页](https://github.com/Torch1230/CombatSolver/issues/174)确认已正式指派 `s1f102500012`（issue updated_at=2026-10-02T10:49:17Z）。没有推送、评论、PR、原游戏目录部署或发布。 上游PR #188随后合入 `fbe469b77e55f1269a563725df001023597cd9a0`，父提交为本次冻结基线；该性能合并涉及根捕获、Hook、搜索及测试入口，但未直接改T018统计文件。本轮未变基，既有结论仍只属于冻结基线；发布前整合另做影响范围核对。

## T018：健康消费者饱和也不再中断战斗

原包首先发生的是统计存储构造读取 JSON 时的 NUL/offset=0 错误，随后消费者退出、队列填满。包内没有损坏文件本体或文件名；构造器会读 `.run.json` 和 `.history.json`，因此不能断定损坏来自哪种文件，也没有证据解释写坏来源。四 NUL 字节 `.run.json` 是代表性最小输入，并非声称还原了历史文件。

在消费者已退出隔离之外，本次补上仍正常运行但暂时消费不过来的路径：

- 容量保持256；满队列的周期 sync 提示可由下次30秒周期重试，未丢弃业务记录。
- 业务信号被拒绝时停止本次统计，明确记录 reason、runId、拒绝信号、待处理数量、原错误及持久化确认状态。问题包新增 `runStatisticsFailure`；有效快照失效，后续设置不能重新启用上传。
- 健康消费者继续排空已接受事件。主线程不抢读其队列；只有 worker 完成后才清理残留。停止状态不阻止观察随后发生的排空异常。
- worker 为受影响跑局写 `.incomplete.json` 标记，再从最新记录降级为 partial，保留已累计的事件与已收到的结算。被拒绝的 end 未被当成已接受事件；保持 pending，交由原生历史恢复结算。
- 存储重开、后续 Save 与原生结算均保留 partial。若标记已经落盘而旧 full 文件和 `.sent` 尚在，重开会持久化修正、撤销旧收据并重新排入待上传；纠正后的收据不会每次重启重复失效。

这不是损坏文件修复，也不声称丢失事件已恢复。标记由后台写入：若进程在首次标记落盘前退出，或存储本身不可写，完整性持久化仍无法保证，失败详情保留未确认状态。活动进程不再提供有效统计或继续上传。已经发出的网络请求也不能被本地取消追溯撤回。

独立审查找出并修正了 `_incompleteRun` 早于失败对象发布的竞态，以及旧 `.sent` 阻止 partial 纠正的重启边界。测试使用屏障控制消费者，不靠 sleep 推测队列时序。

## T017：已定位第三方字段生产者

原包明确加载 TheHeroExpansion 1.0.0.0。公开提交 `0726b34cb2329bc5b1484b7ac7d110a981603526` 的 [SovereignBladePatch](https://github.com/BlackHero20/TheHeroExpansionMod/blob/0726b34cb2329bc5b1484b7ac7d110a981603526/TheHeroExpansionCode/Patches/SovereignBladePatch.cs) 对原版 CanonicalVars getter 无条件追加 `StringVar("SeekingEdgeSuffix")`，同补丁还增加两个变量。[EdgeOfDestinyPower](https://github.com/BlackHero20/TheHeroExpansionMod/blob/0726b34cb2329bc5b1484b7ac7d110a981603526/TheHeroExpansionCode/Powers/EdgeOfDestinyPower.cs) 写入显示后缀，同时含有额外攻击等战斗行为。

因此不应把该字段按原版显示字段放行：忽略字符串不能实现该 Mod 的战斗语义。当前保留未知字段拒绝。`replay/checkpoint.json.build` 已记录该 Mod 的 MVID `bd5b6a43-df85-4910-9687-3a96b702c7dd`，但包中没有实际 DLL 或对应源码映射，未证明历史二进制与此提交完全一致，也未验收完整第三方适配。此项的可交付结果是纠正“原版遗漏字段”的分诊前提和明确适配范围，而不是声称已修兼容性。没有证据将字段归因于 RegentFX。

## T020：坏状态早于战斗，合法状态对照通过

原包 `pre-combat/in-memory-current_run.save` 的 `map_point_history[2][11]` 对应 TINKER_TIME：选择 Skill、Chaos；获得合法 type=2/rider=6 的牌，同时记录移除该牌及获得 type=0 的升级牌。`players[0].deck[14]`、combat_start 和首个根已经含无效的 MAD_SCIENCE+1。该房间没有 upgraded_cards 记录。数组本身不证明精确操作顺序或调用者，但足以排除“最早可见坏状态出现在搜索 Fork”的说法。

原版0.111.0的升级只添加 Innate；保存属性恢复在升级前写回类型与附加效果。独立托管探针证明合法状态经升级、MutableClone、SavedProperties JSON往返保持 Skill/Chaos；仅按 canonical ID 重建再升级的负对照变为 None/None。此探针不是游戏语义验收。

另有真实游戏 `B015-MAD-SCIENCE` 通过：原生升级和保存恢复、根/Fork/MutablePreview、兄弟分支修改隔离、合法出牌的8格挡/1能量消耗/1生成牌，以及原生完整状态和 RNG 差分；无效 None 明确抛出原有异常。没有新增默认 Attack 或无依据字段复制补丁。

[FreeLoadout 当前公开升级入口](https://github.com/Quorafind/FreeLoadout/tree/2646b2d1f17cd2e4afe2321450f7c7c49d46fc7e) 直接调用原生 CardCmd.Upgrade，不能凭原包加载列表归罪该 Mod。要确认替换调用者，仍需历史 Mod 精确构建或 TinkerTime 房间的编辑/移除/重建操作记录。

## T016 / T019

`B015-BOUNDARIES` 已在0.111.0/Ritsu0.6.2通过两项严格原生对照：

- T016：保留原生Stock、将原敌HP设为1，第一张SHIV击杀并产生不同CombatId的替补；验证活动/已退场身份、Fork、父分支不变、对替补第二张SHIV的完整与增量回放、两步原生完整状态一致。
- T019：替补执行无伤害Boot Up，玩家1HP实际打出CrimsonMantle确认SelfDamage=1，EndTurn到T+1；预测死亡与终局戳、Fork、原生ProcessPendingLoss安全点前完整状态一致。

这两个最小边界在本轮行为源码修改前后没有差异；未改卡牌目标或Power生命周期实现。它们没有复现原包异常，不能代替原报告完整动作链、Silent/A9配置及BaseLib/其他Mod环境，也不构成“已修复”结论。T016后续已从原根核验五步前缀，剩余问题是生产生成器如何产生无效的第六步，见下文。T019的实际构建及失败调用已在后续核对中定位，见下文；不能把主线最小通过描述为修复了实验分支。

夹具校正均保留失败证据：第一次增量调用误传priorActionCount=0，状态键/续用相同但评分不一致，后改为1；第二次沿用胜利EndCombatInternal观察点，原生已正常死亡但无对应快照，120秒无结果。核对原版后改用败局ProcessPendingLoss，另加15秒局部等待；未扩大总预算或放宽状态断言。

### 构建身份更正与 T019 实验分支根因

此前遗漏了 `replay/checkpoint.json.build`，错误地写成“原包没有实际构建MVID”，并把 LINQ 异常猜成 First；现明确撤回这两项表述。该文件记录完整 informational version 和 MVID。T016/T017 对应官方0.47.2提交 `1b9109695b34f82f98b74c8915215b3b38c9d5d4`，Solver MVID `0ac802ba-4f27-4050-9c11-f342f12fbdfd`；下载官方包SHA256为 `d908542df68d3e8e0f0f294d9d081c6e6f9479c703141ca605dcc9755a19d1d1`，身份相符。

T019 实际使用[多人实验提交 f1461c7d](https://github.com/Torch1230/CombatSolver/commit/f1461c7d6dbb2145a0df7fd6743c65bc59c56c35)，Solver MVID `151cb9b9-f2d3-4f9a-b12e-a9d3517576a5`，并非官方0.47.2产物。该提交 `PersistentPowerSupport.TriggerAfterSideTurnStart` 在执行玩家回合开始 Power 后，用 `combat.Players.Last(member => simulator.State.GetCreature(member.Creature).IsAlive)` 选最后存活玩家，再判断本轮 participants 是否包含它以触发 Rampart。唯一玩家1HP被 CrimsonMantle 自伤击杀后，谓词没有匹配项，即抛出原报告 `Sequence contains no matching element`。即使没有 RampartPower，这个 Last 也会先求值。

该守卫来自多人分支 `8a5e493f`；`f1461c7d` 不是冻结main基线的祖先，主线没有这段逻辑。因而不应在main加入该守卫再修复，也不能写成main已经删除旧缺陷。已准备针对实验分支的最小补丁提案：`LastOrDefault` 加显式非空参与者判断，保留有存活玩家时的最后参与者一次触发规则；无人存活时继续原有败局安全点。原生 PendingLoss 下 GainBlock 无效果，不能通过提前更改胜负判定或吞异常处理。补丁与分支多人生存组合仍需在目标分支验收。 [补丁提案](b015-t019-experimental-branch.patch)仅供目标分支评审，未应用到main。

控制实验仅把上述历史 Last 守卫移植到冻结main，运行同一 `B015-BOUNDARIES` 输入，在披风自伤后精确复现 `Sequence contains no matching element → Enumerable.Last → TriggerAfterSideTurnStart`；该输入在未注入守卫的主线已通过严格原生差分。证据 `b015-boundaries-historical-guard-062`，两侧游戏0.111.0/Ritsu0.6.2、预算120秒。临时守卫与构建DLL已恢复；这是“重建历史条件”的因果负对照，不是实际f1461c7 DLL或原报告T5动作链复放，也不代表所附补丁完成多人语义验收。

### T016 原包恢复进展

官方ZIP通过 Preflight（材料完整性），实际在0.111.0/Ritsu0.6.3恢复 `combat_start` 后，原生二进制状态与 ContinuationStamp 均严格匹配。但整个 RestoreOnly 请求仍为 Failed：原包只有开战与导出两个不可搜索检查点，录制事件仅一个 GAMBLING_CHIP Hook、没有选牌；start目标cursor=0后收到原生自动Hook，被录制驱动按 `recorded_action_mismatch:0` 拒绝。这不等于原根状态不匹配，也不等于完整恢复或原失败前缀验收成功。证据 `t016-original-restore-063`；下一步仅按日志记录的明确选牌与失败动作前缀做确定性诊断，不延长预算盲搜。 已新增专项 `B015-T016-ORIGINAL-PREFIX`：Builder严格恢复原根并在原生SetupPlayerTurn边界捕获，Executor按日志选择及六动作逐步做完整/增量回放和原生状态/RNG差分。第一次请求 `t016-original-prefix-063` 在建局读日志时失败，因为检查点导入器不提取diagnostics；尚未执行语义步骤。夹具随后改为从原ZIP只读指定、有大小上限的日志条目。 修订请求 `t016-original-prefix-zip-063` 已完成原开战双状态、日志选牌及前五步完整/增量/原生全状态和RNG对账，六个日志父标量也一致；第六步按原日志强制重放SHIV target2，精确抛出原 `CardPlay has no target creature`。此时实际与预测原敌ID1仍有4HP，ID2不存在。该证据证明原记录动作不适用于已重建父状态，但尚未证明当前生产候选生成会自行产生该动作，不能自动重定向目标或将这一步当完整修复基线。已排除HelicalDart出牌前击杀假设（实际为出牌后临时敏捷）；继续对已核验父根做有界生产候选/兄弟分支诊断。

### T016 生产生成与并发归属检查

在已核验的五步父节点上调用生产 `PrepareCardActions`，再分别回放一个 SHIV 和一个 BACKSTAB 兄弟分支。两者均可击杀ID1并产生87HP的ID2；父节点与原生状态保持不变，前后候选目标仍为ID1。证据 `t016-production-siblings-063`；整份请求在随后强制历史无效第六步处仍为 Failed。

最后一次请求 `t016-parallel-baselib-063` 加载原报告同MVID的 BaseLib3.4.7（`e1ce449c-88a4-4ba0-bbee-eb1e33f0310b`）。从五步父节点和合法第六步之后的子节点组成两个父节点，限定2个准备任务、4个真实动作任务；通过生产 `AdmittedJobScheduler`、`ExpansionLane`、执行、发布和 `Receive` 路径检查归属。每个动作都与独立串行回放做完整状态/RNG差分，同时检查两个父节点及原生状态未变化。

配置DOP2，实测 `maximumActiveWorkers=2`、`maximumActiveActionReplayWorkers=2`，实际动作执行重叠。全部分项通过：五步父节点候选使用ID1，六步父节点候选使用ID2，没有跨父节点目标或状态污染，`currentGeneratorProducedInvalidTarget=false`。之后强制历史 SHIV→ID2 仍抛 `CardPlay has no target creature`，故总结果仍为 **Failed**，不是绿色回归用例或当前生产生成器RED。该结果不代表原DOP16全前沿搜索、原始0.47.2完整运行或所有Mod环境通过。

游戏平台MVID与原Windows不同；官方Ritsu0.6.3的MVID也与报告不同，另有QuickRestart等Mod未加载；这些差异写入结果。BaseLib身份相同不能消除其他环境差异。日志缺少候选生成时的父节点标识、准备目标列表及派发记录，当前材料无法区分历史版本差异、完整前沿时序或环境影响。没有新的可判别假说时停止重复健康场景，不扩大120秒预算，不加自动改目标、跳过动作或吞异常补丁。

专项入口为 `coverage/unattended/b015-t016-original-prefix.json`。它只适用于B015官方T016原ZIP，读取唯一指定日志条目且限制1MiB，不执行档案内容；选择以失败日志中的明确记录为依据，报告 `historySource=original_native_root_plus_failed_candidate`，不伪称 ReplayRecorded。旧状态键只允许已审计的费用后缀增量，所有旧字段及实例序号必须匹配；本次6步映射实际没有改键或序号。15秒局部取消是合作式取消，worker排空可能等待，外层120秒仍是硬上限。

本机使用隔离macOS启动器执行；Windows/Linux公共入口可按以下参数复跑（这两平台本轮未执行；完整Mod身份需另按当地隔离实例流程配置）：

```bash
./tools/run-unattended-test.sh --scenario-id B015-T016-ORIGINAL-PREFIX --character-id SILENT --encounter-id AXEBOTS_NORMAL --checkpoint-archive-path .local/issue-bundles/174/raw/B015/T016/reports/e63cd12543994bb08b9497e45839005c.zip --checkpoint-selector start --replay-mode RestoreOnly --timeout-seconds 120 --cleanup-instance-on-exit
```

```powershell
pwsh -NoProfile -File tools/run-unattended-test.ps1 -ScenarioId B015-T016-ORIGINAL-PREFIX -CharacterId SILENT -EncounterId AXEBOTS_NORMAL -CheckpointArchivePath .local/issue-bundles/174/raw/B015/T016/reports/e63cd12543994bb08b9497e45839005c.zip -CheckpointSelector start -ReplayMode RestoreOnly -TimeoutSeconds 120 -CleanupInstanceOnExit
```

### T020 进一步收敛

最早 `recording/origin.save`（save_time1790564493）已经包含坏牌；一秒后的战前存档相同。开局到首次手动记录的16秒内，永久牌组由15张变14张，当前房间新增移除坏牌历史，但战斗手牌仍保留该坏实例。这说明永久牌组删除没有同步清除当前战斗牌，并不能确定谁执行替换/删除。

原包已提供 FreeLoadout MVID `ab0c70b6-95fd-4711-bfa5-70286aad00cf`、Rewind MVID `fe9e6d12-c159-4895-962c-e910ab635d80`；缺的是与这些身份匹配的实际实现，或第45层替换发生前后的操作记录，而不是再次索取MVID。公开Rewind发布说明不足以证明字段丢失，未将其他同类回退模组冒认为该二进制。

用户转回外部分析后，独立核验[FreeLoadout 108分支固定提交3b151757](https://github.com/Quorafind/FreeLoadout/tree/3b151757b6997effa338abe7219595e4a04fe625)：`Tabs/AddCardsTab.cs` 的“获得升级牌”从 ModelDb 原型 CreateCard，在尚未入Deck时 Upgrade，然后 Add；没有复制 TinkerTime 字段，也没有 CopyCardOverride。其 manifest0.9.12/BonModConfig 与原包另一张 BATTLE_TRANCE 的 `fl_edit=1` 相容。它能产生与坏牌相同的 +1/type0/无rider/无fl_edit 形态，但这不是已知MVID到该提交的映射，更不是原局调用证据；未安装、编译或执行此Mod。

本机正版0.111.0独立核对：Add在获得历史序列化之后才赋 FloorAddedToDeck；Remove在实际移除前序列化移除历史；CardCmd.Upgrade仅在当时处于Deck才记升级历史；FromSerializable先恢复SavedProperties再升级。type为AlwaysSave，默认None会写0；rider默认None按SaveIfNotTypeDefault省略。这些规则解释了“移除合法A→目录新建B→入库前升级→加入”的可行链，不足以证明“B一定入库前新建升级”：移出后修改、内部升级API、恢复/历史重写或缺记录仍须区分。

其他公开入口不能混同：[InspectCardEdit](https://github.com/Quorafind/FreeLoadout/blob/3b151757b6997effa338abe7219595e4a04fe625/InspectCardEdit.cs)的获得与 LoadoutStore 重建会登记编辑覆盖；[OverrideStore](https://github.com/Quorafind/FreeLoadout/blob/3b151757b6997effa338abe7219595e4a04fe625/OverrideStore.cs)与保存补丁才注入/恢复fl_edit。已知覆盖表为模组目录 `data/run_overrides.json`。仅凭坏牌无fl_edit不能排除所有第三方恢复路径。前战结束至本战约96.299秒、一次INIT都成立，但日志索引声明前战仅摘要，录制也为空，不能据“没看到恢复日志”排除同进程恢复。

剩余判别证据收敛为：匹配已知MVID的只读实现/构建映射，加TinkerTime替换边界调用记录或前后检查点；若查恢复，需源快照及恢复后字段。未发现需要在CombatSolver对合法MadScience补默认字段的证据。


## 验证记录与限制

游戏正版0.111.0（41cef1ea）、macOS arm64、.NET9.0.109。RitsuLib[0.6.2](https://github.com/BAKAOLC/STS2-RitsuLib/releases/tag/v0.6.2)及[0.6.3](https://github.com/BAKAOLC/STS2-RitsuLib/releases/tag/v0.6.3)来自官方发布包并核对官方SHA256；后续历史版本检查针对0.6.2编译。前期统计修改前后对照使用本机0.6.5，不与历史0.6.2的结果混称。

所有请求预算120秒，实例使用独立HOME、禁用Steam、结束后删除。下载依赖的 Finder hidden 标志导致一次 Mod 未加载；只清除隔离副本标志后继续。一次启动器目录冲突发生在游戏启动前，随后改为每请求唯一目录；检查未残留本任务游戏进程。这些启动失败均不计语义基线。游戏结果后的 Godot 静态字符串/RID 退出错误仍存在，不声称退出健康性验收通过。

| 请求或检查 | 证据目录 | 结果 |
|---|---|---|
| 已退出消费者，原始实现/初次修复 | `t018-before` / `t018-after` | Failed capacity exceeded / Passed，同输入 |
| 健康消费者饱和，修改前/修改后 | `t018-saturation-before` / `t018-saturation-after` | Failed capacity exceeded / Passed，同输入 |
| 饱和最终边界的历史依赖复核 | `t018-saturation-final-062` | Passed，22.84秒（Ritsu0.6.2） |
| 饱和最终边界、排空再失败与失败元数据 | `t018-saturation-final` | Passed（Ritsu0.6.5） |
| Store纯.NET合同 | `statistics-contracts-v2.log` | Passed；含partial恢复、旧收据撤销及纠正收据保留 |
| 已退出消费者最终检查 | `t018-worker-final-062` | Passed，25.12秒（Ritsu0.6.2） |
| T016原报告Ritsu0.6.3依赖复核（同时包含T019边界） | `b015-boundaries-063` | Passed，29.14秒 |
| T019重建历史Last条件负对照 | `b015-boundaries-historical-guard-062` | Failed，精确命中原Last异常；主线同输入Passed |
| T016原包combat_start严格恢复 | `t016-original-restore-063` | 开战双状态匹配；后续自动Hook使整个请求Failed |
| T016原根、选牌和五步原生差分 | `t016-original-prefix-zip-063` | 前五步通过；强制历史第六步目标不存在，总Failed |
| T016生产准备与兄弟分支 | `t016-production-siblings-063` | 候选/父节点检查通过；随后历史第六步总Failed |
| T016生产双父节点并发，同MVID BaseLib | `t016-parallel-baselib-063` | 2准备/4动作且实际并发2；全部归属差分通过，未生成非法目标；随后历史第六步总Failed |
| T016替补目标与T019原生败局差分 | `b015-boundaries-loss-062` | Passed，30.14秒（Ritsu0.6.2） |
| MadScience合法/非法边界，真实原生差分 | `b015-madscience-062-retry` | Passed，27.37秒（Ritsu0.6.2） |

证据位于忽略的 `.local/evidence/`；专项原包来源与托管探针位于 `.local/triage-agents/`。正式结论只覆盖上述最小行为；未执行原包全环境的 ReplayRecorded/整场部署、真实统计上传或Windows/Linux验收。整批仍用 Refs #174，不关闭问题。

## 本地阶段交付与待确认项

可供后续评审/提交PR的生产修复范围仅为 **T018统计消费者故障与健康队列饱和隔离**，包括持久partial和问题包诊断；T016/T019/T020附带夹具与调查记录不宣称修复。T017保留未知字段拒绝；T019附实验分支补丁提案而未修改main。发布前需在届时上游上整合并按实际影响验证，本轮没有推送或发包授权。

维护者问题草稿（尚未发送）：

1. T017字段已定位 TheHeroExpansion；可否将其按仓库原版范围改列第三方适配，而不在本批放行未知字段？
2. T019原产物来自多人实验提交f1461c7而非正式0.47.2；该缺陷应在哪个维护分支提交？附LastOrDefault最小提案，目标分支的单人败局/多人生存组合仍须验收。
3. T016当前限定生产生成、兄弟回放和真实双父节点并发均未产生非法目标。是否有仍会自然报错的当前构建报告，或能补采首次失败的父节点/准备候选/派发归属？已记录历史构建MVID，无需重复索取同一身份；需要将错误动作与生成它的实际父状态关联。
4. T020能否提供与已知FreeLoadout/Rewind MVID对应的实现或构建映射，以及TinkerTime替换前后的检查点/操作调用记录？若是恢复流程，需要源快照和恢复后的字段。现有公开机制仅证明可行，不能据此归责某Mod。
