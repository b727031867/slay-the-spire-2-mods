$ErrorActionPreference = 'Stop'
$classifier = Join-Path (Split-Path $PSScriptRoot -Parent) 'scripts/Get-GameLogErrors.ps1'
$cases = @(
    @{ Name='native dummy shader in headless'; Headless=$true; Text="ERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=0; Known=1 },
    @{ Name='same native message without headless'; Headless=$false; Text="ERROR: Parameter `"shader`" is null.`n   at: shader_set_code (servers/rendering/dummy/storage/material_storage.cpp:163)"; Unexpected=1; Known=0 },
    @{ Name='same null message from gameplay'; Headless=$true; Text="ERROR: Parameter `"mem`" is null.`n   at: ResolveCombo (src/Sts2Mod/Combo/ComboPower.cs:1)"; Unexpected=1; Known=0 },
    @{ Name='missing source never ignored'; Headless=$true; Text='ERROR: Parameter "shader" is null.'; Unexpected=1; Known=0 },
    @{ Name='observed native RID context'; Headless=$true; Text="ERROR: Initializing already initialized RID`n   at: get_or_null (./core/templates/rid_owner.h:246)"; Unexpected=0; Known=1 },
    @{ Name='unknown error remains failure'; Headless=$true; Text="ERROR: Card resolution failed`n   at: OnPlay (card.cs:1)"; Unexpected=1; Known=0 }
)
foreach ($case in $cases) {
    $result = & $classifier -Text $case.Text -Headless:$case.Headless
    if ($result.Unexpected.Count -ne $case.Unexpected -or $result.KnownHeadless.Count -ne $case.Known) {
        throw "Log classification failed: $($case.Name)"
    }
    Write-Output "PASS $($case.Name)"
}
Write-Output "LOG_CLASSIFICATION_PASS assertions=$($cases.Count)"
