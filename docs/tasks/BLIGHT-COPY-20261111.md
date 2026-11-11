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

最终专项、冻结、发布与下载核验待完成后追加。
