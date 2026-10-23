param([Parameter(Mandatory)][string]$PackageDirectory, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$package = & (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $PackageDirectory
$info = $package.Info
if (!$OutputDirectory) {
    $name = "Sts2Mod-$($info.modVersion)-$($info.gameVersion)-$($info.codeCommit.Substring(0,12))-candidate-" +
        [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ')
    $OutputDirectory = Join-Path $projectRoot "artifacts/$name"
}
$destination = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $destination) { throw 'Candidate already exists; preserving it.' }
if ((Test-Path -LiteralPath "$destination.zip") -or (Test-Path -LiteralPath "$destination.zip.sha256")) {
    throw 'Candidate archive already exists; preserving it.'
}
if ($destination.StartsWith($package.PackageDirectory + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Candidate output must be outside the source package.'
}
$documents = [ordered]@{
    'README.zh-CN.md'='docs/player/README.zh-CN.md'
    'CHANGELOG.md'='docs/player/CHANGELOG.md'
    'ASSETS.md'='docs/ASSETS.md'
}
foreach ($source in $documents.Values) {
    if (!(Test-Path -LiteralPath (Join-Path $projectRoot $source) -PathType Leaf)) { throw "Missing document: $source" }
}
$documentCommit = (git -C $projectRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Document source commit is required.' }
$documentDirty = [bool](git -C $projectRoot status --porcelain -- $documents.Values)
$workshopSources = @('workshop/workshop.json','workshop/description.bbcode.txt','workshop/item-id.txt','workshop/assets/image.png','workshop/assets/rules.png')
$workshopDirty = [bool](git -C $projectRoot status --porcelain -- $workshopSources)
$config = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/workshop.json') | ConvertFrom-Json
if ($config.visibility -ne 'friends_only') { throw 'Workshop visibility must be friends_only.' }
$config.description = Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/description.bbcode.txt')
$itemId = (Get-Content -Raw -LiteralPath (Join-Path $projectRoot 'workshop/item-id.txt')).Trim()
if ($itemId -ne '3823282901') { throw 'Release must update the existing Workshop item.' }
New-Item -ItemType Directory -Path $destination | Out-Null
$core = Join-Path $destination 'Sts2Mod'
Copy-Item -LiteralPath $package.PackageDirectory -Destination $core -Recurse
foreach ($name in $package.Hashes.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $core $name)).Hash -ne $package.Hashes[$name]) {
        throw "Copied core differs from input: $name"
    }
}
$documentHashes = [ordered]@{}
foreach ($name in $documents.Keys) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $documents[$name]) -Destination (Join-Path $destination $name)
    $documentHashes[$name] = (Get-FileHash -LiteralPath (Join-Path $destination $name)).Hash
}
$workshop = Join-Path $destination 'workshop'
New-Item -ItemType Directory -Path (Join-Path $workshop 'previews') -Force | Out-Null
$config | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $workshop 'workshop.json') -Encoding utf8
Set-Content -LiteralPath (Join-Path $workshop 'mod_id.txt') -Value $itemId -Encoding ascii
Copy-Item -LiteralPath (Join-Path $projectRoot 'workshop/assets/image.png') -Destination $workshop
Copy-Item -LiteralPath (Join-Path $projectRoot 'workshop/assets/rules.png') -Destination (Join-Path $workshop 'previews/rules.png')
$workshopHashes = [ordered]@{}
foreach ($name in @('workshop.json','mod_id.txt','image.png','previews/rules.png')) {
    $workshopHashes[$name] = (Get-FileHash -LiteralPath (Join-Path $workshop $name)).Hash
}
@{
    schemaVersion=2; codeCommit=$info.codeCommit; modVersion=$info.modVersion
    gameVersion=$info.gameVersion; gameBuild=$info.gameBuild; baseLibVersion=$info.baseLibVersion
    coreFiles=$package.Hashes; documentCommit=$documentCommit; documentDirty=$documentDirty; documents=$documentHashes
    workshopDirty=$workshopDirty; workshopFiles=$workshopHashes
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $destination 'release-info.txt') -Encoding utf8
& (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $destination | Out-Null
$zip = "$destination.zip"
Compress-Archive -LiteralPath $core,(Join-Path $destination 'README.zh-CN.md'),(Join-Path $destination 'CHANGELOG.md'),
    (Join-Path $destination 'ASSETS.md'),(Join-Path $destination 'release-info.txt'),$workshop -DestinationPath $zip
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
[pscustomobject]@{ Candidate=$zip; CandidateDirectory=$destination; CandidateSHA256=$hash; CodeCommit=$info.codeCommit }
