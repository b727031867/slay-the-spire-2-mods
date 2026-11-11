# 雷蚀回响文案修正

日期：2026-11-11；分支：`codex/blight-copy-20261111`；基线：`main b0bfb7f`。

## 影响评估

- 截图中的“增加6层或回合”省略了回合数，“再降低”还可能让人误读为负力量/负敏捷重复增强。中英文改为分别写明层数、回合数，并用“则”/“instead”区分负属性的处理方式。
- 修改点：`TC_C28.description` 的中英文运行时资源 → 普通/升级卡牌描述与卡牌选择界面。沿用动态变量 `Stacks`（3/6）和 `ComboPreview`。
- `EchoingBlight.OnPlay`、卡牌费用、升级规则、目标筛选及存档均不变。同步工坊说明与本次更新注记；版本仍为0.3.19文案热修。
- 必跑：干净构建、Cards专项（含加载、现有全部雷蚀回响边界用例及新增普通/升级描述三处数值替换检查）。
- 不跑：Full（用户未要求）、磁盘存档（未改持久化）、图形与联机（未获此类测试授权）。无GUI检查不证明截图中的实际排版。
- 发布授权：用户本轮明确要求“修复此问题并且发布到工坊”，按此指令完成针对性验证后直接公开更新原条目3823282901；不将该指令或无GUI结果记为用户试玩PASS。

## 验证与交付

### 实施与独立审查

- 主Agent直接实施；原生 `Explorer`（`ccs-ytx/deepseek-v4-1-flash-261017 / high`）只读核对，确认每种负面效果仅调用一次 `PowerCmd.ModifyAmount`；原生 `Reviewer`（`gpt-6-luna / medium`）独立审查实际diff与调用点。
- Reviewer发现整段描述的数字统计会混入连携预览中的3/3。最初改为第一行统计后，实际Cards又识别出原生虚无关键词会插入正文前。最终直接移除同卡同状态的 `ComboPresentation.Preview` 文本，再对完整描述统计三处数值；保留完整描述中不得残留占位符的检查。严格验证器要求普通/升级两个新增标记，未降低断言要求。
- Reviewer复核最终方案后未发现阻塞问题。探索和审查均未写入工作区，没有使用Coder或DSH。
- 中英文JSON分别检查普通3/升级6的三处数值占位符与单个连携占位符，PASS。工坊源说明5880 UTF-8字节（<8000）。

### 构建与被替代结果

- 最终干净核心：`7ae5a75766ba`，0.3.19；游戏v0.107.1/build23811903；BaseLib3.4.7；PC-A。
- 核心SHA256：
  - `Sts2Mod.dll`：`9660A0835B2084BE026DF5F45B7E0F9E2AF0897448EE39DE7EC1495C56ABB149`
  - `Sts2Mod.pck`：`C28426CF5F48571FE43952183D5D285C74F719EE2CF3305F82318419EF879E9E`
  - `Sts2Mod.json`：`65E95B41950B8CE7BEBBFAC0B734B914390407EB4D2E66FA1AD9EE6F6BD0A2ED`
  - `build-info.txt`：`081ED7B6F72B515312D759853A18F281E3FC1F4404EF5F62B2949DD1F90E0E24`
- 最终构建成功，日志`.local/blight-copy-build-verified.log`。原有沙虫测试的两条nullable警告未处理；Godot编辑器导入有`TravelerTestRunner`类型解析及`_EDITOR_GET`诊断，不能仅凭导出退出码宣称运行时通过，以下以最终实际加载/专项结果为准。
- `a5eea71`的首次Cards因独立审查发现断言误判风险主动中止，仅结束本次PID51944；日志`game-20261111T1358091599686Z.log`及原FAIL报告保留，不计PASS。
- `6ef160a`的Cards在683项后实际FAIL：`BLIGHT_description_all_three_amounts_False`，原因是上述虚无前缀行；日志`game-20261111T1400267181799Z.log`及原FAIL报告保留。
- 中间`d61ad8d`构建识别到`Add<T>`返回`CardModel`而预览格式化要求`ElementCardModel`；补上已知卡牌类型转换后重建为最终`7ae5a75`。未交付或发布中间失败包。

### 最终Cards专项

- 同一最终核心无GUI Cards：1196项PASS，未知错误0；分类器接受的原生headless诊断6条，未忽略未知错误。普通/升级的两个新增描述标记及既有全部BLIGHT边界标记均存在。
- 日志：`.local/logs/game-20261111T1409363852444Z.log`；SHA256：`E3FBBCD322BEE75873F123048CD18D5CCFDB142DAA68D1F23D65B221F127D293`；耗时1248.183秒。
- 绑定报告：上述日志的`.result.json`（schemaVersion2，完整核心与游戏/依赖/设备哈希）；副本`.local/blight-copy-cards.json`。套件自带加载、资源和身份验证，不另跑本地Load。
- 后续仅补充验证/交付文档，不重建或改变该被测核心；冻结时复用同包报告。

### 冻结、公开发布与实际下载

- 已按检查结果合入并推送`main 62954f1`，远端提交核对一致；相对被测`7ae5a75`的运行时源码无差异，后续未重建。
- `Invoke-ReleasePreparation -Suite Cards -TestResultFile .local/blight-copy-cards.json`：PREPARED，复用同包/同环境报告；自动轮转COMPLETE，保留最新3份交付。
- 快照：`artifacts/Sts2Mod-0.3.19-v0.107.1-7ae5a75766ba-candidate-20261111T1430586453004Z`。
- 快照SHA256：`3E74E99E5D3D802F647F0502111626E34EBE450AB3B291D56E3A55607FC94320`；候选ZIP SHA256：`79E810CB1DAAD36FEC360CF6A7CC74FE67D1FD8E4DEC7C2AC86DEBAE1F53A3CF`。
- 上传目录：`.local/workshop/7ae5a75766ba6b4645d5c927ba17372923195d1e-3E74E99E5D3D`。上传前再次核对实际配置public、原条目3823282901、说明5880 UTF-8字节，核心/玩家文档/工坊配置与图片均匹配冻结快照。
- 官方ModUploader v0.2.0成功上传；ZIP SHA256：`2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`。上传及独立远端/下载/CDN证据位于`.local/blight-copy-release/`。
- 独立inspect：`k_EResultOK`、`k_ERemoteStoragePublishedFileVisibilityPublic`、consumerApp2868840；Steam原生下载installed=true/state5，144138803字节，安装目录`E:/SteamLibrary/steamapps/workshop/content/2868840/3823282901`，七文件SHA256与冻结包全部一致。
- 封面和唯一规则图均HTTP200并与源SHA256一致；封面`452B5B899B2FB29F82C471381539A8D71D7370E695A749898378E3BAB14C74EA`，规则图`6B72EB9B9A1B73E1E0F898B3E1F68952B3A35DD48182651012F8FE8471BF205B`。不以上传回调替代实际验证。
- 移走本地Sts2Mod测试副本（可恢复备份`.local/backups/removed-20261111T1432155932819Z`），仅从工坊副本运行Headless Load：PASS、正确7ae5a75身份、资源正常、未知错误0、原生headless错误0，主菜单到达。日志`game-20261111T1432157829967Z.log`，SHA256：`11B81A4B26D20A9965BC16EECAF74A6F3D0B337FBCF605CA4C9AD22F3210F4D2`；结果`.local/blight-copy-workshop-load.json`。
- BaseLib实际载入本机`mods/BaseLib`的v3.4.7，和专项绑定依赖哈希一致；Steam查询3737335127虽报告installed，但其返回缓存目录实际不存在，故不将其当成真实下载版本核验。本轮未重新下载依赖或改变既有依赖部署，远端依赖下载字节未验证。
- 仅结束本轮启动的测试进程，没有启动图形游戏或修改用户存档。Full、图形排版、自然整局、真实联机与用户试玩均NOT_RUN；直接发布授权不记为试玩PASS。
- 统一入口已成功保留最新三份交付；任务中间构建a5eea7102e4f、6ef160ae6989及失败空目录d61ad8d2cae7的单独删除被自动审批策略拒绝（blocked by policy，未给更具体原因），命令未执行，目录保留。未改用其他方式绕过策略；失败日志及恢复备份保留。
