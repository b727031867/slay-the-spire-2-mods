param([Parameter(Mandatory)][ValidateSet('PC-A','PC-B')][string]$DeviceId)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'Build-Mod.ps1') -DeviceId $DeviceId
if ($LASTEXITCODE -ne 0) { throw 'Candidate build failed.' }
$commit = (git -C $projectRoot rev-parse HEAD).Trim()
$manifest = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'src/Sts2Mod/Sts2Mod.json') | ConvertFrom-Json
$dependencies = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'dependencies.json') | ConvertFrom-Json
$baseName = "Sts2Mod-$($manifest.version)-$($dependencies.gameVersion)-$($commit.Substring(0,12))"
$source = Join-Path $projectRoot "artifacts/$baseName/Sts2Mod"
$destination = Join-Path $projectRoot "artifacts/$baseName-candidate"
if (Test-Path -LiteralPath $destination) { throw 'Candidate already exists; preserving it.' }
New-Item -ItemType Directory -Path $destination | Out-Null
Copy-Item -LiteralPath $source -Destination $destination -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs/player/README.zh-CN.md'),(Join-Path $projectRoot 'docs/player/CHANGELOG.md'),(Join-Path $projectRoot 'docs/ASSETS.md') -Destination $destination
$zip = "$destination.zip"
Compress-Archive -LiteralPath (Join-Path $destination 'Sts2Mod'),(Join-Path $destination 'README.zh-CN.md'),(Join-Path $destination 'CHANGELOG.md'),(Join-Path $destination 'ASSETS.md') -DestinationPath $zip
$hash = (Get-FileHash -LiteralPath $zip).Hash
"$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
Write-Output "Candidate=$zip"
Write-Output "CandidateSHA256=$hash"
