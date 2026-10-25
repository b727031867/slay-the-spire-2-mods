# 构建、部署与验证

工程为src/Sts2Mod，Mod ID为Sts2Mod。开发/发布版本、分支、包身份与验证范围统一见[STATUS](STATUS.md)；玩法规则见[玩家指南](player/README.zh-CN.md)。

## 开发到发布的顺序

2026-10-24起，每个开发任务从最新共享main建立独立codex/分支，涵盖玩法、修复、脚本与文档。按影响范围检查通过后合入main并推送，核对远端结果后删除已完成且无独有工作的任务分支。纯文档做一致性与链接检查，不启动游戏。

合并后提供已测试的本地候选供用户试玩，记录源码S、核心四文件SHA256和用户反馈。用户明确确认该候选试玩成功后才发布工坊；未确认时停留在待试玩/待发布，不影响提交、合并和打包。不得先上传工坊供试玩，不能用Full PASS或PREPARED代替用户反馈。核心字节改变或重新构建后，须重新验证并确认新包；仅文档重新包装且核心逐字节相同，可沿用该核心的试玩确认，另核对新快照与上传内容。

## 制品保留与清理

- 后续每台PC只保留最近3份可交付制品，按冻结/交付时间排序，以实际包/快照哈希区分；相同Mod版本的不同发布包仍分别计数。
- 一份制品包含同次交付的核心目录、完整快照、ZIP、SHA256、准备报告及工坊上传副本，相关副本合并计数。默认构建不生成中间ZIP，最终候选统一归档；不在其他目录长期堆放重复二进制。
- 统一交付入口Invoke-ReleasePreparation完成测试校验、冻结、ZIP校验及上传目录准备，并写入PREPARED报告后，自动调用Remove-OldDeliverables保留本次及最近两份其他交付。相同快照的重复目录只占一份，相同核心的独立构建/ZIP按关联副本处理；当没有保留快照再使用该核心时一并删除。已有`*-release`完整快照和独立核心交接ZIP会纳入首次轮转。失败测试、冻结或上传目录准备不会触发清理。
- 保留源码/原始素材、依赖和工具缓存、用户配置/存档、原始诊断日志、测试结果与有意保留的恢复备份；它们不计入可交付制品。旧二进制过期后，历史报告仍保留原来的包身份、哈希和PASS/FAIL，不把已删除包描述为本机仍可复用。不要把普通制品改名为备份来规避上限。
- 自动范围为本仓库artifacts下的已识别交付及其.local/workshop副本，包含过期快照、ZIP、校验文件、准备报告、阶段耗时和已核对的关联核心。先验证包、归档哈希、上传内容与所有删除路径，拒绝目录联接和未知上传文件；校验异常在删除前停止。清理结果返回并写入当前.preparation.json的Retention字段；清理异常会抛错并保留新候选和已有准备报告，排除原因后可用`Remove-OldDeliverables.ps1 -PreparationFile '<报告路径>'`重试，不重跑测试。
- 使用默认输出或artifacts的直接子目录才能自动轮转；自定义输出到其他目录时返回Retention.Status=SKIPPED并警告，不删除任意外部目录。Build-Mod、Build-ReleaseCandidate和Prepare-Workshop单独运行不算完成交付，不触发删除；已有测试报告时应通过Invoke-ReleasePreparation的-TestResultFile复用证据完成交付。未完成任务的构建、未成功准备的候选、其他临时包和工作树仍由任务收尾核对清理，不由脚本猜测用途。

首次过渡：2026-10-24按用户确认的清单删除12个本地/20个远端已完成分支、两个旧工作树和未跟踪平衡分析；用户执行BAT后核实剩余143项目标全部不存在，2532个保留文件大小/修改时间不变。累计清单约6.7 GiB。本轮此前明确保留的历史快照、回滚包和最新回归工作树不追加删除；下一次交付时将这些制品纳入统一盘点，收敛至三份。清单、恢复bundle和本机核验材料位于.local/backups/project-cleanup-20261024，不进入Git。

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

脚本核对游戏版本和Steam构建号，执行锁定依赖还原、ExportRelease/win-x64编译、Godot导入与PCK导出，默认只输出包含源码提交的四文件核心目录，避免和最终候选重复压缩。需要独立交接核心ZIP时显式加`-Archive`，会输出ZIP及SHA256；正式发布仍由Build-ReleaseCandidate生成完整快照ZIP。构建目录旁的`.timings.json`记录校验、还原、编译、导入、导出和收尾耗时。还原和导出统一配置，避免Godot的Debug编辑器依赖改写发布锁文件。构建不会自动复制文件到游戏。正式交接必须从实际代码提交S的干净工作区执行。

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

先汇总本轮代码和资源改动，再提交、构建并测试同一包。以下入口默认对尚未完成候选验收的核心部署并运行Full，不隐式编译、不提交Git、不上传Steam，也不改STATUS中的发布结论。低风险修复按影响评估和用户授权显式使用`-Suite Cards,Events`等范围；入口不会自动猜测或缩减测试范围。

```powershell
.\scripts\Invoke-ReleasePreparation.ps1 -PackageDirectory '<干净核心目录>/Sts2Mod'
```

未传报告时入口依次校验核心、Deploy-Mod部署（已有BaseLib须匹配）、Invoke-ModTests运行所选无GUI专项、核对核心未变化、Build-ReleaseCandidate冻结快照、Prepare-Workshop准备上传目录。已有工坊重复副本须在部署前按原流程可恢复备份，避免双重加载。部署备份行为与Deploy-Mod相同，不自动恢复本地Mod；结束后本机保留候选供后续使用。

已完成测试时传报告即可复用，不部署、不启动游戏、不重新构建：

```powershell
# 独立测试时绑定实际四文件包；本地部署必须与它逐字节一致。
$test = .\scripts\Invoke-ModTests.ps1 -Suite Cards -CodeCommit '<包S>' -PackageDirectory '<核心目录>/Sts2Mod'
# Events也支持同样入口，所需范围取整批改动的并集。
.\scripts\Invoke-ReleasePreparation.ps1 -PackageDirectory '<同一核心目录>/Sts2Mod' -Suite Cards -TestResultFile ($test.LogFile+'.result.json')
```

`-TestResultFile`可接收多个报告。新报告schemaVersion=2绑定核心四文件SHA256、机器、游戏构建/程序集、BaseLib三文件哈希及稳定日志哈希；测试前后检查源包、本地部署与环境不变，结束本轮进程后才校验并哈希日志。复用时重新核对当前环境及严格日志检查器，并检查所需套件覆盖；Full报告可覆盖各普通专项，Cards不能冒充Full或Events。没有绑定的旧报告、丢失/改变的日志、不同包或环境会拒绝复用，不自动重跑耗时测试。省略`-PackageDirectory`仍可生成旧式报告，但不能用于自动复用。需要同一机器只有一份可识别BaseLib安装，避免混淆本地和工坊依赖。

输出PREPARED、核心身份/哈希、所选范围、测试结果及是否复用、阶段耗时、快照/ZIP哈希与上传目录，保存为快照目录旁的.preparation.json。测试报告另含DurationSeconds。失败直接终止，保留诊断日志便于排查；失败中间包排查完成后清理，不作为可交付制品保留。磁盘专项按改动范围使用上文SavePrepare/SaveResume。用户试玩不阻塞提交、合并和打包，但工坊发布必须等待对应候选试玩成功；图形自动化、真实联机和自然整局系统性验证仍按用户明确要求执行并单列结果。

准备成功后自动执行上文三份制品轮转，返回Retention.Status、Kept和Removed并回写报告。自动清理只处理托管目录；当前安装、用户存档、独立日志、备份和未完成任务保持不动。该步骤不表示用户试玩成功或已上传工坊。

如已完成对应验收，只需独立冻结和准备：

```powershell
$candidate = .\scripts\Build-ReleaseCandidate.ps1 -PackageDirectory '<已验收核心目录>/Sts2Mod'
.\scripts\Prepare-Workshop.ps1 -SnapshotDirectory $candidate.CandidateDirectory
```

Build-ReleaseCandidate不构建。它保留四文件核心字节，附带三份玩家文档，以及已填充说明的workshop.json、mod_id.txt、封面和规则图。release-info.txt使用schemaVersion=2，分别记录核心、文档和工坊文件SHA256，并记录文档提交、文档dirty与工坊dirty。ZIP包含完整快照，重新包装有新的ZIP哈希。

Get-ReleaseSnapshot校验快照身份、全部文件清单/哈希、固定工坊ID、BaseLib依赖及图片大小，兼容历史friends_only快照供审计和制品轮转。新Build-ReleaseCandidate和Prepare-Workshop只接受public；旧候选需要从原核心按当前公开配置重新冻结，不能直接上传旧好友目录。Prepare-Workshop只读取快照，逐字节生成上传内容并校验复制结果，不再从仓库读取文档、配置或图片。默认目录为.local/workshop/<S>-<快照哈希前12位>，已有目录拒绝覆盖；需要刷新内容就重新冻结，不手改旧上传目录。旧schemaVersion=1候选必须用原核心重新包装；Prepare-Workshop原PackageDirectory参数已改为SnapshotDirectory，防止静默回退到仓库内容。

准备不证明验收通过，独立包装入口不会要求Full报告，也不自动检查用户反馈。正式发布前由执行者审阅测试结果并核对用户对该核心包的试玩成功确认，再按[工坊说明](../workshop/README.md)上传、核对远端可见性、Steam实际下载和加载，更新STATUS及发布报告，并执行三份制品保留规则。快照哈希用于发现意外改动，不是数字签名。

脚本离线回归（合成包/日志，不启动游戏、不访问Steam）：

```powershell
.\tests\Test-ReleasePackaging.ps1
.\tests\Test-ReleasePreparation.ps1
.\tests\Test-DeliverableRetention.ps1
.\tests\Test-ModTestEntry.ps1
```

## 工坊可见性与可选联机测试

2026-10-25起工坊条目3823282901及后续更新均公开。上传配置使用 `public`，新候选及上传准备入口拒绝其他值；上传前核对用户对候选的试玩成功确认及实际工作目录，上传后读取远端确认Public。历史friends_only/private/visibility=null只记录当时事实，不作为后续上传策略。只有用户明确另行要求才更改可见性。用户试玩仍是新内容发布前置条件；仅修改已有条目权限时，可按用户明确授权运行WorkshopProbe public 3823282901，不上传新包，再inspect核对Public。联机仍为可选，用户明确要求时再与朋友执行，并将结果与无GUI单人验收分开记录。
