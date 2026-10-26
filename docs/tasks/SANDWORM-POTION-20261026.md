# 沙虫被火焰药水击杀后卡住 · 2026-10-26

## 用户提供的现象

另一台电脑的双旅行者联机对局，火焰药水击杀沙虫后不进入通关/奖励，保存后重新加载提示断开连接、InternalError。截图不是本机，因此本机旧日志不作为事故日志。

## 定向复现与边界

- 用户明确授权复现，不授权全量回归。测试仅用隔离存档、无Steam云后端、后台无GUI；未操作正常存档和用户游戏进程。
- 基线main 57bc569 + 本次诊断代码，dirty=true；PC-A，游戏v0.107.1/build23811903，BaseLib3.4.7。编译0警告0错误。
- 入口：Start-ModGame.ps1 -Headless -TravelerSandworm -PassThru。必须由调用者保留返回Process并负责超时/结束清理；它不是Full套件，也未接入统一回归入口。
- 场景：第二幕Hive，进阶10，固定种子SANDWORM26；铁甲战士1名、旅行者1名、旅行者2名。测试生命5000仅防止夹具中意外死亡；正常卡牌/药水规则不变。
- 双人通过StartNewMultiplayerRun建立两名玩家，但传输层为NetSingleplayerGameService，不连接Steam；读档使用SetUpSavedSingleplayer，不覆盖真实联机读档握手/同步。
- 第一轮让沙虫施放流沙；每名玩家对应一份SandpitPower。把沙虫HP设为1，然后通过原生PotionCmd领取FirePotion，并以UsePotionAction真正使用药水击杀；不使用Kill(force)代替药水。
- 检查药水动作完成、沙虫死亡、战斗房间IsPreFinished、原生NRewardsScreen可见；真实SaveManager写入隔离磁盘，再返回菜单、LoadRunSave/RunState.FromSerializable/LoadRun，核对重新显示胜利奖励。
- 三组各6项，共18项通过，未复现卡死/InternalError。严格分类未知ERROR=0，已知Godot无GUIdummy纹理错误1条（精确来源匹配现有分类器），不改或放宽分类规则。
- 日志：.local/logs/game-20261026T1337039234745Z.log；沙盒72a0deb73c5743198722b87c9129087a。测试进程已结束。
- 初版仅入场/流沙/重载12项通过，日志game-20261026T1332200279227Z.log。早期地图点选择、canonical/mutable和AddPlayerDebug夹具错误另保留原日志，不是玩家故障证据；最终夹具已修正。
- 离线Test-ModTestEntry 12项通过；git diff --check通过。

## 当前结论与下一步

根因未确定，不作猜测修复，不宣称真实联机或图形验证通过。需要发生故障的主机/客机godot.log（已重启则对应时间的godot*.log），以及主机当前联机存档current_run_mp.save的副本；只读分析，保留原文件。若缺少日志，可由用户在那台电脑UI中重载原联机档，报错后立即退出并保存日志，避免再次启动轮转覆盖。截图InternalError本身不含异常堆栈。

此次仅增加显式启动的诊断夹具及入口，不更改玩法，不发布诊断包。遗物图片任务独立保留。
