param([Parameter(Mandatory)][AllowEmptyString()][string]$Text, [switch]$Headless)
$ErrorActionPreference = 'Stop'
# Match the native source context as well as the message. A generic null/RID
# error from gameplay or any other subsystem must never become a passing test.
$nativeHeadlessSources = @{
    'ERROR: Attempting to initialize the wrong RID' = '^at: initialize_rid \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Initializing already initialized RID' = '^at: get_or_null \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Attempting to use an uninitialized RID' = '^at: get_or_null \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Parameter "mem" is null.' = '^at: initialize_rid \(.*core/templates/rid_owner\.h:\d+\)$'
    'ERROR: Parameter "shader" is null.' = '^at: shader_set_code \(.*servers/rendering/dummy/storage/material_storage\.cpp:\d+\)$'
    'ERROR: Parameter "t" is null.' = '^at: texture_2d_initialize \(.*servers/rendering/dummy/storage/texture_storage\.h:\d+\)$'
}
$unexpected = [Collections.Generic.List[string]]::new()
$known = [Collections.Generic.List[string]]::new()
$lines = $Text -split '\r?\n'
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -notmatch '^ERROR:') { continue }
    $source = if ($i + 1 -lt $lines.Count) { $lines[$i + 1].Trim() } else { '' }
    if ($Headless -and $nativeHeadlessSources.ContainsKey($line) -and $source -match $nativeHeadlessSources[$line]) {
        $known.Add($line)
    } else { $unexpected.Add($line) }
}
[pscustomobject]@{ Unexpected = @($unexpected.ToArray()); KnownHeadless = @($known.ToArray()) }
