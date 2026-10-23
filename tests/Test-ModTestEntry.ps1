$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
. (Join-Path $root 'scripts/ModTestSuites.ps1')
$fixture=Join-Path $root ('.local/test-entry-tests/'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$log=Join-Path $fixture 'synthetic.log'
$identity='a'*40
$valid=@"
[Startup] Time to main menu: 1
STS2MOD_READY version=0.3.5+$identity baselib=3.4.7 resource=ok
Finished mod initialization for 'BaseLib' (BaseLib)
Finished mod initialization for 'Traveler Elements' (Sts2Mod)
"@
Set-Content $log $valid
$result=& (Join-Path $root 'scripts/Test-ModRun.ps1') -Suite Load -LogFile $log -CodeCommit $identity
if($result.Status -ne 'PASS' -or $result.Rendering -ne 'NOT_RUN'){throw 'Normalized load result invalid'}
$withProcess=& (Join-Path $root 'scripts/Test-ModRun.ps1') -Suite Load -LogFile $log -CodeCommit $identity -GameProcessId 123 -SandboxDirectory 'fixture-only' -ResultFile (Join-Path $fixture 'process-result.json')
$saved=Get-Content -Raw (Join-Path $fixture 'process-result.json') | ConvertFrom-Json
if($saved.GameProcessId -ne 123 -or $saved.SandboxDirectory -ne 'fixture-only'){throw 'Process identity lost in structured result'}
$count=2
foreach($suite in @('Visuals','Cards','Character','Events','Full','SavePrepare','SaveResume')){
    $failed=$false
    try {& (Join-Path $root 'scripts/Test-ModRun.ps1') -Suite $suite -LogFile $log -CodeCommit $identity | Out-Null} catch {$failed=$true}
    if(!$failed){throw "Incomplete suite incorrectly passed: $suite"}
    $count++
}
Add-Content $log "ERROR: synthetic unknown gameplay failure"
$failed=$false
try {& (Join-Path $root 'scripts/Test-ModRun.ps1') -Suite Load -LogFile $log -CodeCommit $identity -ResultFile (Join-Path $fixture 'failed.json') | Out-Null}catch{$failed=$true}
$failure=Get-Content -Raw (Join-Path $fixture 'failed.json') | ConvertFrom-Json
if(!$failed -or $failure.Status -ne 'FAIL'){throw 'Unknown errors must fail and serialize failure'}
$count++
$failed=$false
try {& (Join-Path $root 'scripts/Start-ModTest.ps1') -Suite Visuals -SandboxDirectory 'invalid' | Out-Null}catch{$failed=$true}
if(!$failed){throw 'Unrelated suite accepted a save sandbox'}
$count++
$failed=$false
try {& (Join-Path $root 'scripts/Start-ModTest.ps1') -Suite SaveResume | Out-Null}catch{$failed=$true}
if(!$failed){throw 'Resume without sandbox accepted'}
$count++
foreach($suite in @('Load','Visuals','Cards','Character','Events','Full','SavePrepare','SaveResume')){
    if(!(Get-ModTestSuite $suite).Marker){throw "Missing completion marker: $suite"}
}
Write-Output "MOD_TEST_ENTRY_PASS assertions=$count fixture=$fixture (synthetic logs only; no game launched)"
