param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$Full)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Test-ComboVisualRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -WithinExpansion
if ($Full) {
    & (Join-Path $PSScriptRoot 'Test-TravelerCharacterRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
} else {
    & (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
}
$text = Get-Content -Raw -LiteralPath $LogFile
if ($text -notmatch '(?m)^STS2MOD_BALANCE15_COMPLETE assertions=\d+ multiplayer=NOT_RUN rendering=NOT_RUN\r?$') {
    throw '0.3.15 balance regression incomplete.'
}
foreach ($case in @('finale_new_combat_resets','finale_counts_trigger_without_reward',
    'rock_combo_followup_keeps_normal_strength','rock_native_weak_vulnerable_once')) {
    if (!$text.Contains("STS2MOD_TEST_PASS BAL15_$case")) { throw "Missing balance boundary: $case" }
}
foreach ($up in @('False','True')) {
    foreach ($case in @('finale_combat_count_survives_turn','finale_uncapped_combat_scaling')) {
        if (!$text.Contains("STS2MOD_TEST_PASS BAL15_${case}_$up")) { throw "Missing combat scaling: $case/$up" }
    }
    foreach ($strength in @(-5,0,3)) {
        if (!$text.Contains("STS2MOD_TEST_PASS BAL15_rock_triple_strength_preview_${strength}_$up")) {
            throw "Missing triple Strength: $strength/$up"
        }
    }
}
foreach ($order in @('BB','UU','BU','UB','BUBU')) {
    foreach ($case in @('order_description','order_resets')) {
        if (!$text.Contains("STS2MOD_TEST_PASS BAL15_resonance_${case}_$order")) { throw "Missing Resonance order: $case/$order" }
    }
    foreach ($index in 0..$order.Length) {
        if (!$text.Contains("STS2MOD_TEST_PASS BAL15_resonance_order_${order}_$index")) { throw "Missing Resonance slot: $order/$index" }
    }
}
if ($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]|SCRIPT ERROR') { throw 'Expanded card acceptance failed.' }
if (!$text.Contains('STS2MOD_TEST_PASS ART_card_relic_portraits_unique')) { throw 'Card/relic portraits are not unique.' }
foreach ($id in @('TC_C19','TC_C20','TC_C21','TC_C22','TC_C23','TC_C24','TC_C25','TC_C26','TC_C27','TC_C28',
    'TC_R03','TC_R04','TC_R05','TC_R06','TC_R07')) {
    if (!$text.Contains("STS2MOD_TEST_PASS ART_draft_$id")) { throw "Missing draft texture: $id" }
}
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
foreach ($case in @('pool_28_types_11_12_5','rarity_10_13_5','all_ids_once','boss_native_rare_rewards',
 'retain_keyword_survives_native_flush','plans_empty_hand_returns_without_choice','plans_upgraded_can_skip',
 'plans_single_eligible_retained_card_upgrades','plans_no_upgradable_cards_returns_without_choice',
 'rock_heart_stacks_gain_4_plus_6','rock_heart_regenerates_after_block_spent',
 'power_cards_do_not_advance_and_copies_stack','guidance_two_copies_once_each','guidance_second_combo_no_second_draw',
 'guidance_resets_on_next_turn','guidance_new_copy_can_trigger_after_old_spent',
 'zero_draw_does_not_consume_actual_reward','guidance_after_blocked_draw_still_triggers',
 'paimon_combo2_guidance1_compass0','echo_copies_add_base_reward','echo_boosts_second_rock_lightning',
 'echo_does_not_stack_with_paimon','random_defensive_attack_triggers_echo_and_actual_reward','base_kill_does_not_trigger_echo',
 'zero_attack_damage_does_not_trigger_echo',
 'resonance_base_and_upgrade_share_ordered_slots','resonance_third_reward_does_not_repeat','resonance_second_actual_after_skip_gains_energy','resonance_cap_does_not_repeat',
 'extra_turn_resets_actual_reward_counter','resonance_can_trigger_on_extra_turn',
 'resonance_skipped_reward_is_not_second','resonance_no_energy_after_skipped_reward',
 'final_combo_kill_skips_followup_abilities_and_compass','new_combat_clears_ability_and_combo_state')) {
    if ($text -notmatch ('(?m)^STS2MOD_TEST_PASS EXP_' + [regex]::Escape($case) + '\r?$')) { throw "Missing expansion case: $case" }
}
foreach ($up in @('False','True')) {
    foreach ($case in @('gale_all_enemies','shattering_rock','thunder_mark_vulnerable_before_combo','wind_shelter',
      'patient_rampart_block','charge_thunder_base','charge_thunder_next_turn_energy','charge_thunder_not_repeated',
      'finale_zero_combos','finale_three_combos_without_element_advance','finale_native_preview_matches_modified_damage','plans_native_selection',
      'plans_upgrade_persists_without_retention','rock_heart_no_immediate_block','rock_heart_turn_start_new_block','rock_heart_repeats_without_retention','rock_heart_adds_to_native_retention')) {
        if (!$text.Contains("STS2MOD_TEST_PASS EXP_${case}_$up")) { throw "Missing upgraded/base case: $case/$up" }
    }
    foreach ($id in 7..19) {
        $key = 'TC_C' + $id.ToString('00')
        foreach ($case in @('native_roundtrip','library_description')) {
            if (!$text.Contains("STS2MOD_TEST_PASS EXP_${case}_${key}_$up")) { throw "Missing model check: $key/$up/$case" }
        }
    }
}
foreach ($id in 7..19) {
    if (!$text.Contains('STS2MOD_TEST_PASS EXP_portrait_TC_C' + $id.ToString('00'))) { throw "Missing portrait $id" }
}
if ($text -notmatch '(?m)^STS2MOD_EXPANSION_COMPLETE assertions=\d+ cards=28 powers=5 multiplayer=NOT_RUN rendering=NOT_RUN\r?$') {
    throw 'Expansion test has not completed.'
}
if ($text -notmatch '(?m)^STS2MOD_POOL22_COMPLETE assertions=\d+ cards=9 relics=4 rendering=NOT_RUN multiplayer=NOT_RUN\r?$') {
    throw '2026-10-22 card/relic expansion suite incomplete.'
}
foreach ($case in @('x_requires_element_no_payment','burst_replay_preserves_preplay_reward_snapshot',
 'x_replay_preserves_element_and_payment_snapshot','x_native_chemical_x_bonus',
 'x_native_free_autoplay_captures_without_payment','medicine_no_exhaust_no_proc','medicine_first_status_five_block',
 'medicine_once_per_turn','medicine_native_ethereal_next_turn','statue_initial_record_no_proc',
 'statue_trigger_not_actual_reward','statue_once_per_combat','drum_third_actual_arms',
 'drum_ignores_aoe_random_auto_and_x_skill','drum_only_first_hit_not_combo','drum_expires_at_turn_end',
 'prism_plain_skill_manual_combo_entry','prism_instance_survives_discard_redraw',
 'prism_serialized_mark_copy_does_not_inherit','prism_next_turn_rerolls_same_instance',
 'prism_auto_first_assigns_without_combo','prism_auto_consumes_first_card_slot',
 'tuning_interrupted_choice_restores_hand_input',
 'infusion_empty_records_only','infusion_two_copies_one_roll_two_dexterity',
 'infusion_accumulates_and_removes_only_own_dexterity','infusion_effect_combo_rewards_and_statue',
 'infusion_cap_updates_record_without_reward','infusion_turn_start_after_reset_single_roll')) {
    if (!$text.Contains("STS2MOD_TEST_PASS POOL22_$case")) { throw "Missing expansion boundary: $case" }
}
foreach ($up in @('False','True')) {
    foreach ($case in @('cleanse_exhaust_draw','cleanse_empty','cleanse_optional','rock_cleanse_before_block_reusable',
      'headwind_damage_then_weak','opportunist_layers_native_preview','x_lightning_dead_targets_and_artifact')) {
        if (!$text.Contains("STS2MOD_TEST_PASS POOL22_${case}_$up")) { throw "Missing expansion card case: $case/$up" }
    }
    foreach ($count in @(0,1,2,3,4,7)) {
        if (!$text.Contains("STS2MOD_TEST_PASS POOL22_burst_hits_${count}_$up")) { throw "Missing burst count: $count/$up" }
    }
    foreach ($element in @('Wind','Rock','Lightning')) {
        foreach ($x in @(0,1,3)) {
            if (!$text.Contains("STS2MOD_TEST_PASS POOL22_x_branch_${element}_${x}_$up")) { throw "Missing X branch: $element/$x/$up" }
        }
        if (!$text.Contains("STS2MOD_TEST_PASS POOL22_tuning_choice_free_exhaust_combo_${element}_$up")) {
            throw "Missing tuning choice: $element/$up"
        }
        if (!$text.Contains("STS2MOD_TEST_PASS POOL22_tuning_blocks_hand_input_and_restores_${element}_$up")) {
            throw "Missing tuning hand input lock: $element/$up"
        }
    }
    foreach ($id in 20..28) {
        if (!$text.Contains("STS2MOD_TEST_PASS POOL22_model_locale_roundtrip_TC_C${id}_$up")) { throw "Missing expansion model: $id/$up" }
    }
}
if (!$Full -and $text -notmatch '(?m)^STS2MOD_EXPANSION_FOCUSED_COMPLETE assertions=\d+ cards=28 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN\r?$') {
    throw 'Focused completion marker missing.'
}
foreach ($case in @('chain_native_all_enemy_hits_False','chain_native_all_enemy_hits_True',
 'tailwind_one_draw_exhaust_boundary_False','tailwind_one_draw_exhaust_boundary_True',
 'guidance_wind_rock_draws','guidance_pairs_share_once','echo_wind_rock_six_block','echo_paimon_block_eight_no_stack',
 'echo_doubles_lightning_wind','echo_paimon_draw_two_no_stack',
 'echo_preview_matches_draw','upgraded_resonance_first_refunds_energy','upgraded_resonance_once_per_turn',
 'compass_third_actual_reward_strength','compass_fourth_no_second_strength','compass_fifth_combo_blocked',
 'compass_strength_accumulates_next_turn','new_combat_strength_reset_without_compass')) {
    if (!$text.Contains("STS2MOD_TEST_PASS BAL_$case")) { throw "Missing balance case: $case" }
}

foreach ($up in @('False','True')) {
    foreach ($case in @('all_stackable_debuffs_only','empty_target_still_resolves_one_combo',
      'native_artifact_blocks_each_increase','native_ethereal_upgrade_removal')) {
        if (!$text.Contains("STS2MOD_TEST_PASS BLIGHT_${case}_$up")) { throw "Missing blight case: $case/$up" }
    }
}
Write-Output 'EXPANDED_CARDS_PASS cards=28 powers=5 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
