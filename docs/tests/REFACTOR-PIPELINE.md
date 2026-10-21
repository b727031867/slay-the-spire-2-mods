# 工程重构验证 / PC-A

当前为未发布工作区改动，不改变已发布0.3.5或私有工坊。此前文档整理的未提交改动继续保留。

## 实现范围

- Build-Mod只构建核心；Build-ReleaseCandidate接收PackageDirectory，只校验和包装。Get-ModPackage供包装与工坊准备共享，拒绝dirty、篡改、非四文件目录及清单版本冲突。
- release-info.txt区分原核心codeCommit/四文件哈希与文档提交、dirty和哈希；重新包装ZIP使用新SHA256，不沿用原ZIP身份。
- Invoke-ModTests按Suite统一无GUI启动、完成等待、严格验收、结构化结果与本轮进程清理；Start-ModTest和Test-ModRun允许分步操作。旧检查器保留，断言和未知错误分类不放宽。
- ComboPower保留原生Power状态适配及卡牌调用门面。ComboResolver保持动作预占、原生命令、实际收益计数、能力回调、罗盘的先后；ComboFeedback负责原生手牌刷新、气泡、派蒙闪烁与特效。

## 已完成离线验证

- ExportRelease/win-x64编译：0警告、0错误。
- 纯规则49项、日志分类26项。
- 发布包装15项：核心字节保留、原来源不变、文档来源、归档、相对路径解析、拒绝覆盖/dirty/篡改/版本及依赖冲突/额外文件。
- 测试入口11项：标准化结果和PID、六种未完成套件拒绝、未知错误拒绝、失败JSON、沙盒参数约束。使用合成日志，不计为真实游戏通过。

## 被测开发包

基线HEAD为63e48febcf91aec52d3a549530575b274b32d5b3，dirty=true，程序集身份为同一哈希加-dirty。开发ZIP SHA256为84FA0365A7A1CD3089FFEAA84246767F3CE5DC97FB27A6DD626E2C0D23B8AC4F；这不是已试玩发布包S，也不作为正式发布候选。

DLL SHA256：F455D24EBDD90DD6A0AAD364F2FD1D0CA732DB8753064FE581CFEF3AC3F81A3A。

PCK SHA256：4C1E6A523CBBEF4F44B73A5CE45B742AE0F8233F8F32A7C3699855FBD4685F6E。

本地C#源码快照摘要：9703CBF9CFE6E6316E11F8497D98C1C3AFEA8EDD8B4A5E03F6FA058D337C0A88；逐文件哈希保留在.local/refactor-source-snapshot.json。源码模型ID、数值和存档规则未改，卡牌调用门面保留。

真实发布核心S=628cd327cc1e...另做包装工具验证，四核心文件保持；生成的新ZIP SHA256为4B1CF6F78D4B6B0F1B4E6C8523ED2B8BC28AACCAED00EF0F60DA17D39AC8E8AD。该ZIP仅验证重包装工具，包含原发布二进制及当时工作区文档，不包含重构开发DLL，不作为新版本发布。

## 原生无GUI验证

游戏v0.107.1/build23811903、BaseLib3.4.7。下表是同一dirty开发包的独立运行结果，不能将断言数相加作为单次全量结果。每份日志旁保存标准化结果JSON。

| Suite | 结果 | 断言 | PID（均已结束） | 本地日志 |
| --- | --- | --- | --- | --- |
| Load | PASS | 0 | 15376 | game-20261021T0224556888651Z.log |
| Character | PASS | 59 | 35812 | game-20261021T0230246866121Z.log |
| Cards | PASS | 257 | 5532 | game-20261021T0231299999059Z.log |
| Full | PASS | 410 | 14320 | game-20261021T0248317795358Z.log |
| Visuals | PASS | 49 | 33440 | game-20261021T0258497961476Z.log |
| SavePrepare | PASS | 13 | 13716 | game-20261021T0259194340489Z.log |
| SaveResume | PASS | 250 | 28308 | game-20261021T0259481315740Z.log |

全部成功记录均经原有严格检查器验证，未知错误0；不修改Get-GameLogErrors分类规则。无GUI原生渲染器及退出诊断按现有精确规则单列，不声称所有原生日志没有ERROR。用户图形试玩、多人和自然Boss系统性平衡均NOT_RUN。

SavePrepare原生保存退出后重新部署同一开发包，SaveResume在新进程读取同一沙盒35c6bf58e232422e95aabd2a34ed287b，原生磁盘路径与Steam云隔离。连续场景6场23回合、82次出牌、48次连携、1次自然胜利，其余5场预算清理；5000生命、每场4回合、无手牌/能量注入，不是自然通关或Boss平衡验收。

所有测试PID均已结束，未启动图形窗口。原工坊7文件与测试前快照逐项SHA256相同，本地测试Mod和BaseLib已移入可恢复备份，原本地安装状态恢复。最终备份目录.local/backups/refactor-test-20261021T0248311694236Z；未上传工坊或改Steam订阅。

统一入口实际验证了启动、完成等待、日志拒绝、结构化结果、保存退出/恢复及本轮进程清理。分步入口保留供手动编排；默认旧启动输出保持兼容，PassThru返回原始Process对象以确保清理对象身份。

### 保留的失败记录

- 首次Full在300秒等待上限处超时，未完成套件，不计通过；进程已清理、安装已恢复。完整回归/恢复默认时限调整为1800秒，超时仍失败。
- 后续Full完成410项游戏断言，但严格日志检查拒绝3条Godot ScriptManagerBridge.SwapGCHandleForType的“Handle is not initialized”托管异常；日志game-20261021T0237084855854Z.log，PID12804已结束、原安装已恢复。该轮整体FAIL；不修改分类器或忽略未知错误，也不根据堆栈位置直接断言与本Mod无关。

同一开发包随后在新的Full进程中完成410项断言，并以未知错误0通过严格验收；句柄异常未再次出现，但原因未确定，保留为需关注的间歇性启动诊断，不宣称修复。
