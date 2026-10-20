$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) { throw 'Close the game before removing the mod.' }
$projectRoot = Split-Path $PSScriptRoot -Parent
$modsRoot = [IO.Path]::GetFullPath((Join-Path $env:STS2_GAME_DIR 'mods'))
$target = [IO.Path]::GetFullPath((Join-Path $modsRoot 'Sts2Mod'))
if ((Split-Path $target -Parent) -ne $modsRoot -or (Split-Path $target -Leaf) -ne 'Sts2Mod') { throw 'Invalid removal target.' }
if (!(Test-Path -LiteralPath $target)) { Write-Output 'Sts2Mod is already absent.'; return }
foreach ($path in @($modsRoot,$target)) {
    if ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Refusing a redirected removal path.' }
}
$manifest = Get-Content -Raw -LiteralPath (Join-Path $target 'Sts2Mod.json') | ConvertFrom-Json
if ($manifest.id -ne 'Sts2Mod') { throw 'Directory ownership check failed.' }
$allowed = @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt')
if (@(Get-ChildItem -LiteralPath $target -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $allowed -or ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) }).Count) { throw 'Unexpected files in mod directory; preserving it.' }
$backup = Join-Path $projectRoot ('.local/backups/removed-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
New-Item -ItemType Directory -Force -Path $backup | Out-Null
Copy-Item -LiteralPath $target -Destination (Join-Path $backup 'Sts2Mod') -Recurse
# The resolved absolute path was checked above to be exactly <configured game>/mods/Sts2Mod.
Remove-Item -LiteralPath $target -Recurse -Force
Write-Output "Removed Sts2Mod. BackupDirectory=$backup"
Write-Output 'BaseLib, other mods and saves were preserved. Restore with Deploy-Mod.ps1 -PackageDirectory <backup>/Sts2Mod.'
