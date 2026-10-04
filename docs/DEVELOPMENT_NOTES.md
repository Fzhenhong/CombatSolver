# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

0.49.2 全平台发布定稿见 [历史卷 15](archive/development/volume-15.md)，内容性 Mod 提示初始记录见 [历史卷 14](archive/development/volume-14.md)，玩家说明见 [0.49.2 更新日志](releases/0.49.2-RELEASE_NOTES.md)。

0.49.3 发布定稿与 BaseLib 生成牌回归见 [历史卷 18](archive/development/volume-18.md)，框架与局外 Mod 兼容性验证见 [历史卷 17](archive/development/volume-17.md)，玩家说明见 [0.49.3 更新日志](releases/0.49.3-RELEASE_NOTES.md)。

## 0.49.4（待发布）

### 问题上传引导（2026-10-05）

- 搜索期间的原生输入按已有 player / solver / system 来源记录。每次搜索单独保存玩家输入标记；录制明细不完整时仍更新该标记。手操导致的结果过期记录为 `ManualSearchResultStale`，显示重新计算提示；来源不明的过期继续保留反馈提示，同场其他诊断问题保持原分类。
- Runtime 根据本场实际角色、战斗牌堆及牌组、遗物、怪物、Power、药水、附魔、灾厄与球的模型来源保存上传引导资格。发现第三方内容后，本场搜索初始化、计算、部署和回合准备的普通异常保留真实错误与类别，统一省去上传提示；反馈横幅、路线详情和全自动暂停提示使用同一资格。新战斗重新判断。
- 上传策略独立于模拟准入；仅安装框架、局外 Mod 或本场未出现的新怪物不影响原版问题的反馈。设置中的主动上传入口继续可用。
- 日志站只读查询确认：观者 `4e74450ff819415bb96c14b9211aa6c7` 在第三方姿态镜像中发生普通计算异常；过期报告 `71d726926a2249efad12730b4cc05fa8` 的 generation 46 搜索期间有玩家出牌及结束回合输入。本轮调整上传引导，未复跑观者战斗或修改姿态语义；账号只读权限未回写线上报告。
- 最终行为源码的最小原生合同 `CONTENT-MOD-FAILURES` Passed，runId `73fa6ecba0ff47ee9adf777a84abd640`，25.508 秒；验证明细见 [测试入口](TEST_MATRIX.md)。玩家说明见 [0.49.4 更新日志](releases/0.49.4-RELEASE_NOTES.md)。
