$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
Set-Location (Split-Path $PSScriptRoot -Parent)
Write-Host 'STS2 development environment is ready.'
Write-Host 'Check: .\scripts\Test-DevEnvironment.ps1'
Write-Host 'Tools: dotnet, ilspycmd; Godot executable: $env:STS2_GODOT_EXE'
