param(
    [Parameter(Mandatory)][string]$PackageDirectory,
    [string]$OutputDirectory,
    [ValidateNotNullOrEmpty()][ValidateSet('Load','Visuals','Cards','Character','Events','Full')][string[]]$Suite=@('Load'),
    [ValidateNotNullOrEmpty()][string[]]$TestResultFile
)
$ErrorActionPreference='Stop'
$total=[Diagnostics.Stopwatch]::StartNew()
$stage=[Diagnostics.Stopwatch]::StartNew()
$timings=[ordered]@{}
$package=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $PackageDirectory
$Suite=@($Suite | Select-Object -Unique)
if ($Suite -contains 'Full') { $Suite=@('Full') }
$timings.ValidateSeconds=$stage.Elapsed.TotalSeconds; $stage.Restart()
$tests=@()
if ($TestResultFile) {
    $environment=& (Join-Path $PSScriptRoot 'Get-ModTestEnvironment.ps1')
    foreach ($file in $TestResultFile) {
        $test=Get-Content -Raw -LiteralPath $file | ConvertFrom-Json
        if ($test.SchemaVersion -ne 2 -or $test.Status -ne 'PASS' -or $test.Headless -isnot [bool] -or $test.Headless -ne $true -or
            $test.CodeCommit -ne $package.Info.codeCommit) { throw "Reusable test identity missing or invalid: $file" }
        foreach ($name in $package.Hashes.Keys) {
            if ($test.CoreFiles.$name -ne $package.Hashes[$name]) { throw "Reusable test core mismatch: $name" }
        }
        foreach ($property in $environment.PSObject.Properties) {
            if ($test.Environment.($property.Name) -ne $property.Value) { throw "Reusable test environment mismatch: $($property.Name)" }
        }
        if (!(Test-Path -LiteralPath $test.LogFile -PathType Leaf) -or
            (Get-FileHash -LiteralPath $test.LogFile).Hash -ne $test.LogSHA256) { throw "Reusable test log changed or missing: $file" }
        $validated=& (Join-Path $PSScriptRoot 'Test-ModRun.ps1') -Suite $test.Suite -LogFile $test.LogFile -CodeCommit $package.Info.codeCommit
        if ($validated.Status -ne 'PASS') { throw "Reusable test log failed validation: $file" }
        $tests+=$test
    }
    foreach ($required in $Suite) {
        if (!@($tests | Where-Object { $_.Suite -eq $required -or $_.Suite -eq 'Full' }).Count) {
            throw "Reusable results do not cover required suite: $required"
        }
    }
} else {
    & (Join-Path $PSScriptRoot 'Deploy-Mod.ps1') -PackageDirectory $package.PackageDirectory | Out-Host
    foreach ($required in $Suite) {
        $test=& (Join-Path $PSScriptRoot 'Invoke-ModTests.ps1') -Suite $required -CodeCommit $package.Info.codeCommit -PackageDirectory $package.PackageDirectory
        if ($test.Status -ne 'PASS' -or $test.Suite -ne $required -or $test.CodeCommit -ne $package.Info.codeCommit -or $test.Headless -isnot [bool] -or $test.Headless -ne $true) {
            throw "A passing headless result is required: $required"
        }
        $tests+=$test
    }
}
$timings.TestsSeconds=$stage.Elapsed.TotalSeconds; $stage.Restart()
$current=& (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $package.PackageDirectory
foreach ($name in $package.Hashes.Keys) {
    if ($current.Hashes[$name] -ne $package.Hashes[$name]) { throw 'Core changed during testing; rebuild and retest.' }
}
$candidate=& (Join-Path $PSScriptRoot 'Build-ReleaseCandidate.ps1') -PackageDirectory $package.PackageDirectory -OutputDirectory $OutputDirectory
$snapshot=& (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $candidate.CandidateDirectory
foreach ($name in $package.Hashes.Keys) {
    if ($snapshot.Info.coreFiles.$name -ne $package.Hashes[$name]) { throw 'Frozen core differs from the tested package.' }
}
$timings.FreezeSeconds=$stage.Elapsed.TotalSeconds; $stage.Restart()
$workshop=& (Join-Path $PSScriptRoot 'Prepare-Workshop.ps1') -SnapshotDirectory $candidate.CandidateDirectory
$timings.WorkshopSeconds=$stage.Elapsed.TotalSeconds
$timings.TotalSeconds=$total.Elapsed.TotalSeconds
$result=[pscustomobject]@{
    Status='PREPARED'; CodeCommit=$package.Info.codeCommit; CoreFiles=$package.Hashes
    RequiredSuites=$Suite; Tests=$tests; ReusedTests=[bool]$TestResultFile
    Full=@($tests | Where-Object Suite -eq 'Full') | Select-Object -First 1
    Timings=$timings
    Candidate=$candidate.Candidate; CandidateSHA256=$candidate.CandidateSHA256
    SnapshotDirectory=$candidate.CandidateDirectory; SnapshotSHA256=$workshop.SnapshotSHA256
    WorkshopDirectory=$workshop.WorkshopDirectory; Uploaded=$false
    Rendering='NOT_RUN'; Multiplayer='NOT_RUN'; DiskResume='NOT_RUN'
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath ($candidate.CandidateDirectory+'.preparation.json') -Encoding utf8
$retention=& (Join-Path $PSScriptRoot 'Remove-OldDeliverables.ps1') -PreparationFile ($candidate.CandidateDirectory+'.preparation.json')
$result | Add-Member -NotePropertyName Retention -NotePropertyValue $retention
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath ($candidate.CandidateDirectory+'.preparation.json') -Encoding utf8
$result
