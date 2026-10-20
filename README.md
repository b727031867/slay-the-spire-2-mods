# 杀戮尖塔 2 Mod 项目

面向《杀戮尖塔 2》（Slay the Spire 2）的 Mod 开发项目，通过新增卡牌、遗物与组合机制，拓展现有角色的构筑和出牌选择。

首版内容已实现为 Mod 0.1.1：六张铁甲战士主题牌、风↔岩、岩↔雷、雷↔风三类双向连招，以及共享罕见遗物「派蒙的应援」。每人每回合共享 3 次连招，派蒙使每次连招奖励翻倍。TASK-003 单人无GUI集成验收已完成，成果交付main；图形界面、人工自然试玩及双人另行专项测试。

## 项目方向

- 首版优先扩展现有角色的卡牌与遗物，后续再考虑独立角色。
- 以出牌顺序和组合连招为核心，增加攻击、防御与抽牌之间的选择。
- 提供简体中文文本、连招状态提示及安装说明。
- 以单人和双人联机为支持目标，分别进行游戏内验收。

TASK-001已确认「旅行者」风/岩主题、铁甲战士宿主：风起之刃1费6伤害（升级9），岩之守护1费7格挡（升级10）；主动风→岩额外4格挡。TASK-002随后补齐完整6张卡牌、派蒙遗物及风／岩／雷三条连招；数值仍需玩家试玩调整。

完整规则与验收标准见 [需求说明书](STS2-Mod-Requirements-v0.1.md)。

## 当前进度

| 工作项 | 状态 |
| --- | --- |
| 首版需求与验收用例 | 已有设计草案 |
| 当前 PC（PC-A）开发环境 | 已安装并记录版本 |
| 第二台 PC（PC-B）开发环境 | 已完成独立安装、构建与真实游戏验证 |
| 引用本机真实游戏程序集编译 | 已通过，0 警告、0 错误 |
| Godot 无界面资源包导出 | 已通过 |
| 最小 Mod 的真实加载与原生模组列表 | PC-A已通过；干净提交包完成重复部署、移除与恢复 |
| Mod 框架与游戏接口调查 | 固定 BaseLib 3.3.2；风岩基础命令与回合时序已用于真实测试 |
| 卡牌、遗物与连招实现 | 六张牌、三条连招、派蒙遗物、状态及中英文本已完成 |
| 正式玩法的单人及双人联机验收 | 单人机制、正常获取、原生磁盘恢复及持续场景通过；UI/自然人工试玩和双人NOT_RUN |
| 开发包 | 0.1.1 PC-A干净包，具体提交和SHA256见TASK-003交接 |

TASK-001代码和PC-A单人范围已交付，状态为DONE_SINGLEPLAYER：21项纯规则、28项真实游戏断言、手动界面和原生奖励获取通过。当前按用户要求优先单人游戏验证，双人联机后续专项测试，不阻塞本次交付。[草稿PR #2](https://github.com/b727031867/slay-the-spire-2-mods/pull/2)、[交接与下载](docs/handoffs/TASK-001.md)、[测试报告](docs/tests/TASK-001-PC-A-run-01.md)。完整读档及双人尚未验收。当前状态见 [STATUS](docs/STATUS.md)，工具版本见 [TECH_BASELINE](docs/TECH_BASELINE.md)，接口依据见 [API_FINDINGS](docs/API_FINDINGS.md)。

TASK-002 内容开发已完成，TASK-003 状态为DONE_SINGLEPLAYER_HEADLESS：27项纯规则、13项日志分类、12项正常获取/保存准备、255项磁盘恢复/持续场景及94项全量真实游戏断言通过，普通启动不启用测试。连续场景为受控牌组、6场24回合84次出牌，不等于自然整局或平衡通过。原生无GUI退出资源诊断已通过无本Mod对照记录，未知运行错误仍失败。被测S、包SHA256及限制见[报告03](docs/tests/TASK-003-PC-A-run-03.md)、[任务](docs/tasks/TASK-003.md)和[交接](docs/handoffs/TASK-003.md)；历史结果保留，不替代当前证据。

## 技术栈与版本

项目采用 C#、.NET 与 Godot .NET 工具链。

| 组件 | 当前基线 |
| --- | --- |
| 本机核查的游戏版本 | v0.107.1，Steam 构建 23811903 |
| .NET SDK | 10.0.402，由 `global.json` 固定 |
| Mod 目标框架 | net9.0，与本机游戏一致 |
| Godot .NET / Godot.NET.Sdk | 4.5.1 |
| Mod 模板 | Alchyr.Sts2.Templates 2.5.3 |
| 唯一运行时框架 | BaseLib 3.3.2，NuGet精确版本与锁文件固定 |
| 源码查看工具 | ILSpy CLI 11.1.0.9782 |
| 版本管理与编辑器 | Git、VS Code + C# 扩展 |

正式最小工程采用BaseLib 3.3.2，不同时引入RitsuLib；最初的`tools/EnvironmentCheck`仍作为独立工具链检查。其他游戏或基础库版本需要重新核查，尚不在已验证范围内。

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

构建正式最小工程（PC-B将设备参数改为PC-B）：

```powershell
.\scripts\Build-Mod.ps1 -DeviceId PC-A
```

构建要求干净提交，并输出可部署目录、ZIP和SHA256。使用实际输出目录运行`Deploy-Mod.ps1 -PackageDirectory '<目录>/Sts2Mod' -InstallDependencies`，再运行`Start-ModGame.ps1 -Headless`。全量回归加`-TravelerTest`，完成后用`Test-TravelerRun.ps1 -LogFile '<日志>' -CodeCommit '<提交>' -Task003 -Headless`检查。首次原生模组确认、加载日志检查、移除与恢复步骤见 [开发操作说明](docs/DEVELOPMENT.md)。

本地配置与构建产物由 `.gitignore` 排除。PC-A 和 PC-B 均可开发、构建和测试，通过同一个 GitHub 仓库接续工作；开发和测试不绑定某一台设备。两台 Windows 设备均已完成独立工具链核查。

## 两台 PC 的协作与产物同步

- **代码与文档**：通过 Git 提交和推送，另一台 PC 在检查本地改动后同步指定分支和提交。切换设备前更新任务与交接记录；同时开发使用不同任务分支。
- **构建产物**：需要跨机复用的 Mod 包通过 GitHub Releases 附件交接，不直接提交 DLL、PCK 或 ZIP 到源码仓库。未批准公开的包可使用草稿 Release；公开测试包标为预发布，正式发布仍由项目负责人决定。
- **版本追踪**：每份包记录完整源码提交、构建设备、游戏和依赖版本及 SHA256。另一台 PC 下载后校验；自行重新构建时记录该包的新校验值。
- **本地环境**：两台 PC 各自安装工具和游戏、配置 `local.settings.json`，不共享绝对路径、游戏程序集或存档。PC-A 的验证结果不代表 PC-B 已通过。
- **游戏测试**：两台 PC 均可做单人测试；双人联机需两端客户端、账号及操作者条件满足，并交换主客机验收。

目前已有本地构建、打包和检查脚本；当前开发包身份及构建方式见 [TASK-003交接](docs/handoffs/TASK-003.md)。历史草稿附件和实测结果保留在 [TASK-001交接](docs/handoffs/TASK-001.md)及[PC-A首轮报告](docs/tests/TASK-001-PC-A-run-01.md)，历史最小工程记录见 [TASK-000交接](docs/handoffs/TASK-000.md)。尚未启用自动构建上传或公开发布。详细接入与切换步骤见 [双 PC 协作说明](docs/COLLABORATION.md)，调整依据见 [决策记录](docs/DECISIONS.md)。

## 仓库结构

```text
.
├── STS2-Mod-Requirements-v0.1.md  # 需求、规则、验收用例与协作约定
├── docs/
│   ├── STATUS.md                 # 当前进度与下一步
│   ├── TECH_BASELINE.md          # 按设备记录工具版本与验证证据
│   ├── COLLABORATION.md          # 双 PC 开发测试与产物交接
│   ├── DECISIONS.md              # 已确认的项目决策
│   ├── DEVELOPMENT.md            # 构建、部署、启动、移除与恢复
│   └── API_FINDINGS.md           # 原生加载与玩法接口的调查证据
├── scripts/
│   ├── Enter-DevEnvironment.ps1  # 加载本机工具路径
│   ├── Start-DevShell.ps1        # 开发终端入口
│   └── Test-DevEnvironment.ps1   # 编译与资源包导出检查
├── src/Sts2Mod/                 # 玩法、连招状态、显式测试入口与自制SVG素材
├── tests/ComboRules/            # 不依赖游戏的连招规则断言
├── tools/EnvironmentCheck/      # 环境验证工程，非正式 Mod
├── dependencies.json           # 目标游戏及固定框架下载校验
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

本项目为非官方 Mod 开发项目，与游戏及主题作品的权利方无隶属关系。仓库不包含游戏程序集、游戏资源包或主题作品的图片与音频；本任务自制SVG和原生资源引用见 [素材记录](docs/ASSETS.md)。
