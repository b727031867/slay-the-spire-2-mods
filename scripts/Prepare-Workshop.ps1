param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$packagePath = (Resolve-Path -LiteralPath $PackageDirectory).Path
$info = Get-Content -Raw -LiteralPath (Join-Path $packagePath 'build-info.txt') | ConvertFrom-Json
if ($info.modId -ne 'Sts2Mod' -or $info.dirty -or $info.codeCommit -notmatch '^[a-f0-9]{40}$') { throw 'A clean, identified Sts2Mod package is required.' }
$names = @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt')
if (@(Get-ChildItem -LiteralPath $packagePath -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $names -or ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) }).Count) { throw 'Unexpected package content.' }
foreach ($name in $names[0..2]) {
    if ((Get-FileHash -LiteralPath (Join-Path $packagePath $name)).Hash -ne $info.files.$name) { throw "Checksum mismatch: $name" }
}
$workspace = Join-Path $projectRoot ('.local/workshop/' + $info.codeCommit)
if (Test-Path -LiteralPath $workspace) { throw 'Workspace already exists; preserve its mod_id.txt and review before refreshing it.' }
New-Item -ItemType Directory -Force -Path (Join-Path $workspace 'content'),(Join-Path $workspace 'previews') | Out-Null
foreach ($name in $names) { Copy-Item -LiteralPath (Join-Path $packagePath $name) -Destination (Join-Path $workspace 'content') }
Copy-Item -LiteralPath (Join-Path $projectRoot 'workshop/assets/image.png') -Destination $workspace
Copy-Item -LiteralPath (Join-Path $projectRoot 'workshop/assets/rules.png') -Destination (Join-Path $workspace 'previews')
foreach ($file in @((Join-Path $workspace 'image.png'),(Join-Path $workspace 'previews/rules.png'))) {
    if ((Get-Item -LiteralPath $file).Length -ge 1000000) { throw 'Workshop image must be less than 1MB.' }
}
$config = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/workshop.json') | ConvertFrom-Json
if ($config.visibility -ne 'private') { throw 'Acceptance preparation requires private visibility.' }
$config.description = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/description.bbcode.txt')
$config | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $workspace 'workshop.json') -Encoding utf8
$itemFile = Join-Path $projectRoot 'workshop/item-id.txt'
if (Test-Path -LiteralPath $itemFile) {
    $itemId = (Get-Content -Raw -LiteralPath $itemFile).Trim()
    if ($itemId -notmatch '^[0-9]+$') { throw 'Invalid workshop item identity.' }
    Set-Content -LiteralPath (Join-Path $workspace 'mod_id.txt') -Value $itemId -Encoding ascii
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs/player/README.zh-CN.md'),(Join-Path $projectRoot 'docs/player/CHANGELOG.md'),(Join-Path $projectRoot 'docs/ASSETS.md') -Destination (Join-Path $workspace 'content')
Write-Output "WorkshopDirectory=$workspace"
Write-Output 'Prepared private workspace only. No Steam upload or subscription has been performed.'
