# 2026-10-31 旅行者夹击转身修复

基线：最新 origin/main；设备 PC-A，游戏/依赖版本由构建校验。
改动：TravelerRig.ApplyPose 每帧保留原生 Body.Scale.X 朝向符号，避免覆盖 SurroundedPower 的转身。
调用方：_Process → Advance → ApplyPose，以及 PlayAction 的即时姿势更新；ResetForPreview 只用于初始化和测试，仍恢复默认朝向。
风险：局部视觉变换；不修改目标、伤害、夹击状态、随机数或存档。无新增补丁、资源、配置或依赖。
必跑：构建、Visuals（含加载/未知错误检查）；新增原生夹击状态下左右敌人交替出牌，并验证攻击、防御、受击、死亡和复活动作更新后朝向保持。
不跑：Full（未要求）、其他玩法/事件/存档专项（未改变相关行为）、图形/联机/自然整局（未要求）。无界面逻辑检查不证明实机观感。
扩大条件：原生身体节点或坐标变换断言失败时重新评估；Full仍需明确请求。

验证：待构建和受影响专项。当前用户游戏正在运行，部署/无界面测试需等待用户自行退出，不终止用户进程。

构建：bf8b85b16bacbb88f3a9af2ef4eaa44a77f8103c 干净核心 Build-Mod 成功；git diff --check 通过。现有 TravelerSandwormTests.cs:70 的两条 nullable 警告未改动。
包：artifacts/Sts2Mod-0.3.18-v0.107.1-bf8b85b16bac/Sts2Mod（build-info.txt 记录完整哈希）。DLL SHA256 A89061A5DA932DF45F13C91D1EEBA96ACA1DACA09B6C6D682BC41EE8547033FB；PCK SHA256 2AF7E200016635CC50A854F36FD514355D7E2F632466B9AE29CE8098F8A1A813。
Visuals：BLOCKED，用户游戏 PID 24488 仍在运行；未部署、未启动测试进程、未合入main、未上传工坊。新增回归已编译，但尚未执行。
后续：用户退出游戏后，部署上述同包 → Invoke-ModTests -Suite Visuals 并绑定 PackageDirectory → 成功后合并/推送main → 复用报告执行 Invoke-ReleasePreparation（含保留清理）→ 本地用户试玩。
首轮 Visuals FAIL：左右动作断言通过，但 Cultists 夹具缺少 kaiser_crab_direction 音乐参数，严格检查拒绝。保留日志 game-20261031T1358240391528Z.log；改为第二幕原生 KaiserCrabBoss 夹具，使用真实左右敌人/夹击状态与音乐，并补严格日志用例校验。

最终核心：712a5edaf812cdb8a6a50041e76be9255fd1ec47，干净构建成功，Visuals273项PASS；原生螃蟹战左右交替攻击四次、两侧五种动作的朝向检查全部通过，未知错误0。日志 .local/logs/game-20261031T1400545163227Z.log，绑定报告同名.result.json；测试进程51784已由入口清理。DLL SHA256 1EC9B29177CC6689F8A3A013CD1E10DCB96F96A76A3A866E48C1E37C5B562CE7；PCK与首轮相同，未修改资源。
当前本机使用mods目录的候选与BaseLib3.4.7；原工坊两副本可恢复备份在.local/backups/facing-workshop-20261031T135822Z，避免双重加载。未改存档或线上工坊。
