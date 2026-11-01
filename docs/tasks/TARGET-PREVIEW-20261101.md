# 2026-11-01 目标伤害预览排查

基线：origin/main a31ac67，设备PC-A。用户反馈碎岩重击等卡牌选中敌人时未显示计算后伤害，截图目标有翱翔（50%减伤）及易伤。
调用链：NCardPlay.OnCreatureHover → NCard.SetPreviewTarget → UpdateVisuals → DamageVar/CalculatedDamageVar.UpdateCardPreview → Hook.ModifyDamage；卡牌description使用diff()。当前专项只检查切换易伤目标后数字变大，不证明与实际结算完全一致。
必跑：构建、Visuals（包含加载检查），补所有主动单体攻击普通/升级版在力量和翱翔/易伤下，卡面值对照原生DamageReceivedEntry首段结算。按证据确定最小修复点及扩大范围。
风险/不跑：预览必须只读，不能消耗连携/遗物/随机数，不改存档。Full未要求；图形、联机、自然整局未要求。
当前用户游戏仍在运行，等待用户自行退出后部署，无GUI验证，不停止用户游戏。

当前进度：新增原生DamageReceivedEntry对照用例已编译，干净测试核心8d1bb009d98a已Build-Mod成功；尚无生产修复，不宣称已定位根因。包artifacts/Sts2Mod-0.3.18-v0.107.1-8d1bb009d98a/Sts2Mod尚未部署。用户游戏PID43960仍运行（本轮未启动任何测试进程），Visuals为BLOCKED；未合main、未推送、未发布。收到用户退出消息后先复核进程，部署该诊断包运行Visuals，按实际预览/原生结算差异继续定位，必要时扩展目标选择真实回调与状态更新场景。
