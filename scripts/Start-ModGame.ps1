param([switch]$Headless, [switch]$TravelerDebug, [switch]$TravelerTest, [switch]$TravelerRewards, [switch]$TravelerAcceptance)
$ErrorActionPreference = 'Stop'
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
if ($TravelerDebug -or $TravelerTest -or $TravelerRewards -or $TravelerAcceptance) { $arguments += '--sts2mod-debug' }
if ($TravelerTest -or $TravelerRewards -or $TravelerAcceptance) { $arguments += '--sts2mod-test' }
if ($TravelerRewards) { $arguments += '--sts2mod-rewards' }
if ($TravelerAcceptance) { $arguments += '--sts2mod-acceptance' }
if ($Headless) { $arguments += '--headless' } else { $arguments += @('--windowed','--resolution','1280x720') }
$oldAppId = $env:SteamAppId
$oldGameId = $env:SteamGameId
try {
    # Supply the real App ID for a direct developer launch; Steam still handles account/ownership checks.
    $env:SteamAppId = '2868840'
    $env:SteamGameId = '2868840'
    $windowStyle = if ($Headless) { 'Hidden' } else { 'Normal' }
    $process = Start-Process -FilePath (Join-Path $env:STS2_GAME_DIR 'SlayTheSpire2.exe') -WorkingDirectory $env:STS2_GAME_DIR -ArgumentList $arguments -WindowStyle $windowStyle -PassThru
    Write-Output "GameProcessId=$($process.Id)"
    Write-Output "LogFile=$logPath"
} finally {
    $env:SteamAppId = $oldAppId
    $env:SteamGameId = $oldGameId
}
