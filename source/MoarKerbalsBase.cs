using System;
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;

namespace MoarKerbals
{
    public class MoarKerbalsBase : PartModule
    {
        /// <summary>recipeIngreadients </summary>
        [KSPField]
        public string recipeIngredients = "";

        /// <summary>RecipeAmounts</summary>
        [KSPField]
        public string recipeAmounts = "";

        /// <summary>initiateAction (defaults to #MOAR-Base-01</summary>
        [KSPField]
        public string initiateAction = Localizer.Format("#MOAR-Base-01");

        /// <summary>costFunds (default = 0)</summary>
        [KSPField(isPersistant = false)]
        public float costFunds = 0f;

        /// <summary>costScient (default - 0) </summary>
        [KSPField(isPersistant = false)]
        public float costScience = 0f;

        /// <summary>costReputation (default = 0)</summary>
        [KSPField(isPersistant = false)]
        public float costReputation = 0f;

        /// <summary>internal list of resourcesRequired  </summary>
        internal List<ResourceRequired> resourceRequired = new List<ResourceRequired>();

        /// <summary>internal name of GUI groupName  </summary>
        internal static string groupName = Localizer.Format("#MOAR-003", Version.Text);

        /// <summary>onStart</summary>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            Logging.DLog(logMsg: "MoarKerbalsBase.OnStart");

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

        private protected void FixedUpdate()
        {
            base.OnFixedUpdate();
            // Logging.DLog(logMsg: "MoarKerbalsBase.FixedUpdate", true);
        }

        /// <summary>GatherResources: determine if required resources available and debit them from part.</summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private protected bool GatherResources(Part part)
        {
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;
            Logging.DLog(logMsg: $"MoarKerbals: Global Multiplier: {gblMult:F2}");

            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * gblMult; // uses globalMultiplier
                double available = part.RequestResource(resourceRequired[i].Resource.id, amtRequired);

                // Logging.DLog(logMsg: $"MoarKerbals: DEBUG: {1} : resourceAmounts: {2} need: {3}", resourceRequired[i].resource.ToString(), available.ToString(), amtRequired.ToString()));
                Logging.DLog(logMsg: $"MoarKerbals: {resourceRequired[i].resource} : have: {available:F2} need: {amtRequired:F2}");
                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected

                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);
                    // part.RequestResource(resourceRequired[i].Resource.id, -available, false);
                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);
                    //part.RequestResource(resourceRequired[j].Resource.id, -amtRequired);

                    //Logging.Msg("Insufficient " + resourceRequired[i].resource + " to start Kloning (" + available.ToString() + "/" + amtRequired.ToString() + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    //Logging.Msg(Localizer.Format("#MOAR-Base-02", resourceRequired[i].resource, available.ToString(), amtRequired.ToString()));
                    Logging.Msg(s: $"{Localizer.Format("#MOAR-Base-02")} {resourceRequired[i].resource} : have: {available:F2} need: {amtRequired:F2}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>GatherResources: determine if required resources available and debit them from vessel.</summary>
        /// <param name="part"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private protected bool GatherResources(Part part, double percentage = 1f)
        {
            Logging.DLog(logMsg: "MoarKerbals: GatherResources");
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;
            Logging.DLog(logMsg: $"MoarKerbals: Global Multiplier: {gblMult:F4} percentage {percentage:P2}", true);

            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * percentage * gblMult; // uses globalMultiplier
                double available = vessel.RequestResource(part, resourceRequired[i].Resource.id, amtRequired, false);

                //debug:
                //Logging.DLog("Resource = " + resourceRequired[i].resource + ", Needed: " + amtRequired + ", available: " + available);
                Logging.DLog(logMsg: $"MoarKerbals: Resource: {resourceRequired[i].resource:F4}, need: {amtRequired:F4}, available: {available:F4}");

                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);

                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);

                    //Logging.Msg("Insufficient " + resourceRequired[i].resource + " to start/continue Kuddling (" + available.ToString("F1") + "/" + amtRequired.ToString("F1") + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    //Logging.Msg(Localizer.Format("#MOAR-Shack-07", resourceRequired[i].resource, available.ToString(), amtRequired.ToString()), 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Shack-07", resourceRequired[i].resource, amtRequired, available));

                    return false;
                }
            }
            return true;
        }

        /// <summary>verify science, reputation, and funds availability</summary>
        /// <returns>true if success</returns>
        private protected bool GatherCurrencies()
        {
            // Debug.Log(message: "MoarKerbals: GatherCurrencies");
            Logging.DLog(logMsg: "MoarKerbals: GatherCurrencies");

            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.SANDBOX:
                    Logging.DLog(logMsg: "MoarKerbals: Sandbox");
                    return true;
                case Game.Modes.CAREER:
                    Logging.DLog(logMsg: "MoarKerbals: Career");
                    Logging.DLog(logMsg: $"MoarKerbals: Funds {Funding.Instance.Funds:F4} need {costFunds:F2}.");
                    Logging.DLog(logMsg: $"MoarKerbals: Rep: {Reputation.Instance.reputation:F4} need {costReputation:F2}.");
                    Logging.DLog(logMsg: $"MoarKerbals: Science: {ResearchAndDevelopment.Instance.Science:F4} need {costScience:F2}.");
                    if ((costReputation < Reputation.Instance.reputation) &&
                        (costScience < ResearchAndDevelopment.Instance.Science) &&
                        (costFunds < Funding.Instance.Funds))
                    {
                        return true;
                    }
                    else
                    {
                        Logging.Msg(Localizer.Format("#MOAR-Insufficient-Rep", Reputation.Instance.reputation, costReputation));
                        Logging.Msg(Localizer.Format("#MOAR-Insufficient-Funds", Funding.Instance.Funds, costFunds));
                        Logging.Msg(Localizer.Format("#MOAR-Insufficient-Sci", ResearchAndDevelopment.Instance.Science, costScience));
                    }
                    break;
                case Game.Modes.SCENARIO:
                    Logging.DLog(logMsg: "MoarKerbals: Scenario");
                    return true;
                case Game.Modes.SCIENCE_SANDBOX:
                    Logging.DLog(logMsg: "MoarKerbals: Science Sandbox");
                    Logging.DLog(logMsg: $"MoarKerbals: Science: {ResearchAndDevelopment.Instance.Science:F4} need {costScience:F2}.");
                    if ((costScience < ResearchAndDevelopment.Instance.Science) &&
                        (costFunds < Funding.Instance.Funds))
                        return true;
                    else Logging.Msg(Localizer.Format("#MOAR-Insufficient-Sci", ResearchAndDevelopment.Instance.Science, costScience));
                    break;
                default:
                    Logging.DLog("CurrentGame.Mode: Other");
                    return true;
            }
            return false;
        }

        /// <summary>spend science, reputation, and funds</summary>
        /// <returns>true = success</returns>
        private protected bool DebitCurrencies()
        {
            //Debug.Log(message: "MoarKerbals: DebitCurrencies");
            Logging.DLog(logMsg: "MoarKerbals: DebitCurrencies");

            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.SANDBOX:
                    Logging.DLog(logMsg: "MoarKerbals: Sandbox");
                    return true;
                case Game.Modes.CAREER:
                    Logging.DLog(logMsg: "MoarKerbals: Career");
                    Logging.DLog(logMsg: $"MoarKerbals: Funds: {Funding.Instance.Funds:F4} need {costFunds:F2}.");
                    Logging.DLog(logMsg: $"MoarKerbals: Rep: {Reputation.Instance.reputation:F4} need {costReputation:F2}.");
                    Logging.DLog(logMsg: $"MoarKerbals: Science: {ResearchAndDevelopment.Instance.Science:F4} need {costScience:F2}.");

                    Reputation.Instance.AddReputation((float)-costReputation, TransactionReasons.CrewRecruited);
                    Funding.Instance.AddFunds((float)-costFunds, TransactionReasons.CrewRecruited);
                    ResearchAndDevelopment.Instance.AddScience((float)-costScience, TransactionReasons.CrewRecruited);
                    return true;
                case Game.Modes.SCENARIO:
                    Logging.DLog(logMsg: "MoarKerbals: Scenario");
                    return true;
                case Game.Modes.SCIENCE_SANDBOX:
                    Logging.DLog(logMsg: "MoarKerbals: Science Sandbox");
                    Logging.DLog(logMsg: $"MoarKerbals: Science: {ResearchAndDevelopment.Instance.Science:F4} need {costScience:F2}.");

                    Funding.Instance.AddFunds((float)-costFunds, TransactionReasons.CrewRecruited);
                    ResearchAndDevelopment.Instance.AddScience((float)-costScience, TransactionReasons.CrewRecruited);
                    return true;

                default:
                    Logging.DLog(logMsg: "CurrentGame.Mode: Other");
                    return true;
            }
        }
    }
}