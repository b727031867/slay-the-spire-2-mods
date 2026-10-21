# 0.3.8 / PC-A / 发布报告

状态：0.3.8源码已快进合入main并推送，原工坊3823282901更新成功；干净包完整回归466项、实际Steam下载七文件及两张图片核对通过。实际工坊普通加载、角色77项和视觉87项均通过。用户明确授权完整回归、合并main及更新工坊。

本版修复选牌说明遮挡、分层角色比例及商店旅行者形象。实现和开发失败记录见[修复报告](PRESENTATION-FIXES-038.md)。

固定游戏v0.107.1/build23811903、BaseLib3.4.7、SDK10.0.402和Godot.NET4.5.1。AI游戏测试全部使用隐藏进程和--headless，不自动启动图形游戏。Rendering、Multiplayer、自然Boss平衡及旧档迁移另列，不以无GUI检查或离线动作接触表宣称通过。

## 源码与包身份

源码S=e5934b8695fa38c81ea6df13b3948c43034b3c9a，dirty=false。开发分支codex/fix-traveler-presentation与main均已推送S；后续发布证据文档提交不改变该核心包。

构建使用S的独立干净Git工作树，保留主工作区另一份未跟踪平衡分析文档；未纳入本次提交或玩家包。ExportRelease/win-x64编译0警告、0错误，Godot资源导入和导出通过。

- 核心ZIP：Sts2Mod-0.3.8-v0.107.1-e5934b8695fa.zip，SHA256 B46A82EB01E9D6DF7C435A82A10D223F85997DE6C368B2DEC9DC8ADD0E1A8148。
- 含玩家文档的候选ZIP：Sts2Mod-0.3.8-v0.107.1-e5934b8695fa-candidate-20261021T1159345585950Z.zip，SHA256 5DB00E3A3AAB9875166282969BDFCF49B823051DFDCFC321C7F3300FDE689027。
- 包装保留核心四文件，未重新构建；文档来源S、documentDirty=false。上传七文件为S文档快照，后续发布报告不触发重新包装或上传。

## 验证

干净核心完整回归：PASS，466项，PID21424已结束，日志game-20261021T1158100862047Z.log及同名.result.json。覆盖基础连携、自动/重复结算、额外回合、目标死亡、扩展18牌/4能力、原生商店、篝火、状态/悬停、能量图标与动作事件。未知错误0，严格识别的原生无GUI诊断1，未放宽检查器。49纯规则、26日志分类、15发布包装、11统一入口离线检查通过。

Visuals从原生节点导出五种动作变换与Atlas区域，用Export-RigContactSheet.ps1离线重建于.local/release038/rig-clean.png。它不是游戏截图，不表示图形观感或鼠标选择已验收。早期失败和提前退出均在修复报告保留，不计入最终包通过结果。

## 工坊上传与下载

官方ModUploader v0.2.0，ZIP SHA256 2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578；可执行文件SHA256 7D5283DBAFF01EC5182BC08A09F4AAC864CFF870D7D692AF08DAF3830B4EFA19。上传PID1388，退出码0，返回Successfully uploaded。依赖仍为3737335127，无新条目、无条款代接受。

上传前Steam实际可见性为FriendsOnly，与旧报告所写private不同；生成上传配置将visibility设为null，依据官方工具模板及代码保留现状。上传后再次确认FriendsOnly，不扩大可见性。仓库模板仍以private作为新发布的保守默认值。

经正常Steam API刷新既有订阅并下载，最终state=5、installed=true、bytes=58057046、timestamp=1792584603，版本0.3.8、codeCommit=S、dirty=false。以下七文件逐一匹配上传内容，没有手工写Steam缓存：

| 文件 | SHA256 |
| --- | --- |
| ASSETS.md | 8443FD0299AF29BC2554A7798F0521A1F8FD084A2E53DB62049BC472B857073C |
| build-info.txt | FC1F7562E4DCF9C6DA0752D5E34593EB0C9F6CA5F0F201A9F307B3D7AFE2878B |
| CHANGELOG.md | 32914B465DA3A0F3C4CD57CD5E21BEE69FD5731F35783B6400C01112D0011B85 |
| README.zh-CN.md | 403CA5D7F92398888B3F9D97D0178017012BC7DAA4296A3597C45F0503F86C2D |
| Sts2Mod.dll | 6B19D70B992249417766DFBD66327CEDBBB47165A1EC0B8C3829969D5DE5965F |
| Sts2Mod.json | D7BB42B6727118ADF502A44B0FE7D8C869404A28CAA4796743413A7CE8199EA1 |
| Sts2Mod.pck | 30D2196F0D9EDBD50F1C69A58EBDE28D514D8F7A53FDBAD08D2A9C751070190D |

封面和规则图均HTTP200、image/png且与源文件SHA256一致：封面AD341A4E78FA90B35C62CDE38DD1F172474D4C792BF52B770BC7C4D8A08CC5E4，规则图C8E85383D98D9F4A091DC751A72FC4A997049EB39A3FB9663C48C90746FC8997。原始URL与响应证据在.local/release038/cdn-results.json，未提交账户日志。

BaseLib实际Steam下载为v3.4.7，三文件SHA256与固定缓存一致。本地Sts2Mod和BaseLib副本已校验并移入.local/release038/local-overrides；工坊加载日志确认两者均从workshop/content路径加载。

## 实际工坊加载验收

| 场景 | 结果 | PID（均结束） | 本地日志 |
| --- | --- | --- | --- |
| clean-full | PASS，466项 | 21424 | game-20261021T1158100862047Z.log |
| workshop-load | PASS，0项 | 30156 | game-20261021T1211585823216Z.log |
| workshop-character | PASS，77项 | 5444 | game-20261021T1212138216765Z.log |
| workshop-visuals | PASS，87项 | 29548 | game-20261021T1213248147015Z.log |

各次未知错误均为0。严格识别的已知原生无GUI诊断按上表顺序为1/0/3/0，原生退出诊断均0。原始日志及结构化.result.json保留在.local/logs，摘要在.local/release038，未上传账户日志。

工坊三个场景均确认Sts2Mod来自workshop/content/2868840/3823282901，BaseLib来自workshop/content/2868840/3737335127/BaseLib。最终本机以实际工坊0.3.8生效，本地重复副本可从可恢复备份取回。所有本轮游戏、上传器及探针进程已结束，未启动图形游戏。

本轮不涉及磁盘格式，未将SavePrepare/SaveResume或用户旧档迁移计为通过；自然完整流程、Boss平衡、多人及人工图形观感仍需独立验收。后续文档提交只补充发布证据，不改变S核心四文件或Steam七文件快照。
