# 0.3.12 轻量触发反馈验证

2026-10-22，PC-A；本地dirty开发候选，已部署，未提交或发布工坊。当前玩法沿用0.3.11；此前358项Cards检查是旧包证据，不作为本包全量验收。

## 包身份

- 基础HEAD：`27521c84f411aa456f49de1627ba5442cf9b8c2f`，dirty=true。
- 包目录：`artifacts/Sts2Mod-0.3.12-v0.107.1-27521c84f411-dirty/Sts2Mod`。
- ZIP SHA256：`5C52DED0EED43DE8E5A46502E5F9DEE19D26554BDD9F966A1B27854510F1482F`。
- DLL：`2105BDFED5C19083FCBE41A50A623FC760A8274865DFCA02000CFF427748566A`。
- PCK：`D37253D6D41634B50CF77CB7A7B868DA208BDCC6A0D2377724161F872FFA0509`。
- JSON：`17FDB82E1B20C04826425CBDED6B5CB3DDCA621C9B11C4A6237CD1F1A2F5C4F6`。
- 游戏v0.107.1 / Steam build23811903 / BaseLib3.4.7。
- 部署前副本备份：`.local/backups/20261021T1747415514110Z`；未改用户存档。

## 执行结果

| 检查 | 结果 |
| --- | --- |
| ExportRelease编译、Godot无GUI导入/打包 | PASS，编译0警告0错误 |
| ComboRules纯规则 | PASS，52项 |
| Visuals原生无GUI专项 | PASS，121项，其中13项QUIET新检查 |
| 未知错误 | 0；严格识别的既有原生无GUI诊断1项 |
| 测试进程 | PID29808，由统一入口结束，确认不存在 |
| 图形渲染/人工观感/联机/自然局/磁盘保存专项 | NOT_RUN |

命令：

```powershell
./scripts/Build-Mod.ps1 -DeviceId PC-A -AllowDirty
./scripts/Deploy-Mod.ps1 -PackageDirectory artifacts/Sts2Mod-0.3.12-v0.107.1-27521c84f411-dirty/Sts2Mod
./scripts/Invoke-ModTests.ps1 -Suite Visuals -CodeCommit 27521c84f411aa456f49de1627ba5442cf9b8c2f-dirty -TimeoutSeconds 600
dotnet run --project tests/ComboRules/ComboRules.csproj --no-restore
```

本地日志：`.local/logs/compact-build.log`、`.local/logs/game-20261021T1747417219044Z.log`及同名`.result.json`。隔离存档标识：`8dd0b76fc2e643dea6c27a90048fdc12`。

## 新增验证覆盖

连续24次连携只保留一个节点；固定缩放、短时长与小范围参数；无贴图/粒子子节点；自动清理且连携编码及战斗目标RNG不变。连续24次遗物Flash仍发送24个事件，但图标只保留一个短闪；不改原图标纹理、大小布局、缩放、位置或鼠标行为。三种专属遗物的角色头顶贴图工厂返回空，原生燃烧之血仍可创建原特效；短闪结束后规则/RNG不变，抽0不额外显示气泡。

以上是原生场景节点和逻辑检查，不证明最终屏幕观感。实际大小、色彩在不同分辨率/UI缩放下的辨识度仍需用户试玩确认。测试通过后仅更新文档，核心包未重新构建。
