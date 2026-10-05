# CombatSolver 测试入口

按改动选择最小验证层，方法见 [无人测试](HEADLESS_TESTING.md) 与 [社区验收](community/testing-guide.md)。以下命令提供当前复跑入口，不表示本轮已执行。单人共享损血剪枝的新基线验证见[策略证据](strategy/hp-loss-pruning/README.md#单人共享损血剪枝2026-10-05)。

PR #207 合入0.49.4的本轮证据见[整合验收](performance/pr207-upstream-0494-integration-20261005.md)：成本、组件回复及Smart原生合同通过，零额度遗物本地/外部准入先失败后通过；最终整请求及固定回归结果按该报告更新，旧版本数字不冒充本轮通过。

历史记录见 [归档索引](archive/testing/README.md)，0.48.1 的验证、失败与未验证项见 [历史卷 12](archive/testing/volume-12.md)。

0.49.4 的额外回合镜像顺序、同根成长胜利续用、整场自动部署与上传引导验证见 [历史卷 16](archive/testing/volume-16.md)。

0.49.0 的行为验证沿用本页 PR #203、#204 合并验收与 [战斗状态修复验证](archive/testing/volume-13.md)。版本及发布文档调整采用 L0 检查和发布构建；原有未验证项保留。

0.49.3 的框架、局外 Mod 与 BaseLib 验证见 [历史卷 14](archive/testing/volume-14.md)。

移动运行库内存回收验证见 [历史卷 15](archive/testing/volume-15.md)。

## 0.49.1 日志站硬逻辑（2026-10-04）

强制结束回合选牌、群体 Power、死亡金币回调、延迟能量／等离子球、开局小刀、回合末自动出牌、行动意图、资源隔离、反应格挡和界面归属的原生差分见 [逐类验收](issues/0.49.1-hardbugs-20261004.md#原生验收证据)。该记录保留失败基线、runId、复跑入口、第三方边界及未验证项；生产部署合同包含增量验证和计划外重算断言。

## 0.49.2 内容性 Mod 失败分类（2026-10-04）

`CONTENT-MOD-FAILURES` 与 `VerifyPredictionFailureBoundaries` 同进程 Passed，runId `849fb38580474f7881c05d113beef5d3`，24.505 秒。合同直接调用五个回合阶段、三个金币回调与计算型变量的生产拒绝入口，断言确认的第三方来源、原生回调保持未执行、包装异常、eng/zhs/zht 的 Mod 名称与方括号转义、四类失败账本仅记录暂未适配且不触发上传。原版、共享计算框架与运行库失败继续提示诊断上传；平台接口错误保留主失败类别。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CONTENT-MOD-FAILURES -EnemyCurrentHp 1000 -VerifyPredictionFailureBoundaries -TimeoutSeconds 120 -CleanupInstanceOnExit
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id CONTENT-MOD-FAILURES --enemy-current-hp 1000 --verify-prediction-failure-boundaries --timeout-seconds 120 --cleanup-instance-on-exit
```

来源与验证边界见 [开发历史卷 14](archive/development/volume-14.md)。最终行为源码在版本同步前通过；后续只改版本元数据和文档，采用最终 Release 构建。测试启动器已删除实例，未执行可见 Steam 弹窗排版验收或第三方原包整场回放。

## 0.49.1 内存提交回归（2026-10-04）

`CombatSolver.GcPolicyChecks` 直接编译生产 Runtime 内存代码。`default-commit` 在未改生产代码时 Failed：显式回退后分配上限仍有效；修复后 2 项 Passed，覆盖普通检查点保留限额、不可分割提交完成回收后续行、后续请求重新建立限额与取消保持原准入。

```bash
dotnet run --project tools/testing/checks/CombatSolver.GcPolicyChecks/CombatSolver.GcPolicyChecks.csproj -c Release -- default-commit
```

相邻真实 CLR 回归：`default-entry` 2 项、`recovery-lifecycle` 3 项 Passed，包含实际 NoGC 退出与恢复、显式退出持续生效及诊断失败清理。每项设有 15 或 20 秒截止时间。未重放 18 份原包整场，也没有可见 Steam 或低内存宿主性能结论；[排查记录](issues/0.49.0-memory-commit-regression-20261004.md)保留固定报告身份与触发窗口。

## PR #203 的机制合同

金币、最大生命回复与遗物间接回复的贡献者证据见 [专题记录](performance/gold-max-hp-healing-20261003.md)。独立场景为 `GOLD-HEALING-MECHANISMS`、`MAX-HP-HEALING-CALLBACKS`、`FEED-MAX-HP-CAP`、`RELIC-MAX-HP-HEALING-BOUNDS` 和 `AXEBOT-JOSS-DEFERRED-FORK`，均从平台原生无人入口运行，使用 IRONCLAD、FUZZY_WURM_CRAWLER_WEAK、120 秒上限及实例清理。原作者结果与本轮合并验证分别记账。

金币和回复合同同时传 `-EvidenceDirectory .local/validation/pr203/<场景>`（Bash：`--evidence-directory`），初始生命为 50/80。金纸合同还传 `-RelicsPath coverage/fixtures/regressions/axebot/axebot-joss-deferred-relics.json`（Bash：`--relics-path`）。搜索阶段顺序合同为 `KNOWN-HEALING-MEMBERS` 和 `KNOWN-HEALING-OPENING`，使用 SILENT；受影响的固定前缀合同使用 IRONCLAD，强制结束回合牌合同使用 REGENT。

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


## 0.48.0 硬错误机制合同

这些场景各自停止在共享首因或必要跨回合边界。完整runId、失败基线和未验证项见[历史卷13](archive/testing/volume-13.md)，逐包分类见[问题记录](issues/0.48.0-hardbugs-20261003.md)。

```powershell
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId NATIVE-CHOOSE-OPEN-GATE -CharacterId SILENT -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CALCULATED-GAMBLE-ORDERED-DISCARD -CharacterId SILENT -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FROZEN-LIGHTNING-CHANNELS -CharacterId DEFECT -EncounterId MECHA_KNIGHT_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId REPORT-ROUND-IMBALANCED -CharacterId NECROBINDER -EncounterId BOWLBUGS_NORMAL -InitialPlayerHp 200 -InitialPlayerMaxHp 200 -ClearPlayerHand -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId SECOND-WIND-REPORT-ROOT -CharacterId IRONCLAD -EncounterId DECIMILLIPEDE_ELITE -InitialRoundNumber 3 -InitialPlayerTurnNumber 3 -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId GROUP-DEBUFF-REACTIVE-DRAW -CharacterId NECROBINDER -EncounterId CHOMPERS_NORMAL -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CONSTRUCT-REAPER-ARTIFACT -CharacterId NECROBINDER -EncounterId CONSTRUCT_MENAGERIE_NORMAL -InitialRoundNumber 3 -InitialPlayerTurnNumber 3 -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FIXED-PREFIX-POTION-POLICY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FIXED-PREFIX-TURN-OUTCOMES -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId MIRRORED-HOOK-FILTER -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FROZEN-ROOT-LISTENERS -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CALCULATED-HISTORY-FREEZE -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId VOID-FORM-TURN-CHOICES -CharacterId REGENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId FLATTEN-MUSIC-BOX-ENTRY -CharacterId NECROBINDER -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId SEEKER-ORDERED-OPTIONS -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId RITUAL-TEMPORARY-STRENGTH -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId NOXIOUS-RAMPART-ORDER -CharacterId IRONCLAD -EncounterId TURRET_OPERATOR_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId EVIL-EYE-EXHAUST-HISTORY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId BLOCK-SPEC-CARD-PLAY-IDENTITY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId ROUTE-ADOPTION-LIFETIME -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId PAELS-LEGION-FINISHED-REFERENCE -CharacterId SILENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId END-TURN-RISK-LOSS-ACCOUNTING -CharacterId DEFECT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId MAKE-IT-SO-FULL-HAND -CharacterId REGENT -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId CARD-COST-IDENTITY-CONTRACT -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId DAMPEN-REACTIVE-ROCKET-PUNCH -CharacterId DEFECT -EncounterId KNIGHTS_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId DAMPEN-REACTIVE-MELANCHOLY -CharacterId NECROBINDER -EncounterId KNIGHTS_ELITE -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
pwsh -NoProfile -File tools\testing\run-unattended-test.ps1 -ScenarioId RADIANT-PEARL-ENTRY -CharacterId IRONCLAD -EncounterId FUZZY_WURM_CRAWLER_WEAK -EnemyCurrentHp 1000 -TimeoutSeconds 120 -CleanupInstanceOnExit
```

```bash
./tools/testing/run-unattended-test.sh --scenario-id NATIVE-CHOOSE-OPEN-GATE --character-id SILENT --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CALCULATED-GAMBLE-ORDERED-DISCARD --character-id SILENT --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FROZEN-LIGHTNING-CHANNELS --character-id DEFECT --encounter-id MECHA_KNIGHT_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id REPORT-ROUND-IMBALANCED --character-id NECROBINDER --encounter-id BOWLBUGS_NORMAL --initial-player-hp 200 --initial-player-max-hp 200 --clear-player-hand --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id SECOND-WIND-REPORT-ROOT --character-id IRONCLAD --encounter-id DECIMILLIPEDE_ELITE --initial-round-number 3 --initial-player-turn-number 3 --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id GROUP-DEBUFF-REACTIVE-DRAW --character-id NECROBINDER --encounter-id CHOMPERS_NORMAL --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CONSTRUCT-REAPER-ARTIFACT --character-id NECROBINDER --encounter-id CONSTRUCT_MENAGERIE_NORMAL --initial-round-number 3 --initial-player-turn-number 3 --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FIXED-PREFIX-POTION-POLICY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FIXED-PREFIX-TURN-OUTCOMES --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id MIRRORED-HOOK-FILTER --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FROZEN-ROOT-LISTENERS --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CALCULATED-HISTORY-FREEZE --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id VOID-FORM-TURN-CHOICES --character-id REGENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id FLATTEN-MUSIC-BOX-ENTRY --character-id NECROBINDER --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id SEEKER-ORDERED-OPTIONS --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id RITUAL-TEMPORARY-STRENGTH --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id NOXIOUS-RAMPART-ORDER --character-id IRONCLAD --encounter-id TURRET_OPERATOR_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id EVIL-EYE-EXHAUST-HISTORY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id BLOCK-SPEC-CARD-PLAY-IDENTITY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id ROUTE-ADOPTION-LIFETIME --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id PAELS-LEGION-FINISHED-REFERENCE --character-id SILENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id END-TURN-RISK-LOSS-ACCOUNTING --character-id DEFECT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id MAKE-IT-SO-FULL-HAND --character-id REGENT --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id CARD-COST-IDENTITY-CONTRACT --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id DAMPEN-REACTIVE-ROCKET-PUNCH --character-id DEFECT --encounter-id KNIGHTS_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id DAMPEN-REACTIVE-MELANCHOLY --character-id NECROBINDER --encounter-id KNIGHTS_ELITE --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
./tools/testing/run-unattended-test.sh --scenario-id RADIANT-PEARL-ENTRY --character-id IRONCLAD --encounter-id FUZZY_WURM_CRAWLER_WEAK --enemy-current-hp 1000 --timeout-seconds 120 --cleanup-instance-on-exit
```

## 社区批次 B013 回归夹具（2026-10-03，Refs #172）

以下为贡献者在 PR #204 记录的结果，与本轮验证分别记账。五个主题的独立机制回归在 `src/Testing/Regressions/Community/B013*Checks.cs`，命令统一为
`tools/testing/run-unattended-test.ps1 -ScenarioId <ID> -TimeoutSeconds 120 -CleanupInstanceOnExit`（角色/遭遇按夹具要求，详见 PR #204）：

- `B013-BLOCK-DECIMAL-BOUNDARY`（T010，格挡乘法 Decimal 边界）：修改前 `dec22b8f` Failed（栈同报告）→ 修改后 `e2757311`/`a23dcf21` Passed；代表包 DOP1 哨兵路线与展开/转移一致。
- `B013-FIXED-PREFIX-TERMINAL-BOUNDARY`（T008，固定前缀越过锁定终局）：`971ccdd1` Failed → `0b3f96a6` Passed；代表包哨兵 22809/129762、终局一致。
- `B013-FIXED-PREFIX-TURN-END-CARD`（T007，结束回合卡固定前缀）：`eb64242d` Failed → `899ef881` Passed。
- `B013-RADIANT-PEARL-HAND-DRAW`（T006，抽牌前遗物生成对账）：`cd227ff6` Failed → `15c80923` Passed；代表包 SearchOnly 失配 → `TURN_SETUP_STATE_MATCH`。
- `B013-DEFAULT-GC-LIMIT`（T009，默认 GC 请求分配边界）：`6ac0f466` Failed（`limit=long.MaxValue`）→ `7c6f3b83` Passed。

变基到 0.48.1（`2ead87d9`）后五夹具复跑 Passed：`0f19174f` / `57e2086d` / `9d7173af` / `3892e307` / `fdfc8734`。代表包哨兵在新基线：T010 DOP1 与旧基线逐字段一致（展开 7426、转移 77129、4 回合胜、finalHp 71），耗时 3458.9ms；T009/T006 代表包 7 回合胜、finalHp 88、boundary None（展开 26194、转移 190351，路线随上游搜索改动微调）。

完整修改前后、哨兵与未验证项见 PR #204；批次状态与剩余项（T007 第二样本未复现）同样记录在该 PR。

## PR #203、#204 合并验收（2026-10-04）

以下为本轮直接运行结果；全部使用120秒上限和实例清理，已通过的请求未重复运行。无头验证停在对应机制边界。

| 场景 | runId | 结果与范围 |
| --- | --- | --- |
| `GOLD-HEALING-MECHANISMS` | `3bc7928290c343cca88248c0c28dd95e` | Passed；金币修正、三个回调、完整原生状态/RNG和父分支隔离 |
| `MAX-HP-HEALING-CALLBACKS` | `cd6549478924451882fc600e67724a53` | Passed；最大生命实际增量、封顶与回复回调的原生差分 |
| `FEED-MAX-HP-CAP` | `ddcf661e391c4013a0aa87861f8542f3` | Passed；两次致命出牌的实际增量与成长计数 |
| `RELIC-MAX-HP-HEALING-BOUNDS` | `bee94b31824d449e8f7e9fa55cbe94f5` | Passed；两项直接原生遗物回调与熔化来源排除 |
| `AXEBOT-JOSS-DEFERRED-FORK` | `0fd9902a8a594e36a0a9fbf533ceee65` | Passed；根计数、零状态指纹、父子/兄弟Fork与逐分支消费 |
| `KNOWN-HEALING-MEMBERS` | `fab7a8170a3942e9920e09fa466aa34e` | Passed；严格增量、DOP2、控制质量、成长门与live隔离 |
| `B013-FIXED-PREFIX-TERMINAL-BOUNDARY` | `5b097fd5f17745a0868a6b49d7af3b37` | Passed；终局前缀截断与可交付胜利 |
| `B013-FIXED-PREFIX-TURN-END-CARD` | `c778f285a52a4b9fbbe0e370688e6ec1` | Passed；强制结束回合卡的固定前缀推进 |
| `B013-RADIANT-PEARL-HAND-DRAW` | `ca904a20eafd43b690d8c824f60e2069` | Passed；抽牌前生成的原生数量、升级与归属对账 |
| `B013-DEFAULT-GC-LIMIT` | `3e440c65f9564b5ea6a65d094521b8d7` | Passed；限额、真实Gen2回收续搜、退出清理与CLR模式 |
| `KNOWN-HEALING-OPENING` | `3fecd35f80a34cb792a43ca3b5bc3b1c` | Passed；非认证根延后计划、单次执行与控制质量 |
| `FIXED-PREFIX-TURN-OUTCOMES` | `b18d23d1829b4438a557a5f75acf6425` | Passed；独立前缀完整状态oracle、多回合结果与终局截断 |
| `B013-BLOCK-DECIMAL-BOUNDARY` | `b556026e627c4153bb3d2bf23948a095` | Passed；原生虚弱倍率、完整状态/RNG；模拟96/95/200层与零格挡 |

真实CLR工具：`default-entry` 2项、`diagnostic-failure` 8项、`scopes` 8项 Passed。入口失败基线为日志抛错后信号仍启用；修复后同异常传播、信号清理及后续独占准入均通过。

失败记录：首次无头启动在私有进程身份检查处失败并清理，未进入游戏测试，原因未确定；金币首轮14项对账已通过，但缺EvidenceDirectory导致产物写入失败；金纸首轮缺遗物输入。补齐请求参数后仅重跑失败请求，成功记录在上表。固定前缀09f94286012d420d81242f480ebd1803仍执行旧的终局拒绝断言，合同更新为开局探测和完整搜索共同截断，并断言终局动作数及回合。新增格挡夹具初次编译因原生调用参数及私有setter失败，修正后Release零警告/错误。

L0：原生回复审计工具迁入tools/inspection后构建和真实DLL扫描通过。CoverageCatalog重新生成3035项目录，状态字段未分类为0；`--verify-state-writes`仍因既有InfusedCore.AfterSideTurnStart缺运行证据失败（1项）。PowerShell结构检查247文件通过，工具检查290文件/37项目通过，文档427文件/1635链接及覆盖目录检查通过。额外Bash结构检查因运行耗时停止，未完成；两平台脚本静态语法检查通过。

范围：本轮没有重跑作者长预算全根性能筛查，没有验证低内存玩家宿主、原生整场部署或可见Steam性能；原作者失败与未验证项保留在所属报告。


## 性能研究分支验收记录（2026-10-04）

组件回复界及 Smart 原生合同通过，固定29根回归仍保留女王质量阻断；用户暂停丢路调查，未称全回归通过。完整实测、所有失败及未验证项见[组合上界](performance/component-healing-bound-20261003.md)、[原生组件审计](performance/native-healing-component-audit-20261003.md)和[组合研究](performance/heavy-scene-compositions-20261004.md)。合并前各合同、runId、平台清理与初筛范围完整保留于[固定提交的测试记录](https://github.com/ltlly/CombatSolver/blob/1bea4f8a/docs/TEST_MATRIX.md)。 同一记录补59张手牌阶段合同、Slither根认证、晚回合药水成本及未知消耗堆Feed的原生边界；组合性能仍有质量失败。

回复证明扩展研究：有限再生旧门禁预期失败3f3afd5a…，候选2e84b06a…Passed（精确7／后续2次剪枝）；5bf5eec3…保留可回复分支及未知消耗堆Feed。沙漏10c7a7e1…Passed（六次出牌、三种敌人行动、九份完整状态、16 Fork、四堆未知拒绝）；PR215储君组合8faef194…成本与未知边界、0875367f…实际无药先导／账本／DOP2严格隔离Passed。普通完整请求初筛仍未达到性能或资源质量门槛，未作最终ABBA、固定回归或部署；失败夹具、构建、逐次结果及范围见[补充研究](performance/pr215-recovery-proof-research-20261005.md)。 后续代价前沿旧表a9a36690…预期失败、7de37864…Passed（144组合／实际完整见证）；亡灵组合acef5a7c…Passed（16Fork／DOP2／共享消费／未知消耗堆），整请求战损退化而拒绝。

储君成本研究：Fork缓存 `2898db5d6e904adb8a406ed414c4a664` / `0240805e1a3048df8a82f0dc3c4b8936` Passed；零成长HP计价强制回合 `472a181e6cf24dd48605b6bc098c99bc` / `c8f30d1aca664f0788df5e3f67ad6a83` Passed，包含16实际工作分支、原生完整差分与前向结果单次消费。参与掩码布局 `fdfebc03d724493897c66f47704d30ed` / `36b2a663a51347bdb3b05656ea09d794` Passed，初筛16.12秒、工作/质量不变、分配−5.21%；同容量槽混合 `9e3882a4ced048b79489cc6c1dd56ea1` Passed，16.05秒、工作/质量相同、对原始1.251倍。候选整请求仍未达两倍：Fork缓存ACCA无收益/内存超门槛，强制回合单次16.00秒，未作新的固定回归或部署。完整采样、失败与逐次记录见[成本研究](performance/regent-search-cost-research-20261005.md)。

[全药水审计](performance/native-potion-recovery-certificates-20261004.md)记录64种有效原版药水的分类及58种有条件零回复准入；`14eef45cfeb04fc7bb137f9cec2a8c08` 最小原生合同Passed，全部64种实际用药及最终闭包未完成。[魂枢证据](performance/soul-nexus-0491-research-20261004.md)记录严格Continuation恢复、拟提交版本完整请求ABBA：上游311.39／315.50秒，候选96.02／96.66秒，中位数3.254倍，最差峰值降低50.75%，战损19→6、药水0→1、回合8→5，遗物计数目标满足数2→1；完整原生二进制仍未验证。四次搜索合同Passed，最后上游启动器退出1的身份确认异常及进程/实例清理单列。完整原生部署 `85a6c2eaf9ff495fafba556392362d86` Passed，第5回合63/70生命、StrengthPotion、非预期重算0。其他27个固定根同版本串行回归全部通过：完整根/预算/政策相等，24胜/原有3 NoWin保持，战损、保命、追回及同战损次级目标无退化，峰值最大+7.503%。女王两根按用户暂停要求排除，不能称原29根全量通过。

魂枢机制新增合同：`373890e75fa0483db84b5ffd9a8121d0` Passed，实际用药、两回合完整状态、16 Fork/RNG/父分支/live隔离、禁药与额度仍保留已有再生、未知源拒绝、生成过滤及DOP2严格增量/完整重播。`b9a5e01156fb476d8dfcfb9ece2c0e6b` Passed，8遗物/68抽牌查询及冻结隔离，启动器退出1异常单列；查询不代替完整生命周期。拟提交版本 `965c558fc8d341a9afffb821c6d61468` Smart合同Passed，完整无药胜利、精确7次/后续2次剪枝及原政策门禁。

新增监听者分配原型的首次合同 `2283f3757afc4676b9102fd732393e4d` 因既有GoldCallbacks反射oracle错误失败；更正后 `f99dd720ef0b4b49831d75dcc41d071e` Passed，1685模型/63监听位/完整顺序/Fork/失效/补丁合同通过。整请求317.18秒、0.984倍，未提速，未纳入拟提交版本。其余失败构建、内部短搜取消及未提速候选均在魂枢报告保留。

上游合并后的合同原文归位：组件回复界和Smart资格移入 `Contracts/Search`；双平台门禁由两条根目录违规修正为Passed、search_files=248，Release构建0警告错误。原生 `01d1255ecab441e0825ac28d14135597` Passed：完整无药胜利见证，精确层7次/开局后续2次实际剪枝、药水/成长/遗物/追回门禁、DOP2严格增量及完整协调器/live隔离；实例清理。代表合同不代替合并后固定29根回归。

PR #215原生缺陷对照：`0a69bcdd3c2644a6b9e73638fc153829`预期失败，实际成本14/9的同HP/同回合/同药水数完整胜利。独立修正`805b083ba9b14df89d0fb85f71a16436`及扩展合同`0bfdd959a37f49cda8f24f3adcf1a39c` Passed：内部/共享保留较便宜分支、同成本剪枝、缺失成本保留、零药胜利界、实际药水完整Continuation、父分支/live/RNG隔离。20项胜利界及143项续搜纯合同通过，普通120秒上限及实例清理。 长期入口`POTION-COST-INCUMBENT`原生`8cdb3b3ff63540c3ae04b52760d33065` Passed；结构/工具/覆盖/文档检查通过。未纳入本分支，不作新整请求性能或RSS结论，原始失败与未验证项见[组合研究](performance/heavy-scene-compositions-20261004.md)。

TheHunt／零额度成长研究：`88f095e8686344f797bef57f7e9d9048` Passed（致死／非致死、未生成战后奖励、金币、第三回合、16 Fork、完整状态／RNG隔离）；`5712177f46554f3bb26229f0de3413d2` Passed（实际成长1一药完整胜利、14／9成本门禁、严格HP、未知来源及DOP2同预算质量）。初始完整请求50.93秒未达标；闭合层失败由独立CLR dump证实原型Godot日志错误，修正注入诊断后51.88秒、战损47资源相同，复用未变的原生成功证据。空见证表扫描 `0ce1537149d1477b8e78b1777225211f` Passed，但完整请求48.67秒战损升至62，拒绝纳入。没有最终交错／固定回归或部署。失败、范围与依赖见[研究记录](performance/hunt-zero-growth-proof-research-20261005.md)。

容器拥有者标记研究：c7b320d6…Passed，覆盖列表／字典／集合三代隔离、有序旧枚举、比较器、空容器、16并行子分支，以及实际完整P0先导／请求账本／DOP2严格重播与live/RNG。dev08串行A→C仅16.06→15.83秒、分配−0.149%，未达原始两倍目标；未做最终交错、固定回归或部署，见[搜索成本研究](performance/regent-search-cost-research-20261005.md)。
