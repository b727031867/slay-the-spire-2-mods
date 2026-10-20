param([Parameter(Mandatory)][string]$LogFile, [string]$CodeCommit, [switch]$ExpectAbsent, [switch]$Headless, [switch]$NativeShutdown)
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
$expectedBase = (Get-Content -Raw -LiteralPath (Join-Path (Split-Path $PSScriptRoot -Parent) 'dependencies.json') | ConvertFrom-Json).baseLib.version
if ($ready[0].Value -notmatch ('baselib=' + [regex]::Escape($expectedBase) + ' resource=ok')) { throw 'Dependency/resource verification failed.' }
if ($text -notmatch "Finished mod initialization for 'BaseLib' \(BaseLib\)" -or $text -notmatch "Finished mod initialization for '[^']*' \(Sts2Mod\)") { throw 'Game loader did not report both mods initialized.' }
$errors = & (Join-Path $PSScriptRoot 'Get-GameLogErrors.ps1') -Text $text -Headless:$Headless -NativeShutdown:$NativeShutdown
if ($text -match '\[ERROR\]' -or $errors.Unexpected.Count) { throw 'Game logged an unexpected error. Review it before marking the smoke test as passed.' }
Write-Output $ready[0].Value
Write-Output "PASS: one initialization, pinned dependency/resources, main menu reached; unexpected errors=0, known native headless errors=$($errors.KnownHeadless.Count), native shutdown diagnostics=$($errors.KnownNativeShutdown.Count)."
