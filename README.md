# 旅行者·元素连携 / Traveler Elements

面向《杀戮尖塔2》的单人Mod，当前版本 **0.3.6**。旅行者拥有18张专属奖励牌（4能力）、10张起始牌、旅途罗盘及派蒙的应援，支持中英文。

源码已合入并推送main，原私有工坊3823282901已更新0.3.6，实际Steam下载与工坊加载验收通过。包身份与实际发布状态见[0.3.6报告](docs/tests/RELEASE-036.md)。分层2D战斗动作与适中强度特效仅在讨论，尚未实现。

## 文档入口

- [当前状态与验证范围](docs/STATUS.md)
- [玩家安装与完整玩法](docs/player/README.zh-CN.md)
- [更新日志](docs/player/CHANGELOG.md)：版本演进的统一入口
- [跨PC接续](docs/handoffs/NEXT-PC-CONTINUE-PROMPT.md)
- [已发布包身份与校验值](docs/tests/RELEASE-036.md)
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

新代码构建与测试见开发说明。复用已试玩的0.3.5包时，按发布报告核对原包与文件哈希；重新构建会产生新的包身份，须单独记录和验收。

## 验证与发布范围

0.3.6干净包与实际工坊各49项原生无GUI专项、普通启动通过，另有49纯规则、26日志分类、15包装、11入口检查通过。此前重构开发包410全量及磁盘结果独立记录，不作为0.3.6干净包全量重跑。多人、旧档迁移与自然Boss系统性平衡未验证。

AI游戏测试遵守[AGENTS.md](AGENTS.md)，使用无GUI、隔离存档并只清理自己的测试进程。工坊保持私有，依赖条目3737335127；公开发布由用户决定。

本项目为非官方同人Mod，与游戏及主题作品权利方无隶属关系。素材归属与分发说明见素材记录。
