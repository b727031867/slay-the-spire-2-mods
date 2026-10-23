$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
$steamApps=Split-Path (Split-Path $env:STS2_GAME_DIR -Parent) -Parent
$basePaths=@((Join-Path $env:STS2_GAME_DIR 'mods/BaseLib'), (Join-Path $steamApps 'workshop/content/2868840/3737335127/BaseLib'))
$basePaths=@($basePaths | Where-Object { Test-Path -LiteralPath (Join-Path $_ 'BaseLib.json') })
if ($basePaths.Count -ne 1) { throw 'Reusable tests require exactly one installed BaseLib copy; remove duplicate local/Workshop overrides first.' }
$base=$basePaths[0]
$game=Get-Content -Raw (Join-Path $env:STS2_GAME_DIR 'release_info.json') | ConvertFrom-Json
$manifest=Get-Content -Raw (Join-Path $steamApps 'appmanifest_2868840.acf')
$build=[regex]::Match($manifest,'"buildid"\s+"(\d+)"').Groups[1].Value
if (!$build) { throw 'Cannot identify installed game build.' }
$baseInfo=Get-Content -Raw (Join-Path $base 'BaseLib.json') | ConvertFrom-Json
[pscustomobject][ordered]@{
    MachineName=[Environment]::MachineName
    GameVersion=$game.version; GameBuild=$build
    GameAssemblySha256=(Get-FileHash -LiteralPath (Join-Path $env:STS2_GAME_DIR 'data_sts2_windows_x86_64/sts2.dll')).Hash
    BaseLibVersion=$baseInfo.version.TrimStart('v')
    BaseLibDllSha256=(Get-FileHash -LiteralPath (Join-Path $base 'BaseLib.dll')).Hash
    BaseLibPckSha256=(Get-FileHash -LiteralPath (Join-Path $base 'BaseLib.pck')).Hash
    BaseLibManifestSha256=(Get-FileHash -LiteralPath (Join-Path $base 'BaseLib.json')).Hash
}
