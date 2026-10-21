# 旅行者·元素连携 / Traveler Elements

0.3.13整合19张卡池平衡、无界罗盘、联机选人修复和轻量触发反馈；元素共鸣基础/升级均改为状态图标短闪。用户已试玩0.3.12并授权整合发布，实际交付结果见[当前状态](docs/STATUS.md)。

本地0.3.12候选将遗物触发收敛为顶部图标短闪，元素连携改为短促小线条，去除放大图片和文字气泡；同位置连续触发合并显示，不改变玩法。当前说明见[玩家指南](docs/player/README.zh-CN.md)，设计范围见[轻量特效](docs/design/COMPACT-TRIGGERS-2026-10-22.md)。尚未发布工坊。

本地0.3.11平衡候选已加入雷霆连锁并调整11项卡牌/遗物规则，奖励池为19张；详见[本轮变更](docs/design/BALANCE-CHANGES-2026-10-22.md)。本轮尚未发布，旧版本验证结果如下。

本地0.3.10开发候选已补齐欧洛巴斯的初始遗物升级：旅途罗盘可替换为无界罗盘，每次实际连携奖励额外抽1，无回合触发次数限制或冷却；连携自身每回合3次上限不变。加入联机布局修复后的最终包角色专项106项通过，尚未发布工坊，见[遗物开发记录](docs/tests/COMPASS-UPGRADE-0310.md)与[最终综合验证](docs/tests/LOBBY-LAYOUT-0310.md)。

**支持单人和联机，已由用户与朋友进行双人联机实测。** 用户反馈除选人页玩家头像和名字重叠外未发现其他异常；该布局问题的修复与验证记录见[联机选人修复](docs/tests/LOBBY-LAYOUT-0310.md)。联机时各玩家使用相同的游戏、Mod及BaseLib版本。

**0.3.9已合并main并发布到原工坊3823282901，保留好友可见设置。** 原母稿七层网格、18关节与脚底约束保持人物一致性；普通攻击为侧身前突刺，防御为双手举剑挡身前，受击轻微后仰，连续出牌直接衔接攻防。干净包Full474项及实际工坊Load/Character77项/Visuals95项通过，七文件下载一致。来源与范围见[发布报告](docs/tests/RELEASE-039.md)，开发过程见[设计](docs/design/TRAVELER-NATURAL-MOTION.md)和[验证](docs/tests/NATURAL-MOTION-039.md)。

面向《杀戮尖塔2》的角色Mod，支持单人和联机，已发布版本 **0.3.9**。旅行者拥有18张专属奖励牌（4能力）、10张起始牌、旅途罗盘及派蒙的应援，支持中英文。

保留0.3.8对选牌长说明遮挡和商店旅行者形象的修复，不改变卡牌与连携结算。游戏自动验证全程无GUI；用户已评审动作草图与本地回放，游戏光照、特效叠加及UI下的画面仍需独立试玩。

## 文档入口

- [当前状态与验证范围](docs/STATUS.md)
- [玩家安装与完整玩法](docs/player/README.zh-CN.md)
- [更新日志](docs/player/CHANGELOG.md)：版本演进的统一入口
- [跨PC接续](docs/handoffs/NEXT-PC-CONTINUE-PROMPT.md)
- [已发布包身份与校验值](docs/tests/RELEASE-039.md)
- [当前需求](STS2-Mod-Requirements-v0.1.md)、[开发操作](docs/DEVELOPMENT.md)、[协作约定](docs/COLLABORATION.md)
- [技术基线](docs/TECH_BASELINE.md)、[接口说明](docs/API_FINDINGS.md)、[素材来源](docs/ASSETS.md)

## 开发环境

| 组件 | 固定基线 |
| --- | --- |
| 平台 | Windows x64 |
| 游戏 | v0.107.1 / Steam build23811903 |
| BaseLib | 精确3.4.7 |
| .NET SDK / 目标框架 | 10.0.402 / net9.0 |
| 开发工具 .NET 9运行时 | 9.0.21 |
| Godot .NET / Godot.NET.Sdk | 4.5.1 |
| 终端 | PowerShell 7 |

各PC使用自己的合法游戏安装和local.settings.json。源码同步不包含游戏程序集、安装包、个人路径或存档，也不代表本机已完成安装验收。

首次配置且文件不存在时复制样例，填写本机路径后在PowerShell 7运行：

```powershell
if (!(Test-Path -LiteralPath .\local.settings.json)) {
    Copy-Item .\local.settings.example.json .\local.settings.json
}
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Test-DevEnvironment.ps1
```

新代码构建与测试见开发说明。复用已校验发布包时，按发布报告核对原包与文件哈希；重新构建会产生新的包身份，须单独记录和验收。

## 验证与发布范围

已发布0.3.9的干净包Full474项、实际工坊Character77项和Visuals95项通过，来源见发布报告。2026-10-22新增用户与朋友的双人联机实测反馈，未报告其他联机异常；联机选人布局修复的自动检查和用户联机实测分别记录。旧档迁移与自然Boss系统性平衡未作系统验收。

AI游戏测试遵守[AGENTS.md](AGENTS.md)，使用无GUI、隔离存档并只清理自己的测试进程。工坊保留好友可见设置，依赖条目3737335127。

本项目为非官方同人Mod，与游戏及主题作品权利方无隶属关系。素材归属与分发说明见素材记录。
