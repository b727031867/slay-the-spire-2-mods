param(
    [Parameter(Mandatory)][ValidateSet('Load','Visuals','Cards','NewCards','Character','Events','Full','SavePrepare','SaveResume')][string]$Suite,
    [Parameter(Mandatory)][string]$LogFile,
    [Parameter(Mandatory)][string]$CodeCommit,
    [string]$ResultFile,
    [int]$GameProcessId,
    [string]$SandboxDirectory
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ModTestSuites.ps1')
$null = Get-ModTestSuite $Suite
$evidence=@()
try {
    $evidence = @(switch ($Suite) {
        Load { & (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless }
        Visuals { & (Join-Path $PSScriptRoot 'Test-ComboVisualRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit }
        Cards { & (Join-Path $PSScriptRoot 'Test-ExpandedCardRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit }
        NewCards { & (Join-Path $PSScriptRoot 'Test-NewCardRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit }
        Character { & (Join-Path $PSScriptRoot 'Test-TravelerCharacterRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Focused }
        Events { & (Join-Path $PSScriptRoot 'Test-TravelerEventsRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit }
        Full {
            & (Join-Path $PSScriptRoot 'Test-TravelerRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Task003 -Headless
            & (Join-Path $PSScriptRoot 'Test-ExpandedCardRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Full
            & (Join-Path $PSScriptRoot 'Test-TravelerEventsRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
        }
        SavePrepare { & (Join-Path $PSScriptRoot 'Test-Task003EndToEnd.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Phase Prepare }
        SaveResume { & (Join-Path $PSScriptRoot 'Test-Task003EndToEnd.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Phase Resume }
    })
    $status='PASS'; $failure=$null
} catch {
    $status='FAIL'; $failure=$_.Exception.Message
}
$text = if (Test-Path -LiteralPath $LogFile) { Get-Content -Raw -LiteralPath $LogFile } else { '' }
$cases = @([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ([^\r\n]+)\r?$') | ForEach-Object { $_.Groups[1].Value })
$result = [pscustomobject]@{
    SchemaVersion=1; Suite=$Suite; Status=$status; CodeCommit=$CodeCommit
    LogFile=$ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($LogFile); Assertions=$cases.Count; Cases=$cases
    Headless=$true; Rendering='NOT_RUN'; Multiplayer='NOT_RUN'; Evidence=$evidence; Failure=$failure
}
if ($PSBoundParameters.ContainsKey('GameProcessId')) {
    $result | Add-Member -NotePropertyName GameProcessId -NotePropertyValue $GameProcessId
    $result | Add-Member -NotePropertyName SandboxDirectory -NotePropertyValue $SandboxDirectory
}
if ($ResultFile) { $result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $ResultFile -Encoding utf8 }
if ($status -eq 'FAIL') { throw $failure }
$result
