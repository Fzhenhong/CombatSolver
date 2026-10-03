# Q002 路线质量排查（2026-10-03）

任务 [#150](https://github.com/Torch1230/CombatSolver/issues/150) 的 O001–O005 整批由 `shun-tong` 认领。当前完成材料核验、检查点 profile 导入修复，以及 O005 在固定 5 秒成员预算下的路线改善与原生验收；其他四个主题和全批验收尚未完成。按主题组织提交，使用 `Refs #150`；全批验收前不使用 `Closes #150`。

基线源码 `9d21f5fc`，manifest 0.48.0；Windows 游戏 0.111.0、RitsuLib 0.6.5。已合入上游 `556e7299`，下列基线均在修改 C# 前取得；后续 profile 修复单独记录。原始 ZIP、完整日志和诊断政策保存在 `.local/issue-bundles/Q002/`，逐请求证据保存在 `.local/checkpoint-batch/`，不提交玩家存档。

## 材料与严格恢复

五包安全盘点通过，原生事件索引均声明完整；声明完整不代表已回放至战斗结束。`start` 验证开战及首个可操作状态；`latest` 只诊断录制后的检查点，不证明求解器能从开局找到玩家路线。

| 主题 / 报告 | 已取得证据 | 当前限制 |
|---|---|---|
| O001 无厌沙虫 / `be3cf3410d4444188ca98ef79b2052f6` | 开战完整 continuation 与原生状态通过，run `20de6853260b40d3aaf59902df212534` | 后续恢复在事件 30 的选牌状态格式处失败，见下文；未证明完整人工路线可比较。 |
| O002 女王 / `8fbb4f7f7d1b4e30832296d73e7a114d` | 开战失败 run `facf75d4184d42b18affd343bc461218`：遗物槽 1 原生导入为 `DEPRECATED_RELIC` | 存档实际持有第三方模型 `ANCIENT_AFFECTION_RELIC_DEVOTED_BOOMING_CONCH`；需要原版内容代表，不能删除遗物伪造同根。 |
| O003 永世沙漏 / `c5e305b95ff444fea1949a58c0b17b04` | 开战通过 run `729dd829931e4489acd8c3059777663a`；最新检查点 T4、29 事件的完整 continuation 与原生状态通过 run `ffb487504c644112a8f349b0d18ec56d` | 原报告 55→2 是跨 T1/T4 的预测变化，人工实机通关尚未验证。 |
| O004 乐加维林族母 / `b76f10234d8b435286083ae1e614646d` | 最新检查点 T1、4 事件通过 run `3d9e776c288440d0833a07fedbb9cab2`，包含喝能力药水、选牌、打出 Tools of the Trade | 51→5 是预测变化；额外药水按每瓶 9 HP 计机会成本，需区分已经使用与未来计划使用。 |
| O005 实验体 / `93133dcfdb114695b08aec08c36716c4` | 最新检查点 T2、12 事件通过 run `9d69774307834aa5ac38b1ecab3ec10f`，完整 continuation 与原生状态一致 | 原报告 55→10 是预测变化；本轮原生执行及人工前缀参照见下文，不宣称同原报告预算的提升。 |

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

诊断仅覆盖原始 `combat_start`，保留原包 Beam、节点、候选容量、药水、成长与遗物政策，显式修改 profile 时间预算并设置 `fixedBudget=true`。后续查出导入遗漏搜索组合与药水奖励预测开关，以下旧结果只代表当时实际执行的诊断政策，不能称完整原报告政策。profile 时间是成员预算，不能当作整个协调器墙钟时间；短搜不能用于宣称同原报告预算的改善。

| 主题 | 本轮诊断 | 结果 |
|---|---|---|
| O001 | profile 5,000 ms；run `7d7fdcf4e11744409ef680327f183d98` | 协调器 18,616.64 ms；展开 58,352、转移 434,195；`onlyDeathRoutes=true`，最终玩家 HP 0、敌方 HP 79。`projectedBattleHpLost=6` 不能解读为损血 6 的获胜路线。 |
| O003 | profile 10,000 ms；run `cd349ecd871e491bae73b04234907af3` | 请求 120 s 超时，无完成结果或可归因进度快照；记录未验证，继续其他样例，不提高请求超时。 |
| O004 | profile 5,000 ms；run `fce35b90c7c547b09650443b06245eff` | 协调器 5,170.59 ms；搜索 8 回合，最终玩家 HP 48、敌方 HP 148、无药水。`onlyDeathRoutes=false`、预测损血 3，但 `combatEndedTurn=null`，只能称存活的未完成路线，不能与完整胜利的损血 5 比较。 |
| O005 | profile 5,000 ms；run `53af01da51bc487b9bb0c94d470afd2e` | 请求总搜索 15,927.54 ms，选中成员 3,691.69 ms；第 7 回合预测获胜，最终玩家 HP 18、敌方 HP 0、损血 22、无药水。不与原报告 600,000 ms 的损血 10 宣称同预算差距。 |

O005 从原始开战根使用相同诊断政策执行 `DeploySolver`，run `dad1abb4070145fdad350ae71a609595`，44.15 s，状态 `deployment_completed` / 请求 Passed。实机战斗 outcome 为初始 HP 38、治疗 2、战中最终 HP 18、损血 22、自伤 20、敌方 HP 0、无药水，实际存活。终局断言另记录 `NativeOutcome:combatEnded=True:turn=8:hp=24:maxHp=89`；不能把终局 HP 24 和战斗 outcome HP 18 混用来算战损。执行采用 Instant/0 秒，实例删除成功。

执行断言 `UnexpectedReplans:0` 只统计 `StateMismatch` 与 `DeploymentDrift`；导出的 `unexpectedReplans=1` 还加上 `ContinuationMissing` 与 `PlanExhausted`，当前结果未细分后两项，不能宣称全部类型重算为零。原生录制导入路径仍将 `comparisonScope` 标为 `checkpoint`，批次工具没有据此自动产出人工优劣结论；完整实机胜负依据是本次 `actualOutcome`，不是该范围标签。上述记录口径问题留待测试入口修正，不通过手改结果掩盖。

O005 是本轮取得完整原生获胜基线的主题，可优先研究自伤换能量的兑现及终局 HP 排序。旧包玩家在 T1 使用 Dark Embrace、Burning Pact、Armaments、Bloodletting 和 Fiend Fire 等牌，T2 检查点已原生恢复；其后损血 10 的记录仍是预测，尚未实机验证。自伤占本轮战损 20/22 是排查线索，不足以断言 Bloodletting、保路或排序存在错误。

O001 首次诊断覆盖文件包含空的 `brightestFlameMaxHpLossLimit`，被 `invalid_policy_override` 拒绝，搜索并未执行；不能将批次等待超时计入搜索性能。修正为只覆盖 `profile` 与 `fixedBudget` 后才取得上述基线。

本节的早期诊断当时尚无搜索优化结论；O005 后续固定诊断政策的改善见最后一节。其他有效缺口仍须证明同根、同政策、同预算、同一比较区间的合法实际存活路线，并扣除额外药水成本；目标和受影响哨兵验收完成后才报告修复。

## 搜索政策开关恢复与 O004 后续诊断

O004 原报告明确记录 `useNoveltyPortfolio=true/useBeamWidthPortfolio=false`，原生录制只有4个事件：喝能力药水、选 Tools of the Trade、系统续执行及打出该牌，没有整场人工通关记录。后续预测获胜T12、损血5还计划使用辉光药水，因此相对无药路线共涉及两瓶药水，不能只计算最后一瓶或把51死亡路线当作51损血胜利。

旧导入实际使用 `useNoveltyPortfolio=false/useBeamWidthPortfolio=true`。本轮 `Q002-O004-guided-prefix-deployment-authorized` / run `9fef20e5c6dd494bbf8258b06e3377a9` 从latest严格恢复4个原生事件，再按旧导入5秒执行；39.56秒在未消灭敌人且玩家未存活处 Failed。初始选中预测为未完成路线（HP51、敌方171、损血0），不是完整获胜；不把失败部署写成可兑现人工基线。首次沙箱启动在本地 admission.lock 访问处失败，未进入游戏；授权启动的结果独立保留。

Testing 的 `ApplyRecordedCheckpointPolicy` 现恢复四项已记录开关：新颖性组合、宽度组合、早期探索和药水奖励预测；Resolve入口同时接受对应显式覆盖及旧 settings 字段。显式false同样生效，缺字段时保留既有本机设置回退。没有改生产评分、预算或默认开关。最小合同修复前 run `471c2401d4ad477da15c5198894f6e93` 在开关被当前设置替换处失败；修复后 `3cd61267424244d19d0091e8305b94dc` Passed（19.85秒），验证两组相反真/假值、冻结政策和旧包回退，原profile/清除合同也通过。首次合同夹具 `e44043fe7a494f2689e8564a35b805dc` 缺并行度，在目标断言前失败；修正夹具后才取得失败基线。

修复后 `Q002-O004-recorded-switches-opening-search` / `f071884ce17449feb9cd9476003e8cdb` 使用原ZIP start、原政策开关及固定5秒profile，Passed且执行政策为true/false；总搜索6,091.01ms，搜索到T8，预测损血3、HP48、敌方154，无药水，未获胜。另在latest原生前缀后做独立10秒诊断（不是5秒质量对照），run `251b51542ff7452ca7fb8629b84a6351` Passed：总搜索11,716.83ms，T15、损血15、HP36、敌方94，无新增药水，仍未获胜。没有提高120秒请求上限，不能把搜索Completed/Passed当作战斗获胜。原报告120,000ms搜索及记录的T12完整路线尚未原生执行验证；O004的有效优化缺口与生产修复仍待定位。

这项导入错误也限制早期O001/O003诊断政策口径。O005报告原本关闭宽度组合、开启药水奖励预测，旧导入则相反；既有22→8双方实际政策一致，固定诊断政策下的改善证据保留，但不是完整原报告开关组合验收。修复导入后要重跑旧诊断，需显式设置 `useBeamWidthPortfolio=true/predictPotionReward=false/useNoveltyPortfolio=false/useEarlyTurnExploration=false`；原报告开关组合须另建同条件基线，不能混用结果。

## O005 人工前缀参照与 profile 导入修复

`Q002-O005-guided-T2-deployment` / run `7fa90699021f4f608f040ea03f48fab6` 严格恢复原生首回合的 12 个事件至 T2，再以原 5,000 ms 诊断政策执行求解器后续。35.63 s 完成；完整战斗 outcome 为初始 HP 38、治疗 2、战中最终 HP 30、损血 10、自伤 5、无药水、实际存活。此 outcome 包括重放的首回合，不能把它当作仅 T2 以后的损血。它证明玩家前缀配合当前求解器后续可以兑现低损路线；不证明开局自主搜索已找到，也不是全手动通关。

基线已有四个 `DARK_EMBRACE` 专门前缀成员：普通/宽/次段分别获胜损血 22/38/26，基础分成员未胜。能力已进入候选，不能把差距归因于根本没搜能力。针对换血收益，先做仅关闭 `CurrentEnergy` 中途加分的实验。

修复前 `Q002-O005-energy-zero-experiment` / run `8674422264c342d19e26a1e49595bfc7` 虽然实机战损仍为 22、自伤 20，但 `policyOverrides.profile.beamWeightPerturbation` 为 `CurrentEnergy:0`，`executedPolicy.profile.beamWeightPerturbation` 却为 null。因此实验没有实际施加扰动，不能作为“降低能量评分无效”的证据。首因是 `ApplyRecordedCheckpointPolicy` 只把基础容量、时间映射到玩家设置，Runtime 再从设置重建 profile，其他记录字段丢失。

修复将完整不可变 `SolverSearchProfile` 放入 `ProtocolHost` 的当前请求，在 `CaptureSearchPolicy` 捕获时消费。独立 CLI 权重扰动保持既有优先级；请求的 `finally/Reset` 清除 profile，普通请求继续使用原设置。没有改默认权重、状态键、模拟或终局政策。覆盖合同 `CHECKPOINT-PROFILE-CONTRACT` 检查完整字段进入冻结政策，并清除后恢复普通 profile；实际报告再核对 `executedPolicy`。这属于实验入口修复，O005 搜索质量仍未解决。

修复后合同请求 `08c185eab6434ca3a95ebfef60a5f820` Passed（20.40 s）。首个启动请求 `9ae3ae2abd2a4d2591d496dbba1d7650` 在启动器进程身份登记处失败、未进入合同；独立实例重试通过。两个实例均已删除，不修改启动器掩盖该失败。

`Q002-O005-energy-zero-profile-fixed` / run `1730f063cf0a40ca846c5327208d0a43` 原包部署 Passed，42.61 s；`executedPolicy.profile.beamWeightPerturbation` 确认为 `CurrentEnergy:0`，证明文件中的完整 profile 实际进入搜索。实机损血 30、自伤 26、战中 HP 10、无药水、存活获胜，比原基线损血 22 更差。撤回关闭能量加分的优化假设，仅保留导入修复；不将显式实验参数作为生产默认。Release 构建零警告/错误、结构门禁通过。未完成 O005 保路根因定位、默认搜索同预算改善或最终质量哨兵验收。

## O005 原生首回合与候选丢失点

测试专用 `Q002-O005-OPENING-PATH` 从未修改的原始 ZIP 的 start 恢复至 T1 可出牌根。读取事件 2 的原生 payload，经游戏 `PlayerChoiceResult.FromNetData` 解得 Burning Pact 选择 `HOWL_FROM_BEYOND`，不是此前尚未证实的 Ascenders Bane 猜测。参照顺序为 Dark Embrace、Burning Pact（上述消耗）、Armaments、Bloodletting、Bully、Fiend Fire、Enthralled、Cruelty、EndTurn。每步从当前影子手牌绑定完整状态身份、目标、附魔重放次数及选择请求身份，检查增量/完整回放一致，不靠只比较牌名重放。

随后原生执行全部 12 个录制事件；当前原生 T2 与影子 T2 完整 ContinuationStamp 严格相等，无 legacy 归一化。额外用新 worker 核对：即使 live 已到 T2，原先冻结的搜索根仍严格等于原 T1。搜索使用该 T1 的完整政策、战损账本和根；玩家序列只用作观察针，不作为固定前缀、候选或评分输入。

最终 run `3fdeb72d4155449a826f7f87f4875668` Passed，27.90 s；2,399 条纯值事件，零丢弃，完整外层候选池与排序/必保/路由/选中索引合同通过。`O005-opening-state.json`、`O005-recorded-choice.json`、`Q002O005Opening-path-trace.json` 保存在 `.local/q002-o005-opening-path-final/`。观察器未改变冻结根或 live，实例自动删除。

| 准确玩家前缀第 4 步 Bloodletting 后 | 原始排名（从 0 开始） | 候选池 / 保留容量 | 必保 / 路由 / 选中 | 外层最终池 |
|---|---|---|---|---|
| 普通能力成员 | 240 | 249 / 60 | 均无 | 不含该前缀 |
| 宽能力成员 | 240 | 249 / 90 | 均无 | 不含该前缀 |
| 次段能力成员 | 240 | 249 / 60 | 均无 | 不含该前缀 |
| 基础分能力成员 | 235 | 249 / 60 | 均无 | 不含该前缀 |

以上四个成员均准确生成第 4 步，入场转置为 `accepted_new_state`，完成动作提交并进入 `PruneInput` / 完整外层排序。普通、宽、次段分数为 -5,692,003.902，基础分为 -6,104,003.902；该状态 HP 37、能量 5、格挡 5、手牌 6、可用手牌价值 36，结束当前准备时的威胁投影 HP 为 11。没有观察到第 4 步的严格同状态别名在最终池存活，也未准确生成第 5 步 Bully。普通主搜在第 3 步后未再准确展开；本轮只捕获第 4 步完整池，不编造主搜第 3 步的实际排名。

宽成员保留了另一顺序 Dark Embrace→Bloodletting→Burning Pact→Armaments，虽然可见指标与第 4 步相同，其完整状态键不同；不能把它自动算作参照路线的等价别名或低损后缀证明。当前结论限于这条合法参照顺序在能力成员第 4 步被中途保留淘汰，排除了该步未生成或入场转置拒绝；尚未确定覆盖同类场景的最小生产修复。

观察器复制完整候选池会影响有时间限制的搜索工作量，诊断请求的预测损血 77、请求总搜索 8,408.50 ms 未实机部署，也不作为默认搜索退化、质量或性能基线。该诊断提交未修改生产搜索；后续优化及最终验收见下一节。

夹具开发期间的失败单独保留：`abe2fa9140024766b894eb513d9c7c65` 使用遭遇 ID 作为怪物 ID，在根约束处失败；`e48e5a53d61b40d581656b3662f47d76` 解码选择后拒绝了错误的 Ascenders Bane 假设，均未进入搜索。`645c212a1be141af86430c244720b4ff` 验证原生首回合并观察第 2 步池；根据实际首丢点改为第 4 步池后，`b81add17e2b64146be521ec7be592491` Passed。最终源码再增加原生推进后冻结原根的严格合同，取得上述最终 run；不把失败或观察位置不同的请求合并成质量通过。

## O005 分支攻击估值与原生验收

`CardChoiceSupport.CardValue` 只读取基础 Damage，忽略 CalculatedDamage。Bully 等计算攻击牌在可用手牌、保留攻击及附魔重放三个中间估值通道中因此失去状态变化和升级收益。搜索现在统一读取已登记的 `CalculatedVarSpecRegistry` 分支计算，以当前可攻击目标中的最大原始计算伤害替换基础 Damage 分量；可用手牌另计附魔额外重放。未登记的计算牌继续原估值，不调用绑定 live 的计算 delegate。权重、终局比较、Beam/分支容量、政策和模拟动作语义未改。

只修手牌估值不能解决问题；额外保路代表也挤掉了原来的更好路线。以下均为相同原包、相同 5 秒政策的无观察器搜索实验，失败候选已完整撤回，没有部署到正常游戏或进入最终源码：

| 实验 | runId | 预测完整战损 |
|---|---|---|
| 只补计算攻击的手牌价值 | `3b9e9e977e64414a94241802a2e370ea` | 29 |
| 增加卖血后手牌代表 | `154258d93fed4b78a5e34716085fdeba` | 23 |
| 同值优先较近卖血 | `5989f49b59414248be920ded3091b697` | 23 |
| 保留卖血后的资源交接 | `916e2eba8fbd48bd90361c43f3059153` | 23 |
| 扩展至移除加费等手牌准备 | `33d7a2877db64ccf8664f532b202d2e0` | 29 |
| 优先刚完成的准备 | `86bfdf7f07a24f3e9580ea65f00a4db1` | 33 |

最终统一三处估值的 `d79063a566e348a9a0bc01ed39cbd23a` 预测获胜战损 8、无药水，T9 结束；请求总搜索 12,900.86 ms，展开 52,519、转移 195,662。原生完整部署 `eb16ab8d1a1c414a965edb7283abd8f6` Passed（41.86 s）：初始 HP38、开局回血2、战中结束 HP32，实际损血8、自伤8、敌方 HP0、无药水、无未归因损血、计划外重算0。战后回血后的界面 HP38 不用于战损计算。修改前原生基线 `dad1abb4070145fdad350ae71a609595` 实际损血22、自伤20、结束 HP18；双方使用原始 start 与同一完整执行政策。该诊断实际为 `useBeamWidthPortfolio=true`、`predictPotionReward=false`，而原报告为 false/true；两侧 `useNoveltyPortfolio=false`。22→8 对照有效，尚不能外推到原报告开关组合。新路线包含自己的消耗选择和后续顺序，不注入玩家开局或照抄人工前缀。

最终保存基线DLL与候选DLL分别在独立冷启动实例中进行正常搜索成对对照，完整根与完整政策严格相同，DOP16、诊断关闭，未扩大预算。两次均 Passed；请求总耗时无增加，不能把选中成员耗时当请求耗时，也不外推为可见游戏帧率改善。

| 源码 | runId | 完整战损 / 药水 / 结束回合 | 总搜索 ms | 总展开 / 总转移 |
|---|---|---|---|---|
| 保存基线4265aa43 | `1e971860a69e4d35b33d9e81772e56b2` | 22 / 0 / T7 | 13,333.21 | 48,497 / 207,318 |
| 最终候选 | `457f8cc2fab54e6996a74a59012c01f3` | 8 / 0 / T9 | 12,430.01 | 53,243 / 198,199 |

估值合同 `2aae841f4879480c8e68fb25d55841c8` Passed（29.78 s）。原始首回合每步增量/完整回放、原生12事件后的完整 T2 continuation，以及原生推进后冻结 T1 根均严格一致。真实 Armaments 前后，重放价值6→7、保留攻击价值56→63；第4步可用手牌价值49。2,814条观察事件、零丢弃；准确玩家前缀第4步仍未保留，所以不宣称修复保证保留这条具体排列。整场质量改善来自实际搜索找到的另一条路线，诊断时间不用于性能结论。

独立哨兵 [calculated-attack-routing-sentinel.json](../../coverage/unattended/calculated-attack-routing-sentinel.json) 固定 Silent、Body Slam+Glam、升级、消耗选择和格挡准备，使用训练外牌序/RNG。原始请求敌方 HP60 被原生模型上限钳为55，两侧完整根、政策和完整路线严格相同；均为T1、战损0、无药水、敌方HP0、无时间截断。正常离线协调器基线/候选为551.68/534.77 ms，展开107/104、转移431/444；不作为原生性能或广泛提速结论。原生 `4bffc6ef7448474988e4adef480b179c` Passed（18.82 s），启用逐转移增量核验，HP60、T1获胜、计划外重算0；该验证的时间不纳入性能对照。相邻无战后回血卖血哨兵仍为战损3、T2、无药水，完整根和工作量103/268一致。

测试过程中的失败不合并为通过：`38261b0dfaaa43548dfe14f38da207e4` 观察第6步却断言第4步完整池，属于夹具配置错误；`a8eb4058d1ca43958c712e86ac05612a` 使用未经确认的22→33估值断言，在搜索前失败，随后改为核对真实升级增益并导出标量。启动请求 `59899824679a4667bdfc9c3e33144c7e` 未通过启动器私有进程身份登记、未执行合同；独立实例重试结果分别记录。所有游戏实例自动清理。验收范围限于此固定政策的O005及上述哨兵，未验证原报告600,000 ms配置、所有计算攻击牌或O001–O004。
