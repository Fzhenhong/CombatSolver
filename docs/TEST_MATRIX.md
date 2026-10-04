# CombatSolver 测试入口

按改动选择最小验证层，方法见 [无人测试](HEADLESS_TESTING.md) 与 [社区验收](community/testing-guide.md)。以下命令提供当前复跑入口，不表示本轮已执行。

历史记录见 [归档索引](archive/testing/README.md)，0.48.1 的验证、失败与未验证项见 [历史卷 12](archive/testing/volume-12.md)。

## Q002 O004 原预算与开局补搜隔离（2026-10-04）

原ZIP/latest、原120,000ms profile/开关、覆盖仅`fixedBudget=true`，SearchOnly `d71823b3fa414e70a50bead7f2b27699`及DeploySolver `e42428465feb47c083d5721a124d7a94`均Passed，80动作/完整根戳/执行政策相同；T12/5损、整场能力药水+发光水2瓶、Instant/0秒、四类重算合计0。latest在玩家4原生事件后，不能证明开局；真正start `a79419b9e5e0421eb8f39621d818665e`仍死亡51，仅改用药上限的实验`1ed9d378d1754e32b46c4e081f244038`超时无result，已撤回。

最小成员诊断`Q002-O004-OPENING-POTION-POSTERIOR` / `8ced0c7fbdcd4aa8b4036c7759235e41` Passed（55.10s）；原ZIP/start/RestoreOnly、原政策，实际成员限30秒/60,000节点/Beam90/BaseScoreOnly。82动作完整回放T12/5损/2瓶，原生4事件的完整ContinuationStamp及冻结开局严格一致；相同固定前缀最多1/2瓶均死亡51。`Q002-O004-OPENING-POTION-RANK` / `0b08f9e230c144638a4177113b09ac24` Passed（47.98s），仅取消BaseScoreOnly仍死亡51。隔离前缀不代表协调器自主发现、性能或完整原生部署；观察版`Q002-O004-OPENING-POTION-PATH`最终`30582727eed44c39853dac590fec979c` Passed（48.26s），2,400事件/零丢弃，首回合准确保留/展开，第6步准入后rawRank916/limit90未保留；搜索后实机根与冻结开局仍严格不变。固定能力承诺生产实验曾让独立O005退化8→29，已撤回，不能作修复。完整结果另记[证据](issues/q002-route-quality.md#o004-原预算的检查点与开局差异2026-10-04)。实例自动删除，Linux脚本接受同一scenario，Linux实机未验证。

`Q002-O004-TURN-BOUNDARY-MEMBER` / `8e6403ddf3604b84a356343f97bd10ac` Passed（40.58s）：同开局、固定保存的5步首回合，原生4事件setup及82动作预测见证/冻结根一致，30秒/60,000节点/Beam90/最多2瓶，重建基线后T12获胜/17损/2瓶，19,557.65ms、19,267/201,308。`Q002-O004-POSTERIOR-FRONTIER` / `6be6cfba194e4fef8904d76b9f3616aa` Passed（42.74s），捕获1个自主首回合候选、未达64上限；该候选是7动作、零损/敌方损20，区别于最终死亡路线直接EndTurn。`Q002-O004-FRONTIER-CONTINUATION` / `00f1306f78e243e8abfb2681356c24e3` Passed（56.32s），原成员捕获候选后最终仍死亡51，第二成员用该候选续搜T12/7损/2瓶，13,669.78ms、16,978/180,361；两个各30秒成员不是30秒总预算。预测见证不代表原生T2或整场执行；前缀仍固定了玩家生成能力的两步，不能作正常开局质量验收。实机与冻结根不变、实例删除；这些隔离模式不证明正常编排。

历史未约束候选`4127ebbf89c04d3d98e3c8a7d6399010`曾7损获胜，但请求超120,000节点，已撤回。此次补上开局后验逐成员请求扣账后，正常基线`bd2eeabffa8e4bce94338bbf7eeb02da`与续搜候选`d6f2597f6d9c473fa59d175e76c70b7d`均Passed；原ZIP/start、原120秒profile/开关、仅固定预算覆盖、3GiB，完整根/政策相同，两侧恰好120,000节点。死亡51→T12胜利/7损/2瓶；总搜索50,394.77→53,715.38ms、转移740,320→830,137。复用一个已有生成能力名额、优先自生首回合续搜，不增加配置容量；耗时该对+6.6%，不作广泛性能结论。

独立O005 `29efedaeae4c48cebc11b5f830c57e2b`与当前已验收基线同根/原5秒政策/3GiB，仍T9/8损/无药，总搜索14,241.56→11,991.03ms；该哨兵覆盖共享协调器，未进入本次开局后验。O004原生`2aff815a9b154ca486a1899c3aa7866a` Passed（88.69s），同根/政策/全部动作，Instant/0秒、T12实际7损/治疗和自伤0/2瓶/四类计划外重算0，恰好120,000节点。首次原生`4754791bd5364eb78add3d531a491614`访问冲突退出，无result且原始日志已清理；后一次同条件留日志通过，首次故障原因未定。Release及职责门禁通过，自有实例全部删除；5损目标与Linux未验证，完整范围见[证据](issues/q002-route-quality.md#o004-请求账本与自主首回合续搜2026-10-04)。

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

0.49.0 的行为验证沿用本页 PR #203、#204 合并验收与 [战斗状态修复验证](archive/testing/volume-13.md)。版本及发布文档调整采用 L0 检查和发布构建；原有未验证项保留。

## 移动运行库内存回收（2026-10-04）

`portable-runtime` 先在原回收逻辑复现 Mono 同形的 API 拒绝，修复后 5 项 Passed。直接链接生产代码并注入被拒绝的按类型 GC 信息接口，验证一次检测后不再调用、普通检查点及不可分割提交续行、取消、自动及手动真实阻塞回收、不可用暂停观测和其他异常继续传播。

```bash
dotnet run --project tools/testing/checks/CombatSolver.GcPolicyChecks/CombatSolver.GcPolicyChecks.csproj -c Release -- portable-runtime
```

桌面实际 CLR 相邻合同 `default-commit` 2 项、`checkpoint` 1 项、`diagnostic-failure` 8 项、`recovery-lifecycle` 3 项 Passed。模式均由同一 GC 工具运行，方法见[工具入口](../tools/testing/checks/CombatSolver.GcPolicyChecks/README.md)。

原生 `B013-DEFAULT-GC-LIMIT` Passed，runId `ac4ee495b5ad48158c0d709a49b0abd2`，22.883 秒；包含限额、真实回收续行、退出和暂停观测缺失时的工作量累计。PowerShell 入口为 `tools/testing/run-unattended-test.ps1 -ScenarioId B013-DEFAULT-GC-LIMIT -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit`；Bash 对应 `tools/testing/run-unattended-test.sh --scenario-id B013-DEFAULT-GC-LIMIT --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit`。实例已删除。

原 Android 设备与原包整场回放未执行；来源、失败基线及验证范围见[开发记录](archive/development/volume-15.md#移动运行库内存回收2026-10-04)。

## 0.49.1 日志站硬逻辑（2026-10-04）

强制结束回合选牌、群体 Power、死亡金币回调、延迟能量／等离子球、开局小刀、回合末自动出牌、行动意图、资源隔离、反应格挡和界面归属的原生差分见 [逐类验收](issues/0.49.1-hardbugs-20261004.md#原生验收证据)。该记录保留失败基线、runId、复跑入口、第三方边界及未验证项；生产部署合同包含增量验证和计划外重算断言。

## 0.49.2 内容性 Mod 失败分类（2026-10-04）

`CONTENT-MOD-FAILURES` 与 `VerifyPredictionFailureBoundaries` 同进程 Passed，runId `849fb38580474f7881c05d113beef5d3`，24.505 秒。合同直接调用五个回合阶段、三个金币回调与计算型变量的生产拒绝入口，断言确认的第三方来源、原生回调保持未执行、包装异常、eng/zhs/zht 的 Mod 名称与方括号转义、四类失败账本仅记录暂未适配且不触发上传。原版、共享计算框架与运行库失败继续提示诊断上传；平台接口错误保留主失败类别。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CONTENT-MOD-FAILURES -EnemyCurrentHp 1000 -VerifyPredictionFailureBoundaries -TimeoutSeconds 120 -CleanupInstanceOnExit
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id CONTENT-MOD-FAILURES --enemy-current-hp 1000 --verify-prediction-failure-boundaries --timeout-seconds 120 --cleanup-instance-on-exit
```

来源与验证边界见 [开发历史卷 14](archive/development/volume-14.md)。最终行为源码在版本同步前通过；后续只改版本元数据和文档，采用最终 Release 构建。测试启动器已删除实例，未执行可见 Steam 弹窗排版验收或第三方原包整场回放。

## 0.49.1 内存提交回归（2026-10-04）

`CombatSolver.GcPolicyChecks` 直接编译生产 Runtime 内存代码。`default-commit` 在未改生产代码时 Failed：显式回退后分配上限仍有效；修复后 2 项 Passed，覆盖普通检查点保留限额、不可分割提交完成回收后续行、后续请求重新建立限额与取消保持原准入。

```bash
dotnet run --project tools/testing/checks/CombatSolver.GcPolicyChecks/CombatSolver.GcPolicyChecks.csproj -c Release -- default-commit
```

相邻真实 CLR 回归：`default-entry` 2 项、`recovery-lifecycle` 3 项 Passed，包含实际 NoGC 退出与恢复、显式退出持续生效及诊断失败清理。每项设有 15 或 20 秒截止时间。未重放 18 份原包整场，也没有可见 Steam 或低内存宿主性能结论；[排查记录](issues/0.49.0-memory-commit-regression-20261004.md)保留固定报告身份与触发窗口。

## PR #203 的机制合同

金币、最大生命回复与遗物间接回复的贡献者证据见 [专题记录](performance/gold-max-hp-healing-20261003.md)。独立场景为 `GOLD-HEALING-MECHANISMS`、`MAX-HP-HEALING-CALLBACKS`、`FEED-MAX-HP-CAP`、`RELIC-MAX-HP-HEALING-BOUNDS` 和 `AXEBOT-JOSS-DEFERRED-FORK`，均从平台原生无人入口运行，使用 IRONCLAD、FUZZY_WURM_CRAWLER_WEAK、120 秒上限及实例清理。原作者结果与本轮合并验证分别记账。

金币和回复合同同时传 `-EvidenceDirectory .local/validation/pr203/<场景>`（Bash：`--evidence-directory`），初始生命为 50/80。金纸合同还传 `-RelicsPath coverage/fixtures/regressions/axebot/axebot-joss-deferred-relics.json`（Bash：`--relics-path`）。搜索阶段顺序合同为 `KNOWN-HEALING-MEMBERS` 和 `KNOWN-HEALING-OPENING`，使用 SILENT；受影响的固定前缀合同使用 IRONCLAD，强制结束回合牌合同使用 REGENT。

## 当前矩阵命令

矩阵启动器只读取本节的平台原生命令；历史记录不作为自动执行清单。以下两项分别检查倾泻手空边界与相邻效果作用域，不运行整场搜索。需要当前游戏及 RitsuLib 路径，矩阵启动器统一管理实例并在结束时清理。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CASCADE-EMPTY-HAND-NATIVE -EnemyCurrentHp 1000 -HeadlessFastModeForTest Instant -DeploymentFastModeForTest Instant -DeploymentInterActionDelaySecondsForTest 0 -TimeoutSeconds 120
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId EFFECT-SCOPE-ADJACENT-CONTRACT -EnemyCurrentHp 1000 -HeadlessFastModeForTest Instant -DeploymentFastModeForTest Instant -DeploymentInterActionDelaySecondsForTest 0 -TimeoutSeconds 120
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id CASCADE-EMPTY-HAND-NATIVE --enemy-current-hp 1000 --headless-fast-mode-for-test Instant --deployment-fast-mode-for-test Instant --deployment-inter-action-delay-seconds-for-test 0 --timeout-seconds 120
./tools/testing/run-unattended-test.sh --scenario-id EFFECT-SCOPE-ADJACENT-CONTRACT --enemy-current-hp 1000 --headless-fast-mode-for-test Instant --deployment-fast-mode-for-test Instant --deployment-inter-action-delay-seconds-for-test 0 --timeout-seconds 120
```


## 0.48.0 硬错误机制合同

这些场景各自停止在共享首因或必要跨回合边界。完整runId、失败基线和未验证项见[历史卷13](archive/testing/volume-13.md)，逐包分类见[问题记录](issues/0.48.0-hardbugs-20261003.md)。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId NATIVE-CHOOSE-OPEN-GATE -CharacterId SILENT -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CALCULATED-GAMBLE-ORDERED-DISCARD -CharacterId SILENT -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FROZEN-LIGHTNING-CHANNELS -CharacterId DEFECT -EncounterId MECHA_KNIGHT_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId REPORT-ROUND-IMBALANCED -CharacterId NECROBINDER -EncounterId BOWLBUGS_NORMAL -InitialPlayerHp 200 -InitialPlayerMaxHp 200 -ClearPlayerHand -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId SECOND-WIND-REPORT-ROOT -CharacterId IRONCLAD -EncounterId DECIMILLIPEDE_ELITE -InitialRoundNumber 3 -InitialPlayerTurnNumber 3 -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId GROUP-DEBUFF-REACTIVE-DRAW -CharacterId NECROBINDER -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CONSTRUCT-REAPER-ARTIFACT -CharacterId NECROBINDER -EncounterId CONSTRUCT_MENAGERIE_NORMAL -InitialRoundNumber 3 -InitialPlayerTurnNumber 3 -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FIXED-PREFIX-POTION-POLICY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FIXED-PREFIX-TURN-OUTCOMES -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId MIRRORED-HOOK-FILTER -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FROZEN-ROOT-LISTENERS -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CALCULATED-HISTORY-FREEZE -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId VOID-FORM-TURN-CHOICES -CharacterId REGENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FLATTEN-MUSIC-BOX-ENTRY -CharacterId NECROBINDER -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId SEEKER-ORDERED-OPTIONS -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId RITUAL-TEMPORARY-STRENGTH -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId NOXIOUS-RAMPART-ORDER -CharacterId IRONCLAD -EncounterId TURRET_OPERATOR_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId EVIL-EYE-EXHAUST-HISTORY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId BLOCK-SPEC-CARD-PLAY-IDENTITY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId ROUTE-ADOPTION-LIFETIME -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId PAELS-LEGION-FINISHED-REFERENCE -CharacterId SILENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId END-TURN-RISK-LOSS-ACCOUNTING -CharacterId DEFECT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId MAKE-IT-SO-FULL-HAND -CharacterId REGENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CARD-COST-IDENTITY-CONTRACT -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId DAMPEN-REACTIVE-ROCKET-PUNCH -CharacterId DEFECT -EncounterId KNIGHTS_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId DAMPEN-REACTIVE-MELANCHOLY -CharacterId NECROBINDER -EncounterId KNIGHTS_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId RADIANT-PEARL-ENTRY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id NATIVE-CHOOSE-OPEN-GATE --character-id SILENT --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CALCULATED-GAMBLE-ORDERED-DISCARD --character-id SILENT --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FROZEN-LIGHTNING-CHANNELS --character-id DEFECT --encounter-id MECHA_KNIGHT_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id REPORT-ROUND-IMBALANCED --character-id NECROBINDER --encounter-id BOWLBUGS_NORMAL --initial-player-hp 200 --initial-player-max-hp 200 --clear-player-hand --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id SECOND-WIND-REPORT-ROOT --character-id IRONCLAD --encounter-id DECIMILLIPEDE_ELITE --initial-round-number 3 --initial-player-turn-number 3 --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id GROUP-DEBUFF-REACTIVE-DRAW --character-id NECROBINDER --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CONSTRUCT-REAPER-ARTIFACT --character-id NECROBINDER --encounter-id CONSTRUCT_MENAGERIE_NORMAL --initial-round-number 3 --initial-player-turn-number 3 --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FIXED-PREFIX-POTION-POLICY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FIXED-PREFIX-TURN-OUTCOMES --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id MIRRORED-HOOK-FILTER --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FROZEN-ROOT-LISTENERS --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CALCULATED-HISTORY-FREEZE --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id VOID-FORM-TURN-CHOICES --character-id REGENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FLATTEN-MUSIC-BOX-ENTRY --character-id NECROBINDER --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id SEEKER-ORDERED-OPTIONS --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id RITUAL-TEMPORARY-STRENGTH --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id NOXIOUS-RAMPART-ORDER --character-id IRONCLAD --encounter-id TURRET_OPERATOR_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id EVIL-EYE-EXHAUST-HISTORY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id BLOCK-SPEC-CARD-PLAY-IDENTITY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id ROUTE-ADOPTION-LIFETIME --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id PAELS-LEGION-FINISHED-REFERENCE --character-id SILENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id END-TURN-RISK-LOSS-ACCOUNTING --character-id DEFECT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id MAKE-IT-SO-FULL-HAND --character-id REGENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CARD-COST-IDENTITY-CONTRACT --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id DAMPEN-REACTIVE-ROCKET-PUNCH --character-id DEFECT --encounter-id KNIGHTS_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id DAMPEN-REACTIVE-MELANCHOLY --character-id NECROBINDER --encounter-id KNIGHTS_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id RADIANT-PEARL-ENTRY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
```

## 社区批次 B013 回归夹具（2026-10-03，Refs #172）

以下为贡献者在 PR #204 记录的结果，与本轮验证分别记账。五个主题的独立机制回归在 `src/Testing/Regressions/Community/B013*Checks.cs`，命令统一为
`tools/testing/run-unattended-test.ps1 -ScenarioId <ID> -TimeoutSeconds 120 -CleanupInstanceOnExit`（角色/遭遇按夹具要求，详见 PR #204）：

- `B013-BLOCK-DECIMAL-BOUNDARY`（T010，格挡乘法 Decimal 边界）：修改前 `dec22b8f` Failed（栈同报告）→ 修改后 `e2757311`/`a23dcf21` Passed；代表包 DOP1 哨兵路线与展开/转移一致。
- `B013-FIXED-PREFIX-TERMINAL-BOUNDARY`（T008，固定前缀越过锁定终局）：`971ccdd1` Failed → `0b3f96a6` Passed；代表包哨兵 22809/129762、终局一致。
- `B013-FIXED-PREFIX-TURN-END-CARD`（T007，结束回合卡固定前缀）：`eb64242d` Failed → `899ef881` Passed。
- `B013-RADIANT-PEARL-HAND-DRAW`（T006，抽牌前遗物生成对账）：`cd227ff6` Failed → `15c80923` Passed；代表包 SearchOnly 失配 → `TURN_SETUP_STATE_MATCH`。
- `B013-DEFAULT-GC-LIMIT`（T009，默认 GC 请求分配边界）：`6ac0f466` Failed（`limit=long.MaxValue`）→ `7c6f3b83` Passed。

变基到 0.48.1（`2ead87d9`）后五夹具复跑 Passed：`0f19174f` / `57e2086d` / `9d7173af` / `3892e307` / `fdfc8734`。代表包哨兵在新基线：T010 DOP1 与旧基线逐字段一致（展开 7426、转移 77129、4 回合胜、finalHp 71），耗时 3458.9ms；T009/T006 代表包 7 回合胜、finalHp 88、boundary None（展开 26194、转移 190351，路线随上游搜索改动微调）。

完整修改前后、哨兵与未验证项见 PR #204；批次状态与剩余项（T007 第二样本未复现）同样记录在该 PR。

## PR #203、#204 合并验收（2026-10-04）

以下为本轮直接运行结果；全部使用120秒上限和实例清理，已通过的请求未重复运行。无头验证停在对应机制边界。

| 场景 | runId | 结果与范围 |
| --- | --- | --- |
| `GOLD-HEALING-MECHANISMS` | `3bc7928290c343cca88248c0c28dd95e` | Passed；金币修正、三个回调、完整原生状态/RNG和父分支隔离 |
| `MAX-HP-HEALING-CALLBACKS` | `cd6549478924451882fc600e67724a53` | Passed；最大生命实际增量、封顶与回复回调的原生差分 |
| `FEED-MAX-HP-CAP` | `ddcf661e391c4013a0aa87861f8542f3` | Passed；两次致命出牌的实际增量与成长计数 |
| `RELIC-MAX-HP-HEALING-BOUNDS` | `bee94b31824d449e8f7e9fa55cbe94f5` | Passed；两项直接原生遗物回调与熔化来源排除 |
| `AXEBOT-JOSS-DEFERRED-FORK` | `0fd9902a8a594e36a0a9fbf533ceee65` | Passed；根计数、零状态指纹、父子/兄弟Fork与逐分支消费 |
| `KNOWN-HEALING-MEMBERS` | `fab7a8170a3942e9920e09fa466aa34e` | Passed；严格增量、DOP2、控制质量、成长门与live隔离 |
| `B013-FIXED-PREFIX-TERMINAL-BOUNDARY` | `5b097fd5f17745a0868a6b49d7af3b37` | Passed；终局前缀截断与可交付胜利 |
| `B013-FIXED-PREFIX-TURN-END-CARD` | `c778f285a52a4b9fbbe0e370688e6ec1` | Passed；强制结束回合卡的固定前缀推进 |
| `B013-RADIANT-PEARL-HAND-DRAW` | `ca904a20eafd43b690d8c824f60e2069` | Passed；抽牌前生成的原生数量、升级与归属对账 |
| `B013-DEFAULT-GC-LIMIT` | `3e440c65f9564b5ea6a65d094521b8d7` | Passed；限额、真实Gen2回收续搜、退出清理与CLR模式 |
| `KNOWN-HEALING-OPENING` | `3fecd35f80a34cb792a43ca3b5bc3b1c` | Passed；非认证根延后计划、单次执行与控制质量 |
| `FIXED-PREFIX-TURN-OUTCOMES` | `b18d23d1829b4438a557a5f75acf6425` | Passed；独立前缀完整状态oracle、多回合结果与终局截断 |
| `B013-BLOCK-DECIMAL-BOUNDARY` | `b556026e627c4153bb3d2bf23948a095` | Passed；原生虚弱倍率、完整状态/RNG；模拟96/95/200层与零格挡 |

真实CLR工具：`default-entry` 2项、`diagnostic-failure` 8项、`scopes` 8项 Passed。入口失败基线为日志抛错后信号仍启用；修复后同异常传播、信号清理及后续独占准入均通过。

失败记录：首次无头启动在私有进程身份检查处失败并清理，未进入游戏测试，原因未确定；金币首轮14项对账已通过，但缺EvidenceDirectory导致产物写入失败；金纸首轮缺遗物输入。补齐请求参数后仅重跑失败请求，成功记录在上表。固定前缀09f94286012d420d81242f480ebd1803仍执行旧的终局拒绝断言，合同更新为开局探测和完整搜索共同截断，并断言终局动作数及回合。新增格挡夹具初次编译因原生调用参数及私有setter失败，修正后Release零警告/错误。

L0：原生回复审计工具迁入tools/inspection后构建和真实DLL扫描通过。CoverageCatalog重新生成3035项目录，状态字段未分类为0；`--verify-state-writes`仍因既有InfusedCore.AfterSideTurnStart缺运行证据失败（1项）。PowerShell结构检查247文件通过，工具检查290文件/37项目通过，文档427文件/1635链接及覆盖目录检查通过。额外Bash结构检查因运行耗时停止，未完成；两平台脚本静态语法检查通过。

范围：本轮没有重跑作者长预算全根性能筛查，没有验证低内存玩家宿主、原生整场部署或可见Steam性能；原作者失败与未验证项保留在所属报告。
