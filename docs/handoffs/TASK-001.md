# TASK-001 开发交接

状态：代码和PC-A单人验证完成；**WAITING_MULTIPLAYER**。不自动进入TASK-002，不把未执行的双人或读档验收记PASS。

## 身份与获取

- 分支：`codex/TASK-001`；被测代码提交S：`3c95e19441261410ed254ceada47c83c1af53b62`。
- [草稿PR #2](https://github.com/b727031867/slay-the-spire-2-mods/pull/2)，基线`codex/TASK-000`；PR #1未合并，当前不自动合并或重写main。
- [GitHub草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-aa378cfffebbb02514e2)，ID `417111140`，草稿标签 `traveler-v0.0.2-3c95e1944126`；须用有草稿访问权限的账号下载。
- ZIP `Sts2Mod-0.0.2-v0.107.1-3c95e1944126.zip`，附件ID `655816219`，92,141字节；SHA附件ID `655816256`。
- SHA256：`553A055CF7EED753FAFE4EEAAD26F5794176929FE7375B6278F808D3CBF10A76`，PC-A已从GitHub回下载比对。
- 基线：Mod0.0.2 / 游戏v0.107.1 / Steam build23811903 / BaseLib3.3.2 / Godot.NET4.5.1 / net9.0 / SDK10.0.402。

## 完成内容

铁甲普通卡池加入风起之刃和岩之守护，稳定ID为TC_C01/02；基础/升级为1费6/9伤害、1费7/10格挡。主动风→岩固定额外4格挡，每位玩家每回合最多2次，触发后消费序列。普通牌保留，自动牌和额外结算不推进；达到2次保持空序列。玩家状态在各自Power实例中，基础效果前预占，使用原生命令执行基础与奖励，没有额外网络广播。

原生状态图标显示上一元素、剩余次数及用完提示；触发有“风岩守势：+4格挡”短气泡。提供中英文本和自制SVG。显式测试入口、纯规则断言、原生队列测试、奖励获取测试及日志检查脚本已加入，默认关闭。

## 另一台PC复现

1. 先检查本地改动，再获取本分支。仅文档的后续提交不是包内代码身份S；需要精确重建被测包时在干净目录检出S。构建较新提交会产生新身份和新包，应另记报告。
2. 按`TECH_BASELINE.md`和`COLLABORATION.md`安装本机工具/游戏，用自己的`local.settings.json`配置路径；不要复制PC-A绝对路径、程序集或存档。核对游戏构建及BaseLib精确版本。
3. 下载草稿ZIP及SHA，运行`Get-FileHash -Algorithm SHA256`核对，解压到忽略目录。先退出游戏，运行`Deploy-Mod.ps1 -PackageDirectory '<解压目录>/Sts2Mod' -InstallDependencies`。
4. 默认`Start-ModGame.ps1`启动并用`Test-ModLoad.ps1 -LogFile '<本次日志>' -CodeCommit '<S>'`确认。规则测试用`-TravelerTest`；等待完成后以`Test-TravelerRun.ps1`检查。正常奖励入口用`-TravelerRewards`，出现READY后手动选择主题牌，再加`-Acquisition`检查。具体命令见[开发说明](../DEVELOPMENT.md)。
5. PC-B独立跑单人后记录自己的结果，不能继承PC-A的PASS。双方使用同一个包开展TC-16/17，交换主客机，核对生命/格挡/上一元素/次数和每动作结算次数；`tc_hand`仅限单人，不能据此声称多人手牌注入已提供。测试新局通过正常获取或双方已核实的原生调试入口准备牌。

PC-A可继续在同一电脑开发测试；设备不是固定开发/测试角色。当前安装的为上述ZIP恢复出的0.0.2，最后以默认参数启动，测试能力关闭。

## 验证与后续

[PC-A报告](../tests/TASK-001-PC-A-run-01.md)：21项纯规则、28项真实规则断言、手动UI与取消、原生奖励获取、重复构建、部署/移除/恢复及GitHub回下载通过。

下一项优先验收是PC-B及双人TC-16/17。完整退出读档TC-19、实际暂停/恢复、不同版本TC-22、额外回合、死亡复活和英文UI未测，报告中保留边界；没有宣称中途连招恢复或断线重连支持。四张后续牌、雷元素连招、派蒙遗物和公开发行仍未开始。
