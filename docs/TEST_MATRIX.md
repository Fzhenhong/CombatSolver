# CombatSolver 测试入口

按改动选择最小验证层，方法见 [无人测试](HEADLESS_TESTING.md) 与 [社区验收](community/testing-guide.md)。以下命令提供当前复跑入口，不表示本轮已执行。

历史记录见 [归档索引](archive/testing/README.md)，0.48.1 的验证、失败与未验证项见 [历史卷 12](archive/testing/volume-12.md)。

## 当前矩阵命令

矩阵启动器只读取本节的平台原生命令；历史记录不作为自动执行清单。以下两项分别检查倾泻手空边界与相邻效果作用域，不运行整场搜索。需要当前游戏及 RitsuLib 路径，矩阵启动器统一管理实例并在结束时清理。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CASCADE-EMPTY-HAND-NATIVE -EnemyCurrentHp 1000 -HeadlessFastModeForTest Instant -DeploymentFastModeForTest Instant -DeploymentInterActionDelaySecondsForTest 0 -TimeoutSeconds 120
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId EFFECT-SCOPE-ADJACENT-CONTRACT -EnemyCurrentHp 1000 -HeadlessFastModeForTest Instant -DeploymentFastModeForTest Instant -DeploymentInterActionDelaySecondsForTest 0 -TimeoutSeconds 120
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id CASCADE-EMPTY-HAND-NATIVE --enemy-current-hp 1000 --headless-fast-mode-for-test Instant --deployment-fast-mode-for-test Instant --deployment-inter-action-delay-seconds-for-test 0 --timeout-seconds 120
./tools/testing/run-unattended-test.sh --scenario-id EFFECT-SCOPE-ADJACENT-CONTRACT --enemy-current-hp 1000 --headless-fast-mode-for-test Instant --deployment-fast-mode-for-test Instant --deployment-inter-action-delay-seconds-for-test 0 --timeout-seconds 120
```

## 社区批次 B013 回归夹具（2026-10-03，Refs #172）

五个主题的独立机制回归在 `src/Testing/Regressions/Community/B013*Checks.cs`，命令统一为
`tools/testing/run-unattended-test.ps1 -ScenarioId <ID> -TimeoutSeconds 120 -ExitOnComplete`（角色/遭遇按夹具要求，详见 PR #204）：

- `B013-BLOCK-DECIMAL-BOUNDARY`（T010，格挡乘法 Decimal 边界）：修改前 `dec22b8f` Failed（栈同报告）→ 修改后 `e2757311`/`a23dcf21` Passed；代表包 DOP1 哨兵路线与展开/转移一致。
- `B013-FIXED-PREFIX-TERMINAL-BOUNDARY`（T008，固定前缀越过锁定终局）：`971ccdd1` Failed → `0b3f96a6` Passed；代表包哨兵 22809/129762、终局一致。
- `B013-FIXED-PREFIX-TURN-END-CARD`（T007，结束回合卡固定前缀）：`eb64242d` Failed → `899ef881` Passed。
- `B013-RADIANT-PEARL-HAND-DRAW`（T006，抽牌前遗物生成对账）：`cd227ff6` Failed → `15c80923` Passed；代表包 SearchOnly 失配 → `TURN_SETUP_STATE_MATCH`。
- `B013-DEFAULT-GC-LIMIT`（T009，默认 GC 请求分配边界）：`6ac0f466` Failed（`limit=long.MaxValue`）→ `7c6f3b83` Passed。

变基到 0.48.1（`2ead87d9`）后五夹具复跑 Passed：`0f19174f` / `57e2086d` / `9d7173af` / `3892e307` / `fdfc8734`。代表包哨兵在新基线：T010 DOP1 与旧基线逐字段一致（展开 7426、转移 77129、4 回合胜、finalHp 71），耗时 3458.9ms；T009/T006 代表包 7 回合胜、finalHp 88、boundary None（展开 26194、转移 190351，路线随上游搜索改动微调）。

完整修改前后、哨兵与未验证项见 PR #204；批次状态与剩余项（T007 第二样本未复现）同样记录在该 PR。

