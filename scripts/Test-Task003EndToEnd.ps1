param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit,
    [Parameter(Mandatory)][ValidateSet('Prepare','Resume')][string]$Phase)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless -NativeShutdown
$text=Get-Content -Raw -LiteralPath $LogFile
$errors=& (Join-Path $PSScriptRoot 'Get-GameLogErrors.ps1') -Text $text -Headless -NativeShutdown
if ($text -match 'STS2MOD_TEST_FAIL|\[ERROR\]' -or $errors.Unexpected.Count) { throw 'TASK-003 end-to-end game test failed.' }
if ($text -notmatch '(?m)^STS2MOD_SANDBOX storage=native_disk cloud=false marker=V1\r?$') { throw 'Native disk sandbox was not installed.' }
if ($text -notmatch '(?m)^\[INFO\] NGame\.Quit called\r?$' -or $text -notmatch '(?m)^\[INFO\] Steamworks shutdown succeeded!\r?$') { throw 'Native quit/shutdown did not complete.' }
$required=if ($Phase -eq 'Prepare') {
    @('TASK003_native_room_reward_enters_deck','TASK003_native_shop_card_purchase_cost_and_deck',
      'TASK003_sold_card_slot_not_stocked','TASK003_native_shop_paimon_purchase',
      'TASK003_sold_paimon_slot_not_stocked','TASK003_obtained_paimon_absent_from_both_bags',
      'TASK003_paimon_not_offered_again_0','TASK003_paimon_not_offered_again_1','TASK003_paimon_not_offered_again_2',
      'TASK003_save_fixture_six_base_and_upgrades','TASK003_native_disk_save_exists','TASK003_leave_unsaved_wind_before_exit')
} else {
    @('TASK003_native_saved_run_read','TASK003_disk_reload_exact_deck_and_upgrades',
      'TASK003_disk_reload_exact_relics_and_gold','TASK003_reload_no_duplicate_relic_or_stale_combo',
      'TASK003_reload_no_duplicate_registration','TASK003_reload_paimon_stays_removed_from_bags',
      'TASK003_first_rock_after_reload_has_no_old_wind_reward','TASK003_paimon_still_doubles_after_reload',
      'TASK003_soak_meaningful_sample')
}
foreach ($case in $required) {
    if ($text -notmatch ('(?m)^STS2MOD_TEST_PASS '+[regex]::Escape($case)+'\r?$')) { throw "Missing passing case: $case" }
}
if ($Phase -eq 'Prepare') {
    if ($text -notmatch '(?m)^STS2MOD_E2E_PREPARED assertions=\d+ cards=6 upgrades=6 paimon=1 shouldSave=True boundary=combat_entry\r?$') { throw 'Prepare phase incomplete.' }
    foreach ($source in @('source=room card=TC_C','source=shop card=TC_C','source=shop relic=TC_R01')) {
        if (!$text.Contains('STS2MOD_E2E_ACQUIRED '+$source)) { throw "Missing acquisition provenance: $source" }
    }
} else {
    foreach ($case in $required[0..5]) {
        if ([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS '+[regex]::Escape($case)+'\r?$').Count -ne 2) { throw "Expected two native reload checks: $case" }
    }
    if ($text -notmatch '(?m)^STS2MOD_E2E_COMPLETE assertions=\d+ acquisition=PASS disk_restart=PASS soak=PASS multiplayer=NOT_RUN ui=NOT_RUN\r?$'
        -or $text -notmatch '(?m)^STS2MOD_SOAK_COMPLETE seed=TASK003A battles=6 turns=\d+ actions=\d+ combos=\d+ natural_wins=\d+ energy_injection=false hand_injection=false bounded_turns=4\r?$') { throw 'Resume/soak phase incomplete.' }
}
$count=[regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ').Count
$marker=if ($Phase -eq 'Prepare') { 'STS2MOD_E2E_PREPARED' } else { 'STS2MOD_E2E_COMPLETE' }
$match=[regex]::Match($text,[regex]::Escape($marker)+' assertions=(\d+)')
if ([int]$match.Groups[1].Value -ne $count) { throw 'Assertion total does not match the completion marker.' }
Write-Output "TASK003_END_TO_END_PASS phase=$Phase assertions=$count"
