# TASK-011 / PC-B / 0.3.4 移除常驻面板

2026-10-21用户截图要求：只去掉左上被框出的界面，悬停卡牌时在原生「本次主动出牌预览」提示即可。用户自行保存退出，未关闭其游戏。

删除ComboHud/ComboPips及NCombatUi._Ready挂载补丁，NPower动态图标补丁独立保留。ElementCardModel与ComboPresentation逻辑/文案保持，卡牌、连携、遗物数值与存档规则不变。无/风/岩/雷原生状态图标继续刷新。没有将面板简单隐藏，也没有留下后台面板轮询。

专项改为原生战斗UI全后代中面板缺席、四种实际状态纹理、原生卡牌HoverTip的格挡/抽牌/单体追击/随机追击，以及派蒙、无法出牌、次数耗尽和跨回合/战斗。100次提示读取不得改元素编码、随机计数、能量或手牌。

最终干净源码S `70e0a584887f7d24b5e377309131bd03b86db08f`，包Sts2Mod-0.3.4-v0.107.1-70e0a584887f.zip，SHA256 `080E9E5D41285454D6C4F290B11FAEE8BBE9E5CA5543B469CB66EF91C189B2D0`。游戏v0.107.1/build23811903、BaseLib3.4.7。C#编译0警告/0错误，Godot导入/导出完成且进程成功；编辑器退出有一条已见EditorSettings::_EDITOR_GET诊断，单列，不声称整个构建无ERROR或已修复该引擎诊断。日志build-034-clean.log。

49项纯规则、26项日志分类及检查器语法通过。相同S包部署前确认用户已自行退出；旧本地包可恢复备份在.local/backups/20261020T1920259389511Z。

专项38项通过（15项原生选人准备及23项本轮界面/提示检查），PID33448、日志game-20261020T1920260585329Z.log，隔离原生沙盒e28e05fdc8e14a52b0f82d28beda3999。Test-ComboVisualRun.ps1严格核对通过；一次初始化0.3.4+S、依赖/资源正常，未知错误0，已知原生无GUI诊断3单列。没有走原生退出流程，不据此声称修复退出资源诊断。

```text
STS2MOD_VISUAL_COMPLETE assertions=23 panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN
STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=38 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False
```

测试进程已清理，没有启动图形游戏。已部署S包，DLL/JSON/PCK/build-info四文件与构建包逐一哈希相同；旧版备份可恢复。源码S后续仅更新文档，不重建或冒充其他包身份。不会把0.3.3的409项回归当作0.3.4重测；本次未重跑完整卡池/磁盘回归，未上传工坊、推送分支或合并main。图形观感仍需用户试玩确认。
