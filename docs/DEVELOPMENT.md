# 构建、部署与验证

当前工程src/Sts2Mod，Mod ID为Sts2Mod，版本0.3.5。接续分支main，当前规则见[玩家指南](player/README.zh-CN.md)，包身份及验证范围见[发布报告](tests/TASK-011-PC-B-release-035.md)。

## 统一测试入口

自动运行、等待完成、严格验收并清理自己的测试进程：

```powershell
.\scripts\Invoke-ModTests.ps1 -Suite Full -CodeCommit '<实际程序集身份S>'
```

Suite支持Load、Visuals、Cards、Character、Full、SavePrepare和SaveResume。统一入口强制无GUI，返回Suite、Status、Assertions、Cases、CodeCommit、LogFile、SandboxDirectory和PID；结果JSON保存到日志旁。Rendering和Multiplayer标明NOT_RUN，不把节点测试视作图形验收。

需要分开启动与读取日志时：

```powershell
$launch = .\scripts\Start-ModTest.ps1 -Suite Visuals
# 等待套件完成后再验收；结束时由调用方清理$launch.Process。
.\scripts\Test-ModRun.ps1 -Suite Visuals -LogFile $launch.LogFile -CodeCommit '<S>'
```

分步启动由调用方等待并只清理该PID。Invoke-ModTests自动等待套件完成标记，超时或失败也清理本轮进程。各套件保留严格检查器和断言，Full同时检查基础、角色、扩展卡池及视觉证据；旧脚本作为兼容入口保留。

| Suite | 范围 |
| --- | --- |
| Load | 普通加载、来源身份、依赖及未知错误 |
| Visuals | 元素状态、原生悬停、能量图标及无自定义面板 |
| Cards | 扩展卡池、能力、随机追击和美术 |
| Character | 选人、初始模型、角色专属获取 |
| Full | 完整单人回归及相关套件 |
| SavePrepare / SaveResume | 原生磁盘准备与新进程恢复 |

所有测试经Start-ModGame.ps1 -Headless启动，自动套件使用隔离存档；Load仅普通启动。TravelerRewards等待人工操作，不纳入自动套件。

纯规则和日志分类检查：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release
.\tests\Test-LogClassification.ps1
```

## 前提

AI执行的游戏测试一律使用无GUI模式，避免影响其他工作。加载检查使用`Start-ModGame.ps1 -Headless`；回归和验收分别使用`-TravelerTest -Headless`或`-TravelerAcceptance -Headless`，日志检查按支持情况加`-Headless`。自动测试参数也会在启动脚本中强制启用无GUI。仅在用户明确要求图形测试时再安排人工界面验证；TravelerRewards等待人工选牌，不作为无人值守测试运行。测试结束只清理本次启动的进程，不启动图形窗口。

- 游戏v0.107.1、Steam构建23811903，Steam客户端已运行并登录拥有游戏的账号。
- .NET SDK 10.0.402、Godot .NET 4.5.1、本机合法游戏引用；各设备配置自己的`local.settings.json`。
- 唯一额外运行时框架为BaseLib 3.4.7；NuGet引用与锁文件固定版本，官方运行时下载地址和SHA256在`dependencies.json`。
- 首次运行时游戏会提示是否加载模组；允许后需要退出并重启游戏。脚本不修改该确认设置。

## 构建

在工作区根目录的PowerShell 7执行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Build-Mod.ps1 -DeviceId PC-A
```

PC-B使用`-DeviceId PC-B`。构建默认要求所有源码和配置已提交；开发中可显式加`-AllowDirty`生成带dirty标记的本地调试包，这类包不能作为指定干净提交的交接产物。

脚本核对游戏版本和Steam构建号，执行锁定依赖还原、ExportRelease/win-x64编译、Godot导入与PCK导出，并输出包含源码提交的目录、ZIP和SHA256文件。还原和导出统一配置，避免Godot的Debug编辑器依赖改写发布锁文件。构建不会自动复制文件到游戏。正式交接必须从实际代码提交S的干净工作区执行。

构建输出的核心目录包含四文件；发布准备另附玩家指南、更新日志和素材说明，共七文件：

```text
Sts2Mod/
  Sts2Mod.dll
  Sts2Mod.json
  Sts2Mod.pck
  build-info.txt
```

`build-info.txt`内容为JSON，记录源码提交、dirty状态、构建设备、游戏/基础库版本与逐文件SHA256。扩展名必须为`.txt`：本机原生加载器会递归将mods目录下所有`.json`视作模组清单。包不包含BaseLib或游戏程序集。

## 部署、启动和检查

用构建输出的实际`PackageDirectory`替换下方占位符；部署前退出游戏。

```powershell
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<实际包目录>/Sts2Mod' -InstallDependencies
.\scripts\Start-ModGame.ps1 -Headless
```

`-InstallDependencies`下载并校验官方BaseLib 3.4.7，安装到`mods/BaseLib`；当前Mod安装到`mods/Sts2Mod`。已有目录先备份至`.local/backups/`，遇到未知文件、错误ID或重定向目录则拒绝覆盖。后续可省略该参数复用已匹配版本的BaseLib。不要同时安装Workshop的另一版本BaseLib；此工作流使用本地固定版本。

启动脚本会输出游戏进程ID与本次日志路径。它通过子进程环境提供真实Steam App ID，Steam仍执行账号/拥有权校验；启动失败时先确认Steam登录状态。AI测试使用`-Headless`，本轮自动化图形界面验收记录NOT_RUN；用户试玩结论单列。

进入主菜单后检查（用实际日志路径和完整代码提交替换占位符）：

```powershell
.\scripts\Test-ModLoad.ps1 -LogFile '<实际日志路径>' -CodeCommit '<完整代码提交S>' -Headless
.\scripts\Collect-ModLogs.ps1 -LogFile '<实际日志路径>'
```

应看到一次`STS2MOD_READY version=<Mod版本>+<S> baselib=3.4.7 resource=ok`，游戏加载器完成两个Mod初始化并进入主菜单。`Test-ModLoad`还会拒绝出现未知 ERROR 的日志；仅在显式 `-Headless` 时按错误文本与紧随的原生源码位置联合分类已知 Godot RID/shader 渲染错误，并输出数量。相同文本来自其他源码位置仍会失败。

全量单人无GUI回归使用`Start-ModGame.ps1 -Headless -TravelerTest`，完成后执行`Test-TravelerRun.ps1 -LogFile '<日志>' -CodeCommit '<S>' -Task003 -Headless`。`-TravelerAcceptance`仅运行TASK-003边界子集，不能替代全量回归，也不能单独传给要求全量标记的检查器。

### 原生获取和磁盘恢复

使用main中的最新检查器；已发布包来源S及SHA256见[发布报告](tests/TASK-011-PC-B-release-035.md)。自行重新构建时记录构建返回的新S和SHA256，不沿用旧包身份。

```powershell
$prepare = .\scripts\Invoke-ModTests.ps1 -Suite SavePrepare -CodeCommit '<包S>'
# 准备进程原生退出后，重新部署同一包：
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<同一包目录>/Sts2Mod'
.\scripts\Invoke-ModTests.ps1 -Suite SaveResume -CodeCommit '<同一包S>' -SandboxDirectory $prepare.SandboxDirectory
```

自动测试使用唯一的本地隔离存储：`.local/task003-saves/<id>`存元数据，真实原生存档写入`user://sts2mod-acceptance/<id>`。没有Steam云后端，不拷贝用户档案；启动时仍读取本机原生模组启用设置。测试显示/音频设置使用默认值且静音，run/progress/prefs保留真实磁盘路径。首次准备拒绝已存在的目录，恢复要求所属标记；不会删除或覆盖用户profile。

准备阶段领取未修改的原生战斗奖励、按原价购买普通商店的主题牌和派蒙，再保存18种奖励牌的基础/升级覆盖夹具。恢复阶段使用原生主菜单同一路径加载磁盘run，核对牌组、遗物、金币、重复注册/发奖和战斗入口状态；连续场景不补手牌/能量，每场最多4回合，不等于自然整局或平衡性通过。

原生退出存在不依赖本Mod的headless资源清理诊断，由严格分类器按退出阶段、错误文本和原生源码位置限定。E2E检查器单独统计经过对照的精确引擎类型，且必须位于NGame.Quit和Steam shutdown成功之后；其他错误仍失败。普通全量回归不启用这项退出分类。不要用泛化过滤规则隐藏未知错误，也不要把该限制写成“退出没有任何ERROR”。

原始日志保存在`.local/logs/`，不上传；收集脚本仅导出有限诊断摘录到`artifacts/logs/`，分享前仍需检查内容。普通游戏启动可能同步Steam云；自动验收使用上述隔离命名空间且无云后端。部署与移除脚本不修改存档。

## 移除与恢复

退出游戏后：

```powershell
.\scripts\Remove-Mod.ps1
```

脚本只备份并移除经过ID和路径检查的`mods/Sts2Mod`，保留BaseLib、其他Mod和存档。它输出备份目录，可用原构建包或该备份下的Sts2Mod目录恢复：

```powershell
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<备份目录>/Sts2Mod'
```

移除后AI加载检查仍使用Start-ModGame.ps1 -Headless，并使用`Test-ModLoad.ps1 -LogFile '<日志>' -ExpectAbsent`检查本项目不再加载；BaseLib作为共享框架仍会加载。

## 发布包装：复用已验收核心包

Build-Mod.ps1负责从源码构建四文件核心包。Build-ReleaseCandidate.ps1不再构建，也不读取当前HEAD替换核心源码身份：

```powershell
.\scripts\Build-ReleaseCandidate.ps1 -PackageDirectory '<已校验核心目录>/Sts2Mod'
```

入口通过Get-ModPackage校验四个非空普通文件、干净标记、源码提交、清单版本和三文件SHA256，再逐字节复制核心；附带当前玩家文档及release-info.txt，分别记录核心四文件哈希、文档提交/dirty与文档哈希。输出为新ZIP和SHA256，拒绝覆盖已有目录或归档。文档dirty标记不改变核心clean身份。

新二进制先从干净源码运行Build-Mod.ps1，再将返回的PackageDirectory交给包装入口。重新包装ZIP拥有新SHA256，不能沿用原ZIP身份；核心S和文件哈希保持。Prepare-Workshop也复用核心校验器，仅准备私有目录，不上传。

脚本回归：

```powershell
.\tests\Test-ReleasePackaging.ps1
.\tests\Test-ModTestEntry.ps1
```

包装测试使用合成四文件夹具验证流程与拒绝条件，不视为可加载游戏包；测试入口离线用例使用合成日志，真实游戏结果另记。
