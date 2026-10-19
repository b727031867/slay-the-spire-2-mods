# TASK-002 增量A交接

总体TASK-002仍IN_PROGRESS；本轮仅雷光突刺与岩雷连招，按用户“剩余额度约14%，完成部分并推送”的要求控制范围。分支`codex/TASK-002-lightning`基于已交付的`codex/TASK-001`；不自动合并main。

代码S：`06e16c144727378e335ae0b724d0c42e374f6ac5`，Mod0.0.3。目标游戏v0.107.1 / Steam build23811903，BaseLib3.3.2，工具基线不变。

包：`Sts2Mod-0.0.3-v0.107.1-06e16c144727.zip`，108,468字节；SHA256：`59CDC0A4CFD074EC38C6B23EE8C4F98A129DF223C224691AC02A7053AAC9D2FC`。精确测试范围见[报告](../tests/TASK-002-PC-A-run-01.md)。[草稿PR #3](https://github.com/b727031867/slay-the-spire-2-mods/pull/3)；[草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-872f3642b3aa121dcc6d)（ID417136064，ZIP附件655880524，SHA附件655880564）。须以有草稿访问权限的账号下载。上传响应的SHA256 digest与本地一致；本轮未做回下载复验。包仅含本Mod的4文件，依赖单独安装。

PC-A验证：31项纯规则、44项真实游戏进程无界面断言通过，编译0警告/0错误、PCK导出成功；不是新增卡牌的手动图形界面验收。当前本机安装0.0.3，自动测试已结束，默认启动关闭测试入口。

按[开发说明](../DEVELOPMENT.md)部署；规则测试用`Start-ModGame.ps1 -TravelerTest`，日志检查加`Test-TravelerRun.ps1 ... -Lightning`。显式测试模式的`tc_hand 1 lightning`或`tc_hand 1 lightning upgraded`分别准备普通/升级岩雷手牌。默认模式无赠牌。

本轮不包含TC_C04/05/06、雷→风抽牌、派蒙、完整退出读档与双人专项。新增牌正常奖励获取、手动中文/英文界面与自然整局试玩未测，不能沿用TASK-001风岩的界面与获取PASS。雷→风目前只更新风元素，不发抽牌奖励。

后续优先单人：补齐剩余牌与雷风/派蒙，并补实际获取、界面、存档和试玩；双人另行专项。后续文档提交不改变被测代码S，重建较新提交应另记新身份与包。
