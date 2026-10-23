param([Parameter(Mandatory)][string]$PackageDirectory, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$package = & (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $PackageDirectory
& (Join-Path $PSScriptRoot 'Deploy-Mod.ps1') -PackageDirectory $package.PackageDirectory | Out-Host
$test = & (Join-Path $PSScriptRoot 'Invoke-ModTests.ps1') -Suite Full -CodeCommit $package.Info.codeCommit
if ($test.Status -ne 'PASS' -or $test.Suite -ne 'Full' -or $test.CodeCommit -ne $package.Info.codeCommit -or $test.Headless -ne $true) {
    throw 'A passing headless Full result for this core is required.'
}
$current = & (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory $package.PackageDirectory
foreach ($name in $package.Hashes.Keys) {
    if ($current.Hashes[$name] -ne $package.Hashes[$name]) { throw 'Core changed during testing; rebuild and retest.' }
}
$candidate = & (Join-Path $PSScriptRoot 'Build-ReleaseCandidate.ps1') -PackageDirectory $package.PackageDirectory -OutputDirectory $OutputDirectory
$snapshot = & (Join-Path $PSScriptRoot 'Get-ReleaseSnapshot.ps1') -SnapshotDirectory $candidate.CandidateDirectory
foreach ($name in $package.Hashes.Keys) {
    if ($snapshot.Info.coreFiles.$name -ne $package.Hashes[$name]) { throw 'Frozen core differs from the tested package.' }
}
$workshop = & (Join-Path $PSScriptRoot 'Prepare-Workshop.ps1') -SnapshotDirectory $candidate.CandidateDirectory
$result = [pscustomobject]@{
    Status='PREPARED'; CodeCommit=$package.Info.codeCommit; CoreFiles=$package.Hashes; Full=$test
    Candidate=$candidate.Candidate; CandidateSHA256=$candidate.CandidateSHA256
    SnapshotDirectory=$candidate.CandidateDirectory; SnapshotSHA256=$workshop.SnapshotSHA256
    WorkshopDirectory=$workshop.WorkshopDirectory; Uploaded=$false
    Rendering='NOT_RUN'; Multiplayer='NOT_RUN'; DiskResume='NOT_RUN'
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath ($candidate.CandidateDirectory + '.preparation.json') -Encoding utf8
$result
