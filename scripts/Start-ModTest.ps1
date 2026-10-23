param(
    [Parameter(Mandatory)][ValidateSet('Load','Visuals','Cards','Character','Events','Full','SavePrepare','SaveResume')][string]$Suite,
    [string]$SandboxDirectory
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ModTestSuites.ps1')
$definition = Get-ModTestSuite $Suite
if ($SandboxDirectory -and $Suite -ne 'SaveResume') { throw 'Only SaveResume accepts an existing sandbox.' }
if ($Suite -eq 'SaveResume' -and !$SandboxDirectory) { throw 'SaveResume requires the prepare sandbox.' }
$arguments = @{ Headless=$true }
if ($definition.Flag) { $arguments[$definition.Flag]=$true }
if ($SandboxDirectory) { $arguments.SandboxDirectory=$SandboxDirectory }
$launch = & (Join-Path $PSScriptRoot 'Start-ModGame.ps1') @arguments -PassThru
if (!$launch.GameProcessId -or !$launch.LogFile -or !$launch.Process) {
    throw 'Launch did not return its owned process and log.'
}
[pscustomobject]@{
    Suite=$Suite; GameProcessId=$launch.GameProcessId; LogFile=$launch.LogFile
    SandboxDirectory=$launch.SandboxDirectory; Headless=$true; Process=$launch.Process
}
