# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

## 下一版本（开发中）

组件回复界与重场景研究继续进行，详见下节。

## 性能研究分支：组件回复界与重场景组合（2026-10-04）

本分支保留[组件回复界及 Smart 验收](performance/component-healing-bound-20261003.md)、[原生组件审计](performance/native-healing-component-audit-20261003.md)和[组合研究](performance/heavy-scene-compositions-20261004.md)。原固定回归的女王质量阻断与未达到两倍的实验继续保留；用户暂停丢路调查，仅接受感染棱柱约1.98倍和 Silent 精英约1.86倍的特定场景例外。其余慢根仍需完整请求至少两倍、战损不增加、峰值内存至多增加10%。

合入上游0.49.1后继续全药水认证，并新增当前上游导出的 SoulNexusElite 问题包。孤立研究候选尚未完成最终回调证明及回归，不因最小合同通过而部署。原分支合并前的完整开发记录保留于[固定提交](https://github.com/ltlly/CombatSolver/blob/1bea4f8a/docs/DEVELOPMENT_NOTES.md)。

[全原版药水清单](performance/native-potion-recovery-certificates-20261004.md)分类64种有效药水及2种非生产模型，58种仅在完整组件闭包条件下可给出零回复界；原生最小合同通过，实际64种用药效果及最终回归未完成。[魂枢研究](performance/soul-nexus-0491-research-20261004.md)记录上游整请求312.20秒、峰值8.21GB、19战损/0药/8回合；该包仍要求两倍，诊断和新原型不作达标成果。

合并后的两条Testing根目录门禁失败已按上游结构修正：组件回复界与Smart资格合同原文移入 `Contracts/Search`，partial及请求协议不变，两平台门禁通过。监听者合同的GoldCallbacks反射oracle改为三个金币回调的并集；原生1685模型/63监听位合同通过，运行时派发规则未改。
