using System.Reflection;
using System.Runtime.CompilerServices;
using MoarKerbals;

UnityEngine.Debug.unityLogger.logEnabled = false;
int passed = 0;
void Check(bool value) { if (!value) throw new Exception("Clone assertion failed"); }
void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
var source = (ProtoCrewMember)RuntimeHelpers.GetUninitializedObject(typeof(ProtoCrewMember));
typeof(ProtoCrewMember).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(source, "Original Kerman");
source.careerLog = new FlightLog(); source.flightLog = new FlightLog();
source.careerLog.AddEntry(FlightLog.EntryType.PlantFlag, "Moho");
source.flightLog.AddEntry(FlightLog.EntryType.Orbit, "Mun");
source.gender = ProtoCrewMember.Gender.Female; source.trait = "Engineer";
source.courage = 0.75f; source.stupidity = 0.125f; source.isBadass = true; source.veteran = true;
source.experience = 27.5f; source.experienceLevel = 3;
source.InventoryNode = new ConfigNode("INVENTORY"); source.InventoryNode.AddNode("STOREDPART").AddValue("partName", "evaJetpack");
Test("production template copies attributes and gives a distinct name without mutating source", () => {
    var clone = KerbalCloning.Template(source, "New Kerman", "Kerbin");
    Check(clone.GetValue("name") == "New Kerman" && source.name == "Original Kerman");
    Check(clone.GetValue("trait") == "Engineer" && clone.GetValue("gender") == "Female");
    Check(float.Parse(clone.GetValue("brave")) == source.courage && float.Parse(clone.GetValue("dumb")) == source.stupidity);
    Check(bool.Parse(clone.GetValue("badS")) && bool.Parse(clone.GetValue("veteran")));
    Check(source.careerLog.HasEntry(FlightLog.EntryType.PlantFlag, "Moho") && source.InventoryNode.HasNode("STOREDPART"));
});
Test("production template has empty inventories and no copied flight or achievement history", () => {
    var clone = KerbalCloning.Template(source, "New Kerman", "Kerbin");
    Check(clone.GetNode("INVENTORY").nodes.Count == 0 && clone.GetNode("FLIGHT_LOG").nodes.Count == 0);
    var career = new FlightLog(); career.Load(clone.GetNode("CAREER_LOG"));
    Check(career.Count == 1 && career[0].type.StartsWith(KerbalCloning.TrainingPrefix));
    Check(!career.HasEntry(FlightLog.EntryType.PlantFlag) && !career.HasEntry(FlightLog.EntryType.Orbit));
    Check(clone.GetValue("idx") == "-1" && clone.GetValue("state") == "Available");
});
Test("inherited XP encodes exact float and re-registers from actual stock career serialization", () => {
    var clone = KerbalCloning.Template(source, "New Kerman", "Kerbin");
    var log = new FlightLog(); log.Load(clone.GetNode("CAREER_LOG"));
    uint bits = Convert.ToUInt32(log[0].type.Substring(KerbalCloning.TrainingPrefix.Length), 16);
    Check(BitConverter.ToSingle(BitConverter.GetBytes(bits), 0) == 27.5f);
    var game = new ConfigNode("GAME"); game.AddNode("ROSTER").AddNode(clone);
    KerbalCloning.RegisterSavedTraining(game);
    KerbalCloning.RegisterCrewTraining(clone);
    KerbalCloning.Register(KerbalCloning.TrainingPrefix + "7FC00000"); // NaN is ignored.
    KerbalCloning.Register(KerbalCloning.TrainingPrefix + "7F800000");
    KerbalCloning.Register("unrelated");
});
Test("installed KSP XP calculation retains exact inherited experience after serialized reload", () => {
    // Only the body's data boundary is supplied; stock XP registration/calculation is unchanged.
    var body = (CelestialBody)RuntimeHelpers.GetUninitializedObject(typeof(CelestialBody));
    body.bodyName = "Kerbin"; body.isHomeWorld = true;
    body.scienceValues = new CelestialBodyScienceParams { RecoveryValue = 1 };
    var system = (PSystemManager)RuntimeHelpers.GetUninitializedObject(typeof(PSystemManager));
    typeof(PSystemManager).GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, system);
    typeof(PSystemManager).GetField("<localBodies>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
        .SetValue(system, new List<CelestialBody> { body });
    var template = KerbalCloning.Template(source, "New Kerman", "Kerbin");
    var log = new FlightLog(); log.Load(template.GetNode("CAREER_LOG"));
    Check(KerbalRoster.CalculateExperience(new[] { log }) == 27.5f);
    // Simulate a fresh session by removing registration caches, then use production load registration.
    var registered = (HashSet<string>)typeof(KerbalCloning).GetField("registered", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    registered.Clear();
    foreach (string field in new[] { "xpHomeTypes", "xpHomeTypeNames", "xpHomeValues" })
        ((System.Collections.IList)typeof(KerbalRoster).GetField(field, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Clear();
    Check(KerbalRoster.CalculateExperience(new[] { log }) == 0);
    KerbalCloning.RegisterCrewTraining(template);
    Check(KerbalRoster.CalculateExperience(new[] { log }) == 27.5f);
    Check(KerbalRoster.CalculateExperienceLevel(27.5f) == source.experienceLevel);
});
Test("MKS profession identifiers remain exact and zero-XP clones have no inherited career entries", () => {
    foreach (string trait in new[] { "Miner", "Technician", "Kolonist", "Mechanic", "Civilian" }) {
        source.trait = trait; source.experience = 0;
        var clone = KerbalCloning.Template(source, "New Kerman", "Kerbin");
        Check(clone.GetValue("trait") == trait);
        var career = new FlightLog(); career.Load(clone.GetNode("CAREER_LOG")); Check(career.Count == 0);
    }
});
Console.WriteLine(passed + " production clone-template groups passed using installed KSP serialization; native creation pending.");
namespace MoarKerbals { public static class Logging { public static void DLogError(string text) => Console.WriteLine(text); } }
