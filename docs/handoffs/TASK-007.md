# TASK-007 旅行者角色框架交接

分支`codex/TASK-007-traveler`，Mod0.2.0，未合并main；沿用私有工坊3823282901。准确源码S、候选哈希及验证范围见[报告](../tests/TASK-007-PC-A-run-01.md)。

先保留本机修改，再fetch并同步此分支；配置本机local.settings.json。环境仍为PowerShell7、.NET SDK10.0.402（目标net9.0）、Godot.NET4.5.1、游戏v0.107.1/build23811903、精确BaseLib3.4.7。新增编译引用游戏自带0Harmony.dll，Private=false，不将其复制到包中。无需复制PC-A工具缓存或用户存档。

启动游戏选择旅行者。75生命、3能量、99金币；起始牌3打击/3防御加风起之刃、岩之守护、雷光突刺、顺风整备各1；起始仅旅途罗盘，派蒙从角色专属来源获得。六张奖励牌原ID保留，另有两个Basic模型。基础牌不带元素，变形保留角色归属。

罗盘每场首个实际连携奖励后抽1，不被派蒙翻倍。无目标和0张连携抽牌不消耗；自身尝试抽牌后即消耗，不承诺绕过手牌上限。角色小卡池暂无稀有/能力牌，商店缺少类型会补位、Boss用原生稀有度回退。新增十张卡牌仍只是设计草案，不属于已交付内容。

快速专项：Start-ModGame.ps1 -TravelerCharacter -Headless，完成后Test-TravelerCharacterRun.ps1 -Focused。全量：Start-ModGame.ps1 -TravelerTest -Headless，完成后Test-TravelerCharacterRun.ps1。检查器须传入日志和被测完整CodeCommit。真实磁盘使用既有-TravelerEndToEnd/-ResumeTask003脚本与独立SandboxDirectory；仅结束本轮身份核对的测试进程。

角色战斗/选人图为原创简化矢量，属性动画不等于正式骨骼美术；营地/商人/能量等暂复用原生占位。后续先人工确认选人布局、身体位置/动作、地图和营地，再讨论扩充卡池和平衡。旧用户局迁移、自然整局、多人和PC-B独立测试均需各自证据。


被测S：`a6c3a2ae1ee348120d035cd8b9425f7dde945391`。最终候选SHA256：`A834FCCB46D33C9AA9FF41DC8FF3D88FE9A593936EBB579C3659EDA193F3A5F2`。[GitHub草稿包及校验下载](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-81b98a729e263448f622)。ZIP核心来自S、玩家文档在S后修正文案；核心不变，勿用S最初包装的D3F哈希。Steam实际七文件与最终ZIP一致，PC-A已更新，测试进程0。

42项专项包含在195项全量内；13项准备、281项恢复另计。6场24回合是受控样本，自然胜利0。角色示意图见[原创简化矢量预览](../art/TASK-007-character-preview.png)，不是实机截图。
