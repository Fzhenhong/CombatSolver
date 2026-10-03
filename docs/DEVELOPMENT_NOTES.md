# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)，0.48.1 批次见 [历史卷 10](archive/development/volume-10.md)。

0.48.2的战斗状态与路线执行修复见[历史卷11](archive/development/volume-11.md)；日志站逐包处理及两份保留首因见[排查记录](issues/0.48.0-hardbugs-20261003.md)。

## 0.48.2（开发中）

本批玩家更新日志见 [0.48.2 更新日志](releases/0.48.2-RELEASE_NOTES.md)，以已发布 0.48.1 为比较基线，汇总战斗执行修复和 PR #203、#204 的玩家可见变化。

感谢 [ltlly](https://github.com/ltlly) 的 [PR #203](https://github.com/Torch1230/CombatSolver/pull/203)：补全金币修改、修改后通知与获得后回调，贪婪之手记录实际所得；狂宴与果汁共用实际最大生命增量及回复入口。DragonFruit、ChosenCheese 和 DarkstonePeriapt 的回复来源未封闭时保留完整回复余量。根冻结跑局监听成员，分支独占资源与遗物状态；金纸仅在有待结算计数时增加指纹字段。主搜索前计划仍只准入严格认证根，保留其他根的阶段顺序。

原作者的原生合同、29 根筛查及失败记录见 [金币与最大生命回复](performance/gold-max-hp-healing-20261003.md) 和 [上游整合证据](performance/upstream-0480-merge-check-20261003.md)；全来源审计由 [性能资料入口](performance/README.md) 收纳。

感谢 [yM7-1](https://github.com/yM7-1) 的 [PR #204](https://github.com/Torch1230/CombatSolver/pull/204)：固定前缀在战斗终局处截断，接受标记正确的强制结束回合卡牌；抽牌前遗物生成沿唯一入口结算，RadiantPearl 与主线既有实现合并。关闭 NoGC 时也按系统余量设置请求分配边界，排空后的检查点执行普通 Gen2 回收并更新限额。

格挡乘法保留所有可表示的原生中间值，虚弱等后续修正完整执行；指数因子从 96 层起按 decimal 溢出处理，最终结算时使用既有格挡上限，零格挡保持零。合并验收增加了 Shadowmeld 与 Frail 的完整原生状态/RNG 差分及父分支隔离。默认 GC 的限额与入口诊断成功后才登记作用域；失败通过 finally 释放压力信号并传播原异常，真实 CLR 故障合同已由失败基线转为通过。

贡献者的 B013 失败基线、五项通过及代表包哨兵见 [测试矩阵](TEST_MATRIX.md)。T007 第二样本在贡献者当前版本未复现；低内存玩家宿主、原生整场部署与可见游戏性能仍无本轮证据。原有完整根长预算数据沿作者报告保留，本轮验证结果另记。

2026-10-04合并验收：13个无头机制场景和18项真实CLR检查通过，全部无头实例由启动器删除；最终Release零警告/错误。固定前缀的开局探测与完整搜索统一截断终局后缀，合同核对终局动作数、回合及跨回合完整状态。本轮结果、失败基线与检查边界见[测试矩阵](TEST_MATRIX.md)。
