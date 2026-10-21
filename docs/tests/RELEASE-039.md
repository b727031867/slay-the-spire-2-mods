# 0.3.9 / PC-A / 发布报告

状态：0.3.9已提交、快进合入main并推送，原工坊3823282901更新成功；干净包Full474项、实际Steam下载七文件及两张图片、实际工坊Load/Character77项/Visuals95项全部通过。2026-10-21完成源码与上传，跨2026-10-22完成最终工坊验收和记录。

用户先评审动作草图和本地回放，再明确授权提交、合并main和更新原创意工坊。此版本统一原始母稿与关节连接，实现前突刺、双手防御、后仰受击及连续攻防，不改变玩法结算。开发候选和中间失败范围保留于[自然动作验证](NATURAL-MOTION-039.md)，不计作干净包的发布证据。

## 源码与构建

源码S=565c671ef9598e54a00c55e6dda7308c6a60190a，dirty=false。分支codex/traveler-natural-motion已推送并快进合入main。随后bcd0457仅同步工坊英文说明及测试文档；最终发布记录提交不改变S核心包。

固定Windows x64、游戏v0.107.1/build23811903、BaseLib3.4.7、SDK10.0.402与Godot.NET4.5.1。从S独立干净Git工作树构建，编译0警告0错误，资源导入与导出通过。主工作区未跟踪的平衡分析文档未纳入提交或玩家包。构建工作树已清理，包、日志、测试夹具及部署备份保留于主工作区忽略目录。

- 核心ZIP：Sts2Mod-0.3.9-v0.107.1-565c671ef959.zip，SHA256 `382EED2556563DA169798CA1F3689273B8C055F91B3E7C6521800B11208B8AD6`。
- 玩家候选ZIP：Sts2Mod-0.3.9-v0.107.1-565c671ef959-candidate-20261021T1545127734269Z.zip，SHA256 `3421B801A67C9E750BB555B955C54C420170393C4DD9979542F8C8F002667F45`。
- 包装保留S核心四文件，不重新编译；玩家文档来源S，documentDirty=false。工坊配置的中英文说明采用bcd0457文档更新，上传时visibility=null以保留实际好友可见设置。
- 原母稿body.png未修改，SHA256 `83D3B97B5847F29E69466E94B4C8B5C335C7CD1C9CA3449B48CC15739F1F6436`。

## 验证与范围

干净包原生Full474项通过，含卡牌、连携、回合、18牌/4能力、原生角色/商店/篝火、悬停/能量与动作事件。49纯规则、26日志分类、15发布包装、11统一入口离线检查通过。严格检查器未放宽。

干净包从实际Godot节点导出的六组300帧，与用户评审版本的导出JSON逐字节相同，SHA256 `E20A7BB0BB8CA4FAB4486E3154E81581FD4A029E83A85BEB7812921B74A43449`，保留于.local/release039/motion-clean.json。它证明导出的动作数据一致，不等同于用户试玩过指定干净二进制，也不证明游戏光照、特效与UI下的最终画面。

过程诊断：完整回归入口首次误填CodeCommit参数，原生474项完成后身份校验失败；没有把该次入口直接标为通过。依据build-info.txt的实际S，对同一原生日志重新调用原有严格Full检查器后通过，初始失败保留于.local/release039/full-launch-identity-failure.json，最终结果见原日志同名.result.json及full-clean-corrected.result.json。一次只读Steam探针未指定AppID导致初始化失败，明确设置2868840后恢复；未要求重新登录或修改账户。

全部游戏测试使用隔离存档、隐藏进程和--headless，未启动图形游戏。Rendering、Multiplayer、自然完整流程/Boss平衡及旧档迁移仍为NOT_RUN，不以自动检查或离线回放宣称通过。

## 工坊上传与实际下载

官方ModUploader v0.2.0，可执行文件SHA256 `7D5283DBAFF01EC5182BC08A09F4AAC864CFF870D7D692AF08DAF3830B4EFA19`。上传PID32240，退出码0，返回Successfully uploaded；原条目3823282901与依赖3737335127不变，未新建条目、未代接受条款。上传前后Steam API均为FriendsOnly，没有扩大可见性。

使用正常Steam API刷新既有订阅并下载，最终state=5、installed=true、bytes=58084288、timestamp=1792598168；实际版本0.3.9、codeCommit=S、dirty=false。没有手工写Steam缓存。以下七文件与上传快照逐一一致：

| 文件 | SHA256 |
| --- | --- |
| ASSETS.md | 5779EC01B01344D7A19EDAECADD548974A8B0607F2B475A5CBB3C5D48AB921EE |
| build-info.txt | 6288056248E754461A66BB4967DEB49CDCB984EAD38E86F48072E848712E111A |
| CHANGELOG.md | 34777A97AC29A7EF6BF4019BDB3990B4CEE99179E1570317ECBE5B7B3AA391E3 |
| README.zh-CN.md | 362E6BD8BEFCF8BAE20A697D4BBD506CAC8F8AA9CEC537F8A7CFD93659AA5ACB |
| Sts2Mod.dll | 54AC9BC40AFB6BCC2ED9EDD1945A791C5F004474668F2C53E60D029BB33ABAA3 |
| Sts2Mod.json | 91D89D3EDD1EE750EDCF27EBA09916D3C20748DC2CE57C68336D6B2A62FBDBC6 |
| Sts2Mod.pck | 9C94AB1A2288806C71C1149F6D6A3F89BBAB78A2FC3E1A848EF616885CDFA139 |

封面与规则图均HTTP200、image/png，与本地源文件SHA256一致：封面`AD341A4E78FA90B35C62CDE38DD1F172474D4C792BF52B770BC7C4D8A08CC5E4`，规则图`C8E85383D98D9F4A091DC751A72FC4A997049EB39A3FB9663C48C90746FC8997`。图片、下载状态、上传输出及逐文件哈希保留于.local/release039，不上传账户日志。

## 实际工坊加载验收

| 场景 | 结果 | PID（均已结束） | 日志 |
| --- | --- | --- | --- |
| clean-full | PASS，474项 | 29928 | game-20261021T1544274242848Z.log |
| workshop-load | PASS | 29024 | game-20261021T1557431679361Z.log |
| workshop-character | PASS，77项 | 11928 | game-20261021T1557583951911Z.log |
| workshop-visuals | PASS，95项 | 23500 | game-20261021T1559100045349Z.log |

各次未知错误0，严格识别的原生无GUI诊断依上表顺序为3/0/4/0，退出诊断均0。日志与结构化结果保留在.local/logs，摘要在.local/release039。

三个工坊场景均确认实际DLL/PCK来自workshop/content/2868840/3823282901及3737335127/BaseLib。本地Sts2Mod和BaseLib先与工坊内容核对后移入.local/release039/local-overrides，PC-A当前以实际工坊0.3.9及BaseLib3.4.7生效。干净测试部署前的开发包备份保留在.local/release039/clean-deploy-backups。所有本轮游戏、上传器和探针进程已结束；后续文档提交不重新包装或上传S的七文件快照。
