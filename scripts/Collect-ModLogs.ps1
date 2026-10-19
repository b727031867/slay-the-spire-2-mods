param([Parameter(Mandatory)][string]$LogFile)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$logPath = (Resolve-Path -LiteralPath $LogFile).Path
$outputDir = Join-Path $projectRoot 'artifacts/logs'
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
# Export a narrow diagnostic excerpt; raw logs can include account IDs, environment and save paths.
$lines = @(Get-Content -LiteralPath $logPath | Where-Object { $_ -match 'STS2MOD_READY|Finished mod initialization|RUNNING MODDED|Exception thrown while loading mod|Exception thrown when calling mod initializer|Skipping loading mod|Time to main menu|Godot errored while loading PCK' })
$redacted = foreach ($line in $lines) {
    $line.Replace($env:USERPROFILE, '<USERPROFILE>') -replace '\b7656119\d{10}\b','<STEAM_ID>'
}
$destination = Join-Path $outputDir ([IO.Path]::GetFileNameWithoutExtension($logPath) + '-excerpt.txt')
$redacted | Set-Content -LiteralPath $destination -Encoding utf8
Write-Output "Excerpt=$destination"
Write-Output 'Review the excerpt before sharing. Raw logs remain local.'
