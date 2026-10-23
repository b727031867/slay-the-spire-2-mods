# 切换目标伤害预览修复 · 2026-10-23

用户发现选中旅行者卡牌并切换怪物时，文案仍显示固定伤害；要求修复，图形UI由用户自行验证。

当前结果：用户退出游戏后已部署PC-A本地修复包，Visuals专项191项PASS（新增伤害预览63项），测试进程7804已结束，未知错误0；等待用户自行图形UI验证，未发布工坊。

## 影响评估

- 基线：已发布素材包e717acc，当前源码6831a1a；游戏v0.107.1/build23811903、BaseLib3.4.7，PC-A。
- 根因：原生NCard.SetPreviewTarget→UpdateVisuals→CardModel.UpdateDynamicVarPreview已经更新DamageVar/CalculatedDamageVar.PreviewValue，且运行力量、虚弱、目标易伤及卡牌自定义伤害钩子；但中英文模板用{Damage}/{CalculatedDamage}，DynamicVar.ToString读取BaseValue。原生diff格式才会读取PreviewValue并按增减显示颜色。
- 修改：两种语言的实际伤害占位符统一用{Damage:diff()}/{CalculatedDamage:diff()}，去除其外层固定blue色；不改基础计算、出牌伤害、目标逻辑或Hook。天地交征明确标注的每X基础系数保留，连携奖励规则文案不纳入主攻击数值。
- 调用方/资源：全部普通、群体、多段、计算型伤害卡及起始打击的原生描述渲染；两语言cards.json。测试复用Visuals原生节点及严格检查器，新增切换目标、切回、清空目标、基础/升级及只读检查。
- 验证：构建/导入/导出＋Visuals无GUI专项，复用其中加载和未知错误检查；比对不同怪物状态下原生卡牌节点文字与预览值，两语言模板覆盖；不执行Cards/Full（真实伤害/共享结算未变）、存档专项（无持久化变化）。
- 图形观感、实际鼠标悬停交互由用户验收；不操作用户当前游戏、不自动发布工坊。

## 当前执行结果

- 已修改中英文各12条伤害描述，JSON解析与git diff --check通过；未改真实伤害结算。
- C#构建0警告0错误，Godot无GUI导入/导出通过；开发包为artifacts/Sts2Mod-0.3.17-v0.107.1-6831a1a9f12b-dirty.zip，SHA256 0E655843DE048193CB181517EDC722D5C2EA5FB64CBF5A15CB3ECFC2F459BC51。
- 新增原生NCard.SetPreviewTarget用例已编译，尚未执行。Visuals专项与部署BLOCKED：用户正常游戏PID4548仍在运行；已询问用户退出或暂不部署，未停止用户进程，也未写入正在使用的安装。
- 当前游戏仍使用先前已发布素材版本；本次修复尚未安装或发布。退出游戏后应备份工坊当前安装、部署开发包并运行Visuals，再交由用户图形UI验证。

### 本次构建核心哈希

- Sts2Mod.dll：03842403616F50ED1BFC604E8E7E44D4339D30F34EF5B129CCD26EF2A5B0ACD9
- Sts2Mod.pck：835740FC254665411FA017ABB6948DD8A7C2E0647DE1EECA36D41392694A2B00
- Sts2Mod.json：0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A

## 用户退出后的部署与验证

- 用户明确回复已退出游戏、允许部署；已确认无SlayTheSpire2进程后操作。原工坊BaseLib与Sts2Mod共10文件及SHA256保存在.local/damage-preview/workshop-before与workshop-before-hashes.json。
- 当前游戏mods/BaseLib为3.4.7，mods/Sts2Mod为本次修复开发包；部署后的四个核心文件SHA256与构建输出一致。工坊原条目未更新。
- 实际源码标识6831a1a9f12b2721b9c0a6b9680327a544d43ddc-dirty；Visuals无GUI原生专项191项PASS，其中新增63项：两语言伤害变量格式、10种单体伤害卡×基础/升级×目标易伤及虚弱切换/切回/清空目标，以及预览只读检查。力量、玩家虚弱、目标易伤和计算型卡牌均进入原生动态预览。
- 原生卡牌节点的DescriptionLabel文字与目标对应PreviewValue一致，基础伤害、随机数计数、连携状态、能量、敌人HP不因预览改变。覆盖普通伤害、碎岩重击、三相终击、乘隙雷击和多段雷霆迸发；群体卡的两语言格式一并核对。
- 首次日志校验命令手填的预期源码完整哈希错误，导致身份校验FAIL；游戏实际加载的版本与build-info完全一致。原始FAIL结果保留，按build-info读取正确标识后复验同一完整日志PASS，未重复启动游戏或放宽检查器。
- 日志game-20261023T1309371359770Z.log；有效结构化结果.local/damage-preview/visuals-result.json。加载一次、BaseLib和资源正确、未知错误0、已知headless诊断0。
- 本轮仅启动并清理测试进程7804，隔离目录a2348ee556e64bbebb436bc1d727495d；未停止用户进程、未改正常存档、未打开图形窗口。
- 当前可进入游戏自行UI验证；实际鼠标悬停、图形观感及联机NOT_RUN，不把原生节点文字检查当作图形验收。
