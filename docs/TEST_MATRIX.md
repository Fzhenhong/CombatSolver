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

固定 29 根 / `6f4d8f6e` 回归保留亡灵投影 10→17 战损的拒绝结果及储君首领 +40.04% 峰值样本。旧路线 88 前缀完整状态不变，金纸零待结算计数改变了指纹；候选修复保留零状态的原键并显式编码非零计数。`AXEBOT-JOSS-DEFERRED-FORK` / `6bf2b7299eae440ba42c87d18f27f290` Passed，覆盖原有非零父子计数和新增零计数区分、逐分支消费及隔离；参数沿用上方金纸夹具，120 秒帽、清理实例，不运行 Solve。最终无插桩交错对照四轮均为 10 战损/2 瓶/12 回合，峰值最大值增加 2.48%；只有该根进入修改的金纸分支，其余 28 根复用未受影响证据。储君首领兼容宿主交错对照峰值增加 1.48%，接口不兼容的失败对照不计入验收。完整来源、逐项结果与限制见[专题证据](performance/gold-max-hp-healing-20261003.md)。

拟提交 DLL `89a8e2c2…` 的受试体无插桩串行交错四轮全部获胜；基线 146.40/153.18 秒、26 战损/0 瓶，候选 47.58/48.32 秒、24 战损/0 瓶。中位提速 3.12 倍，保守最慢候选仍为 3.03 倍；最大候选峰值/最小基线峰值增加 7.68%，离线门槛通过。完整根、牌序/RNG、配置、预算和目录一致；原生整场部署仍未完成，不扩大到其他慢根。

[组件与间接回调审计](performance/native-healing-component-audit-20261003.md)追加当前 DLL 的44项具体效果和80项来源哈希，以及45项战斗结束、18类怪物直接造牌、62种直接施加Power的定位证据。没有新增运行时认证或原生合同；静态无生命路径不能判为安全。此批只做L0文档/数据口径检查，完整可达性、组件证书及认证开销仍未完成。

[根覆盖诊断](performance/native-healing-root-coverage-20261003.json)使用已验证`89a8e2c2…`捕获29个原固定根，不执行Solve；全部根文本与既有回归一致，live不变。严格剩余环境资格6/29、已知来源策略29/29；先有的两个目标诊断直接复用。旧回归的主胜利界计数按E44原DLL来源复用，不能称为新证书或协调器总剪枝。隔离宿主首轮编译缺少Dispose入口、首轮CLI误用`--output`的失败均未启动搜索；修正仅在忽略目录的诊断代码。此时新组件尚未准入；后续新增组件合同、拒绝统计及独立认证开销见下条。

`COMPONENT-HEALING-BOUND` / `7aad45400e0a4cbab3cf1c090805e494` 与 `COMPONENT-SMART-BOUND` / `1f1fe45db2ec480d98516b063f9a8a38` Passed：四项完整原生用药状态、再生叠加/禁药/额度、全部牌堆和永久牌组未知来源拒绝、父子/live/RNG；完整无药胜利及Smart精确层7次/开局后续2次实际剪枝、政策门禁、DOP2严格增量与协调器。120秒帽，实例删除；前置失败与未达两倍原型保留。源码相同的合同复用至拟提交DLL `f266bf00…`，两根最终ABBA完成，目标根中位67.48倍且质量一致/峰值−91.87%；完整29根固定回归完成，女王交错对照候选两次NoWin，质量阻断保留；受试体补充ABBA中位3.00倍/战损26→24/峰值+6.86%通过，原单次+10.76%样本保留，未称全回归通过。复跑和范围见[组合上界](performance/component-healing-bound-20261003.md)。

`COMPONENT-FINAL-DEV10-FROZEN-DEPLOY` / `19c246e2aa6b401780644c1f498d10f8` 原生结果Passed，完整初始根与离线B1相等，14战损/0瓶/2回合、无意外重算；启动器清理退出码1，独立确认PID及私有实例不存在。隔离研究 `COMPONENT-SILENT-POTIONS` / `d99199c3d427412382a904ae17d31802` 的两项完整用药状态/Fork/双顺序差分Passed，但无插桩整请求初筛23.49秒未改善，原型未纳入生产版本。详见上方组合上界报告及JSON。

女王追加组件合同 `81f5705b0d3c481e954b7f1a014ce638` Passed，但完整初筛298.77秒NoWin。隔离只读续接Fork的原生16线程合同 `654a913c7a7c48e9b94721bd868bbcb9`、搜索收尾 `ce76832c0c0a48aab8fe3a6440b87baf`、严格增量 `c97962ac8ab3488f95004f7558bd2153` 均Passed并清理实例；完整初筛女王294.85秒Win67/0/T12，未达两倍、未完成固定回归，未纳入生产源码。

隔离普通/回合Fork合同 `60011a63755b40da868b8b09c3fdd444` Passed并清理实例：非空历史尾部封存、16线程完整状态/RNG、子修改和父/live隔离、原生出牌/抽牌、非严格/严格搜索收尾。完整请求初筛女王300.29秒Win67/0/T12、猎手精英24.27秒Win40/0/T4，均未达两倍；没有最终交错或完整固定回归，不纳入生产源码。

隔离变量表复制合同 `691669b8c7c844d4aed28279c79f7f08` 及具体对象许可合同 `9d30b70de8034ef487d335be285f1a30` Passed并清理实例，覆盖原生模型/变量状态、0/1/2/4/9变量、拥有者、并发克隆、元数据、未知补丁/字段桥回退及live/RNG。完整请求A/B/对象许可初筛23.62/23.44/23.61秒，工作数、Win40/0/T4相同，无实质收益且未达原始两倍；无最终交错或完整回归，未纳入生产源码。

隔离跨成员转移诊断三次结果保留。最终同前缀哈希/动作数/历史长度分组124617次跨成员重复，无记录标签差异；宽松分组326次历史差异。只统计机会，未验证完整状态复用或性能。储君首领六项新类型和四项同DLL复用来源只有源码审查，新增组合资格、原生/Fork差分及性能均未执行；见上述回复上界与组件审计报告。

储君首领组件研究：`COMPONENT-REGENT-BOSS` / `642030889ba7496f9ddfb01e2bd90b69`，30.21秒Passed，原生初始/生成来源、两药水、遗物回调、Frantic及流沙强制死亡、完整状态/Fork/父/live/RNG；通知研究：`HYBRID-MAILBOX-CONTRACT` / `8e59254a56f04f9c872d9d3ad0b56ef8`，60.08秒Passed，串行/并行、严格增量、DOP2/16取消/异常与部分工作排空。实例均删除。各自无插桩完整初筛16.76/22.01秒未达原始2倍，通知单次峰值相对f266增加75.55%；未纳入生产，未做最终交错/全固定回归/目标整场，见[报告](performance/component-healing-bound-20261003.md)。

分配研究：`ALLOCATION-LAYOUT-CONTRACT` / `9bf3158cf4d24fbfad0be546d27f1827`，34.77秒Passed，原始怪物指纹排序、键新增/值覆盖、不可变键表Fork、完整父/live/RNG、63监听方法组/1672模型、串行/并行及严格增量。初次`00640b97736c47c38801e82e8f34ce49`因旧GoldCallbacks断言Failed，方法组修正仅在隔离夹具，两个实例均删除。女王无插桩完整初筛295.85秒NoWin、原始1.05倍，未纳入生产或继续交错/回归/目标整场，见[报告](performance/component-healing-bound-20261003.md)。
