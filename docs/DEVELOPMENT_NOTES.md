# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.50.0 的 PR #207、PR #211、智能药水与组合补搜定稿见 [历史卷 20](archive/development/volume-20.md)，玩家说明见 [0.50.0 更新日志](releases/0.50.0-RELEASE_NOTES.md)。各渠道发布结果以 `releases/CombatSolver-0.50.0.publish-state.json` 的统一发布记录为准。

性能研究的逐次结果、失败与未验证项保留在同卷及 [性能专题](performance/README.md)。

## Captured hand-size consistency (contributor draft)

Live continuation captures RitsuLib's final hand limit; predicted continuation and combat fingerprints consume each player's root-captured limit. Root player order remains frozen through forks. No new runtime dependency or change to native hand-size semantics.

The equivalent 0.48.1 source candidate passed native Dredge13 and CrashLanding5 with the adapter fix disabled. The change is rebased onto 0.50.0 and builds in Release, but those native results and timing samples belong to 0.48.1. Rebased native/performance acceptance is pending; this proposal stays Draft. See the accompanying PR report and figures.
