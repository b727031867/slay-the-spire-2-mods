param([Parameter(Mandatory)][string]$SnapshotDirectory)
$ErrorActionPreference = 'Stop'
$path = (Resolve-Path -LiteralPath $SnapshotDirectory).Path
if ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Snapshot must be a regular directory.' }
$items = @(Get-ChildItem -LiteralPath $path -Recurse -Force)
if (@($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count) { throw 'Snapshot has redirected files or directories.' }
$infoPath = Join-Path $path 'release-info.txt'
$info = Get-Content -Raw -LiteralPath $infoPath | ConvertFrom-Json
$package = & (Join-Path $PSScriptRoot 'Get-ModPackage.ps1') -PackageDirectory (Join-Path $path 'Sts2Mod')
if ($info.schemaVersion -ne 2 -or $info.codeCommit -ne $package.Info.codeCommit -or
    $info.modVersion -ne $package.Info.modVersion -or $info.gameVersion -ne $package.Info.gameVersion -or
    $info.gameBuild -ne $package.Info.gameBuild -or $info.baseLibVersion -ne $package.Info.baseLibVersion) {
    throw 'Snapshot identity differs from its core package; rebuild the snapshot with the current packaging script.'
}
$groups = @(
    @{ Prefix='Sts2Mod'; Hashes=$info.coreFiles; Names=@('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt') },
    @{ Prefix=''; Hashes=$info.documents; Names=@('README.zh-CN.md','CHANGELOG.md','ASSETS.md') },
    @{ Prefix='workshop'; Hashes=$info.workshopFiles; Names=@('workshop.json','mod_id.txt','image.png','previews/rules.png') }
)
$expected = @('release-info.txt')
foreach ($group in $groups) {
    if (@($group.Hashes.PSObject.Properties).Count -ne $group.Names.Count) { throw 'Snapshot file inventory differs.' }
    foreach ($name in $group.Names) {
        $relative = if ($group.Prefix) { $group.Prefix + '/' + $name } else { $name }
        $expected += $relative
        $file = Join-Path $path $relative
        if (!(Test-Path -LiteralPath $file -PathType Leaf) -or !(Get-Item -LiteralPath $file).Length -or
            $group.Hashes.$name -notmatch '^[A-Fa-f0-9]{64}$' -or
            (Get-FileHash -LiteralPath $file).Hash -ne $group.Hashes.$name) { throw "Snapshot checksum mismatch: $relative" }
    }
}
if (@($items | Where-Object { !$_.PSIsContainer }).Count -ne $expected.Count) { throw 'Snapshot has unexpected files.' }
$config = Get-Content -Raw -LiteralPath (Join-Path $path 'workshop/workshop.json') | ConvertFrom-Json
# Preserve validation/retention of historical friends-only snapshots; Prepare-Workshop enforces public uploads.
if ($config.visibility -notin @('public','friends_only') -or [string]::IsNullOrWhiteSpace($config.description) -or
    @($config.dependencies).Count -ne 1 -or $config.dependencies[0] -ne 3737335127 -or
    (Get-Content -Raw -LiteralPath (Join-Path $path 'workshop/mod_id.txt')).Trim() -ne '3823282901') { throw 'Invalid frozen Workshop policy or identity.' }
foreach ($name in @('image.png','previews/rules.png')) {
    if ((Get-Item -LiteralPath (Join-Path $path "workshop/$name")).Length -ge 1000000) { throw 'Workshop image must be less than 1MB.' }
}
[pscustomobject]@{ SnapshotDirectory=$path; Info=$info; Package=$package; SnapshotSHA256=(Get-FileHash -LiteralPath $infoPath).Hash }
