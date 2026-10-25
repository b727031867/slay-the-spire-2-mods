# 工坊公开权限与后续发布策略 · 2026-10-25

用户要求现有工坊条目及后续发布均公开；本任务使用codex/public-workshop-visibility分支，基线76acf94。此前仅限好友策略由本次明确授权替代，用户试玩成功后才发布新内容的门槛继续有效。

## 影响与改动

- WorkshopProbe保留既有friends-only命令，新增public命令并复用同一检查/提交路径；校验游戏ID2868840及当前账号所有权，仅调用SetItemVisibility和SubmitItemUpdate，不设置内容、标题、描述、图片或依赖。
- workshop.json、下次发布说明模板及当前规则改为public。Build-ReleaseCandidate和Prepare-Workshop拒绝非公开配置；Get-ReleaseSnapshot继续接受哈希完整的历史friends_only快照，供审计及最近三份制品轮转。旧好友快照不能直接准备新上传，需以原核心和当前public配置重新冻结。
- 同步根规则、README、开发/测试SOP、跨PC入口、工坊/工具说明及STATUS。历史发布报告和冻结材料保留原事实，不修改旧哈希。

## 远端操作与核验

- 初始inspect返回FriendsOnly、EResultOK、consumerApp2868840，条目3823282901已存在，沿用原条目。
- 在本机配置的.NET运行时、进程级SteamAppId/SteamGameId=2868840及隐藏窗口中执行public；submitted=true、result=k_EResultOK、agreementRequired=false。
- 提交后独立inspect确认k_ERemoteStoragePublishedFileVisibilityPublic。标题、封面及相册URL、Steam安装大小109781805字节和安装时间戳1792779050与操作前一致。本次没有上传新包或重新提交远端描述；已有0.3.17内容公开，后续说明模板随下一次批准的新内容上传。
- 本机原始证据：.local/logs/public-visibility-before.jsonl、public-visibility-retry-before.jsonl、public-visibility-update.jsonl及public-visibility-after.jsonl，不上传账号日志。

## 验证范围

- WorkshopProbe Release构建：0警告、0错误；新增公开路径已通过真实可见性提交和独立远端查询。
- Test-ReleasePreparation：80项PASS，含新候选public、拒绝friends_only/private、历史好友快照可读但不能生成新上传目录。
- Test-DeliverableRetention：36项PASS，含公开策略下历史好友快照仍能参与轮转；Test-ReleasePackaging：15项PASS。合计131项离线断言，删除仅作用于独立测试夹具。
- 现有4份历史好友快照只读Get-ReleaseSnapshot校验通过，哈希不变。文档链接及差异检查通过。
- NOT_RUN：游戏构建/启动、Full、图形、真实联机、存档迁移、新包部署、真实制品清理及内容上传；本次只改发布权限、工具及相关配置验证，不修改玩法、资源或存档。
