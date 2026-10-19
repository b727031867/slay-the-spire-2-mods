$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$settingsFile = Join-Path $projectRoot 'local.settings.json'
if (!(Test-Path -LiteralPath $settingsFile)) {
    throw 'Copy local.settings.example.json to local.settings.json and set the local paths first.'
}
$settings = Get-Content -Raw -LiteralPath $settingsFile | ConvertFrom-Json
foreach ($file in @((Join-Path $settings.DotnetRoot 'dotnet.exe'), $settings.GodotExe, (Join-Path $settings.GameDir 'data_sts2_windows_x86_64/sts2.dll'))) {
    if (!(Test-Path -LiteralPath $file)) { throw "Missing dependency: $file" }
}
$env:DOTNET_ROOT = $settings.DotnetRoot
$env:DOTNET_ROOT_X64 = $settings.DotnetRoot
$toolBin = Join-Path (Split-Path $settings.DotnetRoot -Parent) 'bin'
$env:Path = "$($settings.DotnetRoot);$toolBin;$(Split-Path $settings.GodotExe -Parent);$env:Path"
$env:STS2_GAME_DIR = $settings.GameDir
$env:STS2_GODOT_EXE = $settings.GodotExe
