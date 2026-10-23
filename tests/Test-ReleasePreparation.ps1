$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$fixture=Join-Path $root ('.local/release-preparation-tests/'+[guid]::NewGuid().ToString('N'))
$scripts=Join-Path $fixture 'scripts'
$core=Join-Path $fixture 'core'
New-Item -ItemType Directory -Path $scripts,$core,(Join-Path $fixture 'docs/player') -Force | Out-Null
foreach($name in @('Get-ModPackage','Get-ReleaseSnapshot','Build-ReleaseCandidate','Prepare-Workshop','Invoke-ReleasePreparation')) {
    Copy-Item -LiteralPath (Join-Path $root "scripts/$name.ps1") -Destination $scripts
}
Copy-Item -LiteralPath (Join-Path $root 'workshop') -Destination $fixture -Recurse
foreach($name in @('README.zh-CN.md','CHANGELOG.md')) {
    Copy-Item -LiteralPath (Join-Path $root "docs/player/$name") -Destination (Join-Path $fixture 'docs/player')
}
Copy-Item -LiteralPath (Join-Path $root 'docs/ASSETS.md') -Destination (Join-Path $fixture 'docs')
# Only fixture copies replace deployment and native tests; production scripts are never invoked here.
@'
param($PackageDirectory)
Add-Content (Join-Path $PSScriptRoot '../stages.txt') 'deploy'
if((Get-Content (Join-Path $PSScriptRoot '../mode.txt')).Trim() -eq 'deploy-fail'){throw 'synthetic deployment failure'}
'@ | Set-Content (Join-Path $scripts 'Deploy-Mod.ps1')
@'
param($Suite,$CodeCommit)
Add-Content (Join-Path $PSScriptRoot '../stages.txt') 'Full'
$mode=(Get-Content (Join-Path $PSScriptRoot '../mode.txt')).Trim()
if($mode -eq 'throw'){throw 'synthetic native failure'}
if($mode -eq 'change-core'){Add-Content (Join-Path $PSScriptRoot '../core/build-info.txt') ' '}
[pscustomobject]@{Suite=$Suite;CodeCommit=$(if($mode -eq 'identity'){'b'*40}else{$CodeCommit});Status=$(if($mode -eq 'fail'){'FAIL'}else{'PASS'});Headless=($mode -ne 'gui');Rendering='NOT_RUN';Multiplayer='NOT_RUN'}
'@ | Set-Content (Join-Path $scripts 'Invoke-ModTests.ps1')
$commit='a'*40
Set-Content (Join-Path $core 'Sts2Mod.dll') 'fixture DLL'
Set-Content (Join-Path $core 'Sts2Mod.pck') 'fixture PCK'
@{id='Sts2Mod';version='0.3.14';min_game_version='0.107.1';dependencies=@(@{id='BaseLib';min_version='3.4.7'})} | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $core 'Sts2Mod.json')
$hashes=@{}
foreach($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json')){$hashes[$name]=(Get-FileHash (Join-Path $core $name)).Hash}
$info=@{schemaVersion=1;modId='Sts2Mod';modVersion='0.3.14';codeCommit=$commit;dirty=$false;gameVersion='v0.107.1';gameBuild='23811903';baseLibVersion='3.4.7';files=$hashes}
$buildInfo=Join-Path $core 'build-info.txt'
$info | ConvertTo-Json -Depth 4 | Set-Content $buildInfo
$count=0
function Check($condition,$message){if(!$condition){throw $message};$script:count++;Write-Output "PASS $message"}
function Reject([scriptblock]$action,$message){$failed=$false;try{& $action | Out-Null}catch{$failed=$true};Check $failed $message}
Set-Content (Join-Path $fixture 'mode.txt') 'pass'
$result=& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'candidate')
Check ($result.Status -eq 'PREPARED' -and !$result.Uploaded) 'pipeline prepares without claiming upload'
Check (((Get-Content (Join-Path $fixture 'stages.txt')) -join ',') -eq 'deploy,Full') 'deploy precedes headless Full'
Check ($result.Rendering -eq 'NOT_RUN' -and $result.Multiplayer -eq 'NOT_RUN' -and $result.DiskResume -eq 'NOT_RUN') 'unrun scopes remain explicit'
$saved=Get-Content -Raw ($result.SnapshotDirectory+'.preparation.json') | ConvertFrom-Json
Check ($saved.CoreFiles.'Sts2Mod.dll' -eq $hashes.'Sts2Mod.dll' -and $saved.Full.Status -eq 'PASS') 'summary binds Full result to core hashes'
$expanded=Join-Path $fixture 'expanded'
Expand-Archive -LiteralPath $result.Candidate -DestinationPath $expanded
$validated=& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded
Check ($validated.SnapshotSHA256 -eq $result.SnapshotSHA256) 'ZIP contains complete frozen snapshot'
Set-Content (Join-Path $fixture 'docs/player/README.zh-CN.md') 'changed after freeze'
$configPath=Join-Path $fixture 'workshop/workshop.json'
Set-Content $configPath '{"visibility":"private"}'
Set-Content (Join-Path $fixture 'workshop/assets/image.png') 'changed source art'
Set-Content (Join-Path $fixture 'workshop/item-id.txt') '999'
$prepared=& (Join-Path $scripts 'Prepare-Workshop.ps1') -SnapshotDirectory $expanded -OutputDirectory (Join-Path $fixture 'second-workspace')
foreach($name in @('README.zh-CN.md','CHANGELOG.md','ASSETS.md')) {
    Check ((Get-FileHash (Join-Path $prepared.WorkshopDirectory "content/$name")).Hash -eq $validated.Info.documents.$name) "frozen document reused: $name"
}
Check ((Get-Content -Raw (Join-Path $prepared.WorkshopDirectory 'workshop.json') | ConvertFrom-Json).visibility -eq 'friends_only') 'repository config changes cannot alter frozen visibility'
Check ((Get-Content -Raw (Join-Path $prepared.WorkshopDirectory 'mod_id.txt')).Trim() -eq '3823282901') 'frozen item identity retained'
Check ((Get-FileHash (Join-Path $prepared.WorkshopDirectory 'image.png')).Hash -eq $validated.Info.workshopFiles.'image.png') 'repository art changes cannot alter frozen images'
Check (@(Get-ChildItem (Join-Path $prepared.WorkshopDirectory 'content') -File).Count -eq 7) 'upload content remains exactly seven files'
Copy-Item -LiteralPath (Join-Path $root 'workshop/workshop.json') -Destination $configPath -Force
Copy-Item -LiteralPath (Join-Path $root 'workshop/item-id.txt') -Destination (Join-Path $fixture 'workshop/item-id.txt') -Force
Copy-Item -LiteralPath (Join-Path $root 'workshop/assets/image.png') -Destination (Join-Path $fixture 'workshop/assets/image.png') -Force
Reject {& (Join-Path $scripts 'Prepare-Workshop.ps1') -SnapshotDirectory $expanded -OutputDirectory $prepared.WorkshopDirectory} 'existing upload workspace preserved'
Reject {& (Join-Path $scripts 'Prepare-Workshop.ps1') -SnapshotDirectory $expanded -OutputDirectory (Join-Path $expanded 'nested')} 'output inside snapshot rejected'
foreach($relative in @('README.zh-CN.md','workshop/image.png','workshop/workshop.json','Sts2Mod/Sts2Mod.dll')) {
    $file=Join-Path $expanded $relative
    $original=[IO.File]::ReadAllBytes($file)
    try {
        Add-Content $file 'tampered'
        Reject {& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded} "tampered snapshot rejected: $relative"
    } finally {[IO.File]::WriteAllBytes($file,$original)}
}
$manifestPath=Join-Path $expanded 'release-info.txt'
$original=[IO.File]::ReadAllBytes($manifestPath)
try {
    $manifest=Get-Content -Raw $manifestPath | ConvertFrom-Json
    $manifest.schemaVersion=1
    $manifest | ConvertTo-Json -Depth 5 | Set-Content $manifestPath
    Reject {& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded} 'legacy unfrozen candidate rejected'
} finally {[IO.File]::WriteAllBytes($manifestPath,$original)}
foreach($policy in @('visibility','dependency','item','image-size')) {
    $relative=switch($policy){'item'{'mod_id.txt'} 'image-size'{'image.png'} default{'workshop.json'}}
    $file=Join-Path $expanded "workshop/$relative"
    $originalFile=[IO.File]::ReadAllBytes($file)
    try {
        if($policy -eq 'item'){Set-Content $file '999'}
        elseif($policy -eq 'image-size'){[IO.File]::WriteAllBytes($file,[byte[]]::new(1000000))}
        else {
            $config=Get-Content -Raw $file | ConvertFrom-Json
            if($policy -eq 'visibility'){$config.visibility='private'}else{$config.dependencies=@(999)}
            $config | ConvertTo-Json -Depth 5 | Set-Content $file
        }
        $manifest=Get-Content -Raw $manifestPath | ConvertFrom-Json
        $manifest.workshopFiles.$relative=(Get-FileHash $file).Hash
        $manifest | ConvertTo-Json -Depth 5 | Set-Content $manifestPath
        Reject {& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded} "invalid frozen policy rejected despite matching hash: $policy"
    } finally {
        [IO.File]::WriteAllBytes($file,$originalFile)
        [IO.File]::WriteAllBytes($manifestPath,$original)
    }
}
Set-Content (Join-Path $expanded 'extra.txt') 'unexpected'
Reject {& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded} 'unexpected snapshot files rejected'
foreach($mode in @('deploy-fail','fail','throw','identity','gui','change-core')) {
    Set-Content (Join-Path $fixture 'mode.txt') $mode
    $output=Join-Path $fixture "rejected-$mode"
    $fullBefore=@(Get-Content (Join-Path $fixture 'stages.txt') | Where-Object {$_ -eq 'Full'}).Count
    Reject {& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -OutputDirectory $output} "pipeline stops on $mode"
    Check (!(Test-Path $output)) "no snapshot produced after $mode"
    if($mode -eq 'deploy-fail') {
        Check (@(Get-Content (Join-Path $fixture 'stages.txt') | Where-Object {$_ -eq 'Full'}).Count -eq $fullBefore) 'deployment failure never starts Full'
    }
    $info | ConvertTo-Json -Depth 4 | Set-Content $buildInfo
}
Set-Content $configPath '{"visibility":"private"}'
Reject {& (Join-Path $scripts 'Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'private-candidate')} 'invalid repository Workshop visibility rejected at freeze'
Write-Output "RELEASE_PREPARATION_PASS assertions=$count fixture=$fixture"
