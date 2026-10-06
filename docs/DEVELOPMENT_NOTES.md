# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

2026-10-06 [计划续搜复用完整胜利证明](performance/plan-witness-propagation-20261006.md)：严格组件根将已有及首成员新发现的完整胜利传给后续计划，沿原回复上界与政策门禁剪枝；Runtime 仅在重建 NoGC 的检查点直接请求一次压缩完整回收。最终 11 根 44 次串行交错质量字段一致、峰值全部通过，铁甲 39.414→10.578 秒（3.726 倍），峰值 -65.834%；新增原生真实计划循环和一次压缩完成合同通过。组件根覆盖 2/11，未扩大来源证书；历史战损波动与全部原始慢根缺口保留。

历史记录见 [归档索引](archive/development/README.md)。0.50.0 的 PR #207、PR #211、智能药水与组合补搜定稿见 [历史卷 20](archive/development/volume-20.md)，玩家说明见 [0.50.0 更新日志](releases/0.50.0-RELEASE_NOTES.md)。各渠道发布结果以 `releases/CombatSolver-0.50.0.publish-state.json` 的统一发布记录为准。

性能研究的逐次结果、失败与未验证项保留在同卷及 [性能专题](performance/README.md)。

## 下一版本（开发中）

### Q002 能力与药水边界续搜（PR #213）

接入 [shun-tong](https://github.com/shun-tong) 的 [PR #213](https://github.com/Torch1230/CombatSolver/pull/213)：在现有组合成员及共享节点、时间预算内，从搜索自主生成的能力、用药和后续损血边界继续求解，改善长战斗路线。整合沿用 0.50.0 的组件回复证明、药水成本保护与智能开局药水准入。回放入口恢复完整搜索 profile 与录制开关，历史阶段见 [卷 21](archive/development/volume-21.md)，当前主线的同根质量及耗时证据见 [Q002 记录](issues/q002-route-quality.md#0500-主线整合2026-10-05)。

强制用药续搜按实际回合边界的完整回放状态准入：即使已生成路线在之后走到死亡，也可复用此前满足用药政策、仍存活的稳定边界。死亡、胜利和未完整应用的前缀由边界验证拒绝，`FIXED-PREFIX-TURN-OUTCOMES` 覆盖该合同及实机状态隔离。
