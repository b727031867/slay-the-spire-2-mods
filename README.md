# 杀戮尖塔 2 Mod 项目

面向《杀戮尖塔 2》（Slay the Spire 2）的 Mod 开发项目，通过新增卡牌、遗物与组合机制，拓展现有角色的构筑和出牌选择。

项目目前处于技术调查与开发准备阶段。仓库已包含需求说明、开发环境脚本和可编译的环境验证工程，尚未实现正式卡牌内容，也没有可供玩家安装的 Mod 发布包。

## 项目方向

- 首版优先扩展现有角色的卡牌与遗物，后续再考虑独立角色。
- 以出牌顺序和组合连招为核心，增加攻击、防御与抽牌之间的选择。
- 提供简体中文文本、连招状态提示及安装说明。
- 以单人和双人联机为支持目标，分别进行游戏内验收。

当前需求草案推荐以「旅行者」为卡牌主题、「派蒙」为遗物主题，暂以铁甲战士为宿主，规划 6 张卡牌、1 件遗物与风／岩／雷三条有方向的连招。主题、宿主与初始数值仍待确认和试玩调整，不能视为已完成的游戏内容。

完整规则与验收标准见 [需求说明书](STS2-Mod-Requirements-v0.1.md)。

## 当前进度

| 工作项 | 状态 |
| --- | --- |
| 首版需求与验收用例 | 已有设计草案 |
| Windows 开发环境 | 已安装并记录版本 |
| 引用本机真实游戏程序集编译 | 已通过，0 警告、0 错误 |
| Godot 无界面资源包导出 | 已通过 |
| Mod 框架兼容版本与游戏接口调查 | 待完成 |
| 卡牌、遗物与连招实现 | 待开发 |
| 游戏加载、单人及双人联机验收 | 未执行 |
| 玩家发布包 | 尚未提供 |

环境验证通过只说明工具链能够编译和打包，不代表 Mod 已通过游戏兼容性或联机测试。当前状态见 [STATUS](docs/STATUS.md)，本机核查证据见 [TECH_BASELINE](docs/TECH_BASELINE.md)。

## 技术栈与版本

项目采用 C#、.NET 与 Godot .NET 工具链。

| 组件 | 当前基线 |
| --- | --- |
| 本机核查的游戏版本 | v0.107.1，Steam 构建 23811903 |
| .NET SDK | 10.0.402，由 `global.json` 固定 |
| Mod 目标框架 | net9.0，与本机游戏一致 |
| Godot .NET / Godot.NET.Sdk | 4.5.1 |
| Mod 模板 | Alchyr.Sts2.Templates 2.5.3 |
| 源码查看工具 | ILSpy CLI 11.1.0.9782 |
| 版本管理与编辑器 | Git、VS Code + C# 扩展 |

BaseLib 与 RitsuLib 为候选框架；正式工程会按目标游戏分支选定一个兼容方案并固定版本。当前环境验证工程不依赖二者。其他游戏版本需要重新核查，尚不在已验证范围内。

## Windows 开发入门

需要已安装的《杀戮尖塔 2》、PowerShell 7，以及上表版本的 .NET SDK 和 Godot .NET 编辑器。游戏程序集由开发者在自己的电脑上提供，仓库不附带这些文件。

1. 克隆仓库并进入目录：

   ```powershell
   git clone https://github.com/b727031867/slay-the-spire-2-mods.git
   Set-Location slay-the-spire-2-mods
   ```

2. 复制本地配置样例，填写实际工具路径和游戏安装路径：

   ```powershell
   Copy-Item .\local.settings.example.json .\local.settings.json
   ```

3. 在 PowerShell 7 中加载环境并运行检查：

   ```powershell
   . .\scripts\Enter-DevEnvironment.ps1
   dotnet --version
   .\scripts\Test-DevEnvironment.ps1
   ```

检查脚本会编译 `tools/EnvironmentCheck`、执行 Godot 无界面导入，并将资源包输出到 `artifacts/environment-check/`，不会自动安装到游戏。

本地配置与构建产物由 `.gitignore` 排除。后续 Debian 无界面开发与 Windows 游戏实测通过 Git 交接；Debian 端安装与构建目前尚未验证。

## 仓库结构

```text
.
├── STS2-Mod-Requirements-v0.1.md  # 需求、规则、验收用例与协作约定
├── docs/
│   ├── STATUS.md                 # 当前进度与下一步
│   └── TECH_BASELINE.md          # 工具版本、环境证据与限制
├── scripts/
│   ├── Enter-DevEnvironment.ps1  # 加载本机工具路径
│   ├── Start-DevShell.ps1        # 开发终端入口
│   └── Test-DevEnvironment.ps1   # 编译与资源包导出检查
├── tools/EnvironmentCheck/      # 环境验证工程，非正式 Mod
├── global.json                 # 固定 .NET SDK 版本
└── local.settings.example.json  # 本地路径配置样例
```

## 开发路线

1. **技术调查**：锁定游戏版本、Mod 框架和接口，确认构建与加载路径。
2. **垂直验证**：用风／岩两张牌验证单条连招、状态显示与双人同步。
3. **首版内容**：补齐 6 张卡牌、升级效果、3 条连招和派蒙遗物。
4. **集成验收**：覆盖正常获取、边界规则、存档和双人主客机测试。
5. **发布准备**：生成可追溯安装包、校验值、玩家说明与素材归属清单。

报告问题时，请附游戏构建号、Mod 与依赖版本、复现步骤和必要日志。未实际执行的游戏用例应标记为未测试。

## 项目与素材说明

本项目为非官方 Mod 开发项目，与游戏及候选主题作品的权利方无隶属关系。仓库不包含游戏程序集、游戏资源包或候选作品的图片与音频；新增素材的来源与可分发依据将在发布前逐项记录。
