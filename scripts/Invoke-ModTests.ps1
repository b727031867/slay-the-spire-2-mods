param(
    [Parameter(Mandatory)][ValidateSet('Load','Visuals','Cards','NewCards','Character','Events','Full','SavePrepare','SaveResume')][string]$Suite,
    [Parameter(Mandatory)][string]$CodeCommit,
    [string]$SandboxDirectory,
    [string]$PackageDirectory,
    [ValidateRange(1,3600)][int]$TimeoutSeconds=300
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ModTestSuites.ps1')
if (!$PSBoundParameters.ContainsKey('TimeoutSeconds') -and $Suite -in @('Cards','Full','SaveResume')) { $TimeoutSeconds=1800 }
$definition=Get-ModTestSuite $Suite
$total=[Diagnostics.Stopwatch]::StartNew()
$package=$null
if ($PackageDirectory) {
    $package=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $PackageDirectory
    $environment=& (Join-Path $PSScriptRoot 'Get-ModTestEnvironment.ps1')
    $installedPath=Join-Path $env:STS2_GAME_DIR 'mods/Sts2Mod'
    $installed=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $installedPath
    if ($package.Info.codeCommit -ne $CodeCommit -or $environment.GameVersion -ne $package.Info.gameVersion -or
        $environment.GameBuild -ne $package.Info.gameBuild -or $environment.GameAssemblySha256 -ne $package.Info.gameAssemblySha256 -or
        $environment.BaseLibVersion -ne $package.Info.baseLibVersion) { throw 'Test package or environment identity mismatch.' }
    foreach ($name in $package.Hashes.Keys) {
        if ($installed.Hashes[$name] -ne $package.Hashes[$name]) { throw "Deployed test package differs: $name" }
    }
}
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
    # Freeze the owned process's log before validating and hashing reusable evidence.
    if ($ownedProcess -and !$ownedProcess.HasExited) { $ownedProcess.Kill(); $ownedProcess.WaitForExit() }
    $result=& (Join-Path $PSScriptRoot 'Test-ModRun.ps1') -Suite $Suite -LogFile $launch.LogFile -CodeCommit $CodeCommit -GameProcessId $launch.GameProcessId -SandboxDirectory $launch.SandboxDirectory
    if ($package) {
        $current=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $PackageDirectory
        $installed=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $installedPath
        foreach ($name in $package.Hashes.Keys) {
            if ($current.Hashes[$name] -ne $package.Hashes[$name] -or $installed.Hashes[$name] -ne $package.Hashes[$name]) {
                throw "Test package changed during execution: $name"
            }
        }
        $currentEnvironment=& (Join-Path $PSScriptRoot 'Get-ModTestEnvironment.ps1')
        foreach ($property in $environment.PSObject.Properties) {
            if ($currentEnvironment.($property.Name) -ne $property.Value) { throw "Test environment changed: $($property.Name)" }
        }
        $result.SchemaVersion=2
        $result | Add-Member -NotePropertyName CoreFiles -NotePropertyValue $package.Hashes
        $result | Add-Member -NotePropertyName Environment -NotePropertyValue $environment
        $result | Add-Member -NotePropertyName LogSHA256 -NotePropertyValue (Get-FileHash -LiteralPath $launch.LogFile).Hash
    }
    $result | Add-Member -NotePropertyName DurationSeconds -NotePropertyValue ([math]::Round($total.Elapsed.TotalSeconds,3))
    $result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath ($launch.LogFile+'.result.json') -Encoding utf8
    $result
} catch {
    # Persist launcher failures (timeout/early exit) as failures too, without relaxing validation.
    # A failed post-test hash check must never leave a passing report behind.
    if ($launch.LogFile) {
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
