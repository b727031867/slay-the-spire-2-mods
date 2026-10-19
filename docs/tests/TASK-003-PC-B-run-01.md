# TASK-003 PC-B 单人集成报告 01

日期：2026-10-19。范围：完成 TASK-002 首版内容并开始 TASK-003 单人集成验收。

## 身份

- 被测代码 S：`744b0490e268aef67d19a64877c3a5c7c35bd620`
- 测试工具修正：`bc1458bc71cd775ddf1874dc0866a7e4a4e13d37`
- Mod：0.1.0；游戏：v0.107.1 / Steam build 23811903；BaseLib：3.3.2
- PC-B：Windows 11 x64 build 26200；.NET SDK 10.0.402；Godot .NET 4.5.1
- 包：`Sts2Mod-0.1.0-v0.107.1-744b0490e268.zip`，213,281 字节
- SHA256：`1F1A6629C65C6491C01D6F340BB5A91907EDD633AF2E215D75328D95ED3EABC1`

## 结果

| 检查 | 结果 |
|---|---|
| PC-B 环境检查、真实游戏程序集编译、PCK 导出 | PASS；环境 PCK 与 PC-A 哈希一致 |
| 正式 Mod 干净构建 | PASS；0 警告、0 错误 |
| 纯规则 | PASS；35 项 |
| TASK-002 内容 | PASS；六张牌、三连招、一个遗物；完成标记 79 项 |
| 完整单人真实游戏自动化 | PASS；91 项 |
| 独立日志检查 | PASS；一次 READY、代码提交匹配、BaseLib/resource 正常、无未知错误 |
| 自动/重复/额外回合 | PASS |
| 死亡目标与战斗结束边界 | PASS；不转移目标，最终击杀不发战斗内奖励 |
| 正常卡池和遗物池 | PASS（池注册）；人工点击获取 NOT_RUN |
| 图形 UI、自然整局、平衡性 | READY_FOR_USER_PLAYTEST |
| 完整退出读档 | NOT_RUN |
| 双人主客机 | NOT_RUN |

完整 headless 日志产生四条 Godot 渲染 RID/shader 错误文本。它们在同一日志中不影响主菜单、模组加载或 91 项逻辑断言；检查器仅在显式 `-Headless` 时按精确文本过滤这四条，任何其他 ERROR 仍会失败。无窗口测试不替代图形界面验收。

发现并修复：基础伤害击杀最后一个敌人后，引擎 `IsEnding` 可能尚未及时反映结束状态。实现增加存活敌人检查，确保不再发连招和派蒙奖励。修复后针对性验收 12 项与本次完整 91 项回归均通过。

可审计摘要见 [evidence/TASK-003-PC-B-rules.txt](evidence/TASK-003-PC-B-rules.txt)。原始日志保留在本机 `.local/logs/game-20261019T1412294491392Z.log`，不提交仓库。
