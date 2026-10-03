# 金币与最大生命回复链：原生差分和剪枝边界

本阶段接续现有回复审计，合并上游 `56b6d6ee` 后修复金币回调及实际最大生命增量。版本保持 0.48.0。原生语义验证已通过，最终性能与部署待下节更新；不声明新增两倍提速。

完整逐项结果和源码哈希见[结构化证据](gold-max-hp-healing-20261003.json)。前期[来源审计](native-health-source-audit-20261003.md)与[0.48.0 回归](upstream-0480-merge-check-20261003.md)保留取得证据时的版本、失败和限制。

## 当前版本原生语义

依据已安装游戏 0.111.0 / `41cef1ea`，DLL SHA-256 `2b40d2df538db1ceb5fa48d958c80ab730ada1e07db88a870aff01a661768b9f`，MVID `8a76776c-0ce1-4d4f-90bd-8cce653dad8e`。复用此前定向反编译证据；补充 HandOfGreed、BowlerHat、Ectoplasm、RunState、IRunState、RelicModel 的版本绑定来源。

| 来源/入口 | 对象、时点、条件与次数 | 本阶段处理 |
| --- | --- | --- |
| PlayerCmd.GainGold | 先用战斗 child 作用域 Modify；整数部分变化的修正者按原序 AfterModify；修正后 decimal > 0 才获得金币并 AfterGain | 记录截断后的实际金币；正小数获得 0 金币仍触发回调 |
| DragonFruit.AfterGoldGained | 仅自身玩家、未熔化；每次合法获得金币增加 MaxHp 并同步回复实际上限增量；可重复 | 持有时保守无限回复界，未证明战斗总触发数 |
| BowlerHat / Ectoplasm | 自身金币依次乘以 1.25 / 归零；原序影响结果；AfterModify 只有展示效果 | 实际金币用于长期资源记录；明确审计的展示回调省略 |
| RunState.IterateHookListeners(null) | AfterGain 使用跑局监听序列：根永久牌/附魔、活动玩家遗物与药水、全局来源、run subscriber | 根冻结成员；遗物/药水消费分支状态；Fork 共享只读根成员；不读后台 live |
| CreatureCmd.GainMaxHp | 非负 decimal，整数化并封顶 999999999，再 Heal 实际 MaxHp 差值 | 引擎单一 helper，保留 HP 变更回调 |
| HandOfGreed / Feed | 合法致命效果，复制/重放依各次原生出牌结算；初始持有与回收另属可达性 | 保留成长计数；金币按修正后实际整数记录；Feed 按实际封顶增量回复 |
| FruitJuice / RedSkull | 实际用药增加最大生命；越过半血阈值时 HP 回调更新力量 | 药水复用同一 helper，执行 Prepare/Complete 原路径 |
| ChosenCheese / DarkstonePeriapt | 分别在战斗结束增加上限 1、永久牌组加入自身诅咒时增加上限 6，均同步回复 | 缺少完整回调和获取闭包时保留完整余量；只排除已熔化的来源 |
| 未审计金币 override | 修改/通知/获得任一入口 | 显式拒绝；不能用非 gameplay manifest 的 Ignored 标签证明安全 |

DragonFruit 缺席或熔化只能证明这一机制贡献为 0，不能证明其他来源没有回血。严格证书没有扩面；已知来源策略补上实际持有的 DragonFruit，继续区分策略估计与封闭证明。它可能降低剪枝率，正确性优先。

## 最小原生验证

负基线 `88843d10d5b94471ad61d36d0efd20f4`：持有 DragonFruit 获得 20 金币后，原生 51/81、预测 50/80，金币均 157。严格证书拒绝正确，已知来源策略及模拟漏了间接回复。

| 合并后 scenario | runId | 证据 |
| --- | --- | --- |
| GOLD-HEALING-MECHANISMS | `c68a0c923fa84f48b9f4ce1ee9f68831` | 14 项检查：无回复、负/零/正/小数金币、两种修正顺序、熔化、三入口未知拒绝、两个真实致命打牌 |
| MAX-HP-HEALING-CALLBACKS | `1e611f0aa1624d908d7c4f07d893fde4` | 8 项：零/小数/正数、部分/完整封顶、非 owner、RedSkull 阈值、真实 FruitJuice 用药 |
| FEED-MAX-HP-CAP | `de2062aa4fc847f98b7293a8d848d22b` | 两次真实致命打牌，实际上限增量 2/0；成长计数保持 |

三个请求均 Passed，覆盖完整状态/RNG、父分支/Fork 与 live 隔离；未知入口检查不执行回调。Linux 自有无头实例、IRONCLAD、初始 50/80、120 秒帽，均按清理开关删除。没有运行 Solve，因此不带增量搜索插桩。

复跑用 `tools/run-unattended-test.sh --scenario-id <表中 scenario> --character-id IRONCLAD --initial-player-hp 50 --initial-player-max-hp 80 --timeout-seconds 120 --exit-on-complete --cleanup-instance-on-exit`，具体种子和请求见结构化证据指向的本地原始材料。Release 构建零警告/错误，13.46 秒。

开发中的夹具失败与修正独立保留：10 项基础金币差分已通过后，原生熔化要求蜡制；随后修正 canonical 测试模型构造；未知 override 被通用 registry 归为 Ignored 的实质缺口改为显式拒绝。旧 Failed 请求没有改写为 Passed，最终证据取自上述合并后版本。

新金币 registry 所需的参与位图元数据补齐后，最终 Release 构建 13.66 秒、零警告/错误，两端结构门禁 `search_files=247`。三个金币方法共用一位，只保守保留成员，不影响精确方法分派；避免越过 ulong 位宽产生别名。受该变更影响的金币根 fixture 另以 `e45b785e58be408eb24a058cea44cddc` Passed；未重复未受影响的最大生命与 Feed 检查。

CoverageCatalog 已按金币 Hook 位图版本 DLL 实际重新生成，五个金币 Hook 条目与三个标准 descriptor 一致；状态字段和分支 live 读取的定向 verify 通过。没有运行全量 verify，其他既有运行证据缺口继续保留。

追加遗物边界负基线 `35f61a69acf74cc3b6cc56f6e5269f8d` Failed：ChosenCheese 的已知回复界为 0，原生 `AfterCombatEnd` 回调却将生命/上限从 50/80 增到 51/81。补齐 ChosenCheese 和 DarkstonePeriapt 后，`RELIC-MAX-HP-HEALING-BOUNDS` / `b16bb21caa4441bda3f931705fc0dddd` Passed：两项真实原生回调的完整状态/RNG、父分支/live 隔离和两项熔化排除通过，实际增量分别为 1/6。预测共用已通过的 GainMaxHp helper；没有声称模拟了永久牌组搬移或完整战斗结束派发，也没有扩大严格证书。该请求 27.70 秒，最终构建 14.06 秒、零警告/错误；已通过且未受影响的金币、Feed 和 FruitJuice 检查复用原证据。

## 最终回归与性能

固定 29 根完整协调器回归已中止：首根储君女王在 299.66 秒没有取得完整胜利，旧版 293.30 秒/67 战损/0 瓶胜利的质量门槛未通过。原始四个已完成请求均保留，未把宿主 Passed 当作质量通过；受试体 48.49 秒/24 战损/0 瓶胜利、猎手首领 46.21 秒/62 战损/1 瓶胜利、另一女王 31.44 秒/63 战损/2 瓶胜利均为单次筛查，不是最终性能验收。

诊断单变量移除上游新增的星能临时费用清理后，取得 302.28 秒/67 战损/0 瓶/12 回合胜利；这一旧处理版本没有提交、部署或用于正式成果。正确清理已立即恢复。当前旧版胜利路线在此前版本、最新版本及诊断版本均能完整影子回放：78 动作、67 战损/0 瓶/13 回合，79 个前缀状态键逐一相同，live 根不变；这只证明模拟回放，不替代原生部署，也不能说明正式搜索已经生成或保留该路线。

完整协调器仅跟踪上述状态的串行诊断中，旧版本 301.52 秒未获胜，新版本 300.95 秒取得 67 战损/0 瓶/12 回合胜利；两版都在第 5 回合生成旧路线的第 23 个前缀，保留至第 22 个，首次淘汰位置和策略标签相同。由此不能把先前失败归因于合并或金币补齐。诊断本身影响耗时和分配，均不计入性能验收；这些结果保留为搜索发现/保留与运行时调度的待隔离证据。新增两遗物边界后的候选尚未完成全根性能回归。

最终全根回归及必要的串行交错对照仍待通过。与既有原始基线保持根、牌序、九条 RNG、政策、极高配置、并行度 16 和预算不变，记录完整请求耗时及进程峰值；诊断与正确性插桩不混入验收。

当前未测量认证自身开销、分支认证覆盖率与拒绝分类，不把原生单效果通过当作性能结论。全部原版回复的九类审计、生成递归封闭证明、DragonFruit 有限触发总数及受试体原生整场部署仍未完成。原始慢根保持两倍目标；约 1.98 倍例外仅适用于用户上传的棱柱。
