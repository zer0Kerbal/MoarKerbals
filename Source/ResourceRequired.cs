using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoarKerbals
{
    public class ResourceRequired
    {
        public string resource;
        public double amount;
        public PartResourceDefinition Resource;

        public ResourceRequired(string resource, double amount)
        {
            this.resource = resource;
            this.amount = amount;
            Resource = PartResourceLibrary.Instance.GetDefinition(resource);

        }
    }
}
