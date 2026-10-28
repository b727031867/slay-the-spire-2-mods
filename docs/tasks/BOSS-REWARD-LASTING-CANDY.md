# Boss击杀后奖励结算卡住（2026-10-28）

## 日志与根因

bugLog/godot.log:126900及godot2026-10-28T16.58.33.log:383186显示同一异常：LastingCandy.TryModifyCardRewardOptions将筛选后的单稀有度能力牌候选传给CardCreationOptions，仍使用BossEncounter概率，原生构造器拒绝。调用链经过CardFactory.CreateForReward、CardReward.Populate、CombatRoom.OfferRoomEndRewards及NCombatUi.ShowRewards。重载已完成战斗再次生成奖励，重复同一异常。日志证据指向奖励生成，不是1024伤害溢出。

## 改动前影响评估

- 基线：origin/main fa83047；事故包0.3.17。游戏v0.107.1/build23811903，BaseLib3.4.7。
- 修改点：仅替换LastingCandy奖励钩子内部的自定义候选构造调用；单稀有度候选改用Uniform，其余概率、候选筛选、重复候选回退、奖励标记、随机源和遗物触发条件沿用原生代码。
- 调用方：正常战斗奖励、Boss奖励、读取已完成房间后重新生成奖励；修复对使用此遗物的所有角色生效，不改全局CardCreationOptions校验。
- 资源/配置：无运行时资源、数值、存档结构或配置变化。测试加入Character现有夹具，并更新严格日志检查器必需标记。
- 风险：遗物局部兼容补丁依赖当前原生构造调用；若后续游戏改变该方法结构，替换数必须为1，否则明确报错。普通三张Boss奖励的概率不变；追加能力候选只剩一种稀有度时均匀选择。
- 必跑：构建、Character无GUI专项（含加载、奖励、原生胜利界面），新用例覆盖原生异常复现、真实遗物钩子单稀有度候选、混合/空候选、Monster/Elite/Boss与0/1/2/4战计数、奖励序列化后重新生成及重复Populate。
- 不跑：Full（用户未要求）、连携规则（未改）、Events及磁盘两进程专项（未改事件或存储格式）。奖励序列化重建不等于旧用户档或真实联机恢复验证。图形、联机、自然整局NOT_RUN。
- 发布：本次仅交付本地修复候选，不上传工坊。

## 验证

待补本轮结果；首次编译发现新增测试误将CardReward.Cards当作CardCreationResult集合，已按原生CardModel枚举修正。历史沙虫夹具两条nullable警告保留。
