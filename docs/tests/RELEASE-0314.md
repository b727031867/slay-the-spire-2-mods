# 0.3.14 / PC-A / 发布报告

2026-10-22。分支 codex/card-pool-expansion-0314 已完成 ponytail-review 审查，快进合入 main 并推送；原工坊3823282901已成功更新至0.3.14，保留好友可见。实际工坊Load、Character90项及Visuals128项通过，七文件及两张CDN图片哈希一致。

## 审查与范围

审查 origin/main...b3764e6 的完整运行时代码、Reserve 调用方、模型注册、双语说明、测试和发布入口，按单游戏进程及各玩家独立状态评估。没有未解决的阻断问题，详见[审查报告](REVIEW-0314.md)。人工制品拦截灌注回合末负敏捷是用户本轮明确确认的预期交互，保持原实现；本机原生 TemporaryDexterityPower 同样调用负数 PowerCmd.Apply。审查阶段未修改玩法代码。

新增8张卡牌与4件遗物，奖励池27张（11攻击/11技能/5能力），专属遗物模型7种（含初始遗物升级）。更新玩家指南、更新日志、工坊中英文说明、封面和规则图。新卡/遗物继续复用既有插画。原有未跟踪平衡分析草稿保留，不纳入提交或发布包。

## 源码与包身份

核心源码S=b3764e6b2be5fa6bafddd6a3f27db0709d643b01，dirty=false；从独立干净Git工作树构建，Windows x64、v0.107.1/build23811903、BaseLib3.4.7。编译0警告0错误，Godot导入与PCK导出通过。

文档/图片提交H=a0fccb8bf14119275119672a32bfd7e07c732461，玩家包装 documentDirty=false。H不替代S的程序集身份。随后交付记录仅更新文档，不重新构建或上传S快照。

- 核心ZIP：Sts2Mod-0.3.14-v0.107.1-b3764e6b2be5.zip，SHA256 5AE83F064F90F604A214DDD57BA47C8361FF3A0324243ACE0DDB122DF200E8B8。
- 玩家ZIP：Sts2Mod-0.3.14-v0.107.1-b3764e6b2be5-candidate-20261022T1229368354662Z.zip，SHA256 2FFFD58EB91131FEA7D1D013CFDE5AD43B301472FD0364FF10245497CD836C97。

| 核心文件 | SHA256 |
| --- | --- |
| Sts2Mod.dll | 50D6DCFA3A4883A2AF8247A22CBB682157DB8B7CCF30AAE70B48F96195147118 |
| Sts2Mod.pck | 4D91C3EC3565A26ABE84F7ACD5702F4F1616FB6AD7B5BBD3F2869634B6677CC1 |
| Sts2Mod.json | 01AEB19EF4B6AFEA26FF2CCC574BA74E3DA3A1455C89E2C17D9E558FD572A3C6 |
| build-info.txt | B60509EDB4E45B42BC483B31C13EBDDF5F18BC9901AB890ECA77D2595E279A97 |

## 干净包验证

- Full：636项PASS，含新增专项105项。未知错误0，严格分类的原生headless诊断1项，退出诊断0。
- 原生磁盘SavePrepare：13项PASS；27张奖励牌基础/升级版本保存通过。未知错误0，headless诊断0，严格分类的原生退出诊断9项。
- 同包重新部署后SaveResume：258项PASS。未知错误0，headless诊断2项，严格分类的原生退出诊断9项。复用同一隔离目录45bb9e4a949e4626a58b60cfc88eafc4，使用原生菜单退出。
- 离线：纯连携规则52、日志分类26、发布包装15、统一入口11项全部PASS。脚本夹具不是游戏加载证据。

磁盘恢复包含受控牌组、高生命与每场四回合预算，强制清场与自然胜利分别记录，不是自然整局或平衡验证。全部游戏验证无GUI、使用隔离档，未拷贝用户存档或启用测试Steam云。进程由统一入口持有并清理。

| 套件 | PID | 日志 |
| --- | --- | --- |
| Full | 34868 | game-20261022T1225505294291Z.log |
| SavePrepare | 14832 | game-20261022T1240587364642Z.log |
| SaveResume | 33856 | game-20261022T1241275122247Z.log |

## 工坊交付

条目3823282901、依赖3737335127，上传配置visibility=null，Steam API上传前后均确认FriendsOnly。官方ModUploader v0.2.0，EXE SHA256 7D5283DBAFF01EC5182BC08A09F4AAC864CFF870D7D692AF08DAF3830B4EFA19。上传PID6496，退出码0，日志明确Successfully uploaded。沿用既有条目，不更改可见性。

Steam原生订阅/下载完成，state=5、installed=true、bytes=58202807、timestamp=1792673072；没有手工写入Steam缓存。实际下载的全部七文件SHA256与上传快照一致。封面与规则图的Steam CDN返回内容均为PNG且与源文件SHA256一致：

- 封面：544004A1764B51AD6F95258C5F2A326EC4769417273143CFC4E84FB2EC75B91D。
- 规则图：16E8CEB6B6419392318325E4D45A01E7C694DB0C506FE75A461EC4D43CDC1481。

本地Sts2Mod/BaseLib逐文件与工坊核对一致后移动到.local/release0314/local-overrides，恢复实际工坊加载；未改动其他Mod或用户存档。本地原始证据存于.local/release0314和.local/logs，不提交原始账号日志。

## 未验证项

本轮图形渲染、真实鼠标/手柄选牌与选目标、多人联机/重连、自然完整流程与Boss平衡、用户旧档迁移：NOT_RUN。此前版本用户双人实测属于历史证据，不替代新增交互的验证。原生战斗入口恢复不等同于任意选择界面中途精确接续。已知原生headless/退出诊断按严格文本与源码位置分类，不能声称无任何ERROR。

## 实际工坊加载验收

| 套件 | 结果 | 断言 | PID | 日志 |
| --- | --- | --- | --- | --- |
| Load | PASS | 0 | 28596 | game-20261022T1245347412054Z.log |
| Character | PASS | 90 | 29216 | game-20261022T1245494156911Z.log |
| Visuals | PASS | 128 | 35612 | game-20261022T1246486482311Z.log |

三个套件全部PASS，未知错误、已知headless诊断和退出诊断均0。日志确认DLL来自workshop/content/2868840/3823282901和3737335127/BaseLib，无本地副本覆盖。全部测试PID及上传PID已结束；未启动图形游戏。临时干净构建工作树已清理，核心包、玩家包与原始证据保留。最终记录提交不改动已上传七文件和图片。
