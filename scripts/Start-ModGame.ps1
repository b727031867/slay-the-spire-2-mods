param([switch]$Headless, [switch]$TravelerDebug, [switch]$TravelerTest, [switch]$TravelerRewards, [switch]$TravelerAcceptance,
    [switch]$TravelerCharacter, [switch]$TravelerCards, [switch]$TravelerVisuals, [switch]$TravelerEvents, [switch]$TravelerEndToEnd, [switch]$ResumeTask003, [string]$SandboxDirectory, [switch]$PassThru)
$ErrorActionPreference = 'Stop'
# Automated game tests must not open a window or steal focus from the user's work.
if ($TravelerCharacter) { $TravelerTest = $true }
if ($TravelerCards) { $TravelerTest = $true }
if ($TravelerVisuals) { $TravelerTest = $true }
if ($TravelerEvents) { $TravelerTest = $true }
if ($TravelerTest -or $TravelerAcceptance -or $TravelerEndToEnd -or $ResumeTask003) { $Headless = $true }
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
$projectRoot = Split-Path $PSScriptRoot -Parent
if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) { throw 'The game is already running.' }
if (!(Get-Process -Name steam -ErrorAction SilentlyContinue)) { throw 'Open Steam and sign in with a game-owning account first.' }
$expected = Get-Content -Raw (Join-Path $projectRoot 'dependencies.json') | ConvertFrom-Json
$gameInfo = Get-Content -Raw (Join-Path $env:STS2_GAME_DIR 'release_info.json') | ConvertFrom-Json
if ($gameInfo.version -ne $expected.gameVersion) { throw 'Installed game version differs from the validated baseline.' }
$logDir = Join-Path $projectRoot '.local/logs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logPath = Join-Path $logDir ('game-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ') + '.log')
$arguments = @('--log-file', ('"' + $logPath + '"'))
if ($TravelerEndToEnd -or $ResumeTask003) { $TravelerTest = $true }
if ($TravelerTest -or $TravelerAcceptance -or $TravelerRewards) {
    $sandboxRoot = [IO.Path]::GetFullPath((Join-Path $projectRoot '.local/task003-saves'))
    if ((Test-Path -LiteralPath $sandboxRoot) -and ((Get-Item -LiteralPath $sandboxRoot).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Refusing a redirected sandbox root.' }
    if (!$SandboxDirectory) {
        if ($ResumeTask003) { throw 'Resume requires the existing SandboxDirectory from the prepare launch.' }
        $SandboxDirectory = Join-Path $sandboxRoot ([Guid]::NewGuid().ToString('N'))
    }
    $SandboxDirectory = [IO.Path]::GetFullPath($SandboxDirectory)
    if ((Split-Path $SandboxDirectory -Parent) -ne $sandboxRoot) { throw 'Test sandbox must be an immediate child of .local/task003-saves.' }
    $marker = Join-Path $SandboxDirectory '.sts2mod-test-sandbox'
    if ($ResumeTask003) {
        if ((Test-Path -LiteralPath $SandboxDirectory) -and ((Get-Item -LiteralPath $SandboxDirectory).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Refusing a redirected sandbox.' }
        if (!(Test-Path -LiteralPath $marker) -or (Get-Content -Raw -LiteralPath $marker).Trim() -ne 'STS2MOD_TEST_SANDBOX_V1') { throw 'Not an owned test sandbox.' }
    } else {
        if (Test-Path -LiteralPath $SandboxDirectory) { throw 'Prepare requires a fresh sandbox; existing data is preserved.' }
        New-Item -ItemType Directory -Path $SandboxDirectory -Force | Out-Null
        Set-Content -LiteralPath $marker -Value 'STS2MOD_TEST_SANDBOX_V1' -Encoding ascii
    }
    $arguments += @('--sts2mod-sandbox', ('"' + $SandboxDirectory + '"'))
    if (!$PassThru) { Write-Output "SandboxDirectory=$SandboxDirectory" }
}
if ($TravelerDebug -or $TravelerTest -or $TravelerRewards -or $TravelerAcceptance) { $arguments += '--sts2mod-debug' }
if ($TravelerTest -or $TravelerRewards -or $TravelerAcceptance) { $arguments += '--sts2mod-test' }
if ($TravelerRewards) { $arguments += '--sts2mod-rewards' }
if ($TravelerAcceptance) { $arguments += '--sts2mod-acceptance' }
if ($TravelerCharacter) { $arguments += '--sts2mod-character' }
if ($TravelerCards) { $arguments += '--sts2mod-cards' }
if ($TravelerVisuals) { $arguments += '--sts2mod-visuals' }
if ($TravelerEvents) { $arguments += '--sts2mod-events' }
if ($TravelerEndToEnd) { $arguments += '--sts2mod-e2e' }
if ($ResumeTask003) { $arguments += '--sts2mod-resume' }
if ($Headless) { $arguments += '--headless' } else { $arguments += @('--windowed','--resolution','1280x720') }
$oldAppId = $env:SteamAppId
$oldGameId = $env:SteamGameId
try {
    # Supply the real App ID for a direct developer launch; Steam still handles account/ownership checks.
    $env:SteamAppId = '2868840'
    $env:SteamGameId = '2868840'
    $windowStyle = if ($Headless) { 'Hidden' } else { 'Normal' }
    $process = Start-Process -FilePath (Join-Path $env:STS2_GAME_DIR 'SlayTheSpire2.exe') -WorkingDirectory $env:STS2_GAME_DIR -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
    if ($PassThru) {
        [pscustomobject]@{
            GameProcessId=$process.Id; LogFile=$logPath; SandboxDirectory=$SandboxDirectory
            Headless=[bool]$Headless; Process=$process
        }
    } else {
        Write-Output "GameProcessId=$($process.Id)"
        Write-Output "LogFile=$logPath"
    }
} finally {
    $env:SteamAppId = $oldAppId
    $env:SteamGameId = $oldGameId
}
