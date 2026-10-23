# 0.3.17 工坊发布 / 2026-10-23

## 影响评估与授权

用户要求发布0.3.17以便游玩，取代此前“暂不发布”的安排。基线为main b62b559，玩法最终来源262e959；本轮仅同步工坊中英文说明、更新说明及封面/规则图版本号，不修改玩法或存档格式。调用链为Build-Mod→Invoke-ReleasePreparation（部署、Full、冻结）→官方ModUploader→Steam下载→工坊副本加载。固定更新条目3823282901，保持friends_only和BaseLib依赖3737335127。

风险为发布内容身份、重复加载与依赖匹配。使用独立干净工作树构建；发布候选执行一次Full及同包SavePrepare/SaveResume，保留所有断言与未知错误检查；上传后检查实际下载七文件、封面和相册图片哈希以及实际工坊加载。Full覆盖的Cards/Character/Visuals/事件不重复单跑；人工图形、真实联机、自然整局平衡及旧用户档迁移NOT_RUN。只处理本次游戏进程，备份本地与工坊副本，不接触用户存档。

## 已完成证据（持续补充）

- 干净核心3e6413fec2af3b2979d757621998eec315e836a4，dirty=false，PC-A；0.3.17，游戏v0.107.1/build23811903，BaseLib3.4.7。
- C#构建0警告0错误，Godot无GUI导入/导出成功。
- 核心ZIP SHA256：7178D1FB8F063C2797D0424288CA60DA83B9D1B88C264C7FD40B0C28FE75D5EA。
- 离线规则57、日志分类26、发布包装15、发布准备38、测试入口11项通过，共147项；WorkshopProbe编译0警告0错误。
- 官方ModUploader v0.2.0 Windows x64归档SHA256：2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。
- 发布前Steam查询条目可见性为FriendsOnly，订阅且已安装；本地与工坊BaseLib均为3.4.7。旧工坊两目录已移入.local/release0317/workshop-before，记录逐文件SHA256供还原核对；旧本地Sts2Mod由Deploy-Mod备份。

当前状态：0.3.17已更新原工坊条目3823282901，仅限好友；Steam已实际下载并验证加载，本机只使用工坊副本。

## 最终发布与验证

- 同一干净核心Full1076条断言完整PASS，其中事件352项；日志game-20261023T1121052209749Z.log。未知错误0，图形与联机NOT_RUN。
- 同包SavePrepare14项PASS，日志game-20261023T1141271071384Z.log；以新进程恢复SaveResume259项PASS，日志game-20261023T1141558825578Z.log。两个原生加载边界均核对事件模型持久化；受控六场连续战斗按四回合预算执行，非自然整局或旧档迁移验收。
- 玩家文档补齐0.3.17规则及历史验证描述，仅重新冻结文档，未重建核心。最终文档提交969b48947506631d5632a753e47cdb7abd56ddea，documentDirty=false，workshopDirty=false。
- 最终快照SHA256：F861BCA01ED0F1A4CB6E262B8672726851CC7D78D3217774ED4D5F75A4A11471。最终候选ZIP SHA256：A07CAB43A36B221BE3D175519D6092CEA3F7EAE4AEF9B305A9D48BA6FA080540。先前EE742EE开头的候选是文档同步前的未上传快照，不作为已发布产物。
- 官方上传器退出码0并返回Successfully uploaded，更新既有条目3823282901，未创建新条目；无须接受协议。上传后WorkshopProbe实时查询FriendsOnly。
- Steam原生订阅下载后，七文件逐一SHA256与最终冻结上传目录一致；安装时间戳1792755892，实际文件总量58614579字节。BaseLib实际工坊副本为3.4.7。
- 封面及rules相册图均HTTP200、PNG签名正确，逐文件SHA256与源图一致。封面022FF0D11C9911A02929BF9F4B9DD37981FFF60221E01293C5552E9EC95BB3EA；规则图397EE9BD5A3F0D0B958CF3FAC32FB0756A5F74CE1167F69FC94A10E209DF69B3。
- 实际工坊Load PASS，日志game-20261023T1145453475095Z.log；明确从workshop/content/2868840/3823282901加载0.3.17+3e6413fec2af3b2979d757621998eec315e836a4，BaseLib3.4.7、resource=ok，只初始化一次并到达主菜单，未知错误0、已知headless诊断0。
- 原工坊目录先按原哈希完整还原，再由Steam下载更新，未手工复制候选到Steam缓存。匹配工坊字节的本地BaseLib与Sts2Mod覆盖包已移入.local/release0317/local-overrides，旧Sts2Mod另由Deploy-Mod保留备份；游戏mods目录不保留重复副本。
- 全部本轮测试进程已结束；未启动图形游戏、未复制或修改用户存档。未跟踪平衡草稿保留不提交。

## 核心四文件SHA256
- Sts2Mod.dll：240D18CFCAF929F37A38DFDEAB7C30353790100D4148F850732FE38033C2D401
- Sts2Mod.json：0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A
- Sts2Mod.pck：10C847D97DFFF115958F1717089ACDE5E25FB3D54946769F3CBCDB91CF10A4B5
- build-info.txt：57259FA2969ED4AB87B4525D852A6312DCCF8A0DD901B68A3B17E24713F493F6
