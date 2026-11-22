param(
    [string]$KspRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program',
    [string]$BackupRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) ('KSP Mod Backups\MoarKerbalsParts-metadata-' + (Get-Date -Format 'yyyyMMdd-HHmmss-ffff')))
)
$ErrorActionPreference = 'Stop'
$target = Join-Path $KspRoot 'GameData\KerbthulhuKineticsProgram\MoarKerbalsParts\MoarKerbalsParts.version'
if (!(Test-Path -LiteralPath $target)) { throw 'MoarKerbalsParts is not installed; nothing was created.' }
$beforeHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
$metadata = Get-Content -Raw -LiteralPath $target | ConvertFrom-Json
$version = '{0}.{1}.{2}.{3}' -f $metadata.VERSION.MAJOR,$metadata.VERSION.MINOR,$metadata.VERSION.PATCH,$metadata.VERSION.BUILD
if ($version -ne '0.9.99.0') { throw ('This repair was checked for parts version 0.9.99.0; found ' + $version + '. Inspect newer metadata before reapplying.') }
$metadata.NAME = 'MoarKerbals Parts (MOARP)'
$metadata.URL = 'https://raw.githubusercontent.com/zer0Kerbal/MoarKerbalsParts/master/MoarKerbalsParts.version'
$metadata.DOWNLOAD = 'https://github.com/zer0Kerbal/MoarKerbalsParts/releases'
$metadata.CHANGE_LOG_URL = 'https://raw.githubusercontent.com/zer0Kerbal/MoarKerbalsParts/master/Changelog.cfg'
$metadata.GITHUB.USERNAME = 'zer0Kerbal'
$metadata.GITHUB.REPOSITORY = 'MoarKerbalsParts'
$metadata.INSTALL_LOC.NAME = 'MoarKerbalsParts'
$json = ($metadata | ConvertTo-Json -Depth 8).Replace("`r`n", "`n") + "`n"
$validated = $json | ConvertFrom-Json
if ($validated.GITHUB.REPOSITORY -ne 'MoarKerbalsParts' -or $validated.VERSION.BUILD -ne 0) { throw 'Replacement validation failed.' }
$original = [IO.File]::ReadAllText($target)
if ($original -eq $json) { Write-Output ('Metadata already repaired: ' + $target); return }
New-Item -ItemType Directory -Path $BackupRoot | Out-Null
$backup = Join-Path $BackupRoot 'MoarKerbalsParts.version.before'
Copy-Item -LiteralPath $target -Destination $backup
if ((Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash -ne $beforeHash) { throw 'Backup hash mismatch.' }
$temporary = Join-Path $BackupRoot 'MoarKerbalsParts.version.repaired'
[IO.File]::WriteAllText($temporary, $json, [Text.UTF8Encoding]::new($false))
$null = Get-Content -Raw -LiteralPath $temporary | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $beforeHash) { throw 'Installed metadata changed concurrently; repair aborted.' }
Copy-Item -LiteralPath $temporary -Destination $target
$afterHash = (Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash
if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -ne $afterHash) { throw 'Installed metadata hash mismatch.' }
$receipt = [ordered]@{version=$version;target=$target;backup=$backup;before_sha256=$beforeHash;after_sha256=$afterHash;scope='Metadata identity and URLs only; version and KSP range preserved; no DLL/part/save edits'}
[IO.File]::WriteAllText((Join-Path $BackupRoot 'receipt.json'), ($receipt | ConvertTo-Json -Depth 4), [Text.UTF8Encoding]::new($false))
Write-Output ('Metadata repaired: ' + $target)
Write-Output ('Verified backup and receipt: ' + $BackupRoot)
