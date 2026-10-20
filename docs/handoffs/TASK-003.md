# TASK-003 无GUI验收交接

当前PC-A包为Mod 0.1.1：`Sts2Mod-0.1.1-v0.107.1-88cdb3108a20.zip`。干净代码S：`88cdb3108a200d8328f3dfaced15e4549cb5b610`；SHA256：`0231D4BAC922DA118D1B395F90BF0197ABBAC8E55DBC46125C134944020FD932`。从当前分支阅读最新[状态](../STATUS.md)与[PC-A报告](../tests/TASK-003-PC-A-run-02.md)。PC-B旧0.1.0包及91项结果仅为历史证据。

ZIP和`.zip.sha256`已同步至[GitHub草稿附件](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-8342af480195e136f78c)，release ID `417907943`，计划标签`traveler-v0.1.1-88cdb3108a20`；需使用有权限的账号访问。服务端返回的两份附件大小及SHA256均与本地一致，仍是草稿，未公开发行。

当前规则：风↔岩给4格挡；岩↔雷给触发牌同一存活敌方目标4点原生攻击；雷↔风抽1。三类双向连招共用每人每回合3次，成功后消费序列；普通牌不打断，自动出牌与额外结算不推进。共享罕见遗物派蒙使每次成功连招奖励翻倍（8格挡/8攻击/抽2），重复持有不叠加。

岩雷连招若触发牌无敌方目标或原目标已死亡，不重定向攻击，仍消费次数；派蒙也不会凭空产生格挡等其他奖励。基础效果击杀最后敌人时，不执行战斗内连招奖励。

在另一台PC先保留本地改动、fetch并确认`codex/TASK-002-continue`最新内容；部署前核对游戏v0.107.1/build23811903、BaseLib3.3.2及包校验。也可从S干净检出按[DEVELOPMENT](../DEVELOPMENT.md)自行构建并记录新包身份，不复用另一台电脑的个人路径或存档。

所有AI游戏测试使用`Start-ModGame.ps1 -Headless`。全量使用`-TravelerTest`；针对性`-TravelerAcceptance`仅覆盖12项TASK-003边界，不能替代全量结果。记录返回PID及日志，等待完成后用`Test-TravelerRun.ps1 -Task003 -Headless`核对全量证据与S；结束时只停止本次测试进程，不启动图形窗口。

后续工作按[TASK-003](../tasks/TASK-003.md)推进：无GUI正常获取链路、独立测试档的原生保存/退出/恢复、固定种子连续战斗与数值反馈、缺陷闭环。当前尚未执行的范围保持NOT_RUN；不因单人机制通过就把TASK-003整体标为DONE。UI/自然人工试玩待明确安排；双人按DEC-004后续专项测试。本轮不合并main，不公开发布版本。
