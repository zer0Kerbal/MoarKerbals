# MoarKerbals 1.3.15.0 for KSP 1.12.5

MoarKerbals was originally created by strideknight and maintained by zer0Kerbal. Recruitment includes work derived from LinuxGuruGamer's Civilian Population. Existing licences and attribution apply.

## Crew operations

- KloneKabin checks crew and vessel situation before payment. The normal cloning path is available when the accident rate or maximum affected crew is zero. Positive accident settings bound casualties. Additional births respect capacity and pay individually.
- The optional source-attribute mode copies a selected occupant's stock attributes, appearance, profession and XP into a Kerbal with a unique name. The clone receives normal stock EVA equipment, an empty KIS inventory and fresh mission history. A separate inherited-skills credit preserves XP.
- KuddleShack displays status, countdown, ready pairs and blockers. Independent pairs keep their progress across saves and catch up on return. Off pauses progress. A player action completes ready births, with one payment per offspring. Missing pairs and full cabins block completion.
- KuddleShack crew and pair labels show male/female symbols. A TextMeshPro fallback supplies the missing glyphs.
- Recruitment uses registered trait identifiers, preserves experience and refreshes profession effects. Optional MKS integration adds ten professions and a guarded recruitment action to the Tundra Akademy alongside its normal training.
- Cloning, reproduction and recruitment check, debit and refund resources through one adapter using unlocked tanks on the current vessel. Payments accept exact balances and use the currencies appropriate to the game mode.

## Compatibility and existing saves

- ModuleManager is required. The MoarKerbalsParts, KIS, FreeIVA and MKS integration paths are conditional.
- Supported parts scale from 2 to 6 seats for KloneKabins and KloneSchools and from 3 to 10 seats for KuddleShacks. Matching IVA and KIS seats are reserved. Occupied indices are preserved when shrinking, and KIS menus refresh after crew changes.
- The stock lab seat expansion is skipped when FreeIVA is present. The parts-pack compatibility patch supplies a supported TweakScale type and the documented KloneSchool recipe where an override is absent. Optional scripts under `Optional/MoarKerbalsParts` repair metadata and changelog formatting for the supported installed parts version.
- Existing per-save settings migrate to uniquely named pages while preserving unrelated settings. Old single-pair progress migrates to the matching pair. Retain a save backup before downgrading, because earlier builds cannot preserve the newer per-pair records.

## Build and verification

Use PowerShell 7 and .NET SDK 8 to run `./build.ps1 -Configuration Release -Package`. Pass `-KspRoot` for a non-default installation. The script uses local KSP references, runs offline checks and stages a verified package under `artifacts`. Install the packaged `GameData` contents with KSP closed. Keep one MoarKerbals DLL.

Offline checks cover payment, scaling, clone attributes, pair timing, settings load/save, metadata and packaging. In-game scenarios should exercise zero-accident cloning, launchpad restrictions, pair progress across scene changes, enlarged-seat KIS transfer, and recruitment.
