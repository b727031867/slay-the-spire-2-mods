param(
    [Parameter(Mandatory)][ValidateSet('Load','Visuals','Cards','Character','Full','SavePrepare','SaveResume')][string]$Suite,
    [Parameter(Mandatory)][string]$CodeCommit,
    [string]$SandboxDirectory,
    [ValidateRange(1,3600)][int]$TimeoutSeconds=300
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ModTestSuites.ps1')
if (!$PSBoundParameters.ContainsKey('TimeoutSeconds') -and $Suite -in @('Full','SaveResume')) { $TimeoutSeconds=1800 }
$definition=Get-ModTestSuite $Suite
$launch=& (Join-Path $PSScriptRoot 'Start-ModTest.ps1') -Suite $Suite -SandboxDirectory $SandboxDirectory
# Retain this process object, so a reused PID can never cause another process to be stopped.
$ownedProcess=$launch.Process
try {
    $clock=[Diagnostics.Stopwatch]::StartNew()
    do {
        $text=if(Test-Path -LiteralPath $launch.LogFile){Get-Content -Raw -LiteralPath $launch.LogFile}else{''}
        if($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR|(?m)^ERROR: System\.'){throw 'Native test reported a failure; inspect the test log.'}
        if($text -match $definition.Marker){break}
        if(!$ownedProcess -or $ownedProcess.HasExited){throw 'Test process exited before its suite completed.'}
        if($clock.Elapsed.TotalSeconds -ge $TimeoutSeconds){throw "Test suite timed out: $Suite"}
        Start-Sleep -Milliseconds 500
    } while($true)
    if($Suite -in @('SavePrepare','SaveResume') -and $ownedProcess -and !$ownedProcess.HasExited){
        if(!$ownedProcess.WaitForExit(15000)){throw 'Native save test did not exit after shutdown.'}
    }
    $result=& (Join-Path $PSScriptRoot 'Test-ModRun.ps1') -Suite $Suite -LogFile $launch.LogFile -CodeCommit $CodeCommit -ResultFile ($launch.LogFile+'.result.json') -GameProcessId $launch.GameProcessId -SandboxDirectory $launch.SandboxDirectory
    $result
} catch {
    # Persist launcher failures (timeout/early exit) as failures too, without relaxing validation.
    if (!(Test-Path -LiteralPath ($launch.LogFile+'.result.json'))) {
        [pscustomobject]@{
            SchemaVersion=1; Suite=$Suite; Status='FAIL'; CodeCommit=$CodeCommit
            LogFile=$launch.LogFile; GameProcessId=$launch.GameProcessId; SandboxDirectory=$launch.SandboxDirectory
            Headless=$true; Rendering='NOT_RUN'; Multiplayer='NOT_RUN'; Failure=$_.Exception.Message
        } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath ($launch.LogFile+'.result.json') -Encoding utf8
    }
    throw
} finally {
    if($ownedProcess -and !$ownedProcess.HasExited){$ownedProcess.Kill(); $ownedProcess.WaitForExit()}
    Write-Host "Test log: $($launch.LogFile)"
    if($launch.SandboxDirectory){Write-Host "Test sandbox: $($launch.SandboxDirectory)"}
}
