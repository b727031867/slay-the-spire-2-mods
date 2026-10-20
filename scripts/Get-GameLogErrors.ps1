param([Parameter(Mandatory)][AllowEmptyString()][string]$Text, [switch]$Headless, [switch]$NativeShutdown)
$ErrorActionPreference = 'Stop'
# Match the native source context as well as the message. A generic null/RID
# error from gameplay or any other subsystem must never become a passing test.
$nativeHeadlessSources = @{
    # Godot 4.5.1 emits this from get_or_null(), called by initialize_rid().
    # Additionally require its adjacent dummy-renderer cascade below.
    'ERROR: Attempting to initialize the wrong RID' = '^at: get_or_null \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Initializing already initialized RID' = '^at: get_or_null \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Attempting to use an uninitialized RID' = '^at: get_or_null \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Parameter "mem" is null.' = '^at: initialize_rid \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Parameter "shader" is null.' = '^at: shader_set_code \(.*servers/rendering/dummy/storage/material_storage\.cpp:\d+\)$'
    'ERROR: Parameter "material" is null.' = '^at: material_set_shader \(.*servers/rendering/dummy/storage/material_storage\.cpp:\d+\)$'
    'ERROR: Parameter "t" is null.' = '^at: texture_2d_initialize \(.*servers/rendering/dummy/storage/texture_storage\.h:\d+\)$'
    # Godot dummy renderer texture cleanup cascade; additionally require the
    # immediately preceding native texture_2d_initialize failure below.
    'ERROR: Parameter "texture" is null.' = '^at: texture_free \(.*servers/rendering/dummy/storage/texture_storage\.h:\d+\)$'
}
$unexpected = [Collections.Generic.List[string]]::new()
$known = [Collections.Generic.List[string]]::new()
$shutdown = [Collections.Generic.List[string]]::new()
# Reproduced without Sts2Mod in the native headless shutdown baseline. Only
# classify these exact engine types after native Quit AND Steam shutdown.
$shutdownTypes = @('N26RendererEnvironmentStorage11EnvironmentE',
    'PN13RendererDummy14TextureStorage12DummyTextureE','N13RendererDummy9DummyMeshE',
    'N13RendererDummy15MaterialStorage13DummyMaterialE','N13RendererDummy15MaterialStorage11DummyShaderE',
    'PN18TextServerAdvanced22ShapedTextDataAdvancedE','PN18TextServerAdvanced12FontAdvancedE',
    'PN18TextServerAdvanced27FontAdvancedLinkedVariationE')
$nativeQuit = $false
$steamStopped = $false
$lines = $Text -split '\r?\n'
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -eq '[INFO] NGame.Quit called') { $nativeQuit = $true }
    if ($nativeQuit -and $line -eq '[INFO] Steamworks shutdown succeeded!') { $steamStopped = $true }
    if ($line -notmatch '^ERROR:') { continue }
    $source = if ($i + 1 -lt $lines.Count) { $lines[$i + 1].Trim() } else { '' }
    $contextAllowed = $true
    if ($line -eq 'ERROR: Parameter "texture" is null.') {
        $contextAllowed = $i -ge 2 -and $lines[$i - 2] -eq 'ERROR: Parameter "t" is null.' -and
            $lines[$i - 1].Trim() -match $nativeHeadlessSources['ERROR: Parameter "t" is null.']
    }
    if ($line -eq 'ERROR: Attempting to initialize the wrong RID') {
        $following = $lines[$i..([Math]::Min($i + 9, $lines.Count - 1))] -join "`n"
        $contextAllowed = $following -match 'ERROR: Parameter "shader" is null\.\n\s*at: shader_set_code \(servers/rendering/dummy/storage/material_storage\.cpp:\d+\)' -or
            $following -match 'ERROR: Parameter "material" is null\.\n\s*at: material_set_shader \(servers/rendering/dummy/storage/material_storage\.cpp:\d+\)'
    }
    if ($Headless -and $contextAllowed -and $nativeHeadlessSources.ContainsKey($line) -and $source -match $nativeHeadlessSources[$line]) {
        $known.Add($line)
    } elseif ($Headless -and $NativeShutdown -and $steamStopped -and
        (($line -match "^ERROR: \d+ RID allocations of type '([^']+)' were leaked at exit\.$" -and $Matches[1] -in $shutdownTypes) -or
         ($line -match '^ERROR: \d+ resources still in use at exit \(run with --verbose for details\)\.$' -and $source -match '^at: clear \(core/io/resource\.cpp:\d+\)$'))) {
        $shutdown.Add($line)
    } else { $unexpected.Add($line) }
}
[pscustomobject]@{ Unexpected = @($unexpected.ToArray()); KnownHeadless = @($known.ToArray()); KnownNativeShutdown = @($shutdown.ToArray()) }
