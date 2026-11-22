param(
    [string]$KspRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program',
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$Package
)
$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot
if (!(Test-Path -LiteralPath (Join-Path $KspRoot 'KSP_x64_Data\Managed\Assembly-CSharp.dll'))) { throw 'KspRoot must point to KSP 1.12.5.' }
& dotnet build (Join-Path $repoRoot 'source\MoarKerbals.csproj') -c $Configuration --nologo "-p:KspRoot=$KspRoot" -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'Mod build failed.' }
& dotnet run --project (Join-Path $repoRoot 'tests\MoarKerbals.Tests.csproj') -c Release -- $repoRoot $KspRoot (Join-Path $repoRoot "artifacts\$Configuration\MoarKerbals.dll")
if ($LASTEXITCODE -ne 0) { throw 'Regression checks failed.' }
& dotnet run --project (Join-Path $repoRoot 'tests\Runtime\KuddleRuntime.Tests.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'KuddleShack runtime checks failed.' }
& dotnet run --project (Join-Path $repoRoot 'tests\ResourceRuntime\ResourceRuntime.Tests.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Recipe resource-flow checks failed.' }
& dotnet run --project (Join-Path $repoRoot 'tests\SettingsRuntime\SettingsRuntime.Tests.csproj') -c Release "--property:KspRoot=$KspRoot"
if ($LASTEXITCODE -ne 0) { throw 'Settings persistence checks failed.' }
& dotnet run --project (Join-Path $repoRoot 'tests\CloneRuntime\CloneRuntime.Tests.csproj') -c Release "--property:KspRoot=$KspRoot"
if ($LASTEXITCODE -ne 0) { throw 'Clone attribute checks failed.' }
& (Join-Path $repoRoot 'tests\MetadataRepair.Tests.ps1') -RepoRoot $repoRoot -KspRoot $KspRoot
if (!$Package) { return }
if ($Configuration -ne 'Release') { throw 'Only Release builds may be packaged.' }
$version = (Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'MoarKerbals.version') | ConvertFrom-Json).VERSION
$versionText = '{0}.{1}.{2}.{3}' -f $version.MAJOR,$version.MINOR,$version.PATCH,$version.BUILD
$artifactRoot = Join-Path $repoRoot 'artifacts'
# Unique staging paths avoid replacing any earlier verified package.
$stageRoot = Join-Path $artifactRoot ('MoarKerbals-' + $versionText + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss-ffff'))
$addonRelative = 'GameData\KerbthulhuKineticsProgram\MoarKerbals'
$addonStage = Join-Path $stageRoot $addonRelative
New-Item -ItemType Directory -Path $stageRoot | Out-Null
Copy-Item -LiteralPath (Join-Path $repoRoot 'GameData') -Destination $stageRoot -Recurse
New-Item -ItemType Directory -Path (Join-Path $addonStage 'Plugins') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $artifactRoot 'Release\MoarKerbals.dll') -Destination (Join-Path $addonStage 'Plugins\MoarKerbals.dll')
Copy-Item -LiteralPath (Join-Path $repoRoot 'MoarKerbals.version') -Destination $addonStage
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\MODERN-KSP.md') -Destination (Join-Path $stageRoot 'README.md')
Copy-Item -LiteralPath (Join-Path $repoRoot 'Optional') -Destination $stageRoot -Recurse
$files = @(Get-ChildItem -LiteralPath $stageRoot -File -Recurse | ForEach-Object {
    @{ path = [IO.Path]::GetRelativePath($stageRoot,$_.FullName).Replace('\','/'); sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash; size = $_.Length }
})
$manifest = @{ version=$versionText; source=(git -C $repoRoot rev-parse HEAD); sourceDirty=[bool](git -C $repoRoot status --porcelain); validation='Release and regression checks passed; gameplay pending'; files=$files }
[IO.File]::WriteAllText((Join-Path $stageRoot 'manifest.json'),($manifest | ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
$zipPath = $stageRoot + '.zip'
Compress-Archive -Path (Join-Path $stageRoot '*') -DestinationPath $zipPath
# Validate every packaged entry against its staging hash.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    foreach ($file in $files) {
        $entry = $archive.GetEntry($file.path)
        if ($null -eq $entry) { throw ('Missing ZIP entry: ' + $file.path) }
        $stream = $entry.Open()
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
        if ($hash -ne $file.sha256) { throw ('ZIP hash mismatch: ' + $file.path) }
    }
} finally { $archive.Dispose() }
Write-Output ('Verified package: ' + $zipPath)
Write-Output ('SHA256: ' + (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash)
