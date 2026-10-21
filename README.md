# 旅行者·元素连携 / Traveler Elements

**0.3.9已合并main并发布到原工坊3823282901，保留好友可见设置。** 原母稿七层网格、18关节与脚底约束保持人物一致性；普通攻击为侧身前突刺，防御为双手举剑挡身前，受击轻微后仰，连续出牌直接衔接攻防。干净包Full474项及实际工坊Load/Character77项/Visuals95项通过，七文件下载一致。来源与范围见[发布报告](docs/tests/RELEASE-039.md)，开发过程见[设计](docs/design/TRAVELER-NATURAL-MOTION.md)和[验证](docs/tests/NATURAL-MOTION-039.md)。

面向《杀戮尖塔2》的单人Mod，版本 **0.3.9**。旅行者拥有18张专属奖励牌（4能力）、10张起始牌、旅途罗盘及派蒙的应援，支持中英文。

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

0.3.7干净包及实际工坊均通过71项角色检查（含篝火12项）和82项战斗演出专项，实际工坊普通启动与7文件下载校验通过。用户试玩的开发包及此前443项全量分别记录，不冒称本包全量重跑。多人、旧档迁移与自然Boss系统性平衡未验证。

AI游戏测试遵守[AGENTS.md](AGENTS.md)，使用无GUI、隔离存档并只清理自己的测试进程。工坊保持私有，依赖条目3737335127；公开发布由用户决定。

本项目为非官方同人Mod，与游戏及主题作品权利方无隶属关系。素材归属与分发说明见素材记录。
