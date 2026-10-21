# 接续说明 / 0.3.8

0.3.8已提交、快进合入main并推送，原工坊3823282901更新完成；干净源码S=e5934b8695fa38c81ea6df13b3948c43034b3c9a，后续发布证据文档提交不改变核心包。修复选牌说明遮挡、人物比例和商店形象；最终来源、完整回归466项、实际Steam下载与工坊加载结果见[发布报告](../tests/RELEASE-038.md)及STATUS。

稳定接续分支为main。保留未提交改动，git fetch后仅快进同步，不硬重置、强推或覆盖本机配置。各PC独立核对游戏v0.107.1/build23811903、BaseLib精确3.4.7、SDK10.0.402、Godot.NET4.5.1和PowerShell7。

正式包来自干净源码提交S，后续文档提交不改变S核心包。Build-ReleaseCandidate只包装已校验核心，不隐式构建。工坊保持同一ID和依赖3737335127；本轮API实测为好友可见，与旧文档的private不同，更新配置visibility=null保留实际状态。通过正常Steam下载核对文件，不手工写Steam缓存、不复制用户存档。

统一测试使用Invoke-ModTests，Headless及本轮Process对象清理由入口保证。SavePrepare/SaveResume只在涉及磁盘行为时使用隔离命名空间。图形反馈来自用户试玩，开发和发布授权不自动视为图形测试许可。

本次实现与开发失败见[修复报告](../tests/PRESENTATION-FIXES-038.md)。0.3.8干净包Full466项、实际工坊Load/Character77项/Visuals87项分别记录；不要混用旧开发包结果。一次旧开发包脚本注册异常未确认根因，严格检查器不得忽略。PC-A当前从实际工坊加载，本地重复副本备份在.local/release038/local-overrides；主工作区未跟踪的平衡分析文档未纳入此发布。
