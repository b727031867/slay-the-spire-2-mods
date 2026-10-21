# 开发与运行基线

| 项目 | 固定值 |
| --- | --- |
| 平台 | Windows x64 |
| Steam App ID | 2868840 |
| 游戏版本 / 构建 / 提交 | v0.107.1 / 23811903 / 59260271 |
| BaseLib | 精确3.4.7，NuGet与锁文件固定 |
| .NET SDK | 10.0.402，由global.json固定 |
| 目标框架 / 开发工具运行时 | net9.0 / 9.0.21 |
| Godot .NET / Godot.NET.Sdk | 4.5.1 |
| Mod模板 | Alchyr.Sts2.Templates 2.5.3 |
| 接口查看工具 | ILSpy CLI 11.1.0.9782 |
| 终端 | PowerShell 7 |

游戏内置运行时由游戏提供，开发工具运行时不替代游戏运行时。依赖地址和SHA256以dependencies.json为准；程序集版本字段不等同于游戏发行版本。

PC-A与PC-B均可开发、构建和单人测试，各自使用本机工具路径、合法游戏及local.settings.json。当前版本与逐设备接手范围见[STATUS](STATUS.md)。环境验证不能由另一台PC结果推断。

## 本机核对

在PowerShell 7填写本机配置后执行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet --version
ilspycmd --version
.\scripts\Test-DevEnvironment.ps1
```

PC-A显式使用用户安装的PowerShell 7；另一PC定位自己的安装路径。不要复制其他设备的路径、凭据、缓存或存档。系统PATH可能优先命中旧dotnet，先加载项目环境入口。

环境检查验证工具链、真实程序集编译及无GUI资源导出，不能代替Mod加载、玩法、工坊和多人验收。版本不同先报告，不静默更新锁定基线。版本演进统一见[更新日志](player/CHANGELOG.md)。
