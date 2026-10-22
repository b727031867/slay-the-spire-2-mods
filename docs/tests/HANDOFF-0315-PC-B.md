# 0.3.15 跨PC交接 / 2026-10-23

用户最新要求：额度不足，优先提交保存，明天另一台PC继续。已经停止本次尚未完成的Full测试，只停止本次启动的PID20952；未触碰用户游戏或存档。不要将本轮中断计为Full通过。

## Git与范围

续接分支：`codex/BUG-element-tuning-hand-input`。玩法与测试提交 `d2ffc9b85f4ff2f1521048485c1842d6153d2f3f`；最终核心源码 `39310d5ae0455515307edc971f9c39ed5fbf3466` 仅将模组清单简介27张修正为28张。尚未合入main，origin/main仍为0.3.14的aca50e68。用户授权最终测试通过后提交、合main并更新原创意工坊，无需再请求同一授权。

已实现此前所有卡牌/遗物调整、元素调律输入锁、14张重复资源独立草图及雷蚀回响新草图；奖励池28张。完整规则见玩家指南、BALANCE-0315.md，审查见REVIEW-0315.md。原有用户未跟踪平衡分析草稿保留在PC-A，未纳入提交。

## 已完成

- C#与Godot干净构建通过，0警告0错误，最终S的dirty=false；游戏v0.107.1/build23811903、BaseLib3.4.7。
- 规则57、日志分类26、包装15、入口11项PASS，后三项为合成夹具。
- 开发候选Cards571项完整PASS，log=game-20261022T1629181103006Z.log，PID5360已清理。
- 干净玩法包d2ffc9b的Full已通过660条断言，无逻辑失败，用户中断后未完成整套。log=game-20261022T1644015171846Z.log；进程20952已停止，入口早退结果属于主动中断。
- 最终39310d5仅清单简介文本改变；两程序集完整反编译文本差异仅AssemblyInformationalVersion提交标识。未伪标Full为最终S的直接运行。
- 首轮编译补齐新牌Pool、Dazed命名空间。三相终击测试跨回合能量不足和Canonical原始费用误用已修正；Cards默认时限从300改1800秒，严格断言/错误分类不放宽。

## 下一台PC必须完成

1. 拉取本分支，读取AGENTS.md。定位本机PowerShell7，配置本机local.settings.json；不要复制PC-A绝对路径。检查依赖版本与游戏build。
2. 从最终核心39310d5或之后仅交接文档变更的干净工作树构建，记录真实源码身份，部署并跑完整Full（默认1800秒）。当前代码无已知未解决逻辑失败，但Full必须完整收尾。
3. 最终包运行SavePrepare；重新部署同包，以返回的SandboxDirectory运行SaveResume。跨PC不能使用PC-A隔离目录或拷贝用户存档。
4. 完成最终包Load/Character/Visuals。图形人工、联机/重连与自然平衡仍NOT_RUN，历史用户联机反馈不能替代本版验证。
5. 更新发布报告/STATUS/玩家指南状态，提交并合入main（先fetch，避免覆盖远端变化），推送。
6. 用Build-ReleaseCandidate包装；Prepare-Workshop后更新原条目3823282901，依赖3737335127，上传配置visibility=null保留好友可见（上传前后用Steam API核实）。不能把Prepare-Workshop默认private直接上传。已更新workshop中英说明、封面和规则图到0.3.15/28张。
7. 官方ModUploader v0.2.0，需在该PC验证工具与Steam登录。上传后通过Steam原生下载，核对上传/下载七文件及两张CDN图片哈希，再从实际工坊副本运行Load/Character/Visuals。移走本地覆盖前逐文件核对并备份，不能直接删除未核实内容。
8. 补充最终交付证据，清理仅自己创建的进程/临时工作树，不启动图形游戏。

## PC-A本地证据与安装状态

仓库`.local/release0315/`存构建日志、核心哈希、反编译差异、Full中断检查点；`.local/balance0315/`存开发回归；`.local/logs/`存实际日志。忽略目录不会随Git迁移，其他PC应重新构建/验证。

最终核心ZIP：`artifacts/Sts2Mod-0.3.15-v0.107.1-39310d5ae045.zip`，SHA256 `3F774443D1F06FB15B11A4557F062B74F3F067EBB6A3E3B7D09980B37CA0D8A8`。完整文件哈希见RELEASE-0315.md。

PC-A仍安装用于Full的干净d2ffc9b本地包（与最终S玩法一致），未切回实际工坊0.3.14；本机不运行游戏。最终S包已构建但未部署。两份工坊上传快照仅位于本机.local/workshop，均未上传；最终应使用39310d5身份，旧d2ffc9b快照不发布。临时构建工作树保留于.local/release0315-build，detached39310d5且Git干净；其包已复制主artifacts，可在续接后核实清理。
