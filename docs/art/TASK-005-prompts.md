# TASK-005 生成式插图记录

工具：内置image_gen（imagegen技能默认模式），2026-10-20。每项独立生成，无参考图。六张卡图transparent_background=false；paimon为true。原始输出直接复制为项目PNG，不覆写默认生成目录。未使用CLI/API密钥。

共同提示词：

```text
Use case: stylized-concept. Production landscape card illustration for a fantasy deckbuilding roguelike mod, no card frame, no UI, no letters, no numbers, no watermark. Original stylized hand-painted storybook fantasy, chunky readable shapes, confident dry-brush edges, textured deep navy background, warm parchment highlights, restrained painterly shading and dramatic lighting. Composition must read when cropped to a wide 500:380 card portrait and at tiny hand-card scale. Clear central action and strong silhouette, keep important subjects inside central 75%. Not a geometric placeholder icon. 
```

## wind.png

在共同提示词后追加：

```text
Asset: Wind Blade. One close-up armored gauntlet sweeping a short sword diagonally upward, a large crescent ribbon of mint turquoise wind curling along its blade, a few leaves and dust streaks. Focus on weapon and hand; no full character, no recognizable existing character. The blade slash clearly moves forward. Color: jade mint, ivory, deep navy.
```

## rock.png

在共同提示词后追加：

```text
Rock Guard. A broad layered amber stone shield floating upright in front of a cropped armored forearm, braced against one visible incoming impact from the left. Solid heavy silhouette, faceted ochre slabs, ivory impact chips. Gold ochre and warm stone, navy shadows. Protective stationary defensive posture.
```

## lightning.png

在共同提示词后追加：

```text
Lightning Thrust. A cropped armored hand driving a narrow spear-like sword straight toward one distant dark stone target. A sharp violet-white lightning bolt concentrated on the weapon tip, energetic perspective and jagged arcs. Clear single-target forward thrust, amethyst lavender and ivory highlights, navy backdrop.
```

## tailwind.png

在共同提示词后追加：

```text
Tailwind Prep. A leather travel satchel open on a worn stone ledge, two folded parchment maps and a rolled scarf lifted into a smooth mint wind spiral. Quiet resourceful preparation, no weapon and no person. Warm brown satchel and ivory parchment contrasted with mint jade wind and navy background. Readable broad shapes.
```

## rock_advance.png

在共同提示词后追加：

```text
Rock Advance. A cropped armored boot and shield-bearing forearm lunging forward through a rolling wave of ochre stone slabs, one clenched fist projecting a diagonal rock ram. Strong forward momentum AND a protective layered stone shoulder wall, gold ochre and ivory, navy shadows. Tight focused action, no whole body, no recognizable character.
```

## thunder_prep.png

在共同提示词后追加：

```text
Thunder Prep. A pair of armored gauntlets held defensively around a glowing lavender orb, a broad curved protective electric ward with one suspended ivory scroll beside it. Calm gathering energy, not a stabbing action. Rounded silhouette, pale amethyst and white charged light, dark navy backdrop, clear guarding and preparation.
```

## paimon.png

在共同提示词后追加：

```text
Asset: a single relic inventory icon of a cheerful original floating travel-companion charm: tiny ivory cloak with star-shaped gold halo and simple smiling face, lavender scarf, two small outstretched sleeves. One compact bold silhouette, minimal internal details, centered and large. Warm ivory and gold with restrained lavender accents, no text or circle border. Transparent background, not a full scene. Must be readable at 48 by 48 pixels. No detailed realistic face.
```

人工检查：已查看七图联系表，主体、配色和用途匹配，未见水印或UI文字；原图在游戏内缩放、裁切及小图可读性仍需人工实机确认。派蒙PNG含真实透明通道。所有项目引用均指向仓库中的文件。

