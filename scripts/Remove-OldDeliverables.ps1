param([Parameter(Mandatory)][string]$PreparationFile)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$artifacts=[IO.Path]::GetFullPath((Join-Path $root 'artifacts'))
$reportPath=(Resolve-Path -LiteralPath $PreparationFile).Path
$report=Get-Content -Raw -LiteralPath $reportPath | ConvertFrom-Json
if ($report.Status -ne 'PREPARED') { throw 'Retention requires a successful delivery report.' }
$current=& (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $report.SnapshotDirectory
if ($reportPath -ne ($current.SnapshotDirectory+'.preparation.json') -or $report.SnapshotSHA256 -ne $current.SnapshotSHA256) {
    throw 'Delivery report does not identify its frozen snapshot.'
}
# shortcut: custom outputs are unmanaged; use artifacts for automatic retention instead of deleting arbitrary paths.
if ((Split-Path $current.SnapshotDirectory -Parent) -ne $artifacts) {
    Write-Warning 'Custom output is outside artifacts; automatic retention was skipped.'
    return [pscustomobject]@{Status='SKIPPED';Reason='UnmanagedOutput';Removed=@()}
}

function Assert-RegularTree([string]$Path) {
    $entry=Get-Item -LiteralPath $Path -Force
    $ancestor=$entry
    while ($null -ne $ancestor) {
        if ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Redirected retention path: $Path" }
        $ancestor=if ($ancestor -is [IO.DirectoryInfo]) { $ancestor.Parent } else { $ancestor.Directory }
    }
    if ($entry.PSIsContainer -and @(Get-ChildItem -LiteralPath $Path -Recurse -Force | Where-Object {
        $_.Attributes -band [IO.FileAttributes]::ReparsePoint
    }).Count) { throw "Redirected retention contents: $Path" }
}
Assert-RegularTree $artifacts
$workshopRoot=[IO.Path]::GetFullPath((Join-Path $root '.local/workshop'))
if (Test-Path -LiteralPath $workshopRoot) { Assert-RegularTree $workshopRoot }
$snapshots=@()
$cores=@()
foreach ($directory in Get-ChildItem -LiteralPath $artifacts -Directory) {
    $path=$directory.FullName
    $archived=Test-Path -LiteralPath "$path.zip"
    $preparation="$path.preparation.json"
    $isSnapshot=Test-Path -LiteralPath (Join-Path $path 'release-info.txt')
    $isCore=Test-Path -LiteralPath (Join-Path $path 'Sts2Mod/build-info.txt')
    if (!$isSnapshot -and !$isCore) { continue }
    if ($isSnapshot) {
        # A bare freeze is not a delivery; legacy named release snapshots predate preparation reports.
        if (Test-Path -LiteralPath $preparation) {
            $evidence=Get-Content -Raw -LiteralPath $preparation | ConvertFrom-Json
            if ($evidence.Status -ne 'PREPARED') { continue }
            $deliveredAt=(Get-Item -LiteralPath $preparation).LastWriteTimeUtc
        } elseif ($directory.Name -like '*-release') {
            $evidence=$null
            $deliveredAt=(Get-Item -LiteralPath (Join-Path $path 'release-info.txt')).LastWriteTimeUtc
        } else { continue }
        $snapshot=& (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $path
        if ($evidence -and ($evidence.SnapshotDirectory -ne $path -or $evidence.SnapshotSHA256 -ne $snapshot.SnapshotSHA256)) {
            throw "Invalid delivery identity: $preparation"
        }
        $package=$snapshot.Package
    } else {
        $snapshot=$null
        if ($archived) {
            $package=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory (Join-Path $path 'Sts2Mod')
        } else {
            # Unidentified/dirty in-progress builds are not owned deliverables and must survive cleanup.
            try { $package=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory (Join-Path $path 'Sts2Mod') }
            catch { continue }
            if (@(Get-ChildItem -LiteralPath $path -Force).Count -ne 1) { continue }
        }
        if (@(Get-ChildItem -LiteralPath $path -Force).Count -ne 1) { throw "Unexpected core handoff contents: $path" }
        $deliveredAt=if ($archived) { (Get-Item -LiteralPath "$path.zip").LastWriteTimeUtc } else { $directory.LastWriteTimeUtc }
    }
    if (($isSnapshot -or $archived) -and (!$archived -or !(Test-Path -LiteralPath "$path.zip.sha256"))) {
        throw "Missing delivery archive/checksum: $path"
    }
    if ($archived) {
        $archiveHash=(Get-FileHash -LiteralPath "$path.zip").Hash
        $checksum=(Get-Content -Raw -LiteralPath "$path.zip.sha256").Trim()
        if ($checksum -notmatch '^([a-fA-F0-9]{64})\s+' -or $Matches[1] -ne $archiveHash) {
            throw "Delivery archive checksum mismatch: $path"
        }
    }
    if ($isSnapshot -and $evidence -and ($evidence.Candidate -ne "$path.zip" -or $evidence.CandidateSHA256 -ne $archiveHash)) {
        throw "Delivery archive differs from preparation report: $path"
    }
    $coreKey=$package.Hashes.Values -join ':'
    $identity=if ($snapshot) { $snapshot.SnapshotSHA256 } else { $coreKey }
    $entry=[pscustomobject]@{Path=$path;Snapshot=$snapshot;CoreKey=$coreKey;Identity=$identity;DeliveredAt=$deliveredAt;Archived=$archived}
    if ($isSnapshot) { $snapshots+=$entry } else { $cores+=$entry }
}
# Core archives sharing a frozen core are copies of that delivery, not additional retention slots.
$deliveries=@($snapshots)+@($cores | Where-Object { $_.Archived -and $_.CoreKey -notin $snapshots.CoreKey })
$ordered=@($deliveries | Sort-Object @{Expression='DeliveredAt';Descending=$true},
    @{Expression={$_.Path -eq $current.SnapshotDirectory};Descending=$true},Path)
$keep=@()
foreach ($delivery in $ordered) {
    if ($keep.Count -lt 3 -and $delivery.Identity -notin $keep.Identity) { $keep+=$delivery }
}
if ($current.SnapshotDirectory -notin $keep.Path) { throw 'Cannot rotate using an obsolete delivery report; use the latest delivery.' }
$expired=@($ordered | Where-Object { $_.Path -notin $keep.Path })
$expired+=@($cores | Where-Object { $_.CoreKey -in $snapshots.CoreKey -and $_.CoreKey -notin $keep.CoreKey })
$targets=@()
foreach ($delivery in $expired) {
    $path=$delivery.Path
    if ((Split-Path $path -Parent) -ne $artifacts -or $path -eq $current.SnapshotDirectory) {
        throw "Invalid expired delivery path: $path"
    }
    $targets+=$path
    foreach ($suffix in @('.zip','.zip.sha256','.preparation.json','.timings.json')) {
        if (Test-Path -LiteralPath ($path+$suffix)) { $targets+=($path+$suffix) }
    }
    if ($delivery.Snapshot) {
        $snapshot=$delivery.Snapshot
        $uploadNames=@((Split-Path $path -Leaf))
        if ($snapshot.SnapshotSHA256 -notin @($keep | Where-Object Snapshot | ForEach-Object { $_.Snapshot.SnapshotSHA256 })) {
            $uploadNames+=($snapshot.Info.codeCommit+'-'+$snapshot.SnapshotSHA256.Substring(0,12))
        }
        foreach ($uploadName in $uploadNames) {
            $upload=Join-Path $workshopRoot $uploadName
            if (!(Test-Path -LiteralPath $upload)) { continue }
            Assert-RegularTree $upload
            $groups=@(@{Path=(Join-Path $upload 'content');Hashes=$snapshot.Info.coreFiles},
                @{Path=(Join-Path $upload 'content');Hashes=$snapshot.Info.documents},
                @{Path=$upload;Hashes=$snapshot.Info.workshopFiles})
            $expectedFiles=0
            foreach ($group in $groups) {
                foreach ($file in $group.Hashes.PSObject.Properties) {
                    $expectedFiles++
                    if ((Get-FileHash -LiteralPath (Join-Path $group.Path $file.Name)).Hash -ne $file.Value) {
                        throw "Upload copy differs from expired snapshot: $upload"
                    }
                }
            }
            if (@(Get-ChildItem -LiteralPath $upload -Recurse -Force -File).Count -ne $expectedFiles) {
                throw "Unexpected upload contents: $upload"
            }
            $targets+=$upload
        }
    }
}
$targets=@($targets | Select-Object -Unique)
foreach ($path in $targets) { Assert-RegularTree $path }
# Validate the complete removal plan before deleting any directory or sidecar.
foreach ($path in $targets) { Remove-Item -LiteralPath $path -Recurse -Force }
[pscustomobject]@{Status='COMPLETE';Kept=@($keep.Path);Removed=@($targets)}
