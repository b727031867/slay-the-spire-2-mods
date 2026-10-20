# TASK-005 接续与试玩

分支：`codex/TASK-005-experience`，未合并main。候选0.1.3；游戏v0.107.1/build23811903及BaseLib3.4.7。

源S：`1ec8a6a64354c40e84aa84576e12d6302b0445ef`。
候选ZIP：`Sts2Mod-0.1.3-v0.107.1-1ec8a6a64354-candidate.zip`；SHA256 `AF63876A300ACE586EA91360441CC4E629F5D224E0B2A978E1FD376FAD8BB9BD`。

[草稿包下载](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-1baded6279d5fa08596b)。46项纯规则、13项日志分类、122项同工坊包真实游戏断言（94+28）及普通启动通过，运行ERROR为0；无GUI素材/生命周期验证不等于图形观感通过。全部测试进程已结束。后续工具及报告提交不改变包来源S。

同一[私有工坊条目3823282901](https://steamcommunity.com/sharedfiles/filedetails/?id=3823282901)已更新为0.1.3。等待Steam处理完成并确认游戏模组版本后再试玩；不要与本地副本重复安装。另一台PC保留本地改动后同步任务分支，读取[报告](../tests/TASK-005-PC-A-run-01.md)，开发环境仍各机独立配置。

重点人工确认：派蒙提示8格挡/2抽牌是否与实际一致；无目标岩雷预览是否易懂；六牌文本是否拥挤；新卡图裁切与小手牌辨识度；透明遗物图标可读性；三种短特效是否在正确位置且不遮挡意图。不要将无GUI生命周期检查视为动画观感验收。

生成图来源及完整提示词：[素材记录](../ASSETS.md)、[提示词](../art/TASK-005-prompts.md)。七张运行时PNG在src/Sts2Mod/Sts2Mod/art；原SVG保留历史。普通启动不需要开发工具，测试能力默认关闭。

复测：Start-ModGame -TravelerTest -Headless；完成后使用Test-ExperienceRun -LogFile返回日志 -CodeCommit完整S。只清理本轮测试进程。公开发行和合并main仍需另行指示。
