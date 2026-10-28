# 旅行者内容结算同类问题排查（2026-10-28）

用户在Boss修复发布过程中要求排查其他卡牌、遗物、事件，并明确范围为旅行者Mod内的其他内容。

## 改动前影响评估

- 基线：main 18b183e，已发布核心0f77584；游戏v0.107.1/build23811903、BaseLib3.4.7，PC-B。
- 检查范围：30张正常牌池模型、归途诅咒、5种能力、全部旅行者遗物及四个主题事件；追踪生成/筛选/选择候选、战斗结束后钩子、未领取奖励重建和原生奖励接口的兼容。另查原生卡牌、遗物、事件对旅行者小牌池的筛选假设。原生源码来自本机同哈希sts2.dll，临时反编译位于.local/settlement-native，不入库、不分发。
- 已确认问题：原生TheFutureOfPotions交易会生成同稀有度、同牌型的三张不同牌。旅行者Rare Attack及Rare Skill各1张，Uncommon Power仅2张，原生CardFactory排除已生成候选后无牌可抽并抛异常；Trade已先丢弃药水。与上次LastingCandy同属筛选后候选池不满足奖励API前提。
- 继续沿同一生成入口发现SealedDeck.ChooseCards固定生成30张不同奖励牌，旅行者只有28张（两张Basic不会参与普通奖励稀有度抽取），取尽后同样抛异常。仅适配该异步方法内部工厂调用：先应用原生牌池修改，再将候选数限制为当前可用的不同正常奖励牌数量；仍沿用原生选10张入牌组、排序、无升级及移除潘多拉盒子的流程。其他角色不进入分支，不修改全局工厂。
- 最小修复：仅旅行者的该事件按当前解锁牌池和单/多人约束生成药水对应牌型，只随机选择至少有3张同稀有度牌的类型；沿用原生药水→稀有度映射、每瓶一次事件RNG、类型缓存、选项文案、药水丢弃、奖励与升级逻辑。不修改其他角色、全局CardFactory或存档格式。
- 调用方/共享状态：PotionToCardType getter被初始选项与交易过滤器使用；首次初始化缓存，后续读取同一字典，不重复消耗随机数。补丁不改事件外生成逻辑。
- 必跑：构建＋Events（含新失败复现、类型稳定、三档药水原生奖励屏和完成）＋Cards（卡牌、能力、旅行者遗物和最后目标击杀边界）。专项复用加载身份、资源及未知错误检查。严格检查器增加必需标记，不删原断言。
- 不跑：Full（未授权），ComboRules（未改连携规则），两进程磁盘（未改存储格式）。无GUI不证明真实联机、画面观感、旧用户档恢复或自然整局。
- 发布：此前Boss修复0f77584已发布；用户在“发布到工坊”的进行中任务内追加“如果有就一起修复”，本轮定向验证后将补充修复一并更新到同一公开条目。该指令按本次发布任务的范围追加执行，不记为任何核心用户试玩PASS，也不改变后续一般发布门槛。新核心必须重新构建并通过本轮专项，不能复用0f77584的玩法报告。

## 静态检查结论

- 旅行者自身没有其他使用非Uniform构造单稀有度自定义CardCreationOptions的调用；LastingCandy兼容修复已覆盖该遗物所有房间来源。
- 未领取的邮件使用原生可序列化牌池，临时增加OptionCount由finalizer恢复，已有领取/跳过/重复回调检查。
- 四主题事件直接选择具体候选，未复制LastingCandy的稀有度概率错误。随机池取样前有非空边界；引路人无可用遗物时提供离开选项。元素调律三元素候选来自固定完整主题池，各元素均非空。
- 卡牌击杀后连携、回响及罗盘有战斗结束/目标死亡保护；当前Cards专项已有末敌连携击杀回归。原生药水/发现类战斗生成使用GetDistinctForCombat按可用数量取样，不强行要求不足的三张。
- 原生事件/遗物常见按稀有度奖励：旅行者Common10、Uncommon13、Rare5，满足常规3选1与SeaGlass每稀有度5张；ScrollBoxes要求4普通/2罕见，当前池满足。InfestedAutomaton只生成1张，能力与0费候选均存在。TheFutureOfPotions同时筛选稀有度与类型的3选1是本轮确认的新缺陷。

## 验证

- Cards：中间核心6ba3489847748d8bfecb0eeb6e652edb71adedb1，754条断言PASS，868.830秒，未知错误0。日志.local/logs/game-20261028T1147111949965Z.log，同路径schemaVersion=2报告；日志SHA256为191F2FA71D416F35E60865282A6D4B8949D3C7183ED9DE3096C7FF03D8FB9E64。覆盖旅行者卡牌、5种能力、专属遗物、原生普通/Boss奖励、商店、目标死亡、末敌连携击杀及LastingCandy既有回归。
- 最终核心0fc779d6e763432a265224436b86765ecaa87a6f，PC-B构建/无GUI导入/PCK导出PASS。6ba到最终核心未修改Cards、Powers、Relics、Combo任何实现；增量为SealedDeck局部兼容和测试/UID/文档。Cards旧报告仅证明其原包，未伪装成最终哈希报告；增量以最终包Events及加载覆盖，不重复无关卡牌场景。
- Events：最终核心410条断言PASS（其中382条事件断言，另含28条选人夹具断言），134.616秒；包含20条新增回归。三种不足候选的原生失败已精确复现；常见/罕见/稀有三档药水真实事件房间、稳定牌型、三张不同升级奖励、药水消耗、事件结束及重复回调通过。SealedDeck原生30张取空失败复现、28张不同候选、选10张入牌组、PrismaticGem扩充后30张以及模拟铁甲保持30张均通过。SealedDeck测试执行原生选择回调，不声称验证了自定义模式自然整局。
- 最终Events日志：.local/logs/game-20261028T1204482009826Z.log，同路径schemaVersion=2报告绑定最终核心与环境。日志SHA256：AFC9C170E0A443155BB615B0AE007E7AC683E60C34FA81FAC9640098A7FA225A。未知错误0，严格分类器识别6条已知Godot无GUI诊断，图形NOT_RUN。
- 最终DLL SHA256：446FDBCC1888888A7F040B0164483A12B6A30077C7742C054406E268DD9444C5；PCK SHA256：14CB1E41C3414C4AB59A10F2BE2DB1C5C3F0274950C67B15E39F5EC5776712B1。
- 首次SealedDeck编译发现本机Harmony没有AsyncStateMachineMoveNext辅助API，改用标准AsyncStateMachineAttribute定位原生MoveNext，最终构建通过；未增加依赖。历史沙虫夹具两条nullable警告保留。
- 6ba的中间Events进程47984在完成前主动结束，由包含两项修复的最终包替代；原始报告保持提前退出FAIL，不记PASS，取消原因见.local/reward-audit-superseded-events.json。运行6ba时使用其匹配的严格检查器，最终检查器额外强制SealedDeck标记，未删减旧断言或放宽错误分类。
- Cards进程48836、最终Events进程17052均已清理；两轮测试均将原Workshop安装按哈希恢复，没有改用户正常存档。Test-ModTestEntry12项PASS，git diff --check通过。
- 未运行Full、真实联机、图形、旧故障磁盘档恢复或自然整局。接续发布复用最终Events报告并重新核对同核心、环境及日志，不重建最终核心。

## 合并及公开发布

- 所有运行时修改与测试记录已合入并推送main；最终核心仍为0fc779d，无重建。Invoke-ReleasePreparation只复用最终核心的Events报告，未将6ba的Cards报告绑定到另一包。
- 冻结目录：artifacts/Sts2Mod-0.3.17-v0.107.1-0fc779d6e763-candidate-20261028T1208060542418Z。快照SHA256：A933207E98EE52F00CA88457DD7AD3EE9423095069A8FDC3F55459F5738531ED；ZIP SHA256：1E2776C94345741CC4C05EC6417981E5953952F7503392241BDB7E0D8124868C。
- 实际上传目录.local/workshop/0fc779d6e763432a265224436b86765ecaa87a6f-A933207E98EE内public、mod_id.txt=3823282901，核心/文档/配置及图片哈希与冻结快照一致。官方ModUploader v0.2.0显式-i 3823282901成功上传，依赖未变；来源ZIP SHA256为2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578。
- 独立WorkshopProbe查询Public及k_EResultOK。Steam原生下载state=5、118465148字节、timestamp1793189337；七文件SHA256逐项与快照一致，未向Steam缓存复制候选文件冒充下载。封面及规则图均HTTP200，字节SHA256与源文件一致，本轮无需再次修图。
- 下载后的唯一工坊安装Load PASS，未知错误0、已知无GUI错误0；日志.local/logs/game-20261028T1210439884429Z.log，PID29460已清理，无本地重复覆盖副本。此项仅证明下载包启动，不扩写为真实联机或画面通过。
- 证据目录.local/reward-audit-release含candidate.json、upload.log、inspect-after.jsonl、download-after.jsonl、download-hashes.json、cdn-hashes.json、workshop-load.json及retired-intermediate-identities.json。制品轮转COMPLETE，保留3份交付、清理7项过期制品；额外核验并删除两轮临时测试副本及6ba/0d两份本轮过时中间核心，原日志、测试报告、身份哈希、源码和正常安装保留。
- 最终公开版本仍为0.3.17，包含既有Boss结算修复及本次两项兼容修复。用户追加检查/修复指令不记为试玩PASS；Full、真实联机、旧故障存档恢复及自然整局仍未验证。
