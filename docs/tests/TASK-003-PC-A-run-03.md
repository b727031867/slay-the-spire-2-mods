# TASK-003 PC-A 单人无GUI验收报告 03

日期：2026-10-20。任务完成范围为单人无GUI集成验收；UI/人工自然试玩和双人按DEC-004/005延期，最终公开支持联机的发布条件不变。用户本轮明确授权验收后合并main。

## 被测身份与结果

- 干净代码S：`0058ea8eedaa1fe1b00f279c0599f983c3431b9b`，Mod 0.1.1。
- 游戏v0.107.1 / Steam build23811903；游戏程序集SHA256 `A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`；BaseLib3.3.2。
- PC-A Windows x64；.NET SDK10.0.402、net9.0、Godot .NET4.5.1；构建0警告/0错误。
- 包`Sts2Mod-0.1.1-v0.107.1-0058ea8eedaa.zip`，224,465字节；SHA256 `CAD4D0FE257012ABFEAB449DF4A982A2988E986F4014B2F65C991964AD056D36`。来源S与后续报告/合并提交分开记录。

| 检查 | 结果 |
| --- | --- |
| 干净构建、PCK导出及部署 | PASS；0警告、0错误 |
| 纯规则 | PASS；27项 |
| 日志分类正反例、PowerShell语法 | PASS；13项分类及全部scripts/tests脚本解析 |
| 最终同包准备阶段 | PASS；12项；已知dummy texture错误1、已对照退出诊断9、未知0 |
| 重新部署同包、新进程恢复与连续场景 | PASS；255项；已知dummy texture错误1、已对照退出诊断9、未知0 |
| 最终同包全量真实游戏机制回归 | PASS；94项，TASK-002完成位于82；日志ERROR=0 |
| 普通启动自动测试默认关闭 | PASS；同S普通无GUI启动，仅READY，无SANDBOX/E2E/TEST启动标记，日志无未知错误。 |

S之后修正了PowerShell检查器的续行语法（`d4ee293`），并移除售空检查中恒等金币比较、将已验证的入库/金币和售空状态分开断言；联合检查条件和原生流程未改变，重新编译0警告/0错误。卡牌、连招、遗物、初始化和隔离存储代码与S一致；运行包仍按实际S追踪。

ZIP和SHA256已上传[GitHub草稿附件](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-1251651e5497a6e6a63c)，release ID `417945312`；服务端大小及digest匹配，draft=true，未公开发行。

## 实际获取与跨进程保存恢复

准备阶段以固定种子TASK003A开启独立可保存测试局，使用正常地图战斗和受控后续战斗生成未修改的原生战后奖励；由原生RewardsSetSynchronizer选择奖励，调用无窗口场景的原生选牌回调完成入库，不注入键鼠、不修改奖励列表。商店通过普通MerchantRoom及其原始Inventory生成，按真实价格购买，没有指定派蒙ID强行填入商品。

固定种子最终结果：第9场奖励中领取岩之守护TC_C02；第3家商店以49金币购买TC_C02，第22家商店以194金币购买派蒙。取得后两个遗物袋均不再包含派蒙，后续3家普通商店未再次提供派蒙。

六张牌各添加基础和升级版本作为保存覆盖夹具；这十二张牌的调试添加不记为正常获取证据。经原生SaveManager.SaveRun写入磁盘，主动打一张风牌留下未保存的战斗状态，返回原生主菜单并调用NGame.Quit退出。进程结束后重新部署同一份S包，再启动新进程用原生LoadRunSave、RunState.FromSerializable、SetUpSavedSingleplayer及NGame.LoadRun恢复。

对比磁盘外部预期清单：牌组全部ID与升级等级、全部遗物及金币完全一致；派蒙只一份，两个遗物袋中继续不存在；卡/遗物注册各一次。再通过主菜单执行第二次原生读档，重复核对。恢复后的首次岩牌只有基础7格挡，不继承退出前风序列；随后风牌触发翻倍格挡，验证派蒙恢复后有效。

边界是原生战斗入口回滚，不是任意中途回合/动作的续接。测试元数据在`.local/task003-saves/<唯一ID>`；实际run/progress/prefs使用真实GodotFileIo磁盘文件，位于`user://sts2mod-acceptance/<同一ID>`，没有Steam云后端，未覆盖用户的steam/account/profile存档。模组初始化前，游戏仍按原生流程读取本机模组启用设置；测试图形/音频设置显式初始化、音量置零，不验收图形设置持久化。

## 持续战斗与数值反馈

最终同包恢复场景通过255项断言（含两次读档与持续战斗）。6场×4回合，共24回合、84次原生出牌、29次成功连招，约1.21次/回合；自然胜利0，未完成场次按预算由夹具清理。奖励观测：格挡11次、累计增加88格挡；抽牌12次、手牌净增加24；追加攻击1次、敌方生命实际减少8；奖励跳过5次（无有效目标等约束）。不把成功连招次数等同于全部奖励可执行。

这是受控牌组、高玩家生命值、固定遇敌和每场最多4回合的持续机制场景；战斗之间对未自然结束的敌人强制清理，清理不计为自然击杀或正常胜利。抽牌、洗牌、消耗、能量花费及敌方回合使用原生命令；战斗循环不补能量、不直接添加手牌。每步检查状态与单独规则层一致性、每回合最多3次、能量非负、手牌不超10及无无界抽牌循环，检查跨回合/战斗重置。

数值判断：该样本只能说明连续执行与边界稳定性；含每种牌基础/升级的受控牌组和高生命值会扭曲收益/存活率，不能证明实际奖励环境下的强度平衡。因此本轮保留0.1.1现有数值，未据此调整伤害、格挡、抽牌或派蒙倍率。自然整局与玩家手感继续延期。

## 缺陷闭环与引擎限制

- 测试存储最初直接使用Windows绝对路径，原生GodotFileIo拒绝；改为唯一user://命名空间。
- 全新测试档的战斗教学会进入原生未就绪界面；通过测试档原生SetFtuesEnabled(false)关闭引导，不修改用户进度。
- 保存夹具卡牌必须通过RunState.CreateCard关联Owner，正常地图进入必须选择未访问节点；已修正测试准备。
- 对售空格子直接触发再次购买会调用原生界面不支持的FailureOutOfStock回调；改验售空状态，并通过遗物袋及后续商店验证派蒙不重复，不伪造可点击的正常购买场景。
- ModInitializer内部递归读取测试settings会触发原生设置迁移空引用；改为原生测试显示设置初始化，run/progress/prefs仍真实落盘和读回。带上述错误的探索日志全部不作为PASS证据。
- 真实正常地图/读档偶发Godot dummy texture初始化`Parameter "t" is null`；只在headless下同时匹配`texture_2d_initialize`和`servers/rendering/dummy/storage/texture_storage.h`来源分类，玩法或缺来源的同文本仍拒绝。
- ENV-001（非阻塞、保留）：原生无GUI退出仍报告RID/字体/资源在退出时被占用。移除Sts2Mod、仅保留既有BaseLib的对照进程同样复现完全相同的8类原生类型及资源退出诊断，见[对照证据](evidence/TASK-003-native-shutdown-baseline.txt)。新分类只在显式NativeShutdown、已记录NGame.Quit且Steam shutdown成功之后接受这些精确类型，单独计数；运行中同文本和未知类型/错误仍失败。不宣称退出资源释放或内存泄漏已修复。

修复追踪：测试路径P1→`3e2f58e`；教学/Owner/地图准备P1→`5d0606d`；售空回调与dummy来源分类P2→`054c3db`；原生Quit流程P2→`46427d8`；设置递归P1→`7902e56`；退出诊断来源P3→`0058ea8`；检查器续行P1→`d4ee293`。以上测试工具缺陷已闭环，新增生产玩法阻塞缺陷0；ENV-001作为非阻塞环境限制保留。相同退出类型不证明不存在额外资源泄漏，本轮不作内存泄漏验收。

新分类共13项正反例通过；完整玩法回归仍使用原检查器，不启用退出诊断例外。生产卡牌、连招及遗物规则本轮未修改。

## 复现与证据

使用main最新检查器；运行时被测包来源为S，PowerShell检查器的换行解析修正位于`d4ee2930577622eb0cc058ddad1e6d37bbdf2466`，未修改Mod代码或重新标记包。新电脑从包含该修正的main干净检出构建，记录自己的新S/包哈希；或部署本报告所列S包。首先运行`Start-ModGame.ps1 -TravelerEndToEnd -Headless`，记录SandboxDirectory、PID和LogFile。等待原生进程退出，执行`Test-Task003EndToEnd.ps1 -Phase Prepare -LogFile '<准备日志>' -CodeCommit '<S>'`。

随后再次部署同一包，执行`Start-ModGame.ps1 -ResumeTask003 -SandboxDirectory '<先前返回目录>' -Headless`，等待退出，再用`Test-Task003EndToEnd.ps1 -Phase Resume`核对新日志及相同S。使用`Start-ModGame.ps1 -TravelerTest -Headless`独立运行完整机制套件，完成后用`Test-TravelerRun.ps1 -Task003 -Headless`检查该日志及S，再结束本次进程。

最终原始日志保留本机：准备`.local/logs/game-20261020T0230027285287Z.log`；恢复`.local/logs/game-20261020T0231511243441Z.log`；全量`.local/logs/game-20261020T0234054736506Z.log`。普通启动日志`.local/logs/game-20261020T0238141788617Z.log`。无本Mod退出对照为`.local/task003/native-shutdown-baseline.log`。

可分享摘录：[准备阶段](evidence/TASK-003-PC-A-prepare.txt)、[恢复与持续战斗](evidence/TASK-003-PC-A-resume.txt)、[全量回归](evidence/TASK-003-PC-A-final-rules.txt)。摘录只含白名单标记与原生退出诊断；原始日志、存档、游戏程序集和个人路径不提交仓库。

未测范围：图形呈现/文字布局/鼠标、人工自然整局/平衡、双人主客机和跨玩家同步均NOT_RUN，不以此次main合并宣称公开发布或联机已验收。此前报告02、PC-B报告01是各自旧S的历史证据。
