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
'exact_ten_card_starter','compass_only_starting_relic','twenty_models_eighteen_reward_cards_in_exclusive_pool',
'ironclad_and_silent_no_theme_cards','paimon_and_compass_exclusive_not_shared','basic_transform_stays_in_exclusive_pool',
'compass_triggers_after_random_defensive_attack','compass_cooldown_turn_no_draw','compass_second_combo_no_extra_draw',
'compass_draws_again_two_turns_after_trigger','extra_turn_counts_as_compass_cooldown_turn','extra_turn_cooldown_does_not_draw',
'compass_resets_for_new_combat','paimon_doubles_combo_not_compass',
'compass_not_consumed_when_combo_draws_zero','compass_triggers_after_earlier_skipped_rewards',
'compass_ready_icon_1_active','compass_spent_icon_0_disabled','compass_next_turn_cooldown_indicator',
'compass_ready_after_one_gap_turn','compass_ready_after_extra_turn_gap','compass_unused_ready_turn_is_not_consumed',
'compass_own_draw_zero_still_starts_cooldown','compass_zero_draw_cooldown_survives_turn',
'compass_combat_end_clears_cooldown_and_icon','compass_indicator_publishes_native_counter_changes')) {
    if($text -notmatch ('(?m)^STS2MOD_TEST_PASS CHAR_'+[regex]::Escape($case)+'\r?$')){throw "Missing character case: $case"}
}
foreach($kind in @('Monster','Elite','Boss')) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_reward_pool_$kind")){throw "Missing native reward case: $kind"}
}
foreach($index in 0..4) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_shop_five_slots_and_shared_potions_$index")){throw "Missing native shop case $index"}
}
if($text -notmatch '(?m)^STS2MOD_CHARACTER_CASES_COMPLETE assertions=\d+ starter=10 exclusive_cards=18 starter_relic=1 rendering=NOT_RUN multiplayer=NOT_RUN'){throw 'Character cases incomplete'}
foreach($case in @('traveler_uses_custom_scene','ironclad_resource_unchanged','native_room_mounts_traveler',
 'custom_texture_imported','no_inherited_ironclad_spine','seated_illustration_bounded','native_interaction_nodes_exist',
 'rest_and_smith_buttons_preserved','native_flip_keeps_art_and_hitbox_together','flip_restores_original_pose',
 'native_option_bubble_works','room_and_character_free_on_exit')) {
 if(!$text.Contains("STS2MOD_TEST_PASS REST_$case")){throw "Missing rest-site case: $case"}
}
if($text -notmatch '(?m)^STS2MOD_REST_SITE_COMPLETE assertions=\d+ scene=PASS native_options=PASS cleanup=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){
 throw 'Rest-site suite incomplete'
}
if($Focused -and $text -notmatch '(?m)^STS2MOD_CHARACTER_COMPLETE assertions=\d+ singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False'){throw 'Focused run incomplete'}
Write-Output 'TRAVELER_CHARACTER_PASS starter=10 exclusive_cards=18 starter_relic=1 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
