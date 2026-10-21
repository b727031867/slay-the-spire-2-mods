# 0.3.7 / PC-A / 发布报告

状态：源码已提交、快进合入main并推送，原私有工坊3823282901已更新0.3.7；实际Steam下载、图片和工坊加载验收完成。用户明确授权提交、合并main和更新工坊。

## 源码与包身份

源码S：06c103601f4431718cf4d33aa84204b8ff6209dc，dirty=false。开发分支codex/traveler-combat-presentation和main均已推送S；本报告等后续文档提交不改变S核心包。

核心ZIP：artifacts/Sts2Mod-0.3.7-v0.107.1-06c103601f44.zip，SHA256 71C03A7DD93514839AD71FCCD93BA8A224F44F507E072CE1E81C34445806A4A5。

含玩家文档及release-info.txt的候选ZIP：Sts2Mod-0.3.7-v0.107.1-06c103601f44-candidate-20261021T0658094901285Z.zip，SHA256 E55FCEF669ED41BC4F015DEA0D9CBECFFA04031ADC023B510C980DC54D484FD4。核心四文件保持S，包装没有隐式构建；文档来源为S、documentDirty=false。

用户试玩包为0.3.6+1f24c71-dirty，ZIP SHA256 B61CB5780BC82402D62394DA7726D7E2CA8594473E5AF6ECA7099D483CCC4FBB，本轮开始逐项确认与本机安装一致。与S比较，293个功能源码/资源/测试文件未变，仅程序集/清单版本和封面脚本版本属于允许的元数据变化，另有发布资料调整；PCK与该试玩包逐字节相同，SHA256 1669A06EFAAA89F6D075166EA55EE7E3904D902B24FEC3C5AF16E057379433B5。不能据此宣称用户试玩过指定干净DLL。

## 实现与验证范围

11部件分层2D角色动作覆盖普通攻击、防御、全挡、部分格挡、直接受伤、生命损失、倒地与复活。风/岩/雷分别有攻击和防御特效，连携融合反馈对齐实际奖励；篝火使用旅行者专属坐姿休息图。卡池、伤害、格挡、抽牌、额度、随机目标规则和存档ID保持。

固定游戏v0.107.1/build23811903、BaseLib3.4.7、SDK10.0.402、Godot.NET4.5.1。干净ExportRelease/win-x64编译0警告、0错误，资源导入/导出通过。49纯规则、26日志分类、15包装、11统一入口离线检查通过；离线日志在.local/release037。

| 场景 | 结果 | PID（均结束） | 本地日志 |
| --- | --- | --- | --- |
| clean-character | PASS，71项 | 10928 | game-20261021T0656318747995Z.log |
| clean-visuals | PASS，82项 | 34064 | game-20261021T0657414405047Z.log |
| workshop-load | PASS，0项 | 12576 | game-20261021T0701257190779Z.log |
| workshop-character | PASS，71项 | 29848 | game-20261021T0701413780414Z.log |
| workshop-visuals | PASS，82项 | 23576 | game-20261021T0702490137323Z.log |

本表各次未知错误0、原生无GUI错误0；AI游戏检查全部--headless及隐藏窗口，Rendering/Multiplayer为NOT_RUN。用户试玩反馈单列，不表示自然完整流程/Boss平衡、旧档迁移或多人已作系统验收。此前战斗演出443项属于[冻结开发包](COMBAT-PRESENTATION.md)，[篝火开发包71项](REST-SITE.md)也单独记录，不计入S全量重跑。本次范围由版本元数据变化和既有用户试玩/开发检查决定，未重复运行无关磁盘场景。

旧开发包一次Godot脚本注册异常仍按FAIL保留，后续同包重跑通过不代表已修复根因；本轮5次检查均无该错误，没有放宽严格检查器。

## 工坊上传、下载及图片

Mega Crit官方ModUploader v0.2.0，Windows ZIP SHA256 2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578；可执行文件与该ZIP内文件校验一致。上传PID10800，退出码0，返回Successfully uploaded。保持原条目3823282901、private与依赖3737335127；没有新增条目、扩大可见性或代接受条款。

通过正常Steam订阅/下载API刷新已有订阅。最终API为k_EResultOK、consumerApp2868840、private、state5、bytes58045126、timestamp1792565990。实际目录恰有7文件，逐一与上传工作目录匹配，版本0.3.7、codeCommit=S、dirty=false。

| 文件 | SHA256 |
| --- | --- |
| ASSETS.md | EE1F95B418A46E8BF7A0E2F36EFEEC3E40FD9259B8CBE07875DB0752CF8020C3 |
| build-info.txt | 51DC8D5E4611180BE491BC64512D46D3E8ABAC72EDB807A89FAEA5805C0875FB |
| CHANGELOG.md | F7C0524B4F976326B32F366E5C2D04E5062139B747F825565111A83471AFC7F0 |
| README.zh-CN.md | 340D902EAB42B2FFA51598BC2642F4FA9E611F638656B8F41DDE98A09CED0FB1 |
| Sts2Mod.dll | 5AA052390FAC0D177A6834FBD37221C757F83910692BE3853AA9C77A86D5901D |
| Sts2Mod.json | 7CDA5E9003C5DE66D2F5B0A9CFE37C1D0C9FC1E50BBED66043CA9E70D3607D0E |
| Sts2Mod.pck | 1669A06EFAAA89F6D075166EA55EE7E3904D902B24FEC3C5AF16E057379433B5 |

附带文档是S上传时的固定快照。后续仓库发布记录更新不自动改变Steam附带文档，不为记录证据重复打包或上传。

封面HTTP200、image/png，SHA256 902F9AB617AFD15F6D18956A41F448A993BC85C8623BCDAE9276346E2E5C9A84；规则图HTTP200、image/png，SHA256 A553479612D475E4384BA7D37A9B507207D62BEBDE03EF5C5BBB2E7A1B3B319B，均与源图匹配。新CDN地址和原始校验留于.local/release037/cdn-results.json，不提交账户日志。

## 本机生效

实际工坊Load、Character及Visuals日志均确认Mod从workshop/content/2868840/3823282901加载，BaseLib从workshop/content/2868840/3737335127/BaseLib加载。既有BaseLib为v3.4.7，三文件与固定缓存哈希一致，无需升级依赖。

本地Sts2Mod和BaseLib重复副本已校验并移动到可恢复备份.local/release037/local-overrides，没有手工写Steam缓存。所有本轮游戏、上传器与探针进程已结束，未自动启动图形游戏；当前以实际工坊0.3.7生效。
