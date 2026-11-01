# 2026-11-01 升级选牌预览

基线：origin/main 8dca908，设备 PC-A，任务分支 codex/upgrade-selection-ui。

影响评估：JourneyPlans 使用通用手牌选择，选中后仍显示实卡原属性。普通版选1张，升级版可选0至2张；原生 UpgradeSelect 仅单选，不能直接替换。MisalignedSurvey 对非旅行者执行永久升级，却使用没有升级预览的 FromDeckGeneric。修复须保持实卡身份、确认前只读、撤选/跳过、多选、原生选择同步和事件费用校验；不改存档格式、升级效果或元素规则。

检查范围：生产 CardSelectCmd 调用方、原生 NPlayerHand/NUpgradePreview/选中卡容器、现有卡牌与同伴事件回归、严格日志检查器。消耗、保留、检索和奖励使用通用选择符合用途；ElementTuning 的候选已在显示前升级。仅修复有证据的两处升级入口。

必跑：构建、Cards（复用其中加载与视觉检查）、Events，补真实挂载原生控件的无GUI断言；检查确认前实卡不变、升级属性、多选和预览清理。不跑 Full（未授权）、图形、联机和磁盘恢复（本次不改持久化）；不把无GUI控件断言表述为观感验证。

Laya：原调用因 Ollama 后台未运行而 skipped。用户明确要求恢复服务后，工具启动被自动审批拒绝，未绕过；用户手动打开 Ollama。恢复检查确认版本0.40.10、本地 Laya 模型存在，/api/ps 显示 size_vram=732797336，路由接口成功。旧日志不足以确定服务退出原因。新决策9dba4ec3ae8d41f4ad482fe93bf34066选择 gpt-6.1-sol medium（complex 38.2%，moderate 33.9%，confidence 0.176）；已派发唯一执行代理 /root/laya_upgrade_fix。路由与执行日志只保存在本机，不提交原始工单。
