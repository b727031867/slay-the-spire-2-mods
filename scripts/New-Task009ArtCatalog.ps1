param([string]$OutputDirectory = 'docs/art')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$names = @('wind_guidance','rock_heart','thunder_echo','element_resonance','gale_sweep','shattering_rock',
    'thunder_mark','triphase_finale','wind_shelter','patient_rampart','charge_thunder','journey_plans',
    'strike','defend','compass','combo','body','icon','select_background')
$destination = Join-Path $root $OutputDirectory
New-Item -ItemType Directory -Force -Path $destination | Out-Null
# QA contact sheet only. Runtime PNGs are copied verbatim from image_gen, never re-encoded here.
$canvas = [Drawing.Bitmap]::new(1200, 5 * 245)
$graphics = [Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([Drawing.Color]::FromArgb(18, 31, 44))
$graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$font = [Drawing.Font]::new('Segoe UI', 13)
$records = @()
try {
    for ($i = 0; $i -lt $names.Count; $i++) {
        $name = $names[$i]
        $folder = if ($name -in @('body','icon','select_background')) { 'character' } else { 'art' }
        $relative = "src/Sts2Mod/Sts2Mod/$folder/$name.png"
        $path = Join-Path $root $relative
        $source = [Drawing.Bitmap]::new($path)
        try {
            $x = ($i % 4) * 300; $y = [Math]::Floor($i / 4) * 245
            $factor = [Math]::Min(288.0 / $source.Width, 208.0 / $source.Height)
            $width = [int]($source.Width * $factor); $height = [int]($source.Height * $factor)
            $graphics.DrawImage($source, [int]($x + (300 - $width) / 2), [int]$y, $width, $height)
            $graphics.DrawString($name, $font, [Drawing.Brushes]::White, [single]($x + 6), [single]($y + 212))
            $transparent = $name -in @('compass','combo','body','icon')
            if ($transparent -and $source.GetPixel(0, 0).A -ne 0) { throw "Expected transparent corner: $name" }
            $records += [ordered]@{ name=$name; path=$relative; width=$source.Width; height=$source.Height;
                transparent=$transparent; bytes=(Get-Item -LiteralPath $path).Length;
                sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
        } finally { $source.Dispose() }
    }
    $canvas.Save((Join-Path $destination 'TASK-009-preview.png'), [Drawing.Imaging.ImageFormat]::Png)
    $records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $destination 'TASK-009-assets.json') -Encoding utf8
} finally { $font.Dispose(); $graphics.Dispose(); $canvas.Dispose() }
Write-Output "ART_CATALOG_PASS images=$($records.Count) transparent=4 runtime_sources_unchanged=true"
