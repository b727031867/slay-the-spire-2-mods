# 0.3.6 / PC-A / 发布报告

状态：源码已提交、main已快进合并并推送、原私有工坊3823282901已更新。2026-10-21用户明确授权上述动作；战斗演出只讨论，不属于此次实现。

## 源码与包身份

源码S：0685d66bddf7ec29c604fd61e99c81553fa73aad，dirty=false；开发分支codex/refactor-release-tests-combo与main均已推送。文档整理、工程重构及版本0.3.6一并提交。S之后的发布证据文档提交不改变本包身份。

核心ZIP SHA256：AFC615AA20375FFFF852B60CF66B56C6B25BDFAB4DEA5214D4B97B590D882A86。

附玩家文档及release-info.txt的候选ZIP SHA256：B458067FE023DBC34AC771E4B260037A2225064A02FC2D9A62AF50034B5317A8，包名Sts2Mod-0.3.6-v0.107.1-0685d66bddf7-candidate-20261021T0314194474553Z.zip。核心四文件保持S，不隐式重建。

用户报告“试玩没问题”与0.3.6干净构建的自动验收分别记录：本轮开始时本机工坊安装仍为原0.3.5，不能据此宣称用户人工试玩过指定0.3.6二进制。重构dirty开发包的410全量、13准备/250恢复及一次句柄异常/复跑见[重构报告](REFACTOR-PIPELINE.md)；不将其计为0.3.6干净包全量重跑。

## 构建与专项

固定游戏v0.107.1/build23811903、BaseLib精确3.4.7、SDK10.0.402、Godot.NET4.5.1。干净ExportRelease/win-x64编译0警告、0错误，Godot导入导出成功。49纯规则、26日志分类、15包装及11入口离线检查通过。

S与此前已测重构C#源码一致，仅清单/程序集版本及发布文档、封面版本等元数据变化；未改玩法或动画。S本轮运行普通启动与49视觉专项，不冒称重跑全量、磁盘或图形人工验收。

| 场景 | 结果 | PID（均结束） | 本地日志 |
| --- | --- | --- | --- |
| 干净包普通无GUI启动 | PASS，未知错误0 | 29784 | game-20261021T0313576644777Z.log |
| 干净包49项原生专项 | PASS，未知错误0 | 8948 | game-20261021T0314132735566Z.log |
| 实际工坊普通无GUI启动 | PASS，未知错误0 | 33684 | game-20261021T0317313368023Z.log |
| 实际工坊49项原生专项 | PASS，未知错误0 | 35080 | game-20261021T0317475432886Z.log |

49项包括15选人、11能量、23状态图标/原生悬停及只读性。Rendering/Multiplayer保持NOT_RUN。每份日志旁保留结构化结果；无用户键鼠或图形游戏窗口。

## 工坊上传与实际下载

Mega Crit官方ModUploader v0.2.0，Windows ZIP SHA256 2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。上传PID9264、退出码0，返回Successfully uploaded同ID3823282901。保持private、依赖3737335127，无新增条目、扩大可见性或代接受条款。

通过正常Steam订阅/下载API刷新原有订阅；上传前后state均5。最终API确认k_EResultOK、consumerApp2868840、private、bytes55645479、timestamp1792552557。没有手工写Steam缓存。

实际下载目录恰有7文件，逐一与上传目录哈希一致，清单0.3.6、build-info的S和dirty=false匹配：

| 文件 | SHA256 |
| --- | --- |
| ASSETS.md | 99F4C3C34918A84D1E74585E9160D4CFC62626444C1A7477E6AC01ED5579491D |
| build-info.txt | 9654B9923CF17859BEF9C62BB0914F853B3116E612A669BB79BA9333909D67FC |
| CHANGELOG.md | 44A157CB027E9C0D5DE996465CEC5822DCEDF24DF7A9DA55C875382AF3166D01 |
| README.zh-CN.md | 465E7717E64FF8101046C4C5CB11BD41D18618D0B802D75FF3A7DD524FB8BC56 |
| Sts2Mod.dll | E0D5401AF7AB3AE21D96CC433E8298E218CD0C02B51BF51E705A027620D1294E |
| Sts2Mod.json | 14243BC0B340E8839EC9883A34E11985A1DD4753369D491E0BCF7A9895B99268 |
| Sts2Mod.pck | 4C1E6A523CBBEF4F44B73A5CE45B742AE0F8233F8F32A7C3699855FBD4685F6E |

附带文档哈希固定标识上传时快照，后续仓库文档更新不自动更新Steam文件，不再次重包或上传。

封面HTTP200、image/png，SHA256 EDABA1536A31F2A0CEA7AD4B16A645F355180E2B4209A711F07F104F9E254148；规则图HTTP200、image/png，SHA256 AC5B94D4C49A38B8ACFF3C2EA60DF4EF3AEA6DEE5F484FE21FCB0611E2E17DFD。均从最终API返回CDN URL读取，与源图相同。新URL与原始API结果留于.local/release036-after.out及release036-cdn.json，不提交账户日志。

## 本机生效与边界

最终日志明确从工坊3823282901加载Mod、从工坊3737335127/BaseLib加载BaseLib。BaseLib既有工坊副本为v3.4.7，三文件与本地锁定官方包哈希一致；本轮没有重新下载或升级BaseLib。没有本地同ID副本重复加载。

干净包预发布测试先可恢复隔离旧工坊副本，结束后恢复，随后才由Steam正常更新至0.3.6。所有AI游戏、上传及探针进程已结束，未启动图形游戏。受控战斗和此前开发包结果不作为自然Boss平衡、用户旧档迁移或多人证明。

分层2D动作与适中强度特效的[讨论稿](../design/TRAVELER-COMBAT-PRESENTATION.md)尚未实现，未改变本次战斗演出。当前接续从main开始。
