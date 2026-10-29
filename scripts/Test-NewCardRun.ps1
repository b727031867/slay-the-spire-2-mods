param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
if ($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR') { throw 'New-card suite failed.' }
if ($text -notmatch '(?m)^STS2MOD_NEWCARDS_COMPLETE assertions=\d+ cards=10 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$') { throw 'New-card suite incomplete.' }
foreach ($id in 29..38) {
    foreach ($up in @('False','True')) {
        if ($text -notmatch "(?m)^STS2MOD_TEST_PASS NEW_metadata_TC_C${id}_${up}\r?$") { throw "Missing new card metadata: $id/$up" }
    }
    if ($text -notmatch "(?m)^STS2MOD_TEST_PASS NEW_art_TC_C${id}\r?$") { throw "Missing new card art: $id" }
}
foreach ($case in @('pool','search','retain','counter','dexterity','vigor','navigation','needle','poison_wind','satchel','barrier','boundaries')) {
    if ($text -notmatch "(?m)^STS2MOD_TEST_PASS NEW_${case}_complete\r?$") { throw "Missing new card behavior: $case" }
}
'NEW_CARDS_PASS cards=10 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
