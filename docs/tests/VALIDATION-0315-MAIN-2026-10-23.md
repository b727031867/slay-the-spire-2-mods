# 0.3.15 合入 main 前验证 / 2026-10-23

用户要求：验证 codex/BUG-element-tuning-hand-input，完成后合入 main 并推送，暂不发布工坊。本轮未调用上传器、未更新工坊；线上仍记录为0.3.14。

## 源码与合并

- 原分支检查点9c801de，玩法d2ffc9b，清单39310d5；与main d25dd87在临时分支codex/validate-0315-main合并为ff244ba1b727034550ba602958ee768c6b969fc0。
- 三处冲突仅为docs/STATUS.md、workshop/workshop.json、workshop/description.bbcode.txt。保留main状态入口及冻结发布流程，更新0.3.15说明；配置保留friends_only。未改玩法规则。
- 最终核心066cf954ec001f53dd5e2b887d3decfe2bf02a91。相对ff244ba仅修正磁盘连续战斗测试的选牌范围及状态页空行；Full运行路径和生产玩法代码未改变，未重复运行耗时Full，不将ff244ba的直接运行冒记为066cf95。
- 两次Build-Mod均使用干净提交，C# 0警告0错误，Godot无GUI导入/PCK导出成功。Windows x64、v0.107.1/build23811903、BaseLib3.4.7。
- 最终ZIP：Sts2Mod-0.3.15-v0.107.1-066cf954ec00.zip；SHA256 810A370AB5B64BE479FE5346D7500EE903616060C8B3A342CB8BDE327E2BBE94。

## 验证结果

| 套件 | 源码身份 | 结果 | 断言 | PID | 日志 |
| --- | --- | --- | --- | --- | --- |
| Full | ff244ba | PASS | 724 | 38016 | game-20261023T0240197795473Z.log |
| SavePrepare | 066cf95 | PASS | 13 | 25096 | game-20261023T0301575081174Z.log |
| SaveResume | 066cf95 | PASS | 295 | 15644 | game-20261023T0302275259112Z.log |
| Load | 066cf95 | PASS | 0（启动/身份校验） | 36428 | game-20261023T0304442533725Z.log |
| Character | 066cf95 | PASS | 97 | 8576 | game-20261023T0304593814474Z.log |
| Visuals | 066cf95 | PASS | 128 | 22940 | game-20261023T0306040047320Z.log |

- 离线规则57、日志分类26、发布包装15、发布准备38、测试入口11项通过，共147项；后四类为合成夹具，不代表真实游戏加载。
- Full包含0.3.15平衡专项41项、卡池专项124项（含雷蚀回响、调律输入锁、灌注及棱晶交互）；资源唯一性检查保留。Full节点/提示表现检查不等于画面渲染验收。
- 最终SavePrepare保存全部28张奖励牌基础/升级版本；重新部署同包，使用返回的同一隔离目录00f02ae779c84aaf941a99dcf1b28951，经过两次原生加载验证牌组、升级、遗物、金币一致，无重复注册、遗物或旧连携状态。
- 连续战斗：6场、24回合、103动作、55次连携，3场自然胜利，其余3场在四回合预算后由夹具换战/退出清理。使用高生命受控档，未在连续战斗中注入手牌或能量，不代表自然整局或平衡验证。
- Load、Character、Visuals最终独立套件均无原生headless或退出诊断。
- 严格检查器均确认未知错误0。Full已知原生headless诊断1；最终SavePrepare为1及原生退出诊断9，SaveResume为0及原生退出诊断9。按固定文本和源码位置分类，没有放宽断言或忽略未知错误。

## 初次失败与修正边界

首次真实启动时Steam尚未运行；静默启动客户端后，Steam自动更新重启，前两次游戏启动分别因NoSteamClient和ConnectToGlobalUser失败。两次日志保留为FAIL，仅清理各自测试PID；客户端就绪后Full完整通过。

首轮ff244ba SavePrepare13项通过，SaveResume在46条通过后失败于TASK003_soak_sequence_b1_t3_a2（game-20261023T0259010186471Z.log，PID27592）。原因是旧oracle只模拟手动出牌，元素调律选择的额外元素牌实际正确推进连携，oracle漏计。三相灌注同样会产生手动出牌之外的随机效果推进。

最小修正仅在RunContinuousBattles选择普通手牌，排除ElementTuning与TriphaseInfusion，保留严格序列、次数上限、能量、手牌、回合及新战斗重置断言；不修改生产结算。两牌仍全部经过磁盘保存/读取，其效果链由Full专项验证。本段连续战斗不覆盖两牌效果链的跨战斗组合游玩。

## 安装与范围

所有游戏测试均使用--headless、隐藏窗口、独立标记隔离档及原生磁盘保存，无测试Steam云后端；未复制或触碰用户存档。统一入口持有测试进程对象并清理。测试前本机没有本地Sts2Mod/BaseLib副本，结束后逐文件核对后备份移走测试副本，恢复原工坊加载；不启动图形游戏。

未验证：图形渲染、鼠标/手柄实际选牌、多人/重连、自然完整流程/Boss平衡、用户旧档迁移。本轮不发布工坊，也不将本地加载说成实际工坊0.3.15加载证据。原始日志和结构化结果保留在.local/verify0315、.local/logs。测试本地副本已校验移至.local/verify0315/local-overrides；所有测试进程已结束。
