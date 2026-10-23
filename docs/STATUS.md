# 当前项目状态

本页是开发版本、已发布版本、接续分支和待办的唯一入口。README、开发/协作说明和交接文件只链接本页；版本历史见[更新日志](player/CHANGELOG.md)，历史测试报告保留当时事实。

更新：2026-10-23。本页记录已知证据，不代表实时查询Steam或确认各PC当前安装。

| 项目 | 当前记录 |
| --- | --- |
| 开发/接续分支 | main；后续工作从main新建独立任务分支 |
| 已发布版本 | 0.3.14 |
| 核心源码 | b3764e6b2be5fa6bafddd6a3f27db0709d643b01，dirty=false |
| 当前内容 | 27张奖励牌、5种能力、7种专属遗物模型 |
| 工坊 | 原条目3823282901；仅限好友friends_only；依赖3737335127 |
| 下一步 | 新需求按任务分支开发；发布时使用统一准备入口和冻结快照 |

0.3.14已完成审查、合入main并更新原工坊。干净包Full636项（含新增专项105项）、SavePrepare13项、同包SaveResume258项，以及实际工坊Load、Character90项和Visuals128项通过；下载七文件和两张CDN图片哈希一致。人工制品拦截灌注负敏捷为用户确认的预期交互。包身份、哈希和范围见[发布报告](tests/RELEASE-0314.md)与[审查报告](tests/REVIEW-0314.md)。本轮图形交互、联机/重连、自然Boss平衡及用户旧档迁移未系统验收。

2026-10-23完成状态入口统一、发布快照冻结和发布准备串联。90项离线断言通过，WorkshopProbe在固定.NET环境下编译0警告0错误；未因本次流程改动重新运行真实游戏或上传工坊。实现和边界见[流程验证](tests/RELEASE-PIPELINE-2026-10-23.md)。

发布准备入口只到PREPARED，不代表上传或上线。它校验并部署核心包，运行无GUI Full，复核核心字节，再冻结核心、文档、工坊配置与图片并生成仅限好友上传目录。操作见[开发说明](DEVELOPMENT.md)与[工坊说明](../workshop/README.md)。涉及磁盘行为时另跑SavePrepare/SaveResume；图形、联机、自然整局和旧档迁移结果单列。

长期约束：自动游戏测试无GUI，使用隔离存档和无测试云后端，只清理本轮进程。工坊上传前核对实际目录为friends_only，上传后用WorkshopProbe确认FriendsOnly；历史private或visibility=null不作为后续配置。可见性来源见[设置验证](tests/WORKSHOP-FRIENDS-ONLY.md)与[AGENTS.md](../AGENTS.md)。
