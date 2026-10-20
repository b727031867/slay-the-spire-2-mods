# TASK-003 单人无GUI验收交接

当前成果按DEC-006交付main，任务状态DONE_SINGLEPLAYER_HEADLESS。另一台PC先保留本地修改并fetch，工作区干净且无分叉时切换main并`git pull --ff-only`；不要继续从旧lightning分支接手。

被测运行包来源S：`0058ea8eedaa1fe1b00f279c0599f983c3431b9b`，Mod0.1.1；包`Sts2Mod-0.1.1-v0.107.1-0058ea8eedaa.zip`，224,465字节；SHA256 `CAD4D0FE257012ABFEAB449DF4A982A2988E986F4014B2F65C991964AD056D36`。

ZIP和SHA256文件已同步至[GitHub草稿附件](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-1251651e5497a6e6a63c)，release ID `417945312`，计划标签`traveler-v0.1.1-0058ea8eedaa`；需有权限账号访问。两份附件的服务端digest和大小均与本地一致，仍为草稿，未公开发行。

游戏v0.107.1/build23811903、BaseLib3.3.2、.NET SDK10.0.402、Godot .NET4.5.1。用本机路径配置，不复制另一台PC的个人配置或存档。可部署本包并用main最新检查器验证S；也可main干净构建，记录新包自己的提交与哈希。检查器换行语法修正及联合断言去重是S之后的工具调整，不把后续main提交填成本包来源。

当前规则：风↔岩4格挡；岩↔雷同一存活敌方目标4点原生攻击；雷↔风抽1；三类共用每人每回合3次。派蒙共享罕见、每次奖励翻倍（8/8/抽2）、不叠加。普通牌不打断，自动/额外结算不推进，无目标不转移攻击，基础效果结束战斗不发战斗内奖励。

通过范围：27项纯规则、13项日志分类、12项准备、255项磁盘恢复/持续场景和94项全量回归。实际奖励和普通商店获取、派蒙防重复、六种牌/升级完整磁盘恢复、同包重新部署、两次原生读档、6场24回合持续机制均有证据。具体来源与数值见[报告03](../tests/TASK-003-PC-A-run-03.md)。

复现按[DEVELOPMENT](../DEVELOPMENT.md)：`-TravelerEndToEnd -Headless`准备新档并等待原生退出；用E2E检查器Phase Prepare核对日志和S；重新部署同包，再`-ResumeTask003 -SandboxDirectory '<返回目录>' -Headless`，等待退出并检查Phase Resume。元数据在`.local/task003-saves/<id>`，真实存档在独立`user://sts2mod-acceptance/<id>`，不接云、不覆盖用户profile；恢复为战斗入口回滚。完整94项机制回归另用`-TravelerTest -Headless`，不是只跑12项旧边界子集。

保留限制：原生无GUI退出资源诊断ENV-001由无本Mod对照确认同类型，精确分类并单独计数，未知错误仍失败；没有证明内存泄漏已修复。持续场景使用受控牌组/高生命值、每场4回合预算，未自然获胜的场次由夹具清理，不证明整局手感或平衡。图形UI/自然人工试玩、双人主客机与跨玩家同步均NOT_RUN。没有公开发行Release。
