using System.Globalization;
using MoarKerbals.Core;

int passed = 0;
void Test(string name, Action test) { test(); Console.WriteLine("PASS " + name); passed++; }
void Check(bool result) { if (!result) throw new Exception("Assertion failed."); }
void Equal(double expected, double actual) { if (Math.Abs(expected - actual) > 1e-7) throw new Exception($"Expected {expected}, got {actual}."); }
void Reject(Action action) { try { action(); } catch (FormatException) { return; } throw new Exception("Invalid recipe accepted."); }
Test("gender markers decorate only crew headings and remain idempotent", () => {
    Check(GenderLabels.Pair("Malcolm", "Megdrin") == "Malcolm \u2642 + Megdrin \u2640");
    Check(GenderLabels.Heading("Ann (17 L - 0.045 t)", "Ann", false) == "Ann \u2640 (17 L - 0.045 t)");
    Check(GenderLabels.Heading("Ann \u2640 (17 L - 0.045 t)", "Ann", false) == "Ann \u2640 (17 L - 0.045 t)");
    Check(GenderLabels.Heading("Anna (17 L)", "Ann", true) == "Anna (17 L)");
    Check(GenderLabels.Heading("Ann's inventory", "Ann", false) == "Ann's inventory");
});
Test("procedural symbol glyphs contain rings and the correct cross or arrow", () => {
    Check(GenderLabels.Distance(false, 28, 47) > 0 && GenderLabels.Distance(true, 28, 47) > 0);
    Check(GenderLabels.Distance(false, 28, 61) < 0 && GenderLabels.Distance(true, 28, 61) < 0);
    Check(GenderLabels.Distance(false, 28, 10) < 0 && GenderLabels.Distance(true, 28, 10) > 0);
    Check(GenderLabels.Distance(false, 18, 19) < 0 && GenderLabels.Distance(true, 18, 19) > 0);
    Check(GenderLabels.Distance(true, 53, 72) < 0 && GenderLabels.Distance(false, 53, 72) > 0);
});
var recipe = Rules.ParseRecipe("ElectricCharge,Oxidizer,Ore", "8000,100,150");
Dictionary<string, double> Stock(double ore = 500) => new() { ["ElectricCharge"] = 16000, ["Oxidizer"] = 500, ["Ore"] = ore };
double Request(Dictionary<string, double> stock, string name, double amount)
{
    double actual = amount > 0 ? Math.Min(stock[name], amount) : amount;
    stock[name] -= actual;
    return actual;
}
Test("volume capacity fits three families at 3.75m without rounding up", () => {
    Equal(3, Rules.VolumeCapacity(3, 2.5, 2.5));
    Equal(10, Rules.VolumeCapacity(3, 3.75, 2.5));
    Equal(24, Rules.VolumeCapacity(3, 5, 2.5));
    Equal(2, Rules.VolumeCapacity(3, 2.49, 2.5));
    Equal(1, Rules.VolumeCapacity(3, 0.1, 2.5));
    foreach (double scale in new[] { 0.0, -1, double.NaN, double.PositiveInfinity, 100 })
    {
        bool rejected = false;
        try { Rules.VolumeCapacity(3, scale, 2.5); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected);
    }
});
Test("shrinking keeps sparse occupied seat indices until crew leave", () => {
    Equal(10, Rules.PreserveOccupiedSeats(3, new[] { 0, 9 }));
    Equal(6, Rules.PreserveOccupiedSeats(3, new[] { -1, 5, 1 }));
    Equal(3, Rules.PreserveOccupiedSeats(3, new[] { 0, 1 }));
    Equal(3, Rules.PreserveOccupiedSeats(3, Array.Empty<int>()));
});
Test("successful birth consumes one recipe and one currency payment", () => {
    var stock = Stock(); int operations = 0, debits = 0;
    Check(Rules.Transact(recipe, 1, () => true, (n, a) => Request(stock, n, a), () => { operations++; return true; }, () => debits++));
    Equal(8000, stock["ElectricCharge"]); Equal(400, stock["Oxidizer"]); Equal(350, stock["Ore"]); Check(operations == 1 && debits == 1);
});
Test("failure at final ingredient refunds each different earlier amount exactly", () => {
    var stock = Stock(37); bool touched = false;
    Check(!Rules.Transact(recipe, 1, () => true, (n, a) => Request(stock, n, a), () => { touched = true; return true; }, () => touched = true));
    Equal(16000, stock["ElectricCharge"]); Equal(500, stock["Oxidizer"]); Equal(37, stock["Ore"]); Check(!touched);
});
Test("insufficient currency never touches resources or crew", () => {
    Check(!Rules.Transact(recipe, 1, () => false, (n, a) => throw new Exception("Resource touched"), () => throw new Exception("Crew touched"), () => throw new Exception("Currency touched")));
});
Test("failed crew insertion refunds resources and takes no currency", () => {
    var stock = Stock(); int debits = 0;
    Check(!Rules.Transact(recipe, 1, () => true, (n, a) => Request(stock, n, a), () => false, () => debits++));
    Equal(16000, stock["ElectricCharge"]); Equal(500, stock["Oxidizer"]); Equal(500, stock["Ore"]); Check(debits == 0);
});
Test("exception during failed operation refunds resources", () => {
    var stock = Stock();
    try { Rules.Transact(recipe, 1, () => true, (n, a) => Request(stock, n, a), () => throw new InvalidOperationException(), () => throw new Exception("Currency touched")); }
    catch (InvalidOperationException) { }
    Equal(16000, stock["ElectricCharge"]); Equal(500, stock["Oxidizer"]); Equal(500, stock["Ore"]);
});
Test("multiplier applies to each resource once", () => {
    var stock = Stock(); Check(Rules.Transact(recipe, .5, () => true, (n, a) => Request(stock, n, a), () => true, () => {}));
    Equal(12000, stock["ElectricCharge"]); Equal(450, stock["Oxidizer"]); Equal(425, stock["Ore"]);
});
Test("invalid multipliers cannot create or consume resources", () => {
    foreach (double multiplier in new[] { -1d, double.NaN, double.PositiveInfinity })
        Check(!Rules.Transact(recipe, multiplier, () => throw new Exception(), (n, a) => throw new Exception(), () => throw new Exception(), () => throw new Exception()));
});
Test("empty recipe still requires currency affordability", () => {
    Check(!Rules.Transact(Array.Empty<Ingredient>(), 1, () => false, (n, a) => throw new Exception(), () => throw new Exception(), () => throw new Exception()));
});
Test("exact currency balances and zero-cost operations are accepted", () => {
    Check(Rules.CanAfford(5000, 5000)); Check(Rules.CanAfford(0, 0)); Check(Rules.CanAfford(-20, 0)); Check(!Rules.CanAfford(4999, 5000)); Check(!Rules.CanAfford(100, -1));
});
Test("last seat is available even when a source Kerbal is required", () => Check(Rules.HasRoom(1, 2, true)));
Test("full and overfull cabins are rejected", () => { Check(!Rules.HasRoom(2, 2, true)); Check(!Rules.HasRoom(3, 2, false)); });
Test("source requirement is independent of free capacity", () => { Check(!Rules.HasRoom(0, 2, true)); Check(Rules.HasRoom(0, 1, false)); Check(!Rules.HasRoom(0, 0, false)); });
Test("zero accident probability always follows the successful path", () => {
    for (int i = 0; i < 100; i++) Check(!Rules.Accident(0, i / 100d));
    Check(Rules.Accident(1, .999)); Check(Rules.Accident(.15, .149)); Check(!Rules.Accident(.15, .15));
});
Test("orbital-only and surface-only facilities respect launchpad, water and orbit independently", () => {
    foreach (string surface in new[] { "PRELAUNCH", "LANDED", "SPLASHED" })
    {
        Check(!Rules.SituationAllowed(surface, true, false, true));
        Check(Rules.SituationAllowed(surface, true, true, false));
    }
    Check(Rules.SituationAllowed("ORBITING", true, false, true));
    Check(!Rules.SituationAllowed("ORBITING", true, true, false));
    Check(!Rules.SituationAllowed("FLYING", true, true, true));
    Check(Rules.SituationAllowed("PRELAUNCH", false, false, false));
});
Test("recipe parsing is invariant under comma-decimal locales", () => {
    var before = CultureInfo.CurrentCulture;
    try { CultureInfo.CurrentCulture = new CultureInfo("de-DE"); Equal(14.5, Rules.ParseRecipe("Water", "14.5")[0].Amount); }
    finally { CultureInfo.CurrentCulture = before; }
});
Test("empty and duplicate recipes are handled deterministically", () => {
    Check(Rules.ParseRecipe("", "").Length == 0); var parsed = Rules.ParseRecipe(" Ore,Ore ", "2,3"); Check(parsed.Length == 1); Equal(5, parsed[0].Amount);
});
Test("malformed, missing, negative and nonfinite amounts are rejected", () => {
    Reject(() => Rules.ParseRecipe("A,B", "1")); Reject(() => Rules.ParseRecipe("A", "-1")); Reject(() => Rules.ParseRecipe("A", "NaN"));
    Reject(() => Rules.ParseRecipe("A", "Infinity")); Reject(() => Rules.ParseRecipe("", "1")); Reject(() => Rules.ParseRecipe("A", ""));
});
Test("MKS professions are retained, invalid and duplicate identifiers excluded", () => {
    var known = new HashSet<string> { "Pilot", "Engineer", "Scientist", "Kolonist", "Miner", "Technician", "Mechanic", "Biologist", "Geologist", "Farmer", "Medic", "Quartermaster", "Scout", "Civilian" };
    string config = string.Join(",", known) + ",Pilot,Unknown, Farmer "; var traits = Rules.Traits(config, known.Contains);
    Check(traits.Length == 13 && traits.Contains("Farmer") && traits.Contains("Kolonist") && !traits.Contains("Civilian") && !traits.Contains("Unknown"));
});
Test("standalone list excludes missing MKS professions safely", () => {
    var traits = Rules.Traits("Pilot,Farmer,Scientist", n => n == "Pilot" || n == "Scientist"); Check(traits.SequenceEqual(new[] { "Pilot", "Scientist" }));
});
Test("disabled reproduction pauses and removal of parents resets progress", () => {
    Equal(60, Rules.Advance(60, 100, 360, false, true)); Equal(0, Rules.Advance(60, 100, 360, true, false));
});
Test("progress cannot exceed readiness or advance backwards after a revert", () => {
    Equal(360, Rules.Advance(350, 10000, 360, true, true)); Equal(100, Rules.Advance(100, -50, 360, true, true));
});
if (args.Length >= 2)
{
    string repoRoot = args[0], kspRoot = args[1];
    Test("installed stock EVA default loadouts provide a parachute and jetpack", () => {
        int loadouts = 0;
        foreach (string path in Directory.GetFiles(Path.Combine(kspRoot, "GameData", "Squad", "Parts", "Prebuilt"), "kerbalEVA*.cfg"))
            foreach (var part in Config.Read(path).Children.Where(n => n.Name == "PART"))
                foreach (var inventory in part.Children.Where(n => n.Name == "MODULE" && n.Values.GetValueOrDefault("name") == "ModuleInventoryPart"))
                    foreach (var defaults in inventory.Children.Where(n => n.Name == "DEFAULTPARTS"))
                    {
                        // The small Config reader stores one value per key, so inspect repeated name entries directly.
                        string text = File.ReadAllText(path);
                        Check(text.Contains("name = evaChute") && text.Contains("name = evaJetpack"));
                        loadouts++;
                    }
        Check(loadouts > 0);
    });
    Test("compiled resource audit covers all three ability modules and rejects bypass paths", () =>
        ResourceAudit.Verify(args.Length > 2 ? args[2] : Path.Combine(repoRoot, "artifacts", "Release", "MoarKerbals.dll")));
    string compatibility = Path.Combine(repoRoot, "GameData", "KerbthulhuKineticsProgram", "MoarKerbals", "Compatibility");
    var mksPatch = Config.Read(Path.Combine(compatibility, "MKS.cfg"));
    Test("all addon configuration trees parse", () => {
        foreach (var file in Directory.GetFiles(Path.Combine(repoRoot, "GameData"), "*.cfg", SearchOption.AllDirectories)) Config.Read(file);
    });
    Test("MKS recruitment list matches the ten professions in the actual installation", () => {
        var installed = Config.Read(Path.Combine(kspRoot, "GameData", "UmbraSpaceIndustries", "MKS", "Kolonists.cfg"));
        var names = installed.Children.Where(n => n.Name == "EXPERIENCE_TRAIT").Select(n => n.Values["name"]).ToArray();
        string configured = mksPatch.Children[1].Children.Single().Values["%recruitmentTraits"];
        var actual = Rules.Traits(configured, _ => true);
        Check(names.Length == 10 && actual.Length == 13 && names.All(actual.Contains));
    });
    Test("Akademy patch adds one guarded recruitment module and does not remove MKS training", () => {
        var akademy = mksPatch.Children[0];
        Check(akademy.Name.Contains("Tundra_Akademy") && akademy.Name.Contains("!MODULE[KerbalRecruitment]") && akademy.Name.Contains("NEEDS[MoarKerbals,KolonyTools]"));
        Check(akademy.Children.Count == 1 && akademy.Children[0].Values["name"] == "KerbalRecruitment");
        var part = Config.Read(Path.Combine(kspRoot, "GameData", "UmbraSpaceIndustries", "MKS", "Parts", "Tundra_Akademy.cfg")).Children.Single();
        Check(part.Children.Any(n => n.Name == "MODULE" && n.Values.GetValueOrDefault("name") == "SpaceAcademy"));
    });
    Test("FreeIVA prevents both halves of the six-seat lab patch", () => {
        var lab = Config.Read(Path.Combine(compatibility, "MobileScienceLab.cfg"));
        Check(lab.Children.Count == 2 && lab.Children.All(n => n.Name.Contains("!FreeIva")));
    });
    Test("all recipe resources exist in the installed stock or community definitions", () => {
        var stock = Config.Read(Path.Combine(kspRoot, "GameData", "Squad", "Resources", "ResourcesGeneric.cfg"));
        var resourceNames = stock.Children.Where(n => n.Name == "RESOURCE_DEFINITION").Select(n => n.Values["name"]).ToHashSet();
        foreach (var file in new[] { "Academy.cfg", "KloneKabin.cfg", "KuddleShack.cfg", "MKS.cfg" })
        {
            var config = Config.Read(Path.Combine(compatibility, file));
            foreach (var node in config.All().Where(n => n.Values.ContainsKey("recipeIngredients")))
                foreach (var ingredient in Rules.ParseRecipe(node.Values["recipeIngredients"], node.Values["recipeAmounts"]))
                    Check(resourceNames.Contains(ingredient.Name));
        }
    });
    Test("release metadata consistently targets KSP 1.12.5", () => {
        using var version = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "MoarKerbals.version")));
        Check(version.RootElement.GetProperty("VERSION").GetProperty("MINOR").GetInt32() == 3);
        Check(version.RootElement.GetProperty("KSP_VERSION_MAX").GetProperty("PATCH").GetInt32() == 5);
    });
    string partsRoot = Path.Combine(kspRoot, "GameData", "KerbthulhuKineticsProgram", "MoarKerbalsParts");
    if (Directory.Exists(partsRoot))
    {
        var fixes = Config.Read(Path.Combine(compatibility, "MoarKerbalsParts.cfg"));
        Test("parts scaling fix modifies the obsolete type without adding modules or changing size choices", () => {
            var patch = fixes.Children[0];
            Check(patch.Name.Contains("AFTER[MoarKerbalsParts]") && patch.Name.Contains("TweakScale"));
            var module = patch.Children.Single();
            Check(module.Name == "@MODULE[TweakScale]:HAS[#type[KW250]]" && module.Values.Count == 1 && module.Values["@type"] == "stack");
            string scaleDefinitions = File.ReadAllText(Path.Combine(kspRoot, "GameData", "TweakScale", "patches", "010_ScaleTypes.cfg"));
            Check(System.Text.RegularExpressions.Regex.IsMatch(scaleDefinitions, @"SCALETYPE\s*\{\s*name\s*=\s*stack\s"));
            var old = Config.Read(Path.Combine(partsRoot, "Compatibility", "TweakScale.cfg")).Children.Single().Children.Single();
            Check(old.Values["defaultScale"] == "2.5" && old.Values["scaleFactors"] == "2.5, 3.75" && old.Values["freeScale"] == "false");
        });
        Test("parts KuddleShack declares surface and orbital operation", () => {
            var module = fixes.Children[1].Children.Single();
            Check(module.Name == "@MODULE[KuddleShack]" && module.Values["%allowOrbital"] == "true" && module.Values["%allowSplashedOrLanded"] == "true");
        });
        Test("expanded KuddleShack reserves matching KIS and IVA seats", () => {
            var patch = fixes.Children[3];
            Check(patch.Name.Contains("moar-kuddleshack") && patch.Values["@CrewCapacity"] == "10");
            var exponents = patch.Children.Single(n => n.Name == "@MODULE[TweakScale]").Children.Single();
            Check(exponents.Name == "%TWEAKSCALEEXPONENTS[Part]" && exponents.Values["%name"] == "Part" && exponents.Values["%CrewCapacity"] == "0");
            var cabin = Config.Read(Path.Combine(partsRoot, "Parts", "moar-kuddleshack.cfg")).Children.Single();
            int seats = int.Parse(cabin.Values["CrewCapacity"]);
            var iva = Config.Read(Path.Combine(partsRoot, "Spaces", "KuddleShack", "KuddleShack.cfg")).Children.Single();
            Check(seats == 3 && iva.Children.Count(n => n.Values.GetValueOrDefault("name") == "InternalSeat") == seats);
            var expanded = fixes.Children.Single(n => n.Values.GetValueOrDefault("@name") == "MoarKuddleShackExpandedInternal");
            var added = expanded.Children.Where(n => n.Name == "PROP").ToArray();
            Check(added.Length + seats == 10 && added.Select(n => n.Values["name"]).Distinct().Count() == 7);
            Check(patch.Children.Single(n => n.Name == "@INTERNAL").Values["@name"] == expanded.Values["@name"]);
            var controller = patch.Children.Single(n => n.Name == "MODULE");
            Check(controller.Values["name"] == "ModuleMoarScaledCrew" && controller.Values["maximumSeats"] == "10");
            foreach (var prop in added)
                Check(fixes.Children.Any(n => n.Values.GetValueOrDefault("name") == prop.Values["name"] || n.Values.GetValueOrDefault("@name") == prop.Values["name"]));
            Check(File.Exists(Path.Combine(kspRoot, "GameData", "Squad", "Props", "PropsGeneric", "Seat_Passenger.mu")));
        });
        Test("all four scaled cloning and school cabins reserve six KIS inventories and real IVA seats", () => {
            foreach (var entry in new[] {
                ("moar-klonebay-ground", "KloneBay", "KloneBay.cfg"),
                ("moar-klonebay-orbital", "KloneBay", "KloneBay_Orbital.cfg"),
                ("moar-kloneschool", "School", "School.cfg"),
                ("moar-kloneschool-orbital", "School", "School_Orbital.cfg") })
            {
                var patch = fixes.Children.Single(n => n.Name.StartsWith("@PART[" + entry.Item1 + "]:HAS"));
                var controller = patch.Children.Single(n => n.Name == "MODULE");
                Check(patch.Values["@CrewCapacity"] == "6" && controller.Values["nominalSeats"] == "2"
                    && controller.Values["maximumSeats"] == "6" && controller.Values["lastKnownScale"] == "2.5");
                Equal(2, Rules.VolumeCapacity(2, 2.5, 2.5));
                Equal(6, Rules.VolumeCapacity(2, 3.75, 2.5));
                var exponents = patch.Children.Single(n => n.Name == "@MODULE[TweakScale]").Children.Single();
                Check(exponents.Values["%CrewCapacity"] == "0");
                var original = Config.Read(Path.Combine(partsRoot, "Spaces", entry.Item2, entry.Item3)).Children.Single();
                var expanded = fixes.Children.Single(n => n.Name.StartsWith("+INTERNAL[") &&
                    n.Values.GetValueOrDefault("@name") == patch.Children.Single(c => c.Name == "@INTERNAL").Values["@name"]);
                var added = expanded.Children.Where(n => n.Name == "PROP").ToArray();
                Check(original.Children.Count(n => n.Values.GetValueOrDefault("name") == "InternalSeat") + added.Length == 6);
                var indices = added.Select(p => fixes.Children.Single(n =>
                    n.Values.GetValueOrDefault("name") == p.Values["name"] || n.Values.GetValueOrDefault("@name") == p.Values["name"]))
                    .Select(p => p.Children.Single(n => n.Name.Contains("MODULE")).Values.First(v => v.Key.EndsWith("capacityIndex")).Value);
                Check(indices.SequenceEqual(new[] { "3", "4", "5" }));
            }
        });
        Test("both dedicated schools receive documented upstream costs without replacing an existing recipe", () => {
            foreach (string name in new[] { "moar-kloneschool.cfg", "moar-kloneschool-orbital.cfg" })
            {
                var school = Config.Read(Path.Combine(partsRoot, "Parts", name)).Children.Single();
                var module = school.Children.Single(n => n.Name == "MODULE" && n.Values.GetValueOrDefault("name") == "KerbalRecruitment");
                Check(module.Values["recruitmentTraits"] == "Pilot,Engineer,Scientist");
            }
            var patch = fixes.Children[2];
            Check(patch.Name.Contains("moar-kloneschool,moar-kloneschool-orbital"));
            var recipe = patch.Children[0];
            Check(recipe.Name.Contains("~recipeIngredients[*],~recipeAmounts[*]"));
            Check(recipe.Values["recipeIngredients"] == "ElectricCharge,LiquidFuel,Ore");
            var values = Rules.ParseRecipe(recipe.Values["recipeIngredients"], recipe.Values["recipeAmounts"]);
            Check(values.Select(i => i.Amount).SequenceEqual(new[] { 5000d, 50d, 50d }));
            var costs = patch.Children[1].Values;
            Check(costs["%costFunds"] == "5000" && costs["%costScience"] == "1" && costs["%costReputation"] == "2");
        });
    }
}
Console.WriteLine($"{passed} regression groups passed.");

// Small offline reader for the ordinary ConfigNode layout used by these configuration files.
sealed class Config
{
    public string Name = "";
    public Dictionary<string, string> Values = new();
    public List<Config> Children = new();
    public IEnumerable<Config> All() { yield return this; foreach (var child in Children) foreach (var n in child.All()) yield return n; }
    public static Config Read(string path)
    {
        var root = new Config(); var stack = new Stack<Config>(); stack.Push(root); string header = "";
        foreach (string raw in File.ReadLines(path))
        {
            string line = raw.Split(new[] { "//" }, 2, StringSplitOptions.None)[0].Trim();
            if (line.Length == 0) continue;
            if (line == "{")
            {
                if (header.Length == 0) throw new Exception("Missing node header: " + path);
                var child = new Config { Name = header }; stack.Peek().Children.Add(child); stack.Push(child); header = "";
            }
            else if (line == "}") { if (stack.Count == 1 || header.Length != 0) throw new Exception("Unbalanced node: " + path); stack.Pop(); }
            else if (line.Contains('=')) { var pair = line.Split('=', 2); stack.Peek().Values[pair[0].Trim()] = pair[1].Trim(); }
            else { if (header.Length != 0) throw new Exception("Unopened node: " + path); header = line; }
        }
        if (stack.Count != 1 || header.Length != 0) throw new Exception("Unclosed node: " + path);
        return root;
    }
}
