# Windows 开发环境基线

核查日期：2026-10-19。范围：按需求书第 8 节安装 Windows 开发与后续测试所需工具。需求原文仍为根目录 `STS2-Mod-Requirements-v0.1.md`，本次没有改动玩法设计。

## 已核实的游戏

| 项目 | 本机证据 |
|---|---|
| Steam App ID | 2868840 |
| 游戏版本 | v0.107.1 |
| 游戏发布分支字段 | release_info.json：v0.107.1；Steam manifest 未设置 BetaKey，未通过 Steam UI 另行确认分支选择 |
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

重新配置其他电脑时：复制 `local.settings.example.json` 为 `local.settings.json` 并填入实际路径；按本表固定版本安装；运行上述检查。Debian 端仍需独立安装 Linux 版本工具与私有游戏引用，本次未连接或安装远程 Debian。

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
| 游戏加载该 Mod、正常卡池、UI、存档 | NOT_RUN |
| 双人、主客机同步 | NOT_RUN |
| Debian 编译、无 UI 导出 | NOT_RUN |

验证工程只用于证明真实程序集能被编译器引用，以及编辑器能生成资源包。它没有卡牌或连招实现，也没有自动部署到游戏。无需额外安装 Godot 导出模板即可完成此次 `.pck` 导出；完整可执行游戏导出不在本次范围内。

## 后续 TASK-000 的剩余工作

1. 按 v0.107.1 核查并固定 BaseLib 或 RitsuLib 之一的兼容版本；本次只装模板，没有给游戏安装尚未核实的运行时框架。
2. 模板默认 BaseLib 引用为浮动版本；创建正式工程时须改成经过兼容验证的精确版本。模板构建默认会复制文件到游戏 mods，首次构建前须改为显式部署或临时输出目录。
3. 核实卡池、遗物、主动/自动出牌、动作 ID、回合和同步接口；准备最小可加载 Mod。
4. 建立 Debian 私有引用准备步骤与 Git 远程交接；GitHub 仓库信息见 STATUS。
5. 单人和双人验证仍需游戏实测和第二客户端条件。安装开发环境不代表 TASK-000 全部完成。

## 来源

- [模板作者安装指南](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)：.NET SDK、Godot/MegaDot、模板与默认部署行为。
- [模板作者仓库](https://github.com/Alchyr/ModTemplate-StS2)：本机另外检查了 NuGet 2.5.3 包内的 csproj、props、入口源码与导出配置。
- [Godot 官方 4.5.1 发布页](https://github.com/godotengine/godot-builds/releases/tag/4.5.1-stable)：本次安装标准 Godot .NET 4.5.1，未安装 MegaDot。
- [Godot 官方资源包导出说明](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_pcks.html)。
- [.NET 10 官方发布元数据](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json)、[.NET 9 官方发布元数据](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/9.0/releases.json)。
- [ILSpy 作者仓库](https://github.com/icsharpcode/ILSpy)。
