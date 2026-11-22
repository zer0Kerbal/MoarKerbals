param(
    [string]$KspRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program',
    [string]$BackupRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) ('KSP Mod Backups\MoarKerbalsParts-changelog-' + (Get-Date -Format 'yyyyMMdd-HHmmss-ffff')))
)
$ErrorActionPreference = 'Stop'
$folder = Join-Path $KspRoot 'GameData\KerbthulhuKineticsProgram\MoarKerbalsParts'
$metadata = Get-Content -Raw -LiteralPath (Join-Path $folder 'MoarKerbalsParts.version') | ConvertFrom-Json
$version = '{0}.{1}.{2}.{3}' -f $metadata.VERSION.MAJOR,$metadata.VERSION.MINOR,$metadata.VERSION.PATCH,$metadata.VERSION.BUILD
if ($version -ne '0.9.99.0') { throw 'Changelog repair requires inspection for parts versions other than 0.9.99.0.' }
$target = Join-Path $folder 'Changelog.cfg'
$original = [IO.File]::ReadAllText($target)
$beforeHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
$repaired = [regex]::Replace($original, '(?m)^([\t ]*)version[\t ]*=[\t ]*([^\r\n]+)', {
    param($match)
    $old = $match.Groups[2].Value.Trim()
    $parsed = $null
    if ([Version]::TryParse($old, [ref]$parsed)) { return $match.Value }
    $prefix = [regex]::Match($old, '^[vV]?(\d+(?:\.\d+){1,3})')
    if (!$prefix.Success) { throw ('Unrecognised historical version: ' + $old) }
    $components = @($prefix.Groups[1].Value.Split('.'))
    while ($components.Count -lt 4) { $components += '0' }
    $indent = $match.Groups[1].Value
    return $indent + 'version = ' + ($components -join '.') + "`n" + $indent + 'versionName = ' + $old
})
$repaired = [regex]::Replace($repaired, '<b\s+style="color:([a-zA-Z]+)"\s*>(.*?)</b>', '<color=$1><b>$2</b></color>')
$repaired = [regex]::Replace($repaired, '(?m)^([\t ]*license[\t ]*=[\t ]*)"([^"\r\n]+)(?=\r?$)', '$1$2')
foreach ($match in [regex]::Matches($repaired, '(?m)^[\t ]*version[\t ]*=[\t ]*([^\r\n]+)')) {
    $parsed = $null
    if (![Version]::TryParse($match.Groups[1].Value.Trim(), [ref]$parsed)) { throw 'Repaired changelog has an invalid version.' }
}
if ($original -eq $repaired) { Write-Output 'Changelog versions and formatting already valid.'; return }
New-Item -ItemType Directory -Path $BackupRoot | Out-Null
$backup = Join-Path $BackupRoot 'Changelog.cfg.before'
Copy-Item -LiteralPath $target -Destination $backup
if ((Get-FileHash -LiteralPath $backup).Hash -ne $beforeHash) { throw 'Changelog backup mismatch.' }
$temporary = Join-Path $BackupRoot 'Changelog.cfg.repaired'
[IO.File]::WriteAllText($temporary, $repaired, [Text.UTF8Encoding]::new($false))
if ((Get-FileHash -LiteralPath $target).Hash -ne $beforeHash) { throw 'Concurrent changelog change; repair aborted.' }
Copy-Item -LiteralPath $temporary -Destination $target
$afterHash = (Get-FileHash -LiteralPath $temporary).Hash
if ((Get-FileHash -LiteralPath $target).Hash -ne $afterHash) { throw 'Changelog install mismatch.' }
$receipt = @{ target=$target; backup=$backup; before_sha256=$beforeHash; after_sha256=$afterHash; scope='Normalise historical versions, convert unsupported HTML bold/color to Unity rich text, remove unmatched license quote; preserve descriptions and all change entries' }
[IO.File]::WriteAllText((Join-Path $BackupRoot 'receipt.json'), ($receipt | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
Write-Output ('Changelog repaired; backup: ' + $BackupRoot)
