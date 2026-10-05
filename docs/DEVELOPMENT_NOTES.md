# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

0.49.2 全平台发布定稿见 [历史卷 15](archive/development/volume-15.md)，内容性 Mod 提示初始记录见 [历史卷 14](archive/development/volume-14.md)，玩家说明见 [0.49.2 更新日志](releases/0.49.2-RELEASE_NOTES.md)。

0.49.3 发布定稿与 BaseLib 生成牌回归见 [历史卷 18](archive/development/volume-18.md)，框架与局外 Mod 兼容性验证见 [历史卷 17](archive/development/volume-17.md)，玩家说明见 [0.49.3 更新日志](releases/0.49.3-RELEASE_NOTES.md)。

## 下一版本（开发中）

### 单人共享损血剪枝（2026-10-05）

完整合规胜利在串行提交处立即发布，各组合成员共享按失窃量、药水量、成长来源次数及遗物目标分桶的见证。同根同政策的会话仅携带完整零药路线，根戳、损血账本或政策变化时失效。未知回血、开放药水档及未知成长上界保留展开。

可执行零药胜利使用独立于纯 HP 上界的准入条件；成长和遗物路线同样参与下一次同根请求的完整路线选优。纯 HP 界继续使用原有资格限制。原生 `PRIMARY-INCUMBENT-REUSE` 连续三次搜索通过完整路线质量、成长次数、两回合增量回放、政策失效和 live 隔离检查。

未终局无风险节点可以等值截断，可能放弃同收益同损血但更早获胜的路线；不承诺原完整排序或所有有限 Beam 根均不退化。实现及复跑见[策略说明](strategy/hp-loss-pruning/README.md)，新基线结果见[测试入口](TEST_MATRIX.md)。
