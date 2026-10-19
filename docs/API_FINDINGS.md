# v0.107.1 接口调查

设备：PC-A；游戏提交59260271，Steam构建23811903。本表来自对本机合法安装程序集的静态检查，原始反编译结果仅保存在被Git忽略的`.local/investigation/`。仓库只保留接口签名和结论，不公开游戏源码或程序集。

## 加载与依赖

- 原生`ModManager.Initialize(IModManagerFileIo, ModSettings?, SemanticVersion?)`扫描可执行文件旁的mods目录；每个JSON均作为清单解析，随后按依赖拓扑顺序加载DLL、PCK并调用标注`ModInitializerAttribute`的静态方法。
- `ModManifest`具备`id/name/author/description/version/has_pck/has_dll/affects_gameplay/min_game_version/dependencies`；`ModDependency`仅有`id`和`min_version`，没有最大版本字段。本工程通过包锁、部署检查和入口版本检查固定BaseLib 3.3.2，不能声称原生清单提供精确版本拦截。
- 原生首次加载要求用户同意；不同意时记录Skipped/Disabled并跳过，需要允许后重启。`nomods`参数可完全跳过加载。
- 选择BaseLib 3.3.2作为唯一框架，来自2026-06-21官方发行，匹配本机版本阶段并已完成实际加载验证。RitsuLib也提供兼容包，但本任务不引入第二个框架；不把最新主分支包直接用于旧游戏。
- 最小项目直接使用原生入口，BaseLib只作固定的框架依赖与后续扩展基础。此次没有Harmony补丁和自定义网络消息。

## 下一阶段可使用的接口

| 需求 | 已核实接口 | 结论与边界 |
|---|---|---|
| 卡牌/遗物加入池 | `ModHelper.AddModelToPool<TPoolType,TModelType>()`；`IroncladCardPool`、`IroncladRelicPool` | 原生提供池扩展入口；正式卡牌与遗物仍须验证解锁、奖励与商店的实际获取，不以注册成功替代FR-02/05 |
| 主动合法出牌 | `PlayCardAction`验证手牌、CanPlay和目标，支付资源后调用`CardModel.OnPlayWrapper(..., isAutoPlay:false, ...)` | 取消/非法目标在进入出牌流程前退出；实际钩子边界将在TASK-001验证 |
| 自动与复制 | `CardPlay.IsAutoPlay`、`PlayIndex`、`PlayCount`、`IsFirstInSeries` | 原生上下文区分自动和Replay系列，不按卡名推断 |
| 结算前后钩子 | `AbstractModel.BeforeCardPlayed(CardPlay)`、`AfterCardPlayed(PlayerChoiceContext,CardPlay)` | 本机代码顺序是Before→基础OnPlay→战斗仍继续时After，可用于预占与奖励分阶段；实际实现需测重入和战斗结束 |
| 动作ID | `GameAction.Id:uint?`、`OwnerId:ulong`；`GameAction.OnEnqueued(...,uint)` | ID由队列分配；`ResumeAfterGatheringPlayerChoice(uint newId)`会重新赋ID，不能把恢复后的ID当成另一张牌。必须在TASK-001确定并实测稳定的逻辑动作关联方式 |
| 回合/战斗边界 | `AfterSideTurnStart(...)`、`AfterPlayerTurnStart(PlayerChoiceContext,Player)`、`BeforeSideTurnEnd(...)`、`AfterCombatEnd(CombatRoom)` | 原生存在玩家/阵营两类时序；额外回合与双方同时行动需实际验证，不能假定玩家轮流行动 |
| 攻击伤害 | `DamageCmd.Attack(decimal)`返回AttackCommand | 使用原生攻击构建链，不伪造第二次打牌；具体伤害修正与回调待TC-24验证 |
| 格挡 | `CreatureCmd.GainBlock(Creature,decimal,ValueProp,CardPlay?,bool)` | 原生结算接口，具体选择的ValueProp与原生牌保持一致并测试 |
| 抽牌 | `CardPileCmd.Draw(PlayerChoiceContext,decimal,Player,bool)` | 沿用原生选择上下文和队列 |
| 多人同步 | `ActionQueueSynchronizer`处理RequestEnqueueAction、ActionEnqueued与恢复消息；按确定顺序同步动作 | 存在原生命令/动作链；本任务仅静态调查，不宣称双人实测通过，不额外广播一次奖励 |

## 验证等级

真实游戏已验证最小加载、DLL/PCK读取及BaseLib版本检查。上表的玩法接口为静态证据，尚未用于正式卡牌实现；存档恢复、正常获取、伤害回调、动作幂等及双人同步均需后续游戏用例。

## 公开参考

- [BaseLib 3.3.2官方发行](https://github.com/Alchyr/BaseLib-StS2/releases/tag/v3.3.2)。
- [BaseLib 3.3.2项目配置](https://github.com/Alchyr/BaseLib-StS2/blob/v3.3.2/BaseLib.csproj)。
- [模板作者安装与构建指南](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)。
- [Godot资源包说明](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_pcks.html)。
- [Steamworks API初始化说明](https://partner.steamgames.com/doc/sdk/api)。
