# 黏糊格挡数值显示修复 · 2026-10-23

- 范围：用户要求暂时只修复相关代码，不部署、不发布、不启动游戏验证。
- 根因：TravelerDefend与原生DefendIronclad都调用CreatureCmd.GainBlock(BlockVar, CardPlay)，由原生Hook.ModifyBlock应用Goopy.EnchantBlockAdditive（Amount−1）。Goopy自身在该牌打出后增加附魔Amount及DeckVersion附魔Amount；本Mod没有绕过该结算。显示模板却用{Block}读取基础值，所以仍显示5。
- 修改：中英文9张直接格挡牌描述改用原生{Block:diff()}并去掉外层固定blue色，展示含附魔的预览格挡。真实格挡、黏糊累计、消耗规则和存档保持原生行为；不将累计改成全局所有卡牌出牌次数。
- 回归：新增Visuals中的18项检查，覆盖两语言全部格挡模板、普通/升级防御连续打出3次的显示与实得格挡、黏糊增长/消耗及附魔序列化；严格日志检查器同步要求标记。
- 已执行：C#编译0警告0错误，eng/zhs各9个格挡占位符检查通过，检查器PowerShell语法与git diff --check通过。
- NOT_RUN：新增原生运行时用例、游戏/UI检查、完整回归。按用户范围没有部署、打包发布、启动或操作游戏，也没有修改安装和正常存档；后续获准验证时运行Visuals。
