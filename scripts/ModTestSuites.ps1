# Shared suite catalog. Keep native flags and strict legacy validators behind this interface.
function Get-ModTestSuite {
    param([Parameter(Mandatory)][ValidateSet('Load','Visuals','Cards','Character','Events','Full','SavePrepare','SaveResume')][string]$Suite)
    $catalog = @{
        Load=@{ Flag=$null; Marker='\[Startup\] Time to main menu:' }
        Visuals=@{ Flag='TravelerVisuals'; Marker='(?m)^STS2MOD_VISUAL_FOCUSED_COMPLETE ' }
        Cards=@{ Flag='TravelerCards'; Marker='(?m)^STS2MOD_EXPANSION_FOCUSED_COMPLETE ' }
        Character=@{ Flag='TravelerCharacter'; Marker='(?m)^STS2MOD_CHARACTER_COMPLETE ' }
        Events=@{ Flag='TravelerEvents'; Marker='(?m)^STS2MOD_EVENTS_COMPLETE ' }
        Full=@{ Flag='TravelerTest'; Marker='(?m)^STS2MOD_TEST_COMPLETE ' }
        SavePrepare=@{ Flag='TravelerEndToEnd'; Marker='(?m)^\[INFO\] Steamworks shutdown succeeded!' }
        SaveResume=@{ Flag='ResumeTask003'; Marker='(?m)^\[INFO\] Steamworks shutdown succeeded!' }
    }
    [pscustomobject]$catalog[$Suite]
}
