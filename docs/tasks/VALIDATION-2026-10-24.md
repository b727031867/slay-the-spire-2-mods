# 2026-10-24 合并验证记录

## 范围与基线

用户授权提交、编译与测试，并将另一会话更新的资源素材纳入同一批验证。本轮不发布工坊。

- `6eafda93e641c65170d3bd884e0d2aa043a67bb7`：黏糊格挡预览、元素标签金色、中英文“元素之心”名称、天地交征雷分支X次原生攻击及对应回归检查。
- `1ca075c5905911b918a36783448875672b43f302`：4张事件图、8张事件遗物图、归途卡图及PNG路径；事件专项补充实际Texture2D加载检查。
- `e0d530c55314b2fbcc64142fce3578760069e74d`：补录Godot生成的测试文件UID，为最终构建代码身份。
- PC-A：游戏v0.107.1/build23811903，BaseLib3.4.7，模组0.3.17。

影响范围为卡牌行为、预览及事件图片。选用Cards（已包含加载、预览、黏糊和卡牌用例）及事件专项；不重复运行Visuals/Load。不涉及共享战斗生命周期、保存格式或工具链变化，未运行Full和磁盘跨进程专项。人工UI、联机和自然通关为NOT_RUN。

## 构建与离线检查

- 独立工作目录构建，两个构建均为clean source，ExportRelease/win-x64编译及Godot无界面导入、PCK导出成功。
- ComboRules 57项、日志分类26项、测试入口11项通过；中英文各19张固定元素牌金色标签及遗物名称检查通过。
- 第一包`6eafda9`的Cards通过708项，日志`.local/logs/game-20261023T1619027970544Z.log`及同名`.result.json`；其结果不作为最终包运行证据。
- 最终包：`.local/artwork-release/artifacts/Sts2Mod-0.3.17-v0.107.1-e0d530c55314/Sts2Mod`。
- 最终ZIP SHA256：`5FBFB562991DDAA73AB487B5F3110F183D12643E5CF493E31EDF15E06E7B7A27`。

| 最终核心文件 | SHA256 |
| --- | --- |
| Sts2Mod.dll | C9593C7A88A456D7930AF15257C45B41C460B4BECEAF4222CC389E55A165A885 |
| Sts2Mod.pck | AF3335EEBEE57A161DE0DEDFD3D4C6BD67DFFED9F198AFF95900B52530AB4091 |
| Sts2Mod.json | 0ED61F44AD1D737B4038FD876A513DDC850452F1CEB12ECFD41F1755A7D6F58A |

## 最终包运行验证

- Cards：PASS，708项断言；加载身份与最终核心一致，未知错误0。日志`.local/logs/game-20261023T1634470113323Z.log`及同名`.result.json`。
- 新增天地交征预览/实际多段伤害24项、黏糊预览/实际格挡/增长18项均PASS。
- 事件专项：PASS，361项断言，含4张事件图、8张遗物图及归途卡图的实际纹理加载；5角色事件与原生奖励流程通过，未知错误0。日志`.local/logs/game-20261023T1649149018102Z.log`，执行记录`.local/test-combined-fixes.log`。
- 两轮均为无GUI、独立命名空间存档；仅清理本轮测试进程。最终测试后按原文件哈希恢复工坊安装，将本地测试覆盖包移入`.local/current-fixes-test-20261023T1634459955827Z/tested-Sts2Mod`，未改正常存档。
- 本轮没有上传工坊或执行git push。已发布内容仍为此前建筑师结局修复；本轮代码与新素材仅提交及构建验证。图形观感、人工试玩、联机、自然通关、Full和磁盘跨进程专项均为NOT_RUN。
