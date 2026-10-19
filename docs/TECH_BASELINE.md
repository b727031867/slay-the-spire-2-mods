# Windows 开发环境基线

核查日期：2026-10-19。范围：按需求书第 8 节安装当前 PC（PC-A）的 Windows 开发与测试工具。需求原文仍为根目录 `STS2-Mod-Requirements-v0.1.md`，本次协作方式调整没有改动玩法设计。

协作基线：PC-A 和后续接入的 PC-B 均可开发、构建与测试，通过 GitHub 同步代码、文档和构建产物。操作约定见 [COLLABORATION](COLLABORATION.md)，本次决定见 [DECISIONS](DECISIONS.md)。以下版本与 PASS 记录均只属于 PC-A，不代表 PC-B 已安装或验证。

## 设备登记

| 设备 | 用途 | 环境与验证状态 |
|---|---|---|
| PC-A | 当前电脑，开发、构建、单人测试及联机测试 | 环境、程序集编译、PCK导出及最小Mod图形界面加载通过；正式玩法与联机未测试 |
| PC-B | 后续另一台电脑，同样开发、构建与测试 | 尚未接入；实际系统、工具、游戏版本、本地引用、构建和游戏测试均待核查 |

PC-B 先按同一 Windows 工具链准备，接入后再记录实测版本。两台 PC 使用各自的本地配置和合法游戏安装；涉及联机时统一游戏分支、构建与 Mod 及依赖版本。

## 已核实的游戏

| 项目 | 本机证据 |
|---|---|
| Steam App ID | 2868840 |
| 游戏版本 | v0.107.1 |
| 游戏发布分支字段 | release_info.json：v0.107.1；Steam manifest未设置BetaKey；实际启动日志报告Platform branch为public |
| Steam buildid | 23811903 |
| 游戏提交 | 59260271 |
| 游戏程序集 | data_sts2_windows_x86_64/sts2.dll，文件版本 0.1.0.0（该值不等于游戏发行版本） |
| 游戏目标框架 | sts2.runtimeconfig.json：net9.0，内置运行时 9.0.7 |
| GodotSharp | 文件版本 4.5.1.0 |
| Harmony | 文件版本 2.4.2.0 |

绝对安装路径仅保存在被 Git 忽略的 `local.settings.json`。游戏引用直接读取本机文件，没有复制进仓库、发布包或上传。

## 工具版本与安装方式

| 工具 | 版本 | 状态 |
|---|---|---|
| Windows | Windows 11 专业版，10.0.26200，x64 | 已有 |
| PowerShell | 7.6.5 | 已有；所有本次 shell 命令显式使用用户安装的 PowerShell 7 |
| Git | 2.45.0.windows.1 | 已有 |
| VS Code | 1.134.0 | 已有 |
| .NET SDK | 10.0.402 | 已安装；global.json 精确锁定 |
| .NET 9 运行时 | 9.0.21 | 已安装，供 net9 工具使用 |
| SDK 自带 .NET 10 运行时 | 10.0.13 | 已安装 |
| Godot .NET | 4.5.1.stable.mono.official.f62fdbde1 | 已安装，匹配本机 GodotSharp 4.5.1 |
| Godot.NET.Sdk | 4.5.1 | 实际还原并编译通过；packages.lock.json 记录 GodotSharp/SourceGenerators 4.5.1 |
| Alchyr.Sts2.Templates | 2.5.3 | 已安装，包含空 Mod、内容 Mod、角色 Mod 三种模板 |
| BaseLib | 3.3.2 | 唯一运行时框架；官方发行包校验通过，真实游戏初始化通过 |
| ILSpy CLI | 11.1.0.9782 | 已安装并成功启动 |
| VS Code C# 扩展 | ms-dotnettools.csharp 2.160.4 | 已安装 |
| 扩展依赖 | ms-dotnettools.vscode-dotnet-runtime 3.2.0 | 自动安装 |

采用较新的 SDK 编译 net9.0，运行目标仍是本机游戏使用的 .NET 9。没有把项目目标框架改为 net10.0。

便携工具位于当前用户的 `%LOCALAPPDATA%/STS2Modding/`，无需管理员安装。该目录内保存 `Install-Toolchain.ps1`、下载包、安装清单 `installed-manifest.json` 和修改前用户环境变量 `environment-before.json`。

SDK 与 .NET 9 运行时按微软官方发布元数据验证 SHA512；Godot 按官方 GitHub release asset digest 验证 SHA256。安装包验证后才解压。VS Code 工作区终端已配置 PowerShell 7 和本地 SDK 路径；机器专用设置被 Git 忽略。

用户环境已登记 DOTNET_ROOT、DOTNET_ROOT_X64 与工具路径。系统 PATH 内还有原先的 .NET 6 主机，Windows 可能优先命中它。项目内应使用下方环境入口，确保 `dotnet` 指向本次 SDK；不依赖重新登录后的 PATH 排序。

## 使用和复查

在项目根目录的 PowerShell 7 执行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet --version
ilspycmd --version
.\scripts\Test-DevEnvironment.ps1
```

或者用 PowerShell 7 的 `-NoExit -File scripts/Start-DevShell.ps1` 打开专用终端。VS Code 重新打开本项目后，新建集成终端也会使用本地 SDK。Godot 可用 `& $env:STS2_GODOT_EXE --editor --path .\tools\EnvironmentCheck` 打开环境验证工程。

PC-B 接入时：克隆同一 GitHub 仓库，按本表固定版本安装工具，复制 `local.settings.example.json` 为该机的 `local.settings.json` 并填入实际路径；从该机合法游戏安装读取引用，然后运行上述检查。PC-A 上的工具安装目录和下载缓存不会随 Git 自动同步，当前也没有通用的一键环境安装脚本；逐项检查步骤见 COLLABORATION。PC-B 的结果须单独写入本表和测试报告。

## 验证事实

| 检查 | 结果 |
|---|---|
| SDK 版本、Godot 版本 | PASS |
| 固定版本模板安装 | PASS |
| ILSpy 启动 | PASS，11.1.0.9782 |
| 引用真实 sts2.dll、0Harmony.dll 和 GodotSharp 编译 EnvironmentCheck | PASS，Release，退出码 0，0 警告、0 错误 |
| Godot 无 UI 导入验证工程 | PASS，退出码 0 |
| Godot 无 UI 导出资源包 | PASS，退出码 0 |
| 导出结果 | artifacts/environment-check/EnvironmentCheck.pck，840 字节 |
| 资源包 SHA256 | 1885468FED231FC669B517A6ECEECF1316DB2A827E453643E0691B96B6B21318 |
| src/Sts2Mod的DLL/PCK和BaseLib初始化 | PASS，干净提交af2bd55139fa；每次READY一次，进入图形主菜单，无启动ERROR |
| 连续两次干净构建 | PASS，ExportRelease/win-x64，DLL与PCK哈希分别一致 |
| 重复部署、重启、移除、恢复 | PASS，移除后不加载本项目，BaseLib保留，备份恢复同一提交 |
| 原生模组列表可见性 | PASS，游戏设置→Modding列出BaseLib与STS2 Mod - Technical Preview |
| 正常卡池、连招UI、玩法与存档 | NOT_RUN，尚未实现相关内容 |
| 双人、主客机同步 | NOT_RUN |
| PC-B 环境、编译、资源包导出与游戏测试 | NOT_RUN，设备尚未接入 |

原先tools/EnvironmentCheck只证明工具链可用；后续src/Sts2Mod已建立真实可加载入口。两者均没有卡牌或连招实现。无需额外安装Godot导出模板即可完成当前PCK导出；完整可执行游戏导出不在本次范围内。正式包身份与精确结果见 [TASK-000报告](tests/TASK-000-PC-A-run-01.md)，跨机获取与部署见 [handoff](handoffs/TASK-000.md)。

## 后续 TASK-000 的剩余工作

已完成本次第1、2步的最小路径：选用并固定BaseLib 3.3.2、精确NuGet引用与锁文件、真实加载和资源读取；工程没有模板中的自动部署钩子，构建与部署由独立脚本执行。依赖官方ZIP的SHA256为`600d7ab3e4d735ec9bc97f06f1dfb03e51680d1ba2756117bcabcc5ea1d50e8a`。

玩法接口调查见 [API_FINDINGS](API_FINDINGS.md)，实际操作见 [DEVELOPMENT](DEVELOPMENT.md)。后续TASK-001须解决动作暂停/恢复时ID变化的逻辑动作关联，验证正常获取、回合与奖励时序；这些静态证据不等于玩法已实测。

PC-B接入后独立核查环境；双人测试还需要两个可同时使用的客户端、账号及操作者。当前无自动打包上传或公开发行流程。

## TASK-001 当前增量（2026-10-19）

以上TASK-000矩阵是历史加载阶段结果。当前Mod0.0.2已在同一版本基线实现并验证风/岩两张牌、单条连招、原生状态与奖励入口。代码S为`3c95e19441261410ed254ceada47c83c1af53b62`；21项纯规则、28项真实游戏断言、手动界面、两次干净构建与部署/移除/恢复通过。默认启动不运行测试Node，游戏内`tc_hand`也明确拒绝未开启测试参数的调用。

PC-B及双人仍NOT_RUN/BLOCKED，完整退出读档NOT_RUN，阶段WAITING_MULTIPLAYER。精确包、结果及限制见[TASK-001报告](tests/TASK-001-PC-A-run-01.md)和[交接](handoffs/TASK-001.md)。Godot导入配置与源码UID文件随仓库提交，工具基线未变。

## 来源

- [模板作者安装指南](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)：.NET SDK、Godot/MegaDot、模板与默认部署行为。
- [模板作者仓库](https://github.com/Alchyr/ModTemplate-StS2)：本机另外检查了 NuGet 2.5.3 包内的 csproj、props、入口源码与导出配置。
- [Godot 官方 4.5.1 发布页](https://github.com/godotengine/godot-builds/releases/tag/4.5.1-stable)：本次安装标准 Godot .NET 4.5.1，未安装 MegaDot。
- [Godot 官方资源包导出说明](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_pcks.html)。
- [.NET 10 官方发布元数据](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json)、[.NET 9 官方发布元数据](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/9.0/releases.json)。
- [ILSpy 作者仓库](https://github.com/icsharpcode/ILSpy)。
