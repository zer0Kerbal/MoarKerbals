// Execute production resource adapter/transaction code against a stock-API boundary double.
// These checks verify explicit flow-policy selection; they do not emulate KSP's live crossfeed graph.
using MoarKerbals;
using MoarKerbals.Core;

int passed = 0;
void Check(bool value) { if (!value) throw new Exception("Resource-flow assertion failed"); }
void Equal(double a, double b) { Check(Math.Abs(a - b) < 1e-8); }
void Test(string name, Action test) { test(); passed++; Console.WriteLine("PASS " + name); }
Part Base() => new() { stores = new() {
    // The larger tank is outside the stack path; the smaller tank is reachable.
    new Store { id = 1, amount = 500, maximum = 17600, stackConnected = false },
    new Store { id = 1, amount = 50, maximum = 60.5, stackConnected = true },
    new Store { id = 1, amount = 250, maximum = 500, flowEnabled = false, stackConnected = true },
    new Store { id = 2, amount = 2800, maximum = 2800, stackConnected = true },
    new Store { id = 3, amount = 675, maximum = 14400, stackConnected = false },
    new Store { id = 3, amount = 0, maximum = 49.5, stackConnected = true },
    new Store { id = 4, amount = 90, maximum = 25600, stackConnected = true }
} };
Test("vessel-wide lookup includes large tank and excludes locked tank", () => {
    var p = Base(); p.GetConnectedResourceTotals(1, ResourceFlowMode.STACK_PRIORITY_SEARCH, out var routed, out _);
    Equal(50, routed);
    new RecipeResources(p).Totals("Oxidizer", out var total, out var maximum);
    Equal(550, total); Equal(17660.5, maximum);
});
Test("one completed operation withdraws 100 Oxidizer using the displayed vessel-wide pool", () => {
    var p = Base(); var resources = new RecipeResources(p); int operations = 0, charges = 0;
    Check(Rules.Transact(Rules.ParseRecipe("Oxidizer", "100"), 1, () => true, resources.Request,
        () => { operations++; return true; }, () => charges++));
    resources.Totals("Oxidizer", out var total, out _); Equal(450, total);
    Equal(250, p.stores[2].amount); Check(operations == 1 && charges == 1);
    Check(p.requests.All(r => r.flow == ResourceFlowMode.ALL_VESSEL));
});
Test("later resource shortage refunds through the same policy without charging or creating crew", () => {
    var p = Base(); var resources = new RecipeResources(p); bool operated = false;
    Check(!Rules.Transact(Rules.ParseRecipe("Oxidizer,ElectricCharge", "100,8000"), 1, () => true,
        resources.Request, () => { operated = true; return true; }, () => operated = true));
    resources.Totals("Oxidizer", out var total, out _); Equal(550, total);
    Equal(2800, p.stores[3].amount); Check(!operated);
    Check(p.requests.Any(r => r.amount < 0) && p.requests.All(r => r.flow == ResourceFlowMode.ALL_VESSEL));
});
Test("shared resource adapter cannot be configured back to stack routing", () => {
    var constructors = typeof(RecipeResources).GetConstructors();
    Check(constructors.Length == 1 && constructors[0].GetParameters().Length == 1);
    var p = Base(); var resources = new RecipeResources(p);
    Equal(100, resources.Request("Oxidizer", 100));
    Check(p.requests.Single().flow == ResourceFlowMode.ALL_VESSEL);
});
Test("Akademy recipe draws from large LiquidFuel tank with sufficient electric charge and Ore", () => {
    var p = Base(); p.stores[3].amount = p.stores[3].maximum = 5500;
    p.GetConnectedResourceTotals(3, ResourceFlowMode.STACK_PRIORITY_SEARCH, out var routed, out _); Equal(0, routed);
    var resources = new RecipeResources(p); int operations = 0, charges = 0;
    resources.Totals("LiquidFuel", out var available, out _); Equal(675, available);
    Check(Rules.Transact(Rules.ParseRecipe("ElectricCharge,LiquidFuel,Ore", "5000,50,50"), 1, () => true,
        resources.Request, () => { operations++; return true; }, () => charges++));
    Equal(500, p.stores[3].amount); Equal(625, p.stores[4].amount);
    Equal(40, p.stores[6].amount); Check(operations == 1 && charges == 1);
    Check(p.requests.All(r => r.flow == ResourceFlowMode.ALL_VESSEL));
});
Console.WriteLine(passed + " production resource-flow groups passed (API boundary host; native KSP pending).");

public enum ResourceFlowMode { ALL_VESSEL, STACK_PRIORITY_SEARCH }
public class PartResourceDefinition { public int id; public ResourceFlowMode resourceFlowMode; }
public class PartResourceLibrary
{
    public static PartResourceLibrary Instance = new();
    public PartResourceDefinition GetDefinition(string name) => new() {
        id = name == "Oxidizer" ? 1 : name == "ElectricCharge" ? 2 : name == "LiquidFuel" ? 3 : 4,
        resourceFlowMode = name == "Oxidizer" || name == "LiquidFuel" ? ResourceFlowMode.STACK_PRIORITY_SEARCH : ResourceFlowMode.ALL_VESSEL };
}
public class Store
{
    public int id; public double amount, maximum;
    public bool flowEnabled = true, stackConnected;
}
public class Part
{
    public List<Store> stores = new();
    public List<(ResourceFlowMode flow, double amount)> requests = new();
    IEnumerable<Store> Eligible(int id, ResourceFlowMode mode) =>
        stores.Where(s => s.id == id && s.flowEnabled && (mode == ResourceFlowMode.ALL_VESSEL || s.stackConnected));
    public void GetConnectedResourceTotals(int id, ResourceFlowMode mode, out double amount, out double capacity)
    { amount = Eligible(id, mode).Sum(s => s.amount); capacity = Eligible(id, mode).Sum(s => s.maximum); }
    public double RequestResource(int id, double amount, ResourceFlowMode mode)
    {
        requests.Add((mode, amount)); double remaining = Math.Abs(amount);
        foreach (var store in Eligible(id, mode))
        {
            double moved = Math.Min(remaining, amount > 0 ? store.amount : store.maximum - store.amount);
            store.amount += amount > 0 ? -moved : moved; remaining -= moved;
        }
        return Math.Sign(amount) * (Math.Abs(amount) - remaining);
    }
}
