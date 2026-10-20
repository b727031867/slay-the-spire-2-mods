param([Parameter(Mandatory)][string]$LogFile, [Parameter(Mandatory)][string]$CodeCommit, [switch]$WithinExpansion)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ModLoad.ps1') -LogFile $LogFile -CodeCommit $CodeCommit -Headless
$text=Get-Content -Raw -LiteralPath $LogFile
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
if(!$WithinExpansion){
 if($text -notmatch '(?m)^STS2MOD_VISUAL_FOCUSED_COMPLETE assertions=(\d+) singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN shouldSave=False\r?$'){
  throw 'Visual focused suite incomplete'
 }
 if([regex]::Matches($text,'(?m)^STS2MOD_TEST_PASS ').Count -ne [int]$Matches[1]){throw 'Focused assertion total mismatch'}
}
Write-Output 'COMBO_VISUAL_PASS panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
