param([Parameter(Mandatory)][string]$LogFile,[Parameter(Mandatory)][string]$CodeCommit)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-TravelerRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Task003 -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
foreach($name in @('UX_block_feedback_4_and_8','UX_draw_feedback_1_and_2','UX_attack_feedback_base8','UX_limited_draw_feedback_actual_count','UX_hover_100_times_no_gameplay_mutation','UX_rock_preview_paimon8_in_card_and_tip','UX_lightning_preview_paimon_draw2','UX_preview_matches_native_block_reward','UX_no_reward_promise_when_unplayable','UX_limit_exhausted_preview','UX_no_target_warns_consumed_use','UX_no_target_preview_matches_native_skip','UX_three_effects_free_without_gameplay_wait')) {
    if($text -notmatch ('(?m)^STS2MOD_TEST_PASS '+[regex]::Escape($name)+'\r?$')){throw "Missing presentation case: $name"}
}
foreach($name in @('wind','rock','lightning','tailwind','rock_advance','thunder_prep','paimon')){
    if(!$text.Contains("STS2MOD_TEST_PASS UX_art_import_$name")){throw "Missing art import: $name"}
}
if($text -notmatch '(?m)^STS2MOD_UX_COMPLETE assertions=(\d+) rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){throw 'Presentation tests incomplete'}
$uxCount=$Matches[1]
if($text -notmatch '(?m)^STS2MOD_TEST_COMPLETE assertions=(\d+) '){throw 'Full suite incomplete'}
$total=[int]$Matches[1]
if(([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ')).Count -ne $total){throw 'Assertion total mismatch'}
Write-Output "EXPERIENCE_PASS total=$total presentation=$uxCount rendering=NOT_RUN"
