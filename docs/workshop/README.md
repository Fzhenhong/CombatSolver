# 创意工坊多语言介绍

- [english.txt](english.txt)：English 标题与介绍对应的英文正文。
- [schinese.txt](schinese.txt)：迁移前的中文介绍原文，放入简体中文介绍。
- 条目：`3790899961`，游戏 App ID：`2868840`。

2026-09-13 用户要求保留原有完整简介，仅删除链接；今后的中英文简介不放网址或URL标签，开源入口改用GitHub项目搜索提示。功能、许可署名、依赖、性能说明、交流群和反馈内容均保留，不能用精简草案覆盖原文。发布配置的默认英文description须同步更新。

中英文介绍的依赖版本与 manifest 保持一致。2026-10-03 两个明确语言的 Steamworks 描述更新均返回 OK，只同步介绍，未上传二进制或改动更新日志。

维护时先对更新句柄调用 `SetItemUpdateLanguage`，再设置标题、描述，最后提交。不能用语言标签替代语言字段。未指定语言时 Steamworks 默认写 English；当前官方 ModUploader CLI 没有语言参数，本地 workshop.json 已改为英文标题/介绍作为默认输入，避免之后发包把中文覆盖回 English。更新简中内容时使用明确的 `schinese` 更新句柄。

接口约定：[Steamworks ISteamUGC](https://partner.steamgames.com/doc/api/isteamugc?language=english#SetItemUpdateLanguage)。

示例图顺序为中文、英文 1、英文 2、英文 3。本地上传工作区 previews/ 同步保存 `01-cn1.jpg`、`02-en1.jpg`、`03-en2.jpg`、`04-en3.jpg`；官方上传器会按此目录增删远端示例图，API 更新截图后也必须同步暂存目录。图片原件保留在用户桌面，上传副本为 1920×1080 JPEG，每张小于 1 MB；不包含在 Mod 内容包中。
