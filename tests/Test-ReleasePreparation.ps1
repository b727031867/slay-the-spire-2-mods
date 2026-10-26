$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$fixture=Join-Path $root ('.local/release-preparation-tests/'+[guid]::NewGuid().ToString('N'))
$scripts=Join-Path $fixture 'scripts'
$core=Join-Path $fixture 'core'
New-Item -ItemType Directory -Path $scripts,$core,(Join-Path $fixture 'docs/player') -Force | Out-Null
foreach($name in @('Get-ModPackage','Get-ReleaseSnapshot','Build-ReleaseCandidate','Prepare-Workshop','Invoke-ReleasePreparation','Remove-OldDeliverables')) {
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
Add-Content (Join-Path $PSScriptRoot '../stages.txt') $Suite
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
Check (((Get-Content (Join-Path $fixture 'stages.txt')) -join ',') -eq 'deploy,Load') 'default preparation only runs headless Load'
Check (!$result.ReusedTests -and $result.Timings.TotalSeconds -ge 0 -and $result.Timings.FreezeSeconds -ge 0) 'stage timings and fresh testing recorded'
Check ($result.Rendering -eq 'NOT_RUN' -and $result.Multiplayer -eq 'NOT_RUN' -and $result.DiskResume -eq 'NOT_RUN') 'unrun scopes remain explicit'
$saved=Get-Content -Raw ($result.SnapshotDirectory+'.preparation.json') | ConvertFrom-Json
Check ($saved.CoreFiles.'Sts2Mod.dll' -eq $hashes.'Sts2Mod.dll' -and $saved.Tests[0].Status -eq 'PASS' -and !$saved.Full) 'summary binds default Load result to core hashes'
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
Check ((Get-Content -Raw (Join-Path $prepared.WorkshopDirectory 'workshop.json') | ConvertFrom-Json).visibility -eq 'public') 'repository config changes cannot alter frozen public visibility'
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
$legacyConfig=Join-Path $expanded 'workshop/workshop.json'
$legacyConfigBytes=[IO.File]::ReadAllBytes($legacyConfig)
try {
    $config=Get-Content -Raw $legacyConfig | ConvertFrom-Json
    $config.visibility='friends_only'
    $config | ConvertTo-Json -Depth 5 | Set-Content $legacyConfig
    $manifest=Get-Content -Raw $manifestPath | ConvertFrom-Json
    $manifest.workshopFiles.'workshop.json'=(Get-FileHash $legacyConfig).Hash
    $manifest | ConvertTo-Json -Depth 5 | Set-Content $manifestPath
    $legacy=& (Join-Path $scripts 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $expanded
    Check ($legacy.Info.codeCommit -eq $commit) 'historical friends-only snapshot remains valid for inspection and retention'
    Reject {& (Join-Path $scripts 'Prepare-Workshop.ps1') -SnapshotDirectory $expanded -OutputDirectory (Join-Path $fixture 'legacy-upload')} 'historical friends-only snapshot cannot prepare a new upload'
    Check (!(Test-Path (Join-Path $fixture 'legacy-upload'))) 'legacy visibility rejected before copying upload files'
} finally {
    [IO.File]::WriteAllBytes($legacyConfig,$legacyConfigBytes)
    [IO.File]::WriteAllBytes($manifestPath,$original)
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
Set-Content $configPath '{"visibility":"friends_only"}'
Reject {& (Join-Path $scripts 'Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'friends-candidate')} 'new friends-only candidate rejected by public policy'
Set-Content $configPath '{"visibility":"private"}'
Reject {& (Join-Path $scripts 'Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory (Join-Path $fixture 'private-candidate')} 'invalid repository Workshop visibility rejected at freeze'
Copy-Item -LiteralPath (Join-Path $root 'workshop/workshop.json') -Destination $configPath -Force

# These fixtures exercise evidence validation without Steam or native game launches.
@'
[pscustomobject]@{MachineName='fixture';GameAssemblySha256='fixture-game';BaseLibDllSha256='fixture-base'}
'@ | Set-Content (Join-Path $scripts 'Get-ModTestEnvironment.ps1')
@'
param($Suite,$LogFile,$CodeCommit)
if((Get-Content -Raw $LogFile).Trim() -ne 'PASS fixture'){throw 'synthetic strict log rejection'}
[pscustomobject]@{Suite=$Suite;Status='PASS';CodeCommit=$CodeCommit}
'@ | Set-Content (Join-Path $scripts 'Test-ModRun.ps1')
$nativeLog=Join-Path $fixture 'native.log'
Set-Content $nativeLog 'PASS fixture'
$package=& (Join-Path $scripts 'Get-ModPackage.ps1') -PackageDirectory $core
$validEvidence=@{
    SchemaVersion=2;Suite='Full';Status='PASS';Headless=$true;CodeCommit=$commit
    CoreFiles=$package.Hashes;Environment=(& (Join-Path $scripts 'Get-ModTestEnvironment.ps1'))
    LogFile=$nativeLog;LogSHA256=(Get-FileHash $nativeLog).Hash
}
$evidenceFile=Join-Path $fixture 'evidence.json'
$validEvidence | ConvertTo-Json -Depth 8 | Set-Content $evidenceFile
$stagesBefore=Get-Content -Raw (Join-Path $fixture 'stages.txt')
$reused=& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -TestResultFile $evidenceFile -Suite Cards,Events -OutputDirectory (Join-Path $fixture 'reuse')
Check ($reused.ReusedTests -and $reused.Status -eq 'PREPARED' -and $reused.Tests.Count -eq 1) 'matching Full evidence reused for required focused coverage'
Check ((Get-Content -Raw (Join-Path $fixture 'stages.txt')) -eq $stagesBefore) 'reuse neither deploys nor starts native tests'
foreach($mode in @('legacy','core','metadata','environment','fail','gui','invalid-boolean','identity','coverage','loghash','missing-log','invalid-log')) {
    $evidence=$validEvidence | ConvertTo-Json -Depth 8 | ConvertFrom-Json
    switch($mode) {
        legacy {$evidence.SchemaVersion=1}
        core {$evidence.CoreFiles.'Sts2Mod.pck'='different'}
        metadata {$evidence.CoreFiles.'build-info.txt'='different'}
        environment {$evidence.Environment.GameAssemblySha256='different'}
        fail {$evidence.Status='FAIL'}
        gui {$evidence.Headless=$false}
        invalid-boolean {$evidence.Headless='true'}
        identity {$evidence.CodeCommit='b'*40}
        coverage {$evidence.Suite='Cards'}
        loghash {$evidence.LogSHA256='different'}
        missing-log {$evidence.LogFile=Join-Path $fixture 'missing.log'}
        invalid-log {
            Set-Content $nativeLog 'unknown gameplay error'
            $evidence.LogSHA256=(Get-FileHash $nativeLog).Hash
        }
    }
    $evidence | ConvertTo-Json -Depth 8 | Set-Content $evidenceFile
    $output=Join-Path $fixture "reuse-reject-$mode"
    Reject {& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -TestResultFile $evidenceFile -OutputDirectory $output} "invalid reusable evidence rejected: $mode"
    Check (!(Test-Path $output)) "no freeze after invalid reusable evidence: $mode"
    Set-Content $nativeLog 'PASS fixture'
}
Set-Content (Join-Path $fixture 'mode.txt') 'pass'
Reject {& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -Suite @()} 'empty required suite list rejected'
Set-Content (Join-Path $fixture 'docs/player/README.zh-CN.md') 'focused candidate documentation'
$focused=& (Join-Path $scripts 'Invoke-ReleasePreparation.ps1') -PackageDirectory $core -Suite Cards,Events -OutputDirectory (Join-Path $fixture 'focused')
Check (($focused.Tests.Suite -join ',') -eq 'Cards,Events' -and !$focused.Full) 'explicit focused suites do not silently run Full'

# Exercise the real evidence recorder with a disposable hidden shell, never the game.
foreach($name in @('Invoke-ModTests','ModTestSuites')) {
    Copy-Item -LiteralPath (Join-Path $root "scripts/$name.ps1") -Destination $scripts -Force
}
$info.gameAssemblySha256='fixture-game'
$info | ConvertTo-Json -Depth 4 | Set-Content $buildInfo
$installed=Join-Path $fixture 'game/mods/Sts2Mod'
New-Item -ItemType Directory -Path $installed -Force | Out-Null
@'
$env:STS2_GAME_DIR=Join-Path $PSScriptRoot '../game'
$changed=(Get-Content (Join-Path $PSScriptRoot '../mode.txt')).Trim() -eq 'environment-changed'
[pscustomobject]@{MachineName='fixture';GameVersion='v0.107.1';GameBuild='23811903';GameAssemblySha256=$(if($changed){'changed'}else{'fixture-game'});BaseLibVersion='3.4.7'}
'@ | Set-Content (Join-Path $scripts 'Get-ModTestEnvironment.ps1')
@'
param($Suite,$SandboxDirectory)
$log=Join-Path $PSScriptRoot ('../bound-'+[guid]::NewGuid().ToString('N')+'.log')
Set-Content $log '[Startup] Time to main menu: fixture'
Set-Content (Join-Path $PSScriptRoot '../last-log.txt') $log
$process=Start-Process -FilePath (Get-Process -Id $PID).Path -ArgumentList '-NoProfile','-Command','Start-Sleep -Seconds 30' -WindowStyle Hidden -PassThru
[pscustomobject]@{Process=$process;GameProcessId=$process.Id;LogFile=$log;SandboxDirectory='fixture'}
'@ | Set-Content (Join-Path $scripts 'Start-ModTest.ps1')
@'
param($Suite,$LogFile,$CodeCommit,$GameProcessId,$SandboxDirectory)
$mode=(Get-Content (Join-Path $PSScriptRoot '../mode.txt')).Trim()
switch($mode){
 'change-core' {Add-Content (Join-Path $PSScriptRoot '../core/build-info.txt') ' '}
 'change-installed' {Add-Content (Join-Path $PSScriptRoot '../game/mods/Sts2Mod/build-info.txt') ' '}
 'change-env' {Set-Content (Join-Path $PSScriptRoot '../mode.txt') 'environment-changed'}
 'fail-log' {throw 'synthetic log failure'}
}
[pscustomobject]@{SchemaVersion=1;Suite=$Suite;Status='PASS';CodeCommit=$CodeCommit;Headless=$true;LogFile=$LogFile}
'@ | Set-Content (Join-Path $scripts 'Test-ModRun.ps1')
foreach($mode in @('pass','change-core','change-installed','change-env','fail-log')) {
    $info | ConvertTo-Json -Depth 4 | Set-Content $buildInfo
    foreach($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json','build-info.txt')) {
        Copy-Item -LiteralPath (Join-Path $core $name) -Destination $installed -Force
    }
    Set-Content (Join-Path $fixture 'mode.txt') $mode
    if($mode -eq 'pass') {
        $bound=& (Join-Path $scripts 'Invoke-ModTests.ps1') -Suite Load -CodeCommit $commit -PackageDirectory $core
        Check ($bound.SchemaVersion -eq 2 -and $bound.CoreFiles.'Sts2Mod.pck' -eq $hashes.'Sts2Mod.pck' -and
            $bound.LogSHA256 -eq (Get-FileHash $bound.LogFile).Hash -and $bound.DurationSeconds -ge 0) 'test recorder binds core environment stable log and duration'
    } else {
        Reject {& (Join-Path $scripts 'Invoke-ModTests.ps1') -Suite Load -CodeCommit $commit -PackageDirectory $core} "test recorder rejects $mode"
        $lastLog=(Get-Content (Join-Path $fixture 'last-log.txt')).Trim()
        $failed=Get-Content -Raw ($lastLog+'.result.json') | ConvertFrom-Json
        Check ($failed.Status -eq 'FAIL') "recorder persists failure instead of PASS: $mode"
    }
}
Write-Output "RELEASE_PREPARATION_PASS assertions=$count fixture=$fixture"
