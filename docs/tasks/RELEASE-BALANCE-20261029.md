# 2026-10-29 平衡更新公开发布

授权：用户在本聊天明确要求“不需要跑测试，发布到工坊”。本次按该直接发布指令执行，沿用既有已验证候选核心，不重建、不运行任何新游戏/离线测试；该指令不记为用户试玩成功，后续一般发布门槛不变。

范围：更新已有公开条目3823282901；仅修改上传说明和更新日志，保留0.3.17、BaseLib依赖及核心字节。旧上传说明仍是10月28日奖励修复，故从原核心重新冻结当前说明，不修改旧快照。

核心：7b0d63b155cdb5a97494fc790e5ea65f28ad7b59 / PC-A。DLL SHA256为6A24293F1F233842E55F0F0B050A9274730366D082022D9561D47B240BA1C1E9，PCK SHA256为9A405DF7A4B55E1B9990C5B04778A42E5A16925E082166E79EFD5B5DD434F9AD。既有Cards809项及Events410项通过报告见[原候选验证](BALANCE-20261029.md)。发布准备仅复核这些既有报告与哈希，不再执行测试。

影响/核对：工坊description、changeNote、冻结元数据和上传目录；检查说明与实现一致、JSON/public/条目ID/依赖、快照与上传字节。上传后核对远端Public、Steam实际下载七文件哈希及图片可读取性。游戏加载、Full、图形、联机、自然整局、旧档迁移与本轮用户试玩均NOT_RUN。

首轮上传：快照834D4191075786443C50CFD3A7E2196D3AB709AA01C04D2A9CB912E4B06CDFA7，官方上传器在提交阶段返回k_EResultInvalidParam；日志`.local/balance-20261029-release/upload.log`保留。独立inspect确认仍为原timestamp1793189337和118465148字节，未把失败记为发布成功。说明8688个UTF-8字节，超出Steamworks常量k_cchPublishedDocumentDescriptionMax=8000；压缩工坊内的旧版历史说明，完整历史仍保留仓库CHANGELOG。参考[官方接口](https://partner.steamgames.com/doc/api/ISteamUGC#SetItemDescription)及[Steamworks.NET常量](https://github.com/rlabrecque/Steamworks.NET/blob/master/com.rlabrecque.steamworks.net/Runtime/autogen/SteamConstants.cs)。重新冻结元数据，核心不变。

结果：待重试上传及独立远端核对。
