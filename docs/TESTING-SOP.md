# 按改动影响范围验证

自2026-10-23起，日常开发默认执行“核心检查＋受影响专项”，不默认全量回归。保留已有用例和严格检查器，缩小每次执行集合，不通过删断言、降低阈值或过滤未知错误提速。

## 1. 改动前：写一份简短影响评估

先看实际差异、相关调用方与共享状态，再在任务记录中填写下方模板；小改动直接写在开发记录中，不必另建报告。评估覆盖实现、调用方、资源/配置、测试夹具及日志检查器。

```text
基线：<已验证提交/包、游戏和依赖版本；没有则写无>
改动：<文件/符号 → 行为 → 直接调用方/共享状态>
风险：<局部 / 跨模块 / 存档 / 工具链；理由>
必跑：<核心检查、受影响专项及新增/修复用例>
不跑：<被其他套件覆盖或不受影响的范围；理由>
扩大条件：<发现额外调用方、接口/共享状态变化、失败或基线失效>
```

影响沿“修改点 → 调用方/共享数据 → 玩家行为 → 对应测试”追踪，不能仅按文件目录选测试。找不到调用边界或覆盖证据时扩大验证，不用“只改了一行”作为低风险依据。

## 2. 核心检查：按交付物选最小集合

| 本次交付 | 核心检查 |
| --- | --- |
| 纯文档、注释 | 差异、链接、命令与现有实现一致性；不构建、不启动游戏 |
| 游戏代码、运行时资源、模组清单 | 构建成功＋加载身份/资源/未知错误检查＋本次行为专项 |
| 连携相关玩法代码 | 上述检查＋离线ComboRules；它只验证ComboState，不能替代原生战斗集成 |
| 脚本、发布工具、日志检查器 | 对应离线脚本回归；只有改到实际加载/部署行为时才补真实无GUI验证 |

专项已调用Test-ModLoad时，复用其中的加载证据，不另外启动Load。Load本身只证明进入主菜单与加载身份，不证明出牌、回合或战斗结束正确。

## 3. 影响范围 → 当前可用入口

| 修改范围 | 默认验证 | 必须扩大时 |
| --- | --- | --- |
| 独立卡牌效果、数值、升级、能力、随机追击、卡图 | Cards＋该改动的正向/边界用例 | 改动共享连携结算、回调顺序、战斗生命周期时Full；涉及事件契约时补事件专项 |
| 元素图标、悬停提示、战斗动作/特效 | Visuals | 同时改变真实伤害/格挡/抽牌或共享钩子时按玩法范围扩大；无GUI不证明观感 |
| 选人、起始牌组/遗物、角色获取、商店/篝火资源 | Character | 跨到卡牌/事件结算时补对应专项；共享状态变化时Full |
| 四事件选项、奖励、契约 | TravelerEvents专项 | 永久元素、契约/牌组/遗物持久化变化时加SavePrepare/SaveResume；共享战斗规则变化时Full |
| ComboState/Resolver、共享元素状态、触发上限、回合/战斗重置、共享Harmony补丁 | ComboRules＋Full | 涉及保存/恢复时再加磁盘专项 |
| 序列化、模型ID/注册、永久元素、原生存档路径/退出恢复 | 相关专项＋SavePrepare/SaveResume | 注册或生命周期影响全局时Full；旧用户档兼容需要独立用例，当前磁盘夹具不证明迁移 |
| 日志分类/测试入口 | Test-LogClassification / Test-ModTestEntry中受影响项 | 修改真实启动、等待、退出或隔离行为时补相应真实无GUI路径 |
| 冻结/打包/工坊目录 | Test-ReleasePackaging / Test-ReleasePreparation中受影响项 | 改动包内运行时文件或部署行为时补加载/对应专项 |
| 游戏/BaseLib/Godot版本、构建导出链、广泛重构或无法界定影响 | 环境检查＋构建＋Full＋磁盘专项 | 保留已知兼容风险；人工与联机仍仅按用户要求执行 |

表内“＋”取并集，已覆盖的套件只跑一次。修Bug至少保留一个能识别该Bug的回归用例；非平凡新逻辑补正向与关键边界检查。已有用例足以证明时直接复用，不为数量增加重复测试。

当前入口粒度是套件，尚无单卡/单用例过滤。Cards虽然执行角色夹具和视觉专项，focused检查器不等价于Character的完整验收；同时改角色获取/注册时仍应补Character或Full。Events已接入统一入口。不要把尚未实现的细粒度筛选写成已支持。

## 4. 执行：开发、集成、发布分别处理

1. 开发中先运行受影响的离线检查，改动稳定后构建/部署一次，再跑选定的无GUI专项。不要每次编辑都重建、部署和Full。
2. 同一批相关改动可合并验证一次；覆盖范围取整批改动的并集。新提交、分支合并或冲突修复后重新评估相对已验证基线的累计差异，不能只看最后一次提交。
3. 若失败，先保留失败日志并修复根因，再重跑失败范围及修复新增的影响范围。超时、缺完成标记、未知ERROR均不算通过，不能靠减断言或放宽过滤转绿。
4. 每个开发任务使用独立codex/分支，选定范围通过后才合入main；纯文档使用一致性/链接检查。共享机制、依赖/工具链升级或范围不明时执行Full。Full包含大量专项，不再额外串行跑Load、Visuals、Cards、Character和事件专项。Full不包含两进程磁盘恢复。
5. 正式发布候选默认保留一次Full。多个小功能集中到候选验收，避免每个小功能各跑一次。磁盘专项按累计改动范围补充。Invoke-ReleasePreparation默认Full；用户明确免完整回归的低风险修复按累计影响显式传入-Suite，不自动推断范围。
6. 同一核心四文件哈希、游戏/依赖/相关环境一致，且现有报告覆盖全部改动时，可复用通过证据；文档提交、换分支名称或重新包装不使核心证据自动失效。优先在Invoke-ModTests传-PackageDirectory生成绑定包和环境的schemaVersion=2报告，再向Invoke-ReleasePreparation传-TestResultFile复用。复用只重新检查包、环境、日志与覆盖，不部署或重跑游戏；旧报告不自动补绑定。独立Build-ReleaseCandidate＋Prepare-Workshop仍可用于已审阅的冻结操作。
7. 同提交重新构建、测试期间核心变化、环境变化或证据不全，均不能仅凭提交号复用通过结果，须按变化重做相应验证。跨PC证据保留原设备归属；需要宣称另一台PC加载通过时，在该PC跑Load，不重跑无关专项。
8. 合入main后提供本地候选供用户试玩，记录核心SHA256和用户明确的成功反馈后才发布工坊；未确认或失败时不上传。Full PASS和PREPARED不能替代用户试玩，核心变化或重新构建后重新验证并确认新包。仅文档重新包装可沿用未改变核心的确认，仍须核对新快照与上传字节。

运行时文本/翻译/资源在PCK内，不能当成纯仓库文档跳过构建。仅更新玩家说明或工坊封面时，复核新快照/上传字节、配置及friends_only，无需重跑未改变核心的玩法测试。

## 5. 使用现有命令

统一入口支持Load、Visuals、Cards、Character、Events、Full、SavePrepare、SaveResume，专项按评估选择一个或多个：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
# 连携相关改动的离线核心规则检查
dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release
# 以下示例是局部卡牌改动，不再额外跑Load和Visuals
.\scripts\Invoke-ModTests.ps1 -Suite Cards -CodeCommit '<实际包S>'
```

事件专项优先使用`Invoke-ModTests.ps1 -Suite Events -CodeCommit '<包S>' -PackageDirectory '<核心目录>'`，可生成同样的复用证据；以下手动入口保留用于排查，在同一PowerShell会话中等待标记、严格检查并始终清理本次进程：

```powershell
$launch = .\scripts\Start-ModGame.ps1 -Headless -TravelerEvents -PassThru
try {
    $clock = [Diagnostics.Stopwatch]::StartNew()
    do {
        $text = if (Test-Path -LiteralPath $launch.LogFile) {
            Get-Content -Raw -LiteralPath $launch.LogFile
        } else { '' }
        if ($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR') { throw 'Event suite failed; inspect log.' }
        if ($text -match '(?m)^STS2MOD_EVENTS_COMPLETE ') { break }
        if ($launch.Process.HasExited) { throw 'Event process exited before completion.' }
        if ($clock.Elapsed.TotalSeconds -ge 300) { throw 'Event suite timed out.' }
        Start-Sleep -Milliseconds 500
    } while ($true)
    .\scripts\Test-TravelerEventsRun.ps1 -LogFile $launch.LogFile -CodeCommit '<实际包S>'
} finally {
    if (!$launch.Process.HasExited) { $launch.Process.Kill(); $launch.Process.WaitForExit() }
    Write-Host "Test log: $($launch.LogFile)"
}
```

磁盘两进程操作、构建与干净包要求仍见[开发说明](DEVELOPMENT.md)。所有游戏验证遵守[AGENTS.md](../AGENTS.md)：无GUI、唯一隔离档、无测试云后端，只清理本轮进程。

## 6. 收尾：记录覆盖范围，不追求断言总数

在原任务记录中追加：实际提交S/dirty、核心哈希、执行设备与环境、所选套件及结果/日志、复用证据、跳过理由、遗留风险。未执行写NOT_RUN；环境阻塞写BLOCKED；未受影响与尚未验证需要区分。

图形、人工交互、联机、自然整局和平衡、旧档迁移分别记录，不能从headless PASS推断。2026-10-24起，用户试玩成功是工坊发布门槛，不阻塞提交、合并或打包；AI图形测试和联机仍须用户明确要求，不自行运行。收尾按[开发说明](DEVELOPMENT.md#制品保留与清理)仅保留最近3份可交付制品并清理已完成分支。暂不发布工坊的指令仍有效。

后续只有套件内部耗时持续成为瓶颈时，再考虑拆出小型原生战斗Smoke或单卡过滤；届时同步启动参数、夹具、严格检查器和入口测试。当前先复用现有套件，不新增自动推断依赖的测试平台。
