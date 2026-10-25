$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$fixture=Join-Path $root ('.local/retention-tests/'+[guid]::NewGuid().ToString('N'))
$scripts=Join-Path $fixture 'scripts'
$core=Join-Path $fixture 'core'
New-Item -ItemType Directory -Path $scripts,$core,(Join-Path $fixture 'docs/player') -Force | Out-Null
foreach ($name in @('Get-ModPackage','Get-ReleaseSnapshot','Build-ReleaseCandidate','Prepare-Workshop','Invoke-ReleasePreparation','Remove-OldDeliverables')) {
    Copy-Item -LiteralPath (Join-Path $root "scripts/$name.ps1") -Destination $scripts
}
Copy-Item -LiteralPath (Join-Path $root 'workshop') -Destination $fixture -Recurse
foreach ($name in @('README.zh-CN.md','CHANGELOG.md')) {
    Copy-Item -LiteralPath (Join-Path $root "docs/player/$name") -Destination (Join-Path $fixture 'docs/player')
}
Copy-Item -LiteralPath (Join-Path $root 'docs/ASSETS.md') -Destination (Join-Path $fixture 'docs')
@'
param($PackageDirectory)
if (Test-Path (Join-Path $PSScriptRoot '../fail.txt')) { throw 'synthetic deployment failure' }
'@ | Set-Content (Join-Path $scripts 'Deploy-Mod.ps1')
@'
param($Suite,$CodeCommit,$PackageDirectory)
[pscustomobject]@{Suite=$Suite;Status='PASS';CodeCommit=$CodeCommit;Headless=$true}
'@ | Set-Content (Join-Path $scripts 'Invoke-ModTests.ps1')
$commit='a'*40
Set-Content (Join-Path $core 'Sts2Mod.dll') 'fixture DLL'
Set-Content (Join-Path $core 'Sts2Mod.pck') 'fixture PCK'
@{id='Sts2Mod';version='0.3.17';min_game_version='0.107.1';dependencies=@(@{id='BaseLib';min_version='3.4.7'})} |
    ConvertTo-Json -Depth 4 | Set-Content (Join-Path $core 'Sts2Mod.json')
$hashes=@{}
foreach ($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json')) { $hashes[$name]=(Get-FileHash (Join-Path $core $name)).Hash }
@{schemaVersion=1;modId='Sts2Mod';modVersion='0.3.17';codeCommit=$commit;dirty=$false;
    gameVersion='v0.107.1';gameBuild='23811903';baseLibVersion='3.4.7';files=$hashes} |
    ConvertTo-Json -Depth 4 | Set-Content (Join-Path $core 'build-info.txt')
$count=0
function Check($condition,$message) { if (!$condition) { throw $message }; $script:count++; Write-Output "PASS $message" }
function Reject([scriptblock]$action,$message) {
    $failed=$false
    try { & $action | Out-Null } catch { $failed=$true }
    Check $failed $message
}
function Deliver([string]$Text) {
    Set-Content (Join-Path $fixture 'docs/player/README.zh-CN.md') $Text
    & (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core
}
$deliveries=@()
foreach ($number in 1..3) {
    $deliveries+=Deliver "delivery $number"
    Check ($deliveries[-1].Retention.Kept.Count -eq $number -and !$deliveries[-1].Retention.Removed.Count) "first $number deliveries preserved"
}
$first=$deliveries[0].SnapshotDirectory
$second=$deliveries[1].SnapshotDirectory
# Previously published friends-only snapshots remain eligible for safe rotation after the public policy change.
$friendsHistory=Join-Path $fixture 'artifacts/friends-history-release'
Copy-Item -LiteralPath $first -Destination $friendsHistory -Recurse
$friendsConfig=Join-Path $friendsHistory 'workshop/workshop.json'
$friendsManifest=Join-Path $friendsHistory 'release-info.txt'
$config=Get-Content -Raw $friendsConfig | ConvertFrom-Json
$config.visibility='friends_only'
$config | ConvertTo-Json -Depth 5 | Set-Content $friendsConfig
$manifest=Get-Content -Raw $friendsManifest | ConvertFrom-Json
$manifest.workshopFiles.'workshop.json'=(Get-FileHash $friendsConfig).Hash
$manifest | ConvertTo-Json -Depth 5 | Set-Content $friendsManifest
Compress-Archive -LiteralPath @(Get-ChildItem -LiteralPath $friendsHistory | ForEach-Object FullName) -DestinationPath "$friendsHistory.zip"
"$((Get-FileHash "$friendsHistory.zip").Hash)  friends-history-release.zip" | Set-Content "$friendsHistory.zip.sha256"
(Get-Item $friendsManifest).LastWriteTimeUtc=[datetime]'2000-01-01'
# A core shared by retained snapshots survives expiration of an older snapshot.
$oldCore=Join-Path $fixture 'artifacts/old-core'
New-Item -ItemType Directory -Path $oldCore | Out-Null
Copy-Item -LiteralPath $core -Destination (Join-Path $oldCore 'Sts2Mod') -Recurse
Compress-Archive -LiteralPath (Join-Path $oldCore 'Sts2Mod') -DestinationPath "$oldCore.zip"
"$((Get-FileHash "$oldCore.zip").Hash)  old-core.zip" | Set-Content "$oldCore.zip.sha256"
$legacy=Join-Path $fixture 'artifacts/legacy-core'
New-Item -ItemType Directory -Path $legacy | Out-Null
Copy-Item -LiteralPath $core -Destination (Join-Path $legacy 'Sts2Mod') -Recurse
Set-Content (Join-Path $legacy 'Sts2Mod/Sts2Mod.dll') 'different legacy DLL'
$legacyInfo=Get-Content -Raw (Join-Path $legacy 'Sts2Mod/build-info.txt') | ConvertFrom-Json
$legacyInfo.files.'Sts2Mod.dll'=(Get-FileHash (Join-Path $legacy 'Sts2Mod/Sts2Mod.dll')).Hash
$legacyInfo | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $legacy 'Sts2Mod/build-info.txt')
Compress-Archive -LiteralPath (Join-Path $legacy 'Sts2Mod') -DestinationPath "$legacy.zip"
"$((Get-FileHash "$legacy.zip").Hash)  legacy-core.zip" | Set-Content "$legacy.zip.sha256"
Set-Content "$legacy.timings.json" '{}'
(Get-Item "$legacy.zip").LastWriteTimeUtc=[datetime]'2000-01-01'
# This core is shared by every delivery, so its archive must survive expiration of the first snapshot.
$protected=Join-Path $fixture '.local/backups/keep.txt'
New-Item -ItemType Directory -Path (Split-Path $protected -Parent) -Force | Out-Null
Set-Content $protected 'backup evidence'
$unmanaged=Join-Path $fixture 'artifacts/unrelated.txt'
Set-Content $unmanaged 'unrelated'
$unfinished=& (Join-Path $scripts 'Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'artifacts/unfinished')
$fourth=Deliver 'delivery 4'
Check ($fourth.Retention.Status -eq 'COMPLETE' -and $fourth.Retention.Kept.Count -eq 3) 'successful delivery automatically retains three sets'
Check (!(Test-Path $friendsHistory) -and !(Test-Path "$friendsHistory.zip")) 'historical friends-only delivery can expire under the public policy'
foreach ($path in @($first,"$first.zip","$first.zip.sha256","$first.preparation.json",$deliveries[0].WorkshopDirectory)) {
    Check (!(Test-Path -LiteralPath $path)) "expired delivery component removed: $(Split-Path $path -Leaf)"
}
foreach ($delivery in @($deliveries[1],$deliveries[2],$fourth)) {
    Check ((Test-Path $delivery.SnapshotDirectory) -and (Test-Path $delivery.Candidate) -and (Test-Path $delivery.WorkshopDirectory)) 'retained snapshot archive and upload preserved'
}
Check ((Test-Path $protected) -and (Test-Path $unmanaged) -and (Test-Path $unfinished.CandidateDirectory) -and (Test-Path $core)) 'backups unrelated files unfinished candidates and input preserved'
Check (Test-Path $oldCore) 'core archive shared by retained deliveries is preserved'
Check (!(Test-Path $legacy) -and !(Test-Path "$legacy.zip") -and !(Test-Path "$legacy.zip.sha256") -and !(Test-Path "$legacy.timings.json")) 'standalone oldest core handoff and sidecars expire'
$saved=Get-Content -Raw ($fourth.SnapshotDirectory+'.preparation.json') | ConvertFrom-Json
Check ($saved.Retention.Status -eq 'COMPLETE' -and $saved.Retention.Kept.Count -eq 3) 'delivery report persists cleanup outcome'
Set-Content (Join-Path $fixture 'fail.txt') 'fail'
Reject { Deliver 'failed delivery' } 'failed preparation stops before retention'
Check (Test-Path $second) 'failed delivery never expires a previous successful set'
Remove-Item -LiteralPath (Join-Path $fixture 'fail.txt')

# Tampered old archives fail the full preflight before any deletion.
$secondHash=(Get-FileHash "$second.zip").Hash
Set-Content "$second.zip.sha256" (('0'*64)+'  wrong.zip')
Reject { Deliver 'delivery with corrupt history' } 'corrupt expired archive blocks retention'
Check ((Test-Path $second) -and (Test-Path "$second.zip")) 'preflight failure preserves old set'
"$secondHash  $([IO.Path]::GetFileName($second)).zip" | Set-Content "$second.zip.sha256"
$uploadDoc=Join-Path $deliveries[1].WorkshopDirectory 'content/README.zh-CN.md'
$originalDoc=[IO.File]::ReadAllBytes($uploadDoc)
Add-Content $uploadDoc 'unexpected edit'
Reject { Deliver 'delivery with modified upload history' } 'modified upload blocks the entire deletion plan'
Check ((Test-Path $second) -and (Test-Path "$second.zip")) 'upload preflight failure deletes no old snapshot or archive'
[IO.File]::WriteAllBytes($uploadDoc,$originalDoc)
$uploadExtra=Join-Path $deliveries[1].WorkshopDirectory 'unexpected.txt'
Set-Content $uploadExtra 'unrecognized user file'
Reject { Deliver 'delivery with extra upload file' } 'unknown upload files block deletion'
Check (Test-Path $uploadExtra) 'unknown upload file is preserved'
Remove-Item -LiteralPath $uploadExtra
$fifth=Deliver 'delivery 5'
Check ($fifth.Retention.Kept.Count -eq 3 -and !(Test-Path $second)) 'next successful delivery retries cleanup after preflight repair'

# A duplicate snapshot must not remove an upload directory still used by the retained copy.
$duplicate=Join-Path $fixture 'artifacts/duplicate-release'
Copy-Item -LiteralPath $fifth.SnapshotDirectory -Destination $duplicate -Recurse
Copy-Item -LiteralPath $fifth.Candidate -Destination "$duplicate.zip"
"$((Get-FileHash "$duplicate.zip").Hash)  duplicate-release.zip" | Set-Content "$duplicate.zip.sha256"
$again=& (Join-Path $scripts 'Remove-OldDeliverables.ps1') -PreparationFile ($fifth.SnapshotDirectory+'.preparation.json')
Check (!(Test-Path $duplicate) -and (Test-Path $fifth.WorkshopDirectory)) 'duplicate snapshot does not delete retained shared upload'

# Paths in reports cannot redirect cleanup outside the managed artifacts directory.
$custom=Deliver 'custom baseline'
Set-Content (Join-Path $fixture 'docs/player/README.zh-CN.md') 'different custom output'
$customResult=& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'custom-output')
Check ($customResult.Retention.Status -eq 'SKIPPED' -and (Test-Path $custom.SnapshotDirectory)) 'custom output cannot prune managed deliveries'

$bareCore=Join-Path $fixture 'artifacts/bare-build'
New-Item -ItemType Directory -Path $bareCore | Out-Null
Copy-Item -LiteralPath $core -Destination (Join-Path $bareCore 'Sts2Mod') -Recurse
foreach ($variant in @('B','C','D')) {
    Set-Content (Join-Path $core 'Sts2Mod.dll') "new DLL $variant"
    $buildInfo=Get-Content -Raw (Join-Path $core 'build-info.txt') | ConvertFrom-Json
    $buildInfo.files.'Sts2Mod.dll'=(Get-FileHash (Join-Path $core 'Sts2Mod.dll')).Hash
    $buildInfo | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $core 'build-info.txt')
    $latest=Deliver "new core $variant"
}
Check (!(Test-Path $oldCore) -and !(Test-Path "$oldCore.zip") -and !(Test-Path $bareCore)) 'archived and unarchived core copies expire when no retained snapshot uses them'
Check ((Test-Path $core) -and (Test-Path $protected)) 'current input and backups survive multiple core rotations'

$staleSnapshot=& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $unfinished.CandidateDirectory
$staleReport=$unfinished.CandidateDirectory+'.preparation.json'
@{Status='PREPARED';SnapshotDirectory=$unfinished.CandidateDirectory;SnapshotSHA256=$staleSnapshot.SnapshotSHA256;
    Candidate=$unfinished.Candidate;CandidateSHA256=$unfinished.CandidateSHA256} | ConvertTo-Json | Set-Content $staleReport
(Get-Item $staleReport).LastWriteTimeUtc=[datetime]'2000-01-01'
Reject { & (Join-Path $scripts 'Remove-OldDeliverables.ps1') -PreparationFile $staleReport } 'obsolete report cannot displace newer retained deliveries'
Check (Test-Path $latest.SnapshotDirectory) 'obsolete report leaves latest delivery intact'

$junction=Join-Path $fixture 'artifacts/redirected'
$junctionTarget=Split-Path $protected -Parent
New-Item -ItemType Junction -Path $junction -Target $junctionTarget | Out-Null
try {
    Reject { & (Join-Path $scripts 'Remove-OldDeliverables.ps1') -PreparationFile ($latest.SnapshotDirectory+'.preparation.json') } 'directory junction blocks cleanup before deletion'
    Check ((Test-Path $protected) -and (Test-Path $latest.SnapshotDirectory)) 'junction target and retained delivery preserved'
} finally { Remove-Item -LiteralPath $junction -Force }

$reportPath=$latest.SnapshotDirectory+'.preparation.json'
$reportBytes=[IO.File]::ReadAllBytes($reportPath)
try {
    $invalid=Get-Content -Raw $reportPath | ConvertFrom-Json
    $invalid.Status='FAIL'
    $invalid | ConvertTo-Json -Depth 10 | Set-Content $reportPath
    Reject { & (Join-Path $scripts 'Remove-OldDeliverables.ps1') -PreparationFile $reportPath } 'failed report cannot authorize cleanup'
    $invalid.Status='PREPARED'
    $invalid.SnapshotSHA256='0'*64
    $invalid | ConvertTo-Json -Depth 10 | Set-Content $reportPath
    Reject { & (Join-Path $scripts 'Remove-OldDeliverables.ps1') -PreparationFile $reportPath } 'mismatched report identity cannot authorize cleanup'
} finally { [IO.File]::WriteAllBytes($reportPath,$reportBytes) }
Write-Output "DELIVERABLE_RETENTION_PASS assertions=$count fixture=$fixture"
