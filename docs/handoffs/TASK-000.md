# TASK-000：最小加载工程交接

结论：本次授权的第1、2步已完成，PC-A已实际验证可重复构建、部署、真实游戏加载、移除和备份恢复。下一步是TASK-001玩法垂直验证，不能把本任务当成完整Mod验收。

```yaml
task: TASK-000
requirements: [FR-01, FR-08, FR-09-partial]
branch: codex/TASK-000
code_commit: af2bd55139fae1fec88c7862db31d9d5bc853060
build_pc: PC-A
next_pc: PC-A-or-PC-B
game_version: v0.107.1
game_build: '23811903'
dependency_versions: 'BaseLib 3.3.2; Godot.NET.Sdk 4.5.1; .NET SDK 10.0.402; net9.0'
artifact: Sts2Mod-0.0.1-v0.107.1-af2bd55139fa.zip
github_release_id: 417077297
github_asset_id: 655727331
sha256: 0E1349670CD0CD58435D556DE5D866EE47308375598C945B5ED73158D9DE2269
local_checks: 'PASS: repeated clean build, locked restore, identical DLL/PCK hashes, package inventory/checksum'
game_cases: 'PASS: load, restart/redeploy, remove, restore; NOT_RUN: gameplay/save/multiplayer'
known_limits: 'PC-B not connected; no cards/relics/combos; no public release or automated CI'
next_action: 'Use DEVELOPMENT steps on either PC; implement TASK-001 only after reviewing API_FINDINGS'
```

源码提交S只标识实际打包代码；本交接和报告是后续文档提交，不改写包的代码身份。

## 获取与部署

1. 拉取`codex/TASK-000`分支的最新交接记录。要原样重建本报告的源码，在工作区干净的前提下切到完整S（可使用`git switch --detach <S>`）；直接构建更新的文档提交会生成以该新提交命名的新包，须记录新校验值。
2. 按[DEVELOPMENT](../DEVELOPMENT.md)配置各自工具与游戏路径；PC-B须独立验证环境。
3. 从[GitHub草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-59ecdcfdca41046ed7cd)下载ZIP及SHA256文件。草稿需要登录拥有仓库写权限的账号，不是公开下载地址。
4. 核对本报告SHA256，将包解压到本机被Git忽略的目录（例如`.local/incoming/`），退出游戏后执行：

```powershell
. .\scripts\Enter-DevEnvironment.ps1
.\scripts\Deploy-Mod.ps1 -PackageDirectory '<解压目录>/Sts2Mod' -InstallDependencies
.\scripts\Start-ModGame.ps1
```

BaseLib不在ZIP中；依赖脚本从作者官方发行获取固定版本并校验。已有Mod目录先备份，未知内容或不匹配游戏版本会中止。首次原生模组提示允许后重启，脚本不会替用户改写确认设置。

## 预期

- 主菜单加载完成，原生“设置→游戏设置→Modding/模组设置”中有BaseLib和STS2 Mod - Technical Preview。
- 日志中恰好一条`STS2MOD_READY`，含S、`baselib=3.3.2 resource=ok`，并出现两个Mod初始化完成记录。
- `Test-ModLoad.ps1 -LogFile '<实际日志>' -CodeCommit '<S>'`通过。
- 部署、移除和恢复命令及日志收集见DEVELOPMENT；精确结果见[PC-A报告](../tests/TASK-000-PC-A-run-01.md)。

## 下一阶段要点

使用中性ID Sts2Mod不代表主题或宿主已确认。先落实主题/宿主草案是否采用，再开发风/岩两张牌和一条连招。原生`GameAction.Id`会在选择暂停后恢复时变化，先设计稳定逻辑动作关联；静态接口列表见[API_FINDINGS](../API_FINDINGS.md)。双人同步和正式卡牌获取仍待实测，PC-B尚未接入不影响PC-A继续开发。
