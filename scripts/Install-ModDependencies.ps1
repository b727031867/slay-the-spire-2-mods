$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$dependency = (Get-Content -Raw (Join-Path $projectRoot 'dependencies.json') | ConvertFrom-Json).baseLib
$cacheRoot = Join-Path $projectRoot '.local/dependencies'
New-Item -ItemType Directory -Force -Path $cacheRoot | Out-Null
$archive = Join-Path $cacheRoot "BaseLib.$($dependency.version).zip"
if (!(Test-Path -LiteralPath $archive)) {
    Invoke-WebRequest -Uri $dependency.url -OutFile $archive -TimeoutSec 120
}
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $dependency.sha256) {
    throw 'BaseLib download checksum mismatch. Do not install this archive.'
}
$destination = Join-Path $cacheRoot "BaseLib/$($dependency.version)"
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Expand-Archive -LiteralPath $archive -DestinationPath $destination -Force
foreach ($name in @('BaseLib.dll','BaseLib.json','BaseLib.pck')) {
    if (!(Test-Path -LiteralPath (Join-Path $destination $name))) { throw "Missing dependency file: $name" }
}
Write-Output "Verified BaseLib $($dependency.version) prepared in local cache. Deploy-Mod.ps1 installs it explicitly."
