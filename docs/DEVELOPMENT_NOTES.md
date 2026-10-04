# CombatSolver 开发笔记

这里只记录当前未发布的行为变化。发布定稿后将该批次完整移入历史卷；已有章节中的错误直接修正，不追加互相矛盾的“后续说明”。

历史记录见 [归档索引](archive/development/README.md)。0.49.0 批次定稿与 PR #203、#204 合并记录见 [历史卷 12](archive/development/volume-12.md)，战斗状态修复记录见 [历史卷 11](archive/development/volume-11.md)。

玩家更新日志见 [0.49.0 更新日志](releases/0.49.0-RELEASE_NOTES.md)；日志站逐包处理及保留首因见 [排查记录](issues/0.48.0-hardbugs-20261003.md)。

0.49.1 紧急修复定稿见 [历史卷 13](archive/development/volume-13.md)，玩家说明见 [0.49.1 更新日志](releases/0.49.1-RELEASE_NOTES.md)。

0.49.2 全平台发布定稿见 [历史卷 15](archive/development/volume-15.md)，内容性 Mod 提示初始记录见 [历史卷 14](archive/development/volume-14.md)，玩家说明见 [0.49.2 更新日志](releases/0.49.2-RELEASE_NOTES.md)。

0.49.3 框架与局外 Mod 兼容性定稿见 [历史卷 17](archive/development/volume-17.md)，玩家说明见 [0.49.3 更新日志](releases/0.49.3-RELEASE_NOTES.md)。

## 下一版本（开发中）

当前没有待发布的行为改动。

## BaseLib 生成牌回归合同（2026-10-04）

完整修饰器合同的失败来自 Testing 的监听时点预期：新牌完成战斗域登记后还未进入任何牌堆，断言却要求其修饰器已经参与战斗 Hook。原生 BaseLib 3.4.7 的 `BaseLibCardModifiers` 订阅器枚举玩家 `AllPiles`；预测监听表同样按牌堆成员生成。

生成牌核对现在逐项报告修饰器数量、独立身份、Owner、Amount、DeckVersion 和移除标志。生命周期合同直接调用原生 `CardModel.CreateClone`，对比预测与原生状态键、入堆前零个 listener、依次进入 Hand／Draw／Discard／Exhaust／Play 时各一个 listener，以及离堆后的零个 listener；结束后检查实机状态恢复。原有侧表发现、动态挂载／移除、指纹、选牌键、续用和 Fork 检查完整执行。

失败基线 runId `ef33630d6b52447b9279fefce87117e2`，21.952 秒，拆分断言明确失败于入堆前的 listener 要求，其他生成牌字段通过。修正后的完整 `PR18-FOREIGN-ONPLAY-BOUNDARY -VerifyBaseLibCardModifierBoundary` Passed，runId `646fb5504eb445efbaa14fe8068f84b0`，23.892 秒，输出 `BaseLibGeneratedClone:NativeState:Created:Hand:Draw:Discard:Exhaust:Play:Removed`。同进程原有 OnPlay 边界及一次普通原版求解部署 Passed，玩家 HP=80，敌人 HP=0；测试修饰器在求解前移除，这不是任意修饰器效果的整场适配证明。

两次原生运行均加载安装的 BaseLib 3.4.7 与 RitsuLib 原包，采用已提交 Executor 的隔离编译输入，保留工作区原有临时接线。临时启动器使用已有映像查询入口，保留进程身份、租约与清理检查；总超时 120 秒，实例均已删除。复跑入口与范围见 [测试矩阵](TEST_MATRIX.md#0493-框架与局外-mod-兼容性2026-10-04)。本轮源码修改属于 Testing；既有克隆、监听与兼容门禁实现通过原生合同验证。
