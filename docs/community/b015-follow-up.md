# B015 后续：统计饱和隔离与其余主题的证据边界

关联 Refs #174，延续[阶段一](b015-stage-one.md)。源码基线仍为 `88298ae5`；只在 `fix/batch-b015` 本地工作。初始调查时认领获得维护者积极回复、Assignee仍空；收尾刷新[任务页](https://github.com/Torch1230/CombatSolver/issues/174)确认已正式指派 `s1f102500012`（issue updated_at=2026-10-02T10:49:17Z）。没有推送、评论、PR、原游戏目录部署或发布。

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

因此不应把该字段按原版显示字段放行：忽略字符串不能实现该 Mod 的战斗语义。当前保留未知字段拒绝。包中没有该 Mod 的实际 DLL/MVID/源码映射，未证明历史二进制与此提交完全一致，也未验收完整第三方适配。此项的可交付结果是纠正“原版遗漏字段”的分诊前提和明确适配范围，而不是声称已修兼容性。没有证据将字段归因于 RegentFX。

## T020：坏状态早于战斗，合法状态对照通过

原包 `pre-combat/in-memory-current_run.save` 的 `map_point_history[2][11]` 对应 TINKER_TIME：选择 Skill、Chaos；获得合法 type=2/rider=6 的牌，同时记录移除该牌及获得 type=0 的升级牌。`players[0].deck[14]`、combat_start 和首个根已经含无效的 MAD_SCIENCE+1。该房间没有 upgraded_cards 记录。数组本身不证明精确操作顺序或调用者，但足以排除“最早可见坏状态出现在搜索 Fork”的说法。

原版0.111.0的升级只添加 Innate；保存属性恢复在升级前写回类型与附加效果。独立托管探针证明合法状态经升级、MutableClone、SavedProperties JSON往返保持 Skill/Chaos；仅按 canonical ID 重建再升级的负对照变为 None/None。此探针不是游戏语义验收。

另有真实游戏 `B015-MAD-SCIENCE` 通过：原生升级和保存恢复、根/Fork/MutablePreview、兄弟分支修改隔离、合法出牌的8格挡/1能量消耗/1生成牌，以及原生完整状态和 RNG 差分；无效 None 明确抛出原有异常。没有新增默认 Attack 或无依据字段复制补丁。

[FreeLoadout 当前公开升级入口](https://github.com/Quorafind/FreeLoadout/tree/2646b2d1f17cd2e4afe2321450f7c7c49d46fc7e) 直接调用原生 CardCmd.Upgrade，不能凭原包加载列表归罪该 Mod。要确认替换调用者，仍需历史 Mod 精确构建或 TinkerTime 房间的编辑/移除/重建操作记录。

## T016 / T019

`B015-BOUNDARIES` 已在0.111.0/Ritsu0.6.2通过两项严格原生对照：

- T016：保留原生Stock、将原敌HP设为1，第一张SHIV击杀并产生不同CombatId的替补；验证活动/已退场身份、Fork、父分支不变、对替补第二张SHIV的完整与增量回放、两步原生完整状态一致。
- T019：替补执行无伤害Boot Up，玩家1HP实际打出CrimsonMantle确认SelfDamage=1，EndTurn到T+1；预测死亡与终局戳、Fork、原生ProcessPendingLoss安全点前完整状态一致。

这两个最小边界在本轮行为源码修改前后没有差异；未改卡牌目标或Power生命周期实现。它们没有复现原包异常，不能代替原报告完整动作链、Silent/A9配置及BaseLib/其他Mod环境，也不构成“已修复”结论。T016仍需从原根复现完整前缀并确认目标2何时不可解析；T019仍需报告实际构建及参与者/Power组合。

夹具校正均保留失败证据：第一次增量调用误传priorActionCount=0，状态键/续用相同但评分不一致，后改为1；第二次沿用胜利EndCombatInternal观察点，原生已正常死亡但无对应快照，120秒无结果。核对原版后改用败局ProcessPendingLoss，另加15秒局部等待；未扩大总预算或放宽状态断言。

T019 官方 v0.47.2 发布包 SHA256 为 `d908542df68d3e8e0f0f294d9d081c6e6f9479c703141ca605dcc9755a19d1d1`，DLL MVID 为 `0ac802ba-4f27-4050-9c11-f342f12fbdfd`。定向 C#/IL 核对发现 PersistentPowerSupport 整类无 Enumerable.First，入口已分段；对应 GetAmount/GetPower 缺失项返回零/null或用FirstOrDefault，与 tag 一致。包中未带实际 CombatSolver DLL/MVID，所以这仍不能解释原始 First 栈，也不能称“当前已经修好”。

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
| T016替补目标与T019原生败局差分 | `b015-boundaries-loss-062` | Passed，30.14秒（Ritsu0.6.2） |
| MadScience合法/非法边界，真实原生差分 | `b015-madscience-062-retry` | Passed，27.37秒（Ritsu0.6.2） |

证据位于忽略的 `.local/evidence/`；专项原包来源与托管探针位于 `.local/triage-agents/`。正式结论只覆盖上述最小行为；未执行原包全环境的 ReplayRecorded/整场部署、真实统计上传或Windows/Linux验收。整批仍用 Refs #174，不关闭问题。
