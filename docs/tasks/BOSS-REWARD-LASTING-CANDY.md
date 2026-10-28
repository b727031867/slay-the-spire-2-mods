# Boss击杀后奖励结算卡住（2026-10-28）

## 日志与根因

bugLog/godot.log:126900及godot2026-10-28T16.58.33.log:383186显示同一异常：LastingCandy.TryModifyCardRewardOptions将筛选后的单稀有度能力牌候选传给CardCreationOptions，仍使用BossEncounter概率，原生构造器拒绝。调用链经过CardFactory.CreateForReward、CardReward.Populate、CombatRoom.OfferRoomEndRewards及NCombatUi.ShowRewards。重载已完成战斗再次生成奖励，重复同一异常。日志证据指向奖励生成，不是1024伤害溢出。

## 改动前影响评估

- 基线：origin/main fa83047；事故包0.3.17。游戏v0.107.1/build23811903，BaseLib3.4.7。
- 修改点：仅替换LastingCandy奖励钩子内部的自定义候选构造调用；单稀有度候选改用Uniform，其余概率、候选筛选、重复候选回退、奖励标记、随机源和遗物触发条件沿用原生代码。
- 调用方：正常战斗奖励、Boss奖励、读取已完成房间后重新生成奖励；修复对使用此遗物的所有角色生效，不改全局CardCreationOptions校验。
- 资源/配置：无运行时资源、数值、存档结构或配置变化。测试加入Character现有夹具，并更新严格日志检查器必需标记。
- 风险：遗物局部兼容补丁依赖当前原生构造调用；若后续游戏改变该方法结构，替换数必须为1，否则明确报错。普通三张Boss奖励的概率不变；追加能力候选只剩一种稀有度时均匀选择。
- 必跑：构建、Character无GUI专项（含加载、奖励、原生胜利界面），新用例覆盖原生异常复现、真实遗物钩子单稀有度候选、混合/空候选、Monster/Elite/Boss与0/1/2/4战计数、奖励序列化后重新生成及重复Populate。
- 不跑：Full（用户未要求）、连携规则（未改）、Events及磁盘两进程专项（未改事件或存储格式）。奖励序列化重建不等于旧用户档或真实联机恢复验证。图形、联机、自然整局NOT_RUN。
- 发布：本次仅交付本地修复候选，不上传工坊。

## 验证

首次编译发现新增测试误将CardReward.Cards当作CardCreationResult集合，已按原生CardModel枚举修正。历史沙虫夹具两条nullable警告保留。

- 干净核心源码：0f77584e44bce8875fdd4bc52c638888458c523b，PC-B / DESKTOP-BAGBD69；构建、Godot无GUI导入及PCK导出PASS。
- Character：PASS，135条断言，含新增29条LastingCandy回归，81.334秒。原生能力候选被原奖励排除后仅剩Uncommon的精确场景通过；原生Boss三张基础奖励仍全为Rare；奖励序列化后重新生成及重复Populate通过；原生建筑师胜利结算界面通过。
- 加载身份/资源PASS，未知错误0；严格分类器识别1条已知Godot dummy渲染诊断，不算图形验收。
- 日志：.local/logs/game-20261028T1126286550437Z.log；同路径.result.json为schemaVersion=2，绑定核心、环境和日志哈希。日志SHA256：50374E4EDC97CE93A027BE6834A195CD811E4CA1FA2F1D5BFBD8E972272ACF7F。
- DLL SHA256：00CF424B9D3635A12005FB29A705FD5DCE149245460609016268FFCAFBAAA125。
- PCK SHA256：71E74F63D4E48D13FCD64AC0DC9924A7E6D2FC5C8EF37EA8CCC93308C022F2FD。
- 原生测试PID23812已清理；原工坊两个模组目录逐项哈希恢复，测试存档位于唯一隔离命名空间，正常用户存档未修改。
- Test-ModTestEntry：12项PASS；git diff --check通过。未改连携代码，不跑ComboRules；未运行Full、图形、真实联机、旧用户磁盘档恢复或自然整局。未上传工坊，尚无本候选用户试玩确认。
- 后续仅提交生成的脚本UID及文档，复用同核心测试证据，不重建核心。

## 本地交付

- 运行时及验证记录已快进合入main并推送；核心仍为上述0f77584，无重建。
- Invoke-ReleasePreparation使用Character同包报告复核核心、环境、日志及严格断言，PREPARED、ReusedTests=true、Uploaded=false。
- 冻结目录：artifacts/Sts2Mod-0.3.17-v0.107.1-0f77584e44bc-candidate-20261028T1128511051197Z；同名.zip为试玩包，同名.preparation.json为交付及轮转报告。
- ZIP SHA256：C20BB1B301E46405501A38C1DCBAC36B197B998BFC0C3809C4902F97CF31ABE4。
- 快照SHA256：60C68362E499736C6654413454613D1C0E494A80D5510064269FF8B237F756BB。
- Retention COMPLETE，当前PC保留最近3份交付，清理3项过期制品。临时tested-Sts2Mod及tested-BaseLib逐文件对照保留核心/恢复后的工坊安装核验哈希后删除；日志、测试证据及正常安装保留。游戏未保持运行。
- 本机正常安装仍为原工坊版；本次修复通过本地ZIP交付。真实联机双方均需使用同一修复核心；旧故障存档能否恢复仍需在该核心上试玩确认，未更改用户存档。

## 本次发布授权与影响评估

用户在本地修复包交付后明确要求“发布到工坊”，授权本次直接上传已验证0f77584核心，不等待额外试玩确认。此特定指令不记为用户试玩PASS，不修改后续一般发布门槛。

本轮仅更新发布说明与文档：保留0.3.17、原条目3823282901及public；不重建核心，不改存档。以原schemaVersion=2 Character报告复核核心与环境，重新冻结文档及工坊配置；上传前核对实际上传目录public、全部文件哈希和条目ID，上传后独立检查Public、Steam原生下载七文件哈希及两张图片。下载后的Load只证明工坊包启动，不重复玩法回归；Full、图形、真实联机和旧故障存档恢复NOT_RUN。官方ModUploader v0.2.0来源ZIP SHA256为2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。

## 工坊发布完成

- 原条目3823282901更新成功，独立WorkshopProbe查询为k_ERemoteStoragePublishedFileVisibilityPublic及k_EResultOK；依赖未变。
- 新文档快照：artifacts/Sts2Mod-0.3.17-v0.107.1-0f77584e44bc-candidate-20261028T1136075855861Z。快照SHA256：62F8538E2685AB8F4814B746EC5AA702F20D01FCCFE6C37F83FB12C1A25B80D2；ZIP SHA256：40BD2D860B105F520B1CABD8D869993404CF6CA98182D8D4957656E9DF2704D4。自动轮转COMPLETE，保留3份交付。
- Steam原生下载state=5、118450556字节、timestamp1793187396；七文件SHA256与实际上传快照逐项一致，未向Steam缓存复制候选文件冒充下载。
- 初次CDN检查：封面200、旧规则图链接404。仅替换相册index0为冻结源规则图，Steam提交OK；新规则图链接与封面均200，PNG签名及SHA256与源文件一致。二进制内容及公开权限未变。
- 实际下载工坊包Load PASS，未知错误0，日志.local/logs/game-20261028T1137505711580Z.log；测试PID50724已清理。本机保留唯一工坊安装，无本地覆盖副本。
- 证据位于.local/boss-reward-release：candidate.json、upload.log、inspect-final.jsonl、download-after.jsonl、download-hashes.json、repair-preview.jsonl、cdn-hashes.json、workshop-load.json。未重建核心，原Character135项证据保持有效。
- 用户授权直接发布不记为试玩通过；真实联机及旧故障存档恢复仍NOT_RUN。
