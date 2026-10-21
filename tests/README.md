# 验证入口

在PowerShell 7、项目根目录运行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release
.\tests\Test-LogClassification.ps1
.\tests\Test-ReleasePackaging.ps1
.\tests\Test-ModTestEntry.ps1
```

前两项验证纯连携规则和严格日志分类；包装用例使用合成文件，入口用例使用合成日志，不证明游戏加载。夹具留在忽略目录.local，便于检查。

真实游戏使用scripts/Invoke-ModTests.ps1，Suite为Load、Visuals、Cards、Character、Full、SavePrepare或SaveResume，给出实际包的程序集身份CodeCommit。入口强制Headless，等待并校验完成标记，成功或失败后只清理自己启动的进程。日志和结构化JSON留在.local/logs。

Full和SaveResume默认1800秒，其余默认300秒，可显式设置TimeoutSeconds。超时仍是失败。独立启动使用Start-ModTest，已有日志检查使用Test-ModRun。

磁盘流程先SavePrepare，等待原生退出后重新部署同一包，以返回的SandboxDirectory调用SaveResume。不复制用户存档、不启用Steam云。图形、多人、自然Boss平衡单列验证。

旧Test-*Run脚本作为严格检查器保留，统一入口不减少必需用例或忽略未知错误。

Visuals同时覆盖战斗演出：11块分层素材、原生动画转发、六套元素效果、普通攻防、实际格挡与掉血分支、节点限量/释放、倒地与复活。Cards/Full会复用此专项，并检查分层角色仍维持约312px的战斗高度。无GUI结果不证明贴图拼接、遮挡、动作自然度或实际画面观感，详见docs/tests/COMBAT-PRESENTATION.md。

Character包含篝火资源检查：原生RestSiteRoom加载旅行者自定义休息场景与透明贴图，确认休息/锻造按钮、选项气泡、镜像、命中框和退出释放。它不自动点击菜单，也不验证实际画面观感，记录见docs/tests/REST-SITE.md。
