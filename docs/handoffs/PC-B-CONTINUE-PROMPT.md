# PC-B 接续开发提示词

将以下内容作为新电脑上开发AI的完整指令。电脑可自行选择工作目录；无需复制PC-A的个人配置、工具缓存或存档。以远端最新任务记录为准。

---

你现在在另一台电脑PC-B接手开发《杀戮尖塔2》Mod。请先实际检查环境和仓库，再直接推进接续开发，不要只输出计划。当前优先单人游戏验证，双人联机由后续专项测试，不阻塞单人交付；未测不记PASS。

## 1. 仓库和当前身份

- 仓库：https://github.com/b727031867/slay-the-spire-2-mods
- 最新已交付开发分支：`codex/TASK-002-lightning`。不要只拉取main，相关PR尚未统一合并。
- 增量A被测代码S：`06e16c144727378e335ae0b724d0c42e374f6ac5`，Mod0.0.3。
- 已知交接文档提交：`bcd7492ee6c5bbeeffa0c9b7f2e2b00d2766f626`；远端随后可能有提示词/说明更新，先fetch读取最新。
- PR #3：https://github.com/b727031867/slay-the-spire-2-mods/pull/3，基于`codex/TASK-001`；不要擅自合并这些PR、改写main或公开发布Release。

先检查此电脑操作系统、实际工作目录、Git和PowerShell。Windows命令使用PowerShell 7，工具支持shell参数时显式指定本机实际的pwsh.exe；不要照搬PC-A用户GXF的绝对路径，不使用Windows PowerShell 5.1作为开发终端。若本机并非Windows，先说明脚本与运行平台差异，不假装现有.exe路径可用。

## 2. 安全同步源码

没有本地仓库时，在合适的工作目录执行：

```powershell
git clone --branch codex/TASK-002-lightning https://github.com/b727031867/slay-the-spire-2-mods.git
Set-Location slay-the-spire-2-mods
git status --short --branch
git log -3 --oneline
```

已有仓库时，先检查origin URL、当前分支和未提交改动，再`git fetch origin`；工作区干净且无分叉时切换或建立跟踪分支，并`git pull --ff-only`。有未提交内容、同名目录、分叉时先保留和检查，不执行hard reset、强推、覆盖目录或自动stash后丢弃。

同步确认后，从该基线创建`codex/TASK-002-continue`继续工作；若同名分支已存在，先检查是否属于本次任务再复用。Git推送和草稿附件下载需要本机GitHub认证；使用已有凭据管理器或由用户完成登录，不索要/复制PC-A令牌、不输出凭据。认证暂缺时继续可以独立进行的本地准备工作。

按顺序阅读：AGENTS.md（若存在）、docs/STATUS.md、STS2-Mod-Requirements-v0.1.md（内容版本0.3，文件名未改）、docs/DECISIONS.md（尤其DEC-004）、docs/tasks/TASK-002.md、docs/handoffs/TASK-002.md、docs/tests/TASK-002-PC-A-run-01.md、docs/TECH_BASELINE.md、docs/DEVELOPMENT.md、docs/COLLABORATION.md、docs/API_FINDINGS.md和相关代码。历史记录中的旧版本/旧验收安排不能覆盖最新决策与实际清单。

## 3. 本机环境安装和核查

先检测已安装工具，复用符合版本的工具，缺失再安装。项目基线：

- Windows x64、PowerShell 7、Git；VS Code和C#扩展可选。
- .NET SDK **10.0.402**，由global.json精确锁定、禁止roll-forward；项目目标仍为 **net9.0**，不能为了构建改成net10.0。
- .NET 9运行时基线 **9.0.21**，用于net9工具和测试；SDK自带的.NET 10运行时按官方对应发行安装。
- **Godot .NET/Mono 4.5.1**，不是不含C#的标准版；Godot.NET.Sdk 4.5.1已锁定。已有工程不需要重新套模板。
- ILSpy CLI基线11.1.0.9782，需要进一步核实游戏接口时使用。Alchyr.Sts2.Templates 2.5.3仅为历史模板工具，不是编译现有工程的必要条件。
- 唯一运行时框架 **BaseLib 3.3.2**，NuGet和锁文件已有固定引用，运行时ZIP由Install-ModDependencies.ps1按dependencies.json下载并校验。
- Steam已安装并由用户登录拥有游戏的账号；游戏App ID **2868840**。
- 游戏基线 **v0.107.1 / Steam build23811903 / 游戏提交59260271**；引用来自本机`data_sts2_windows_x86_64/sts2.dll`。PC-A该文件SHA256为`A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`。

PC-A用户目录中的Install-Toolchain.ps1、缓存和installed-manifest.json未随仓库同步，不要假设它们存在。必要时从微软、Godot和相应工具作者的官方来源下载固定版本、核验官方哈希，优先安装到本用户目录并记录实际路径。不要复制或上传游戏程序集、游戏PCK、凭据或存档。

先检查实际游戏release_info.json、Steam app manifest与程序集。若游戏已更新，不删除版本检查、不修改dependencies.json凑通过、不直接拿浮动最新版框架替代；先调查合法可用的匹配版本或明确提出兼容迁移范围并记录阻塞。账号登录及需要用户选择游戏版本时再请求用户操作。

## 4. 本地路径与环境检查

仅在不存在时复制local.settings.example.json为local.settings.json，填写PC-B自己的DotnetRoot、GodotExe（Mono控制台exe）和GameDir。不要覆盖已有有效配置。这个文件被Git忽略。

```powershell
if (!(Test-Path -LiteralPath .\local.settings.json)) {
    Copy-Item .\local.settings.example.json .\local.settings.json
}
# 实际填写本机路径并安装工具后，再运行：
. .\scripts\Enter-DevEnvironment.ps1
dotnet --version
.\scripts\Test-DevEnvironment.ps1
dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release
```

每个新终端先加载Enter-DevEnvironment.ps1，避免PATH误选旧.NET。PC-A为31项纯规则断言通过，PC-B需独立记录结果。Test-DevEnvironment只证明工具、引用编译和PCK导出，不代表游戏玩法通过。

在TECH_BASELINE里新增PC-B记录，不覆盖PC-A历史证据。若准备复用现成包，可先执行包验证，再继续环境安装和开发。

## 5. 构建、包验证与部署

方案A：从PC-B干净提交构建：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Build-Mod.ps1 -DeviceId PC-B
```

脚本统一使用ExportRelease/win-x64及locked restore；不要用随意的Debug/Godot编辑器还原改写锁文件。未提交开发改动可显式-AllowDirty，但dirty包不能当作干净提交交接包。若要复现S的确切源码，用独立干净目录检出S；最新文档/环境记录提交重建会得到新codeCommit和包，应如实另记，不能冒用S身份。

方案B：下载PC-A草稿包，须使用有草稿访问权限的GitHub账号：

- https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-872f3642b3aa121dcc6d
- Release ID417136064；ZIP附件655880524；SHA附件655880564。
- `Sts2Mod-0.0.3-v0.107.1-06e16c144727.zip`，108,468字节。
- SHA256：`59CDC0A4CFD074EC38C6B23EE8C4F98A129DF223C224691AC02A7053AAC9D2FC`。
- 包只包含Sts2Mod.dll、Sts2Mod.json、Sts2Mod.pck、build-info.txt，不包含BaseLib和游戏文件。

下载后用Get-FileHash核对SHA256，解压到被忽略的本地目录。退出游戏，再用实际包路径部署：

```powershell
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<实际包目录>/Sts2Mod' -InstallDependencies
.\scripts\Start-ModGame.ps1
.\scripts\Test-ModLoad.ps1 -LogFile '<本次启动输出的日志路径>' -CodeCommit '<该包完整代码提交>'
```

精确记录日志、codeCommit、包SHA和设备PC-B。仅用一个BaseLib 3.3.2实例，排查Workshop重复依赖。首次游戏原生Mod确认如需用户操作，说明原因；不绕过Steam校验。包内build-info必须保持.txt，原生会把mods目录下的JSON当Mod清单，不把本地化JSON散放进去。

## 6. 当前实现与PC-B首轮验证

已有：铁甲普通卡池的风起之刃1费6/9伤害、岩之守护1费7/10格挡、雷光突刺1费7/10伤害。风→岩额外4格挡；岩→雷对同一存活目标额外4攻击伤害，受原生修正，目标死后不转移奖励。两条连招共用每人每回合2次上限，触发后清序列；普通牌不打断，自动/额外结算不推进。状态按玩家分离。默认不赠牌、不改初始牌组。

PC-A记录：31项纯规则、44项真实游戏进程无界面断言通过，风岩旧版另有图形实测。0.0.3雷牌手动UI、实际奖励/商店获取、自然整局与完整退出读档尚未测；不能沿用旧版岩牌的获取PASS。

退出当前游戏，以`Start-ModGame.ps1 -TravelerTest`运行独立单人测试局；等待STS2MOD_TEST_COMPLETE及STS2MOD_LIGHTNING_COMPLETE，再运行：

```powershell
.\scripts\Test-TravelerRun.ps1 -LogFile '<本次日志>' -CodeCommit '<该包完整代码提交>' -Lightning
```

可加-Headless做逻辑集成，但图形检查必须正常启动。显式测试模式中，Shift+8打开原生控制台，`tc_hand 1 lightning`或`tc_hand 1 lightning upgraded`准备岩雷手牌；原有`tc_hand 1`准备风岩。仅单人玩家阶段可用，不改永久牌组，也不重置连招状态；下一组先正常结束回合。

测试run用ShouldSave=false，但游戏仍可能维护模组档案进度和Steam云同步。不要打开或改动用户既有run来做破坏性测试；不手工删除/覆盖用户存档。新增牌需补正常获取、界面和原生支持边界内的读档验证。原生普通存档不保存任意战斗中间点的手牌/Power/动作队列，不宣称支持未验证的中途连招恢复或重连。

## 7. 接下来开发范围

环境与现有回归通过后，继续TASK-002，按可验证的小增量推进，优先完成：

1. 顺风整备TC_C04：0费，抽1/升级抽2，消耗，风元素；雷→风额外抽1，并验证抽牌和选择上下文，不重复消费同一逻辑动作。
2. 磐岩进击TC_C05：1费，5伤害再3格挡，升级7伤害再5格挡，岩元素。
3. 雷鸣整备TC_C06：1费，5格挡再抽1，升级8格挡再抽1，雷元素。
4. 派蒙的应援TC_R01：罕见遗物，每位持有者每回合首次成功连招后额外3格挡；只奖励一次，重复遗物不叠加，不增加连招计数；加入正常获取池。

保留三条连招共用最多2次、基础→连招→派蒙顺序、预占状态防重入、原生命令结算、稳定ID、默认调试关闭等既有契约。雷→风目前未实现，不误认为已完成。实际接口先查当前合法游戏程序集和已有API_FINDINGS，不凭经验猜API。

补单人实际获取、图形状态、完整退出读档、额外回合、自动/重复、死亡/目标移除等相关测试，并做正常战斗试玩。双人专项延期，不强制要求此时第二账号/同时在线；未测保持NOT_RUN。旧任务中的14%额度只解释上次增量范围，不预设新会话也有相同限制；资源不足时优先交付完整、可验证的小增量。

## 8. 持续交接与远端同步

用户已授权本项目环境搭建、实现、构建、部署、单人测试，以及将代码/文档和开发包同步到现有GitHub仓库。直接完成已授权的必要步骤，不反复询问是否继续；只有缺少必要信息、认证或确实需要用户决策时才简短说明并请求协助。

完成每个可交付增量：提交源码S→干净构建→测试→记录报告与handoff→提交文档H→推送任务分支。记录本机工具、游戏/依赖、S、包SHA、设备、PASS/FAIL/NOT_RUN和真实证据；不让文档H冒充包内S。

新PR以当前开发基线为准，说明未合并依赖；推送后验证远端提交，创建PR后附加到会话。安装包/校验文件放草稿Release，原始日志和凭据留本地，不上传游戏资源或程序集。不擅自合并、强推或公开发行。交接结束留下本机可复现的环境、准确文档及明确未完成项。

开始时用简短摘要说明检测到的电脑、分支与环境缺口，然后立即执行同步和准备；环境/认证需要用户操作时，继续推进不依赖该阻塞的工作。
