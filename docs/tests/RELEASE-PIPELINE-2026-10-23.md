# 状态入口与发布快照流程 / 2026-10-23

本次只调整文档和PowerShell发布流程，不改玩法、卡牌数值、游戏资源、依赖版本或工坊远端。既有未提交的可见性与WorkshopProbe改动保留，不作为本次新实现。

README、开发说明、协作说明、跨PC接续、TASK-011入口和工坊说明统一链接STATUS。STATUS只保留当前开发/发布身份、证据来源、待办与长期约束；版本演进由CHANGELOG和已有报告承载，历史发布报告不追溯改写。

Build-ReleaseCandidate把四文件核心、三份玩家文档、填好说明的工坊配置、条目ID、封面和规则图冻结到schemaVersion=2快照；清单记录文件SHA256、文档提交及dirty状态。Get-ReleaseSnapshot验证清单、身份、全部文件哈希、重定向目录/文件、工坊好友可见策略、固定ID与依赖、图片大小。Prepare-Workshop只读取已验证快照，并校验输出字节；同一代码提交可有不同快照工作目录，已有目录拒绝覆盖。旧PackageDirectory参数改为SnapshotDirectory，旧schemaVersion=1候选需要从原核心重新包装。

Invoke-ReleasePreparation顺序为：校验核心 → 部署 → 无GUIFull → 核心哈希复核 → 冻结 → 冻结核心与被测包复核 → 准备上传目录 → 保存.preparation.json。部署/测试失败或核心变化直接终止。结果为PREPARED和Uploaded=false，不调用上传器、不提交或合并Git，不改STATUS发布结论。磁盘专项与人工试玩另做；真实运行会按Deploy-Mod备份并保留候选部署，不自动恢复旧Mod。

离线检查均通过，共90项断言：

| 检查 | 结果 |
| --- | --- |
| Test-ReleasePackaging.ps1 | 15 PASS |
| Test-ReleasePreparation.ps1 | 38 PASS |
| Test-LogClassification.ps1 | 26 PASS |
| Test-ModTestEntry.ps1 | 11 PASS |
| 修改/新增的5个PowerShell文件语法解析 | PASS |

发布准备用例在.local/release-preparation-tests/3e045b214a6c486cb3c8ab429fd5c784中复制真实包装脚本，只替换夹具中的部署和游戏测试入口；覆盖冻结后源文档/图片/配置/ID变化、ZIP解压复用、各类快照篡改、策略字段重算哈希后的拒绝、已有目录保护、部署及测试失败、错误身份、非无GUI结果和测试期间核心变化。夹具保留在忽略目录；没有调用真实部署、游戏或Steam。

NOT_RUN：真实游戏Full、磁盘恢复、图形、联机、实际工坊上传和下载验收。本次没有重新构建核心包，历史0.3.14开发验收不能记为本次脚本接入的真实游戏验证。哈希用于意外改动检测，不是数字签名；正式上传前仍须按工坊说明复核实际上传目录，上传后验收FriendsOnly及Steam下载内容。
