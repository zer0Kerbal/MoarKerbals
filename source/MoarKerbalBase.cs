using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSP.Localization;

namespace MoarKerbals
{
    public class MoarKerbalBase : PartModule
    {
        /// <summary>The recipe ingredients</summary>
        [KSPField]
        public string recipeIngredients = "";

        /// <summary>The recipe amounts</summary>
        [KSPField]
        public string recipeAmounts = "";

        /// <summary>The initiate action</summary>
        [KSPField]
        public string initiateAction = Localizer.Format("#MOAR-Base-01");

        internal List<ResourceRequired> resourceRequired = new List<ResourceRequired>();

        public override void OnStart(PartModule.StartState state)
        {
            if (recipeIngredients != "" && recipeAmounts != "")
            {
                var resourceList = recipeIngredients.Split(',');
                var str_amounts = recipeAmounts.Split(',');
                var resourceAmounts = new double[str_amounts.Length];
                for (int i = 0; i < resourceList.Length; i++)
                {
                    resourceList[i] = resourceList[i].Trim();
                    resourceAmounts[i] = double.Parse(str_amounts[i]);

                    resourceRequired.Add(new ResourceRequired(resourceList[i], resourceAmounts[i]));
                }
            }
        }

        internal bool GatherResources(Part part)
        {
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;
            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * gblMult;

                Logging.DLog(String.Format("GlobalScale: {#.##}", gblMult));
                double available = part.RequestResource(resourceRequired[i].Resource.id, amtRequired);

                Logging.DLog(String.Format("DEBUG: {1} : resourceAmounts: {2} need: {3}", resourceRequired[i].resource, available, amtRequired));
                if (available < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    part.RequestResource(resourceRequired[i].Resource.id, -available);
                    for (int j = 0; j < i; j++)
                        part.RequestResource(resourceRequired[j].Resource.id, -amtRequired);

                    //Logging.Msg("Insufficient " + resourceRequired[i].resource + " to start Kloning (" + available.ToString() + "/" + amtRequired.ToString() + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Base-02", resourceRequired[i].resource, available, amtRequired), 5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
            }
            return true;
        }
    }
}
