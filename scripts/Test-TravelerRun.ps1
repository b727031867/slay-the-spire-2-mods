param(
    [Parameter(Mandatory)][string]$LogFile,
    [Parameter(Mandatory)][string]$CodeCommit,
    [switch]$Acquisition,
    [switch]$Lightning,
    [switch]$Task002,
    [switch]$Task003,
    [switch]$Headless
)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless:$Headless
$content = Get-Content -LiteralPath $LogFile -Raw
if ($Acquisition -and ($Lightning -or $Task002 -or $Task003)) { throw 'Choose either acquisition or rule evidence.' }
$errors = & (Join-Path $PSScriptRoot 'Get-GameLogErrors.ps1') -Text $content -Headless:$Headless
if ($content -match 'STS2MOD_TEST_FAIL|\[ERROR\]' -or $errors.Unexpected.Count) { throw 'Traveler game integration test failed. Read the local log.' }
if ($Acquisition) {
    if ($content -notmatch 'STS2MOD_ACQUISITION_COMPLETE shouldSave=false' -or $content -notmatch 'TC21_native_room_reward_selected_into_deck') {
        throw 'The native room reward acquisition test has not completed.'
    }
} else {
    $required = @('TC01_damage6_block11_retains_rock','TC04_reverse_wind_rock_combo','TC05_normal_defend_preserves_sequence','TC06_same_element_then_one_combo','TC08_four_pairs_only_three_bonuses','TC09_wind_crosses_turn_and_triggers_rock','TC10_native_autoplay_preserves_wind','TC10_manual_rock_after_autoplay_triggers','TC11_native_burst_extra_resolution_no_second_advance','TC13_upgraded_damage9_block14','TC15_insufficient_energy_does_not_advance','TC18_new_combat_has_fresh_state')
    foreach ($case in $required) {
        if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing passing case: $case" }
    }
    if ($Lightning -or $Task002 -or $Task003) {
        foreach ($case in @('TC007_lightning_once_in_traveler_pool','TC02_rock_lightning_damage7_plus4','TC13_lightning_upgrade10_plus4','TC08_mixed_combos_share_three_cap','TC10_auto_lightning_preserves_rock','TC10_manual_lightning_after_auto_triggers','TC11_native_duplicated_lightning_one_bonus','TC24_strength_weak_vulnerable_block_native_formula','TC14_dead_target_no_retarget_counter_consumed','native_card_serialization_roundtrip_TC_C03')) {
            if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing Lightning case: $case" }
        }
        if (!$content.Contains('STS2MOD_LIGHTNING_COMPLETE')) { throw 'Lightning tests have not completed.' }
    }
    if ($Task002 -or $Task003) {
        foreach ($case in @(
            'TASK007_six_cards_once_in_traveler_pool','TASK002_theme_cards_absent_from_silent_pool','TASK007_ten_card_starter_four_elements',
            'TASK007_paimon_once_in_exclusive_uncommon_pool','TASK002_paimon_nonstackable_and_absent_from_ironclad_pool',
            'TC23_tailwind_draw1_exhaust_wind','TC13_tailwind_upgrade_draw1_reusable','TC03_lightning_wind_base_draw1_bonus_draw1',
            'TC11_burst_tailwind_two_base_resolutions_one_combo_draw','TC13_rock_advance_damage5_then_block3',
            'TC13_rock_advance_upgrade_damage7_block5','TC13_thunder_prep_block5_then_draw1',
            'TC13_thunder_prep_upgrade_block8_draw1','rock_to_defensive_thunder_attacks_random_enemy',
            'new_cards_lightning_wind_draw_order','reverse_wind_lightning_draws_bonus','reverse_lightning_rock_attacks_same_target',
            'TC12_paimon_doubles_wind_rock_block',
            'TC12_paimon_doubles_rock_lightning_attack','TC12_paimon_doubles_lightning_wind_draw',
            'paimon_doubles_every_successful_combo_without_duplicate_copy','native_relic_serialization_roundtrip_TC_R01',
            'native_card_serialization_roundtrip_TC_C04','native_card_serialization_roundtrip_TC_C05','native_card_serialization_roundtrip_TC_C06')) {
            if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing TASK-002 case: $case" }
        }
        if ($content -notmatch 'STS2MOD_TASK002_COMPLETE assertions=\d+ cards=6 relics=1 multiplayer=NOT_RUN acquisition=POOL_VERIFIED') {
            throw 'Full TASK-002 tests have not completed.'
        }
    }
    if ($Task003) {
        foreach ($case in @(
            'TASK003_auto_tailwind_draws_base_only_and_preserves_lightning',
            'TASK003_manual_tailwind_after_auto_triggers_paimon_double_draw','TASK003_paimon_doubles_before_extra_turn',
            'TASK003_native_extra_turn_resets_combo_and_paimon','TASK003_paimon_doubles_on_extra_turn',
            'TASK003_dead_rock_lightning_target_does_not_retarget_or_create_bonus',
            'TASK003_base_kill_ending_combat_skips_combo_reward')) {
            if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing TASK-003 case: $case" }
        }
        if ($content -notmatch 'STS2MOD_TASK003_AUTOMATION_COMPLETE assertions=\d+ singleplayer=PASS multiplayer=NOT_RUN ui=READY_FOR_USER_PLAYTEST') {
            throw 'TASK-003 automated acceptance has not completed.'
        }
        $finalKill = [regex]::Match($content, '(?m)^STS2MOD_TEST_DETAIL case=final_base_kill player=(\d+) action=(\d+) block_before=7 block_peak=7 block_after=\d+\r?$')
        if (!$finalKill.Success) { throw 'Missing final-kill block observation before native cleanup.' }
        $skip = '(?m)^STS2MOD_REWARD player=' + [regex]::Escape($finalKill.Groups[1].Value) + ' action=' + [regex]::Escape($finalKill.Groups[2].Value) + ' skipped=combat_ended_or_dead\r?$'
        if ($content -notmatch $skip) { throw 'The final killing action did not explicitly skip its combo reward.' }
    }
    if ($content -notmatch 'STS2MOD_TEST_COMPLETE assertions=\d+ multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave=False') {
        throw 'The rule test has not completed.'
    }
}
Write-Output 'Traveler singleplayer test evidence passed. UI and multiplayer require their own recorded checks.'
