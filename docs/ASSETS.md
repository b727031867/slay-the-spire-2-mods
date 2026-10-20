# 素材记录

TASK-001 的 `src/Sts2Mod/Sts2Mod/art/wind.svg`、`rock.svg`、`combo.svg` 是本项目通过矢量代码绘制的占位插图/图标：风刃、岩盾和元素连招符号。未导入《原神》或其他作品的图片、音频或角色立绘；这些自制素材随项目源码分发。

游戏原生卡框、字体、攻击特效和思考气泡只通过已安装游戏的资源路径在运行时调用，不随安装包再分发。BaseLib 由依赖安装脚本从作者发行页独立获取，Mod ZIP 不包含该框架、游戏程序集或游戏资源。

后续替换正式美术时，在此逐项记录来源、作者和许可；本条记录不为未来新增素材自动授权。

TASK-002新增`lightning.svg`、`tailwind.svg`、`rock_advance.svg`、`thunder_prep.svg`和`paimon.svg`，均为本项目通过SVG代码绘制的占位图，未导入外部图片；随源码分发。

TASK-004：`workshop/assets/image.png`（1024×1024封面）、`rules.png`（1280×720规则说明图）由本项目 `scripts/New-WorkshopArtwork.ps1` 用几何线条、色块和本机系统字体绘制，未使用外部图片或生成式图像；不是游戏截图。脚本可在安装微软雅黑的 Windows 上重建，字体不随包分发。页面明确标注单人测试和未验收范围。

作者：GitHub项目所有者 b727031867。当前仓库未选择开源许可；公开查看源码不自动授予第三方复制、修改或再分发权。主题名称及游戏名称的权利属于各自权利方；本说明不授予这些作品的使用许可。本项目不附带其官方图片、音频、字体或游戏资源。BaseLib独立从原作者取得，不打入本Mod包；官方上传器和Steamworks探针依赖只在本地工具目录使用，不随玩家包分发。

TASK-005 / 0.1.3：运行时改用`src/Sts2Mod/Sts2Mod/art/`下wind.png、rock.png、lightning.png、tailwind.png、rock_advance.png、thunder_prep.png及透明paimon.png。七张图由OpenAI内置image_gen工具按本项目提示生成，未输入外部作品图片；不是手绘或游戏原生美术。提示词及用途见[生成记录](art/TASK-005-prompts.md)。原SVG及其历史归属保留，但不再作为这七项运行时插图。

三种连携特效由本项目ElementComboVfx.cs用Godot基本线条绘制，无外部纹理或音频。系统字体与游戏原生卡框不随本Mod重新分发。生成式插图的披露应保留在发布说明和平台要求的相应字段中；本记录不代替平台申报流程。

## 0.2.0 角色框架素材

0.3.0新卡及能力图标：`wind_guidance.svg`、`rock_heart.svg`、`thunder_echo.svg`、`element_resonance.svg`、`gale_sweep.svg`、`shattering_rock.svg`、`thunder_mark.svg`、`triphase_finale.svg`、`wind_shelter.svg`、`patient_rampart.svg`、`charge_thunder.svg`、`journey_plans.svg`均由本项目通过矢量代码绘制，为可辨识的临时牌图/图标。没有导入外部作品素材，不是生成式图片，不冒称最终角色美术。

`Sts2Mod/character/body.svg`、`icon.svg`、选人背景场景、战斗动画场景，以及 `art/compass.svg`、`strike.svg`、`defend.svg` 为本项目代码绘制的原创简化矢量素材，不是生成式图片或实机截图。角色为通用旅行者概念，不使用主题作品立绘。动画为Godot属性动画。原生能量、拖尾、手势、营地/商人及音效通过游戏已有资源路径引用，不打包复制游戏素材；属于临时演出占位，后续可替换。既有六张卡图和派蒙图标继续使用TASK-005记录的生成式插图。
