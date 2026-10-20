# 构建、部署与最小加载检查

当前工程：`src/Sts2Mod`，Mod ID为`Sts2Mod`；0.1.0 已实现六张牌、三条连招与派蒙遗物。下列构建和部署步骤适用于两台PC，首次环境配置参见 [COLLABORATION](COLLABORATION.md)，跨机接手可直接使用[完整提示词](handoffs/PC-B-CONTINUE-PROMPT.md)。

## 前提

2026-10-20用户要求：后续由AI执行的游戏测试一律使用无GUI模式，避免影响其他工作。加载检查使用`Start-ModGame.ps1 -Headless`；回归和验收分别使用`-TravelerTest -Headless`或`-TravelerAcceptance -Headless`，日志检查按支持情况加`-Headless`。自动测试参数也会在启动脚本中强制启用无GUI。仅在用户明确要求图形测试时再安排人工界面验证；下文历史人工选牌流程不作为后台自动测试运行。测试结束只清理本次启动的进程，不启动图形窗口。

- 游戏v0.107.1、Steam构建23811903，Steam客户端已运行并登录拥有游戏的账号。
- .NET SDK 10.0.402、Godot .NET 4.5.1、本机合法游戏引用；各设备配置自己的`local.settings.json`。
- 唯一额外运行时框架为BaseLib 3.3.2；NuGet引用与锁文件固定版本，官方运行时下载地址和SHA256在`dependencies.json`。
- 首次运行时游戏会提示是否加载模组；允许后需要退出并重启游戏。脚本不修改该确认设置。

## 构建

在工作区根目录的PowerShell 7执行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Build-Mod.ps1 -DeviceId PC-A
```

PC-B使用`-DeviceId PC-B`。构建默认要求所有源码和配置已提交；开发中可显式加`-AllowDirty`生成带dirty标记的本地调试包，这类包不能作为指定干净提交的交接产物。

脚本核对游戏版本和Steam构建号，执行锁定依赖还原、ExportRelease/win-x64编译、Godot导入与PCK导出，并输出包含源码提交的目录、ZIP和SHA256文件。还原和导出统一配置，避免Godot的Debug编辑器依赖改写发布锁文件。构建不会自动复制文件到游戏。正式交接必须从实际代码提交S的干净工作区执行。

包内仅有：

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
.\scripts\Start-ModGame.ps1
```

`-InstallDependencies`下载并校验官方BaseLib 3.3.2，安装到`mods/BaseLib`；当前Mod安装到`mods/Sts2Mod`。已有目录先备份至`.local/backups/`，遇到未知文件、错误ID或重定向目录则拒绝覆盖。后续可省略该参数复用已匹配版本的BaseLib。不要同时安装Workshop的另一版本BaseLib；此工作流使用本地固定版本。

启动脚本会输出游戏进程ID与本次日志路径。它通过子进程环境提供真实Steam App ID，Steam仍执行账号/拥有权校验；启动失败时先确认Steam登录状态。脚本支持`-Headless`作为开发选项，但本任务验收采用真实图形界面启动，不将无界面启动当成界面验收。

进入主菜单后检查（用实际日志路径和完整代码提交替换占位符）：

```powershell
.\scripts\Test-ModLoad.ps1 -LogFile '<实际日志路径>' -CodeCommit '<完整代码提交S>'
.\scripts\Collect-ModLogs.ps1 -LogFile '<实际日志路径>'
```

应看到一次`STS2MOD_READY version=<Mod版本>+<S> baselib=3.3.2 resource=ok`，游戏加载器完成两个Mod初始化并进入主菜单。`Test-ModLoad`还会拒绝出现未知 ERROR 的日志；仅在显式 `-Headless` 时可精确过滤已知 Godot RID/shader 渲染噪声。原生清单中显示BaseLib和STS2 Mod - Traveler Elements。

原始日志保存在`.local/logs/`，不上传；收集脚本仅导出有限诊断摘录到`artifacts/logs/`，分享前仍需检查内容。游戏原生启动可能同步Steam云存档，部署和移除脚本本身不修改存档。

## 移除与恢复

退出游戏后：

```powershell
.\scripts\Remove-Mod.ps1
```

脚本只备份并移除经过ID和路径检查的`mods/Sts2Mod`，保留BaseLib、其他Mod和存档。它输出备份目录，可用原构建包或该备份下的Sts2Mod目录恢复：

```powershell
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<备份目录>/Sts2Mod'
```

移除后也可启动游戏，并使用`Test-ModLoad.ps1 -LogFile '<日志>' -ExpectAbsent`检查本项目不再加载；BaseLib作为共享框架仍会加载。

## TASK-001：风岩玩法与测试

两张普通牌加入铁甲战士原生卡池，不修改初始牌组。风起之刃1费6伤害（升级9），岩之守护1费7格挡（升级10）；主动风→岩额外4格挡，每人每回合最多2次。普通牌不打断，自动打出和额外结算不推进。上一元素、剩余次数在原生状态图标的提示中显示；触发有短气泡提示。

默认 `Start-ModGame.ps1` 只启动正常游戏，不开启测试局或赠牌。纯规则测试：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release
```

退出游戏后，以明确参数启动不保存run快照的单人测试局：

```powershell
.\scripts\Start-ModGame.ps1 -TravelerTest -Headless
```

等待日志出现 `STS2MOD_TEST_COMPLETE`，再运行 `Test-TravelerRun.ps1 -LogFile '<本次日志>' -CodeCommit '<S>'`。测试用原生出牌动作和回合转换验证规则；仅在测试夹具中补能量、清手牌、治疗玩家及提高靶子生命。结束后留下风岩手牌供手动检查。测试角色/seed固定，正常模式无这些处理。

游戏内按 `Shift+8`（原生控制台也支持键盘左上角的反引号）打开控制台，输入 `tc_hand 1`，或 `tc_hand 3 upgraded`。该命令要求显式测试参数、单人铁甲战斗、玩家阶段且无执行中的动作；替换当前战斗手牌并提供2/4/6能量，不改永久牌组，不重置连招状态。测试下一组前先正常结束回合。命令用 `-TravelerDebug` 或 `-TravelerTest` 开启，默认会拒绝执行；不能用于双人联机测试。

正常获取入口的独立检查：`Start-ModGame.ps1 -TravelerRewards`。它创建独立测试局，用原生Kill快速结束测试战斗，保留未修改的原生三选一奖励逻辑，跳过不含主题牌的奖励。出现 `STS2MOD_ACQUISITION_READY` 后，手动点击卡牌奖励并选择主题牌；只有原生奖励流程把牌加入永久牌组后才输出 `STS2MOD_ACQUISITION_COMPLETE`。用 `Test-TravelerRun.ps1 ... -Acquisition` 检查证据。该模式不强塞奖励、不替换奖励卡池，属于正常奖励入口的集成测试，不代表自然通关/平衡性测试。

测试局设置 `ShouldSave=false`，不创建该run的保存快照；游戏仍可能按原生行为写入模组档案的进度及同步Steam云数据。脚本不手工读取、删除或覆盖玩家存档，测试模式不要打开自己的既有对局。

双人端必须使用相同代码包、游戏和BaseLib版本，按需求书TC-16/17交换主客机验证；测试手牌通过测试新局的正常获取或双方原生支持的调试入口准备，`tc_hand`明确不适用于多人。缺少第二客户端时保留NOT_RUN/BLOCKED。

## TASK-002/003：完整内容与单人自动化

0.1.0包含六张铁甲主题牌、风岩/岩雷/雷风三条连招及派蒙遗物。岩→雷在基础伤害后对同一存活目标额外执行4点原生攻击伤害；雷→风抽1；派蒙在本回合第一次成功连招后额外3格挡。三条连招共用每回合2次上限。

正常模式不赠牌。显式测试模式下，`tc_hand 1 full paimon`准备完整普通卡组和派蒙，`tc_hand 1 full upgraded paimon`准备升级卡；`lightning`及原有风岩用法保留。调试入口只用于单人测试。

`Start-ModGame.ps1 -TravelerTest -Headless`执行完整单人回归，完成后用`Test-TravelerRun.ps1 -LogFile '<日志>' -CodeCommit '<S>' -Task003 -Headless`检查标记。`Start-ModGame.ps1 -TravelerAcceptance -Headless`可只跑TASK-003边界。无窗口逻辑检查不能替代卡牌图形界面验收；完整退出读档及双人测试仍未执行。
