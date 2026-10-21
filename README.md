# 旅行者·元素连携 / Traveler Elements

**0.3.13已提交、快进合入main并推送，原工坊3823282901已更新，保留好友可见设置。** 整合19张卡池平衡、无界罗盘、联机选人修复和轻量触发反馈；遗物及元素共鸣改为原图标短闪，连携改为短促小线条。干净包Full531项、实际工坊Load/Character90项/Visuals128项通过，七文件和两张图片一致，详见[发布报告](docs/tests/RELEASE-0313.md)。

**支持单人和联机，已由用户与朋友进行双人联机实测。** 用户反馈除选人页玩家头像和名字重叠外未发现其他异常；该布局问题的修复与验证记录见[联机选人修复](docs/tests/LOBBY-LAYOUT-0310.md)。联机时各玩家使用相同的游戏、Mod及BaseLib版本。

面向《杀戮尖塔2》的角色Mod，旅行者拥有19张专属奖励牌（4能力）、10张起始牌和3种专属遗物（含初始遗物升级），支持中英文。保留七层网格、18关节与脚底约束的角色动作，以及商店立绘、篝火坐姿和精简手牌预览。完整数值见[玩家指南](docs/player/README.zh-CN.md)，版本过程见[更新日志](docs/player/CHANGELOG.md)。

自动验证全程无GUI；用户试玩0.3.12候选与最终干净包测试分别记录。最终图形观感、联机复测、自然Boss系统性平衡和旧档迁移未作本轮系统验收。

## 文档入口

- [当前状态与验证范围](docs/STATUS.md)
- [玩家安装与完整玩法](docs/player/README.zh-CN.md)
- [更新日志](docs/player/CHANGELOG.md)：版本演进的统一入口
- [跨PC接续](docs/handoffs/NEXT-PC-CONTINUE-PROMPT.md)
- [已发布包身份与校验值](docs/tests/RELEASE-0313.md)
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

已发布0.3.13的干净包Full531项、实际工坊Character90项和Visuals128项通过，来源见发布报告。此前用户与朋友的双人联机实测反馈及联机选人修复的自动检查分别记录，不等同于本轮网络联机复测。旧档迁移与自然Boss系统性平衡未作系统验收。

AI游戏测试遵守[AGENTS.md](AGENTS.md)，使用无GUI、隔离存档并只清理自己的测试进程。工坊保留好友可见设置，依赖条目3737335127。

本项目为非官方同人Mod，与游戏及主题作品权利方无隶属关系。素材归属与分发说明见素材记录。
