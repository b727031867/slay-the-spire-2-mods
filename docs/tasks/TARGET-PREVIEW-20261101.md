# 2026-11-01 目标伤害预览排查

基线：origin/main a31ac67，设备PC-A。用户反馈碎岩重击等卡牌选中敌人时未显示计算后伤害，截图目标有翱翔（50%减伤）及易伤。
调用链：NCardPlay.OnCreatureHover → NCard.SetPreviewTarget → UpdateVisuals → DamageVar/CalculatedDamageVar.UpdateCardPreview → Hook.ModifyDamage；卡牌description使用diff()。当前专项只检查切换易伤目标后数字变大，不证明与实际结算完全一致。
必跑：构建、Visuals（包含加载检查），补所有主动单体攻击普通/升级版在力量和翱翔/易伤下，卡面值对照原生DamageReceivedEntry首段结算。按证据确定最小修复点及扩大范围。
风险/不跑：预览必须只读，不能消耗连携/遗物/随机数，不改存档。Full未要求；图形、联机、自然整局未要求。
当前用户游戏仍在运行，等待用户自行退出后部署，无GUI验证，不停止用户游戏。
