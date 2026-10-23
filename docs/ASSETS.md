# 素材来源与分发说明

当前运行时卡牌、能力、遗物及角色插图使用项目生成式PNG；TC_C19–TC_C28已换为用户在云盘中确认的卡牌PNG，5种能力使用独立透明PNG状态图标，5件遗物仍用SVG占位草图，元素连携状态图标使用本项目SVG。docs/art中的来源和哈希保留，不作为玩法或发布状态说明。

0.3.9本地候选的战斗角色直接引用原有character/body.png，以相同UV制作七层网格和18关节；未生成或修改栅格图片。旧11部件atlas保留历史，当前开发控制器不再使用它。见[同母稿方案](design/TRAVELER-NATURAL-MOTION.md)。

| 素材 | 来源与用途 |
| --- | --- |
| 六张元素牌与派蒙PNG | OpenAI内置image_gen按项目提示生成，未输入外部作品图片；见[生成记录](art/TASK-005-prompts.md) |
| 12张新牌、起始打击/防御、罗盘/连携图、角色body/icon/select_background，共19项PNG | 内置image_gen；头像参考项目生成角色；见[逐项记录](art/TASK-009-prompts.md)及assets JSON |
| character/head_icon.png | 内置image_gen编辑项目头像，用于Character.Icon及面部特写；见[头像记录](art/TASK-010-portrait.md) |
| character/layers/atlas.png | 内置image_gen以本项目body.png为参考生成11块分层角色素材，运行时通过AtlasTexture区域引用；见[提示、来源与哈希](art/TRAVELER-COMBAT-LAYERS.md) |
| character/rest_site.png | 内置image_gen以项目body.png为参考生成篝火坐姿休息插图，接入原生休息角色场景；见[提示、来源与哈希](art/TRAVELER-REST-SITE.md) |
| character/merchant.tscn | 商店角色场景复用本项目已生成body.png，等比缩放并校准脚底基线；未新增图片或复制原生角色素材 |
| ui/element_none、wind、rock、lightning.svg | 项目代码绘制的正式小尺寸状态图标，无外部图片输入 |
| ui/reward_block、attack、draw.svg及旧插图SVG | 项目矢量源码，当前没有常驻面板使用奖励图标；运行时卡牌和角色插图使用PNG |
| art/TC_C19–TC_C28.png、ui/powers/*.png | 用户2026-10-23生成、上传云盘并确认替换的10张卡牌封面和5张Buff图标；原始PNG字节直接接入，来源、对应关系与SHA256见[导入记录](art/CLOUD-ART-20261023.md) |
| art/drafts/TC_C19–TC_C28.svg、TC_R03–TC_R07.svg | 本项目独立几何占位草图，中文标签转为矢量路径，不分发字体；卡牌SVG保留历史，运行时卡牌及三相灌注状态已改用上述PNG，遗物仍引用SVG |
| 连携特效 | ElementComboVfx.cs使用Godot基本线条绘制，无外部纹理或音频 |
| 普通攻防、受击及六套元素特效 | TravelerRig控制分层关节；CombatElementVfx以Godot曲线、多边形及折线绘制特效，无新增外部音频或纹理 |
| 工坊image.png及rules.png | New-WorkshopArtwork.ps1以几何线条、色块、系统字体绘制的封面/规则图，非游戏截图、非生成式图片 |
| 原生能量、卡框、区域字体、音效及部分通用演出 | 从合法游戏运行时引用，不随Mod复制分发；能量采用ironclad前缀，行内24×24、提示74×74 |

生成式素材应按平台要求披露，来源说明不代替平台申报。系统字体不随包分发。本项目未导入主题作品官方立绘、音频、字体或游戏资源，复用原生演出不代表自制全新骨骼动画。

作者为GitHub项目所有者b727031867，仓库未选择开源许可；公开查看源码不自动授予第三方复制、修改或再分发权。主题及游戏名称权利属于各自权利方，本说明不授予相关作品使用许可。

BaseLib从原作者发行页独立获取，不打入Mod ZIP；游戏程序集、上传器、Steamworks探针依赖、个人配置不随玩家包分发。

0.3.16新增 `art/events/`：四张事件场景、六张遗物/契约图标与归途卡图，共11份原创SVG几何示意图，由代码绘制，无外部图片、字体、音频或生成式位图。它们是开发用示意美术，资源加载通过不等于游戏画面人工验收。

0.3.15本地开发新增 `art/drafts/TC_C28.svg`：雷蚀回响独立几何占位草图，无外部图片或字体。资源导入/游戏加载待后续集中验证。
