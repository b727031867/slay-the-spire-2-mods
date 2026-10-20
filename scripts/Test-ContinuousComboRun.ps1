param([Parameter(Mandatory)][string]$LogFile,[Parameter(Mandatory)][string]$CodeCommit)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Test-ExperienceRun.ps1') -LogFile $LogFile -CodeCommit $CodeCommit
$text=Get-Content -Raw -LiteralPath $LogFile
foreach($suffix in @('normal','paimon')) {
    foreach($name in @('after_pair_retains_rock_and_previews_next_block','three_energy_wind_rock_wind_two_blocks','three_energy_wind_rock_lightning_block_and_attack','wind_lightning_wind_two_draw_rewards','four_cards_reach_shared_three_cap','fifth_card_no_fourth_bonus','random_defensive_reward_still_retains_lightning','can_continue_after_targetless_pair','retains_element_at_turn_boundary')) {
        $case="CHAIN_${name}_$suffix"
        if($text -notmatch ('(?m)^STS2MOD_TEST_PASS '+[regex]::Escape($case)+'\r?$')) { throw "Missing continuous combo case: $case" }
    }
}
Write-Output 'CONTINUOUS_COMBO_PASS cases=18 rules=overlapping_pairs cap=3 singleplayer=PASS rendering=NOT_RUN multiplayer=NOT_RUN'
