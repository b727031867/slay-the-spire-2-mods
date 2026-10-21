# 当前接口说明 / v0.107.1 / BaseLib3.4.7

0.3.8商店：CustomMerchantAnimPath注册独立merchant.tscn，原生NMerchantRoom从预加载缓存实例化NMerchantCharacter。该自动转换实例未被BaseLib3.4.7的CreatedFromFactory护栏识别，原生_Ready仍尝试将首个Node2D绑定为SpineSprite，实测抛异常。TravelerMerchantVisualPatch仅识别本Mod唯一命名的TravelerMerchantArt和body.png，跳过其原生Spine初始化/动画调用；铁甲战士及其他角色继续原流程。Character专项实际进入商店并检查资源、比例、原生控件和释放，详见PRESENTATION-FIXES-038报告。

0.3.8悬停：ElementCardModel仅为战斗手牌提供单一TC_PREVIEW；非手牌不追加自定义提示，所有卡牌移除TC_RULES长段。原生关键字说明仍由游戏提供，完整规则仍在玩家指南。

能量提示：BaseLib3.4.7直接把TextEnergyIconPath写成无尺寸[img]，原生EnergyIconsFormatter按图片本身尺寸排版，HoverTipFactory.ForEnergy按本机角色池取大图。三个池使用EnergyColorName=ironclad，沿用原生分别用于能量提示和行内文字的资源，与现有原生能量计一致；不修改全局渲染器、不分发游戏贴图、不改罗盘自身遗物图片或效果。专项覆盖原生烛台获取后/未获取说明、提灯、能量药水、祭品卡牌说明，以及真实NHoverTipSet节点尺寸。

界面实现：删除NCombatUi._Ready的自定义面板挂载与ComboHud/ComboPips类；仅保留NPower.OnDisplayAmountChanged的本Mod元素纹理刷新。原生HoverTip仍来自ElementCardModel.ExtraHoverTips的TC_PREVIEW条目，未改预览公式/文案/规则。专项验收检查整个原生战斗UI没有TravelerComboHud节点，并直接验证原生HoverTip.Description及状态栏%Icon。

## 原生状态与预览

- NPower.OnDisplayAmountChanged只刷新数值，不重读Icon；针对ComboPower刷新%Icon.Texture，不影响其他Power。
- ElementCardModel.ExtraHoverTips提供TC_PREVIEW原生悬停提示，使用只读VisualPreview，不消耗次数或提前读取随机数。
- 不使用自定义面板挂载、ComboHud/ComboPips或HoveredModelTracker驱动面板。
- 隔离测试档使用明确夹具默认值，不触碰正常档案或模组/平台同意设置。

## 原生遗物和角色图标

当前精确BaseLib3.4.7，游戏仍v0.107.1/build23811903。只记录接口结论，不提交原生反编译源码。

- 左上角`NTopBarPortrait.Initialize(Player)`实际装入`player.Character.Icon`。仅修改CustomIconTexturePath不能证明该入口换图；当前`character/icon.tscn`的TextureRect资源为head_icon.png，并实例化原生Character.Icon检查资源路径。
- 原生遗物支持ShowCounter、DisplayAmount、InvokeDisplayAmountChanged以及RelicStatus.Active/Disabled/Normal。罗盘在可用时1/Active，已用或冷却时0/Disabled，战斗外0/Normal；用原生事件同步，不新增覆盖原生UI的计时节点。动态Title使用本地化LocString参数。
- 另核对NRelicInventoryHolder：运行中ShowCounter为true就显示数值（包括0），DisplayAmountChanged刷新文本；Active/Normal图标白色，Disabled为#808080变暗。“亮”指原生正常亮度对比冷却暗色，不承诺新增持续发光特效。此为接口静态核查，非人工UI验收。
- 自己的AfterPlayerTurnStartEarly计入额外回合；BeforeCombatStart及AfterCombatEnd清理战斗内冷却。战斗入口原生保存恢复重新开始战斗，不声称支持任意动作中途保存当前冷却。

## 加载、动作与原生保存

- ModManager扫描清单并按依赖顺序加载DLL/PCK，ModInitializerAttribute入口初始化。清单仅支持最低依赖字段，精确3.4.7由项目锁文件和入口检查保证。
- CustomCardModel与Pool属性接入角色池，固定模型ID，不要手工重复注册。
- PlayCardAction使用原生支付、目标校验和动作；按动作对象关联暂停/恢复，数字动作Id可能变化。IsAutoPlay和PlayIndex区分自动/额外结算。
- 玩家ComboPower持有独立ComboState，主动元素牌预占配对后执行原生命令；额外回合恢复额度，元素保留至战斗结束。
- 攻击使用DamageCmd.Attack，格挡使用CreatureCmd.GainBlock，抽牌使用CardPileCmd.Draw。基础击杀后IsEnding可能未及时更新，须同时确认有存活敌人。
- 防御技能追击使用原生CombatTargets随机源，预览不得提前取随机数；单体攻击保留原目标，死目标不转火。
- RunManager.ToSave及CombatRoom.ToSerializable保存永久牌组、玩家、地图和遭遇入口，不序列化任意动作中的手牌/Power/队列。读档重新进入战斗，不是中途快照。
- 本地化随PCK位于res://Sts2Mod/localization/<eng|zhs>/，不散放mods目录被当作清单扫描。
- 默认不创建测试Node或开启调试入口；显式测试仅用于隔离单人场景，多人动作链未专项实测。

当前包实测范围见[发布报告](tests/TASK-011-PC-B-release-035.md)，版本过程统一见[更新日志](player/CHANGELOG.md)。
