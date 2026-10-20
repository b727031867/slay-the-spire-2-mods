# 工坊相册断图修复

日期：2026-10-20。条目：3823282901。版本：0.1.3。

用户截图显示右侧封面正常，左侧相册图损坏。通过 Steam 客户端 API 查询确认相册仅有一张 `rules.png`（index 0）。封面 URL 返回 HTTP 200；相册图 URL 返回 HTTP 404：

`https://images.steamusercontent.com/ugc/10147230532007465738/673443C71AD39B1EA1E702F07CC8E12C5256637A/`

使用 SteamUGC.UpdateItemPreviewFile 原位重新上传仓库中的 `workshop/assets/rules.png`。提交结果 `k_EResultOK`，无需新条款确认。重新查询得到：

`https://images.steamusercontent.com/ugc/16800097179411441048/673443C71AD39B1EA1E702F07CC8E12C5256637A/`

下载验证：HTTP 200、Content-Type image/png、53,296 字节，与本地原图完全一致，SHA-256：

`85213C1AC9D0B7D0B38DB902C1FAD46BF8F54557EB98542DEAF510B5BEA6C766`

仅替换相册图片；封面 URL、Mod 安装文件大小和时间戳保持一致，条目仍为 private。本次未运行游戏。工具编译通过（0 警告、0 错误）。验证范围为 Steam API 返回的新链接及实际图片文件；未操控用户的 Steam 窗口，已有页面可能需要重新进入才能刷新链接。

为后续上传补充封面及全部相册 URL 的读取与哈希核验步骤。404 的底层平台原因尚未确认，不将其归因于图片格式或客户端缓存。
