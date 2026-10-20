param([Parameter(Mandatory)][string]$LogFile,[Parameter(Mandatory)][string]$CodeCommit,[switch]$Focused)
$ErrorActionPreference='Stop'
if($Focused) {
    & (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
} else {
    & (Join-Path $PSScriptRoot 'Test-ContinuousComboRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
}
$text=Get-Content -Raw -LiteralPath $LogFile
if($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR'){throw 'Character test failed'}
foreach($case in @('registered_once','native_selection_button_unlocked','native_selection_loads_background_and_starter_info',
'exact_ten_card_starter','compass_only_starting_relic','eight_models_six_reward_cards_in_exclusive_pool',
'ironclad_and_silent_no_theme_cards','paimon_and_compass_exclusive_not_shared','basic_transform_stays_in_exclusive_pool',
'compass_not_consumed_by_targetless_skip','compass_first_success_draws_one','compass_second_combo_no_extra_draw',
'compass_does_not_reset_each_turn','compass_resets_for_new_combat','paimon_doubles_combo_not_compass',
'compass_not_consumed_when_combo_draws_zero','compass_triggers_after_earlier_skipped_rewards')) {
    if($text -notmatch ('(?m)^STS2MOD_TEST_PASS CHAR_'+[regex]::Escape($case)+'\r?$')){throw "Missing character case: $case"}
}
foreach($kind in @('Monster','Elite','Boss')) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_reward_pool_$kind")){throw "Missing native reward case: $kind"}
}
foreach($index in 0..4) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_shop_five_slots_and_shared_potions_$index")){throw "Missing native shop case $index"}
}
if($text -notmatch '(?m)^STS2MOD_CHARACTER_CASES_COMPLETE assertions=\d+ starter=10 exclusive_cards=6 starter_relic=1 rendering=NOT_RUN multiplayer=NOT_RUN'){throw 'Character cases incomplete'}
if($Focused -and $text -notmatch '(?m)^STS2MOD_CHARACTER_COMPLETE assertions=\d+ singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False'){throw 'Focused run incomplete'}
Write-Output 'TRAVELER_CHARACTER_PASS starter=10 exclusive_cards=6 starter_relic=1 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
