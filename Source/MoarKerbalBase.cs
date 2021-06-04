using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoarKerbals
{


    public class MoarKerbalBase : PartModule
    {
        [KSPField]
        public string recipeIngredients = "";
        [KSPField]
        public string recipeAmounts = "";
        [KSPField]
        public string initiateAction = "Initiate Kuddling!";



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
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;
            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * gblMult;

                //Log.dbg(String.Format("GlobalScale: {#.##}", gblMult));
                double available = part.RequestResource(resourceRequired[i].Resource.id, amtRequired);
                //debug:
                //string dStr;
                //dStr = " DEBUG: needs: " + resourceRequired[i].resource + "\n\nresourceAmounts: " + available.ToString() + "\n\nneed: " + amtRequired.ToString();
                //Utilities.msg(dStr, 5f, ScreenMessageStyle.UPPER_LEFT);

                //Log.dbg(String.Format(" DEBUG: {1} : resourceAmounts: {2} need: {3}", resList[i].ToString(),  available.ToString(), resourceAmounts[i].ToString()));
                if (available < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    part.RequestResource(resourceRequired[i].Resource.id, -available);
                    for (int j = 0; j < i; j++)
                        part.RequestResource(resourceRequired[j].Resource.id, -amtRequired);

                    //ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + (resourceAmounts[i] * gblMult).ToString() + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Utilities.msg("Insufficient " + resourceRequired[i].resource + " to start Kloning (" + available.ToString() + "/" + amtRequired.ToString() + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
            }
            return true;
        }


    }
}
