# TASK-000 / PC-A / run-01

日期：2026-10-19。执行来源：本机PowerShell 7实际命令、真实游戏进程日志及computer-use游戏窗口观察；没有以mock替代游戏加载。

## 被测身份

| 项目 | 实际值 |
|---|---|
| 代码提交S | af2bd55139fae1fec88c7862db31d9d5bc853060 |
| 任务分支 | codex/TASK-000 |
| 游戏 | v0.107.1；Steam构建23811903；游戏提交59260271 |
| 设备 | PC-A，Windows x64 |
| 框架 | BaseLib 3.3.2，本地安装；无其他Mod框架 |
| 工具 | .NET SDK 10.0.402；Godot .NET / SDK 4.5.1 |
| 构建配置 | ExportRelease / win-x64 |
| 包 | Sts2Mod-0.0.1-v0.107.1-af2bd55139fa.zip，3751字节 |
| 包SHA256 | 0E1349670CD0CD58435D556DE5D866EE47308375598C945B5ED73158D9DE2269 |
| DLL SHA256 | FB3646AA7384BCC9ED0BA6045EF5A5B9E7B686DFDAB61E905E481AEC838996D1 |
| PCK SHA256 | 4D98AC6BF7BEAC6F486B1D212A9708C9EC6D84CBA5A5B2EE7AB71AF669F28763 |
| GitHub附件 | [草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-59ecdcfdca41046ed7cd)，需有仓库写权限的账号登录 |

包由S干净工作区构建，`build-info.txt`中dirty为false。两次干净构建DLL/PCK哈希一致；ZIP因文件时间等打包元数据可以不同，本报告绑定第二次构建的确切ZIP。

## 检查结果

| 用例 | 预期 | 实际结果 |
|---|---|---|
| 固定版本还原与编译 | 相同配置可重复构建 | PASS，两次锁定还原与ExportRelease/win-x64编译、PCK导出均退出0，0警告、0错误；源码保持干净 |
| 包内容 | 只有本项目文件 | PASS，ZIP只有Sts2Mod.dll/json/pck及build-info.txt；无BaseLib、游戏程序集、PDB或个人配置 |
| 首次部署干净提交包 | 加载正确S并进入主菜单 | PASS，READY含完整S、BaseLib 3.3.2与resource=ok；一次初始化，启动无ERROR |
| 重复部署并重启 | 无重复初始化或加载错误 | PASS，生成备份后覆盖本项目文件；新进程READY仍只有一次，无启动ERROR |
| 移除 | 只移除本项目且不再加载 | PASS，mods/Sts2Mod目录移除；重启进入主菜单，无本项目READY或初始化完成记录 |
| 共享依赖保留 | BaseLib不随本项目卸载 | PASS，移除前后BaseLib.dll哈希一致，移除后的启动仍加载BaseLib |
| 从备份恢复 | 恢复同一被测S | PASS，通过Deploy-Mod指向备份；READY包含完整S，主菜单出现“已加载2个模组”，无启动ERROR |
| 运行中部署保护 | 拒绝修改正在运行的Mod | PASS，开发冒烟阶段明确报Close the game before deploying，未继续部署 |
| 校验不符包 | 在复制到游戏前拒绝 | PASS，复制测试包到.local并修改自身DLL后，部署报Package checksum mismatch；真实部署文件未被该测试替换 |
| 源码身份检查 | dirty包不能冒充相同干净提交 | PASS，将开发期dirty日志按干净提交校验时被拒绝 |
| 原生模组清单 | 可见BaseLib和本项目 | PASS，开发冒烟时实际打开游戏设置→Modding清单，看到两项已启用；最终S包通过主菜单和加载日志复核 |
| GitHub附件校验 | 上传字节与本地包一致 | PASS，GitHub返回asset digest与本报告ZIP SHA256一致；保持草稿，未公开发行 |
| 单人正式玩法/卡池/遗物/连招 | 后续内容用例 | NOT_RUN，尚未实现；本次没有开始新对局 |
| 存档恢复与双人主客机 | 后续验收 | NOT_RUN，PC-B尚未接入；最小加载成功不代表这些检查通过 |

对应FR-01/08的最小加载与部署部分，以及TC-20的相关步骤；不表示卡牌注册、完整FR-09或其余FR/TC已经通过。

## 关键日志与本地证据

最终S的首次启动、重复部署、移除、恢复日志依次为：

- `.local/logs/game-20261019T0844407201112Z.log`
- `.local/logs/game-20261019T0845296079166Z.log`
- `.local/logs/game-20261019T0845479151682Z.log`
- `.local/logs/game-20261019T0846067745734Z.log`

两次构建日志为`.local/logs/build-final-1.log`和`build-final-2.log`；验证汇总为`.local/logs/workflow-results.json`；脱敏摘录保存在`artifacts/logs/`。原始日志含平台账号与云存档信息，只在PC-A本地保留。

有本项目的三次最终启动均包含以下核心记录；移除后该标记不存在：

```text
STS2MOD_READY version=0.0.1+af2bd55139fae1fec88c7862db31d9d5bc853060 baselib=3.3.2 resource=ok
[INFO] Finished mod initialization for 'BaseLib' (BaseLib).
[INFO] Finished mod initialization for 'STS2 Mod - Technical Preview' (Sts2Mod).
[INFO]  --- RUNNING MODDED! --- Loaded 2 mods (2 total)
```

开发过程中已修正两项问题：构建记录使用.json会被原生加载器误认成清单，现改为.txt；Godot编辑器与导出配置不一致会改写锁文件，现统一为ExportRelease/win-x64。最终测试使用修正后的S，不把开发期失败记录当作S的通过证据。

部署和移除脚本没有读写游戏存档；游戏自身启动发生了原生Steam云同步。本次没有手动编辑存档、开始新对局或变更游戏分支。
