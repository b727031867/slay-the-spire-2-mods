# 工坊仅限好友设置 / 2026-10-21

用户要求后续与朋友一起联机测试，开发和测试期间保持仅限好友。该要求替代旧的private策略；历史发布报告保留当时事实。

- 现有条目：3823282901，Slay the Spire 2 app 2868840。
- 变更前远端：k_ERemoteStoragePublishedFileVisibilityPrivate。
- 使用现有Steamworks.NET 15.0.1探针新增的显式friends-only命令，仅提交可见性字段；校验当前账号拥有该游戏条目。未重传Mod文件或修改远端标题、描述、预览图、依赖。
- 提交PID17672：submitted=true，result=k_EResultOK，agreementRequired=false，退出0。
- 独立远端复查PID20396：result=k_EResultOK，visibility=k_ERemoteStoragePublishedFileVisibilityFriendsOnly，退出0；查询禁用缓存。两个进程均已退出。
- 探针Release构建通过，0警告、0错误。配置JSON与准备脚本使用friends_only，脚本拒绝其他可见性；项目约定、开发说明及跨PC接续说明已同步。
- 本地原始输出：.local/friends-only/change.out、after.out。未将账号信息或原始Steam日志入库。

后续每次上传前核对实际工作目录workshop.json，避免复用旧private元数据；上传后读取远端确认FriendsOnly。只有用户明确另行要求才更改可见性。

本次未启动游戏，未测试多人兼容性、图形或自然流程；好友联机测试结果需另行记录。工坊页面旧描述随下次正式更新使用新的仓库文案。