$ErrorActionPreference = 'Stop'
$classifier = Join-Path (Split-Path $PSScriptRoot -Parent) 'scripts/Get-GameLogErrors.ps1'
$cases = @(
    @{ Name='native dummy shader in headless'; Headless=$true; Text="ERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=0; Known=1 },
    @{ Name='same native message without headless'; Headless=$false; Text="ERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=1; Known=0 },
    @{ Name='same null message from gameplay'; Headless=$true; Text="ERROR: Parameter `"mem`" is null.`n   at: ResolveCombo (src/Sts2Mod/Combo/ComboPower.cs:1)"; Unexpected=1; Known=0 },
    @{ Name='missing source never ignored'; Headless=$true; Text='ERROR: Parameter "shader" is null.'; Unexpected=1; Known=0 },
    @{ Name='observed native RID context'; Headless=$true; Text="ERROR: Initializing already initialized RID`n   at: get_or_null (./core/templates/rid_owner.h:246)"; Unexpected=0; Known=1 },
    @{ Name='unknown error remains failure'; Headless=$true; Text="ERROR: Card resolution failed`n   at: OnPlay (card.cs:1)"; Unexpected=1; Known=0 }
    @{ Name='native dummy texture in headless'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: texture_2d_initialize (servers/rendering/dummy/storage/texture_storage.h:84)"; Unexpected=0; Known=1 }
    @{ Name='same texture null from gameplay rejected'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: Resolve (card.cs:84)"; Unexpected=1; Known=0 }
    @{ Name='adjacent native texture cleanup cascade'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: texture_2d_initialize (servers/rendering/dummy/storage/texture_storage.h:84)`nERROR: Parameter `"texture`" is null.`n   at: texture_free (servers/rendering/dummy/storage/texture_storage.h:77)"; Unexpected=0; Known=2 }
    @{ Name='texture cleanup cascade rejected without headless'; Headless=$false; Text="ERROR: Parameter `"t`" is null.`n   at: texture_2d_initialize (servers/rendering/dummy/storage/texture_storage.h:84)`nERROR: Parameter `"texture`" is null.`n   at: texture_free (servers/rendering/dummy/storage/texture_storage.h:77)"; Unexpected=2; Known=0 }
    @{ Name='orphan native texture cleanup remains unexpected'; Headless=$true; Text="ERROR: Parameter `"texture`" is null.`n   at: texture_free (servers/rendering/dummy/storage/texture_storage.h:77)"; Unexpected=1; Known=0 }
    @{ Name='gameplay texture cleanup rejected beside native initialize'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: texture_2d_initialize (servers/rendering/dummy/storage/texture_storage.h:84)`nERROR: Parameter `"texture`" is null.`n   at: ReleasePortrait (src/Sts2Mod/Character.cs:77)"; Unexpected=1; Known=1 }
    @{ Name='native cleanup rejected beside gameplay initialize'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: Resolve (card.cs:84)`nERROR: Parameter `"texture`" is null.`n   at: texture_free (servers/rendering/dummy/storage/texture_storage.h:77)"; Unexpected=2; Known=0 }
    @{ Name='nonadjacent native texture cleanup remains unexpected'; Headless=$true; Text="ERROR: Parameter `"t`" is null.`n   at: texture_2d_initialize (servers/rendering/dummy/storage/texture_storage.h:84)`n[INFO] unrelated event`nERROR: Parameter `"texture`" is null.`n   at: texture_free (servers/rendering/dummy/storage/texture_storage.h:77)"; Unexpected=1; Known=1 }
    @{ Name='wrong RID requires adjacent dummy cascade'; Headless=$true; Text="ERROR: Attempting to initialize the wrong RID`n   at: get_or_null (./core/templates/rid_owner.h:250)"; Unexpected=1; Known=0 }
    @{ Name='wrong RID dummy cascade matches real engine function'; Headless=$true; Text="ERROR: Attempting to initialize the wrong RID`n   at: get_or_null (./core/templates/rid_owner.h:250)`nERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=0; Known=2 }
    @{ Name='wrong RID cascade not ignored in graphical mode'; Headless=$false; Text="ERROR: Attempting to initialize the wrong RID`n   at: get_or_null (./core/templates/rid_owner.h:250)`nERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=2; Known=0 }
    @{ Name='wrong RID gameplay source rejected even beside dummy shader'; Headless=$true; Text="ERROR: Attempting to initialize the wrong RID`n   at: Resolve (card.cs:250)`nERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=1; Known=1 }
    @{ Name='wrong RID dummy material cascade matches real engine functions'; Headless=$true; Text="ERROR: Attempting to initialize the wrong RID`n   at: get_or_null (./core/templates/rid_owner.h:250)`nERROR: Parameter `"material`" is null.`n   at: material_set_shader (servers/rendering/dummy/storage/material_storage.cpp:251)"; Unexpected=0; Known=2 }
    @{ Name='dummy material message rejected without headless'; Headless=$false; Text="ERROR: Parameter `"material`" is null.`n   at: material_set_shader (servers/rendering/dummy/storage/material_storage.cpp:251)"; Unexpected=1; Known=0 }
    @{ Name='same material null from gameplay rejected'; Headless=$true; Text="ERROR: Parameter `"material`" is null.`n   at: SetCardMaterial (card.cs:251)"; Unexpected=1; Known=0 }
)
foreach ($case in $cases) {
    $result = & $classifier -Text $case.Text -Headless:$case.Headless
    if ($result.Unexpected.Count -ne $case.Unexpected -or $result.KnownHeadless.Count -ne $case.Known) {
        throw "Log classification failed: $($case.Name)"
    }
    Write-Output "PASS $($case.Name)"
}
Write-Output "LOG_CLASSIFICATION_PASS assertions=$($cases.Count)"
$shutdownPrefix="[INFO] NGame.Quit called`n[INFO] Steamworks shutdown succeeded!`n"
$shutdownLine="ERROR: 27 RID allocations of type 'PN13RendererDummy14TextureStorage12DummyTextureE' were leaked at exit."
$shutdownCases=@(
    @{Name='verified native shutdown type'; Text=$shutdownPrefix+$shutdownLine; Flag=$true; Unexpected=0; Shutdown=1},
    @{Name='same type during gameplay rejected'; Text=$shutdownLine; Flag=$true; Unexpected=1; Shutdown=0},
    @{Name='shutdown requires explicit flag'; Text=$shutdownPrefix+$shutdownLine; Flag=$false; Unexpected=1; Shutdown=0},
    @{Name='unknown type after shutdown rejected'; Text=$shutdownPrefix+"ERROR: 1 RID allocations of type 'CustomGameplayObject' were leaked at exit."; Flag=$true; Unexpected=1; Shutdown=0},
    @{Name='unknown error after shutdown rejected'; Text=$shutdownPrefix+'ERROR: Save failed'; Flag=$true; Unexpected=1; Shutdown=0}
)
foreach ($case in $shutdownCases) {
    $result=& $classifier -Text $case.Text -Headless -NativeShutdown:$case.Flag
    if ($result.Unexpected.Count -ne $case.Unexpected -or $result.KnownNativeShutdown.Count -ne $case.Shutdown) { throw "Shutdown classification failed: $($case.Name)" }
    Write-Output "PASS $($case.Name)"
}
Write-Output "LOG_CLASSIFICATION_TOTAL_PASS assertions=$($cases.Count+$shutdownCases.Count)"
