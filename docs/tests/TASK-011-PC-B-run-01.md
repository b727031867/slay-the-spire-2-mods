# TASK-011 / PC-B / 0.3.3

2026-10-21。实现常驻元素状态面板、按元素变化的状态图标、选牌连携奖励高亮。只读呈现，不改玩法数值或规则；分支codex/TASK-011-combo-visuals，工坊保持0.3.2。

## 开发记录

第一轮编译纠正原生RunManager.State为非公开属性的问题，改为从当前原生战斗的CreatureNodes读取LocalContext本机玩家，无反射私有字段、无生产DebugOnly调用。随后C#编译0警告/0错误。原生NPower在DisplayAmountChanged时只刷新数值，需针对ComboPower额外刷新%Icon纹理；其他Power不受影响。

开发包f33ad4dc0431012a2f00b77379562acae8d3561c-dirty。没有运行用户图形游戏；部署前检查没有游戏进程。为避免重复加载，将原工坊两个条目可恢复移动到.local/backups/workshop-before-033-20261020T184536Z，订阅未取消；本地安装固定BaseLib3.4.7及开发包。未改Steam缓存清单或用户存档。开发专项PID22756、日志game-20261020T1845368138938Z.log，结果待验证。

首轮及第二轮在VIS_always_visible_without_hover停下。诊断明确ActiveScreenContext为隔离新档案的NEarlyAccessDisclaimer，HUD按设计隐藏于信息弹窗之后；不是放宽生产屏幕过滤。TestSaveSandbox仅在已标记的唯一命名空间设置SeenEaDisclaimer默认值，用户设置/模组加载同意/平台条款不变；额外增加自建测试模态的隐藏/恢复检查。失败日志保留，不计作PASS。PID22756/7376均仅由AI清理。

开发第三包专项48项完整通过（包含15项选人准备与33项新UI检查），日志game-20261020T1850344725459Z.log，PID18136。原生状态栏实际%Icon纹理随出牌更新、四元素及三奖励资源、选中优先/取消回退、派蒙、零次数、跨回合、旧节点释放、模态隐藏恢复通过。100次预览未改变编码、能量、手牌或CombatTargets随机计数。随后又补充禁操作/非本人回合的保护和运行中切换语言的字体刷新，须由最终S回归验证，不将开发包当作最终通过。

七项SVG经Godot Image.load_svg_from_string CPU渲染联系表（.local/icons-033.png）本地查看，形状/颜色清楚；这只是符号美术检查，不是实际HUD截图。

## 最终验收

IN_PROGRESS。最终干净S、包哈希、无GUI规则/节点/完整回归及正常启动完成后记录；开发结果不冒充最终产物。

## 限制

纯无GUI可验证图标资源、原生节点接线/状态同步、透明输入属性和只读预览，不验证实际图形排版与鼠标/手柄手感。面板采用本机游戏原生区域字体，不复制游戏字体。矢量符号是正式小尺寸UI设计，不是此前替换的卡牌插图占位；原PNG牌图和立绘保持。自然Boss、多人和用户旧档迁移未测试。
