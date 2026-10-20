# TASK-004 发布候选交接

PC-A当前范围已完成：27项纯规则、13项日志分类、94项工坊规则、12项获取/保存准备、255项跨进程恢复/受控战斗，及工坊更新、取消订阅、重新订阅和普通启动通过。UI/自然试玩/联机/PC-B仍NOT_RUN；公开发布尚未执行。当前PC保留工坊订阅版，原本本地模组移至`.local/task004/original-mods`备份，无测试游戏进程。

当前分支：`codex/TASK-004-release`。候选：0.1.2，游戏v0.107.1/build23811903、BaseLib3.4.7。工坊私有条目：[3823282901](https://steamcommunity.com/sharedfiles/filedetails/?id=3823282901)，继续更新同一ID。

干净候选来源S：`77e8f5e610b799b38281a421b7343223a8cf453d`。
候选ZIP：`Sts2Mod-0.1.2-v0.107.1-77e8f5e610b7-candidate.zip`。
SHA256：`73C319CC89EB422C78D6C5A17DB07760E85BBDBE831DCE5DA2924701EB95B89F`。
构建包包含Sts2Mod目录、玩家指南、更新记录和素材说明，不含BaseLib、开发工具及游戏文件。细节见[报告](../tests/TASK-004-PC-A-run-01.md)。

候选及校验附件已上传[GitHub草稿Release](https://github.com/b727031867/slay-the-spire-2-mods/releases/tag/untagged-eb690dddad712ef07c1e)，服务端大小和SHA256与本机一致。仅有仓库权限的账号可访问草稿，尚未公开发布。

## PC-B接续

1. 检查本地改动后fetch并切换`codex/TASK-004-release`，勿重置未提交内容；查看最新报告。
2. 玩家验收无需开发环境。登录有权查看私有条目的Steam账号（私有不对其他账号开放），确认游戏版本，订阅本Mod与BaseLib。避免本地mods目录里重复副本，原有文件先备份。
3. 确认Steam实际下载的BaseLib为3.4.7，核对Sts2Mod三个产物SHA256与build-info.txt，再进行无GUI启动、规则及保存恢复检查。不得手动复制内容到Steam缓存来制造订阅通过。
4. 开发验收另需本机PowerShell7、.NET SDK10.0.402、Godot.NET4.5.1、net9运行时和各自local.settings.json。通过本机配置加载环境，不复制PC-A路径。
5. 若自行重建，检出指定S并从干净目录运行Build-ReleaseCandidate；记录该包新校验值，不沿用PC-A ZIP哈希。

图形/自然试玩、联机继续独立列项。公开可见性、源码许可及正式替换美术由负责人另行确定；当前不自动合并main或公开GitHub Release。
