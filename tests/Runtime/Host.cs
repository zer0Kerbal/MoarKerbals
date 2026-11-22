// Minimal game boundary for exercising the unchanged production KuddleShack module.
// This host does not simulate Unity physics, inventories, rendering or resource flow.
using MoarKerbals;
using UnityEngine;

int tests = 0;
void Check(bool value) { if (!value) throw new Exception("Runtime assertion failed"); }
void Test(string name, Action test) { test(); tests++; Console.WriteLine("PASS " + name); }
KuddleShack Cabin(bool enabled = true)
{
    Planetarium.now = 100; Time.unscaledTime = 0; FlightDriver.Pause = false;
    SettingsInterface.required = 21600;
    UIPartActionController.Instance = null;
    var cabin = new KuddleShack { KuddleShackEnabled = enabled };
    cabin.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Malcolm", gender = ProtoCrewMember.Gender.Male });
    cabin.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Megdrin", gender = ProtoCrewMember.Gender.Female });
    cabin.OnStart(StartState.Flying);
    return cabin;
}
void Tick(KuddleShack cabin, double seconds) { Planetarium.now += seconds; Time.unscaledTime += 1; cabin.Update(); }
void AddPair(KuddleShack c, string male, string female)
{
    c.part.protoModuleCrew.Add(new ProtoCrewMember { name = male, gender = ProtoCrewMember.Gender.Male });
    c.part.protoModuleCrew.Add(new ProtoCrewMember { name = female, gender = ProtoCrewMember.Gender.Female });
}

Test("IDLE cabin advances through Unity Update without staged activation", () => {
    var c = Cabin(); Check(c.part.state == PartStates.IDLE);
    Tick(c, 61); Check(c.progressSeconds == 61 && c.timeRemaining == "5h 58m 59s");
    Check(c.activityStatus.StartsWith("In progress") && c.currentPair == "Malcolm \u2642 + Megdrin \u2640");
});
Test("normal updates count elapsed UT once and loaded timewarp advances", () => {
    var c = Cabin(); Tick(c, 1); Tick(c, 0); Tick(c, 3599); Check(c.progressSeconds == 3600);
    Tick(c, 1e6); Check(c.progressSeconds == 21600 && c.Events["ActivateKuddling"].guiActive);
});
Test("Off pauses with an explanation and resume excludes disabled time", () => {
    var c = Cabin(); Tick(c, 60); c.KuddleShackEnabled = false; Tick(c, 100);
    Check(c.progressSeconds == 60 && c.activityStatus == "Off: progress paused");
    c.KuddleShackEnabled = true; Tick(c, 0); Tick(c, 1); Check(c.progressSeconds == 61);
});
Test("missing pair explains the missing gender and resets progress", () => {
    var c = Cabin(); Tick(c, 90); c.part.protoModuleCrew.RemoveAt(1); Tick(c, 1);
    Check(c.progressSeconds == 0 && c.activityStatus.Contains("Needs a female") && c.timeRemaining == "--");
});
Test("full cabin and completion payment blocker are visible", () => {
    var c = Cabin(); c.part.CrewCapacity = 2; Tick(c, 21600);
    Check(c.activityStatus == "Blocked: cabin full" && !c.Events["ActivateKuddling"].guiActive);
    c.part.CrewCapacity = 10; c.blocker = "need 10 Ore (available 0)"; Tick(c, 0);
    Check(c.activityStatus.Contains(c.blocker) && !c.Events["ActivateKuddling"].guiActive);
    c.blocker = ""; Tick(c, 0); Check(c.activityStatus.StartsWith("Ready:") && c.Events["ActivateKuddling"].guiActive);
});
Test("reload catches up game time once while retaining saved progress", () => {
    var c = Cabin(); Tick(c, 120); c.OnStart(StartState.Flying); Tick(c, 1); Check(c.progressSeconds == 121);
    Planetarium.now += 10000; c.OnStart(StartState.Flying); Tick(c, 1); Check(c.progressSeconds == 10122);
    c.OnStart(StartState.Flying); Tick(c, 0); Check(c.progressSeconds == 10122);
});
Test("serialized checkpoint catches up elsewhere and reaches readiness without payment", () => {
    var c = Cabin(); Tick(c, 100); Planetarium.now += 20;
    var node = new ConfigNode(); c.OnSave(node);
    Check(node.values["progressSeconds"] == "120");
    var restored = new KuddleShack { KuddleShackEnabled = true, pairSignature = node.values["pairSignature"],
        timeCheckpointUT = double.Parse(node.values["timeCheckpointUT"], System.Globalization.CultureInfo.InvariantCulture),
        progressSeconds = double.Parse(node.values["progressSeconds"], System.Globalization.CultureInfo.InvariantCulture),
        wasProgressing = bool.Parse(node.values["wasProgressing"]) };
    restored.part.protoModuleCrew.AddRange(c.part.protoModuleCrew);
    restored.OnLoad(node);
    Planetarium.now += 30000; restored.OnStart(StartState.Flying);
    Check(restored.progressSeconds == 21600 && restored.Events["ActivateKuddling"].guiActive && restored.attempts == 0);
    restored.OnSave(new ConfigNode()); restored.OnStart(StartState.Flying);
    Check(restored.progressSeconds == 21600 && restored.attempts == 0);
});
Test("Off checkpoint excludes time elsewhere and unchanged wall time awards nothing", () => {
    var c = Cabin(); Tick(c, 120); c.KuddleShackEnabled = false; c.OnSave(new ConfigNode());
    Planetarium.now += 30000; c.OnStart(StartState.Flying); Check(c.progressSeconds == 120);
    c.KuddleShackEnabled = true; Tick(c, 0); Tick(c, 0); Check(c.progressSeconds == 120);
    Tick(c, 1); Check(c.progressSeconds == 121);
});
Test("changed pair on return resets progress and older saves start a checkpoint safely", () => {
    var c = Cabin(); Tick(c, 120); Planetarium.now += 10000;
    c.part.protoModuleCrew[1].name = "Different parent"; c.OnStart(StartState.Flying); Check(c.progressSeconds == 0);
    var legacy = new ConfigNode(); legacy.SetValue("progressSeconds", 500, true);
    legacy.SetValue("pairSignature", c.pairSignature, true); legacy.SetValue("timeCheckpointUT", -1, true);
    c.OnLoad(legacy);
    Planetarium.now += 30000; c.OnStart(StartState.Flying); Check(c.progressSeconds == 500);
    Tick(c, 1); Check(c.progressSeconds == 501);
    Planetarium.now -= 100; Tick(c, 0); Check(c.progressSeconds == 501);
});
Test("game pause and module disable show status and award no progress", () => {
    var c = Cabin(); FlightDriver.Pause = true; Tick(c, 20);
    Check(c.progressSeconds == 0 && c.activityStatus.Contains("game paused"));
    FlightDriver.Pause = false; c.isEnabled = false; Tick(c, 20);
    Check(c.progressSeconds == 0 && c.activityStatus.Contains("module disabled"));
});
Test("completion validates again on click and failure keeps earned progress", () => {
    var c = Cabin(); c.ActivateKuddling(); Check(c.attempts == 0);
    Tick(c, 21600); c.blocker = "insufficient funds"; c.ActivateKuddling(); Check(c.attempts == 0);
    c.blocker = ""; c.succeed = false; c.ActivateKuddling();
    Check(c.attempts == 1 && c.progressSeconds == 21600 && c.activityStatus.StartsWith("Completion failed"));
    Tick(c, 0); Check(c.activityStatus.Contains("previous attempt failed"));
    c.succeed = true; c.ActivateKuddling(); Check(c.attempts == 2 && c.progressSeconds == 0);
    c.ActivateKuddling(); Check(c.attempts == 2);
});
Test("two couples progress simultaneously with independent starting times and stable partners", () => {
    var c = Cabin(); Tick(c, 100); AddPair(c, "Aaron", "Alice"); Tick(c, 0); Tick(c, 60);
    Check(c.pairs.Count == 2 && c.pairs[0].Male == "Malcolm" && c.pairs[0].Female == "Megdrin");
    Check(c.pairs[0].Progress == 160 && c.pairs[1].Progress == 60);
    c.ShowPairDetails(); Check(ScreenMessages.last.Contains("Malcolm \u2642 + Megdrin \u2640") && ScreenMessages.last.Contains("Aaron \u2642 + Alice \u2640"));
    c.part.protoModuleCrew.Reverse(); Tick(c, 1);
    Check(c.pairs[0].Progress == 161 && c.pairs[1].Progress == 61 && c.pairs[1].Female == "Alice");
});
Test("removing a parent resets only that couple while other couples keep advancing", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 200);
    c.part.protoModuleCrew.RemoveAll(k => k.name == "Malcolm"); Tick(c, 10);
    Check(c.pairs.Count == 1 && c.pairs[0].Male == "OtherM" && c.pairs[0].Progress == 210);
    c.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Replacement", gender = ProtoCrewMember.Gender.Male });
    Tick(c, 10); Check(c.pairs.Count == 2 && c.pairs[0].Progress == 220 && c.pairs[1].Progress == 0);
});
Test("multi-pair clocks serialize independently and catch up background time exactly once", () => {
    var c = Cabin(); Tick(c, 40); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 60);
    var node = new ConfigNode(); c.OnSave(node); c.OnSave(node); Check(node.GetNodes("KUDDLE_PAIR").Length == 2);
    var restored = new KuddleShack { KuddleShackEnabled = true };
    restored.part.protoModuleCrew.AddRange(c.part.protoModuleCrew); restored.OnLoad(node);
    Planetarium.now += 1000; restored.OnStart(StartState.Flying);
    Check(restored.pairs[0].Progress == 1100 && restored.pairs[1].Progress == 1060);
    restored.OnStart(StartState.Flying); Check(restored.pairs[0].Progress == 1100 && restored.pairs[1].Progress == 1060);
    restored.KuddleShackEnabled = false; restored.OnSave(node); Planetarium.now += 30000;
    restored.OnLoad(node); restored.OnStart(StartState.Flying);
    Check(restored.pairs[0].Progress == 1100 && restored.pairs[1].Progress == 1060);
});
Test("legacy single-pair migration preserves its progress without sharing it with new couples", () => {
    var c = Cabin(); AddPair(c, "Aaron", "Alice");
    var node = new ConfigNode(); node.SetValue("pairSignature", "7:MalcolmMegdrin", true);
    node.SetValue("progressSeconds", "500", true); node.SetValue("timeCheckpointUT", "90", true);
    node.SetValue("wasProgressing", "True", true); c.OnLoad(node); c.OnStart(StartState.Flying);
    Check(c.pairs.Count == 2 && c.pairs[0].Male == "Malcolm" && c.pairs[0].Progress == 510);
    Check(c.pairs[1].Progress == 0);
});
Test("saving before OnStart does not discard an unmigrated legacy clock", () => {
    var c = new KuddleShack { KuddleShackEnabled = true };
    var node = new ConfigNode(); node.SetValue("pairSignature", "7:MalcolmMegdrin", true);
    node.SetValue("progressSeconds", "500", true); node.SetValue("timeCheckpointUT", "-1", true);
    c.OnLoad(node); c.OnSave(node); Check(node.GetValue("pairStateVersion") == "0");
    c.OnLoad(node); AddPair(c, "Malcolm", "Megdrin"); c.OnStart(StartState.Flying);
    Check(c.pairs.Count == 1 && c.pairs[0].Progress == 500);
});
Test("maximum simultaneous pairs follows seats without reusing a Kerbal", () => {
    var c = Cabin(); for (int i = 0; i < 5; i++) AddPair(c, "M" + i, "F" + i);
    Tick(c, 0); Check(c.pairs.Count == 5);
    Check(c.pairs.SelectMany(p => new[] { p.Male, p.Female }).Distinct().Count() == 10);
    c.part.CrewCapacity = 6; Tick(c, 0); Check(c.pairs.Count == 3);
});
Test("one completion batch welcomes multiple offspring with one paid operation each", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 21600);
    Check(c.readyPairs == "2 / 2" && c.Events["ActivateKuddling"].guiName == "Welcome ready Kerbals");
    c.ActivateKuddling(); Check(c.created == 2 && c.attempts == 2 && c.pairs.All(p => p.Progress == 0));
    c.ActivateKuddling(); Check(c.created == 2 && c.attempts == 2);
});
Test("batch stops when seats fill and leaves other ready couples earned progress intact", () => {
    var c = Cabin(); for (int i = 0; i < 3; i++) AddPair(c, "M" + i, "F" + i);
    Tick(c, 0); Tick(c, 21600); c.ActivateKuddling();
    Check(c.part.protoModuleCrew.Count == 10 && c.created == 2 && c.attempts == 2);
    Check(c.pairs.Count(p => p.Progress == 21600) == 2 && c.readyPairs == "2 / 4" && c.activityStatus == "Blocked: cabin full");
    c.ActivateKuddling(); Check(c.created == 2);
});
Test("per-birth resource checks stop partial batches without charging uncompleted couples", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 21600);
    c.completionLimit = 1; c.ActivateKuddling();
    Check(c.created == 1 && c.attempts == 1 && c.pairs[0].Progress == 0 && c.pairs[1].Progress == 21600);
    Check(c.activityStatus.Contains("insufficient resources"));
    c.completionLimit = 2; c.ActivateKuddling(); Check(c.created == 2 && c.attempts == 2);
});
Test("a failed later birth preserves earlier success and the failed pair can retry", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 21600);
    c.failOnAttempt = 2; c.ActivateKuddling();
    Check(c.created == 1 && c.attempts == 2 && c.pairs[0].Progress == 0 && c.pairs[1].Progress == 21600);
    c.ActivateKuddling(); Check(c.created == 2 && c.attempts == 3);
});
Test("completion batch stops if the cabin is disabled during an earlier birth", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0); Tick(c, 21600);
    c.afterBirth = () => c.KuddleShackEnabled = false; c.ActivateKuddling();
    Check(c.created == 1 && c.attempts == 1 && c.pairs[1].Progress == 21600);
});
Test("zero-duration setting completes at most the original ready cohort in a batch", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); SettingsInterface.required = 0;
    Tick(c, 0); c.ActivateKuddling(); Check(c.created == 2 && c.attempts == 2);
});
Test("invalid and overlapping saved records cannot create duplicate partners", () => {
    var c = Cabin(); AddPair(c, "OtherM", "OtherF"); Tick(c, 0);
    var node = new ConfigNode(); c.OnSave(node);
    var duplicate = node.AddNode("KUDDLE_PAIR"); c.pairs[0].Write(duplicate);
    var invalid = node.AddNode("KUDDLE_PAIR"); c.pairs[1].Write(invalid); invalid.SetValue("progress", "NaN", true);
    c.OnLoad(node); c.OnStart(StartState.Flying);
    Check(c.pairs.Count == 2 && c.pairs.SelectMany(p => new[] { p.Male, p.Female }).Distinct().Count() == 4);
    node.RemoveNodes("KUDDLE_PAIR"); node.SetValue("progressSeconds", "21600", true);
    c.OnLoad(node); c.OnStart(StartState.Flying); Check(c.pairs.All(p => p.Progress == 0));
});
Test("recruitment uses unlocked vessel resources and reports a named preflight blocker", () => {
    var academy = new KerbalRecruitment { KerbalRecruitmentEnabled = true, blocker = "need 50 LiquidFuel (available 0)" };
    var civilian = new ProtoCrewMember { name = "Neble", trait = "Civilian" };
    academy.part.protoModuleCrew.Add(civilian);
    academy.RecruitKerbal();
    Check(academy.attempts == 0 && civilian.trait == "Civilian");
    Check(Logging.last.Contains("need 50 LiquidFuel") && Logging.last.Contains("No payment taken"));
    academy.blocker = ""; academy.RecruitKerbal();
    Check(academy.attempts == 1 && civilian.trait != "Civilian" && civilian.experienceTrait.TypeName == civilian.trait);
    academy.RecruitKerbal(); Check(academy.attempts == 1 && Logging.last == "No Civilians available to recruit.");
});
Test("cloning delegates to shared payment checks and cannot create an unpaid Kerbal", () => {
    var bay = new KloneBay { KloningKabinEnabled = true, blocker = "need 100 Oxidizer (available 0)" };
    bay.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Source" });
    bay.ActivateKloning(); Check(bay.attempts == 0 && bay.created == 0 && Logging.last.Contains("Oxidizer"));
    bay.blocker = ""; bay.ActivateKloning(); Check(bay.attempts == 1 && bay.created == 1);
    Check(new[] { typeof(KloneBay), typeof(KerbalRecruitment), typeof(KuddleShack) }.All(t => t.BaseType == typeof(MoarKerbalsBase)));
});
Test("zero accident maximum guarantees a successful paid clone even at 100 percent configured accident rate", () => {
    var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
    settings.QuentinTarantinoMode = 0; settings.KribbleMode = false;
    foreach (bool copy in new[] { false, true })
    foreach (int count in new[] { 1, 3 })
    {
        settings.cloneSourceAttributes = copy;
        var bay = new KloneBay { KloningKabinEnabled = true, accidentRate = 1 };
        var originals = Enumerable.Range(0, count).Select(i => new ProtoCrewMember { name = "Source " + i }).ToArray();
        bay.part.protoModuleCrew.AddRange(originals);
        bay.ActivateKloning();
        Check(bay.created == 1 && bay.attempts == 1 && originals.All(bay.part.protoModuleCrew.Contains));
        Check(Logging.last.Contains("Kloning complete"));
    }
    settings.cloneSourceAttributes = false;
});
Test("positive accident maxima still allow failed cloning and cap losses at one or two", () => {
    var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
    foreach (int limit in new[] { 1, 2 })
    {
        settings.QuentinTarantinoMode = limit;
        var bay = new KloneBay { KloningKabinEnabled = true, accidentRate = 1 };
        bay.part.protoModuleCrew.AddRange(Enumerable.Range(0, 3).Select(i => new ProtoCrewMember { name = "Source " + i }));
        bay.ActivateKloning();
        Check(bay.created == 0 && bay.attempts == 1 && bay.part.protoModuleCrew.Count == 3 - limit);
        Check(Logging.last == "Kloning failed. Kerbals lost: " + limit + ".");
    }
    settings.QuentinTarantinoMode = 0;
});
Test("zero accident maximum still enforces resources and free seats", () => {
    var bay = new KloneBay { KloningKabinEnabled = true, accidentRate = 1, blocker = "need 100 Ore" };
    bay.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Source" });
    bay.ActivateKloning(); Check(bay.attempts == 0 && bay.created == 0 && bay.part.protoModuleCrew.Count == 1);
    bay.blocker = ""; bay.part.CrewCapacity = 1;
    bay.ActivateKloning(); Check(bay.attempts == 0 && bay.created == 0 && bay.part.protoModuleCrew.Count == 1);
});
Test("attribute cloning uses selected occupant and overrides Civilian output", () => {
    var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
    settings.cloneSourceAttributes = true; settings.kloneCivilians = true;
    var bay = new KloneBay { KloningKabinEnabled = true };
    var pilot = new ProtoCrewMember { name = "Pilot", trait = "Pilot" };
    var engineer = new ProtoCrewMember { name = "Engineer", trait = "Engineer", experienceLevel = 3 };
    bay.part.protoModuleCrew.AddRange(new[] { pilot, engineer });
    bay.Update(); Check(bay.sourceKerbalName == "Pilot" && bay.Fields["sourceDisplay"].guiActive);
    bay.NextCloneSource(); bay.Update(); Check(bay.sourceDisplay.Contains("Engineer, level 3"));
    bay.ActivateKloning(); Check(bay.created == 1 && ReferenceEquals(bay.lastCloneSource, engineer));
    settings.cloneSourceAttributes = false; settings.kloneCivilians = false;
});
Test("attribute cloning needs a source even when the living-source difficulty rule is disabled", () => {
    var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
    settings.cloneSourceAttributes = true; settings.requireLivingKerbal = false;
    var bay = new KloneBay { KloningKabinEnabled = true };
    bay.ActivateKloning(); Check(bay.attempts == 0 && Logging.last.Contains("selected source"));
    bay.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Other" }); bay.sourceKerbalName = "Departed";
    bay.ActivateKloning(); Check(bay.attempts == 0);
    bay.NextCloneSource(); bay.ActivateKloning(); Check(bay.created == 1 && bay.lastCloneSource.name == "Other");
    settings.cloneSourceAttributes = false; settings.requireLivingKerbal = true;
});
Test("attribute option off keeps legacy random creation and hides the source controls", () => {
    var bay = new KloneBay { KloningKabinEnabled = true, sourceKerbalName = "Departed" };
    bay.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Other" });
    bay.Update(); Check(!bay.Fields["sourceDisplay"].guiActive && !bay.Events["NextCloneSource"].guiActive);
    bay.ActivateKloning(); Check(bay.created == 1 && bay.lastCloneSource == null);
});
Test("KuddleShack PAW headings show current genders without renaming crew or changing inventory labels", () => {
    var c = Cabin();
    var window = new UIPartActionWindow();
    window.labels.Add(new TMPro.TextMeshProUGUI { text = "Malcolm (17 L - 0.045 t)" });
    window.labels.Add(new TMPro.TextMeshProUGUI { text = "Megdrin (17 L - 0.045 t)" });
    window.labels.Add(new TMPro.TextMeshProUGUI { text = "Megdrin's inventory" });
    UIPartActionController.Instance = new UIPartActionController { part = c.part, window = window };
    c.LateUpdate();
    Check(window.labels[0].text == "Malcolm \u2642 (17 L - 0.045 t)");
    Check(window.labels[1].text == "Megdrin \u2640 (17 L - 0.045 t)");
    Check(window.labels[2].text == "Megdrin's inventory");
    Check(c.part.protoModuleCrew[0].name == "Malcolm" && c.pairs[0].Male == "Malcolm");
    // Stock rebuilds a heading; the next refresh restores one marker, then remains idempotent.
    window.labels[0].text = "Malcolm (17 L - 0.045 t)";
    Time.unscaledTime += 1; c.LateUpdate();
    Time.unscaledTime += 1; c.LateUpdate();
    Check(window.labels[0].text == "Malcolm \u2642 (17 L - 0.045 t)");
    c.part.protoModuleCrew.Add(new ProtoCrewMember { name = "Newborn", gender = ProtoCrewMember.Gender.Female });
    window.labels.Add(new TMPro.TextMeshProUGUI { text = "Newborn (17 L - 0.045 t)" });
    Time.unscaledTime += 1; c.LateUpdate();
    Check(window.labels[3].text == "Newborn \u2640 (17 L - 0.045 t)");
    UIPartActionController.Instance = null;
});
Test("welcoming crew refreshes the existing KIS seat menu and marks an open PAW dirty", () => {
    var c = Cabin(); c.part.protoModuleCrew[0].seatIdx = 0; c.part.protoModuleCrew[1].seatIdx = 1;
    var inventories = Enumerable.Range(0, 10).Select(i => new InventoryDouble { host = c.part, podSeat = i }).ToArray();
    c.part.Modules.AddRange(inventories);
    foreach (var inventory in inventories) inventory.UpdateContextMenu();
    Check(!inventories[2].visible);
    UIPartActionController.Instance = new UIPartActionController { part = c.part, window = new UIPartActionWindow() };
    Tick(c, 21600); c.ActivateKuddling();
    Check(c.created == 1 && inventories[2].visible && inventories[2].label == "New Kerbal 1");
    Check(inventories.All(i => i.refreshes == 2 && i.itemToken == "original items"));
    Check(UIPartActionController.Instance.window.displayDirty);
});
Test("missing or failing optional KIS refresh does not undo births or block other menus", () => {
    var c = Cabin(); c.part.protoModuleCrew[0].seatIdx = 0; c.part.protoModuleCrew[1].seatIdx = 1;
    var healthy = new InventoryDouble { host = c.part, podSeat = 2 };
    c.part.Modules.Add(new PartModule { moduleName = "ModuleKISInventory" });
    c.part.Modules.Add(new BrokenInventoryDouble()); c.part.Modules.Add(healthy);
    Tick(c, 21600); c.ActivateKuddling(); Check(c.created == 1 && c.attempts == 1 && healthy.visible);
    UIPartActionController.Instance = null; CrewInventoryRefresh.Refresh(null);
    c.part.Modules.Clear(); CrewInventoryRefresh.Refresh(c.part);
});
Console.WriteLine(tests + " production-module runtime groups passed (hosted; native KSP pending).");

public enum StartState { Flying }
public enum PartStates { IDLE, ACTIVE }
[AttributeUsage(AttributeTargets.Field)] public class KSPField : Attribute
{ public string guiName, groupName; public bool guiActive, guiActiveEditor, isPersistant; }
[AttributeUsage(AttributeTargets.Field)] public class UI_Toggle : Attribute { public string disabledText, enabledText; }
[AttributeUsage(AttributeTargets.Method)] public class KSPEvent : Attribute
{ public string guiName, groupName; public bool guiActive, guiActiveUnfocused; }
[AttributeUsage(AttributeTargets.Class)] public class KSPModule : Attribute { public KSPModule(string name) {} }
public class ProtoCrewMember
{
    public enum KerbalType { Crew, Tourist }
    public enum Gender { Male, Female }
    public string name; public KerbalType type; public Gender gender;
    public int seatIdx, experienceLevel;
    public string trait;
    public string displayName => name;
    public ExperienceTrait experienceTrait;
    public void UnregisterExperienceTraits(Part part) {}
    public void RegisterExperienceTraits(Part part) {}
    public void Die() {}
}
public class ExperienceTrait { public string TypeName; public string Title => TypeName; }
public static class KerbalRoster
{
    public static void SetExperienceTrait(ProtoCrewMember kerbal, string trait)
    { kerbal.trait = trait; kerbal.experienceTrait = new ExperienceTrait { TypeName = trait }; }
}
public class PartModule { public string moduleName; }
public class ConfigNode
{
    public Dictionary<string, string> values = new();
    public List<(string name, ConfigNode node)> nodes = new();
    public void SetValue(string name, object value, bool createIfNotFound) => values[name] = value.ToString();
    public string GetValue(string name) => values.GetValueOrDefault(name);
    public ConfigNode[] GetNodes(string name) => nodes.Where(n => n.name == name).Select(n => n.node).ToArray();
    public void RemoveNodes(string name) => nodes.RemoveAll(n => n.name == name);
    public ConfigNode AddNode(string name) { var node = new ConfigNode(); nodes.Add((name, node)); return node; }
}
public class Part
{
    public PartStates state; public int CrewCapacity = 10; public List<ProtoCrewMember> protoModuleCrew = new();
    public List<PartModule> Modules = new();
    public void RemoveCrewmember(ProtoCrewMember kerbal) => protoModuleCrew.Remove(kerbal);
}
public class Vessel
{
    public bool loaded = true;
    public string situation = "LANDED";
    public List<Part> parts = new();
    public List<ProtoCrewMember> GetVesselCrew() => parts.SelectMany(p => p.protoModuleCrew).ToList();
}
public static class Planetarium { public static double now; public static double GetUniversalTime() => now; }
public static class FlightDriver { public static bool Pause; }
public static class HighLogic { public static bool LoadedSceneIsFlight = true; public static Game CurrentGame = new(); }
public class Game { public Parameters Parameters = new(); }
public class Parameters
{
    private readonly Dictionary<Type, object> pages = new();
    public T CustomParams<T>() where T : new()
    {
        if (!pages.ContainsKey(typeof(T))) pages[typeof(T)] = new T();
        return (T)pages[typeof(T)];
    }
}
public class MoarKerbalsGameplaySettings
{
    public bool kuddleCivilians = false, entireVesselAcademy, dropOut, kloneCivilians, KribbleMode, cloneSourceAttributes;
    public bool requireLivingKerbal = true, situationallyAware = true, recruitOnlyOne = true;
    public int QuentinTarantinoMode;
}
public class Event { public bool guiActive, guiActiveUnfocused; public string guiName; }
public enum ScreenMessageStyle { UPPER_CENTER }
public static class ScreenMessages { public static string last; public static void PostScreenMessage(string text, float duration, ScreenMessageStyle style) => last = text; }
public class UIPartActionWindow
{
    public bool displayDirty;
    public List<TMPro.TextMeshProUGUI> labels = new();
    public T[] GetComponentsInChildren<T>(bool includeInactive) => labels.Cast<T>().ToArray();
}
public class UIPartActionController
{
    public static UIPartActionController Instance;
    public Part part; public UIPartActionWindow window;
    public UIPartActionWindow GetItem(Part p, bool includeSymmetryCounterparts) => p == part ? window : null;
}
public class InventoryDouble : PartModule
{
    public InventoryDouble() { moduleName = "ModuleKISInventory"; }
    public Part host; public int podSeat, refreshes; public bool visible; public string label, itemToken = "original items";
    public virtual void UpdateContextMenu()
    {
        refreshes++; var crew = host.protoModuleCrew.FirstOrDefault(k => k.seatIdx == podSeat);
        visible = crew != null; label = crew?.name;
    }
}
public class BrokenInventoryDouble : InventoryDouble
{ public override void UpdateContextMenu() => throw new InvalidOperationException("Simulated optional KIS error"); }
namespace UnityEngine { public static class Time { public static float unscaledTime; } }
namespace MoarKerbals
{
    public class MoarKerbalsBase
    {
        public Part part = new(); public Vessel vessel = new(); public bool isEnabled = true;
        public MoarKerbalsBase() { vessel.parts.Add(part); }
        protected static readonly System.Random Random = new();
        protected static bool TraitExists(string name) => true;
        public Dictionary<string, Event> Events = new() { ["ActivateKuddling"] = new(), ["NextCloneSource"] = new() };
        public Dictionary<string, Event> Fields = new() { ["sourceDisplay"] = new() };
        public string blocker = "";
        public int attempts;
        public bool succeed = true;
        public int created, completionLimit = int.MaxValue, failOnAttempt = -1;
        public Action afterBirth;
        public virtual void OnStart(StartState state) {}
        public virtual void OnUpdate() {}
        public virtual void OnLoad(ConfigNode node)
        {
            foreach (var field in GetType().GetFields())
                if (Attribute.IsDefined(field, typeof(KSPField)) && node.GetValue(field.Name) is string value)
                    field.SetValue(this, Convert.ChangeType(value, field.FieldType, System.Globalization.CultureInfo.InvariantCulture));
        }
        public virtual void OnSave(ConfigNode node) {}
        public virtual string GetInfo() => "";
        protected string CompletionBlocker() => created >= completionLimit ? "insufficient resources" : blocker;
        protected bool ExecutePaid(Func<bool> operation)
        {
            string reason = CompletionBlocker();
            if (reason.Length != 0) { Logging.Msg(reason); return false; }
            attempts++; return succeed && attempts != failOnAttempt && operation();
        }
        public ProtoCrewMember lastCloneSource;
        protected bool CreateCrew(bool civilian, ProtoCrewMember source = null)
        {
            if (part.protoModuleCrew.Count >= part.CrewCapacity) return false;
            if (source != null && !part.protoModuleCrew.Contains(source)) return false;
            lastCloneSource = source;
            part.protoModuleCrew.Add(new ProtoCrewMember { name = "New Kerbal " + attempts,
                gender = ProtoCrewMember.Gender.Male,
                seatIdx = Enumerable.Range(0, part.CrewCapacity).First(i => !part.protoModuleCrew.Any(k => k.seatIdx == i)) });
            created++;
            afterBirth?.Invoke();
            return true;
        }
        protected void PlaySound(string category, bool success) {}
        protected void NotifyCrewChanged() => CrewInventoryRefresh.Refresh(part);
        protected string RecipeInfo(string text) => text;
    }
    public static class SettingsInterface { public static double required = 21600; public static double kuddleTimeNeeded() => required; }
    public static class SwitchLight
    {
        public static List<PartModule> GetLightModule(Part p) => new();
        public static void On(PartModule p) {} public static void Off(PartModule p) {}
    }
    public static class Logging { public static string last; public static void Msg(string s) => last = s; public static void DLogWarning(string s) {} }
}
namespace KSP.Localization {}
namespace TMPro { public class TextMeshProUGUI { public string text; } }
namespace MoarKerbals { internal static class GenderSymbolFont { internal static void Ensure() {} } }
