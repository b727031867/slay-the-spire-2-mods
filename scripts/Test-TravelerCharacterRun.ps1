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
'exact_ten_card_starter','compass_only_starting_relic','twenty_nine_models_twenty_seven_reward_cards_in_exclusive_pool',
'ironclad_and_silent_no_theme_cards','paimon_and_compass_exclusive_not_shared','basic_transform_stays_in_exclusive_pool',
'compass_four_combo_limit','compass_no_draw_after_first_reward','compass_third_reward_one_strength',
'compass_fourth_allowed_fifth_capped','compass_strength_each_turn_no_cooldown','compass_new_combat_clears_strength',
'compass_skipped_reward_not_strength_progress','compass_third_actual_after_skip_strength_no_draw',
'upgrade_orobas_maps_compass_instead_of_circlet','upgrade_native_preview_names_real_upgrade',
'upgrade_pending_reward_native_serialization','upgrade_vanilla_starter_mapping_unchanged',
'upgrade_native_reward_replaces_original_once','upgrade_saved_relic_roundtrip_no_fake_zero_counter',
'upgrade_traveler_exclusive','upgrade_first_element_has_no_reward_draw','upgrade_three_rewards_draw_three_same_turn',
'upgrade_does_not_remove_shared_combo_cap','upgrade_next_turn_has_no_cooldown','upgrade_paimon_does_not_double_compass',
'upgrade_zero_combo_reward_does_not_trigger','upgrade_blocked_own_draw_is_not_a_new_combo',
'upgrade_draws_again_after_draw_prevention')) {
    if($text -notmatch ('(?m)^STS2MOD_TEST_PASS CHAR_'+[regex]::Escape($case)+'\r?$')){throw "Missing character case: $case"}
}
foreach($case in @('two_traveler_portraits_do_not_overlap','four_mixed_players_and_names_separated',
 'native_portrait_and_ready_state_preserved','positions_stable_after_shake_duration','container_reflow_keeps_rows_separate')) {
 if(!$text.Contains("STS2MOD_TEST_PASS CHAR_lobby_$case")){throw "Missing lobby layout case: $case"}
}
foreach($index in 0..7) {
 if(!$text.Contains("STS2MOD_TEST_PASS CHAR_lobby_repeated_character_switch_$index")){throw "Missing lobby switch case: $index"}
}
foreach($kind in @('Monster','Elite','Boss')) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_reward_pool_$kind")){throw "Missing native reward case: $kind"}
}
foreach($index in 0..4) {
    if(!$text.Contains("STS2MOD_TEST_PASS CHAR_native_shop_five_slots_and_shared_potions_$index")){throw "Missing native shop case $index"}
}
if($text -notmatch '(?m)^STS2MOD_CHARACTER_CASES_COMPLETE assertions=\d+ starter=10 exclusive_cards=27 starter_relic=1 rendering=NOT_RUN multiplayer=NOT_RUN'){throw 'Character cases incomplete'}
foreach($case in @('traveler_uses_custom_scene','ironclad_resource_unchanged','native_room_mounts_traveler',
 'custom_texture_imported','no_inherited_ironclad_spine','seated_illustration_bounded','native_interaction_nodes_exist',
 'rest_and_smith_buttons_preserved','native_flip_keeps_art_and_hitbox_together','flip_restores_original_pose',
 'native_option_bubble_works','room_and_character_free_on_exit')) {
 if(!$text.Contains("STS2MOD_TEST_PASS REST_$case")){throw "Missing rest-site case: $case"}
}
if($text -notmatch '(?m)^STS2MOD_REST_SITE_COMPLETE assertions=\d+ scene=PASS native_options=PASS cleanup=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){
 throw 'Rest-site suite incomplete'
}
foreach($case in @('traveler_scene_does_not_replace_ironclad','traveler_art_no_ironclad_spine',
 'proportional_art_feet_on_floor','native_inventory_and_controls_preserved','character_freed_on_exit',
 'native_ironclad_animation_not_intercepted')) {
 if(!$text.Contains("STS2MOD_TEST_PASS SHOP_$case")){throw "Missing merchant presentation case: $case"}
}
if($text -notmatch '(?m)^STS2MOD_MERCHANT_COMPLETE assertions=\d+ scene=PASS cleanup=PASS rendering=NOT_RUN\r?$'){
 throw 'Merchant presentation suite incomplete'
}
if($Focused -and $text -notmatch '(?m)^STS2MOD_CHARACTER_COMPLETE assertions=\d+ singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False'){throw 'Focused run incomplete'}
Write-Output 'TRAVELER_CHARACTER_PASS starter=10 exclusive_cards=27 starter_relic=1 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
