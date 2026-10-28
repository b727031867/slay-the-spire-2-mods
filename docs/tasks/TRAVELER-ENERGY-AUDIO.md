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
