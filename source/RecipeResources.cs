namespace MoarKerbals
{
    // Keep preflight, payment and refunds on exactly the same stock resource-flow policy.
    internal sealed class RecipeResources
    {
        private readonly Part part;
        public RecipeResources(Part part) { this.part = part; }

        public void Totals(string name, out double available, out double capacity)
        {
            var definition = PartResourceLibrary.Instance.GetDefinition(name);
            part.GetConnectedResourceTotals(definition.id, ResourceFlowMode.ALL_VESSEL, out available, out capacity);
        }

        public double Request(string name, double amount)
        {
            var definition = PartResourceLibrary.Instance.GetDefinition(name);
            return part.RequestResource(definition.id, amount, ResourceFlowMode.ALL_VESSEL);
        }
    }
}
