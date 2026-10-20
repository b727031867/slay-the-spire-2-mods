# TASK-009 / PC-B / 0.3.1

2026-10-20实施，2026-10-21完成最终验收。用户已确认随机追击与全部项目图片占位替换，Boss数值方案先评审。本轮没有数值改动。

## 产物

最终源码S2：`68c8cf864e70896e69b0a7958c8dae9ac37cc191`，分支`codex/TASK-009-combo-art`。Mod0.3.1 / 游戏v0.107.1、build23811903 / BaseLib3.4.7。全量回归基线S1：`34fe175480f13c04986e78f68bbbc360474cb9b6`。

最终干净包：`Sts2Mod-0.3.1-v0.107.1-68c8cf864e70.zip`，SHA256 `5FFC7FD6AF2F48B12A6588E10F5940899A6D51FD1C63BA5D01631023FACFA949`。锁定还原、ExportRelease/win-x64、Godot导入/打包成功，0警告/0错误；19项PNG已逐项读取、尺寸/哈希入档，4张透明图透明角通过。静态联系表已检查，无文字水印或几何占位，角色与头像一致。S1的历史ZIP哈希为`900B95B2DD435065989E872891F1B60A663AECB4D2691055B0E19B0F04507FA4`，不作为交付包。

## 规则与素材

岩雷技能使用原生TargetingRandomOpponents，只从存活敌人中选一个。选择发生在卡牌基础效果后，沿用CombatTargets种子/计数；单体攻击仍用原目标，死目标不改选。原生DamageCmd.Attack保留力量、虚弱、易伤等修正，按实际结果驱动能力/罗盘。次数、派蒙边界及所有卡牌数值不变。

新美术19项是生成式PNG，不是官方角色立绘或原生游戏素材。全部项目C#/TSCN不再引用SVG；旧SVG归档保留。复用原生营地/商人、公共能量及音效演出仍保持，不声称新增完整骨骼/多人动画。[提示、联系表与哈希](../art/TASK-009-prompts.md)。

## 开发期纠正

- 首轮开发全量在`TC08_mixed_combos_share_three_cap`停下：旧预期仍少算防御追击新增的4伤害。改为两敌总伤害28（原24），格挡25和三次上限保持；不是放宽次数断言。
- 新增随机数专项首轮误把玩家对象传给原生Self卡手动动作。原生动作要求显式目标null，因此牌没有打出；纠正夹具提交方式，没有修改正常游戏目标校验。
- 这些失败日志保留本机，不计作最终通过证据。测试不向用户窗口发送输入，不接触用户存档。

## 验收

49项纯规则、20项日志分类、PowerShell语法及中英Power键集检查通过。

S1包先在专项入口通过19项PNG加载、312px配置和11项随机追击检查，证据`.local/logs/game-20261020T1511292081772Z.log`。在随机专项完成后主动停止26732进程，切换同一包的全量回归；不将这次提前结束视为完整新卡专项通过。

S1的358项完整游戏回归通过（含160项新卡/随机/图片），日志`.local/logs/game-20261020T1513047901979Z.log`，检查器`Test-ExpandedCardRun.ps1 -Full`通过；未知错误0，已知原生无GUI诊断0。该包清单为0.3.1，但程序集版本仍标记0.3.0，不作为最终交付包。随后只统一csproj版本为0.3.1，并为构建加入版本一致性校验；`git diff S1 S2 -- src/Sts2Mod`确认唯一变更是csproj版本一行，游戏C#逻辑与图片完全相同。

最终S2包专项205项完整通过，其中新卡/随机/美术160项，包含四种防御牌基础/升级、普通与派蒙、11项随机追击边界及19项图片。检查器`Test-ExpandedCardRun.ps1`通过；版本标记为`0.3.1+68c8cf864e70896e69b0a7958c8dae9ac37cc191`，未知错误0，已知原生无GUI诊断0。日志`.local/logs/game-20261020T1524529779754Z.log`，测试进程25824确认启动时间/路径后已结束。

```text
STS2MOD_EXPANSION_COMPLETE assertions=160 cards=18 powers=4 multiplayer=NOT_RUN rendering=NOT_RUN
STS2MOD_EXPANSION_FOCUSED_COMPLETE assertions=205 cards=18 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN
```

S2原生磁盘准备13项通过，日志`.local/logs/game-20261020T1734240874868Z.log`，进程3756原生退出；正常奖励/商店、18种基础与升级模型、派蒙唯一性及隔离原生保存均通过。准备后重新部署完全相同的S2包，备份为`.local/backups/20261020T1735530644662Z`，再用新进程恢复。未知错误0，已知原生退出诊断9，单列而不声称已修复。

S2新进程恢复/连续战斗269项通过，日志`.local/logs/game-20261020T1735531777898Z.log`，进程3932原生退出。卡牌、升级、遗物、金币及重复注册/陈旧元素两次原生恢复检查通过。六场共24回合、90次出牌、52次连携，1场自然胜利；其余使用四回合预算截断/清理。玩家为受控5000生命，未注入手牌/能量，不能称作自然整局或Boss平衡验证。未知错误0，原生退出诊断9单列。

```text
STS2MOD_E2E_PREPARED assertions=13 cards=18 upgrades=18 paimon=1 shouldSave=True boundary=combat_entry
STS2MOD_SOAK_COMPLETE seed=TASK003A battles=6 turns=24 actions=90 combos=52 natural_wins=1 energy_injection=false hand_injection=false bounded_turns=4
STS2MOD_E2E_COMPLETE assertions=269 acquisition=PASS disk_restart=PASS soak=PASS multiplayer=NOT_RUN ui=NOT_RUN
```

S1早先的磁盘准备仅开发证据，不冒充S2验收。上述S2两阶段均通过`Test-Task003EndToEnd.ps1`严格检查，原始日志留在本机忽略目录，不上传正常存档或用户档案。

最后以不带测试/调试参数的普通无GUI启动验证S2：一次初始化、资源/依赖正常、到达主菜单，清单/程序集版本均0.3.1，未知错误0。日志`.local/logs/game-20261020T1738208772259Z.log`，检查器`Test-ModLoad.ps1 -Headless`通过；25024进程已关闭。所有本轮测试进程均已清理，未启动图形游戏。

交付保持相同S2本地包；源码与报告推送到TASK-009分支，不合并main、不上传工坊。用户可从Steam正常启动，选择旅行者新局试玩；如Steam重新下载旧工坊副本，须避免与本地版重复加载。

## 范围限制

全部游戏测试均后台无GUI，临时存档隔离于Steam云与用户档。资源加载和312px场景配置检查不等同于实机视觉/手动UI通过；自然Boss平衡、多人及用户旧存档迁移未运行。Boss调整仅形成[待评审方案](../design/TRAVELER-LONG-COMBAT-REVIEW.md)，不在本包实现。
