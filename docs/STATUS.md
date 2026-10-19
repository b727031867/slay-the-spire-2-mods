# 项目状态

更新：2026-10-19。

当前完成：PC-A已按根目录需求书安装Windows开发环境；真实程序集编译、Godot无UI导入及PCK导出通过。版本和证据见 `TECH_BASELINE.md`。

当前阶段：TASK-000 技术调查，**环境安装子任务完成**；完整 Mod 兼容性和游戏实测尚未完成。

已有需求基线：`../STS2-Mod-Requirements-v0.1.md`，文档版本0.2；本次仅调整开发测试与GitHub交接安排，玩法规则未变。

已确认协作方式：PC-A（当前电脑）与PC-B（后续另一台电脑）均可开发、构建和测试，通过GitHub同步代码、文档和构建产物，不固定开发端/测试端。具体流程见 [COLLABORATION](COLLABORATION.md)，决策见 [DECISIONS](DECISIONS.md)。

设备状态：PC-A工具链验证通过、游戏实测未执行；PC-B尚未接入，不能沿用PC-A的PASS结论。PC-B接入前继续在PC-A推进开发与单人测试。

下一步：锁定适配 v0.107.1 的单一框架版本，建立正式工程并进行最小加载验证。无需在安装环境阶段确认主题或宿主设计。

产物同步：源码与文档通过Git推送；后续需要跨机复用的Mod包通过GitHub Releases附件交接，记录源码提交、构建设备、依赖版本和SHA256。尚无可安装Mod包，未创建Release，也未启用自动构建/上传流程。

Git：已创建公开仓库 [b727031867/slay-the-spire-2-mods](https://github.com/b727031867/slay-the-spire-2-mods)，本地 main 已推送并跟踪 origin/main。

初始化代码提交：`5f7b07388cb4fefb06cbb0cdf7236f475a36ac6c`，包含需求书、项目 README、环境文档、脚本和验证工程。个人路径配置、游戏二进制文件和本地构建产物未提交。

没有安装游戏内 Mod，没有改变存档或游戏分支。
