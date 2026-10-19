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

## TASK-001 实现依据

- BaseLib `CustomCardModel`结合`[Pool(typeof(IroncladCardPool))]`自动调用原生池扩展，`[CustomID]`固定为TC_C01/TC_C02。构造函数不同时手工重复加入池；测试检查原生解锁池中每种仅1条。
- 两张牌在自己的`OnPlay`中先预占连招，再执行原生基础命令和奖励。`GameActionPlayerChoiceContext.Action`保留动作对象，暂停恢复只改变其数字Id；按动作引用去重，记录当前引擎Id。不持久化对象标识，不增加网络消息。实际暂停/恢复出牌与双人网络幂等仍待实测。
- `ComboLifecycle`是无状态`SingletonModel`订阅者；每个玩家的`ComboPower`可变实例通过`InitInternalData`获得独立`ComboState`。回合开始Early钩子重置，玩家阵营回合结束、死亡与战斗结束清序列。显示用`DisplayAmount`表示剩余次数，原生Amount保留正数并编码元素/已用次数，避免零层被移除且让原生校验覆盖可见规则状态。
- 基础伤害使用`DamageCmd.Attack(...).FromCard(this)`；基础格挡使用`BlockVar(..., ValueProp.Move)`。连招奖励用`CreatureCmd.GainBlock(...,4,ValueProp.Unpowered,cardPlay)`，作为额外固定格挡，不伪造新卡牌动作。状态图标闪烁及原生思考气泡仅负责显示。
- 原生`BurstPower`通过`ModifyCardPlayCount`产生额外结算；实际游戏验证`CardPlay.PlayIndex=1`不会推进序列。`CardCmd.AutoPlay`的原生自动岩牌保留之前的风，后续手动岩牌仍能触发。
- 原生本地化入口读取PCK内`res://Sts2Mod/localization/<eng|zhs>/<cards|powers>.json`。这些JSON不能散放在mods目录；导出排除项只排除根清单和包锁，保留本地化资源。自制SVG的Godot导入设置随源代码提交。
- 显式启动参数控制测试Node/只读奖励观察者，默认不创建。`tc_hand`仅允许显式开启的单人铁甲可出牌阶段；无自定义网络动作/消息类型。
- 保存边界：v0.107.1原生`RunManager.ToSave`序列化永久牌组、玩家与地图；`CombatRoom.ToSerializable`保存遭遇/完成标记/奖励，不序列化正在进行的手牌、Power或动作队列。普通存档不能作为任意战斗中间点快照；不声称支持中途连招恢复。卡牌ID与升级序列化往返可独立测试，完整退出/读档仍需实机用例TC-19，不能用模型往返替代。

## 验证等级

TASK-000验证最小加载、DLL/PCK读取及BaseLib版本检查。TASK-001已经在真实游戏使用基础命令、回合边界、正常奖励与状态提示；精确被测提交、结果及未测项以TASK-001报告为准。完整读档、双人、主客机交换和断线恢复不能由单人结果推断。

## TASK-002/003 PC-B 实测补充

- `CardPileCmd.Draw` 已用于雷→风，沿用当前 `PlayerChoiceContext`；基础抽牌先于连招抽牌。Burst 对整备牌的第二次基础结算不再推进连招。
- `AfterPlayerTurnStart` 在原生额外回合中触发，实测会重置同一玩家的连招次数、上一元素和派蒙本回合奖励状态。
- `CombatManager.Instance.IsEnding` 在基础伤害击杀最后敌人的紧邻结算点可能仍为 false，不能单独用于判断是否发战斗内后续奖励。实现同时检查敌方是否仍有存活成员；91项回归覆盖最后击杀和“原目标死亡但战斗继续”两种边界。
- `CustomRelicModel` 与 `IroncladRelicPool` 已用于派蒙；重复测试遗物由共享的每玩家 `ComboState` 预占一次奖励，因此不会叠加。
- 卡牌与遗物的 `ToSerializable`/`FromSerializable` 模型往返已通过，但原生保存不包含任意战斗中动作队列/Power，因此仍不能等价为完整退出读档验收。

## 公开参考

- [BaseLib 3.3.2官方发行](https://github.com/Alchyr/BaseLib-StS2/releases/tag/v3.3.2)。
- [BaseLib 3.3.2项目配置](https://github.com/Alchyr/BaseLib-StS2/blob/v3.3.2/BaseLib.csproj)。
- [模板作者安装与构建指南](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)。
- [Godot资源包说明](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_pcks.html)。
- [Steamworks API初始化说明](https://partner.steamgames.com/doc/sdk/api)。
