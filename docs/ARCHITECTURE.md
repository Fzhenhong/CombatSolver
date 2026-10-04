# CombatSolver 架构与职责地图

本文只维护当前所有权、依赖边界和源码入口。实现过程、固定根结果和性能数字见 [历史资料](archive/README.md)。持续工作规则见 [AGENTS.md](../AGENTS.md)。

## 1. 运行链

主线程捕获稳定战斗根 → 冻结请求政策 → 后台分支搜索 → 主线程接收结果 → 原版公开入口部署当前回合 → 续用戳核对或重新捕获。

后台只读取根与分支状态；真实战斗对象只用作稳定身份或只读模型元数据。未知语义形成明确边界或失败。

## 2. Runtime

| 入口 | 所有权 |
| --- | --- |
| `src/Runtime/Entry.cs` | 初始化、战斗生命周期接线 |
| `SolverController.cs` | 主线程请求、结果接收、续用、部署和自动执行 |
| `SolverControllerSessions.cs` | 战斗、搜索、部署会话生命周期 |
| `CombatRootSnapshot.cs` | 在主线程捕获并核对稳定根 |
| `ContinuationStamp.cs` | live / predicted 跨回合一致性和字段差异 |
| `SearchGcPolicy.cs` | 进程 GC、NoGC 和跨战斗回收协调 |
| `SearchMemoryPressureSignal.cs` | 注入 Search 的分配边界与回收续搜信号 |
| `NativeChoiceRuntime.cs` | 原生选牌观察与逐实例计划匹配 |
| `PlayerTurnSetupPatches.cs` | 原生回合准备、选择和部署交接 |

政策由主线程从设置冻结到 `SearchPolicySnapshot`。后台通过请求参数、诊断 sink 与压力信号消费外部能力；算法不读取设置单例或操作 GC。Power 显示变量在主线程根捕获时物化；worker 只消费已物化值。克隆并发边界由 `BaseLibCloneConcurrencyPatch` 与原生克隆隔离规则维护。

## 3. Search

| 入口 | 所有权 |
| --- | --- |
| `CombatSearchCoordinator` 与各分片 | 主 Pass、药水审计、组合成员及后处理编排 |
| `SearchRequestPipeline` | 请求级阶段顺序与停止条件 |
| `SearchPassContext` / `SearchPassResult` | 单轮冻结输入及返回合同 |
| `SearchBudgetLedger` / `SearchRequestWorkTotals` | 请求时间、额度和唯一工作量累计 |
| `FrontierContinuationScheduler` | 固定前缀成员派发与用途归因 |
| `CombatBeamSolver.cs` / `.Models.cs` | 不可变根配置、策略接线、节点和运行上下文 |
| `.Phases.cs` / `.Expansion.cs` | Solve 阶段和动作回放入口 |
| `.ParallelExpansion.cs` / `.AdmittedExpansion.cs` | 固定 worker lane、已准入作业及确定性提交 |
| `.PrimaryChoiceReplay.cs` | 原预算必经的首层选择回放与暂存 |
| `.BeamRetentionPolicy.cs` | 去重、Beam、多样性、选择保路、药水配额与 Pareto |
| `.FinalPlanOrdering.cs` / `RouteQualityPolicy` | 完整路线质量与各既有投影顺序 |
| `.StateEvaluation.cs` / `.Terminal.cs` | 评分特征、终局回放和回合结果 |
| `SimulatedCombatState*.cs` | 分支战斗状态与动作语义 |

成员共享原请求账本；预算准入、候选合法性和取优由所属策略决定。分支状态不承担 Beam 政策；中途保路和终局比较保持明确入口。药水反事实与强制用药的硬准入先于质量比较。固定前缀构造实际父链，EndTurn 从模拟前后状态生成 `TurnOutcome`。

`CombatSearchCoordinator.Audits` 的智能开局药水补搜逐成员扣除请求剩余节点和时间。无药路线只有死亡、尚无完整胜利时，最后一个生成能力成员可复用同药水选择成员自生的完整首回合动作，并在其他复合前缀前调度；缓存只存纯值动作，不持有节点或实机状态。仍占原成员名额，沿既有前缀回放重建调度基线、普通排序及药水资格比较，不增加配置容量。

`PowerCommitmentRetention` 按机制族、登记能力集合、药水数量与回合选择有界代表；能力激活顺序不重复占席，不同能力集合不因族相同而合并。Beam 只保护实际代表，其他能力节点不预占代表配额；替换保持容量和既有必保节点。完整状态键、转置支配及终局政策不消费这种启发式分组。显式路径观察只复制纯值承诺及独立字符串数组，不保留节点或模拟器。

## 4. 模拟与 Prediction

| 位置 | 职责 |
| --- | --- |
| `src/Engine/InCombat/Simulation/` | 通用命令时序、历史、RNG、牌堆、伤害和 Fork |
| `src/Engine/InCombat/Mirrors/` | 原版 Hook / Model 方法镜像 |
| `MethodMirrorRegistryDescriptor` | 向 CoverageCatalog 暴露 registry 支持元数据 |
| `src/Prediction/` | 怪物 AI、隐藏状态、生命周期、死亡/召唤、选择与 subscriber 捕获 |

每项战斗语义只有一个权威结算实现。可变值由根快照、影子状态、克隆 Model 或 `PredictionStateStore` 持有。一次 Fork 共用 `PredictionForkContext`，引用随同一上下文重映射；COW 取得可写所有者后再写。

Fork 发生在动作、选牌、Power、死亡和出牌事务允许复制的稳定边界。玩家记录的卡牌/药水嵌套效果作用域由引擎持有；选牌检查点保存不可变身份序列，恢复建立分支独占列表。最外层效果结束后再检查手空。稳定根及跨回合状态的该作用域为空，普通 Fork 要求为空。

未知 gameplay subscriber 显式拒绝；已支持来源在主线程捕获，并在分支中消费隔离状态。登记合同与封闭入口见 [第三方适配手册](third-party/README.md)。

## 5. UI

`SolverOverlaySnapshot.Capture` 是结果到展示的唯一投影边界，可读取 `SolverResult` 与显示元数据。`SolverOverlay`、`SolverRouteRow`、`SolverActionPill` 只渲染只读 snapshot。

路线身份、选择、目标与显示值由主线程投影；UI 不持有搜索分支或从 `ModelDb` 重新解释路线。设置输入由相应面板持有，Controller 负责政策冻结和续用失效。中英文本同时维护。

## 6. Testing

| 入口 | 所有权 |
| --- | --- |
| `UnattendedTestRunner` | 请求级编排与共享 fixture helper |
| `ProtocolHost` | 请求循环与每请求开关 |
| `ScenarioBuilder` | 建局与状态注入 |
| `Executor` | 差分、搜索、部署执行及临时设置 |
| `Assertions` | 执行前后断言 |
| `Writer` | 结果协议和原子写入 |

原生与模拟核对完整状态、顺序、引用、RNG 和续用合同。离线宿主只产搜索指标；headless 不证明真实可见布局或帧时间。入口见 [无人测试](HEADLESS_TESTING.md)、[离线宿主](OFFLINE_SEARCH_HARNESS.md)、[测试证据](TEST_MATRIX.md)。

检查点完整搜索 profile 由 `ProtocolHost` 在请求内持有，Runtime 捕获政策时读取其不可变记录；请求结束清除。显式 CLI 扰动沿既有入口覆盖单项，不将测试 profile 写入玩家设置或后台读取器。

保存预测路线的读取、严格回放、路径观察及原生终局断言属于Testing；生产Search不识别报告或测试场景。`UnattendedTestRunner.Q002PotionPosterior`只诊断一个既有开局用药补搜成员：严格核对原生生成/打牌状态及冻结根，固定前缀的药水上限和排序对照不代表协调器自主发现。部署沿现有Runtime入口，跨战斗结束的真实后台搜索代次只通过`SolverController.Testing`只读暴露；不以会话清零计数或已清空的临时账本推断终局。

`UnattendedTestRunner.Q002BoundaryMembers`复用Testing的原生T3/T4边界证明，对固定准确前缀的搜索进展基线重建做对照；完整根、实机不变和成员profile写入测试产物，不向生产提供报告牌序。

能力路线组合的候选与名额选择仍归`CombatSearchCoordinator.PowerRoutes`：安全且有明显损血的无新增用药胜利可将一个已有变体名额用于自身首回合结束处续搜，动作来自当前搜索结果；既有调度器负责严格前缀回放与启发式基线重建，最终整场政策比较不变。

## 7. 工具与维护

`tools/verify-refactor-boundaries.ps1` 和 `.sh` 维护同一职责边界。CoverageCatalog 从公开描述与结构化证据生成覆盖报告，报告的生成版本与测试来源分别说明。

修改职责时在同一提交替换本文对应章节，并同步相关 skill 与结构门禁。开发进度写入当前开发记录，测试细节写入证据，历史报告冻结归档。
