param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
$path = (Resolve-Path -LiteralPath $PackageDirectory).Path
if (!(Test-Path -LiteralPath $path -PathType Container) -or
    ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
    throw 'A regular core package directory is required.'
}
$names = @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt')
$items = @(Get-ChildItem -LiteralPath $path -Force)
if ($items.Count -ne $names.Count -or @($items | Where-Object {
    $_.PSIsContainer -or $_.Name -notin $names -or !$_.Length -or
    ($_.Attributes -band [IO.FileAttributes]::ReparsePoint)
}).Count) { throw 'Core package must contain exactly four nonempty regular files.' }
$info = Get-Content -Raw -LiteralPath (Join-Path $path 'build-info.txt') | ConvertFrom-Json
if ($info.schemaVersion -ne 1 -or $info.modId -ne 'Sts2Mod' -or
    $info.dirty -isnot [bool] -or $info.dirty -or $info.codeCommit -notmatch '^[a-f0-9]{40}$') {
    throw 'A clean, identified Sts2Mod package is required.'
}
$hashes = [ordered]@{}
foreach ($name in $names) {
    $hashes[$name] = (Get-FileHash -LiteralPath (Join-Path $path $name) -Algorithm SHA256).Hash
    if ($name -ne 'build-info.txt' -and $hashes[$name] -ne $info.files.$name) {
        throw "Checksum mismatch: $name"
    }
}
$manifest = Get-Content -Raw -LiteralPath (Join-Path $path 'Sts2Mod.json') | ConvertFrom-Json
if ($manifest.id -ne $info.modId -or $manifest.version -ne $info.modVersion -or
    !$info.gameVersion -or !$info.gameBuild -or !$info.baseLibVersion) {
    throw 'Package metadata and manifest must identify the same version and dependencies.'
}
$baseDependencies = @($manifest.dependencies | Where-Object { $_.id -eq 'BaseLib' })
if ($baseDependencies.Count -ne 1 -or
    $baseDependencies[0].min_version -ne $info.baseLibVersion -or
    !$manifest.min_game_version -or
    $manifest.min_game_version.TrimStart('v') -ne $info.gameVersion.TrimStart('v')) {
    throw 'Manifest game/BaseLib requirements differ from build metadata.'
}

[pscustomobject]@{ PackageDirectory=$path; Info=$info; Hashes=$hashes }
