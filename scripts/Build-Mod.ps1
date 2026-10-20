param([Parameter(Mandatory)][ValidateSet('PC-A','PC-B')][string]$DeviceId, [switch]$AllowDirty)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Enter-DevEnvironment.ps1')
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
$previousRevision = $env:SourceRevisionId
$previousCI = $env:ContinuousIntegrationBuild
try {
    $commit = (git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'A source commit is required.' }
    $dirty = [bool](git status --porcelain --untracked-files=normal)
    if ($dirty -and !$AllowDirty) { throw 'Commit source/configuration first, or use -AllowDirty for an explicitly marked development package.' }
    $dependencies = Get-Content -Raw dependencies.json | ConvertFrom-Json
    $gameInfo = Get-Content -Raw (Join-Path $env:STS2_GAME_DIR 'release_info.json') | ConvertFrom-Json
    if ($gameInfo.version -ne $dependencies.gameVersion) { throw "Expected game $($dependencies.gameVersion), found $($gameInfo.version)." }
    $steamApps = Split-Path (Split-Path $env:STS2_GAME_DIR -Parent) -Parent
    $appManifestPath = Join-Path $steamApps 'appmanifest_2868840.acf'
    if (!(Test-Path -LiteralPath $appManifestPath)) { throw 'Steam app manifest not found; verify the game build manually before adapting this script.' }
    $appManifest = Get-Content -Raw -LiteralPath $appManifestPath
    $actualBuild = [regex]::Match($appManifest, '"buildid"\s+"(\d+)"').Groups[1].Value
    if ($actualBuild -ne $dependencies.gameBuild) { throw "Expected Steam build $($dependencies.gameBuild), found $actualBuild." }
    $manifest = Get-Content -Raw src/Sts2Mod/Sts2Mod.json | ConvertFrom-Json
    [xml]$projectMetadata = Get-Content -Raw src/Sts2Mod/Sts2Mod.csproj
    if ([string]$projectMetadata.Project.PropertyGroup.Version -ne [string]$manifest.version) {
        throw 'Assembly and mod manifest versions differ. Update both before building.'
    }
    $sourceIdentity = $commit + $(if ($dirty) { '-dirty' } else { '' })
    $packageName = "Sts2Mod-$($manifest.version)-$($dependencies.gameVersion)-$($sourceIdentity.Substring(0,12))" + $(if ($dirty) { '-dirty' } else { '' })
    $output = Join-Path $projectRoot "artifacts/$packageName"
    $modOutput = Join-Path $output 'Sts2Mod'
    New-Item -ItemType Directory -Force -Path $modOutput | Out-Null
    $allowedFiles = @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck','build-info.txt')
    if (@(Get-ChildItem -LiteralPath $modOutput -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $allowedFiles -or ($_.Attributes -band [IO.FileAttributes]::ReparsePoint) }).Count) {
        throw 'Build destination contains unexpected files. Preserve/review it before building again.'
    }
    $env:SourceRevisionId = $sourceIdentity
    $env:ContinuousIntegrationBuild = 'true'
    $project = Join-Path $projectRoot 'src/Sts2Mod/Sts2Mod.csproj'
    & dotnet restore $project --locked-mode -p:Configuration=ExportRelease --runtime win-x64
    if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed.' }
    & dotnet build $project --configuration ExportRelease --runtime win-x64 --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Mod build failed.' }
    $godotProject = Join-Path $projectRoot 'src/Sts2Mod'
    & $env:STS2_GODOT_EXE --headless --path $godotProject --editor --import
    if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }
    & $env:STS2_GODOT_EXE --headless --path $godotProject --export-pack ModPack (Join-Path $modOutput 'Sts2Mod.pck')
    if ($LASTEXITCODE -ne 0) { throw 'Godot export failed.' }
    Copy-Item -LiteralPath (Join-Path $godotProject '.godot/mono/temp/bin/ExportRelease/win-x64/Sts2Mod.dll') -Destination $modOutput -Force
    Copy-Item -LiteralPath (Join-Path $godotProject 'Sts2Mod.json') -Destination $modOutput -Force
    $fileHashes = @{}
    foreach ($name in @('Sts2Mod.dll','Sts2Mod.json','Sts2Mod.pck')) {
        $file = Get-Item -LiteralPath (Join-Path $modOutput $name)
        if (!$file.Length) { throw "Empty build output: $name" }
        $fileHashes[$name] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    @{ schemaVersion=1; modId='Sts2Mod'; modVersion=$manifest.version; codeCommit=$commit; dirty=$dirty; buildDevice=$DeviceId; gameVersion=$gameInfo.version; gameBuild=$actualBuild; gameAssemblySha256=(Get-FileHash -LiteralPath (Join-Path $env:STS2_GAME_DIR 'data_sts2_windows_x86_64/sts2.dll')).Hash; baseLibVersion=$dependencies.baseLib.version; files=$fileHashes } |
        ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $modOutput 'build-info.txt') -Encoding utf8
    $zipPath = "$output.zip"
    Compress-Archive -LiteralPath $modOutput -DestinationPath $zipPath -Force
    $zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
    "$zipHash  $([IO.Path]::GetFileName($zipPath))" | Set-Content -LiteralPath "$zipPath.sha256" -Encoding ascii
    Write-Output "PackageDirectory=$modOutput"
    Write-Output "Package=$zipPath"
    Write-Output "SHA256=$zipHash"
} finally {
    $env:SourceRevisionId = $previousRevision
    $env:ContinuousIntegrationBuild = $previousCI
    Pop-Location
}
