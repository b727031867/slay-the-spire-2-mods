# 0.3.13 / PC-A / 发布报告

2026-10-22。状态：源码已快进合入main并推送，原工坊3823282901更新成功；干净Full531项、实际工坊Load/Character90项/Visuals128项全部通过，Steam实际下载七文件及两张图片一致。

用户已试玩0.3.12候选，要求基础/升级元素共鸣也改为短闪，并授权提交、合并main及更新原工坊。此版本整合0.3.10–0.3.12的罗盘升级、联机选人修复、19张卡池平衡和轻量反馈。用户试玩证据不等同于最终干净二进制的图形验收。

## 源码与包身份

核心源码S=`b8f6bd270240e711b210d2dab46d8dc21bec0a7e`，dirty=false。从独立干净工作树构建，固定Windows x64、v0.107.1/build23811903、BaseLib3.4.7、SDK10.0.402、Godot.NET4.5.1。编译0警告0错误，导入及PCK导出通过。

后续`99903fa`仅更新工坊封面、规则图、生成脚本与设计文档；运行时代码与S一致。此前未跟踪的平衡分析草稿保留，不纳入提交或玩家包。

- 核心ZIP：`Sts2Mod-0.3.13-v0.107.1-b8f6bd270240.zip`，SHA256 `9B75861B7F8D0C247AFC599F378BFB0CF6A56FE530F0E3F8C4552266D6EB5B24`。
- 玩家ZIP：`Sts2Mod-0.3.13-v0.107.1-b8f6bd270240-candidate-20261021T1813498063557Z.zip`，SHA256 `7E878F9B225A8209603DA56460216A76BBE1BEFA979284374B78D73E9D5350B9`。
- 玩家包装保留S四文件，文档来源`99903fa3220a901cda34077b92610c4a024b07fe`、documentDirty=false。

| 核心文件 | SHA256 |
| --- | --- |
| Sts2Mod.dll | B93E06A7616BC55B98F3F0EE2467A1F257312F51E05E40F034C091FA7D7DD718 |
| Sts2Mod.pck | 71D1559ABEFC5E6D076B13B186BB384759043C97165621E3FCAF15092A3C46B0 |
| Sts2Mod.json | 5AA4595AB679D40D39FB10E5036F9B9AD70287EC70C4FBAEAB10C47AB5CE2D16 |
| build-info.txt | B05A0B34FFD6FC397762323DE104E465D551E62B59028D2332C45D7669476D8C |

## 验证

干净包真实Full无GUI回归531项通过；52项纯规则、26项日志分类、15项发布包装、11项统一入口离线检查通过。Full未知错误0，严格识别的既有原生无GUI诊断1项，退出诊断0。不借用0.3.11或0.3.12的检查次数代表本包。

新增共鸣检查覆盖基础与升级版：24次Flash事件保留，图标只保留一个短闪，不发射图片粒子，不创建大图，短闪清理且能量/连携/RNG不变；原有能力触发与实际回能数值由全量卡牌用例验证。

中间失败：开发时测试代码直接调用protected Flash导致一次编译失败，改为测试专用反射调用后编译通过。一次工坊只读探针缺少.NET9环境初始化而未启动，加载既有开发环境后恢复。托管工作树工具因聊天根目录不是Git库而拒绝，改用仓库内独立Git工作树；未修改用户草稿。

全程无GUI，使用隔离测试档；不启动图形游戏。最终图形观感、联机复测、自然完整流程/Boss平衡与旧档迁移均未作本轮系统验收。

## 工坊与交付

条目沿用3823282901，依赖3737335127。上传前后Steam API均确认FriendsOnly，上传配置visibility=null保留实际可见性；不新建条目、不扩大可见性、不代接受条款。上传PID26396已结束，官方上传器明确返回Successfully uploaded；没有保留该已退出进程的ExitCode句柄，故不声称另行捕获退出码0。

官方ModUploader v0.2.0，EXE SHA256 `7D5283DBAFF01EC5182BC08A09F4AAC864CFF870D7D692AF08DAF3830B4EFA19`。封面/规则图更新至19牌、罗盘4次及新版共鸣规则，并完成离线图片检查，非游戏截图。

正常Steam API刷新既有订阅并下载，state=5、installed=true、bytes=58127644、timestamp=1792607054；未手工写入Steam缓存。下载版本0.3.13、codeCommit=S、dirty=false。七文件均与上传快照一致，逐项哈希见本地`.local/release0313/downloaded-hashes.json`，玩家文档三文件哈希见候选包release-info.txt。

封面与规则图均成功从Steam返回的CDN URL下载，并与本地源图逐字节哈希一致：封面`17A505D4CFFC6EF9F7F2B9D10A6990665CFFFC185E9EAC69A16FA9FD94500E88`，规则图`BA933AB57D48335F95A270F736F85DBA412139A05DAA0A243116EDAFCBBF3874`。

本轮本地原始日志保留于`.local/release0313`和`.local/logs`，不上传账号日志。本地Sts2Mod/BaseLib逐文件与工坊核对后可恢复移至`.local/release0313/local-overrides`，让实际工坊加载不被本地副本覆盖。临时干净构建工作树已清理，包与日志保留。

## 实际加载与进程

| 场景 | 结果 | PID | 日志 |
| --- | --- | --- | --- |
| clean-full | PASS，531项 | 27368 | game-20261021T1812239548745Z.log |
| workshop-load | PASS | 30328 | game-20261021T1825133330147Z.log |
| workshop-character | PASS，90项 | 12936 | game-20261021T1825280309685Z.log |
| workshop-visuals | PASS，128项 | 27376 | game-20261021T1826272967451Z.log |

上述四个测试PID及上传PID26396均已确认结束。工坊三个场景的未知错误、原生无GUI诊断、退出诊断均0；日志确认实际DLL来自workshop/content/2868840/3823282901及3737335127/BaseLib，而非本地mods。角色/表现专项使用统一入口隔离档与--headless，普通加载同样无GUI，不自动重启图形游戏。最终记录提交只更新文档，不重新构建或上传S七文件快照。
