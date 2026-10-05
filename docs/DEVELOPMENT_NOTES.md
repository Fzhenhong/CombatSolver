# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

0.49.2 全平台发布定稿见 [历史卷 15](archive/development/volume-15.md)，内容性 Mod 提示初始记录见 [历史卷 14](archive/development/volume-14.md)，玩家说明见 [0.49.2 更新日志](releases/0.49.2-RELEASE_NOTES.md)。

0.49.3 发布定稿与 BaseLib 生成牌回归见 [历史卷 18](archive/development/volume-18.md)，框架与局外 Mod 兼容性验证见 [历史卷 17](archive/development/volume-17.md)，玩家说明见 [0.49.3 更新日志](releases/0.49.3-RELEASE_NOTES.md)。

## 下一版本（开发中）

### 第三方额外回合来源登记

`ExtraTurnMirrors` 为 `ShouldTakeExtraTurn` / `AfterTakingExtraTurn` 开放第三方登记（泛型与按 `Type` 两种）。
原版龙涎香、帕尔之眼与第三方来源按原生监听顺序统一派发；`HookMirrors.ShouldTakeExtraTurn` 在首个 true 处结束判断，
`AfterTakingExtraTurn` 固定成员后依次结算并处理选择暂停。搜索回放与 `LiveEndTurnRiskEvaluator` 共用入口。重写了却没登记的
第三方类型按回合阶段表的口径停止搜索。监听者掩码用最后一位 `ExtraTurnCallbacks`，两个方法共用。
帕尔之眼的后置回调在所属玩家获得额外回合时标记已使用，与原生方法一致；分支计数仍由 `SimulatedCombatState` 持有。

### 第三方规范 Power 预热登记

0.49.2 起规范 Power 预热只访问原版来源，第三方 Power 在搜索里第一次被施加时会撞上
`PowerDynamicVarMaterializationGuardPatch`。`PowerDynamicVarWarmup.RegisterAdaptedCanonicalPower` 让适配层
显式担保某个第三方 Power 的规范实例可以在主线程物化，建根时随原版一起物化，每局一次；物化失败照常抛出。
默认范围不变，没有登记时行为与此前一致。

### 原版怪物出招表补丁的适配声明

`PredictionModPatchAudit.RegisterAdaptedMonsterMachine(Type, string modId)` 让适配层逐个声明某 mod 对某原版怪物
`GenerateMoveStateMachine` 的补丁已适配。`RejectForeignPatches` 只对出招表方法、且仅对登记的
（出招表的声明类型，mod id）组合放行，声明类型可以是被多个怪物继承的抽象基类；其他 mod、其他审计方法及第三方怪物的整体门禁不变。
没有登记时行为与此前一致。动机：平衡尖塔改写了 45 个原版怪物的出招表，0.49.2 起几乎每场都停在
「求解器暂未适配此内容性 Mod」，而其适配层已逐条核对招式效果与条件分支。
