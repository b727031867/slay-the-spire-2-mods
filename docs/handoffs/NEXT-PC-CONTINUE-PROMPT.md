# 接续说明 / 0.3.6

稳定接续分支为main。工程重构已提交并快进合入main、推送完成，原私有工坊3823282901已更新0.3.6。S=0685d66bddf7ec29c604fd61e99c81553fa73aad，dirty=false；包哈希、实际Steam下载、CDN与工坊加载结果见[发布报告](../tests/RELEASE-036.md)。后续文档提交不改变S核心包，不重复构建或上传。

先读根目录AGENTS.md、STATUS、DEVELOPMENT及玩家指南。保留未提交改动，git fetch后仅快进同步，不硬重置、强推或覆盖本机配置。各设备独立核对游戏v0.107.1/build23811903、BaseLib精确3.4.7、SDK10.0.402、Godot.NET4.5.1和PowerShell7。

正式包必须来自干净源码S并记录哈希；Build-ReleaseCandidate只包装已校验核心，不隐式构建。工坊保持同一私有ID和依赖3737335127，通过正常Steam下载核对实际文件，避免本地/工坊重复加载。不复制其他设备路径、凭据或用户存档。

统一测试使用Invoke-ModTests，Headless及本轮Process对象清理由入口保证；SavePrepare原生退出后重新部署同一包，再以同一SandboxDirectory执行SaveResume。原生控件检查、人工图形、受控连续战斗和自然平衡分别报告；旧测试数字不作为新包重跑结果。

战斗演出只处于讨论：用户选择分层2D动画、适中强度。先读[方案](../design/TRAVELER-COMBAT-PRESENTATION.md)，未获实施指令不改动画、数值或增加功能。
