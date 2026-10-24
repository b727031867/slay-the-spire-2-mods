# 跨PC接续入口

先读[当前状态](../STATUS.md)，从中取得开发版本、任务分支、已发布包身份、验证范围和下一步。版本演进见[更新日志](../player/CHANGELOG.md)，不在本页重复维护版本或测试数字。

保留未提交改动；git fetch后检查分支差异，仅在干净且无分叉时快进同步，不硬重置或强推。每台PC使用自己的local.settings.json、合法游戏安装和[技术基线](../TECH_BASELINE.md)，环境或游戏结果不能跨PC直接计为通过。完整协作规则见[协作说明](../COLLABORATION.md)。

每个开发任务从最新共享main建立独立codex/分支，包含修复、工具与文档；按影响范围检查通过后合入main并推送，核对后清理已完成且无独有工作的分支。后续每台PC仅保留最近3份可交付制品及其关联文件，交付时按[开发说明](../DEVELOPMENT.md#制品保留与清理)清理更旧制品和中间包，保留日志、源码、素材、依赖和正常存档。

正式核心来自干净提交S。复用同一包跨PC测试时核对SHA256；重新构建必须记录新包身份。发布快照同时冻结核心、玩家文档、工坊配置与图片；Prepare-Workshop只接收SnapshotDirectory，不读取当前仓库补充发布内容。使用[发布准备入口](../DEVELOPMENT.md)时，PREPARED只表示上传目录已准备。

统一测试使用Invoke-ModTests，无GUI与本轮进程清理由入口保证。磁盘变更按范围另跑SavePrepare/SaveResume。合入main后交付本地候选，用户明确确认对应核心包试玩成功后才发布工坊；未确认时不上传。图形自动化和真实联机仍须用户明确要求，单独记录。游戏测试不复制用户存档、不手工写Steam缓存。

后续上传保持原工坊ID3823282901及friends_only；上传前检查实际workshop.json，上传后用WorkshopProbe确认FriendsOnly。历史报告的private或visibility=null不作为当前操作。实际上传及Steam下载验收见[工坊说明](../../workshop/README.md)。
