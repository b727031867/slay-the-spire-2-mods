# 旅行者能量球与元素音效

基线：main b1ea32b，PC-A；游戏与BaseLib版本由构建入口核对。

- 改动：角色能量球场景；旅行者普通/事件卡牌、遗物和药水池的大小能量图标；出牌表现回调与实际连携奖励反馈增加短音效。
- 风险：局部表现，不改能量计算、连携规则、模型ID或存档；原生公共卡/遗物无主时保留原生图标。小图必须保持24×24，提示图74×74。
- 必跑：可复现音频生成自检、编译/导入/导出、Visuals无GUI专项；包含选人资源加载、原生能量悬停布局、零能量变暗/回能、旋转节点、音频加载、静音和无GUI无播放检查。用户2026-10-28强调限定相关范围，取消完整Character专项（未改角色注册、起始内容或结局）。保持严格日志验证。
- 不跑：Full（未授权）、磁盘恢复（无持久化改动）、图形/听感及真实联机（未授权）。
- 扩大条件：角色场景转换、原生能量格式化或表现钩子失败时补对应检查，不自动运行Full。

视觉为原创SVG：青绿、岩金、雷紫外环，深色中心为能量数字留白。音效为标准库确定性合成的单声道PCM WAV，不使用外部录音或游戏素材。原生伤害音效继续播放，新增声音仅为轻量元素提示。

## 实施与验证（2026-10-28，PC-A）

- 分支：codex/traveler-energy-audio；干净构建核心924401c40765a9821c451e0271bad7bd327ebbae，版本保持0.3.17。后续文档提交不改变此核心。
- 能量球使用BaseLib CustomEnergyCounterPath与原生NEnergyCounter，保留数字、零能量变暗、回能刷新和原生旋转。5个旅行者池接入BaseLib独立大小图标接口；有主的原生牌/遗物/药水按原生规则使用角色牌池，无主公共模型的colorless文字在对局中按原生EnergyIconsFormatter回退到当前角色，显式ironclad前缀保留原生图标。
- 元素音在旅行者出牌表现BeforeCardPlayed首次结算触发；连携音在ShowReward实际奖励反馈触发。播放由BaseLib SFX通道管理、绑定当前Run，无音高随机，无新增玩法等待；同一种提示100ms去重，无GUI静音，主音量/音效音量为0不播放。
- python scripts/New-ElementAudio.py：四个音频确定性、格式、峰值和端点自检PASS，源文件峰值≤0.35（低于-9dBFS），运行时另减6dB。
- 编译/导入/导出PASS，0错误；现有TravelerSandwormTests.cs:70的两项空引用警告保留，本任务未修改该文件。
- 独立Godot无GUI加载最终PCK：四张图尺寸24/74/128/128、透明角，四段音频44.1kHz单声道/0.30–0.52秒/无循环，场景128×128及旋转中心全部PASS。此检查不调用游戏Steam接口，不代表原生能量UI运行通过。
- Test-ModTestEntry：12项PASS；Test-LogClassification：26项PASS。严格检查器新增能量球及音频必要标记，未削弱已有断言。
- Visuals：BLOCKED。Steam起初未运行；后台启动Steam后无GUI测试在游戏启动阶段报Steamworks ConnectToGlobalUser failed（k_ESteamAPIInitResult_FailedGeneric），尚未加载Mod或执行专项。日志.local/logs/game-20261028T0814070381169Z.log及其FAIL结果保留，不计为功能测试通过。
- Character：NOT_RUN，受同一Steam账号环境阻塞；Full/磁盘恢复/图形/听感/联机均NOT_RUN。
- 本轮测试进程已由统一入口清理。仅删除本轮临时本地四文件副本，原7文件Workshop安装已恢复并逐项哈希核对；未修改正常存档，未上传或更改Workshop条目。
- 未合入main、未推送、未冻结正式发布候选；等待用户登录拥有游戏的Steam账号，再对同一核心运行Visuals与Character，通过后按现有交付流程处理。artifacts/Sts2Mod-0.3.17-v0.107.1-924401c40765/Sts2Mod为开发核心，不计PREPARED。

核心SHA256：

| 文件 | SHA256 |
| --- | --- |
| Sts2Mod.dll | 3743057EDEFE393520D0D51E9322260F462BDF4E1F83A1604F54255538F74B4C |
| Sts2Mod.pck | CD9FCEE5E8ED6FE05E69C99570607A16C93A2F4EB4FF630787A1A220046FC734 |
| Sts2Mod.json | 0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A |

## Steam登录后定向验证（2026-10-28）

用户要求仅验证本次改动，最终只运行Visuals专项，复用其中的启动、选人资源及出牌表现证据；取消原计划的完整Character专项。未运行Cards、Events、存档恢复或Full。现有Visuals套件中的严格用例全部保留，无新增过滤入口或放宽未知错误处理。

首次原生验证暴露测试夹具预期错误：原生EnergyIconsFormatter在colorless文案中回退当前角色前缀，因此未取得的共享遗物在旅行者对局里同样使用旅行者图标；修正这两条断言并新增显式ironclad前缀仍保留原生图标的断言。新增隔离断言初次遗漏Energy动态变量导致格式化失败，补齐时先遇到LocString.Add重载编译错误，改用原生Add(DynamicVar)后编译通过。随后计数器检查在原生CombatStateTracker下一帧延迟通知前断言，修正为最多5秒等待真实UI状态，仍严格断言归零变暗和回能恢复。均仅修改测试夹具与检查器，运行时素材及实现未变；早期失败日志和构建日志保留，不计PASS。

- 最终干净核心：423c8008b644089042826aaa5d042f6b6070c560，PC-A；游戏v0.107.1/build23811903，BaseLib3.4.7。
- 构建/导入/导出PASS；PCK哈希与首版一致，测试夹具变化使DLL哈希变化。
- Visuals：PASS，236条断言，73.202秒，未知错误0。包含5个池的能量符号、24×24行内/74×74悬停图、原生显式前缀兼容、360×276悬停布局、自定义原生计数器场景、归零变暗/回能恢复/旋转中心，以及三元素音频映射、四段WAV加载、音量门控和headless静音。
- 日志：.local/logs/game-20261028T0824232093759Z.log；schemaVersion=2报告位于同路径.result.json，绑定最终四文件核心、环境与日志哈希。
- 日志SHA256：01B0941AD39AF0C68CEB4F6294E4BD90B182E85EE5C1394A63F1BFFCA0947726。
- 最终DLL SHA256：FF33D62C29D83D72CCD4A8F8C713F356D307C0E1D28CC785EE7DB764791B4FF7；PCK SHA256：CD9FCEE5E8ED6FE05E69C99570607A16C93A2F4EB4FF630787A1A220046FC734。
- 测试PID33152已结束；移除本轮临时四文件副本，原Workshop安装七文件逐项哈希恢复。未修改正常用户存档。
- Test-ModTestEntry更新后12项PASS。图形观感、实际听感、真实联机NOT_RUN；无GUI静音检查不证明音效听感或最终混音。
- 本次验证完成，保留任务分支与开发核心artifacts/Sts2Mod-0.3.17-v0.107.1-423c8008b644/Sts2Mod；未合入main、未推送、未冻结正式交付或发布Workshop。

## 本次直接发布授权

2026-10-28用户明确要求“发布到工坊，我朋友会进行验证”。此最新特定指令授权本次直接发布已通过Visuals的423c800核心，不等待额外试玩确认；不将该授权或朋友验证计划记作试玩、图形或联机PASS，也不修改后续发布的一般规则。保持0.3.17、原条目3823282901及public。复用同包236项Visuals证据，仅更新随包说明与工坊变更记录，不重建或扩大回归。
