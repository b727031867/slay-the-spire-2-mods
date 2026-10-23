# 构建、部署与验证

工程为src/Sts2Mod，Mod ID为Sts2Mod。开发/发布版本、分支、包身份与验证范围统一见[STATUS](STATUS.md)；玩法规则见[玩家指南](player/README.zh-CN.md)。

## 统一测试入口

先按[影响范围验证SOP](TESTING-SOP.md)评估本次改动，日常默认“核心检查＋受影响专项”，Full用于共享机制等高风险改动和发布候选。自动运行、等待完成、严格验收并清理自己的测试进程，例如局部卡牌改动：

```powershell
.\scripts\Invoke-ModTests.ps1 -Suite Cards -CodeCommit '<实际程序集身份S>'
```

Suite支持Load、Visuals、Cards、Character、Full、SavePrepare和SaveResume。统一入口强制无GUI，返回Suite、Status、Assertions、Cases、CodeCommit、LogFile、SandboxDirectory和PID；结果JSON保存到日志旁。Rendering和Multiplayer标明NOT_RUN，不把节点测试视作图形验收。

需要分开启动与读取日志时：

```powershell
$launch = .\scripts\Start-ModTest.ps1 -Suite Visuals
# 等待套件完成后再验收；结束时由调用方清理$launch.Process。
.\scripts\Test-ModRun.ps1 -Suite Visuals -LogFile $launch.LogFile -CodeCommit '<S>'
```

分步启动由调用方等待并只清理该PID。Invoke-ModTests自动等待套件完成标记，超时或失败也清理本轮进程。各套件保留严格检查器和断言，Full同时检查基础、角色、扩展卡池及视觉证据；旧脚本作为兼容入口保留。专项含加载检查，Cards含视觉检查，Full含这些专项及事件；已有覆盖不额外启动相同套件，磁盘恢复仍独立执行。

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

四事件专项可单独运行 `Start-ModGame.ps1 -Headless -TravelerEvents -PassThru`，记录返回进程与日志；完成 `STS2MOD_EVENTS_COMPLETE` 后使用 `Test-TravelerEventsRun.ps1 -LogFile <日志> -CodeCommit <包身份>` 严格校验，并只清理该次返回的进程。Full也执行此专项，且要求事件完成证据。SavePrepare/SaveResume夹具额外验证永久元素、归途和六种事件遗物的真实磁盘恢复；持续战斗前明确移除这些夹具增益以保留原有soak基线。

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

使用接续分支中的最新检查器；已发布包来源S及SHA256从[STATUS](STATUS.md)进入对应发布报告。自行重新构建时记录构建返回的新S和SHA256，不沿用旧包身份。

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

## 发布准备：测试、冻结与工坊目录

先提交源码，用Build-Mod从干净提交构建核心包。以下入口适用于尚未完成候选验收的核心，会对本轮部署运行Full，不隐式编译、不提交Git、不上传Steam，也不改STATUS中的发布结论。同一核心已有覆盖完整、环境匹配的通过证据时，按SOP核对哈希后使用下文独立冻结入口，避免重复Full。

```powershell
.\scripts\Invoke-ReleasePreparation.ps1 -PackageDirectory '<干净核心目录>/Sts2Mod'
```

入口依次校验核心、Deploy-Mod部署（已有BaseLib须匹配）、Invoke-ModTests运行无GUIFull、核对核心未变化、Build-ReleaseCandidate冻结快照、Prepare-Workshop准备上传目录。已有工坊重复副本须在部署前按原流程可恢复备份，避免双重加载。新入口的部署备份行为与Deploy-Mod相同，不自动恢复本地Mod；结束后本机保留候选供后续使用。

输出PREPARED、核心身份/哈希、Full结果、快照/ZIP哈希与上传目录，保存为快照目录旁的.preparation.json。失败直接终止，保留日志及已生成产物便于排查；不会继续上传。磁盘专项按改动范围使用上文SavePrepare/SaveResume。人工试玩、图形、联机和自然整局验证均为可选，不构成提交、合并、打包或发布门槛；仅在用户明确要求时执行并单列结果。

如已完成对应验收，只需独立冻结和准备：

```powershell
$candidate = .\scripts\Build-ReleaseCandidate.ps1 -PackageDirectory '<已验收核心目录>/Sts2Mod'
.\scripts\Prepare-Workshop.ps1 -SnapshotDirectory $candidate.CandidateDirectory
```

Build-ReleaseCandidate不构建。它保留四文件核心字节，附带三份玩家文档，以及已填充说明的workshop.json、mod_id.txt、封面和规则图。release-info.txt使用schemaVersion=2，分别记录核心、文档和工坊文件SHA256，并记录文档提交、文档dirty与工坊dirty。ZIP包含完整快照，重新包装有新的ZIP哈希。

Get-ReleaseSnapshot校验快照身份、全部文件清单/哈希、固定工坊ID、friends_only、BaseLib依赖及图片大小。Prepare-Workshop只读取快照，逐字节生成上传内容并校验复制结果，不再从仓库读取文档、配置或图片。默认目录为.local/workshop/<S>-<快照哈希前12位>，已有目录拒绝覆盖；需要刷新内容就重新冻结，不手改旧上传目录。旧schemaVersion=1候选必须用原核心重新包装；Prepare-Workshop原PackageDirectory参数已改为SnapshotDirectory，防止静默回退到仓库内容。

准备不证明验收通过，独立包装入口不会要求Full报告。正式发布需审阅对应结果，按[工坊说明](../workshop/README.md)上传、核对远端可见性、Steam实际下载和加载，再更新STATUS及发布报告。快照哈希用于发现意外改动，不是数字签名。

脚本离线回归（合成包/日志，不启动游戏、不访问Steam）：

```powershell
.\tests\Test-ReleasePackaging.ps1
.\tests\Test-ReleasePreparation.ps1
.\tests\Test-ModTestEntry.ps1
```

## 工坊可见性与可选联机测试

工坊条目3823282901始终保持“仅限好友”。上传配置使用 `friends_only`，准备入口拒绝其他值；上传前核对实际工作目录，上传后读取远端确认FriendsOnly。历史发布报告中的private是当时记录，不再作为后续上传策略。只有用户明确另行要求才更改可见性。人工试玩和联机验证不是开发或发布的强制条件；用户明确要求联机测试时再与朋友执行，并将结果与无GUI单人验收分开记录。
