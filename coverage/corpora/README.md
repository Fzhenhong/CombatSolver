# 固定语料

| 目录 | 用途 |
| --- | --- |
| [equivalence/turn-start-60](equivalence/turn-start-60/) | EQ 10、FULL 40、GA 10，共 60 对请求和规格；当前策略对照引用其中代表根 |
| [strategy](strategy/) | `p0.json`、`p2.json`：固定策略批次、预算和对照输入 |
| [novelty](novelty/) | 固定训练/留存局面与排序配置 |
| [runtime-gc](runtime-gc/) | GC 启动配置和压力局面 |

修改路径时同步请求内的 `generatedScenarioPath`。历史记录中的哈希仍指当时输入；语义内容与历史验证范围保持各自来源。

策略语料中的 `.local/issue-bundles/` 引用需要维护者提供对应问题包，源码仓库不包含这些包。目录检查只核对已提交引用，缺少本地问题包不代表可以跳过该根或宣称对照通过。

运行方法见 [固定根工具](../../tools/search/StrategyCorpus/README.md) 与 [离线宿主](../../tools/search/OfflineSearchHarness/README.md)。新批次结果写入 `.local/`，不在本目录追加流水账。
