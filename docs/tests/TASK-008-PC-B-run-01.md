# TASK-008 / PC-B / 0.3.0

日期：2026-10-20。用户批准12张新牌，执行范围为全部实现、测试与本机部署。新卡及能力使用原创矢量占位美术，图形观感/自然整局平衡待用户试玩。

## 产物身份

源码S：`217257a4a0a4fb263779d63e0006410c5867a2a2`；Mod0.3.0，游戏v0.107.1/build23811903，BaseLib3.4.7。分支`codex/TASK-008-card-pool`。

干净包：`Sts2Mod-0.3.0-v0.107.1-217257a4a0a4.zip`。SHA256：`D4735FE58DC61812AE6970FD7114DA41D34D3786308A603200B0B95218178421`。锁定还原、ExportRelease/win-x64编译、Godot导入及PCK导出成功，0警告/0错误；四组中英文键及PowerShell脚本语法检查通过。

## 实现边界

- TC_C07～TC_C18全部加入旅行者正常奖励池：7攻击/7技能/4能力，稀有度8普通/7罕见/3稀有。基础打击/防御另计，起始10张不变。正常商店的能力栏和Boss稀有奖励不再依赖缺类型补位。
- 四张能力不带元素。连携实际奖励计数与已占用连携次数分开：无目标、抽0、追加攻击0伤害不发能力奖励；原生攻击被格挡的正伤害仍计入。派蒙仅翻倍基础连携。
- 磐岩之心记录回合结束已有格挡预算，在原生清除格挡时保留余额；不重新加格挡。壁垒/模糊等完整保留效果优先，份数增加保留上限。
- 风指引按活跃份数每回合各一次；同回合后来打出的新份数可在后续有效风雷连携触发。雷霆余响每次有效岩雷追加伤害后一次非攻击群伤。元素共鸣按每回合第3次实际连携奖励回能。
- 三相终击使用原生CalculatedDamageVar，合计伤害预览与力量/虚弱/易伤采用同一原生计算。旅途筹谋使用原生FromHand选择与SingleTurnRetain，升级可选0～2张，空手直接返回。
- 无GUI自动选牌仅在显式测试范围内用原生ICardSelector临时作用域，正常模式不注入自动选择。

## 验收

首轮开发包新卡专项125项通过（含角色共170项）。随后补充动态伤害预览、零伤害及敌方消耗格挡的用例。最终干净包的完整回归327项通过，其中新卡专项129项；`Test-ExpandedCardRun.ps1 -Full`通过，未知错误0、已知原生无GUI诊断3。

纯规则49项（含729种六牌序列）及日志分类20项再次通过。完整游戏日志：`.local/logs/game-20261020T1416273949242Z.log`；测试进程33676已结束。

```text
STS2MOD_EXPANSION_COMPLETE assertions=129 cards=18 powers=4 multiplayer=NOT_RUN rendering=NOT_RUN
STS2MOD_TEST_COMPLETE assertions=327 multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave=False
EXPANDED_CARDS_PASS cards=18 powers=4 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN
```

原生磁盘准备13项、恢复/受控连续战斗275项通过。准备进程27828、恢复进程2480均从原生退出路径自行结束；重新部署完全相同的S包后，在新进程恢复同一个隔离存档。18种牌的基础/升级、遗物与金币精确一致，两次加载无重复注册或陈旧元素，派蒙不再次进入可获取池。正常战斗奖励取得TC_C18，商店取得TC_C05和派蒙，未修改实际报价和奖励池。

准备日志`.local/logs/game-20261020T1426064120693Z.log`，恢复日志`.local/logs/game-20261020T1427165289393Z.log`。两阶段未知错误均0，原生退出诊断各9，按既有严格分类器分列，不声称已修复原生资源退出诊断。

```text
STS2MOD_E2E_PREPARED assertions=13 cards=18 upgrades=18 paimon=1 shouldSave=True boundary=combat_entry
STS2MOD_SOAK_COMPLETE seed=TASK003A battles=6 turns=24 actions=93 combos=54 natural_wins=0 energy_injection=false hand_injection=false bounded_turns=4
STS2MOD_E2E_COMPLETE assertions=275 acquisition=PASS disk_restart=PASS soak=PASS multiplayer=NOT_RUN ui=NOT_RUN
```

连续战斗使用受控卡组与5000生命，每场4回合后强制清理进入下一场，6场均非自然胜利；期间未注入手牌/能量。不能据此判断自然整局或平衡通过。旅途筹谋在测试作用域使用原生ICardSelector自动选择，正常游戏仍为原生人工选择界面。

最终普通启动（不带测试/调试/自动选牌参数）也通过，仅一个Mod初始化、BaseLib3.4.7及资源校验正常并到达主菜单，未知错误0。日志`.local/logs/game-20261020T1429294639264Z.log`；本轮启动的5876进程已清理，所有本轮测试进程均已结束，未自动启动图形游戏。

本机保留相同S包部署。重新部署备份为`.local/backups/20261020T1427164325652Z`。源码和交付记录保留在TASK-008分支，未合并main或上传工坊。用户可从Steam正常启动，选择旅行者新局试玩。

完整原始日志只留本机忽略目录，不上传凭据、存档或用户档案。新建测试局、磁盘准备/恢复均使用独立原生命名空间，无Steam云后端。多人、用户旧存档迁移、图形交互与自然整局不由无GUI结果推断。
