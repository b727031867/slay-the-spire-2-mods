# TASK-010 / PC-B / 0.3.2

2026-10-21。用户已试玩并批准方案A、头像大头照、罗盘每两个回合一次及图标提示，授权更新工坊、推送远端和跨PC交接。分支`codex/TASK-010-balance-workshop`，不合并main、不改变工坊private。

## 规则与边界

元素共鸣由第3次改为第2次实际连携回能；费用、份数、能量和三次上限不变，第3次不重复，无实际收益不计。方案B未实现。

罗盘每战首次可用；触发后下一次自己的回合冷却，再下一次恢复，额外回合计入，未触发保留机会。自身抽牌尝试即进入冷却，受禁抽/满手限制；基础连携没有实际奖励不消耗。图标通过原生计数和状态事件显示亮1/暗0，标题显示可用/本回合已用/冷却，战斗结束清理。用户已自行保存退出；只停止本轮AI测试进程。

## 开发验证（不替代最终干净包）

开发包a85f42c34cd11312e3e3eb9e5e6bd67297a4f312-dirty角色专项59项通过，日志game-20261020T1801423845235Z.log，PID11108已结束。含额外回合、无实际奖励、自身抽0、未触发保留、战斗结束和原生计数事件。该dirty包不上传工坊。

左上角原生Character.Icon场景和CustomIconTexturePath均接入新的head_icon.png；生成过程、引用和哈希见[头像记录](../art/TASK-010-portrait.md)。新工坊封面与规则图已本地视觉检查，文字完整、无裁切。

## 最终验收

干净源码S：`a3ffe66bd48dce568af2bdff6f7bdd2bb4bf0b18`。Mod清单/程序集0.3.2，游戏v0.107.1/build23811903、BaseLib3.4.7。后续验收脚本/文档提交不重建或冒充S产物；游戏C#与资源始终使用S包。

包`Sts2Mod-0.3.2-v0.107.1-a3ffe66bd48d.zip`，SHA256 `834612844C136AC5F66A48379F64F1F082833D8FA2E003BECCB0E39F2C487016`。锁定还原、ExportRelease/win-x64、Godot导入导出成功；本次干净构建日志build-032-clean.log编译0警告/0错误，无Godot ERROR。开发构建曾有编辑器退出EditorSettings诊断，未在干净构建复现，不冒称已经修复引擎。49项纯规则、PowerShell语法及中英cards/powers/relics键集（41/34/10）一致性通过。

全量运行早期出现Godot无GUI的相邻`texture_2d_initialize`空t及`texture_free`空texture诊断。来源与[Godot官方4.5.1 dummy texture实现](https://github.com/godotengine/godot/blob/4.5.1-stable/servers/rendering/dummy/storage/texture_storage.h)一致，记录为原生无GUI诊断，不声称消除了它。新增分类仅接受Headless且前一对行是确切dummy初始化失败、当前来源也是dummy释放；孤立、不同源、图形模式及非相邻均仍失败。新增6项反例/正例，总26项日志分类通过；S旧检查器会把新的释放诊断判为未知，本轮采用随后提交的严格分类器。不是放宽游戏断言或忽略所有null错误。

S包完整游戏回归376项通过，含角色专项44项、新卡/图片/随机专项164项，日志game-20261020T1809366758754Z.log，`Test-ExpandedCardRun.ps1 -Full`严格检查通过。元素共鸣第2次每份回能、第3次不重复、跳过后按实际奖励计数与额外回合通过；20项PNG和原生头像接线通过。未知错误0，已知原生无GUI诊断2，退出诊断0（此项不是原生退出流程）。PID13852由AI结束，不启动图形界面。

```text
STS2MOD_EXPANSION_COMPLETE assertions=164 cards=18 powers=4 multiplayer=NOT_RUN rendering=NOT_RUN
STS2MOD_TEST_COMPLETE assertions=376 multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave=False
```

S包原生磁盘准备13项通过，日志game-20261020T1820077858128Z.log，PID27820原生退出。重新部署同一S包（备份.local/backups/20261020T1821170315088Z）后，PID7544新进程恢复/受控连续战斗250项通过，日志game-20261020T1821171476184Z.log；两阶段均通过Test-Task003EndToEnd.ps1。唯一沙盒为.local/task003-saves/30f412f73e6e4b4fb9498565c2cd79a6，原生user命名空间对应该ID，无Steam云。

```text
STS2MOD_E2E_PREPARED assertions=13 cards=18 upgrades=18 paimon=1 shouldSave=True boundary=combat_entry
STS2MOD_SOAK_COMPLETE seed=TASK003A battles=6 turns=23 actions=82 combos=48 natural_wins=1 energy_injection=false hand_injection=false bounded_turns=4
STS2MOD_E2E_COMPLETE assertions=250 acquisition=PASS disk_restart=PASS soak=PASS multiplayer=NOT_RUN ui=NOT_RUN
```

六场23回合82次出牌48次连携，1自然胜利，其余四回合预算清理；受控5000生命且无手牌/能量注入，不是自然通关或Boss平衡验证。两阶段均原生退出，未知错误0、无GUI诊断0、已知原生退出诊断各9，未声称修复退出资源泄漏。

官方工坊上传及Steam实际下载/CDN核对已完成，详细见下节。历史TASK-009与开发期59项不是本轮完整通过证据。

## 上传工具与工坊工作区

准备目录`.local/workshop/a3ffe66bd48dce568af2bdff6f7bdd2bb4bf0b18`，mod_id=3823282901，visibility=private，依赖3737335127不变。上传前只准备，未创建新条目。官方ModUploader v0.2.0 Windows x64 ZIP SHA256 `2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`，只对上传进程设置.NET主版本前滚。WorkshopProbe使用官方随附Steamworks.NET15.0.1包，SHA256 `E32DA6B4199384A1AAA15169514FD957B9FF8BA5BDC487BFED6F91E856DC01A6`。未提交二进制或账号日志、未接受平台条款。

正常Steam下载7文件已逐项比较PASS（不是手工复制到Steam缓存）：

| 文件 | SHA256 |
|---|---|
| Sts2Mod.dll | 72210BDE349BF804A52C2851A565996B3D0E570F44329797F4C6DB0245EBBCCB |
| Sts2Mod.json | 79B9F6469BF94E0DE39642CD4B7DA019357BAA46A391B7130717170746C983BB |
| Sts2Mod.pck | 6C5441B006C74B2DF999805180384FC77A69D263C1F4BE85C9B4A0C7BA74B227 |
| build-info.txt | B0BBA713FBA3C6CF88584732D9B464C503D3BEBCF579E42B2D75D10D4EF27D13 |
| README.zh-CN.md | DC877839051604925005B0B0DA7717526EF829E2A10EB3B2A7C6F78A0628032A |
| CHANGELOG.md | 3D918C1CF88865FE49786DC3CF509A71464DC010FDB81526363C0586A7D39B67 |
| ASSETS.md | 81CE53980EEB18B8D65BD558BA76870BD6B73735950DF69A639D8DFECD9E1A8A |

封面SHA256 `0543FEDF8BE2EAADB059B7C360ECB0B815DCD259A229E1EB11EBB9D9BAA81E9F`；相册rules.png `470C841A2E17FE53B3E108E812C42642C091A3F4E4561EAF90B33E269BC12F8D`。均小于1MB。

2026-10-21 02:24（UTC+8）官方上传进程4852返回Successfully uploaded，ID3823282901未变，dependencies没有改动。官方工具常规打印条款说明，未出现需账号持有人接受的新协议流程；没有代操作接受协议按钮。账号与原始Steam输出仅本机忽略目录保存，不提交日志。上传后WorkshopProbe返回k_EResultOK、private、consumerApp2868840；没有新建或公开条目。

通过原生SteamUGC订阅/下载（上传前两个条目已是订阅状态5，未取消订阅）获取Mod与BaseLib。Mod最终state=5，installed=true，bytes=55593136，timestamp1792520670；实际安装目录steamapps/workshop/content/2868840/3823282901中7文件与上表完全相同，build-info明确0.3.2、S且dirty=false。BaseLib实际在3737335127/BaseLib子目录，版本3.4.7；DLL/JSON/PCK与锁定官方包分别一致：`55863CA6ADC30A61A7544874E157A519FFE148CCF2B18025947609C4BFF813CE` / `8B0D470C1C624EED9F7BEBF79FA7985ED7F8F4965154713546890A910A61D6F8` / `95E653B9AEA9A3DCBC45971B8933C854E314B897488643DB71DF1E14A76B808D`。

从远端API取得的[新封面](https://images.steamusercontent.com/ugc/9842013854829469669/1F69B5A80717D50F060CA3734257BE59CE12FE46/)和[相册rules.png](https://images.steamusercontent.com/ugc/10777887187744484321/C3A80A97B94317C079768CC389E2E9CC35315F97/)均实取HTTP200、Content-Type=image/png、PNG签名89504E470D0A1A0A，SHA256分别与源封面/规则图相同，无需修复相册。验证文件留在本机忽略目录，未向Steam缓存写入。

确认游戏已退出后，校验本地同ID副本与下载一致，将mods/Sts2Mod和mods/BaseLib可恢复移动到`.local/backups/workshop032-20261020T182549Z`，防止双重加载；未删除文件、未改动正常存档。随后普通无测试/调试参数的无GUI工坊启动通过：日志game-20261020T1825499858736Z.log明确从两条Steam Workshop路径加载DLL/PCK；一次初始化0.3.2+S、BaseLib3.4.7、到达主菜单，未知错误0、原生无GUI诊断0。PID4416已由AI结束。当前本机以工坊副本生效，不重新复制本地副本或启动图形游戏。

再用全新隔离沙盒从工坊加载S包运行角色专项59项，通过Test-TravelerCharacterRun.ps1 -Focused，日志game-20261020T1826176543881Z.log。含44项角色/罗盘检查，未知错误0，原生无GUI诊断4单列；PID33124由AI结束。该专项不冒称为从工坊再次完整运行376项或自然整局。最终API复查仍private、state5、同ID、同CDN地址与下载大小。所有本轮测试游戏进程已结束，没有启动图形游戏。

## 交付身份

远端交接分支为`codex/TASK-010-balance-workshop`。S后续只修改验收脚本（严格区分新的dummy纹理清理诊断及6项测试）和文档，不改变src/Sts2Mod或workshop上传内容；通过git diff S -- src/Sts2Mod workshop核对无差异。工坊/本机交付包继续标记S，不能用后续验收记录提交冒充包身份。没有合并main、创建PR或公开Release。另一PC按接续提示词获取远端此分支；当前机器没有对另一PC执行登录、同步或测试。

## 限制与交接

全部AI游戏验证无GUI，隔离原生存档且无Steam云。不启动图形游戏。原生UI资源/状态逻辑不证明人工观感，自然Boss平衡、多人和用户旧档迁移未验证；受控六场不等于自然通关。接续入口为[另一台PC提示词](../handoffs/NEXT-PC-CONTINUE-PROMPT.md)，仅准备交接不声称另一PC已同步或运行。
