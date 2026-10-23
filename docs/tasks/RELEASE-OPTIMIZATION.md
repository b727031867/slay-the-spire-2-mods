# 发布流程优化 · 2026-10-24

- 基线：3a514b5。最近两轮Cards各约867秒，构建约21秒，历史工坊上传约9–15秒；优化重点是避免重复验收，不削弱上传核验。
- 范围：Build-Mod默认只生成四文件核心目录，-Archive保留独立归档；Invoke-ModTests可通过-PackageDirectory绑定包与环境，新增统一Events入口；Invoke-ReleasePreparation支持显式所需专项及-TestResultFile复用。构建、测试与发布准备分别记录阶段/总耗时。
- 证据：可复用报告schemaVersion=2包含四文件SHA256、机器、游戏版本/构建/程序集、BaseLib三文件、日志SHA256。测试前后核对源包、实际本地部署和环境，停止仅本轮进程后固定日志；复用重新核对环境、日志和严格校验器及套件覆盖。旧schemaVersion=1报告不会自动补绑定。报告用于本地完整性追踪，不是数字签名。
- 保留：默认Full；用户明确免全量的低风险修复可显式选择专项；核心/快照哈希、friends_only、固定条目ID、依赖与图片策略、上传成功后的Steam实际下载核验均不变。PREPARED仍不代表已发布；不自动上传、不自动操作工坊。
- 影响/风险：发布工具链及测试入口，未改生产卡牌逻辑、素材和存档；本地/工坊BaseLib重复安装会拒绝绑定。阶段耗时是本轮实测，后续网络速度不可保证。中途加入新资源仍需要重新验证最终包，应在构建前汇总同一批改动。
- 必跑：发布准备/打包/入口的离线回归，包含复用成功、错误包/元数据/环境/日志/覆盖拒绝、运行中包或环境变化写FAIL；真实无GUI专项验证统一入口与报告绑定，再用同一报告实际冻结并准备目录，确认不重复启动游戏。
- 不跑：未改玩法，不重复Cards/Full、磁盘恢复、图形或联机；不上传工坊、不压缩或删减原素材。
- 离线结果：Test-ReleasePreparation76项、Test-ReleasePackaging15项、Test-ModTestEntry12项通过，共103项；PowerShell语法与git diff --check通过。合成证据/隐藏临时shell不代表原生游戏测试。
- 真实构建：干净源码0f8fe0c40c5ef0a1c038632ac60a2b5a86b0ceb5，默认未生成中间ZIP；总21.204秒（校验0.208、还原3.268、编译4.846、导入4.937、导出7.645、收尾0.294秒）。阶段记录`.local/artwork-release/artifacts/Sts2Mod-0.3.17-v0.107.1-0f8fe0c40c5e.timings.json`。
- 真实统一Events入口：PASS，事件361项；schemaVersion=2报告已绑定四文件、环境及稳定日志。测试过程124.973秒，日志`.local/logs/game-20261023T1709264352210Z.log`及同名`.result.json`。未知错误0，图形及联机NOT_RUN。
- 同包复用实测：使用以上报告、显式-Suite Events，未重新部署或启动游戏；PREPARED、ReusedTests=true、Uploaded=false。准备总6.107秒，其中包校验0.242、证据复核0.072、冻结并归档4.977、工坊目录复制核验0.813秒。结果`.local/optimization-reuse-smoke.json`；相较重新执行该事件专项省去了约125秒的测试。此测量不是工坊上传速度，也不是Full性能基准。
- 复用快照SHA256：E860FA547297202FDDF93F614380F03F2002A64BED2488F82C340776F61983E7；候选ZIP SHA256：D385013B5DB4B4FF3D30D5BEC8F02DD0E564381ECDE98A122DE52C52608137AC。仅用于验证准备流程，不作为完成全量验收或已发布声明。
- 测试结束后按备份哈希恢复原工坊副本、本地测试覆盖包移出游戏目录、仅本轮游戏进程已清理，正常存档不动。还原后的游戏/BaseLib环境哈希仍与新报告一致。不上传工坊，不推送远端。
