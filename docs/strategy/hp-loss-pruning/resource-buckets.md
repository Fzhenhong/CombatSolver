# 成长与遗物目标的损血桶

2026-10-04；第四层补丁基线 `aeb66fba1`，CombatSolver 0.49.1 / 游戏 0.111.0。源码补丁为 `04-resource-buckets.patch`，接在[前三层补丁](README.md)之后。

下文记录第四层实现及其当轮结果。后续已加入[各成长目标独立基准与复制对象耗尽后的重新认证](growth-targets.md)，复制哨兵的最新对照见第五层记录。

## 实现与边界

共享表扩为 `(最终失窃数量, 显式用药次数, 各成长来源次数向量, 遗物达标掩码)`。例如一次 Feed 与一次 Royalties 不合桶；HappyFlower 达标与 PenNib 达标不合桶，即使总次数相同。遗物掩码表示具体目标组合，不区分同一达标区间内的每个计数值。额度、目标范围和优先级由同一冻结政策决定。

完整合规胜利发布实际资源桶，战略战损扣除本路线实际成长、遗物额度和既有条件回血信用。成长和遗物桶不污染普通 HP 标量界。跨成员共享全部已发现的桶；同根重复请求仍只携带所保留的完整零药路线及其对应桶，政策变化失效，不携带所有历史数值界。

未终局不能按当前奖励量直接剪枝。查询采用最乐观的最终桶：资源损失下界、成长次数上界、全部遗物目标达标掩码；损血下界同时预留这些目标的最高额度。只有该桶已有完整胜利见证、药水档封闭、节点无风险时才消费界限。没有最大目标桶的见证就保留节点。部分成长次数或部分遗物达标组合虽已独立入表，尚未证明后续不可能改善时，不会直接拿当前部分桶去截断。

遗物计数会继续变化、绕回或离开目标区间，因此不把“现在达标”当作永久达标。假设最终可以全部达标只是保守的收益上界；即使目标组合实际上不可同时达到，也只会少剪，不会提前排除更好路线。MeatOnTheBone 的实际回血沿既有回复上界预留，不再按目标额度重复扣一次。

冻结的 GrowthOpportunityTargets 是达标元数据，不能直接充当所有未来机制下的收益上界。第一版另行认证一个较小的原版牌组闭包：

- Feed、TheHunt、GeneticAlgorithm、TheScythe 必须具有消耗关键字；Royalties 必须是能力牌或具有消耗。遗传算法与镰刀还要求永久牌组版本。
- 上界由根中已经实现的收益加仍可用的相应牌实例数计算，不依赖根敌人数，避免后续召唤敌人使击杀上界失效。
- 普通搭配仅允许已列明的基础攻击、防御，以及原版状态/诅咒；不允许生成、变牌、复制或捞消耗牌。附魔、灾厄、待返回牌、被持有的偷牌、药水及非审计能力/遗物使认证失败。
- 遗物闭包目前为 BurningBlood、BlackBlood、RingOfTheSnake、BoundPhylactery、DivineRight、HappyFlower、PenNib、Nunchaku；玩家能力闭包为基础力量、敏捷、虚弱、易伤、脆弱、荆棘及 RoyaltiesPower。
- 已冻结的成长机会若标为不受限，即使另一个窄认证给出数值，也继续旁路。实际次数超过认证上界也拒绝剪枝。
- HandOfGreed、Alchemize、Goopy、ForbiddenGrimoire、MadScience、第三方来源、复制/重放/取回组合等没有在此版本建立完整上界，保留原展开。不是把这些来源的未来收益设为0。

只有消耗成长闭包成立且 Feed 的所有可能收益已兑现时，才释放不可取回的已消耗 Feed 的未来治疗预留。未认证、仍有可用 Feed、待返回或被偷回血牌继续保守处理。

等值截断扩展到已匹配的资源目标上界桶：可能舍弃同资源收益、同战损但更早胜利的路线，不承诺保留原完整排序。牌堆去重、RNG、Beam 排名和实际战斗结算没有随此次分桶修改。

## 本轮证据

主项目与离线宿主 Release 构建0警告、0错误。24项资源桶合同、20项共享表合同、10项偷窃边界通过。资源桶合同覆盖来源向量与目标掩码隔离、开放未来奖励、未知/不受限上界、复制与捞消耗回退、父子隔离、普通标量隔离、第三方目标回退及已消耗 Feed 的回血预留。合同为影子边界检查，不是 actual/simulated 原生状态差分。

独立普通 .NET 进程，Coordinator、宽度组合、Disabled、DOP1、掩码0、无No-GC；基线DLL来自 `aeb66fba1`。主对照 Beam45、20000节点、20000ms；复制哨兵 Beam20、4000节点、10000ms。两侧均无返回预算边界。

| 固定根 | 两侧共同终局 | 总展开：旧 → 新 | 总转移：旧 → 新 | 损血剪枝：旧 → 新 |
| --- | --- | --- | --- | --- |
| Royalties | 胜利，收益1次/额度5，战损0，零药，第1回合 | 7572 → 11 | 20336 → 43 | 0 → 5 |
| Nunchaku 计数目标 | 胜利，计数2达标/额度3，战损0，零药，第1回合 | 887 → 3 | 2194 → 9 | 0 → 6 |
| DualWield 复制成长哨兵 | 胜利，收益2次/额度10，战损0，零药，第1回合 | 5669 → 5669 | 20247 → 20247 | 不启用成长桶消费 |

两组主对照单次搜索秒数分别为4.68→0.56、1.11→0.50；复制哨兵5.50→5.49。仅为小样本离线观察，不外推稳定性能或可见帧时间。

成长同根见证续用通过：第二次请求保留完整路线，按完整政策质量检查不退化，改用药政策时清空见证。偷牌哨兵仍展开7/转移10、战损17、保全资源、零药、第2回合；NotYet 哨兵仍先回血后击杀，展开243/转移527、战损0、零药、第1回合。最后的标量隔离补充仅影响普通标量路径，成长及非零额度遗物主对照原先已旁路该路径；新增外部来源门只旁路第三方登记或目标，原版主对照未触发。不重复相同主对照。

本机输入、完整结果、失败调查位于忽略目录 `.local/resource-buckets/`。未执行原问题ZIP原生回放、可见Steam性能和完整原生部署；未对全部成长来源、混合目标或正数药水档作完整搜索结论。正数药水桶沿用既有胜利资格和封闭门控，没有扩大为所有强制药水路径。

静态检查：工具目录检查通过。文档检查仍有既存 Q008/Q009/Q010 忽略ZIP缺失链接。结构门禁未全通过：基线 `aeb66fba1` 的Search策略ID字面量已经为651，超过门槛645；本次最终仍为651，没有增加。此次不扩大范围修改已有牌堆黑名单或提高门槛。成长认证使用模型类型匹配，不新增策略ID字符串。

## 复跑输入与入口

复用 `tools/search/OfflineSearchHarness/`，新增环境变量 `OFFLINE_HARNESS_RESOURCE_SETTINGS` 指向仅供离线测试的 SolverSettingsData JSON。宿主只读取其中 GrowthBudgets、RelicStrategyEnabled、RelicCounterRules；预算仍来自CLI，不改玩家设置文件。策略文件示例：

```json
{"growthBudgets":{"royalties":5}}
```

```json
{"relicStrategyEnabled":true,"relicCounterRules":[{"id":"Nunchaku","enabled":true,"minimum":2,"maximum":2,"hpAllowance":3,"priority":1}]}
```

成长根：REGENT / FUZZY_WURM_CRAWLER_WEAK / GROWTHBUCKET20261004，飞升0、敌HP6、玩家75/75、初始能量3、清空牌组及牌堆。手牌 Royalties（永久牌组）、2张 StrikeRegent、4张 DefendRegent；抽牌堆5张 StrikeRegent，原生起始遗物。遗物根：IRONCLAD / 同遭遇 / RELICBUCKET20261004，敌HP12、玩家80/80、能量3；手牌3张 StrikeIronclad、4张 DefendIronclad，抽牌堆5张 StrikeIronclad，添加 Nunchaku，其他同上。

复制根以成长根为底，种子 GROWTHCOPY20261004、能量4，手牌改为 Royalties、DualWield、StrikeRegent、DefendRegent，各1张；抽牌堆仍5张 StrikeRegent。收益仍须达到2次，不能用更少收益换更少节点。

边界检查：设置 `OFFLINE_HARNESS_RESOURCE_BUCKET_CHECKS=1`，请求 `coverage/fixtures/scenarios/state/royalties-resource-0170.json`，运行 `--milestone M1`。共享表用 `--check-primary-incumbents`。同根验证在成长根的Coordinator调用中加 `--verify-shared-incumbent-reuse`。对照通过 `OFFLINE_HARNESS_COMBATSOLVER_DLL` 指定旧/新DLL，分别启动独立进程。
