param([Parameter(Mandatory)][string]$SnapshotDirectory, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$snapshot = & (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $SnapshotDirectory
if (!$OutputDirectory) {
    $OutputDirectory = Join-Path $projectRoot ('.local/workshop/' + $snapshot.Info.codeCommit + '-' + $snapshot.SnapshotSHA256.Substring(0,12))
}
$workspace = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
if (Test-Path -LiteralPath $workspace) { throw 'Workspace already exists; preserve its mod_id.txt and review before refreshing it.' }
if ($workspace.StartsWith($snapshot.SnapshotDirectory + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Workshop output must be outside the frozen snapshot.'
}
Copy-Item -LiteralPath (Join-Path $snapshot.SnapshotDirectory 'workshop') -Destination $workspace -Recurse
$content = Join-Path $workspace 'content'
Copy-Item -LiteralPath $snapshot.Package.PackageDirectory -Destination $content -Recurse
foreach ($name in @('README.zh-CN.md','CHANGELOG.md','ASSETS.md')) {
    Copy-Item -LiteralPath (Join-Path $snapshot.SnapshotDirectory $name) -Destination $content
}
foreach ($group in @(@{Path=$content;Hashes=$snapshot.Info.coreFiles},@{Path=$content;Hashes=$snapshot.Info.documents},@{Path=$workspace;Hashes=$snapshot.Info.workshopFiles})) {
    foreach ($entry in $group.Hashes.PSObject.Properties) {
        if ((Get-FileHash -LiteralPath (Join-Path $group.Path $entry.Name)).Hash -ne $entry.Value) { throw "Copied snapshot differs: $($entry.Name)" }
    }
}
[pscustomobject]@{ WorkshopDirectory=$workspace; CodeCommit=$snapshot.Info.codeCommit; SnapshotSHA256=$snapshot.SnapshotSHA256 }
