# 建筑师通关结算卡住修复 · 2026-10-23

用户实机通关后卡在建筑师房间，无动画且无法结算。只读检查用户godot.log发现TheArchitect.WinRun空引用，原流程在访问Dialogue.EndAttackers时中断，尚未调用ActChangeSynchronizer.SetLocalPlayerReady。

最终状态：已修复并发布工坊，FriendsOnly确认；干净核心0663019的Character105项通过，Steam下载七文件与上传快照一致，PC-A仅使用工坊副本。用户原存档完全保留，由用户继续原局验收。

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

## 最终发布与验证

- 干净核心066301992d118688610ead6dc1aa271a4b0a8b24，dirty=false，PC-A；版本0.3.17，游戏v0.107.1/build23811903，BaseLib3.4.7。C#构建0警告0错误，Godot无GUI导入/导出成功。
- 正式同包Character105项PASS，含VICTORY_dialogue_available_{0,1,2,10,100}、isolated_unsaved_fixture、native_architect_room、native_win_and_summary。原生建筑师事件已实际接入一条旅行者对话，经过结尾双方攻击任务、原生ActChange投票与WinRun后进入可见NGameOverScreen；没有调用测试替代结算。
- 日志game-20261023T1352599774119Z.log与同名.result.json；未知错误0、已知headless诊断0。测试进程22152已由finally清理，隔离目录9902d82a61774b41a6f4615526b6c8d3。正常用户存档未复制到测试夹具、未修改。
- 本次仅改两份生产本地化资源，使用现有BaseLib扩展，无生产C#补丁；不跑Full/Cards或磁盘专项。shouldSave=false原生路径证明结局任务与结算屏可达，不宣称已验证用户原局历史写入、真实联机或GPU画面。用户明确自行使用原存档通关验证。
- 核心ZIP SHA256：B9891746192750F1252BC6B0FC38215E9CB4A588F13BFC2EECC2A7E7EE78A16E。候选ZIP SHA256：77EE0C1561C250C554C5EE8F47A47DB50B67D66A137B2A9A737BD4C2B99EC252。快照SHA256：1E06868C2183DA8CDFB40EDAA5A9F7B5D5625C593DA314149A1D81647D6F9BEB。
- 官方ModUploader v0.2.0，归档SHA256 2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。实际上传目录.local/workshop/victory-20261023-release，workshop.json检查friends_only，mod_id.txt及显式-i均为3823282901；上传七文件与快照一致。退出码0且Successfully uploaded，未创建新条目。
- 上传后WorkshopProbe实时两次确认FriendsOnly；Steam原生下载完成，state=5，安装时间戳1792763692，总大小90353819字节。七文件SHA256与上传快照全部一致，见.local/victory-fix/download-hashes.json及inspect-final.json。
- 原工坊BaseLib/Sts2Mod安装完整备份于.local/victory-fix/workshop-before，逐文件哈希在workshop-before-hashes.json。BaseLib原缓存已按原哈希还原；本地覆盖包移到.local/victory-fix/local-overrides，PC-A仅使用已下载工坊版本，不手工复制新包到Steam缓存。
- 正常存档不改、不重置进度；重新启动后由用户继续原存档验证建筑师结局。测试进程均已结束，没有启动图形游戏。

### 已发布核心四文件SHA256

- Sts2Mod.dll：A55BE6BA60D8A9CB9B17E7FD49EC394AF2DDDECAC4F2178F5EB5C0CC1AA1B3C2
- Sts2Mod.json：0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A
- Sts2Mod.pck：5E6637213B5144BBC47769B803FD49391BBE79AF1039DEB7CFF67281729A3DE2
- build-info.txt：851BD10AF5FBA6902006142BB7C8A043CBE280B9B9EA8570C488919DBA54D6F9
