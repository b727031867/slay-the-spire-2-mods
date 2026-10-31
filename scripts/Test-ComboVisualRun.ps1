param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$WithinExpansion)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
foreach($language in @('eng','zhs')) {
 if(!$text.Contains("STS2MOD_TEST_PASS GOOPY_block_format_$language")){throw "Missing Goopy block format: $language"}
}
foreach($upgraded in @('False','True')) {
 foreach($case in @('native_enchantment','serialized_growth')) {
  if(!$text.Contains("STS2MOD_TEST_PASS GOOPY_${case}_$upgraded")){throw "Missing Goopy case: $case/$upgraded"}
 }
 foreach($played in 0..2) {
  foreach($case in @('preview','actual_block_and_growth')) {
   if(!$text.Contains("STS2MOD_TEST_PASS GOOPY_${case}_${upgraded}_$played")){throw "Missing Goopy play: $case/$upgraded/$played"}
  }
 }
}
if(!$text.Contains('STS2MOD_GOOPY_COMPLETE preview=PASS actual_block=PASS growth=PASS rendering=NOT_RUN')){
 throw 'Goopy block preview suite incomplete'
}
foreach($id in 19..28) {
 if(!$text.Contains("STS2MOD_TEST_PASS PORTRAIT_fills_native_frame_TC_C$id")){throw "Missing portrait framing check: $id"}
}
if(!$text.Contains('STS2MOD_PORTRAIT_COMPLETE cards=10 ratio=25:19 source=unchanged rendering=NOT_RUN')){
 throw 'Portrait framing suite incomplete'
}
foreach($language in @('eng','zhs')) {
 if(!$text.Contains("STS2MOD_TEST_PASS PREVIEW_damage_format_$language")){throw "Missing damage format check: $language"}
}
foreach($id in @('TC_C01','TC_C03','TC_C05','TC_C12','TC_C13','TC_C14','TC_C22','TC_C23','TC_C26','TC_STRIKE')) {
 foreach($upgraded in @('False','True')) {
  foreach($step in @('target_damage','switch_back','clear_target')) {
   if(!$text.Contains("STS2MOD_TEST_PASS PREVIEW_${step}_${id}_$upgraded")){throw "Missing damage target preview: $id/$upgraded/$step"}
  }
 }
}
if(!$text.Contains('STS2MOD_TEST_PASS PREVIEW_readonly_state_rng_energy_hp') -or
 !$text.Contains('STS2MOD_DAMAGE_PREVIEW_COMPLETE formats=eng,zhs targeting=PASS readonly=PASS rendering=NOT_RUN')){
 throw 'Damage preview suite incomplete'
}
foreach($case in @('pool_TravelerCardPool','pool_TravelerRelicPool','pool_TravelerPotionPool',
 'pool_TravelerEventCardPool','pool_TravelerEventRelicPool',
 'custom_counter_native_scene','counter_zero_darkens','counter_gain_restores','counter_rotates_about_center',
 'native_inline_glyph_bounded','acquired_native_relic_inline','unowned_native_relic_inline','explicit_native_prefix_preserved',
 'other_native_relic_inline','owned_potion_inline','card_inline','native_hover_icon_bounded','native_hover_layout_bounded')) {
 if(!$text.Contains("STS2MOD_TEST_PASS ENERGY_$case")){throw "Missing energy icon case: $case"}
}
if(!$text.Contains('STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN')){
 throw 'Energy icon suite incomplete'
}
foreach($element in @('Wind','Rock','Lightning')) {
 foreach($case in @('mapping','resource')) {
  if(!$text.Contains("STS2MOD_TEST_PASS AUDIO_${case}_$element")){throw "Missing element audio case: $case/$element"}
 }
}
foreach($case in @('none_silent','resource_combo','volume_mute','headless_silent')) {
 if(!$text.Contains("STS2MOD_TEST_PASS AUDIO_$case")){throw "Missing audio case: $case"}
}
if(!$text.Contains('STS2MOD_ELEMENT_AUDIO_COMPLETE resources=PASS headless=SILENT listening=NOT_RUN')) {
 throw 'Element audio suite incomplete'
}
if($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR'){throw 'Combo visual acceptance failed'}
foreach($case in @('no_custom_panel_fresh_combat','no_custom_panel_after_plays','no_custom_panel_next_combat',
 'native_card_tip_block','native_card_tip_draw','native_card_tip_record','native_card_tip_unplayable','native_card_tip_paimon',
 'native_card_tip_targeted_attack','native_card_tip_random_attack','native_card_tip_exhausted',
 '100_tooltip_reads_no_state_rng_energy_or_hand_mutation','turn_reset_preserves_status',
 'selection_card_has_no_combo_overlay','hand_has_one_short_preview_no_rulebook')) {
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
foreach($direction in @('left','right')) {
 foreach($case in @('native_facing') + @('Attack','Defend','Hit','Dead','Revive' | ForEach-Object { "facing_survives_$_" })) {
  if(!$text.Contains("STS2MOD_TEST_PASS ANIM_surrounded_${case}_$direction")){throw "Missing surrounded facing case: $case/$direction"}
 }
}
if(!$text.Contains('STS2MOD_TEST_PASS ANIM_surrounded_native_body_is_rig')){throw 'Missing surrounded native body check'}
foreach($case in @('native_scene_single_mother_connected_skin','native_attack_reaches_layered_rig',
 'independent_arm_and_torso_not_whole_image_slide','mixed_card_defense_uses_attack_recovery',
 'pose_returns_without_drift','death_cannot_be_overwritten_by_old_actions','native_revive_restores_pose','lowercase_revive_alias_restores_pose',
 'simultaneous_effects_bounded','effects_free_without_combo_or_rng_changes','native_plain_strike_contact',
 'native_plain_defend_gain','native_mixed_card_both_events','native_fully_blocked_reaction',
 'native_partially_blocked_reaction','native_unblocked_reaction','zero_damage_has_no_hurt_pose',
 'non_attack_hp_loss_has_no_weapon_contact','repeated_hits_restore_pose',
 'bind_pose_reconstructs_original_mother_without_rescaling_parts','source_triangles_drawn_once_no_duplicate_shoulders',
 'guard_bends_knees_with_planted_feet','interrupt_blends_from_current_pose_no_teleport','contact_does_not_jump_animation_clock',
 'hand_and_sword_keep_rigid_shape_during_swing','guard_two_palms_share_hilt_blade_upright_in_front',
 'side_on_forward_thrust_has_horizontal_blade','thrust_to_guard_switch_keeps_current_pose',
 'guard_to_thrust_reentry_has_no_idle_reset','hurt_side_on_leans_back_with_supported_feet')) {
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
foreach($case in @('combo_burst_reuses_single_node','combo_small_fixed_bounds_short_lifetime',
 'combo_has_no_images_or_particles','combo_cleanup_no_rules_or_rng_changes',
 'relic_flash_event_preserved_one_icon_pulse','relic_pulse_no_layout_scale_texture_or_input_changes',
 'relic_no_creature_portrait_flash','native_portrait_suppressed_TC_R01','native_portrait_suppressed_TC_R02',
 'native_portrait_suppressed_TC_R03','other_relic_factory_unchanged','relic_cleanup_no_rules_or_rng_changes',
 'zero_draw_no_bubble_or_relic_effect','other_power_unchanged',
 'resonance_icon_pulse_TC_ELEMENT_RESONANCE','resonance_no_large_art_TC_ELEMENT_RESONANCE',
 'resonance_cleanup_no_gameplay_mutation_TC_ELEMENT_RESONANCE',
 'resonance_icon_pulse_TC_FIRST_ELEMENT_RESONANCE','resonance_no_large_art_TC_FIRST_ELEMENT_RESONANCE',
 'resonance_cleanup_no_gameplay_mutation_TC_FIRST_ELEMENT_RESONANCE')) {
 if(!$text.Contains("STS2MOD_TEST_PASS QUIET_$case")){throw "Missing compact trigger case: $case"}
}
if(!$text.Contains('STS2MOD_COMPACT_TRIGGERS_COMPLETE compact=PASS cleanup=PASS scope=custom_only rendering=NOT_RUN')){
 throw 'Compact trigger suite incomplete'
}
if(!$WithinExpansion){
 if($text -notmatch '(?m)^STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=(\d+) singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False\r?$'){
  throw 'Visual focused suite incomplete'
 }
 if([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ').Count -ne [int]$Matches[1]){throw 'Focused assertion total mismatch'}
}
Write-Output 'COMBO_VISUAL_PASS panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
