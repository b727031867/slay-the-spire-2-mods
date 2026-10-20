param([Parameter(Mandatory)][string]$PackageDirectory, [switch]$InstallDependencies)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
$projectRoot = Split-Path $PSScriptRoot -Parent
if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) { throw 'Close the game before deploying.' }
$packagePath = (Resolve-Path -LiteralPath $PackageDirectory).Path
$info = Get-Content -Raw -LiteralPath (Join-Path $packagePath 'build-info.txt') | ConvertFrom-Json
if ($info.modId -ne 'Sts2Mod' -or $info.schemaVersion -ne 1) { throw 'Unsupported package identity.' }
$allowed = @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt')
if (@(Get-ChildItem -LiteralPath $packagePath -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $allowed }).Count) { throw 'Package contains unexpected files.' }
foreach ($name in $allowed[0..2]) {
    if ((Get-FileHash -LiteralPath (Join-Path $packagePath $name) -Algorithm SHA256).Hash -ne $info.files.$name) { throw "Package checksum mismatch: $name" }
}
$gameInfo = Get-Content -Raw (Join-Path $env:STS2_GAME_DIR 'release_info.json') | ConvertFrom-Json
if ($gameInfo.version -ne $info.gameVersion) { throw 'Package and installed game versions differ.' }
$steamApps = Split-Path (Split-Path $env:STS2_GAME_DIR -Parent) -Parent
$appManifest = Get-Content -Raw -LiteralPath (Join-Path $steamApps 'appmanifest_2868840.acf')
$actualBuild = [regex]::Match($appManifest, '"buildid"\s+"(\d+)"').Groups[1].Value
if ($actualBuild -ne $info.gameBuild) { throw 'Package and installed Steam builds differ.' }
if ((Get-FileHash -LiteralPath (Join-Path $env:STS2_GAME_DIR 'data_sts2_windows_x86_64/sts2.dll')).Hash -ne $info.gameAssemblySha256) { throw 'Package was built against different game assemblies.' }
$modsRoot = [IO.Path]::GetFullPath((Join-Path $env:STS2_GAME_DIR 'mods'))
New-Item -ItemType Directory -Force -Path $modsRoot | Out-Null
if ((Get-Item -LiteralPath $modsRoot).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Refusing a redirected mods directory.' }
$backupRoot = Join-Path $projectRoot ('.local/backups/' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
function Copy-OwnedMod([string]$id, [string]$source, [string[]]$files) {
    $target = [IO.Path]::GetFullPath((Join-Path $modsRoot $id))
    if ((Split-Path $target -Parent) -ne $modsRoot -or (Split-Path $target -Leaf) -ne $id) { throw 'Invalid deployment target.' }
    if (Test-Path -LiteralPath $target) {
        if ((Get-Item -LiteralPath $target).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Refusing a redirected mod directory.' }
        if (@(Get-ChildItem -LiteralPath $target -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $files -or ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) }).Count) { throw "Unexpected content in existing $id directory; preserving it." }
        $oldManifest = Get-Content -Raw -LiteralPath (Join-Path $target "$id.json") | ConvertFrom-Json
        if ($oldManifest.id -ne $id) { throw 'Existing directory belongs to a different mod.' }
        New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null
        Copy-Item -LiteralPath $target -Destination (Join-Path $backupRoot $id) -Recurse
    }
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    foreach ($file in $files) { Copy-Item -LiteralPath (Join-Path $source $file) -Destination (Join-Path $target $file) -Force }
    Write-Output "Deployed $id"
}
if ($InstallDependencies) {
    & (Join-Path $PSScriptRoot 'Install-ModDependencies.ps1')
    $baseSource = Join-Path $projectRoot ".local/dependencies/BaseLib/$($info.baseLibVersion)"
    Copy-OwnedMod 'BaseLib' $baseSource @('BaseLib.dll','BaseLib.json','BaseLib.pck')
} else {
    $baseManifest = Join-Path $modsRoot 'BaseLib/BaseLib.json'
    if (!(Test-Path -LiteralPath $baseManifest)) { throw 'BaseLib is missing. Use -InstallDependencies to install the pinned dependency.' }
    if ((Get-Content -Raw -LiteralPath $baseManifest | ConvertFrom-Json).version.TrimStart('v') -ne $info.baseLibVersion) { throw 'Installed BaseLib version differs from the package.' }
}
Copy-OwnedMod 'Sts2Mod' $packagePath $allowed
if (Test-Path -LiteralPath $backupRoot) { Write-Output "BackupDirectory=$backupRoot" }
Write-Output 'Restart the game to load the mod. No saves were changed by this script.'
