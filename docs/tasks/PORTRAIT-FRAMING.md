# 竖版卡图两侧留黑修复 · 2026-10-23

用户截图指出逆风牵制、雷霆连锁、乘隙雷击等卡图两侧留黑，要求修复并直接发布工坊，不跑完整回归。

最终状态：已修复并发布既有工坊3823282901，保持FriendsOnly与0.3.17。干净核心458356c的Visuals201项通过，实际Steam下载七文件与上传快照一致；PC-A仅使用工坊安装，未运行完整回归或图形游戏验证。

## 影响评估

- 基线：已发布素材核心e717acc；本地包含已通过Visuals191项的动态伤害预览修复，发布需一并包含。
- 根因：TC_C19–TC_C28直接引用1024×1536竖版原图；原生卡图区域250×190（25:19），既有横图接近此比例。原生卡框居中等比显示，竖图左右留空。
- 修改：每张新卡图添加Godot AtlasTexture资源，原图不改，显示区域Rect2(12,388,1000,760)，居中截取25:19；对应CustomPortraitPath改指向tres。Buff独立资源不改，不添加全局UI补丁。
- 范围：卡牌库、手牌、奖励、升级预览等均经CardModel.Portrait/BaseLib资源入口取得同一纹理；旧卡牌、卡框、玩法及存档ID不改。成本是竖图顶部和底部不进入卡框，原PNG完整保留。
- 必跑：构建导入/导出＋Visuals专项（含已有动态伤害预览检查）；新增10个原生卡牌节点检查，确认实际Texture为对应AtlasTexture、区域/尺寸正确且与原生卡框宽高比一致。
- 不跑：Full/Cards完整玩法回归、磁盘存档专项（用户授权不跑完整回归；未改共享玩法或持久化）；无GUI不代表图形观感与自然局验收。
- 发布：同一工坊3823282901，保持friends_only，版本沿用0.3.17；使用干净源码包，核对上传文件与Steam实际下载哈希。不重复创建条目。

原始10张PNG已逐一与云盘下载源核对SHA256，均保持一致。动态伤害预览修复纳入同一源码和包；原有Visuals191项记录仅作历史，本次候选将执行含卡框新增检查的Visuals。

实测校准：首次候选按近似4:3取景被新增严格检查拒绝；原生节点实测250×190、KeepAspectCentered，最终区域使用居中1000×760，即原生尺寸的4倍。初次失败日志保留，未降低检查精度。

## 最终验证与发布证据

- 源码458356cf792e55bc13e023cea8e13b05869ea199，dirty=false，PC-A；0.3.17，游戏v0.107.1/build23811903，BaseLib3.4.7。C#构建0警告0错误，Godot无GUI导入/导出通过。
- 同一干净包Visuals201项PASS，含10项原生卡框纹理检查和63项动态伤害预览检查，未知错误0、已知headless诊断0。日志game-20261023T1320198091914Z.log及同名.result.json；测试进程9492已由入口finally清理。隔离目录4eb29857df2947e19ceabbb865b76c14，正常存档未改动。
- 本次没有跑Full/Cards完整玩法回归或磁盘专项；用户明确要求免完整回归。无GUI原生节点几何检查不作为GPU/人工观感或真实联机验证。
- 初始近似4:3候选及补充几何日志候选均被严格新增断言拒绝，日志game-20261023T1317472349377Z.log、game-20261023T1318525818056Z.log保留。实测原生框250×190、KeepAspectCentered，校正为1000×760居中AtlasTexture；最终检查精确要求纹理尺寸=原生框尺寸×4，不放宽断言。
- 核心ZIP SHA256：458FCF2C60D0FFFBFE6F9197902C3664099DE495A4208A78E4D93EF264BD29E5。冻结候选ZIP SHA256：922BAA78DB6378755DB1723C6741026E26AF4DF798A2F96BA0E4874A0950ADE0。快照SHA256：204B2E222FB9D8AFDD59EC76FBE99928CD660390E469D659D29E53EB1757EF13。
- 官方ModUploader v0.2.0，归档SHA256 2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。上传目录.local/workshop/portrait-20261023-release实际workshop.json检查friends_only，mod_id.txt与显式-i均为3823282901；七文件上传字节与快照匹配。退出码0且返回Successfully uploaded，未创建新条目。
- 上传后WorkshopProbe两次实时查询FriendsOnly。Steam原生下载完成，state=5，安装时间戳1792761743，90348887字节；七文件SHA256逐项与上传快照一致，记录.local/damage-preview/portrait-download-hashes.json。首次45秒窗口结束时仍在下载，保留状态61记录，没有重复上传。
- 原工坊BaseLib按原哈希从.local/damage-preview/workshop-before还原；原卡牌安装仍在同目录的3823282901备份中。本次本地BaseLib/Sts2Mod覆盖包移入.local/damage-preview/portrait-local-overrides，PC-A仅使用Steam下载的已发布版本，没有手工复制新包进Steam缓存。
- 上传与远端日志：.local/damage-preview/portrait-upload.out、portrait-inspect-final.json、portrait-download.json；未启动图形游戏，未触碰用户进程与正常存档。

### 已发布核心四文件SHA256

- Sts2Mod.dll：6402FB7AEA8F5264FF675AE6DE0343C9BD8B8E17FB86D8FC8D2E216858C625AA
- Sts2Mod.json：0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A
- Sts2Mod.pck：825563DBB52814D1E313A9928CF1CFB69596BE11B5ED3BE26428D5C251114183
- build-info.txt：77A5588D950FDD30B09F12D728CA36FC85E4DC9A2906BA3899474474AC0C3F96
