# 素材来源与分发说明

当前运行时卡牌、能力、遗物及角色插图使用本项目生成式PNG，元素状态图标使用本项目SVG。docs/art中的创建提示和哈希作为来源证明保留，不作为玩法或发布状态说明。

| 素材 | 来源与用途 |
| --- | --- |
| 六张元素牌与派蒙PNG | OpenAI内置image_gen按项目提示生成，未输入外部作品图片；见[生成记录](art/TASK-005-prompts.md) |
| 12张新牌、起始打击/防御、罗盘/连携图、角色body/icon/select_background，共19项PNG | 内置image_gen；头像参考项目生成角色；见[逐项记录](art/TASK-009-prompts.md)及assets JSON |
| character/head_icon.png | 内置image_gen编辑项目头像，用于Character.Icon及面部特写；见[头像记录](art/TASK-010-portrait.md) |
| ui/element_none、wind、rock、lightning.svg | 项目代码绘制的正式小尺寸状态图标，无外部图片输入 |
| ui/reward_block、attack、draw.svg及旧插图SVG | 项目矢量源码，当前没有常驻面板使用奖励图标；运行时卡牌和角色插图使用PNG |
| 连携特效 | ElementComboVfx.cs使用Godot基本线条绘制，无外部纹理或音频 |
| 工坊image.png及rules.png | New-WorkshopArtwork.ps1以几何线条、色块、系统字体绘制的封面/规则图，非游戏截图、非生成式图片 |
| 原生能量、卡框、区域字体、音效及部分通用演出 | 从合法游戏运行时引用，不随Mod复制分发；能量采用ironclad前缀，行内24×24、提示74×74 |

生成式素材应按平台要求披露，来源说明不代替平台申报。系统字体不随包分发。本项目未导入主题作品官方立绘、音频、字体或游戏资源，复用原生演出不代表自制全新骨骼动画。

作者为GitHub项目所有者b727031867，仓库未选择开源许可；公开查看源码不自动授予第三方复制、修改或再分发权。主题及游戏名称权利属于各自权利方，本说明不授予相关作品使用许可。

BaseLib从原作者发行页独立获取，不打入Mod ZIP；游戏程序集、上传器、Steamworks探针依赖、个人配置不随玩家包分发。
