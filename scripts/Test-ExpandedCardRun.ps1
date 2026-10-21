param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$Full)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Test-ComboVisualRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -WithinExpansion
if ($Full) {
    & (Join-Path $PSScriptRoot 'Test-TravelerCharacterRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
} else {
    & (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
}
$text = Get-Content -Raw -LiteralPath $LogFile
if ($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR') { throw 'Expanded card acceptance failed.' }
if (!$text.Contains('STS2MOD_TEST_PASS ART_topbar_uses_face_closeup_scene')) { throw 'Native top-bar portrait is not wired to the face close-up.' }
foreach ($asset in @('wind_guidance','rock_heart','thunder_echo','element_resonance','gale_sweep','shattering_rock',
 'thunder_mark','triphase_finale','wind_shelter','patient_rampart','charge_thunder','journey_plans',
 'strike','defend','compass','combo','body','icon','select_background','head_icon')) {
    if (!$text.Contains("STS2MOD_TEST_PASS ART_final_png_$asset")) { throw "Missing final art: $asset" }
}
foreach ($up in @('False','True')) {
    foreach ($id in @('TC_C02','TC_C16','TC_C06','TC_C17')) {
        if (!$text.Contains("STS2MOD_TEST_PASS RANDOM_seeded_single_living_target_${id}_$up")) { throw "Missing defensive random target: $id/$up" }
    }
}
foreach ($case in @('only_living_enemy_selected','shared_cap_no_fourth_hit_or_rng','zero_damage_no_echo_or_actual_reward')) {
    if (!$text.Contains("STS2MOD_TEST_PASS RANDOM_$case")) { throw "Missing random boundary: $case" }
}
if (!$text.Contains('STS2MOD_TEST_PASS ART_character_layered_battle_scale_312px') -or
    $text -notmatch '(?m)^STS2MOD_RANDOM_COMBO_COMPLETE cases=11 seeded_rng=native cap=3 rendering=NOT_RUN multiplayer=NOT_RUN\r?$') {
    throw 'Final art/random-combo suite incomplete.'
}
foreach ($case in @('pool_18_types_7_7_4','rarity_8_7_3','all_ids_once','boss_native_rare_rewards',
 'retain_keyword_survives_native_flush','plans_empty_hand_returns_without_choice','plans_upgraded_can_skip',
 'rock_heart_stacks_caps_4_plus_6','rock_heart_does_not_create_missing_block','rock_heart_does_not_restore_enemy_consumed_block',
 'power_cards_do_not_advance_and_copies_stack','guidance_two_copies_once_each','guidance_second_combo_no_second_draw',
 'guidance_resets_on_next_turn','guidance_new_copy_can_trigger_after_old_spent',
 'zero_draw_does_not_consume_actual_reward','guidance_after_blocked_draw_still_triggers',
 'paimon_combo2_guidance1_compass1','echo_stacks_3_plus_5_and_hits_all','echo_repeats_on_second_rock_lightning',
 'echo_unaffected_by_paimon_or_strength','random_defensive_attack_triggers_echo_and_actual_reward','base_kill_does_not_trigger_echo',
 'zero_attack_damage_does_not_trigger_echo',
 'resonance_second_reward_grants_one_per_copy','resonance_third_reward_does_not_repeat','resonance_second_actual_after_skip_gains_energy','resonance_cap_does_not_repeat',
 'extra_turn_resets_actual_reward_counter','resonance_can_trigger_on_extra_turn',
 'resonance_skipped_reward_is_not_second','resonance_no_energy_after_skipped_reward',
 'final_combo_kill_skips_followup_abilities_and_compass','new_combat_clears_ability_and_combo_state')) {
    if ($text -notmatch ('(?m)^STS2MOD_TEST_PASS EXP_' + [regex]::Escape($case) + '\r?$')) { throw "Missing expansion case: $case" }
}
foreach ($up in @('False','True')) {
    foreach ($case in @('gale_all_enemies','shattering_rock','thunder_mark_vulnerable_before_combo','wind_shelter',
      'patient_rampart_block','charge_thunder_base','charge_thunder_next_turn_energy','charge_thunder_not_repeated',
      'finale_zero_combos','finale_three_combos_without_element_advance','finale_native_preview_matches_modified_damage','plans_native_selection',
      'plans_native_retention','rock_heart_retains_without_regaining','rock_heart_native_retention_wins')) {
        if (!$text.Contains("STS2MOD_TEST_PASS EXP_${case}_$up")) { throw "Missing upgraded/base case: $case/$up" }
    }
    foreach ($id in 7..18) {
        $key = 'TC_C' + $id.ToString('00')
        foreach ($case in @('native_roundtrip','library_description')) {
            if (!$text.Contains("STS2MOD_TEST_PASS EXP_${case}_${key}_$up")) { throw "Missing model check: $key/$up/$case" }
        }
    }
}
foreach ($id in 7..18) {
    if (!$text.Contains('STS2MOD_TEST_PASS EXP_portrait_TC_C' + $id.ToString('00'))) { throw "Missing portrait $id" }
}
if ($text -notmatch '(?m)^STS2MOD_EXPANSION_COMPLETE assertions=\d+ cards=18 powers=4 multiplayer=NOT_RUN rendering=NOT_RUN\r?$') {
    throw 'Expansion test has not completed.'
}
if (!$Full -and $text -notmatch '(?m)^STS2MOD_EXPANSION_FOCUSED_COMPLETE assertions=\d+ cards=18 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$') {
    throw 'Focused completion marker missing.'
}
Write-Output 'EXPANDED_CARDS_PASS cards=18 powers=4 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
