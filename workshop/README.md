# 创意工坊工作目录

标题：旅行者·元素连携 / Traveler Elements。工程及存档ID仍为Sts2Mod。封面和规则图是原创说明设计图，不是实机截图；不能代替图形验收。

1. 使用 PowerShell 7，从干净提交运行 `scripts/Build-ReleaseCandidate.ps1 -DeviceId PC-A`（另一台改PC-B）。
2. 对构建生成的核心 `Sts2Mod` 目录运行 `scripts/Prepare-Workshop.ps1 -PackageDirectory <目录>`，生成 `.local/workshop/<完整源码提交>/`。仅此目录用于上传，不上传仓库或本机配置。
3. 从官方发行页下载 Mega Crit ModUploader v0.2.0 Windows x64，保存其ZIP哈希。查看生成的workshop.json、image.png、previews/rules.png及content。
4. Steam登录拥有本游戏的账号。使用官方 `ModUploader.exe upload -w <工作目录>` 进行私有上传；此动作会向Steam提交内容。若平台要求同意法律条款，交由账号持有人完成。
5. 成功后保存工作目录内mod_id.txt；跨PC继续更新时传递同一条目ID，不重复创建条目。不要把个人日志或凭据入库。后续更新使用同一ID和新候选内容。
6. 通过Steam订阅下载，比较Steam实际安装目录中文件SHA256和候选包，暂存本地副本后以无GUI从工坊加载。对BaseLib也检查真实下载版本；3.4.7以外目前会被本Mod拒绝。
7. 无GUI不验证排版、鼠标操作及自然整局。订阅/更新/取消订阅与PC-B独立验收分别记录。私有条目只能由有权限的账号访问，不把另一台PC等同于另一个账号。

所有图片小于1MB。配置按[官方v0.2.0模板](https://github.com/megacrit/sts2-mod-uploader/blob/v0.2.0/template/README.md)准备，dependencies为BaseLib的工坊ID3737335127；该字段无法固定依赖版本。tags暂留空，不擅用Tools & APIs标签。分支支持字段按官方提示在网页核查，不虚构版本兼容范围。

发布前须解决实际依赖兼容问题并审阅报告；公开可见性由项目负责人决定。无需为本任务改动卡牌数值或升级BaseLib基线。
