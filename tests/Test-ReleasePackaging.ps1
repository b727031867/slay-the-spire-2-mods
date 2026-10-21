$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$fixture=Join-Path $root ('.local/packaging-tests/'+[guid]::NewGuid().ToString('N'))
$core=Join-Path $fixture 'core'
New-Item -ItemType Directory -Path $core -Force | Out-Null
$commit='a'*40
Set-Content -LiteralPath (Join-Path $core 'Sts2Mod.dll') -Value 'fixture DLL bytes'
Set-Content -LiteralPath (Join-Path $core 'Sts2Mod.pck') -Value 'fixture PCK bytes'
@{id='Sts2Mod';version='0.3.5';min_game_version='0.107.1';dependencies=@(@{id='BaseLib';min_version='3.4.7'})} | ConvertTo-Json | Set-Content (Join-Path $core 'Sts2Mod.json')
$hashes=@{}
foreach($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json')){
    $hashes[$name]=(Get-FileHash (Join-Path $core $name)).Hash
}
$info=@{schemaVersion=1;modId='Sts2Mod';modVersion='0.3.5';codeCommit=$commit;dirty=$false;gameVersion='v0.107.1';gameBuild='23811903';baseLibVersion='3.4.7';files=$hashes}
$buildInfo=Join-Path $core 'build-info.txt'
$info | ConvertTo-Json | Set-Content $buildInfo
$originalInfoHash=(Get-FileHash $buildInfo).Hash
$count=0
function Check($condition,$message) {
    if(!$condition){throw $message}
    $script:count++; Write-Output "PASS $message"
}
function Reject([scriptblock]$action,$message) {
    $rejected=$false
    try {& $action | Out-Null} catch {$rejected=$true}
    Check $rejected $message
}
$output=Join-Path $fixture 'candidate'
$candidate=& (Join-Path $root 'scripts/Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory $output
Check ($candidate.CodeCommit -eq $commit) 'existing core identity retained without building'
foreach($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json','build-info.txt')){
    Check ((Get-FileHash (Join-Path $output "Sts2Mod/$name")).Hash -eq (Get-FileHash (Join-Path $core $name)).Hash) "core bytes retained: $name"
}
Check ((Get-FileHash $buildInfo).Hash -eq $originalInfoHash) 'source build metadata unchanged'
$provenance=Get-Content -Raw (Join-Path $output 'release-info.txt') | ConvertFrom-Json
Check ($provenance.coreFiles.'build-info.txt' -eq $originalInfoHash -and $provenance.documents.'CHANGELOG.md') 'core and document provenance recorded separately'
$expanded=Join-Path $fixture 'expanded'
Expand-Archive -LiteralPath $candidate.Candidate -DestinationPath $expanded
Check ((Get-FileHash (Join-Path $expanded 'Sts2Mod/Sts2Mod.dll')).Hash -eq $hashes.'Sts2Mod.dll') 'archive core bytes match source'
Reject {& (Join-Path $root 'scripts/Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory $output} 'existing candidate is never overwritten'
Push-Location $fixture
try {
    $relative=& (Join-Path $root 'scripts/Build-ReleaseCandidate.ps1') -PackageDirectory $core -OutputDirectory 'relative-candidate'
    Check ($relative.CandidateDirectory -eq (Join-Path $fixture 'relative-candidate')) 'relative output follows PowerShell location'
} finally { Pop-Location }

$info.dirty=$true
$info | ConvertTo-Json | Set-Content $buildInfo
Reject {& (Join-Path $root 'scripts/Get-ModPackage.ps1') -PackageDirectory $core} 'dirty core rejected'
$info.dirty=$false
$info | ConvertTo-Json | Set-Content $buildInfo
$info.baseLibVersion='0.0.0'
$info | ConvertTo-Json | Set-Content $buildInfo
Reject {& (Join-Path $root 'scripts/Get-ModPackage.ps1') -PackageDirectory $core} 'dependency metadata mismatch rejected'
$info.baseLibVersion='3.4.7'
$info.modVersion='9.9.9'
$info | ConvertTo-Json | Set-Content $buildInfo
Reject {& (Join-Path $root 'scripts/Get-ModPackage.ps1') -PackageDirectory $core} 'manifest and build version mismatch rejected'
$info.modVersion='0.3.5'
$info | ConvertTo-Json | Set-Content $buildInfo
Set-Content (Join-Path $core 'extra.txt') 'unexpected'
Reject {& (Join-Path $root 'scripts/Get-ModPackage.ps1') -PackageDirectory $core} 'unexpected package file rejected'
# Retain the unexpected fixture and test checksum rejection on another exact four-file copy.
$corrupt=Join-Path $fixture 'corrupt'
New-Item -ItemType Directory -Path $corrupt | Out-Null
foreach($name in @('Sts2Mod.dll','Sts2Mod.pck','Sts2Mod.json','build-info.txt')){
    Copy-Item -LiteralPath (Join-Path $core $name) -Destination $corrupt
}
$core=$corrupt
Add-Content -LiteralPath (Join-Path $core 'Sts2Mod.dll') -Value 'tamper'
Reject {& (Join-Path $root 'scripts/Get-ModPackage.ps1') -PackageDirectory $core} 'tampered core rejected'
Write-Output "RELEASE_PACKAGING_PASS assertions=$count fixture=$fixture"
