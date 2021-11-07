namespace MoarKerbals
{
    /// <summary>ResourceRequired class</summary>
    public class ResourceRequired
    {
        /// <summary>string resource</summary>
        public string resource;
        /// <summary>double amount</summary>
        public double amount;
        /// <summary>PartResourceDefinition Resource</summary>
        public PartResourceDefinition Resource;

        /// <summary>Initializes a new instance of the <see cref="ResourceRequired"/> class.</summary>
        /// <param name="resource">The resource.</param>
        /// <param name="amount">The amount.</param>
        public ResourceRequired(string resource, double amount)
        {
            this.resource = resource;
            this.amount = amount;
            Resource = PartResourceLibrary.Instance.GetDefinition(resource);

        }
    }
}
