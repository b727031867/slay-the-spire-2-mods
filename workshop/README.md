# 创意工坊工作目录

标题：旅行者·元素连携 / Traveler Elements。工程及存档ID仍为Sts2Mod。封面和规则图是原创说明设计图，不是实机截图；不能代替图形验收。

当前0.3.8已更新原条目3823282901，见[发布报告](../docs/tests/RELEASE-038.md)。本轮Steam API实测为好友可见；更新既有条目时将生成配置的visibility设为null以保留现状，仓库模板的private用于保守的新发布准备，不据旧文档改动实际可见性。

1. 使用PowerShell 7，复用已验收核心包：scripts/Build-ReleaseCandidate.ps1 -PackageDirectory '<核心目录>/Sts2Mod'。该入口只校验和包装，不构建。新代码先独立运行Build-Mod.ps1，再将返回的PackageDirectory交给包装入口，并记录新包来源和哈希。
2. 对构建生成的核心 `Sts2Mod` 目录运行 `scripts/Prepare-Workshop.ps1 -PackageDirectory <目录>`，生成 `.local/workshop/<完整源码提交>/`。仅此目录用于上传，不上传仓库或本机配置。
3. 从官方发行页下载 Mega Crit ModUploader v0.2.0 Windows x64，保存其ZIP哈希。查看生成的workshop.json、image.png、previews/rules.png及content。
4. Steam登录拥有本游戏的账号。使用官方 `ModUploader.exe upload -w <工作目录>` 进行私有上传；此动作会向Steam提交内容。若平台要求同意法律条款，交由账号持有人完成。
5. 成功后保存工作目录内mod_id.txt；跨PC继续更新时传递同一条目ID，不重复创建条目。不要把个人日志或凭据入库。后续更新使用同一ID和新候选内容。
6. 通过Steam订阅下载，比较Steam实际安装目录中文件SHA256和候选包，暂存本地副本后以无GUI从工坊加载。对BaseLib也检查真实下载版本；3.4.7以外目前会被本Mod拒绝。
7. 无GUI不验证排版、鼠标操作及自然整局。订阅/更新/取消订阅与PC-B独立验收分别记录。私有条目只能由有权限的账号访问，不把另一台PC等同于另一个账号。
8. 上传后使用 `WorkshopProbe inspect <id>` 获取封面和全部相册图地址，逐一确认 HTTP 200、图片格式及下载文件 SHA-256 与源文件一致。上传成功回调不等于图片可读取。相册断图时按 `tools/WorkshopProbe/README.md` 定点替换，并重新检查新地址。

所有图片小于1MB。配置按[官方v0.2.0模板](https://github.com/megacrit/sts2-mod-uploader/blob/v0.2.0/template/README.md)准备，dependencies为BaseLib的工坊ID3737335127；该字段无法固定依赖版本。tags暂留空，不擅用Tools & APIs标签。分支支持字段按官方提示在网页核查，不虚构版本兼容范围。

发布前须解决实际依赖兼容问题并审阅报告；公开可见性由项目负责人决定。无需为本任务改动卡牌数值或升级BaseLib基线。
