# 篝火休息图开发验证

2026-10-21：旅行者篝火专属休息图已接入并安装本地。新包Character无GUI71项通过（含篝火12项），普通启动通过，未知错误0；新图在游戏中的构图与遮挡待人工试玩。本次未提交、合入main或更新工坊。

## 修改与素材

Traveler原先继承PlaceholderCharacterModel的铁甲战士CustomRestSiteAnimPath。现指定character/rest_site.tscn，由BaseLib转换为原生NRestSiteCharacter，显示旅行者的透明坐姿休息插图。各幕环境、篝火和原生选项保留；没有为其他角色替换资源，也没有修改休息、锻造或战斗规则。

图片1254×1254、RGBA，高度约451px，沿用金发、青绿披风、白金外套及佩剑，接受右下方的暖色火光。[来源、提示与素材哈希](../art/TRAVELER-REST-SITE.md)已记录。

## 本包身份

- 分支codex/traveler-combat-presentation，基于1f24c71df656a119b8da4e18db8b44866aa36cb5，dirty=true，包含此前战斗演出实现及本次篝火资源。
- 包：artifacts/Sts2Mod-0.3.6-v0.107.1-1f24c71df656-dirty.zip。
- ZIP SHA256：B61CB5780BC82402D62394DA7726D7E2CA8594473E5AF6ECA7099D483CCC4FBB。
- DLL SHA256：A7D8B6CF1E3E8A3F1AC99FC9354F4E77CBD848C8287EC87933175F10363D395E。
- PCK SHA256：1669A06EFAAA89F6D075166EA55EE7E3904D902B24FEC3C5AF16E057379433B5。
- src/scripts/tests文件清单SHA256：B49CCF9CA20C3EE35F176178508FF7EDBA2D1D1CA8974D6516AACC6E13C15EB2；算法同战斗演出验证，清单保留在.local/restsite/source-snapshot.txt。
- 游戏v0.107.1/build23811903、BaseLib3.4.7。此前战斗演出443项全量属于[冻结的战斗演出包](COMBAT-PRESENTATION.md)，不作为本包全量重跑。

## 验证结果

| 检查 | 证据 |
| --- | --- |
| 构建、资源导入与导出 | PASS，0警告、0错误；.local/restsite/build.log |
| Character | PASS，71项；PID27984，game-20261021T0456461077972Z.log，独立存档沙箱2fd22cb839f24a3db7141835d0b6c4cc |
| 其中篝火专项 | PASS，12项：自定义路径、铁甲资源独立、原生房间挂载、透明贴图导入、无继承Spine、尺寸、交互节点、休息/锻造按钮、镜像与命中框同步、反向恢复、原生气泡、离开房间释放 |
| 普通启动 | PASS；PID27912，game-20261021T0459121461348Z.log，初始化一次、主菜单到达 |
| 统一入口合成测试 | PASS，11项；不作为实际游戏加载证明 |

两次游戏运行均为--headless隐藏进程，未知错误0、原生无GUI错误0，结束后只清理本轮进程；当前没有测试游戏进程。无键鼠输入、无图形游戏启动，不等待人工选牌。

原生篝火房间验证通过EnterRoomDebug进入，在原生对象上检查选项与气泡，然后进入新的受控战斗检查旧节点释放；没有自动执行休息回血或锻造选牌，不冒称自然跑图、多人或图形验收。新图跨幕的实际画面构图、光照融合与遮挡仍待人工检查。

## 本地安装

本地mods/Sts2Mod四文件与本包逐项匹配，保留固定BaseLib3.4.7。正常启动游戏即可试玩，新开篝火场景时会使用旅行者专属休息图；正在运行的游戏需要退出后重开才能加载新包。本轮测试已结束，不自动重启图形游戏。工坊保持原0.3.6内容。
