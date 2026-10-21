# 0.3.5 / PC-B / 发布与校验报告

源码已合入并推送main，用户试玩通过，工坊3823282901已更新0.3.5并保持private。接续入口为[main接续说明](../handoffs/NEXT-PC-CONTINUE-PROMPT.md)。本报告游戏、Steam及图片检查均属于PC-B发布验收，本次文档整理未重跑这些检查。

## 已试玩核心包身份

可执行源码S `628cd327cc1eeb24c7c9cbc8336491151c3a3561`；包`Sts2Mod-0.3.5-v0.107.1-628cd327cc1e.zip`，SHA256 `D2983666695385EDD08805286084CC3930332241E394E83E79F36CC491CE26AA`。已发布核心包身份保持不变，本次文档整理未重新打包或上传。

原包49项无GUI专项（11能量/23元素UI与预览/15选人）、49纯规则、26日志分类通过，原报告[TASK-011-PC-B-run-03.md](TASK-011-PC-B-run-03.md)。用户随后人工试玩通过，不等于多人、所有自然Boss平衡或旧档迁移通过。

## 发布准备

接续分支为main。PC-A已同步交接提交63e48febcf91，本机安装及工坊包身份尚未验收。

官方ModUploader v0.2.0 Windows x64 ZIP SHA256 `2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`，Probe使用官方随附Steamworks.NET15.0.1，包SHA256 `E32DA6B4199384A1AAA15169514FD957B9FF8BA5BDC487BFED6F91E856DC01A6`。上传前API确认同条目、private、consumerApp2868840、订阅状态5；旧工坊目录曾可恢复移动，仅元数据installed=true不视为实际安装验收。

本轮使用同一S的四个核心文件，另附发布时的玩家指南、更新记录和素材说明，共7文件。封面和规则说明图更新0.3.5，均小于1MB，已本地检查无裁切。

## 已完成的上传与下载

工坊工作目录`.local/workshop/628cd327cc1eeb24c7c9cbc8336491151c3a3561`，mod_id明确3823282901/private，官方上传PID21848返回Successfully uploaded，依赖没有变化；未代接受法律条款。最终Probe再次确认k_EResultOK、同ID/private、consumerApp2868840、state5，bytes55645593、timestamp1792525484，未新建或公开条目。原始Steam日志仅留本机忽略目录，不提交账号信息。

通过正常Steam订阅/下载API获取（此前已订阅，不取消/重建订阅），不是手工写Steam缓存。实际目录steamapps/workshop/content/2868840/3823282901中恰有7文件；清单0.3.5、build-info的S/dirty=false正确，逐项与上传内容相同。下表固定标识已上传7文件快照；仓库附带文档后续修订不改变这些哈希，也不代表Steam文档已更新：

| 文件 | SHA256 |
|---|---|
| Sts2Mod.dll | 5085834CAEB019122DB91AC20C4127A28E043D8EEE7B461A5D636BFC3622E0BD |
| Sts2Mod.json | 049E267A544F8E03DD475B4D0AC4FAC0FB0EA532C23043884927D8DABBC38AA7 |
| Sts2Mod.pck | 210A8BF52254465BF46C3A70203AD10102DFC955D98C96B9D2F4CCEC77B776C8 |
| build-info.txt | 05E970542DA59E78DD0C9A7B42AD8E0B7B1A834D5AA2FDFF2367DCEC5A138167 |
| README.zh-CN.md | 1A409FC523488A2C6A11EDF75A4B6E70F8F90A54DB6DCB96CA124A88B3C4D97F |
| CHANGELOG.md | E5B85AC0E5E186BBAC0A4E1C96C4CDE48DAE27BD19C9549DE59F56F8CEA38656 |
| ASSETS.md | 6E16C9F0171719D8116896C3F2E6C0809A96062F11F96477D217497A42C300F9 |

封面[CDN](https://images.steamusercontent.com/ugc/12826042512110828709/8A48316A99A75C5D88D6D4F707880CAED5CAE8EF/)，SHA256 `E97CE50656A3F567B2EC9E8F33DDA57D22ECBB50E6C1A302510211C90CB8A442`；相册规则图[CDN](https://images.steamusercontent.com/ugc/11002983442395220938/895CE187F956DB78C30F79DB3152E3C20DFCCAC1/)，SHA256 `E7DA27B5083FF564948B28594151EA7272694BC59C41D369198D79A9D1992D41`。均实际HTTP200、image/png，下载哈希与源图相同，最终API URL不变。验证输出留在.local/workshop-verification/035。

## 本机生效与工坊加载验证

确认没有用户游戏进程后，将mods/Sts2Mod可恢复移动到`.local/backups/workshop035-20261020T194534Z/Sts2Mod`，没有删除，防止与工坊重复加载。PC-B发布验收时仅工坊Mod生效；BaseLib仍保留本地锁定3.4.7，未请求再次下载或升级依赖。其DLL/JSON/PCK哈希为`55863CA6ADC30A61A7544874E157A519FFE148CCF2B18025947609C4BFF813CE` / `8B0D470C1C624EED9F7BEBF79FA7985ED7F8F4965154713546890A910A61D6F8` / `95E653B9AEA9A3DCBC45971B8933C854E314B897488643DB71DF1E14A76B808D`，与锁定官方包一致。本轮不冒称两项依赖都从工坊重新下载验证。

普通无测试参数的无GUI启动通过Test-ModLoad -Headless：PID32684、日志game-20261020T1946045236503Z.log。日志明确从3823282901工坊目录加载Mod DLL/PCK、从本地加载BaseLib；仅一次初始化0.3.5+S、BaseLib3.4.7/resource=ok，到达主菜单，未知错误0、已知原生无GUI诊断0。AI进程已结束，没有启动图形游戏。

再从工坊加载相同S运行TravelerVisuals：PID27988、日志game-20261020T1946273680571Z.log，唯一原生隔离沙盒b6ea5666061a4f76961dfe8c3f314bd3。Test-ComboVisualRun严格核对49项通过（11能量、23元素状态/预览、15选人），未知错误0、已知原生无GUI诊断0。未使用用户存档或Steam云。AI进程已结束。

```text
STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN
STS2MOD_VISUAL_COMPLETE assertions=34 panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN
STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=49 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False
```

## 验证范围与接手

原包与工坊各49项无GUI专项、49纯规则、26日志分类和普通工坊启动通过；main集成时另运行49纯规则与26日志分类通过。用户人工试玩通过单独记录，自动化不证明图形交互。

0.3.5未重跑完整卡池/磁盘回归；多人、自然Boss系统性平衡和用户旧档迁移未验证。PC-A同步源码不代表本机已安装或测试，按接续说明核对本机版本、下载身份和重复加载情况。版本过程见[更新日志](../player/CHANGELOG.md)。
