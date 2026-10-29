# 雷蚀回响固定增层（2026-10-29）

基线：main 85c021e，已发布核心7b0d63b，PC-A，游戏v0.107.1/build23811903、BaseLib3.4.7。
改动：雷蚀回响由已有可叠负面效果翻倍，改为每种已有可叠负面固定增加3层，升级6层；负力量、负敏捷向负方向增加相同绝对值。费用1/0、基础虚无/升级移除虚无、雷元素、不消耗和原生人工制品拦截均不变。
影响：EchoingBlight.OnPlay及其数值/升级，负面效果筛选与PowerCmd.ModifyAmount调用；乘隙雷击按虚弱/易伤层数计算伤害的联动；中英文卡牌说明、玩家指南、工坊当前说明、Cards夹具和严格日志检查器。历史更新日志保留原始事实。
风险：局部卡牌效果与升级数值，不改共享连携、模型ID、存档格式或其他卡牌实现。
必跑：构建/加载/未知错误检查及Cards专项；补固定3/6、重复使用线性增加、大层数、正向力量/敏捷不受影响、乘隙雷击联动；沿用空目标、人工制品、费用、虚无和元素行为检查。
不跑：Full（未要求）、Events（无事件实现改动）、Character（注册获取未变）、磁盘（序列化未变）、图形/联机/自然整局（未要求）。离线ComboRules的源文件与共享结算未改，不重复该检查。

后续授权：用户在本次开发进行中明确要求“测试完成后，发布到工坊”。Cards专项全部通过后，直接将本次已测试核心更新到原公开条目3823282901，不重建；该直接发布指令不记为用户试玩PASS，也不改变以后任务的一般门槛。

## 验证

干净核心：`c5aa5579e70baa17530e726e30e23619fe6de997`，dirty=false，版本0.3.17。PC-A构建、无GUI导入/导出PASS；保留既有Sandworm夹具两条nullable警告。

| 核心文件 | SHA256 |
| --- | --- |
| Sts2Mod.dll | A02546905230DB24E8395A56C35A78FD0F09D413FAC39EF2A82DC0372E5ADF22 |
| Sts2Mod.json | 0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A |
| Sts2Mod.pck | 0EE077E6DAD4C3F3F0C797DE7DBC0FC253AE8A0BE0D1FD7CEE15F00C402D487A |
| build-info.txt | C16922BD991AEB93970013E55448454E01F1AA5CC13B1E3AC85D1177AAEF3D8B |

Cards：815项PASS，未知错误0，950.568秒；其中雷蚀回响14项针对性检查，包含新增6项。重复使用后虚弱2→5→8（升级2→8→14），易伤3→6→9（升级3→9→15）；随后乘隙雷击伤害60/144，确认按固定增量结算。100层只增加3/6，正向力量/敏捷不变；费用、虚无、人工制品、空目标连携和非叠层/正面效果均验证。
日志：`.local/logs/game-20261029T0554276008062Z.log`；schemaVersion=2报告绑定本包、环境和日志SHA256：4A9998731C8D3D6FFA37EDEE89F1B9F8E4C08D15001BB51CFB84EAA2763366BC。PID18752已由入口清理；隔离档`.local/task003-saves/15dbdb45c99e42b89eac89736ce3efff`。未改用户存档。
本轮已将原工坊安装可恢复移动到`.local/backups/blight-fixed-20261029-workshop`，安装本地候选，避免重复加载。中英键/动态变量及当前工坊说明7591个UTF-8字节核对通过；git diff --check通过。雷蚀回响用例提前运行，所有原用例和严格检查器保留。

结果：验证PASS，按用户明确指令完成公开发布。Full、Events、磁盘、图形、联机、自然整局及用户试玩NOT_RUN；不把直接发布指令记为试玩成功。

## 发布回执

核心实现及测试记录已合入并推送main，发布复用上述被测核心，未重新构建。统一入口Invoke-ReleasePreparation复用同包Cards报告，结果PREPARED；快照`artifacts/Sts2Mod-0.3.17-v0.107.1-c5aa5579e70b-candidate-20261029T0611011721829Z`，快照SHA256 `AF8C9EEE245190EE283DC13C4344FD41826EA20A6B5CCEAA70F2DAAEC8D91FFF`，ZIP SHA256 `E01A96BCDD907E5045C259E4715072CCD13ED38B045F4F4FB1C741114A9788AD`。

实际上传目录`.local/workshop/c5aa5579e70baa17530e726e30e23619fe6de997-AF8C9EEE2451`的workshop.json已核对public及原条目3823282901，上传文件逐一匹配冻结快照。官方ModUploader v0.2.0分发包SHA256 `2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`；隐藏进程PID14432成功退出，上传成功。独立WorkshopProbe inspect返回OK、`k_ERemoteStoragePublishedFileVisibilityPublic`、consumerApp2868840；BaseLib依赖3737335127不变。

Steam原生下载原条目完成，state=5、118481745字节、timestamp=1793254317；下载目录七文件（四核心及README.zh-CN.md、CHANGELOG.md、ASSETS.md）全部SHA256匹配快照，无额外文件。封面与规则图均HTTP200且匹配上传源图SHA256，无需修复。BaseLib原生下载state=5，版本3.4.7、三文件均匹配固定依赖。证据保存在`.local/blight-fixed-20261029-release`，原始Steam日志不入库。发布后未另启游戏，加载证据复用同包Cards专项，不宣称独立工坊启动验证。

本机已恢复为唯一工坊安装；经哈希核对、确认无游戏进程后，将重复本地Sts2Mod/BaseLib可恢复移动到`.local/backups/blight-fixed-20261029-local-install`。原工坊备份仍保留；未修改用户存档或配置，未启动图形游戏。

Retention完成，保留本次及此前20261029T0223523477445Z、20261029T0221330809012Z共三份交付；自动移除较旧20261029T0214237270476Z快照及其ZIP、校验、准备报告和ACA3065A8D52上传副本。源码、日志与恢复备份均保留，历史测试结果不变。
