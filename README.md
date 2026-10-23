# 旅行者·元素连携 / Traveler Elements

面向《杀戮尖塔2》的旅行者角色Mod，包含风、雷、岩元素连携、专属卡牌与遗物、中英文文本，以及角色动作、商店和篝火资源。

**开发版本、已发布版本、接续分支、验证范围和下一步只在[当前状态](docs/STATUS.md)维护。** 历史版本见[更新日志](docs/player/CHANGELOG.md)，具体包身份与测试结果从当前状态进入对应报告。

## 文档入口

- [当前状态](docs/STATUS.md)
- [玩家安装与完整玩法](docs/player/README.zh-CN.md)
- [开发操作与发布准备](docs/DEVELOPMENT.md)、[工坊上传与下载验收](workshop/README.md)
- [按改动影响范围验证SOP](docs/TESTING-SOP.md)
- [跨PC接续](docs/handoffs/NEXT-PC-CONTINUE-PROMPT.md)、[协作约定](docs/COLLABORATION.md)
- [需求](STS2-Mod-Requirements-v0.1.md)、[技术基线](docs/TECH_BASELINE.md)、[接口说明](docs/API_FINDINGS.md)、[素材来源](docs/ASSETS.md)

## 首次配置

各PC使用自己的合法游戏安装和local.settings.json；固定工具与依赖版本见[技术基线](docs/TECH_BASELINE.md)。源码同步不包含游戏程序集、个人路径或存档，也不代表本机已完成安装验收。

在PowerShell 7中执行，仅在文件不存在时复制样例并填写本机路径：

```powershell
if (!(Test-Path -LiteralPath .\local.settings.json)) {
    Copy-Item .\local.settings.example.json .\local.settings.json
}
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Test-DevEnvironment.ps1
```

AI游戏测试遵守[AGENTS.md](AGENTS.md)，全程无GUI、使用隔离测试存档并只清理自己的测试进程。工坊3823282901保持仅限好友（friends_only），依赖条目3737335127；仅在用户明确要求时更改可见性。开发后的人工试玩、图形和联机验证均为可选，不阻塞提交、合并、打包或发布；只有用户明确要求时才执行并单独记录。

本项目为非官方同人Mod，与游戏及主题作品权利方无隶属关系。素材归属与分发说明见素材记录。
