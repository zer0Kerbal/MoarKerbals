# Changelog

## 1.3.15.0 - repair KuddleShack gender symbol rendering

- Initialise the generated font's empty kerning table before TextMeshPro builds its lookup tables. The missing table prevented fallback registration and produced squares in crew headings. Include the exception stack in font diagnostics.

## 1.3.14.0 - normal stock equipment for attribute clones

- Give attribute-copy clones fresh stock EVA default equipment (jetpack and parachute), matching ordinary new Kerbals. Keep KIS inventories empty and never copy source items.
- Use stock SetDefaultInventory after the clone's gender/suit/type have loaded. Preserve the stock tourist exception and any installed EVA-default customization.
- Clarify inventory wording in the difficulty tooltip and part information. Existing Kerbals/saves are not retroactively changed.

## 1.3.13.0 - KuddleShack gender markers

- Show male/female symbols beside names in the KuddleShack PAW crew headings, single-pair display and Pair details.
- Supply two procedural SDF glyphs through a runtime TextMeshPro fallback; installed Noto Sans SDF and bundled fallback tables lack both symbols. No external font files or stock asset replacements.
- Preserve actual crew names, saved pair identities, inventories and timers; refresh only the matching cabin's open PAW headings after stock rebuilds or crew changes.
- Native glyph appearance/layout remains pending; offline string, glyph geometry and hosted PAW refresh checks added.

## 1.3.12.0 - zero maximum disables cloning accidents

- Maximum Kerbals affected = 0 now bypasses the entire cloning accident roll, so a valid paid attempt succeeds without accident failure or casualties. Positive casualty limits retain their existing probability and caps.
- Correct difficulty tooltip (including the actual default of zero) and part information. No recipe, costs, multiple-birth rules or existing saves changed.
- Hosted production-module checks force a 100% configured accident probability and verify zero-limit success in random/attribute modes with one/multiple occupants, unchanged positive caps, and normal resource/seat requirements.

## 1.3.11.0 - optional source-attribute cloning

- Add a saved difficulty option, off by default, to copy a selected KloneKabin occupant with a new unique name. Copies stock personal attributes, suit/appearance, profession (including MKS), experience and level; overrides Civilian output and birthday suit for clones only.
- Start with empty stock/KIS inventories and fresh mission history. A new inherited-skills training credit preserves exact XP across reload without copying the source's missions or achievements.
- Add a persistent per-cabin source selector. Missing/departed sources prevent payment; extra births use the same source and shared transaction/refund path. Legacy random generation remains available with the option off.
- Add production-template tests using actual KSP serialization/XP calculation, difficulty quickload checks, and hosted source-selection checks. Native gameplay remains pending.

## 1.3.10.0 — scaled KloneKabin and KloneSchool inventory capacity

- Fix KIS crew-transfer rejection for enlarged ground/orbital cloning cabins and schools: allocate six personal inventories during prefab loading and use volume-scaled two/six-seat instance capacity.
- Supply matching independent IVA seats; preserve occupied seat indices and existing inventories.
- Generalize capacity descriptions and include the pending 1.3.9 KIS menu refresh.
- Offline build/regression validation; native transfer and reload checks pending.

## 1.3.9.0 — refresh KIS inventory buttons after crew changes

- Refresh existing KIS seat-inventory menu events from the shared crew-change notification, then mark the matching open PAW dirty. Covers reproduction, cloning and recruitment notifications without synthetic transfer events, inventory reinitialization or item changes. KIS remains optional; reflection targets its public parameterless UpdateContextMenu method and optional UI failures do not undo completed operations. Hosted tests check the new-seat button, unchanged existing contents, open-PAW rebuild and missing/failing KIS; native validation pending.

## 1.3.8.0 — shared routing for every MoarKerbals ability

- Audit all three consuming module types and all 14 installed ability instances across 13 parts. Cloning remained susceptible after the earlier reproduction/recruitment corrections. Enforce vessel-wide unlocked-tank routing in the shared adapter, remove the per-module opt-out and centralize shortage preflight for every paid action. Checks, payments and refunds use one policy, including cloning accidents and additional births. Add a compiled-DLL audit that rejects native resource calls outside the shared path. Full part inventory and validation limits are documented in Resource-routing-audit.md.

## 1.3.7.0 — recruitment consumables and failure explanations

- Extend unlocked same-vessel resource checks, consumption and refunds to KerbalRecruitment, including the MKS Akademy and KloneSchools. The previous vessel-wide fix applied only to KuddleShack, leaving recruitment subject to LiquidFuel stack routing. Preflight recruitment attempts and report the specific resource or currency shortage before payment. Hosted tests cover the saved large LiquidFuel tank, an empty small tank, the complete Akademy recipe, a named blocker and civilian promotion. Native gameplay verification pending.

## 1.3.6.0 — collision-safe difficulty settings candidate

- Replace generic Settings/Settings2/Settings3 custom-parameter type names with unique MoarKerbals names and update every reader. Migrate recognizable legacy pages on game construction using the event's newly created Game object; ignore unrelated or ambiguous legacy sections and prefer existing unique sections. Preserve UI localization keys and per-save settings semantics. Twelve settings test groups exercise installed KSP load/save methods, including real quicksave parameter sections and repeated round-trips. No save files manually rewritten; native startup/UI validation pending.

## 1.3.5.0 — simultaneous independent KuddleShack pairs

- Maintain stable, disjoint male/female couples up to half the seat capacity, with independent progress, saved checkpoints and background catch-up. Keep unaffected couples' timers when crew changes; migrate the old single-pair clock without copying its progress to new couples. PAW shows ready/total pairs, next readiness and Pair details. Welcome ready Kerbals completes the ready cohort with one payment per offspring, stopping on full seats, insufficient resources, failure or disabling; uncompleted pairs retain readiness.

## 1.3.4.0 — KuddleShack vessel-wide consumables candidate

- Use unlocked tanks on the same vessel for KuddleShack recipe checks, withdrawals and refunds, rather than Oxidizer's engine stack-crossfeed routing. Other facilities retain their existing native resource flow. A larger tank outside the engine crossfeed path could contain enough Oxidizer while the cabin could only see the smaller tank. Hosted adapter/transaction checks cover both tanks, locked tanks, consumption and refunds. Native gameplay verification pending; no save or attachment changes.

## 1.3.3.0 — background reproduction progress

- Persist a game-time checkpoint and running state so enabled KuddleShack progress catches up when returning from another vessel, the Space Centre or save/reload. Clamp at readiness, retain the existing explicit paid completion action, pause while Off, reset after a changed pair, and avoid duplicate elapsed time. Existing saves without a checkpoint retain their earned progress and begin tracking on first load. Remove the loaded-vessel-only PAW wording. Real time with KSP closed does not advance the timer.

## 1.3.2.0 — KuddleShack countdown and status candidate

- Fix frozen reproduction progress on idle cabins: use Unity Update instead of KSP's ACTIVE-part-only OnFixedUpdate callback. Preserve loaded-time, saved-progress and single-payment behaviour. Refresh the PAW twice per real second with a countdown, selected pair, explicit progress/pause/missing-pair/full-cabin status, and completion resource/currency blockers. Rename the completion action to Welcome new Kerbal. Hosted tests exercise the production module on an idle part, timewarp, pause/resume, reload, missing crew and completion retry; native KSP validation is pending.

## 1.3.1.0 — volume-scaled KuddleShack candidate

- Replace the three-seat guard with cubic capacity: 3 seats at 2.5 m, 10 at 3.75 m. Reserve matching KIS inventories at prefab load, add seven independent IVA passenger seats and preserve occupied seats when shrinking. Build and regression checks are offline; native loading, transfers, inventory persistence, portraits and chair placement require gameplay validation. No saves are edited.

## 1.3.0.0 — KSP 1.12.5 fork candidate

- Historical KIS/TweakScale workaround held the KuddleShack at three seats; superseded by volume scaling in 1.3.1.0.

- Changelog display follow-up: convert the parts pack's unsupported HTML-style bold/red tag to Unity rich text and remove its unmatched licence quote, retaining the warning and full history. Test the original malformed formatting explicitly after live repairs.

- Add a backed-up repair for MoarKerbalsParts 0.9.99.0 historical changelog versions that caused KerbalChangelog startup exceptions. Move descriptive version text to versionName, retaining all history; validate numeric versions and repeat application in fixture tests.

- MoarKerbalsParts 0.9.99.0 compatibility: repair undefined KW250 scaling type to installed standard stack type, retaining existing discrete sizes; declare KuddleShack valid on surface and in orbit. Include a backed-up, version-guarded metadata identity/URL repair outside default GameData so installing the candidate alone never creates a partial parts pack. Both KloneSchools gain the upstream-documented per-attempt recipe (5000 EC, 50 LiquidFuel, 50 Ore) and costs (5000 funds, 1 science, 2 reputation); existing compatibility recipes are preserved.

- Issue review follow-up: treat PRELAUNCH as a surface situation, so both orbital-only and surface-only cloning facilities behave correctly on the launchpad. Regression coverage includes launchpad, landed, splashed, orbit, flight and the explicit situation-enforcement toggle.

- Modern .NET SDK build against local KSP references, no automatic deployment or text-template dependency; remove the Windows case-colliding project; fix Debug version constants and align package metadata.
- Fix accident-disabled cloning, duplicate resource charges, incorrect refunds, exact currency balances, Science-mode currency handling, recipe validation and final-seat capacity checks. Failed insertion removes the generated roster entry and refunds its resource payment.
- Validate every action in flight; enforce enabled/timing/situation rules independently of button visibility. Bound cloning casualties by the configured cap. Extra births pay separately.
- Persist reproduction progress and pair identity; pause on disable, reset for a different pair, exclude unloaded time, reset after each completed birth. Charge once at completion instead of during both waiting and birth. Remove crew-event subscription leaks by checking current occupants directly.
- Use registered internal trait identifiers, preserve earned experience and history, and refresh registered profession effects after recruitment. Recruitment remains random; optional dropout chance remains 25%.
- Add optional MKS support for ten additional professions and a duplicate-guarded recruitment module on `Tundra_Akademy`, preserving MKS experience training and customized KolonyTools.dll.
- Guard all stock module additions against duplicates and skip both six-seat lab patches with FreeIVA.
- Make sound choices obey their labelled success/failure settings; include all choices in random selection; fix missing audio paths and idempotent cabin-light controls. Remove fragile main-menu logging singleton and log-file cleanup.
- Add focused executable regression checks, installed-MKS/resource configuration checks and package file hashes. Builds and offline tests do not establish gameplay validation.
