# 建筑师通关结算卡住修复 · 2026-10-23

用户实机通关后卡在建筑师房间，无动画且无法结算。只读检查用户godot.log发现TheArchitect.WinRun空引用，原流程在访问Dialogue.EndAttackers时中断，尚未调用ActChangeSynchronizer.SetLocalPlayerReady。

## 影响评估

- 基线：已发布458356c，游戏v0.107.1/build23811903，BaseLib3.4.7，PC-A。
- 根因：原生建筑师只查角色专属对话（allowAnyCharacterDialogues=false），旅行者没有ancients条目。LoadDialogue得到null，原生允许显示PROCEED但WinRun直接解引用。缺失对话不是无限循环动画。
- 修复：使用BaseLib现有AncientDialogueUtil/PopulateLocKeys扩展，两语言各添加一条旅行者专属可重复对话（0-0r）及0-endattack=Both。首胜与后续胜利都有合法Dialogue，保留原生玩家攻击、建筑师反击、投票同步和结算逻辑，不补丁替换WinRun，不伪造胜利或修改用户存档。
- 影响：建筑师本地化资源及原生结局入口；不改共享战斗、模型ID、存档格式。没有新增生产C#逻辑。
- 检查：构建、导入导出＋Character无GUI专项，新增0/1/2/10/100胜对话选择、原生最终幕入口/选项/动画任务结束与胜利结算屏检查；Full末尾也保留该用例，不默认跑完整玩法回归。
- 隔离：仅使用既有测试存档隔离机制、shouldSave=false受控夹具，不复制正常存档。无GUI不证明画面观感、真实联机或用户存档历史写入。
- 原游戏已由用户退出；当前正常存档不动，修复后用户可自行继续原局验证。

## 开发验证与发布授权

- 用户明确要求修复后直接发布工坊，并自行使用原存档进行通关验证；保持条目3823282901与friends_only，不变更存档或代替用户操作原局。
- 开发包Character专项105项PASS，其中结局8项覆盖对话可用性、原生最终幕入口、动画任务完成和胜利结算屏；日志game-20261023T1350057366522Z.log，测试进程7740已清理，未知错误0。原生ActChangeSynchronizer确实打印All players ready并完成下一阶段。
- 正式发布包使用干净源码构建，按影响范围验证Character，不跑Full/Cards或磁盘专项；用户原存档验证与无GUI夹具分别记录。
