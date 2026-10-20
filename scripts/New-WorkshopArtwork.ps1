# Original vector-style layouts drawn with Windows GDI+. These are not game screenshots.
param([string]$OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'workshop/assets'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
function Brush([string]$hex) { [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($hex)) }
function Label($canvas,[string]$value,[float]$size,[string]$color,[float]$x,[float]$y,[bool]$bold=$false) {
    $style = if($bold){[Drawing.FontStyle]::Bold}else{[Drawing.FontStyle]::Regular}
    $font = [Drawing.Font]::new('Microsoft YaHei UI',$size,$style,[Drawing.GraphicsUnit]::Pixel)
    $brush = Brush $color
    try { $canvas.DrawString($value,$font,$brush,$x,$y) } finally { $font.Dispose(); $brush.Dispose() }
}
function ElementIcon($canvas,[int]$kind,[float]$cx,[float]$cy,[float]$radius,[string]$color) {
    $pen = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml($color),5)
    $brush = Brush $color
    try {
        $canvas.DrawEllipse($pen,$cx-$radius,$cy-$radius,2*$radius,2*$radius)
        if($kind -eq 0) {
            $canvas.DrawArc($pen,$cx-36,$cy-25,70,36,195,290)
            $canvas.DrawArc($pen,$cx-32,$cy-2,62,35,10,240)
            $canvas.DrawLine($pen,$cx-38,$cy+18,$cx+5,$cy+18)
        } elseif($kind -eq 1) {
            $points = [Drawing.PointF[]]@([Drawing.PointF]::new($cx,$cy-37),[Drawing.PointF]::new($cx+34,$cy-10),[Drawing.PointF]::new($cx+22,$cy+30),[Drawing.PointF]::new($cx-22,$cy+30),[Drawing.PointF]::new($cx-34,$cy-10))
            $canvas.DrawPolygon($pen,$points)
            $canvas.DrawLine($pen,$cx,$cy-37,$cx,$cy+30)
            $canvas.DrawLine($pen,$cx-34,$cy-10,$cx,$cy+2)
            $canvas.DrawLine($pen,$cx+34,$cy-10,$cx,$cy+2)
        } else {
            $points = [Drawing.PointF[]]@([Drawing.PointF]::new($cx+12,$cy-40),[Drawing.PointF]::new($cx-28,$cy+5),[Drawing.PointF]::new($cx-2,$cy+5),[Drawing.PointF]::new($cx-12,$cy+40),[Drawing.PointF]::new($cx+30,$cy-12),[Drawing.PointF]::new($cx+5,$cy-12))
            $canvas.FillPolygon($brush,$points)
        }
    } finally { $pen.Dispose(); $brush.Dispose() }
}
function Canvas([int]$width,[int]$height,[string]$path,[scriptblock]$draw) {
    $bitmap = [Drawing.Bitmap]::new($width,$height)
    $canvas = [Drawing.Graphics]::FromImage($bitmap)
    $canvas.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $canvas.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    try { $canvas.Clear([Drawing.ColorTranslator]::FromHtml('#111D29')); & $draw $canvas; $bitmap.Save($path,[Drawing.Imaging.ImageFormat]::Png) }
    finally { $canvas.Dispose(); $bitmap.Dispose() }
}
Canvas 1024 1024 (Join-Path $OutputDirectory 'image.png') {
    param($g)
    Label $g 'TRAVELER ELEMENTS' 24 '#9AADB8' 76 74 $true
    Label $g '旅行者' 108 '#F3EBDD' 68 155 $true
    Label $g '元素连携' 108 '#F3EBDD' 68 275 $true
    $line = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#344955'),2)
    try { $g.DrawLine($line,80,465,944,465); $g.DrawLine($line,80,857,944,857) } finally { $line.Dispose() }
    ElementIcon $g 0 230 610 87 '#7DDBC8'
    ElementIcon $g 1 512 610 87 '#EAC27C'
    ElementIcon $g 2 794 610 87 '#BEA4EA'
    Label $g '风' 36 '#7DDBC8' 211 725 $true
    Label $g '岩' 36 '#EAC27C' 493 725 $true
    Label $g '雷' 36 '#BEA4EA' 775 725 $true
    Label $g '18 张奖励牌  /  4 张能力  /  元素跨回合保留' 28 '#F3EBDD' 80 803
    Label $g '旅行者独立角色 · 单人技术测试版' 31 '#F3EBDD' 80 900 $true
    Label $g 'SLAY THE SPIRE 2 MOD  /  v0.3.5  /  非官方同人' 20 '#9AADB8' 80 960
}
Canvas 1280 720 (Join-Path $OutputDirectory 'rules.png') {
    param($g)
    Label $g '出牌顺序，让元素产生连携' 48 '#F3EBDD' 62 42 $true
    Label $g 'TRAVELER ELEMENTS  /  规则说明图 · 非游戏截图' 20 '#9AADB8' 65 112
    $colors = @('#7DDBC8','#EAC27C','#BEA4EA')
    $titles = @('风 ↔ 岩','岩 ↔ 雷','雷 ↔ 风')
    $rewards = @('+4 格挡','+4 基础攻击','+1 抽牌')
    $doubles = @('派蒙：8 格挡','派蒙：8 基础攻击','派蒙：2 抽牌')
    for($i=0;$i -lt 3;$i++) {
        $x=65+410*$i
        ElementIcon $g $i ($x+50) 225 45 $colors[$i]
        Label $g $titles[$i] 34 $colors[$i] $x 303 $true
        Label $g $rewards[$i] 40 '#F3EBDD' $x 365 $true
        Label $g $doubles[$i] 25 '#9AADB8' $x 430
    }
    Label $g '三类合计每人每回合最多 3 次 · 触发后保留最新元素' 30 '#F3EBDD' 65 525 $true
    Label $g '岩雷：防御技能随机追击；单体攻击追击原目标，死亡不转火。' 24 '#9AADB8' 65 583
    Label $g '单人测试 · 罗盘隔回合触发 · 共鸣第2次回能 · v0.3.5' 23 '#9AADB8' 65 645
}
Get-ChildItem -LiteralPath $OutputDirectory -Filter '*.png' | Select-Object Name,Length
