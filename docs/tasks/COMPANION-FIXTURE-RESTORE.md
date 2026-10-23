# 同伴测试上下文恢复 · 2026-10-24

- 用户选择方案A，要求修复并提交，不重新测试。改动仅限测试夹具与严格日志检查器，不修改生产玩法、素材或正常存档。
- 根因：TestCompanionEvents依次重开五种原生角色对局并替换_player，正常返回后留下DEFECT对局；Full紧接着执行旅行者胜利测试。胜利测试只点击一次选项，面对三行原生DEFECT对话时只推进一行，未调用WinRun，最终等待结算屏超时。之前旅行者Character专项一行对话路径已通过。
- 方案A：五种同伴检查完成后，回主菜单新建无保存的旅行者单人夹具，并调用已有StartFreshCombat同步_player、_target、当前战斗及动态Power引用；断言角色、ComboPower所属、初始连携计数、目标对局和ShouldSave=false，再输出完成标记。不是把已销毁的原对局对象指回去。
- 防护：TestArchitectVictory入口明确断言旅行者身份；事件与角色/Full严格检查器要求对应新标记，避免以后用错误上下文或旧日志通过。没有循环点击其他角色对话来掩盖串扰。
- 影响范围：独立Events完成后返回的上下文，以及Full中Events→Architect顺序；Character新增前提断言。后续获准验证时应运行Full确认顺序问题消除，不能用Character独立通过替代。
- 本轮未构建、未运行任何自动或游戏测试、未部署或发布；旧Full失败记录保留，不改成PASS。仅检查代码差异与提交范围。

## zcode后续验证与主分支同步

- 用户提供zcode完成验证的记录，并授权推送远端主分支。本次仅复核现有产物，不重新运行游戏或构建。
- 对应干净核心：7cc932fb382640323cc141da27b4992f4f34fd05，dirty=false，包目录`.local/regression-worktree/artifacts/Sts2Mod-0.3.17-v0.107.1-7cc932fb3826/Sts2Mod`。
- Full PASS，1232项断言，1269.086秒；schemaVersion=2报告`.local/logs/game-20261023T1853336665603Z.log.result.json`及同名原始日志，四文件包哈希与LogSHA256已重新核对，严格Full日志检查器再次复核PASS（不启动游戏）。
- EVENT_companion_restores_traveler_fixture、VICTORY_traveler_fixture各出现一次，原失败点VICTORY_native_win_and_summary通过，原生结局完成标记summary=PASS；未知错误0。本次结果证明修复后的Events→Architect顺序通过，不覆盖或改写58c039a原FAIL。
- 本次只改测试夹具/检查器，不涉及持久化；按已完成Full覆盖不重复Cards/Character/Events或磁盘专项，人工UI/自然局/联机仍不宣称已验证。
- 修复已位于main，直接推送main及此验证记录即可；本次不更新工坊，工坊已发布核心仍为58c039a。
