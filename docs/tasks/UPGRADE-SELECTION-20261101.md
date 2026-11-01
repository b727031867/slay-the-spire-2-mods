# 2026-11-01 升级选牌预览

基线：origin/main 8dca908，设备 PC-A，任务分支 codex/upgrade-selection-ui。

影响评估：JourneyPlans 使用通用手牌选择，选中后仍显示实卡原属性。普通版选1张，升级版可选0至2张；原生 UpgradeSelect 仅单选，不能直接替换。MisalignedSurvey 对非旅行者执行永久升级，却使用没有升级预览的 FromDeckGeneric。修复须保持实卡身份、确认前只读、撤选/跳过、多选、原生选择同步和事件费用校验；不改存档格式、升级效果或元素规则。

检查范围：生产 CardSelectCmd 调用方、原生 NPlayerHand/NUpgradePreview/选中卡容器、现有卡牌与同伴事件回归、严格日志检查器。消耗、保留、检索和奖励使用通用选择符合用途；ElementTuning 的候选已在显示前升级。仅修复有证据的两处升级入口。

必跑：构建、Cards（复用其中加载与视觉检查）、Events，补真实挂载原生控件的无GUI断言；检查确认前实卡不变、升级属性、多选和预览清理。不跑 Full（未授权）、图形、联机和磁盘恢复（本次不改持久化）；不把无GUI控件断言表述为观感验证。

Laya：原调用因 Ollama 后台未运行而 skipped。用户明确要求恢复服务后，工具启动被自动审批拒绝，未绕过；用户手动打开 Ollama。恢复检查确认版本0.40.10、本地 Laya 模型存在，/api/ps 显示 size_vram=732797336，路由接口成功。旧日志不足以确定服务退出原因。新决策9dba4ec3ae8d41f4ad482fe93bf34066选择 gpt-6.1-sol medium（complex 38.2%，moderate 33.9%，confidence 0.176）；已派发唯一执行代理 /root/laya_upgrade_fix。路由与执行日志只保存在本机，不提交原始工单。

实现：JourneyPlans 保留原生 FromHand/SimpleSelect 协议，为选中的真实卡牌挂独立升级克隆；复用原生升级文本渲染，实卡与选择同步身份不变。预览不接收鼠标/焦点，释放前恢复池节点的输入设置；撤选、确认、取消与容器退树时清理订阅和预览。非旅行者测绘台改用原生 FromDeckForUpgrade，其余资格和费用校验不变。

最终被测核心：423e1a7cf5cdd3d45ceeeb5419d92708aebd4e76（clean），Build-Mod PC-A 成功，游戏 v0.107.1/build23811903、BaseLib3.4.7。DLL SHA256 AF5BED5D4338DCC54C4B6ED1D9283FF0ED79D1BDF6E47D1E740778A693EF588E，PCK SHA256 2AE75A6921224D4691D4BB81C57BB8E44C56838A6838AA9475731EE3749C25BC。当前安装与这份核心一致。

验证：Cards 1192项 PASS，日志 .local/logs/game-20261101T0748535410194Z.log；Events 533项 PASS，日志 .local/logs/game-20261101T0812499022329Z.log；两份同名 .result.json 均为绑定核心/环境/日志哈希的 schemaVersion=2。Cards 包含真实 PlayCardAction 进入升级选牌，覆盖数值/费用/关键字、普通/升级版选择上限、实卡确认前不变、临时升级不改永久牌组、撤选/跳过/取消/重复进入及回调清理。Events 从实际测绘台选项进入原生升级预览，确认前不改卡/金币，取消后保留；原有五角色免费/60金币等检查保留。Test-ModTestEntry12项、Test-LogClassification26项 PASS。测试进程由统一入口回收，未启动GUI。

失败记录：首次 Events 的505项事件逻辑断言通过，但大厅既有控件测试阶段出现原生 RID/mem/texture 渲染级联，其中 wrong RID 未满足严格分类条件，报告保留 FAIL（game-20261101T0810083404877Z.log）。使用完全相同核心新建隔离夹具重跑，严格检查 PASS，未知错误0、已知原生headless错误5；没有修改错误分类、降低断言或重建核心。Cards 未知错误0。

服务恢复补充：查到已有 Ollama 启动项被禁用；用户明确选择恢复自启动后，仅恢复该既有项为启用并回读验证，备份位于本机 .codex/state/laya-worker/recovery/ollama-startup-20261101-155126.json。服务版本接口正常，未安装/升级模型、驱动或Ollama。未重启或注销验证登录自启。Windows 自启动设置说明：[Microsoft Support](https://support.microsoft.com/en-us/windows/experience/startup-boot/configure-startup-applications-in-windows)。

交付范围：Full、图形观感、联机、自然整局、磁盘恢复与用户试玩 NOT_RUN；Workshop说明实际UTF-8为5019字节，配置public。不上传工坊，等待本地候选试玩；仅复用上述最终PASS报告准备候选。
