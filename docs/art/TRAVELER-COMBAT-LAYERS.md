# 角色分层素材来源

生成日期：2026-10-21。使用内置image_gen工具，以项目已有生成立绘src/Sts2Mod/Sts2Mod/character/body.png作为编辑参考，生成透明分层图集；没有输入外部作品立绘。选定输出复制到项目character/layers/atlas.png，不覆盖原立绘。

- 图集：1448 × 1086，RGBA透明PNG。
- SHA256：ADFE0BD64430DCFE69308BA38C358DB34DE24401538B98A120C888EEFAC78C81。
- 使用11个部件：头、躯干、后披风、前衣摆、持剑上臂、持剑前臂与手、剑、另一侧上臂、另一侧前臂与手、左腿、右腿。
- 运行时以TravelerRig中的显式AtlasTexture区域引用，不创建临时裁切文件或做GPU纹理回读。图集的实际留白及剑柄跨格位置以选定输出为准，不假设生成工具严格遵守网格边界。
- 已检查图集内容和透明通道；游戏画面中的关节接缝、遮挡和动作观感尚待人工验收。该素材不是原作品官方素材，也不表示采用了Spine骨骼。

## 最终生成提示（原文）

Use case: identity-preserve. Production asset: a layered 2D game character rig atlas for the EXACT character in reference: same adult male face, blond tousled hair, teal flowing scarf/cape, cream gold coat, dark navy trousers, brown boots, gold bracers and ornate silver sword. Keep painterly style and side-facing-right orientation. Create ONE transparent sprite atlas arranged in a strict 4 column by 3 row equally-sized grid, preferably 2048x1536 pixels. No grid lines, text, labels, backgrounds or shadows. Each cell contains ONLY one detached body part, centered with generous transparent margins, absolutely no parts crossing cell boundaries. Reading left to right: row1 col1 head with hair and neck ONLY (no chest); col2 torso from neck to belt including scarf collar, cream vest and navy shirt, NO arms, legs or long cape; col3 flowing teal BACK scarf/cape ONLY; col4 cream FRONT coat tails with teal sash ONLY. Row2 col1 sword-side upper arm gold pauldron and cream sleeve ONLY, down-pointing; col2 sword-side forearm gold bracer and gloved hand ONLY, down-pointing, no sword; col3 entire ornate silver sword ONLY upright blade pointing DOWN with gold hilt above; col4 opposite upper arm cream sleeve ONLY down-pointing. Row3 col1 opposite forearm and empty gloved hand ONLY down-pointing; col2 left leg navy trousers and brown gold-trim boot ONLY straight down; col3 right leg navy trousers and brown gold-trim boot ONLY straight down; col4 EMPTY transparent. Fill hidden portions under joints naturally so there are no holes during rotation. Organic painterly edges, complete isolated parts, original identity and costume. Parts are animation-ready assets, not a posed character, no duplicate head or assembled body. Actual transparent alpha.
