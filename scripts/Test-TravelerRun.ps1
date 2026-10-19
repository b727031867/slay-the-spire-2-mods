param(
    [Parameter(Mandatory)][string]$LogFile,
    [Parameter(Mandatory)][string]$CodeCommit,
    [switch]$Acquisition,
    [switch]$Lightning
)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
$content = Get-Content -LiteralPath $LogFile -Raw
if ($Acquisition -and $Lightning) { throw 'Choose either acquisition or Lightning rule evidence.' }
if ($content -match 'STS2MOD_TEST_FAIL|(?m)^ERROR:') { throw 'Traveler game integration test failed. Read the local log.' }
if ($Acquisition) {
    if ($content -notmatch 'STS2MOD_ACQUISITION_COMPLETE shouldSave=false' -or $content -notmatch 'TC21_native_room_reward_selected_into_deck') {
        throw 'The native room reward acquisition test has not completed.'
    }
} else {
    $required = @('TC01_damage6_block11_consumed','TC04_reverse_no_combo','TC05_normal_defend_preserves_sequence','TC06_same_element_then_one_combo','TC08_three_pairs_only_two_bonuses','TC09_wind_does_not_cross_turn','TC10_native_autoplay_preserves_wind','TC10_manual_rock_after_autoplay_triggers','TC11_native_burst_extra_resolution_no_second_advance','TC13_upgraded_damage9_block14','TC15_insufficient_energy_does_not_advance','TC18_new_combat_has_fresh_state')
    foreach ($case in $required) {
        if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing passing case: $case" }
    }
    if ($Lightning) {
        foreach ($case in @('TC002_lightning_once_in_native_pool','TC02_rock_lightning_damage7_plus4','TC13_lightning_upgrade10_plus4','TC08_mixed_combos_share_cap','TC10_auto_lightning_preserves_rock','TC10_manual_lightning_after_auto_triggers','TC11_native_duplicated_lightning_one_bonus','TC24_strength_weak_vulnerable_block_native_formula','TC14_dead_target_no_retarget_counter_consumed','native_card_serialization_roundtrip_TC_C03')) {
            if (!$content.Contains("STS2MOD_TEST_PASS $case")) { throw "Missing Lightning case: $case" }
        }
        if (!$content.Contains('STS2MOD_LIGHTNING_COMPLETE')) { throw 'Lightning tests have not completed.' }
    }
    if ($content -notmatch 'STS2MOD_TEST_COMPLETE assertions=\d+ multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave=False') {
        throw 'The rule test has not completed.'
    }
}
Write-Output 'Traveler singleplayer test evidence passed. UI and multiplayer require their own recorded checks.'
