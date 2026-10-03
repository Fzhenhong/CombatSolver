# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

## 0.49.1（开发中）

修复默认 GC 搜索的不可分割提交回退：常规检查点刷新系统内存上限，显式 `UseDefaultGcAndContinue` 在完成回收后将分配所有权交给 CLR。普通请求各自建立限额，取消与作用域释放保留原有生命周期。

0.49.0 日志站固定 18 份报告、16 场战斗的计算失败均命中同一异常；新真实 CLR 合同在修改前失败，修改后通过。`default-commit` 2 项、`default-entry` 2 项与 `recovery-lifecycle` 3 项通过；未重放原包整场或验证可见游戏性能。逐包身份、首因与验证范围见 [内存提交回归](issues/0.49.0-memory-commit-regression-20261004.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。
