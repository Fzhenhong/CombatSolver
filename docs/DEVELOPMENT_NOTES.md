# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)，0.48.1 批次见 [历史卷 10](archive/development/volume-10.md)。

0.48.2的战斗状态与路线执行修复见[历史卷11](archive/development/volume-11.md)；日志站逐包处理及两份保留首因见[排查记录](issues/0.48.0-hardbugs-20261003.md)。

## 下一版本（开发中）

感谢 [ltlly](https://github.com/ltlly) 的 [PR #203](https://github.com/Torch1230/CombatSolver/pull/203)：补全金币修改、修改后通知与获得后回调，贪婪之手记录实际所得；狂宴与果汁共用实际最大生命增量及回复入口。DragonFruit、ChosenCheese 和 DarkstonePeriapt 的回复来源未封闭时保留完整回复余量。根冻结跑局监听成员，分支独占资源与遗物状态；金纸仅在有待结算计数时增加指纹字段。主搜索前计划仍只准入严格认证根，保留其他根的阶段顺序。

原作者的原生合同、29 根筛查及失败记录见 [金币与最大生命回复](performance/gold-max-hp-healing-20261003.md) 和 [上游整合证据](performance/upstream-0480-merge-check-20261003.md)；全来源审计由 [性能资料入口](performance/README.md) 收纳。
