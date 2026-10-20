# TASK-010：方案A、罗盘两回合间隔、头像与工坊交接

2026-10-21用户已评审并试玩，明确采纳方案A。还要求左上角改大头照、初始遗物每两个回合触发一次且图标提示可用状态，更新工坊、推送远端并交接另一台PC。版本0.3.2，分支`codex/TASK-010-balance-workshop`。状态DONE_SINGLEPLAYER_HEADLESS_WORKSHOP_PRIVATE_MANUAL_PENDING。

元素共鸣仅在第2次实际奖励回能一次/份，成本2（升级1）及金额1不变；第3次不重复，跳过/零伤害/抽0不计，额外回合重置。方案B未实现。

罗盘每战首次可用。实际触发后本回合不能再触发，下一次自己的回合冷却，再下一次恢复；未触发不自动消耗，额外回合也计入。自身抽牌尝试后进入冷却，受禁抽/满手限制；基础连携无实际收益则不消耗。原生RelicStatus.Active/Disabled与ShowCounter/DisplayAmountChanged显示1亮或0暗，标题显示就绪/已用/冷却，战斗外恢复普通状态；回战斗入口存档按原生回滚重置，不承诺任意动作中途续接。

左上角通过Character.Icon的icon.tscn接入新的head_icon.png；CustomIconTexturePath也同步，选人/地图icon.png和战斗body.png保持。图像来源与提示见[头像记录](../art/TASK-010-portrait.md)。

工坊必须保留3823282901及private，不新建条目，不接受账号法律条款、不公开发布。上传后检查实际Steam下载的7个文件哈希、版本与来源，以及封面/相册HTTP读取和哈希。测试均无GUI、隔离原生档案，只清理自己启动的进程。未改动/复制用户正常存档。

具体最终S、包哈希和通过范围在[验收报告](../tests/TASK-010-PC-B-run-01.md)及[跨PC接续](../handoffs/NEXT-PC-CONTINUE-PROMPT.md)记录。外部发布只有收到实际成功回调并核对远端/下载后才记PASS。

最终S a3ffe66bd48dce568af2bdff6f7bdd2bb4bf0b18：49纯规则、26日志分类、376完整游戏回归、13原生保存及250恢复/受控连续战斗通过。已更新原私有工坊，正常Steam下载7文件、BaseLib3.4.7及两张CDN图一致；工坊普通启动和角色专项59项通过。测试游戏进程已结束，以工坊副本部署。后续UI观感、自然Boss平衡和多人不在本轮无GUI结论内；未合并main或公开发行。
