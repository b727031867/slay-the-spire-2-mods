# 项目状态

更新：2026-10-19。

当前完成：PC-A工具链安装与验证；固定BaseLib 3.3.2；建立src/Sts2Mod与构建、部署、启动、移除、日志检查脚本；开发冒烟已在真实游戏验证DLL/PCK初始化与原生模组列表。版本和证据见 `TECH_BASELINE.md`、`API_FINDINGS.md`。

当前阶段：TASK-000本次授权的第1、2步DONE。干净提交打包、两次重复构建、真实加载、重复部署、移除及备份恢复验收均通过。任务见 `tasks/TASK-000.md`，操作说明见 `DEVELOPMENT.md`，证据见 `tests/TASK-000-PC-A-run-01.md`。

已有需求基线：`../STS2-Mod-Requirements-v0.1.md`，文档版本0.2；本次仅调整开发测试与GitHub交接安排，玩法规则未变。

已确认协作方式：PC-A（当前电脑）与PC-B（后续另一台电脑）均可开发、构建和测试，通过GitHub同步代码、文档和构建产物，不固定开发端/测试端。具体流程见 [COLLABORATION](COLLABORATION.md)，决策见 [DECISIONS](DECISIONS.md)。

设备状态：PC-A最小Mod图形界面启动与加载通过；正式卡牌玩法、存档和双人联机未测试。PC-B尚未接入，不能沿用PC-A的PASS结论。

下一步：完成本次可追溯包及交接记录后，进入TASK-001两张牌与单条连招验证；动作ID在暂停/恢复时变化，需先设计并实测稳定的逻辑动作关联。主题与宿主仍待确认。

本次代码提交S：`af2bd55139fae1fec88c7862db31d9d5bc853060`，分支`codex/TASK-000`已推送；后续文档提交引用S，不替代被测代码身份。ZIP及SHA256已上传[GitHub草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-59ecdcfdca41046ed7cd)，附件和操作步骤见 `handoffs/TASK-000.md`。未启用自动构建上传，未公开发行。

Git：公开仓库为 [b727031867/slay-the-spire-2-mods](https://github.com/b727031867/slay-the-spire-2-mods)，本次成果保留在`codex/TASK-000`，main仍是此前的共享基线。

初始化代码提交：`5f7b07388cb4fefb06cbb0cdf7236f475a36ac6c`，包含需求书、项目 README、环境文档、脚本和验证工程。个人路径配置、游戏二进制文件和本地构建产物未提交。

本机已安装BaseLib和Sts2Mod用于加载验证；未更换游戏分支，未开始新对局。部署脚本不修改存档；实际游戏启动按原生行为执行Steam云同步。
