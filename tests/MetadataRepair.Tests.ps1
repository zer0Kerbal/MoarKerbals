param([string]$RepoRoot, [string]$KspRoot)
$ErrorActionPreference = 'Stop'
$installed = Join-Path $KspRoot 'GameData\KerbthulhuKineticsProgram\MoarKerbalsParts\MoarKerbalsParts.version'
if (!(Test-Path -LiteralPath $installed)) { Write-Output 'SKIP metadata repair fixture: optional parts not installed'; return }
$fixtureRoot = Join-Path $RepoRoot ('artifacts\metadata-tests-' + (Get-Date -Format 'yyyyMMdd-HHmmss-ffff'))
$relative = 'GameData\KerbthulhuKineticsProgram\MoarKerbalsParts'
$fixtureParts = Join-Path $fixtureRoot $relative
New-Item -ItemType Directory -Path $fixtureParts -Force | Out-Null
$old = Get-Content -Raw -LiteralPath $installed | ConvertFrom-Json
$old.NAME = 'MoarKerbals (MOAR)'
$old.URL = 'https://example.invalid/wrong-project'
$old.GITHUB.REPOSITORY = 'MoarKerbals'
$old.INSTALL_LOC.NAME = 'MoarKerbals'
$target = Join-Path $fixtureParts 'MoarKerbalsParts.version'
[IO.File]::WriteAllText($target,($old | ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
$originalHash = (Get-FileHash -LiteralPath $target).Hash
$backup = Join-Path $fixtureRoot 'backup'
$script = Join-Path $RepoRoot 'Optional\MoarKerbalsParts\Apply-MetadataRepair.ps1'
& $script -KspRoot $fixtureRoot -BackupRoot $backup | Out-Null
$repaired = Get-Content -Raw -LiteralPath $target | ConvertFrom-Json
if ($repaired.GITHUB.REPOSITORY -ne 'MoarKerbalsParts' -or $repaired.INSTALL_LOC.NAME -ne 'MoarKerbalsParts' -or $repaired.URL -ne 'https://raw.githubusercontent.com/zer0Kerbal/MoarKerbalsParts/master/MoarKerbalsParts.version') { throw 'Wrong repaired identity' }
foreach($field in @('VERSION','KSP_VERSION','KSP_VERSION_MIN','KSP_VERSION_MAX')) {
    if (($old.$field | ConvertTo-Json -Compress) -ne ($repaired.$field | ConvertTo-Json -Compress)) { throw ('Metadata changed protected field: '+$field) }
}
if ((Get-FileHash -LiteralPath (Join-Path $backup 'MoarKerbalsParts.version.before')).Hash -ne $originalHash) { throw 'Original backup was not preserved exactly' }
$hash = (Get-FileHash -LiteralPath $target).Hash
& $script -KspRoot $fixtureRoot -BackupRoot (Join-Path $fixtureRoot 'second-backup') | Out-Null
if ((Get-FileHash -LiteralPath $target).Hash -ne $hash -or (Test-Path -LiteralPath (Join-Path $fixtureRoot 'second-backup'))) { throw 'Repair is not idempotent' }
$changelogSource = Join-Path $KspRoot 'GameData\KerbthulhuKineticsProgram\MoarKerbalsParts\Changelog.cfg'
Copy-Item -LiteralPath $changelogSource -Destination (Join-Path $fixtureParts 'Changelog.cfg')
$changelogScript = Join-Path $RepoRoot 'Optional\MoarKerbalsParts\Repair-Changelog.ps1'
$fixtureChangelog = Join-Path $fixtureParts 'Changelog.cfg'
# Explicitly seed the original failures even after the live installation has been repaired.
$seed = [IO.File]::ReadAllText($fixtureChangelog)
$seed = [regex]::Replace($seed,'(?m)^([\t ]*version[\t ]*=[\t ]*)1\.0\.0\.0[\t ]*$', '${1}v1.0')
$seed = $seed.Replace('<color=red><b>DO A CLEAN INSTALL: DELETE EXISTING THEN RE-INSTALL</b></color>','<b style="color:red">DO A CLEAN INSTALL: DELETE EXISTING THEN RE-INSTALL</b>')
$seed = [regex]::Replace($seed,'(?m)^([\t ]*license[\t ]*=[\t ]*)CC BY-NC-SA 4\.0[\t ]*$', '$1"CC BY-NC-SA 4.0')
[IO.File]::WriteAllText($fixtureChangelog,$seed,[Text.UTF8Encoding]::new($false))
$changelogBefore = [IO.File]::ReadAllText((Join-Path $fixtureParts 'Changelog.cfg'))
& $changelogScript -KspRoot $fixtureRoot -BackupRoot (Join-Path $fixtureRoot 'changelog-backup') | Out-Null
$changelogAfter = [IO.File]::ReadAllText((Join-Path $fixtureParts 'Changelog.cfg'))
if ([regex]::Matches($changelogBefore,'(?m)^[\t ]*change[\t ]*=').Count -ne [regex]::Matches($changelogAfter,'(?m)^[\t ]*change[\t ]*=').Count) { throw 'Changelog history lost' }
foreach($match in [regex]::Matches($changelogAfter,'(?m)^[\t ]*version[\t ]*=[\t ]*([^\r\n]+)')) { $parsed=$null; if(![Version]::TryParse($match.Groups[1].Value.Trim(),[ref]$parsed)){throw 'Bad historical version'} }
if (!$changelogAfter.Contains('Let There Be Kerbals!')) { throw 'Historical description lost' }
if ($changelogAfter.Contains('<b style=') -or !$changelogAfter.Contains('<color=red><b>DO A CLEAN INSTALL: DELETE EXISTING THEN RE-INSTALL</b></color>')) { throw 'Unsupported rich text was not repaired' }
if ($changelogAfter -match '(?m)^[\t ]*license[\t ]*=[\t ]*"') { throw 'Unmatched licence quote remains' }
& $changelogScript -KspRoot $fixtureRoot -BackupRoot (Join-Path $fixtureRoot 'second-changelog-backup') | Out-Null
if ([IO.File]::ReadAllText((Join-Path $fixtureParts 'Changelog.cfg')) -ne $changelogAfter) { throw 'Changelog repair not idempotent' }
Write-Output 'PASS changelog repair: numeric versions, Unity-compatible formatting, licence text, retained history/descriptions and idempotency (fixture only)'
$repaired.VERSION.BUILD = 1
[IO.File]::WriteAllText($target,($repaired | ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
$unsupportedHash = (Get-FileHash -LiteralPath $target).Hash
$rejected = $false
try { & $script -KspRoot $fixtureRoot -BackupRoot (Join-Path $fixtureRoot 'unsupported-backup') | Out-Null } catch { $rejected = $true }
if (!$rejected -or (Get-FileHash -LiteralPath $target).Hash -ne $unsupportedHash) { throw 'Unsupported version was modified' }
Write-Output 'PASS metadata repair: correct identity/URLs, protected fields preserved, verified backup, idempotency, unsupported-version refusal (fixture only)'
