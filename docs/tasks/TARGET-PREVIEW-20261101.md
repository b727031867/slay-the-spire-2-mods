# 2026-11-01 目标伤害预览排查

基线：origin/main a31ac67，设备PC-A。用户反馈碎岩重击等卡牌选中敌人时未显示计算后伤害，截图目标有翱翔（50%减伤）及易伤。
调用链：NCardPlay.OnCreatureHover → NCard.SetPreviewTarget → UpdateVisuals → DamageVar/CalculatedDamageVar.UpdateCardPreview → Hook.ModifyDamage；卡牌description使用diff()。当前专项只检查切换易伤目标后数字变大，不证明与实际结算完全一致。
必跑：构建、Visuals（包含加载检查），补所有主动单体攻击普通/升级版在力量和翱翔/易伤下，卡面值对照原生DamageReceivedEntry首段结算。按证据确定最小修复点及扩大范围。
风险/不跑：预览必须只读，不能消耗连携/遗物/随机数，不改存档。Full未要求；图形、联机、自然整局未要求。
当前用户游戏仍在运行，等待用户自行退出后部署，无GUI验证，不停止用户游戏。

当前进度：新增原生DamageReceivedEntry对照用例已编译，干净测试核心8d1bb009d98a已Build-Mod成功；尚无生产修复，不宣称已定位根因。包artifacts/Sts2Mod-0.3.18-v0.107.1-8d1bb009d98a/Sts2Mod尚未部署。用户游戏PID43960仍运行（本轮未启动任何测试进程），Visuals为BLOCKED；未合main、未推送、未发布。收到用户退出消息后先复核进程，部署该诊断包运行Visuals，按实际预览/原生结算差异继续定位，必要时扩展目标选择真实回调与状态更新场景。

诊断结果：8d1bb00的Visuals369项PASS（game-20261101T0619096250724Z.log），12种单体攻击普通/升级在普通、翱翔+易伤场景的48次首段预览与原生结果一致。截图碎岩重击+：(18+2×3)×1.5×0.5=18，数字本身正确。
新复现：adf0ad1的Visuals FAIL（game-20261101T0622583471448Z.log），无界罗盘第3次连携前逆风牵制预览6，原生实打7。根因是Reserve先授予1力量再攻击，原预览只看已有力量。修复限于UnboundJourneyCompass的只读加算预览，在Hook.ModifyDamage同步调用范围标记预览模式，finalizer恢复嵌套上下文；实际结算None模式不增加任何伤害。仅ElementCardModel在手牌、将触发第3次连携且罗盘未用时计算；碎岩重击按3倍力量，普通牌按1倍。原生加算/附魔/乘算/伤害上限顺序保留。
扩大检查：Visuals覆盖全部12种单体攻击的普通/升级、重复预览、同元素/额度耗尽边界；ComboRules确认连携状态规则未改。真实玩法未改，不跑Cards/Full；Full未要求。无GUI不证明图形交互或联机。

最终：干净核心fec6f9d8ec1c1b9d0191251f340419de5c275bf4构建成功，Visuals443项PASS、未知错误0；ComboRules57项PASS（ComboState源码未变化）。12种攻击普通/升级、普通目标/翱翔+易伤48次首段、待触发罗盘24次首段全部等于原生DamageReceivedEntry.TotalDamage；重复读取、同元素、次数耗尽边界通过。日志.local/logs/game-20261101T0627553144881Z.log与绑定.result.json，测试PID55096已清理。DLL SHA256 2E9DF96C2584D27F1B5FA2946F8A9A2ED5F81E391BDD67E1C87C9F61B7C9BB8C，PCK SHA256 F9E7AE552BC07D53DEBAF7D6FF7F4B47D4F5CBE2F170C4F4E82CA19CB43CBA99。
中间ac7664b、9ef2de8的24个罗盘修复对照均通过，但尾部新控制牌未挂载导致NullReference；根因是罗盘额外抽牌累积至手牌上限，补独立用例间清空手牌。两份失败日志game-20261101T0625174305861Z.log与game-20261101T0626263411882Z.log保留；不当成最终包通过证据。没有放宽断言或日志分类。
当前安装与被测最终包一致；只复用最终报告准备候选。Full、图形、联机、自然整局、用户试玩NOT_RUN；不上传工坊，不启动GUI。
