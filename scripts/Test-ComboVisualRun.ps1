param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$WithinExpansion)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
if($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR'){throw 'Combo visual acceptance failed'}
foreach($case in @('always_visible_without_hover','one_hud_per_combat_ui','all_controls_click_through_no_focus',
 'layout_bounds_1280_720','no_unexpected_fixture_modal','modal_hides_hud_and_highlight','hud_returns_after_modal_closes',
 'actual_play_updates_wind_icon','native_status_icon_refreshes_after_play',
 'hover_rock_highlights_block_not_current_element','100_previews_no_state_rng_energy_or_hand_mutation',
 'selected_card_overrides_hover_and_highlights_draw','cancel_selection_returns_to_hover','unhover_clears_highlight',
 'same_element_only_records','ability_has_no_element_highlight','unplayable_never_highlights_reward',
 'paimon_updates_preview_and_chips','actual_play_updates_rock_and_remaining',
 'targeted_attack_highlight_is_base_and_conditional','skill_attack_highlight_is_random_no_rng_use',
 'actual_play_updates_lightning_icon','cap_zero_dims_all_rewards','turn_reset_keeps_element_and_restores_pips',
 'new_combat_clears_element_selection_and_old_panel')) {
 if(!$text.Contains("STS2MOD_TEST_PASS VIS_$case")){throw "Missing visual case: $case"}
}
foreach($element in @('None','Wind','Rock','Lightning')) {
 if(!$text.Contains("STS2MOD_TEST_PASS VIS_icon_resource_$element")){throw "Missing element icon: $element"}
}
foreach($kind in @('WindRock','RockLightning','LightningWind')) {
 if(!$text.Contains("STS2MOD_TEST_PASS VIS_reward_resource_$kind")){throw "Missing reward icon: $kind"}
}
if($text -notmatch '(?m)^STS2MOD_VISUAL_COMPLETE assertions=\d+ nodes=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){
 throw 'Visual-node suite incomplete'
}
if(!$WithinExpansion){
 if($text -notmatch '(?m)^STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=(\d+) singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False\r?$'){
  throw 'Visual focused suite incomplete'
 }
 if([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ').Count -ne [int]$Matches[1]){throw 'Focused assertion total mismatch'}
}
Write-Output 'COMBO_VISUAL_PASS nodes=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
