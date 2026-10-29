# 2026-10-29 平衡更新公开发布

授权：用户在本聊天明确要求“不需要跑测试，发布到工坊”。本次按该直接发布指令执行，沿用既有已验证候选核心，不重建、不运行任何新游戏/离线测试；该指令不记为用户试玩成功，后续一般发布门槛不变。

范围：更新已有公开条目3823282901；仅修改上传说明和更新日志，保留0.3.17、BaseLib依赖及核心字节。旧上传说明仍是10月28日奖励修复，故从原核心重新冻结当前说明，不修改旧快照。

核心：7b0d63b155cdb5a97494fc790e5ea65f28ad7b59 / PC-A。DLL SHA256为6A24293F1F233842E55F0F0B050A9274730366D082022D9561D47B240BA1C1E9，PCK SHA256为9A405DF7A4B55E1B9990C5B04778A42E5A16925E082166E79EFD5B5DD434F9AD。既有Cards809项及Events410项通过报告见[原候选验证](BALANCE-20261029.md)。发布准备仅复核这些既有报告与哈希，不再执行测试。

影响/核对：工坊description、changeNote、冻结元数据和上传目录；检查说明与实现一致、JSON/public/条目ID/依赖、快照与上传字节。上传后核对远端Public、Steam实际下载七文件哈希及图片可读取性。游戏加载、Full、图形、联机、自然整局、旧档迁移与本轮用户试玩均NOT_RUN。

首轮上传：快照834D4191075786443C50CFD3A7E2196D3AB709AA01C04D2A9CB912E4B06CDFA7，官方上传器在提交阶段返回k_EResultInvalidParam；日志`.local/balance-20261029-release/upload.log`保留。独立inspect确认仍为原timestamp1793189337和118465148字节，未把失败记为发布成功。说明8688个UTF-8字节，超出Steamworks常量k_cchPublishedDocumentDescriptionMax=8000；压缩工坊内的旧版历史说明，完整历史仍保留仓库CHANGELOG。参考[官方接口](https://partner.steamgames.com/doc/api/ISteamUGC#SetItemDescription)及[Steamworks.NET常量](https://github.com/rlabrecque/Steamworks.NET/blob/master/com.rlabrecque.steamworks.net/Runtime/autogen/SteamConstants.cs)。重新冻结元数据，核心不变。

## 发布结果

- 2026-10-29重试成功：官方ModUploader v0.2.0发行包（ZIP SHA256：2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578）显式更新3823282901，返回Successfully uploaded；依赖未改。上传进程18276已结束。压缩后说明7322个UTF-8字节，最终实际上传配置为public。
- 最终快照：`artifacts/Sts2Mod-0.3.17-v0.107.1-7b0d63b155cd-candidate-20261029T0223523477445Z`。快照SHA256：64CF777ECE6845101F0A9D413452A10AB3BF61FCFDECB6F27AE79322BCA47A09；ZIP SHA256：D6BD6480D079718838387DB8A10F45560154223DFCD9DA8A8F4752F8AEDA3424。
- 实际上传目录：`.local/workshop/7b0d63b155cdb5a97494fc790e5ea65f28ad7b59-64CF777ECE68`。上传前核对条目ID、public、说明字节数和全部核心/文档/配置/图片哈希，与冻结快照一致，未编辑旧快照。
- 独立WorkshopProbe inspect确认k_EResultOK、k_ERemoteStoragePublishedFileVisibilityPublic。Steam原生下载state=5、118479291字节、timestamp1793240677；四核心文件和三份玩家文档共七文件SHA256逐项与快照一致，没有向Steam缓存复制文件冒充下载。
- 封面和rules.png相册图均HTTP200、SHA256与仓库源图一致；首次查询原相册地址为空，正常内容更新后已有有效地址，不需要另行修图。
- BaseLib原生下载版本3.4.7，三个文件与本机已验证固定依赖字节一致。本机本地mods/Sts2Mod及mods/BaseLib与下载内容核对后，移入`.local/backups/balance-20261029-published-local`作为可恢复备份，当前仅工坊副本生效；保留此前原工坊恢复备份，不改用户存档，不启动游戏。
- 证据：`.local/balance-20261029-release/`下upload-retry.log、inspect-after.jsonl、download-after.jsonl、download-hashes.json、baselib-download.jsonl、cdn-hashes.json。首次失败日志及查询保持原结果，不标成成功。
- Invoke-ReleasePreparation两次均只复用原Cards/Events报告校验同核心、环境和稳定日志，没有新测试或构建。Retention COMPLETE，保留最新三份独立快照（64CF777、834D419、ACA3065）；轮转删除262e959旧核心交接及423c800旧候选、ZIP/校验和已核对关联副本。源码、日志和必要恢复备份保留。
- 游戏Load、Full、图形、联机、自然整局、旧档迁移和本轮用户试玩NOT_RUN；按用户本次直接发布指令执行，不把该指令或文件核对记作试玩PASS。后续一般发布门槛不变。

发布条目：[旅行者·元素连携](https://steamcommunity.com/sharedfiles/filedetails/?id=3823282901)。版本仍为0.3.17，已包含本轮四项平衡调整。
