param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$WithinExpansion)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
foreach($case in @('pool_TravelerCardPool','pool_TravelerRelicPool','pool_TravelerPotionPool',
 'native_inline_glyph_bounded','acquired_native_relic_inline','unowned_native_relic_inline',
 'other_native_relic_inline','owned_potion_inline','card_inline','native_hover_icon_bounded','native_hover_layout_bounded')) {
 if(!$text.Contains("STS2MOD_TEST_PASS ENERGY_$case")){throw "Missing energy icon case: $case"}
}
if(!$text.Contains('STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN')){
 throw 'Energy icon suite incomplete'
}
if($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR'){throw 'Combo visual acceptance failed'}
foreach($case in @('no_custom_panel_fresh_combat','no_custom_panel_after_plays','no_custom_panel_next_combat',
 'native_card_tip_block','native_card_tip_draw','native_card_tip_record','native_card_tip_unplayable','native_card_tip_paimon',
 'native_card_tip_targeted_attack','native_card_tip_random_attack','native_card_tip_exhausted',
 '100_tooltip_reads_no_state_rng_energy_or_hand_mutation','turn_reset_preserves_status')) {
 if(!$text.Contains("STS2MOD_TEST_PASS VIS_$case")){throw "Missing visual case: $case"}
}
foreach($element in @('None','Wind','Rock','Lightning')) {
 foreach($prefix in @('icon_resource','native_status_icon')) {
  if(!$text.Contains("STS2MOD_TEST_PASS VIS_${prefix}_$element")){throw "Missing element status evidence: $element/$prefix"}
 }
}
if($text -notmatch '(?m)^STS2MOD_VISUAL_COMPLETE assertions=\d+ panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){
 throw 'Visual suite incomplete'
}
foreach($case in @('native_scene_eleven_separate_parts','native_attack_reaches_layered_rig',
 'independent_arm_and_torso_not_whole_image_slide','mixed_card_defense_uses_attack_recovery',
 'pose_returns_without_drift','death_cannot_be_overwritten_by_old_actions','native_revive_restores_pose','lowercase_revive_alias_restores_pose',
 'simultaneous_effects_bounded','effects_free_without_combo_or_rng_changes','native_plain_strike_contact',
 'native_plain_defend_gain','native_mixed_card_both_events','native_fully_blocked_reaction',
 'native_partially_blocked_reaction','native_unblocked_reaction','zero_damage_has_no_hurt_pose',
 'non_attack_hp_loss_has_no_weapon_contact','repeated_hits_restore_pose')) {
 if(!$text.Contains("STS2MOD_TEST_PASS ANIM_$case")){throw "Missing combat presentation case: $case"}
}
foreach($element in @('Wind','Rock','Lightning')) {
 foreach($effect in @('Attack','Defend')) {
  if(!$text.Contains("STS2MOD_TEST_PASS ANIM_six_effect_node_${element}_$effect")){throw "Missing element effect: $element/$effect"}
 }
 foreach($effect in @('attack','defend')) {
  if(!$text.Contains("STS2MOD_TEST_PASS ANIM_native_element_${effect}_$element")){throw "Missing native element event: $element/$effect"}
 }
}
if($text -notmatch '(?m)^STS2MOD_COMBAT_PRESENTATION_COMPLETE assertions=\d+ native_events=PASS cleanup=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$'){
 throw 'Combat presentation suite incomplete'
}
if(!$WithinExpansion){
 if($text -notmatch '(?m)^STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=(\d+) singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False\r?$'){
  throw 'Visual focused suite incomplete'
 }
 if([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ').Count -ne [int]$Matches[1]){throw 'Focused assertion total mismatch'}
}
Write-Output 'COMBO_VISUAL_PASS panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
