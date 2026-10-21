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
@{
    schemaVersion=1; codeCommit=$info.codeCommit; modVersion=$info.modVersion
    gameVersion=$info.gameVersion; gameBuild=$info.gameBuild; baseLibVersion=$info.baseLibVersion
    coreFiles=$package.Hashes; documentCommit=$documentCommit; documentDirty=$documentDirty; documents=$documentHashes
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $destination 'release-info.txt') -Encoding utf8
$zip = "$destination.zip"
Compress-Archive -LiteralPath $core,(Join-Path $destination 'README.zh-CN.md'),(Join-Path $destination 'CHANGELOG.md'),
    (Join-Path $destination 'ASSETS.md'),(Join-Path $destination 'release-info.txt') -DestinationPath $zip
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($zip))" | Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
[pscustomobject]@{ Candidate=$zip; CandidateDirectory=$destination; CandidateSHA256=$hash; CodeCommit=$info.codeCommit }
