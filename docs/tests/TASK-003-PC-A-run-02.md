# TASK-003 PC-A 0.1.1 无GUI回归报告 02

日期：2026-10-20。范围：回归0.1.1已有内容，修复回归工具并明确TASK-003后续范围。结果以本报告和关联证据为准，不沿用0.1.0的通过结论。

## 被测身份

- 分支：`codex/TASK-002-continue`
- 干净代码S：`88cdb3108a200d8328f3dfaced15e4549cb5b610`；后续报告提交仅修改文档/证据，未重新标记该包。
- Mod 0.1.1；游戏 v0.107.1 / Steam build 23811903；BaseLib 3.3.2。
- .NET SDK 10.0.402；net9.0；Godot .NET 4.5.1；PC-A Windows x64。
- 游戏程序集SHA256：`A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`。
- 包：`Sts2Mod-0.1.1-v0.107.1-88cdb3108a20.zip`，213,668字节。
- 包SHA256：`0231D4BAC922DA118D1B395F90BF0197ABBAC8E55DBC46125C134944020FD932`。
- 同步：[GitHub草稿附件](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-8342af480195e136f78c)，release ID `417907943`；ZIP和SHA256文件均已上传，服务端digest及大小与本地一致，draft=true，未公开发布。

## 结果

| 检查 | 结果 |
| --- | --- |
| S干净构建、PCK导出及部署 | PASS；0警告、0错误 |
| 纯连招规则 | PASS；27项 |
| 日志分类正反例 | PASS；6项 |
| TASK-003针对性边界复验 | PASS；12项，同一S/包 |
| 全量真实游戏单人回归 | PASS；94项，包含重复使用的原生回合重置检查 |
| TASK-002完成标记 | 第82项；六张牌、一个遗物，acquisition=POOL_VERIFIED |
| 独立全量日志检查 | PASS；一次初始化、完整S匹配、BaseLib/resource正常 |
| 最终日志ERROR | 0；已知headless分类数0，未知错误0，未通过过滤掩盖本次错误 |
| 测试进程清理 | 完成；仅结束本轮自建headless测试进程，未重启GUI |

已覆盖：六牌及升级、三类双向连招、每回合共享3次、派蒙每次奖励翻倍/不叠加、自动打出和重复结算、普通牌穿插、能量不足、原生伤害修正、额外回合、死亡目标不转移、最终击杀不发奖励、卡/遗物池和模型序列化。最后击杀观测为`player=1 action=153 block_before=7 block_peak=7 block_after=0`，同一行动有`skipped=combat_ended_or_dead`证据。

## 本轮问题及修复

1. 首次全量0.1.1运行在TASK-002完成82项后，最后击杀断言失败。原断言在原生战斗清场后检查剩余7格挡，而原生`Player.AfterCombatEnd`会清空格挡；日志显示该行动实际已跳过连招奖励。修正测试以`BlockChanged`记录击杀过程峰值，并要求7→峰值7、同一player/action的明确skip日志。实际清场后为0。未修改生产玩法或放宽奖励规则；修正后先12项针对性回归，再执行同包全量回归。
2. 原日志检查器只按错误文本识别headless渲染问题，并遗漏本机出现的RID文本。共享分类器现在联合匹配原生错误文本及紧随的RID/dummy-renderer源码位置，且只在显式`-Headless`时分类。相同文本来自玩法位置、缺少位置、未知错误或普通图形模式均拒绝。6项正反例通过；不会笼统忽略所有ERROR。
3. 同步需求TC04/08/12/18/21和任务说明中的旧单向、2次上限、派蒙首次+3和专属遗物池描述，使其与现有0.1.1规则一致。

## 执行与复现

在已配置的PowerShell 7环境、Steam已登录、游戏已退出时，从S的干净检出构建：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet run --project tests/ComboRules/ComboRules.csproj -c Release
.\tests\Test-LogClassification.ps1
.\scripts\Build-Mod.ps1 -DeviceId PC-A
.\scripts\Deploy-Mod.ps1 -PackageDirectory 'artifacts/Sts2Mod-0.1.1-v0.107.1-88cdb3108a20/Sts2Mod'
.\scripts\Start-ModGame.ps1 -Headless -TravelerTest
.\scripts\Test-TravelerRun.ps1 -LogFile '<本次输出日志>' -CodeCommit '88cdb3108a200d8328f3dfaced15e4549cb5b610' -Task003 -Headless
```

保留日志检查后才结束本次返回的进程。PC-B需自己的路径配置和设备标识；重新构建的ZIP应记录自己的哈希，不能假设与PC-A逐字节相同。

## 覆盖边界

通过范围是单人原生动作队列中的确定性机制集成、池注册与模型序列化，并非完整TASK-003完成。

- 0.1.1正常奖励/商店实际获取：NOT_RUN（池注册不是获取）。
- 原生保存、退出、重新启动及完整读档：NOT_RUN（模型序列化不是完整读档）。
- 固定种子自然多场/整局与数值平衡：NOT_RUN；当前脚本使用受控高血量/补能量测试场景。
- 图形呈现、文字布局、鼠标交互及人工自然试玩：NOT_RUN，按DEC-005待另行安排。
- 双人主客机、跨玩家隔离及同步：NOT_RUN，按DEC-004延期专项；不得宣称支持已验收。

本次使用`--headless`及隐藏进程，验证窗口句柄为0；未操作游戏GUI。独立测试局设置`ShouldSave=false`，但游戏原生仍可能更新进度和Steam云同步；这不构成用户存档隔离或完整读档验证。未手工编辑/删除用户存档。测试结束不启动图形窗口。

针对性原始日志保留本机`.local/logs/game-20261020T0116496423638Z.log`；完整原始日志为`.local/logs/game-20261020T0117548735643Z.log`；首次失败日志为`.local/logs/game-20261020T0109327074200Z.log`，均不上传。分享摘录见[evidence/TASK-003-PC-A-011.txt](evidence/TASK-003-PC-A-011.txt)。
