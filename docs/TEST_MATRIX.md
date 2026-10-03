# CombatSolver 测试入口

按改动选择最小验证层，方法见 [无人测试](HEADLESS_TESTING.md) 与 [社区验收](community/testing-guide.md)。以下记录保留取得证据时的源码和范围，不能视作本轮重新通过。

历史记录见 [归档索引](archive/testing/README.md)。

## Q002 O004 原预算与开局补搜隔离（2026-10-04）

原ZIP/latest、原120,000ms profile/开关、覆盖仅`fixedBudget=true`，SearchOnly `d71823b3fa414e70a50bead7f2b27699`及DeploySolver `e42428465feb47c083d5721a124d7a94`均Passed，80动作/完整根戳/执行政策相同；T12/5损、整场能力药水+发光水2瓶、Instant/0秒、四类重算合计0。latest在玩家4原生事件后，不能证明开局；真正start `a79419b9e5e0421eb8f39621d818665e`仍死亡51，仅改用药上限的实验`1ed9d378d1754e32b46c4e081f244038`超时无result，已撤回。

最小成员诊断`Q002-O004-OPENING-POTION-POSTERIOR` / `8ced0c7fbdcd4aa8b4036c7759235e41` Passed（55.10s）；原ZIP/start/RestoreOnly、原政策，实际成员限30秒/60,000节点/Beam90/BaseScoreOnly。82动作完整回放T12/5损/2瓶，原生4事件的完整ContinuationStamp及冻结开局严格一致；相同固定前缀最多1/2瓶均死亡51。`Q002-O004-OPENING-POTION-RANK` / `0b08f9e230c144638a4177113b09ac24` Passed（47.98s），仅取消BaseScoreOnly仍死亡51。隔离前缀不代表协调器自主发现、性能或完整原生部署；观察版`Q002-O004-OPENING-POTION-PATH`最终`30582727eed44c39853dac590fec979c` Passed（48.26s），2,400事件/零丢弃，首回合准确保留/展开，第6步准入后rawRank916/limit90未保留；搜索后实机根与冻结开局仍严格不变。固定能力承诺生产实验曾让独立O005退化8→29，已撤回，不能作修复。完整结果另记[证据](issues/q002-route-quality.md#o004-原预算的检查点与开局差异2026-10-04)。实例自动删除，Linux脚本接受同一scenario，Linux实机未验证。

`Q002-O004-TURN-BOUNDARY-MEMBER` / `8e6403ddf3604b84a356343f97bd10ac` Passed（40.58s）：同开局、固定保存的5步首回合，原生4事件setup及82动作预测见证/冻结根一致，30秒/60,000节点/Beam90/最多2瓶，重建基线后T12获胜/17损/2瓶，19,557.65ms、19,267/201,308。`Q002-O004-POSTERIOR-FRONTIER` / `6be6cfba194e4fef8904d76b9f3616aa` Passed（42.74s），捕获1个自主首回合候选、未达64上限；该候选是7动作、零损/敌方损20，区别于最终死亡路线直接EndTurn。`Q002-O004-FRONTIER-CONTINUATION` / `00f1306f78e243e8abfb2681356c24e3` Passed（56.32s），原成员捕获候选后最终仍死亡51，第二成员用该候选续搜T12/7损/2瓶，13,669.78ms、16,978/180,361；两个各30秒成员不是30秒总预算。预测见证不代表原生T2或整场执行；前缀仍固定了玩家生成能力的两步，不能作正常开局质量验收。实机与冻结根不变、实例删除；这些隔离模式不证明正常编排。

O004正常开局候选SearchOnly `4127ebbf89c04d3d98e3c8a7d6399010` Passed（89.45s），与基线完整根/政策相同，死亡51→T12/7损/2瓶、总搜索81,510.32→70,199.03ms；配置成员数/配额不提高，但实际请求展开129,147→134,773均超名义120,000节点，尚需评估账本总约束。仅正常搜索接线已验证，独立哨兵和原生完整部署未执行；候选已保存补丁并撤回，未部署。按用户要求收尾，恢复已验收生产源码；续做材料与步骤见[入口](issues/q002-route-quality.md#下次继续入口2026-10-04)。

## Q002 O003 能力代表保路（2026-10-04）

`Q002-O003-PLAYER-T3-PATH`、原O003 ZIP、selector `4052e28b38544018ab6f3f9b2acd8c8e:5`、RestoreOnly、固定5秒政策、120秒请求：最终夹具 `43ac830f2ac9475f9242febfd4c4bf13` Passed（62.05s），53动作增量/完整回放、原生9事件后的T4完整戳和推进后的冻结T3严格一致；13,558条事件、无丢弃，完整第7步候选池及2个实际别名的严格获胜后缀通过。别名后缀仅证明战斗合法性，不证明调度历史相同；观察耗时不作性能证据。

正常协调器T3同根/同政策/原10秒预算：基线 `30f14344f9e84b15b987ad36273005a6` 未完成、敌方345，候选 `a40cc85a2e3b47a5961bd7868e4bd27e` T10获胜、损血31；总搜索23,541.16/19,670.09ms、展开41,218/41,259、转移259,483/258,567。原生DeploySolver `e9042b8d435341438f929c786b7af9ad` Passed（66.34s），Instant/0秒、实际T10/损血31/敌方0/未归因0、整场Stable Serum1瓶、导出四类计划外重算合计0。

O005同根同5秒政策、同3GiB主机预留的正常哨兵：基线 `8d72a922aa3e4935a0d7513554bea63e` 与候选 `c6d7546911b94057b91110269de4c76c` 均T9/损血8/无药，完整根及执行政策逐字段相同；总搜索15,869.86/16,183.78ms、展开53,351/52,794、转移197,591/194,074。不是观察或增量诊断时间；单对不能证明广泛性能无退化。

激活时机剩余席位实验`a756b255069448e19207a00bdf141608`原T3根/原10秒政策与基线完整根戳/政策相同，仍T10/31损、无新增药水；单对总搜索更慢，无质量改善，已撤回，不列入生产验收。

回合边界成员对照`Q002-O003-TURN-BOUNDARY-MEMBER` / `7b03065ecab94bb78c2ac181437b3c06` Passed（49.76s）：原ZIP/T3/RestoreOnly，6步完整/增量与9事件的完整T4戳一致；固定同一前缀，普通排序/无新增药、5秒/30,000节点各一组。重建基线false为T10/4损、4,449.69ms/7,068展开/29,193转移，true为T10/3损、2,293.38ms/7,066/30,127；实机T4及冻结T3严格不变，实例自动删除。固定前缀未建立普通扩展的能力承诺，两组比较范围是既有基线重建开关；不能代替正常协调器或完整原生执行。

回合续搜候选正常SearchOnly `72de7007a754407599f9ba15d6d7110a` Passed（52.06s），原T3根/原10秒政策/3GiB与e490基线完整根及政策相同：T10/31→4损，总搜索19,670.09→18,250.43ms、41,259→41,888展开/258,567→262,332转移。复用一个已有能力变体名额/原配额，不追加成员。正常独立O005 `e5bbc232e53f473bac3ecde0a37d1aae` Passed，同根/同5秒政策仍T9/8损/无药，总搜索16,183.78→14,241.56ms。正常DeploySolver `58e0a80c82734357aee01cce67d25c2c` Passed（59.91s），与候选完整根/政策/动作相同；Instant/0秒，T10原生4损、治疗2/自伤1/未归因0/敌方0，无新增用药、四类重算合计0。Release 0警告/0错误，职责门禁246通过；实例删除。未验收T1开局，2损目标仍未追平，单对不证明广泛性能/质量无退化。

纯合同：`dotnet run --project tools/PowerCardValuationChecks/PowerCardValuationChecks.csproj -c Release`，`POWER_CARD_VALUATION_CHECKS_OK total=104`；新增集合/激活顺序/药水和回合隔离/死亡和终局排除/配额/输入不变测试运行真实代表选择器，工具值节点不替代原生正确性。实例全部删除，失败夹具、主机排队和未验证范围见[Q002记录](issues/q002-route-quality.md#o003-完整见证与能力代表保路)。Linux脚本接受同一scenario，Linux实机未运行。

## Q002 完整保存预测与原报告开关（2026-10-03）

O004 `CHECKPOINT-RECORDED-PLAN-DEPLOYMENT`、原ZIP/latest、DeploySolver、5秒诊断政策、120秒请求、Instant/0秒：`c073913577214d1bb8caa44161733ad6` Passed（40.29 s），完整80动作逐步增量/完整回放严格一致，原生T12、实际损血5、整场能力药水+发光水2瓶、后台搜索代次无增加、导出四类计划外重算合计0。`CHECKPOINT-RECORDED-PLAN-PATH`、同根SearchOnly的`2caefffca9b94e77b3e2b0e8cb7727e4` Passed：观察130条事件、丢失0，前三步生成并展开，第4步用药未生成，主搜索6139ms耗尽5秒补搜预算。冻结动作仅作观察，不注入搜索；不是质量修复或正常性能证据。夹具失败及复跑入口见[Q002详情](issues/q002-route-quality.md#o004-完整预测可执行性与首个搜索缺口)与[检查点指南](CHECKPOINT_REPLAY.md#验证)。实例均删除。

O005原报告开关正常搜索AB `e61c0f313c7d489b892c0300c44830ad` / `50c1dfadaf1e40e69220f0bd6d1fa8a2` 均Passed；原ZIP/start、完整恢复、完整执行政策逐字段相同、DOP16、5秒成员预算、无观察器。实际关闭宽度组合、开启药水奖励预测，两侧仍为完整战损22/8、无药水、T7/T9；总搜索14914.94/14690.31ms，总展开49423/53526、总转移210900/198547。不宣称复测原报告600,000ms。

同原报告开关原生DeploySolver `d3aa08e6b9f64f8d8d42eb5a8c4303a7` Passed（46.97s）：普通协调器自主搜索、与AB候选完整政策相同，T9获胜，实际初始HP38/治疗2/终局HP32/战损8/自伤8、无药水及未归因损血、导出四类重算合计0；Instant/0秒，实例删除。

## Q002 搜索政策开关导入验收（2026-10-03）

扩展 `CHECKPOINT-PROFILE-CONTRACT`：报告的四项组合/探索/药水奖励开关与当前设置逐项相反，分别覆盖 true 和 false，检查实际设置及冻结政策，缺字段仍保留既有回退。修复前 `471c2401d4ad477da15c5198894f6e93` 在开关丢失断言处 Failed；最终 `3cd61267424244d19d0091e8305b94dc` Passed（19.85秒），同时通过完整 profile 与清除合同，不运行 Solve。前一次夹具 `e44043fe7a494f2689e8564a35b805dc` 缺必填并行度，在测试目标之前失败，单独保留。

复跑沿用 `CHECKPOINT-PROFILE-CONTRACT` 原入口。原始 O004 的 `f071884ce17449feb9cd9476003e8cdb` 另验证从 ZIP start 导入实际 `useNoveltyPortfolio=true/useBeamWidthPortfolio=false`；固定5秒结果仍未胜，不能用其预测战损3作完整质量验收。原生四事件前缀后的10秒搜索 `251b51542ff7452ca7fb8629b84a6351` 仍未胜；不能与5秒开局作为同根同预算对照。实例均自动删除。具体政策、失败部署及限制见 [Q002 记录](issues/q002-route-quality.md#搜索政策开关恢复与-o004-后续诊断)。

## Q002 O005 计算攻击估值验收（2026-10-03）

原始 O005 ZIP start、5,000 ms成员预算、当时实际导入诊断政策的正常搜索 `d79063a566e348a9a0bc01ed39cbd23a` 预测战损8；原生完整部署 `eb16ab8d1a1c414a965edb7283abd8f6` Passed，实际战损8、自伤8、无药水、计划外重算0。修改前同输入原生战损22。两侧均实际启用宽度组合、关闭药水奖励预测；原报告这两项分别为关闭/开启，因此不是完整原报告政策验收。完整录制/模拟T2、增量/完整首回合、冻结T1根与升级估值合同 `2aae841f4879480c8e68fb25d55841c8` Passed；记录真实重放价值6→7及保留攻击56→63，不按推算层数写死数值。

最终正常搜索成对验收 `1e971860a69e4d35b33d9e81772e56b2` / `457f8cc2fab54e6996a74a59012c01f3` 均 Passed：完整根、完整政策相同；总搜索13,333.21/12,430.01 ms，展开48,497/53,243、转移207,318/198,199，完整战损22/8、无药水。使用独立冷启动游戏、DOP16、无路径观察器/增量诊断；不外推帧率或所有输入性能。

训练外 [计算攻击哨兵](../coverage/unattended/calculated-attack-routing-sentinel.json) 在同根、同政策、同预算的正常离线协调器中，两侧完整路线相同，T1零损、无药水、无时间截断；总搜索551.68/534.77 ms，展开107/104、转移431/444。原生 `4bffc6ef7448474988e4adef480b179c` 启用 `VerifyIncrementalSearch` Passed：T1、HP60、计划外重算0，时间只作正确性证据。无战后回血卖血相邻哨兵保持3损/T2、工作量103/268。

哨兵正常对照：保存修改前DLL，分别设置 `OFFLINE_HARNESS_COMBATSOLVER_DLL`，运行 `tools/OfflineSearchHarness/bin/Release/net9.0/OfflineSearchHarness.dll --request coverage/unattended/calculated-attack-routing-sentinel.json --search-mode Coordinator --use-portfolio --beam 60 --nodes 120000 --card-branches 32 --pile-branches 18 --hand-branches 24 --dop 1 --budget-ms 5000 --potion-policy Disabled --out <独立目录>`。原生入口从同一JSON读取角色、种子、遭遇、初始状态和cards，传至 `run-unattended-test.ps1` 的对应参数，带 `ClearRunDeck/ClearPlayerPiles/ClearAllPowers`、显式相同搜索配置及预期零损/无药/零重算；正确性请求另带 `VerifyIncrementalSearch`，部署固定 `Instant/0秒`，请求120秒且自动清理。Linux可用同一输入及GNU参数，未运行Linux游戏。

证据及失败夹具记录见 [Q002 验收](issues/q002-route-quality.md#o005-分支攻击估值与原生验收)。不宣称原报告600,000 ms配置、其余主题或全部计算攻击牌通过；观察器结果不纳入路线质量和性能对照。

## Q002 O005 原生前缀及搜索观察（2026-10-03）

`Q002-O005-OPENING-PATH` 使用原始 O005 ZIP、`CheckpointSelector=start`、`ReplayMode=RestoreOnly`、显式同预算政策及 EvidenceDirectory，请求上限 120 s。先按原 payload 绑定首回合选择，验证每步完整/增量回放及影子回放不改 live；随后原生录制推进到 T2，与影子完整 ContinuationStamp 严格相等，才观察冻结的原始 T1 搜索。`O005-opening-state.json` 保存双方戳和完整动作；`Q002O005Opening-path-trace.json` 保存观察事件、准确动作匹配及状态别名。后者不把状态别名自动视作已知动作生成/保留，也不证明完整胜利。

复跑使用 `tools/run-unattended-test.ps1 -ScenarioId Q002-O005-OPENING-PATH -CharacterId IRONCLAD -EncounterId TEST_SUBJECT_BOSS -CheckpointArchivePath <O005.zip> -CheckpointSelector start -ReplayMode RestoreOnly -ReplayPolicyOverridePath <policy.json> -EvidenceDirectory <evidence> -HeadlessInstance q002-o005-path -TimeoutSeconds 120 -CleanupInstanceOnExit`，按本机传入游戏/Ritsu 路径。Linux 入口使用对应参数；本轮未验证 Linux 实机。结果见 [Q002 排查](issues/q002-route-quality.md)。

最终夹具 run `3fdeb72d4155449a826f7f87f4875668` Passed（27.90 s）：原生/影子 T2 严格相等，原生推进后新 worker 的冻结 T1 戳与原 T1 严格相等；2,399 条纯值事件、零丢弃，完整外层候选池、实际排序及最终保留集合检查通过，根与 live 未被观察修改。四个能力成员在第四步未保留此准确前缀；这是参考路径首丢点证据，不是 O005 已修复。Release 构建零警告/错误，实例自动删除。

## Q002 检查点 profile 完整恢复（2026-10-03）

修复前 O005 的 `8674422264c342d19e26a1e49595bfc7` 覆盖文件为 `CurrentEnergy:0`，执行政策却是 null；损血仍为 22，不能视为有效权重实验。修复后 `CHECKPOINT-PROFILE-CONTRACT` 请求 `08c185eab6434ca3a95ebfef60a5f820` Passed（20.40 s）：完整 profile 字段进入冻结政策，清除后普通 profile 恢复；不运行 Solve，不作为路线或性能验收。首次启动 `9ae3ae2abd2a4d2591d496dbba1d7650` 在进程身份登记前失败，未运行合同；换独立实例后的通过与该失败分别保留。测试实例已自动删除。

复跑：`tools/run-unattended-test.ps1 -ScenarioId CHECKPOINT-PROFILE-CONTRACT -HeadlessInstance profile-contract -TimeoutSeconds 120 -CleanupInstanceOnExit`，按本机覆盖游戏/Ritsu 路径；Linux 使用等价 scenario 与 GNU 参数。未运行 Linux 实机。目标原包及实验后续结果见 [Q002 排查](issues/q002-route-quality.md)。

实际输入链验证：原包 `93133dcfdb114695b08aec08c36716c4` 的 `1730f063cf0a40ca846c5327208d0a43` 完成原生部署（42.61 s），执行政策确为文件请求的 `CurrentEnergy:0`。实验实机战损 30、自伤 26、无药水，较旧默认基线战损 22 更差，未采纳该权重变更；只验收 profile 导入。初始原生恢复通过，实例自动清理。Release 零警告/错误、结构门禁通过，不代表默认搜索质量改善、性能无退化或完整社区验收。

## Windows Steam 配置初始化（2026-10-03）

修改前 `SMOKE-001` 在配置初始化时失败：玩家仅有 Steam 账号配置，启动器未生成私有 `default/1/settings.save`。修改后 `tools/test-headless-profile.ps1` 通过 Steam 迁移、default 优先、配置/进度复制、私有副本复用、来源不变、缺少配置及多账号拒绝检查。Linux 入口通过 Bash 语法检查，未运行 Linux 行为测试。

Windows 游戏 0.111.0、RitsuLib 0.6.5 的 `SMOKE-001` 请求 `84c21af68fc345e2a6d4a442cfa81e7b` Passed（20.55 秒）：游戏加载、测试请求、搜索和原生战斗结束通过，首回合结束，玩家 HP 80。采用 `-TimeoutSeconds 120 -CleanupInstanceOnExit`，实例删除成功；本地证据目录 `.local/preparation-smoke/`。这只是最小冒烟测试，不代表社区报告修复、完整语义或路线质量验收。

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

多人分支归档前的整理检查：主线 419 份 Markdown、1573 个本地链接和锚点通过；多人分支 400 份、1545 个链接通过；独立日志服务 14 份、30 个链接通过。当前指南与活动记录的长度门槛通过。主线与多人分支 Release 构建均为零警告/错误，主线 Windows 结构门禁通过（246 个 Search 文件）。后续第三方目录归并与多人规划归档后，主线 420 份、1574 个链接通过；归档分支停止维护。

两条分支 CoverageCatalog 实际生成，`--verify-state-fields --verify-branch-state-reads` 通过。主线仍有注能核心一个 active exact Hook 缺运行证据；多人分支该目录未报运行证据缺口。目录生成消费历史证据，本次没有运行原生战斗、性能或全量 verify，不扩大旧结果的适用范围。
