# TASK-006：0.1.4 连续连携交接

分支 `codex/TASK-005-experience`，未合并 main。沿用同一私有工坊条目 3823282901；代码与基线见 [验收报告](../tests/TASK-006-PC-A-run-01.md)，规则决定见 DEC-008。

同步前保留本机改动，fetch 后同步该分支。开发环境仍各机独立配置：PowerShell 7、.NET SDK 10.0.402（目标 net9.0）、Godot .NET 4.5.1、游戏 v0.107.1/build23811903、BaseLib 精确 3.4.7。读取 local.settings.example.json 配置本机路径，加载 scripts/Enter-DevEnvironment.ps1；不复制 PC-A 私有配置、凭据或用户存档。玩家订阅安装无需开发工具。

从工坊下载后确认 Mod 0.1.4、build-info 来源及 BaseLib 版本；避免本地/工坊重复加载。可在本机 artifacts 取得报告标识的候选 ZIP，或从干净源码重建。不同源码提交/设备构建须保留自己的来源与哈希，不冒称与本候选字节一致。

复测：Start-ModGame.ps1 -Headless -TravelerTest，等待 STS2MOD_TEST_COMPLETE；运行 Test-ContinuousComboRun.ps1 -LogFile 返回日志 -CodeCommit 被测完整源码 S。仅结束核对过命令行和日志身份的本轮测试进程。测试使用独立命名空间，不操作用户档案。

人工试玩重点：三费是否能稳定规划两次连携；风岩防守、岩雷进攻与雷风抽牌的选择是否均有价值；派蒙与零费顺风整备是否过强。当前每回合仍最多三次，不把机制断言通过视为平衡结论。公开工坊、main 合并及多人测试另行安排。
