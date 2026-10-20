# TASK-002 增量A：PC-A验证

日期2026-10-19，代码S：`06e16c144727378e335ae0b724d0c42e374f6ac5`；分支`codex/TASK-002-lightning`；Mod0.0.3 / 游戏v0.107.1 / Steam build23811903 / BaseLib3.3.2。TASK-002整体仍IN_PROGRESS。

范围：TC_C03雷光突刺（1费7伤害、升级10）和岩→雷额外4点攻击伤害；保留风岩，与岩雷共用每回合2次上限。雷→风抽牌、其余三张牌与派蒙尚未实现。

## 执行与结果

- `dotnet run --project tests/ComboRules/ComboRules.csproj --configuration Release`：31项PASS；包含旧规则、岩雷、反向、混合上限、自动与去重及新增雷元素编码不冲突。
- 从干净S执行`Build-Mod.ps1 -DeviceId PC-A`：编译0警告/0错误，PCK导出成功，dirty=false。本轮一次干净构建，不宣称做过重复构建或跨PC复验。
- 部署后`Start-ModGame.ps1 -Headless -TravelerTest`运行真实游戏进程，独立ShouldSave=false测试局、原生出牌队列与回合动作；补能量/治疗/高生命靶子仅用于夹具。44项PASS，包含TASK-001风岩回归及新增用例。此为无界面逻辑集成，不能替代手动UI验证。
- `Test-TravelerRun.ps1 -LogFile '<本次日志>' -CodeCommit '<S>' -Lightning`通过；原始日志本地文件名`game-20261019T0951410792954Z.log`，可分享摘录见[evidence](evidence/TASK-002-PC-A-rules.txt)。无ERROR或TEST_FAIL。

| 新增检查 | 结果 |
|---|---|
| 铁甲解锁池 | 雷光突刺恰好1条 |
| TC-02岩→雷 | 岩7格挡，雷7基础+4追加=11伤害，次数1，序列无 |
| TC-13雷升级 | 10基础+4追加=14伤害 |
| TC-08混合上限 | 风岩、岩雷、岩雷，只有前两组奖励；总伤害24，总格挡25，次数2，序列无 |
| TC-10自动 | 自动雷只造成7伤害，保留岩且次数0；随后主动雷仍触发，总伤害18 |
| TC-11原生Duplication | 雷基础结算两次，追加仅一次；7+4+7=18伤害，次数1，序列无 |
| TC-24修正与格挡 | 力量2、虚弱、目标易伤及3格挡：基础floor((7+2)×0.75×1.5)=10，追加floor((4+2)×0.75×1.5)=6，扣除3格挡后实际损失13生命 |
| TC-14目标死亡 | 基础7伤害击杀当前目标，另一敌人生命不变，追加不转移；次数1、序列无 |
| 稳定ID与升级模型往返 | 原生序列化模型恢复TC_C03、升级级别1、基础10伤害；不等于完整存档恢复 |

## 包与交接

ZIP `Sts2Mod-0.0.3-v0.107.1-06e16c144727.zip`，108,468字节；SHA256 `59CDC0A4CFD074EC38C6B23EE8C4F98A129DF223C224691AC02A7053AAC9D2FC`。只含本Mod DLL/清单/PCK/build-info.txt，无游戏程序集、BaseLib或外部图片。

文件SHA256：DLL `2085919EE5318FA814C626FEA6B3206D571E60DB11488596EF43EE014FB29E02`；PCK `15BAB7E5F628CD370DD858C26AF35001D436EA67A25ED8BDDDF95AD18A53B5EB`；清单 `41CAA4CDFE914E46ADF58B68F439BDB1A53D3D5A190269351096DFD41E580DD8`。

[草稿PR #3](https://github.com/b727031867/slay-the-spire-2-mods/pull/3)；[草稿包](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-872f3642b3aa121dcc6d)。上传响应digest已核对，不声称本轮回下载测试。自动测试结束后关闭自己的无界面进程，以默认参数启动0.0.3；不继续自动测试或生成永久牌组内容。

## 未测范围

新增雷牌手动UI、中英界面、正常奖励/商店实际获取、自然整局与平衡性均NOT_RUN；只验证了原生卡池入口，不能把TASK-001岩牌获取结果用于雷牌。完整退出读档、实际暂停恢复、死亡复活、其他Mod组合及双人专项仍NOT_RUN。雷→风目前不发抽牌奖励。未实现内容保持TASK-002待办。

本轮没有手工读写或删除玩家存档。测试run设置ShouldSave=false，原生游戏仍可能维护模组档案进度并执行Steam云同步。
