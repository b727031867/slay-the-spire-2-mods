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
   - 新增 `tests/Test-IncrementalLog.ps1`（13项）。
5. **文档与版本**
   - 版本升至 0.3.19（csproj + `Sts2Mod.json`），`STATUS.md` 顶部与“当前记录”表格更新为实际状态，修正“发布准备运行 Full”的过期描述。
   - 工坊说明与变更注记改为 0.3.19，工坊封面/规则图的版本标注同步。
   - 更新日志与玩家指南首段同步。

## 验证

（执行后补充：核心哈希、套件结果、日志、未验证范围）

## 未验证范围

- 图形观感与逐帧手感（headless 只证明逻辑与节点状态）。
- 自然整局、旧档迁移、真实联机。
- Full 全量回归（用户未要求）。
