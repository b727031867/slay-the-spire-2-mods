param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$OutputFile)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$repo = Split-Path $PSScriptRoot -Parent
$poses = @(Get-Content -LiteralPath $LogFile | Where-Object { $_.StartsWith('STS2MOD_POSE_SNAPSHOT ') } |
    ForEach-Object { $_.Substring('STS2MOD_POSE_SNAPSHOT '.Length) | ConvertFrom-Json })
if (!$poses.Count) { throw 'No native rig pose snapshots in this test log.' }
$atlas = [Drawing.Image]::FromFile((Join-Path $repo 'src/Sts2Mod/Sts2Mod/character/layers/atlas.png'))
$sheet = [Drawing.Bitmap]::new(600 * $poses.Count, 660)
$graphics = [Drawing.Graphics]::FromImage($sheet)
$font = [Drawing.Font]::new('Arial', 17)
$brush = [Drawing.SolidBrush]::new([Drawing.Color]::White)
try {
    $graphics.Clear([Drawing.Color]::FromArgb(35, 44, 49))
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    for ($i = 0; $i -lt $poses.Count; $i++) {
        $graphics.DrawString($poses[$i].pose, $font, $brush, [single](600 * $i + 20), [single]15)
        foreach ($part in ($poses[$i].parts | Sort-Object z -Stable)) {
            $m = $part.matrix
            $r = $part.region
            $points = [Drawing.PointF[]]@(
                [Drawing.PointF]::new([single](600*$i+250+1.75*$m[4]), [single](620+1.75*$m[5])),
                [Drawing.PointF]::new([single](600*$i+250+1.75*($m[4]+$r[2]*$m[0])), [single](620+1.75*($m[5]+$r[2]*$m[1]))),
                [Drawing.PointF]::new([single](600*$i+250+1.75*($m[4]+$r[3]*$m[2])), [single](620+1.75*($m[5]+$r[3]*$m[3])))
            )
            $graphics.DrawImage($atlas, $points, [Drawing.RectangleF]::new($r[0],$r[1],$r[2],$r[3]), [Drawing.GraphicsUnit]::Pixel)
        }
    }
    $sheet.Save([IO.Path]::GetFullPath($OutputFile), [Drawing.Imaging.ImageFormat]::Png)
} finally {
    $graphics.Dispose(); $sheet.Dispose(); $atlas.Dispose(); $font.Dispose(); $brush.Dispose()
}
Write-Output "Offline pose reconstruction: $OutputFile (not an in-game screenshot)"
