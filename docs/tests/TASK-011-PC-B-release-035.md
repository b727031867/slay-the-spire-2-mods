# 0.3.5 / PC-B / 远端与工坊发布

2026-10-21用户明确确认0.3.5试玩通过，授权提交远端、更新交接文档及发布Mod到工坊。保持原条目3823282901/private与BaseLib依赖3737335127，不合并main、不新建条目。

## 已试玩核心包身份

可执行源码S `628cd327cc1eeb24c7c9cbc8336491151c3a3561`；包`Sts2Mod-0.3.5-v0.107.1-628cd327cc1e.zip`，SHA256 `D2983666695385EDD08805286084CC3930332241E394E83E79F36CC491CE26AA`。本轮只更新交接、工坊文案及说明图版本，不重建核心包、不改变玩法。

原包49项无GUI专项（11能量/23元素UI与预览/15选人）、49纯规则、26日志分类通过，原报告[TASK-011-PC-B-run-03.md](TASK-011-PC-B-run-03.md)。用户随后人工试玩通过，不等于多人、所有自然Boss平衡或旧档迁移通过。

## 发布准备

远端交接分支`codex/TASK-011-combo-visuals`，入口[另一台PC接续提示词](../handoffs/NEXT-PC-CONTINUE-PROMPT.md)。另一PC尚未同步或运行。本轮只对当前分支普通推送，不强推、不合并main。

官方ModUploader v0.2.0 Windows x64 ZIP SHA256 `2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`，Probe使用官方随附Steamworks.NET15.0.1，包SHA256 `E32DA6B4199384A1AAA15169514FD957B9FF8BA5BDC487BFED6F91E856DC01A6`。上传前API确认同条目、private、consumerApp2868840、订阅状态5；旧工坊目录曾可恢复移动，仅元数据installed=true不视为实际安装验收。

本轮准备使用同一S的四个核心文件，另附当前玩家指南、更新记录和素材说明，共7文件。封面和规则说明图更新0.3.5，均小于1MB，已本地检查无裁切。上传、实际Steam下载、CDN与加载结果待本轮后续记录；此段不预先宣称发布成功。
