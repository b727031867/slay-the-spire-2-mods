# 旅行者篝火休息图

2026-10-21使用内置image_gen工具，以项目已有生成角色body.png为身份与服装参考，生成旅行者的坐姿休息插图。金发、青绿披风、白金外套、深色裤装、护臂和佩剑延续现有角色；暖色火光来自右下方。原生成立绘保留，没有输入外部作品立绘。

- 保存位置：src/Sts2Mod/Sts2Mod/character/rest_site.png。
- PNG为1254 × 1254、RGBA，alpha范围0–255，使用真实透明背景。
- SHA256：DAD4F9649C19F462418F93E4D2DA9A29F76677DE984511ADE77F23278F948F39。
- 接入：Traveler.CustomRestSiteAnimPath指定character/rest_site.tscn，由BaseLib转换成原生NRestSiteCharacter；不再继承铁甲战士的篝火角色场景。
- 运行时插图高度约451px，坐姿石座附近作为角色座位原点；图片随ControlRoot及命中框一起镜像。气泡锚点和选择轮廓沿用原生结构。
- 各幕环境、篝火、休息/锻造按钮由游戏原生场景提供；此图专门绘制旅行者休息形象，没有把火焰、菜单、按钮或背景矩形烘焙到图片中。
- 素材内容和透明通道已检查，游戏内位置、遮挡与原生环境的融合仍需人工试玩；无GUI验证范围见[篝火验证记录](../tests/REST-SITE.md)。

## 最终生成提示（原文）

Use case: identity-preserve. Asset type: production transparent 2D rest-site character illustration for a deckbuilding roguelike campfire scene. Input image is ONLY a character identity and costume reference, not a pose to retain. Paint the SAME adult blond male traveler, same face, tousled sandy blond hair, teal eyes, teal scarf and cape, cream white gold-trimmed long coat, navy clothing, brown boots and gold bracers, same ornate silver sword. Full body in a relaxed seated pose on a small low weathered stone, three-quarter profile facing RIGHT toward an off-canvas campfire. One knee raised, elbow casually resting on knee, shoulders relaxed, a peaceful tired expression with a faint smile. Sword safely sheathed or resting beside his left boot, never brandished. Teal cape drapes naturally behind and onto the stone. Warm orange firelight comes from lower right, restrained cool blue ambient light on left. Painterly fantasy game illustration, cohesive with the reference, clean readable silhouette at 350px tall. Entire character, seat and boots visible with generous transparent margins, centered composition square 1024x1024. ONLY the isolated seated character, small seat and sword. Actual transparent alpha background. NO landscape, NO actual fire/flames or bright halo, NO floor rectangle or opaque black background, NO text, NO logo, NO extra characters, NO menu or UI. The game supplies the campfire, environment and buttons; do not paint them into this asset.
