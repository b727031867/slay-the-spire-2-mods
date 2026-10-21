# 0.3.5 / PC-B / 能量提示贴图修复

本报告记录0.3.5核心包在PC-B的无GUI修复专项。当前交付状态为用户试玩通过、main已推送、原私有工坊已更新；下载与工坊验证见[发布报告](TASK-011-PC-B-release-035.md)。

用户报告开局随机遗物的悬停/查看界面出现巨型罗盘。根因不是随机遗物自己的图片：TravelerCardPool、TravelerRelicPool、TravelerPotionPool将高清compass.png同时当作BigEnergyIconPath与TextEnergyIconPath。BaseLib的能量行内格式不指定缩放，原生HoverTipFactory也依本机角色卡池取得能量图片，故原生遗物同样受影响。

三个池改为原生ironclad能量前缀，分别使用原生行内图标与能量提示图标，与现有能量计一致。没有新增全局UI补丁、裁剪/替换罗盘插图、改遗物效果或存档，也没有恢复用户已要求删除的左上面板。

干净源码S `628cd327cc1eeb24c7c9cbc8336491151c3a3561`；包 `Sts2Mod-0.3.5-v0.107.1-628cd327cc1e.zip`，SHA256 `D2983666695385EDD08805286084CC3930332241E394E83E79F36CC491CE26AA`。游戏v0.107.1/build23811903，BaseLib3.4.7。C#编译0警告/0错误；Godot导入/导出成功，退出时已有的EditorSettings::_EDITOR_GET诊断单列，不冒称整个构建没有引擎诊断。日志`.local/logs/build-035-clean.log`。

49项纯规则、26项日志分类通过。部署前确认没有用户游戏进程；旧本地包可恢复备份`.local/backups/20261020T1933444319328Z`。

原生无GUI专项49项通过：15选人准备、11能量专项、23原有状态图标/悬停预览检查。新增覆盖三个池、原生烛台获取后及未持有说明、另一原生遗物提灯、持有能量药水、旅行者持有祭品卡，以及实际NHoverTipSet节点尺寸。采用确定性原生获取，不声称重现了用户的随机奖励抽取流程。行内图24×24、提示图74×74、提示框360×276；未知错误0、已知原生无GUI诊断0。测试日志`.local/logs/game-20261020T1933445573193Z.log`，原生隔离沙盒`b5f213abeaec446f8870fce01a9710b6`。

```text
STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN
STS2MOD_VISUAL_COMPLETE assertions=34 panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN
STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=49 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False
```

Test-ComboVisualRun严格核对通过；AI测试PID29676已结束，没有启动图形游戏。部署的DLL/JSON/PCK/build-info四文件与构建包逐一哈希一致。源码S之后只记录文档和Godot自动生成的脚本UID，不重建包或混淆来源。未重跑完整卡池/磁盘回归；本次自动化未执行图形渲染或人工鼠标悬停，多人与用户旧档迁移未验证；用户随后报告试玩通过，作为独立证据。
