# 旅行者·元素连携 / Traveler Elements

本地开发候选0.3.9采用原母稿七层网格、18关节连接和脚底约束；已按评审草图实现侧身前突刺、双手举剑防御、侧身后仰受击及攻防连续切换，并提供原速/慢放/逐帧对照。见[设计](docs/design/TRAVELER-NATURAL-MOTION.md)和[验证](docs/tests/NATURAL-MOTION-039.md)。下文为已发布的0.3.8。

面向《杀戮尖塔2》的单人Mod，版本 **0.3.8**。旅行者拥有18张专属奖励牌（4能力）、10张起始牌、旅途罗盘及派蒙的应援，支持中英文。

0.3.8修复选牌连携长说明遮挡、分层角色头身与腿部比例、商店角色仍为铁甲战士的问题。干净包完整回归466项通过，源码已快进合入main并推送，原工坊3823282901已更新，保留实际的好友可见设置。包身份、下载核对和工坊加载结果见[发布报告](docs/tests/RELEASE-038.md)，实现与早期失败记录见[修复报告](docs/tests/PRESENTATION-FIXES-038.md)。

## 文档入口

- [当前状态与验证范围](docs/STATUS.md)
- [玩家安装与完整玩法](docs/player/README.zh-CN.md)
- [更新日志](docs/player/CHANGELOG.md)：版本演进的统一入口
- [跨PC接续](docs/handoffs/NEXT-PC-CONTINUE-PROMPT.md)
- [已发布包身份与校验值](docs/tests/RELEASE-038.md)
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
