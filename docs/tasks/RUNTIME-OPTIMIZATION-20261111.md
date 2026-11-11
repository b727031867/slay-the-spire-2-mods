# 运行时优化与稳定性修复（0.3.19）

日期：2026-11-11
分支：`codex/runtime-optimization-20261111`
基线：`main` @ `bb849fa`

## 影响评估

```text
基线：main bb849fa；游戏v0.107.1/build23811903；BaseLib 3.4.7；无本轮更早包
改动：
  Presentation/TravelerSkin.Update      → 顶点/骨骼变换缓冲复用 → TravelerRig.Advance 每帧调用
  Combo/ComboState.ClearCombat          → 战斗结束同时清空动作去重集合 → ComboPower.Clear、Debug 夹具
  Events/TravelerEventTypographyPatch   → 按容器合并刷新并跳过未变布局 → NEventOptionButton._Ready/Resized
  scripts/Read-LogChunk + Invoke-ModTests → 增量读取日志 → 仅测试入口
风险：局部（表现层与测试工具）；连携状态属共享战斗状态，故补 ComboRules 与原生 Visuals/Cards/Events 专项
必跑：ComboRules、无GUI Visuals（动画）、Events（字号）、Cards（连携集成）；离线脚本回归
不跑：Full（用户未要求）、SavePrepare/SaveResume（未触及序列化）、图形自动化与联机（需用户明确要求）
扩大条件：出现共享连携结算差异、原生调用方变化或专项失败
```

## 改动

1. **角色蒙皮动画**（`TravelerSkin`）
   - 顶点数组改为一次性分配、原地累加；骨骼变换、绑定矩阵改为预分配数组并按下标访问，权重在构造时索引化为 `(int bone, float weight)`。
   - 几何未变化时不再重写 7 个图层的 `Polygon`。
   - 新增 `ANIM_skin_update_reuses_vertex_buffer` 断言，防止回归为逐帧分配。
2. **连携去重集合**（`ComboState`）
   - `ClearCombat()` 清空 `_seen`，避免长时间战斗持续累积动作引用；跨回合去重语义不变（`BeginTurn` 不清）。
   - `SeenCount` 为诊断用属性，供 ComboRules 断言。
3. **事件字号适配**（`TravelerEventTypographyPatch`）
   - 同一容器在一帧内只调度一次延迟同步；容器、宽度、文案与已应用字号都未变时跳过重复测量。
   - 记录的是原生实际生效字号，避免自身触发的 `Resized` 反复ratchet。
   - 新增 `EVENT_option_font_stable_after_repeat_resize_*`，在宽度往返后校验字号恢复一致。
4. **测试入口日志轮询**（`Read-LogChunk.ps1` + `Invoke-ModTests.ps1`）
   - 只读取新增字节，只解码完整行；保留跨读取的多字节与 CRLF 边界；日志截断时从头重扫；进程提前退出前先检查未换行的最后一行。
   - 结束后的完整日志仍由 `Test-ModRun` 严格校验，失败判定未放宽。
   - 新增 `tests/Test-IncrementalLog.ps1`（14项）。
5. **文档与版本**
   - 版本升至 0.3.19（csproj + `Sts2Mod.json`），`STATUS.md` 顶部与“当前记录”表格更新为实际状态，修正“发布准备运行 Full”的过期描述。
   - 工坊说明与变更注记改为 0.3.19，工坊封面/规则图的版本标注同步。
   - 更新日志与玩家指南首段同步。

## 验证

### 干净核心

- 提交：`b30b732c91776622ea4125acde273ff11086712b`（构建目录 `artifacts/Sts2Mod-0.3.19-v0.107.1-b30b732c9177/Sts2Mod`，`dirty=false`）。
- 四文件 SHA256：
  - `Sts2Mod.dll` `574660EF613DE621082B53F2A4C44B0C5D79EC81CF81B411B5C2D633A4107930`
  - `Sts2Mod.pck` `2AE75A6921224D4691D4BB81C57BB8E44C56838A6838AA9475731EE3749C25BC`
  - `Sts2Mod.json` `65E95B41950B8CE7BEBBFAC0B734B914390407EB4D2E66FA1AD9EE6F6BD0A2ED`
  - `build-info.txt` `0C7DC991FFF52738ACAD27720B848EDC884D159F8D4F06A0166F2FA0D90AFBB9`

### 无GUI专项（同一核心）

| 套件 | 断言 | 结果 | 日志 | 日志 SHA256 |
| --- | --- | --- | --- | --- |
| Visuals（动画/蒙皮） | 445 | PASS | `.local/logs/game-20261111T0357237811061Z.log` | `8D6F91C6481893B7673D112702742C53F1A7D9E35D3C65619D110B50F0E895A8` |
| Events（字号适配） | 587 | PASS | `.local/logs/game-20261111T0400129264405Z.log` | `8876DAF05F05C47834F61C678CC0D64705B4D5C0D32C23CBFA22C9317B7C6951` |
| Cards（连携集成） | 1194 | PASS | `.local/logs/game-20261111T0402384634893Z.log` | `94E0C856A8E0BDA4B94E520F6B0CFDB541371EC415F433FFE9F5EF0BF170773D` |

- 三份日志的 `CodeCommit` 与核心一致，未知错误 0；Events 日志中的 3 条原生 headless 错误（RID/null）属既有分类器已接受类型。
- Events 的完成标记为 `STS2MOD_EVENTS_COMPLETE assertions=559`，最终包统计 587（含启动与收尾断言）。

### 离线回归

ComboRules 61、LogClassification 26、ModTestEntry 12、ReleasePreparation 80、ReleasePackaging 15、DeliverableRetention 36、IncrementalLog 14、文档链接 98，全部 PASS。新增 `tests/Test-IncrementalLog.ps1` 覆盖 BOM、CRLF 边界、截断重扫与未换行尾行。

### 代码审查

`reviewer` 独立审查结论为无 blocker/major；两条 minor（polygon 写入缺少回归断言、BOM 边界未覆盖）已修复并复测（提交 `5c6b95f`）。`reviewer-pro` 深度审查因子代理账号额度报错（`subscription_sharing_usage_limit_exceeded`）未能执行，本轮无资金/事务类改动，风险主要集中在表现层缓冲复用，已由专项断言与 Visuals 套件覆盖。

### 冻结与发布准备

- `Invoke-ReleasePreparation` 复用上述三份报告：`Status=PREPARED`，`RequiredSuites=Visuals,Events,Cards`。
- 快照：`artifacts/Sts2Mod-0.3.19-v0.107.1-b30b732c9177-candidate-20261111T0424182940774Z`
  - `SnapshotSHA256` `31858A81DDAF75C0E847AB4BFF2B3F3139C68F77AB179DE8B8E2AC386275E7C1`
  - `CandidateSHA256` `31D41882EC698B295E1A2F58531AAD8DD535166A4CA0DCA4F5093EA26E24AD0B`
- `Retention=COMPLETE`，保留最近 3 份交付。

### 工坊发布与远端核验

- 上传工作目录 `.local/workshop/b30b732c91776622ea4125acde273ff11086712b-31858A81DDAF`；preflight 确认 `visibility=public`、`mod_id=3823282901`、依赖 `3737335127`、说明 UTF-8 字节数 `5641`（< 8000），content 七文件与配置/图片哈希全部匹配快照。
- 官方 ModUploader v0.2.0：`upload_exit=0`，日志含 `Successfully uploaded ... with id 3823282901`。日志尾部 `k_EItemUpdateStatusInvalid` 为既有现象，以远端与下载核验为准。
- `WorkshopProbe inspect 3823282901`：`k_ERemoteStoragePublishedFileVisibilityPublic`、`k_EResultOK`、consumerApp `2868840`，标题正常。
- Steam 原生下载：`installed=True state=5 bytes=144093157`，目录 `E:\SteamLibrary\steamapps\workshop\content\2868840\3823282901`；七文件 SHA256 与快照逐一一致（`downloaded_files=7 mismatches=0`，记录见 `.local/download-hashes-0319.json`）。
- 封面图 CDN：HTTP 200、49995 字节，SHA256 `452B5B899B2FB29F82C471381539A8D71D7370E695A749898378E3BAB14C74EA`，与上传源一致。
- 规则图 CDN：裸 URL 返回 404，带 Steam 图片处理参数可下载（`?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false`），返回 1280x720 PNG。Steam 会重新编码预览图，故不做字节哈希比对：逐像素比较平均通道差 0.788、差值 >10 的像素占 1.57%，缩略图相关性 1.0000（对照“规则图 vs 封面”为 0.1248），判定为同一张图。
- 本轮未记用户试玩 PASS：试玩发生在发布之后，按规则不计为发布前审批证据；发布依据是用户对本次方案的直接指示。

## 未验证范围

- 图形观感与逐帧手感（headless 只证明逻辑与节点状态），尤其蒙皮复用后是否有视觉残留。
- 自然整局、长战斗内存表现、旧档迁移、真实联机。
- Full 全量回归（用户未要求）。
- 用户试玩（发布后进行）。
