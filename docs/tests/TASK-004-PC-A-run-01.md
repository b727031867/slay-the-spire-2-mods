# TASK-004 PC-A 发布候选与工坊安装验收

日期：2026-10-20。状态：**PASS_PC_A_HEADLESS_PRIVATE**。自动化使用无GUI、隐藏进程；未进行图形验收或自然人工试玩。

## 候选身份

- Mod：0.1.2；来源S：`77e8f5e610b799b38281a421b7343223a8cf453d`，干净提交。
- 游戏：v0.107.1 / Steam build23811903，Windows x64。
- BaseLib：3.4.7，NuGet精确锁定；官方ZIP SHA256：`D28C649A3AEA9EF4869050A06BFBADC4863016BCBD4C1740ACC4CD07809E4BD8`。
- 候选ZIP：`Sts2Mod-0.1.2-v0.107.1-77e8f5e610b7-candidate.zip`，SHA256：`73C319CC89EB422C78D6C5A17DB07760E85BBDBE831DCE5DA2924701EB95B89F`。
- 核心部署ZIP：`Sts2Mod-0.1.2-v0.107.1-77e8f5e610b7.zip`，SHA256：`CDC06DD726179CF16211C21DC6322F7373B01A94264FD922D6D264658B4044C7`。
- 私有工坊条目：3823282901，AppID2868840。Steam API查询确认private和完整中文标题；不等于公开发布。
- 官方上传器：Mega Crit ModUploader v0.2.0，Windows x64 ZIP SHA256 `2B55C19CC5932235CA9DBD663CA07D04E5D5A0018402303199F9B4EC8CA06578`。本机SDK没有.NET8，启动时仅对上传器启用DOTNET_ROLL_FORWARD=Major，使用已安装运行时；未更改系统全局设置。

## 已确认结果

1. 初始0.1.1候选fa4978f构建0警告/0错误；私有上传、订阅和七个文件SHA256一致。但纯工坊依赖为3.4.7，初始化实际报出要求3.3.2，判定FAIL并淘汰，不作为可发布候选。
2. 修复为0.1.2候选S：精确升级NuGet、锁文件、运行时守卫、安装脚本基线及检查器期望；保留卡牌、遗物和连携实现。编译0警告/0错误，Godot无GUI资源导出成功。
3. 更新同一私有条目为0.1.2，Steam订阅并请求下载。Steam安装状态5（Subscribed+Installed），七个内容文件逐项SHA256与候选工作目录相同。BaseLib的DLL/JSON/PCK三文件与官方3.4.7发行包逐项SHA256相同。
4. 原有本地Sts2Mod及BaseLib均先移至`.local/task004/original-mods`，未覆盖存档。游戏日志确认DLL/PCK来自Steam实际workshop/content路径；不是本地部署或手工填充Steam缓存。
5. 27项纯规则、13项日志分类与PowerShell脚本语法检查通过。实际工坊包全量94项玩法断言通过；未知运行错误0，精确分类的已知无GUI渲染诊断2，不写成全部日志零ERROR。
6. 同包准备阶段12项通过：真实战斗奖励领取、商店购牌及派蒙和金币扣除、售空及后续商店防重复、六牌基础/升级保存夹具与原生磁盘保存。原生退出成功；已知无GUI纹理诊断1、原生退出诊断9，未知错误0。
7. 在保存后取消本任务创建的Mod订阅，Steam回调OK、状态4（保留缓存但不再订阅）。本地Sts2Mod仍缺席，再次启动到主菜单确认未加载Sts2Mod。随后重新订阅、请求下载，七项文件再次校验一致，再使用原独立存档启动恢复阶段。

8. 同一工坊包恢复阶段255项通过：原生读取磁盘存档、完整牌组/六种基础和升级牌/遗物/金币精确恢复、第二次菜单读取、序列清空和派蒙翻倍。六场受控战斗24回合84次出牌29次连携，自然胜利0；高生命值受控夹具、每场4回合预算、不注入手牌或能量，不能推导自然整局或平衡通过。正常原生退出；已知运行诊断0、原生退出诊断9、未知错误0。
9. 最后不带测试参数从工坊普通无GUI启动，到达主菜单，一次READY且无任何SANDBOX/TEST/E2E/SOAK标记，已知/未知运行错误均0；测试能力默认关闭。仅停止本轮拥有且命令行/日志路径匹配的进程，最终游戏进程0，没有启动图形游戏窗口。
10. 两张原创说明图已实际打开检查，无文字裁切；封面1024×1024、48,910字节，规则图1280×720、53,282字节，均小于1MB。不是游戏内截图。候选ZIP和SHA256已上传GitHub草稿Release，服务端digest和大小校验一致。

## 证据与复现

| 项目 | 结果 | 共享证据 |
| --- | --- | --- |
| 干净构建/导出 | 0警告/0错误 | build-info及包SHA256 |
| 纯规则 / 日志分类 | 27 / 13 PASS | 既有测试程序；结果记于本报告 |
| 工坊更新与文件校验 | Mod七文件、BaseLib三文件PASS | [安装证据](evidence/TASK-004-PC-A-install.txt) |
| 全量玩法 | 94 PASS；已知原生渲染诊断2 | [规则](evidence/TASK-004-PC-A-rules.txt) |
| 获取与保存准备 | 12 PASS；已知纹理1、退出9 | [准备](evidence/TASK-004-PC-A-prepare.txt) |
| 取消订阅 / 重订阅 | 无本地副本，缺席后同包恢复PASS | [安装证据](evidence/TASK-004-PC-A-install.txt) |
| 跨进程恢复及持续战斗 | 255 PASS；已知运行0、退出9 | [恢复](evidence/TASK-004-PC-A-resume.txt) |
| 普通启动 | 无测试入口、主菜单PASS | [安装证据](evidence/TASK-004-PC-A-install.txt) |

按工坊README准备工作目录，使用官方上传器和同一条目ID，Steam订阅实际下载并核验SHA256。暂存本地同ID模组后依次运行：Start-ModGame -TravelerTest，Test-TravelerRun -Task003 -Headless；Start-ModGame -TravelerEndToEnd，Test-Task003EndToEnd -Phase Prepare；原生退出后取消订阅检查缺席，再重订阅同包，Start-ModGame -ResumeTask003 -SandboxDirectory返回值，Test-Task003EndToEnd -Phase Resume；最后普通Start-ModGame -Headless及Test-ModLoad。所有检查器CodeCommit均为S。

原始日志（仅本机，文件名可用于定位）：规则`game-20261020T0312531517988Z.log`；准备`game-20261020T0316310508323Z.log`；取消订阅缺席`game-20261020T0317431833236Z.log`；恢复`game-20261020T0317596373368Z.log`；普通启动`game-20261020T0320057755631Z.log`。私有测试保存命名空间ID为da534921446e4a01a0db0b873e04cd13，不复制正常用户存档。

后续提交仅整理报告、页面文案和交接；候选包仍引用S，不将其改称后续提交来源。最终页面状态更新仅改变workshop.json描述/更新说明，不改变content中的任何候选文件，更新后再次核对文件哈希及private可见性。

## 后续未验收项

第二台PC独立安装、完整图形检查、自然人工整局/平衡与双人专项。公开发行未执行；源码许可未代选。

## 保留范围

- 当前PC-A不等于PC-B独立安装，另一台PC需另行执行。
- 六牌版本完整UI、实机截图、鼠标操作、自然整局/平衡、双人联机及其他Mod组合未验收。
- 工坊前置无法锁定版本；3.4.7以外仍拒绝加载。未来依赖更新需重新适配回归。
- 引擎原生退出资源诊断按既有ENV-001单列；未知运行错误失败。
- 原始日志、账号信息、个人路径及测试存档只留本机；共享证据使用白名单摘录。
- 保留本次为验收创建的两个工坊订阅（本Mod及BaseLib），当前从工坊加载0.1.2/3.4.7；原本本地0.1.1/3.3.2保存在`.local/task004/original-mods`，不与订阅版重复放置。不自动删除Steam缓存或存档。
