# 创意工坊工作目录

标题：旅行者·元素连携 / Traveler Elements。工程及存档ID仍为Sts2Mod。封面和规则图是原创说明设计图，不是实机截图；不能代替图形验收。

开发版本、已发布版本、分支、包身份与验证范围统一见[当前状态](../docs/STATUS.md)。2026-10-25用户明确要求现有条目与后续更新全部公开，配置统一使用public；好友联机测试也不改变公开权限。历史friends_only/private/visibility=null仅记录当时事实，不再作为后续上传配置。旧好友快照可用于历史核验和轮转，但重新上传前必须从原核心按当前public配置重新冻结。

1. 每个开发任务使用独立codex/分支，按影响范围检查通过后合入main。使用PowerShell 7，从干净提交运行Build-Mod.ps1一次，默认不生成中间ZIP。将核心目录交给 `scripts/Invoke-ReleasePreparation.ps1 -PackageDirectory '<核心目录>/Sts2Mod'`，默认部署并运行无GUILoad，再冻结快照和准备上传目录。已具备同包schemaVersion=2通过报告时加`-TestResultFile`复用，避免重新部署和测试；按累计影响传`-Suite`；只有用户明确要求全量回归才传Full。报告绑定方法、覆盖限制及阶段耗时见[开发说明](../docs/DEVELOPMENT.md)。入口不编译、不上传，PREPARED不代表发布完成。磁盘专项按改动范围执行。提供本地候选供用户试玩，必须记录用户明确确认该核心包试玩成功后才能上传；核心变化或重建后重新验证和确认，不能先上传工坊供试玩。联机仍为可选。
2. 统一入口成功后自动保留artifacts内最近3份交付，清理过期快照、ZIP、校验文件、准备报告及已核验的关联核心/上传副本；结果写入.preparation.json的Retention字段。旧包或上传副本校验异常时停止清理并报错，新候选保留。已有测试证据时使用-TestResultFile完成交付，不重复启动游戏。独立Build-ReleaseCandidate与Prepare-Workshop仍可用于冻结/准备，但不触发轮转；自定义输出到artifacts之外时轮转明确标记SKIPPED，范围详见[开发说明](../docs/DEVELOPMENT.md#制品保留与清理)。Prepare-Workshop只读取快照，生成 `.local/workshop/<完整源码提交>-<快照哈希前12位>/`；仅生成的工作目录用于上传，准备完成仍须等待用户试玩成功。
3. 从官方发行页下载 Mega Crit ModUploader v0.2.0 Windows x64，保存其ZIP哈希。查看生成的workshop.json、image.png、previews/rules.png及content。
4. Steam登录拥有本游戏的账号。使用官方 `ModUploader.exe upload -w <工作目录>` 进行公开上传（visibility必须为public）；此动作会向Steam提交内容。若平台要求同意法律条款，交由账号持有人完成。
5. 成功后保存工作目录内mod_id.txt；跨PC继续更新时传递同一条目ID，不重复创建条目。不要把个人日志或凭据入库。后续更新使用同一ID和新候选内容。
6. 通过Steam订阅下载，比较Steam实际安装目录中文件SHA256和候选包，暂存本地副本后以无GUI从工坊加载。对BaseLib也检查真实下载版本；3.4.7以外目前会被本Mod拒绝。
7. 无GUI不验证排版、鼠标操作及自然整局。用户试玩成功确认须在上传前记录，不能用上传后的加载检查代替；系统性图形、自然整局和联机验证未执行时仍记录NOT_RUN，不能从用户单次试玩推定通过。若用户要求联机测试，条目仍公开，并将跨PC与跨账号结果分别记录。
8. 上传后使用 `WorkshopProbe inspect <id>` 获取封面和全部相册图地址，逐一确认 HTTP 200、图片格式及下载文件 SHA-256 与源文件一致。上传成功回调不等于图片可读取。相册断图时按 `tools/WorkshopProbe/README.md` 定点替换，并重新检查新地址。

所有图片小于1MB。配置按[官方v0.2.0模板](https://github.com/megacrit/sts2-mod-uploader/blob/v0.2.0/template/README.md)准备，dependencies为BaseLib的工坊ID3737335127；该字段无法固定依赖版本。tags暂留空，不擅用Tools & APIs标签。分支支持字段按官方提示在网页核查，不虚构版本兼容范围。

后续开发和上传更新保持公开（public），复用条目3823282901。只有用户明确另行要求时才能更改可见性。上传前核对用户试玩成功确认及实际工作目录内workshop.json，旧候选的friends_only/private配置不得直接复用；上传后用WorkshopProbe inspect 3823282901确认远端为Public。只修改已发布条目的权限时，可按用户明确授权运行WorkshopProbe public 3823282901并再次inspect，不上传新内容。联机仍为可选，计划本身不代表多人兼容性已验收。发布前须解决实际依赖兼容问题并审阅报告。每次交付按[制品保留规则](../docs/DEVELOPMENT.md#制品保留与清理)仅保留最近3份可交付制品，清理更旧快照/ZIP、上传副本及中间包，保留必要证据。
