param([Parameter(Mandatory)][string]$LogFile, [string]$CodeCommit, [switch]$ExpectAbsent, [switch]$Headless)
$ErrorActionPreference = 'Stop'
$deadline = [DateTime]::UtcNow.AddSeconds(45)
$text = ''
do {
    if (Test-Path -LiteralPath $LogFile) { $text = Get-Content -Raw -LiteralPath $LogFile }
    if ($text -match '\[Startup\] Time to main menu:') { break }
    Start-Sleep -Milliseconds 500
} while ([DateTime]::UtcNow -lt $deadline)
if ($text -notmatch '\[Startup\] Time to main menu:') { throw 'Game did not reach the main menu within 45 seconds. Inspect the local log and any startup dialog.' }
$ready = [regex]::Matches($text, '(?m)^STS2MOD_READY[^\r\n]*')
if ($ExpectAbsent) {
    if ($ready.Count -or $text -match "Finished mod initialization for '[^']*' \(Sts2Mod\)") { throw 'Sts2Mod was still loaded after removal.' }
    Write-Output 'PASS: game reached main menu without loading Sts2Mod.'
    return
}
if ($ready.Count -ne 1) { throw "Expected one Sts2Mod initialization marker, got $($ready.Count)." }
if ($CodeCommit -and $ready[0].Value -notmatch ('\+' + [regex]::Escape($CodeCommit) + '(?=\s)')) { throw 'Loaded assembly does not match the expected source commit.' }
if ($ready[0].Value -notmatch 'baselib=3\.3\.2 resource=ok') { throw 'Dependency/resource verification failed.' }
if ($text -notmatch "Finished mod initialization for 'BaseLib' \(BaseLib\)" -or $text -notmatch "Finished mod initialization for '[^']*' \(Sts2Mod\)") { throw 'Game loader did not report both mods initialized.' }
$errorLines = [regex]::Matches($text, '(?m)^ERROR:[^\r\n]*') | ForEach-Object Value
if ($Headless) {
    $knownHeadlessRenderErrors = @(
        'ERROR: Attempting to initialize the wrong RID',
        'ERROR: Parameter "mem" is null.',
        'ERROR: Attempting to use an uninitialized RID',
        'ERROR: Parameter "shader" is null.'
    )
    $errorLines = $errorLines | Where-Object { $_ -notin $knownHeadlessRenderErrors }
}
if ($text -match '\[ERROR\]' -or $errorLines.Count) { throw 'Game startup logged an unexpected error. Review it before marking the smoke test as passed.' }
Write-Output $ready[0].Value
Write-Output 'PASS: exactly one initialization, pinned BaseLib and resource check succeeded, game reached main menu without startup errors.'
