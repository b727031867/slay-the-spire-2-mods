$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
$projectRoot = Split-Path $PSScriptRoot -Parent
$checkProject = Join-Path $projectRoot 'tools/EnvironmentCheck/EnvironmentCheck.csproj'
$outDir = Join-Path $projectRoot 'artifacts/environment-check'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
& (Join-Path $env:DOTNET_ROOT 'dotnet.exe') --version
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed.' }
& $env:STS2_GODOT_EXE --headless --version
if ($LASTEXITCODE -ne 0) { throw 'Godot check failed.' }
& (Join-Path $env:DOTNET_ROOT 'dotnet.exe') build $checkProject --configuration Release --nologo
if ($LASTEXITCODE -ne 0) { throw 'Compilation against game assemblies failed.' }
$godotProject = Join-Path $projectRoot 'tools/EnvironmentCheck'
& $env:STS2_GODOT_EXE --headless --path $godotProject --editor --import
if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }
& $env:STS2_GODOT_EXE --headless --path $godotProject --export-pack BasicExport (Join-Path $outDir 'EnvironmentCheck.pck')
if ($LASTEXITCODE -ne 0) { throw 'Godot resource export failed.' }
$pack = Get-Item -LiteralPath (Join-Path $outDir 'EnvironmentCheck.pck')
if ($pack.Length -eq 0) { throw 'Godot exported an empty file.' }
Get-FileHash -Algorithm SHA256 -LiteralPath $pack.FullName
Write-Output 'PASS: SDK, game assembly compilation, Godot import and PCK export. Game loading and multiplayer were not tested.'
