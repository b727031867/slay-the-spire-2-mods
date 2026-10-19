# 双 PC 开发、测试与 GitHub 交接

本说明落实 [DEC-001](DECISIONS.md)：PC-A 和 PC-B 都可开发、构建、打包和测试，按任务接续工作，通过同一个 [GitHub 仓库](https://github.com/b727031867/slay-the-spire-2-mods) 同步成果。

当前PC-A已完成工具链安装与最小Mod加载冒烟，已提供本地构建与打包脚本；PC-B尚未接入。以下是接入与交接约定，当前包和上传状态以STATUS及对应任务交接记录为准。

## 1. 每台 PC 各自准备环境

PC-A 已核查为 Windows；PC-B 先按相同 Windows 工具链准备，接入时核实实际系统与版本。两台 PC 各自拥有独立 Git 工作目录、开发工具和合法游戏安装。

1. 在 PowerShell 7 中克隆仓库，并确认该设备能访问与推送自己的任务分支。
2. 安装 [TECH_BASELINE](TECH_BASELINE.md) 固定的 .NET SDK、.NET 9 运行时、Godot .NET、Git及所需编辑器工具。项目包含环境加载和检查脚本；PC-A 用户目录内的安装脚本与缓存没有包含在仓库中。
3. 只在尚无本地配置时复制 `local.settings.example.json` 为 `local.settings.json`，填写该设备实际的 DotnetRoot、GodotExe 和 GameDir。不要复制另一台 PC 的个人路径配置覆盖自己的配置。
4. 从该设备自己的游戏安装读取引用程序集，核实游戏分支/构建与依赖版本。
5. 执行环境加载及检查，将设备、实际版本和结果记入 TECH_BASELINE；游戏实测另记测试报告。

```powershell
git clone https://github.com/b727031867/slay-the-spire-2-mods.git
Set-Location slay-the-spire-2-mods
# 首次配置且文件不存在时，复制样例并编辑实际路径。
if (!(Test-Path -LiteralPath .\local.settings.json)) {
    Copy-Item .\local.settings.example.json .\local.settings.json
}
# 填好配置并安装工具后执行。
. .\scripts\Enter-DevEnvironment.ps1
dotnet --version
.\scripts\Test-DevEnvironment.ps1
```

两台 PC 的磁盘路径不必一致。`local.settings.json`、游戏程序集、存档、构建缓存与 `artifacts/` 不进入 Git；另一台 PC 不能通过拉取源码自动获得这些内容。

## 2. 开始工作与切换设备

开始工作先读取 STATUS、根目录需求书、TECH_BASELINE、相关任务和交接。确认本次设备、任务分支、HEAD 和负责人。

```powershell
git status --short --branch
git fetch origin
```

工作区干净、目标分支已选定且本地没有分叉时，才可执行 `git pull --ff-only` 同步该分支。若有未提交改动，先保留或提交；若本地与远程分叉，先检查差异并合并，不能为同步而强推或硬重置。

`main` 保存共享基线与已通过相应检查的成果；不因文件在main上就认定游戏功能已验收。正式玩法开发默认使用 `codex/TASK-xxx`，修复使用 `codex/BUG-xxx`。

切换设备前：

1. 完成当前可追踪的增量，提交并推送任务分支。
2. 更新 STATUS、任务和必要的交接记录，写明已经完成的检查、剩余工作与下一执行设备。
3. 需要对方直接复用测试包时，上传对应附件并记录包的确切来源和校验值；没有包则明确让接手设备从指定提交构建。
4. 接手设备先同步记录，再选择继续开发、构建或测试；设备切换不强制改变任务角色。

同一任务分支同一时间由一台 PC 主动修改。需要同时开发时使用不同任务分支并明确范围，完成后合并；不要两边各改一份同名文件后互相覆盖。

## 3. 源码与构建产物分别同步

| 内容 | 同步方式 | 记录要求 |
|---|---|---|
| 源码、需求、任务、交接、脱敏测试摘要 | Git提交与推送 | 分支、完整提交哈希 |
| 需要跨机复用的Mod安装包 | GitHub Releases附件 | 源码提交S、包名、构建设备、游戏/依赖版本、SHA256、附件位置 |
| 未批准公开的开发交接包 | 草稿Release附件 | 接手设备登录有仓库写权限的账号后访问，不记录为公开可下载 |
| 已获准公开的测试版 | 标为预发布的Release附件 | 明确单人/双人实测范围与未验证项；正式发布仍由负责人决定 |
| 游戏程序集、游戏资源、凭据、个人配置、存档 | 各PC本地保留 | 不上传到公开仓库或发布附件 |

当前最小技术预览可由`Build-Mod.ps1`本地打包，尚无正式玩法发布或自动构建上传流程；每次GitHub附件交接状态和地址记入对应handoff。若未来启用GitHub Actions，可通过其Artifacts交接临时包，并在交接中记录下载位置、登录条件和失效时间；不要将可能过期的附件作为唯一长期交接依据。

本地构建输出继续位于被Git忽略的 `artifacts/`。建议包名包含Mod ID、Mod版本、游戏版本和源码短提交号，例如 `<modId>-<modVersion>-<gameVersion>-<shortCommit>.zip`；实际扩展名与内部结构待正式模板验证后固定。

## 4. 提交与测试包必须可追溯

1. 提交源码与构建配置，得到真实代码提交S。
2. 从S的干净源码构建，记录本次设备、工具和依赖版本。不得把未提交源码构建的包标为S。
3. 生成包并计算SHA256；包中不含游戏引用程序集、个人路径与未获分发授权的素材。
4. 上传需要同步的包，在后续交接文档提交H中引用S、附件位置和校验值。H是文档提交，不替代被测代码身份S。
5. 接手PC下载后重新计算SHA256，与交接记录一致后再部署。

校验示例（以实际包路径替换占位符）：

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath '<下载的实际包路径>'
```

若接手PC从S重新构建，该包可能因构建环境或打包差异而有不同SHA256，须记录自己的构建设备和新校验值，不能沿用原包校验。需要两台PC测试完全相同的二进制包时，直接使用同一份已核验的GitHub附件。

## 5. 两台 PC 的测试记录

任一设备都可以先做单人测试和本地缺陷修复，PC-B未接入不阻塞PC-A继续开发。每份报告至少包含执行设备、代码提交S、包SHA256、游戏/依赖版本、用例、结果和证据来源。

双人联机需要两台设备同时具备可用客户端、账号和操作者条件，双方安装同一游戏分支/构建、Mod包及依赖。按需求书交换主客机测试，分别记录PC-A与PC-B承担的主/客机角色；拥有两台PC本身不等于联机测试已通过。

PASS、FAIL、NOT_RUN、BLOCKED分别记录。PC-A的环境或游戏PASS不能自动写为PC-B通过；本机编译与单人测试也不能替代双人实测。问题可在发现它的设备上建独立修复分支解决，生成新提交和新包后回归相关用例。
