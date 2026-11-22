// Actual installed KSP ConfigNode and GameParameters custom-node load/save methods.
// Unity startup/gameplay is not hosted; only the settings migration and serialization path is exercised.
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MoarKerbals;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
// Disable Unity's native log sink in this managed-only host; stock warnings still run their branches.
UnityEngine.Debug.unityLogger.logEnabled = false;
AssemblyLoader.loadedAssemblies = new AssemblyLoader.LoadedAssembyList();
int passed = 0;
var types = new[] { typeof(LegacyFixture.Settings), typeof(FilterFixture.Settings),
    typeof(MoarKerbalsGeneralSettings), typeof(MoarKerbalsGameplaySettings), typeof(MoarKerbalsDisplaySettings) };
var customField = typeof(GameParameters).GetField("customParams", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
var loadMethod = typeof(GameParameters).GetMethod("LoadCustomNode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
void Check(bool condition) { if (!condition) throw new Exception("Settings assertion failed"); }
void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
Game Create(ConfigNode parameters, bool wrapper = false)
{
    GameParameters.ParameterTypes = types.ToList();
    var settings = (GameParameters)RuntimeHelpers.GetUninitializedObject(typeof(GameParameters));
    customField.SetValue(settings, types.ToDictionary(t => t, t => (GameParameters.CustomParameterNode)Activator.CreateInstance(t)));
    foreach (ConfigNode node in parameters.nodes) loadMethod.Invoke(settings, new object[] { node });
    var game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
    game.Parameters = settings;
    var config = new ConfigNode("GAME"); config.AddNode(parameters);
    if (wrapper) { var root = new ConfigNode(); root.AddNode(config); game.config = root; }
    else game.config = config;
    return game;
}
ConfigNode Legacy(int minutes = 20)
{
    var root = new ConfigNode("PARAMETERS");
    var shadows = root.AddNode("Settings"); shadows.AddValue("EnableOrbitShadows", "True");
    var filter = root.AddNode("Settings"); filter.AddValue("hideUnpurchased", "True");
    var general = root.AddNode("Settings"); general.AddValue("globalKloningCostMultiplier", "1.25");
    general.AddValue("kuddleTimeNeeded", minutes.ToString()); general.AddValue("soundVolume", "0.4");
    var gameplay = root.AddNode("Settings2"); gameplay.AddValue("kloneCivilians", "False");
    gameplay.AddValue("kuddleCivilians", "False"); gameplay.AddValue("soundOn", "False");
    var display = root.AddNode("Settings3"); display.AddValue("coloredPAW", "False"); display.AddValue("quietMode", "True");
    return root;
}
ConfigNode Save(Game game)
{
    var node = new ConfigNode("PARAMETERS");
    foreach (var kv in (Dictionary<Type, GameParameters.CustomParameterNode>)customField.GetValue(game.Parameters))
        kv.Value.Save(node.AddNode(kv.Key.Name));
    return node;
}
Test("clone-source difficulty option defaults off and persists through actual KSP save/quickload", () => {
    var game = Create(new ConfigNode("PARAMETERS"));
    Check(!game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes);
    game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes = true;
    for (int i = 0; i < 3; i++) {
        game = Create(Save(game));
        Check(game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes);
    }
    game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes = false;
    Check(!Create(Save(game)).Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes);
});
Test("actual KSP short-name loader reproduces collision despite stored 20-minute value", () => {
    var game = Create(Legacy());
    Check(game.Parameters.CustomParams<LegacyFixture.Settings>().kuddleTimeNeeded == 360);
    Check(game.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == 360);
    Check(game.Parameters.CustomParams<FilterFixture.Settings>().hideUnpurchased);
});
Test("all three legacy pages migrate by Moar-specific markers without modifying foreign sections", () => {
    var node = Legacy(); string before = string.Join("\n", node.GetNodes("Settings").Select(n => n.ToString()));
    var game = Create(node); Check(SettingsPersistence.Migrate(game) == 3);
    var general = game.Parameters.CustomParams<MoarKerbalsGeneralSettings>();
    Check(general.kuddleTimeNeeded == 20 && general.globalKloningCostMultiplier == 1.25 && Math.Abs(general.soundVolume - 0.4) < 1e-6);
    var gameplay = game.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
    Check(!gameplay.kloneCivilians && !gameplay.kuddleCivilians && !gameplay.soundOn);
    Check(game.Parameters.CustomParams<MoarKerbalsDisplaySettings>().quietMode && !game.Parameters.CustomParams<MoarKerbalsDisplaySettings>().coloredPAW);
    Check(before == string.Join("\n", node.GetNodes("Settings").Select(n => n.ToString())));
    Check(!node.GetNode(nameof(MoarKerbalsGeneralSettings)).HasValue("EnableOrbitShadows"));
});
Test("actual KSP save/load round-trip retains unique settings through repeated quickloads", () => {
    var game = Create(Legacy()); SettingsPersistence.Migrate(game);
    game.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded = 70;
    game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().recruitOnlyOne = false;
    for (int i = 0; i < 3; i++)
    {
        game = Create(Save(game)); Check(SettingsPersistence.Migrate(game) == 0);
        Check(game.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == 70);
        Check(!game.Parameters.CustomParams<MoarKerbalsGameplaySettings>().recruitOnlyOne);
        Check(game.Parameters.CustomParams<MoarKerbalsDisplaySettings>().quietMode);
        Check(game.Parameters.CustomParams<FilterFixture.Settings>().hideUnpurchased);
    }
});
Test("unique nodes take precedence over stale legacy nodes and migration is idempotent", () => {
    var node = Legacy(); var unique = node.AddNode(nameof(MoarKerbalsGeneralSettings)); unique.AddValue("kuddleTimeNeeded", "90");
    var game = Create(node); Check(SettingsPersistence.Migrate(game) == 2);
    var page = game.Parameters.CustomParams<MoarKerbalsGeneralSettings>(); Check(page.kuddleTimeNeeded == 90);
    page.kuddleTimeNeeded = 80; Check(SettingsPersistence.Migrate(game) == 0 && page.kuddleTimeNeeded == 80);
    Check(node.GetNodes(nameof(MoarKerbalsGeneralSettings)).Length == 1);
});
Test("loading an older quicksave uses that snapshot rather than settings from another game", () => {
    var current = Create(Legacy(20)); SettingsPersistence.Migrate(current);
    var older = Create(Legacy(360), true); SettingsPersistence.Migrate(older);
    Check(current.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == 20);
    Check(older.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == 360);
});
Test("ambiguous legacy pages are skipped and marker-free Settings nodes are not imported", () => {
    var node = Legacy(); var duplicate = node.AddNode("Settings");
    duplicate.AddValue("kuddleTimeNeeded", "1"); duplicate.AddValue("globalKloningCostMultiplier", "2");
    var game = Create(node); Check(SettingsPersistence.Migrate(game) == 2);
    Check(game.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == 360);
    Check(!node.HasNode(nameof(MoarKerbalsGeneralSettings)));
    var foreign = new ConfigNode("PARAMETERS"); foreign.AddNode("Settings").AddValue("hideUnpurchased", "True");
    Check(SettingsPersistence.Migrate(Create(foreign)) == 0);
});
Test("new games and absent config are harmless and page names are distinct", () => {
    var game = Create(new ConfigNode("PARAMETERS")); Check(SettingsPersistence.Migrate(game) == 0);
    game.config = null; Check(SettingsPersistence.Migrate(game) == 0 && SettingsPersistence.Migrate(null) == 0);
    Check(types.Skip(2).Select(t => t.Name).Distinct().Count() == 3);
    Check(types.Skip(2).All(t => t.Name.StartsWith("MoarKerbals") && t.IsPublic));
});
Test("renamed pages retain their original difficulty UI localization keys", () => {
    var pages = new GameParameters.CustomParameterNode[] { new MoarKerbalsGeneralSettings(),
        new MoarKerbalsGameplaySettings(), new MoarKerbalsDisplaySettings() };
    var titleField = typeof(GameParameters.CustomParameterUI).GetField("_title", BindingFlags.Instance | BindingFlags.NonPublic);
    for (int i = 0; i < pages.Length; i++)
    {
        Check(pages[i].Title == "#MOAR-Settings-Title-" + (i + 1));
        foreach (var field in pages[i].GetType().GetFields())
            foreach (var attribute in field.GetCustomAttributes<GameParameters.CustomParameterUI>())
            {
                string raw = (string)titleField.GetValue(attribute);
                Check(!raw.StartsWith("#MOAR-") || raw.StartsWith("#MOAR-Settings-"));
            }
    }
});
if (args.Length > 0)
{
    foreach (string path in args)
    {
        Test("read-only actual saved parameters migrate: " + Path.GetFileName(path), () => {
            using var reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var lines = new List<string>();
            for (int i = 0; i < 2000 && reader.ReadLine() is string line; i++) lines.Add(line);
            var match = Regex.Match(string.Join("\n", lines), @"(?ms)^\tPARAMETERS\n\t\{.*?^\t\}");
            Check(match.Success); var parsed = ConfigNode.Parse(match.Value);
            var node = parsed.GetNode("PARAMETERS"); Check(node != null);
            var savedPage = node.GetNode(nameof(MoarKerbalsGeneralSettings)) ??
                node.GetNodes("Settings").Single(n => n.HasValue("kuddleTimeNeeded") && n.HasValue("globalKloningCostMultiplier"));
            int expected = int.Parse(savedPage.GetValue("kuddleTimeNeeded"));
            int expectedMigrations = new[] { nameof(MoarKerbalsGeneralSettings), nameof(MoarKerbalsGameplaySettings),
                nameof(MoarKerbalsDisplaySettings) }.Count(name => !node.HasNode(name));
            var game = Create(node); Check(SettingsPersistence.Migrate(game) == expectedMigrations);
            Check(game.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == expected);
            var reloaded = Create(Save(game)); Check(SettingsPersistence.Migrate(reloaded) == 0);
            Check(reloaded.Parameters.CustomParams<MoarKerbalsGeneralSettings>().kuddleTimeNeeded == expected);
        });
    }
}
Console.WriteLine(passed + " settings groups passed using installed KSP load/save methods; native Unity startup pending.");

public abstract class FixtureSettings : GameParameters.CustomParameterNode
{
    public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;
    public override string Section => "Fixture";
    public override string DisplaySection => "Fixture";
    public override string Title => "Fixture";
    public override int SectionOrder => 1;
    public override bool HasPresets => false;
}
namespace LegacyFixture { public class Settings : FixtureSettings { public int kuddleTimeNeeded = 360; public double globalKloningCostMultiplier = 1; } }
namespace FilterFixture { public class Settings : FixtureSettings { public bool hideUnpurchased; } }
namespace MoarKerbals
{
    public static class Version { public const string Text = "test"; }
    public static class Logging
    {
        public static void DLog(string text, bool force = false) => Console.WriteLine(text);
        public static void DLogWarning(string text) => Console.WriteLine(text);
        public static void DLogError(string text) => Console.WriteLine(text);
    }
}
