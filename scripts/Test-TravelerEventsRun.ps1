param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text = Get-Content -Raw -LiteralPath $LogFile
if ($text -match 'STS2MOD_TEST_FAIL|SCRIPT ERROR') { throw 'Traveler event tests failed.' }
if ($text -notmatch '(?m)^STS2MOD_EVENTS_COMPLETE assertions=(\d+) logic=PASS rendering=NOT_RUN multiplayer=NOT_RUN' -or [int]$Matches[1] -lt 90) {
    throw 'Event completion evidence missing or incomplete.'
}
foreach ($case in @('act_range_0','act_range_1','act_range_2','low_hp_entry_locked','depth1_exact_once','leave_keeps_rewards',
    'depth2_cost_card','depth3_curse_added','supplies_cumulative','rare_branch','survey_first_free','survey_second_cost60',
    'survey_second_insufficient_gold','permanent_element_serialization_clone','deck_element_enters_combat',
    'contract_decline','letter_native_start_cost2','address_elemental_temporary_ethereal_free',
    'address_cost_resets_later_turn_draw_normal','letter_heal3','letter_once_per_combat','spool_once_per_turn','nail_once_per_turn',
    'watch_compass_limit5','watch_without_compass_limit4','watch_not_accumulating','mail_reward_restore_no_growth',
    'mail_insufficient_gold_no_card','mail_pay15_take_one','mail_no_skip_after_claim','mail_skip10_once',
    'other_character_excluded','mail_claim_unsubscribes_relic_listener','mail_grid_skip_grants10','mail_grid_skip_unsubscribes_relic_listener')) {
    if (!$text.Contains("STS2MOD_TEST_PASS EVENT_$case")) { throw "Missing event case: $case" }
}
foreach ($id in @('TC_E01','TC_E02','TC_E03','TC_E04')) {
    if (!$text.Contains("STS2MOD_TEST_PASS EVENT_native_room_finish_$id")) { throw "Missing native event room: $id" }
}
Write-Output 'PASS: Traveler events, native rooms, permanent elements and contract rewards.'
