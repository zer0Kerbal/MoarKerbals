using System;
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;

namespace MoarKerbals
{
    public class MoarKerbalsBase : PartModule
    {
        #region KSPFields

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

        #endregion
        #region Variables | Constants

        /// <summary>internal list of resourcesRequired  </summary>
        internal List<ResourceRequired> resourceRequired = new List<ResourceRequired>();

        /// <summary>internal name of GUI groupName  </summary>
        internal static string groupName = Localizer.Format("#MOAR-003", Version.SText);
        #endregion

        /// <summary>The Global Cost Multiplier
        /// Need this to be used in GetInfo()
        /// updated in OnStart() and OnFixedUpdate()
        /// </summary>
        internal double gblMult;

        /// <summary>onStart</summary>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            //Logging.DLog(logMsg: "MoarKerbalsBase.OnStart");
            // Debug.Log(message: "MoarKerbalsBase.OnStart");

            if (recipeIngredients != string.Empty && recipeAmounts != string.Empty)
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

        /// <summary>
        /// Unity FixedUpdate
        /// </summary>
        private protected void FixedUpdate() => base.OnFixedUpdate();

        /// <summary>GatherResources: determine if required resources available and debit them from part.</summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private protected bool GatherResources(Part part)
        {
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;

            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * gblMult; // uses globalMultiplier
                double available = part.RequestResource(resourceRequired[i].Resource.id, amtRequired);

                Logging.DLog(logMsg: $"GatherResources: {resourceRequired[i].resource} : have: {available:F2} need: {amtRequired:F2} |  Global Multiplier: {gblMult:P2}");
                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);
                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);

                    Logging.Msg(s: Localizer.Format("#MOAR-Insufficient", resourceRequired[i].resource, available, amtRequired));
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
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;

            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * percentage * gblMult; // uses globalMultiplier
                double available = vessel.RequestResource(part, resourceRequired[i].Resource.id, amtRequired, false);

                Logging.DLog(logMsg: $"GatherResourcesT: {resourceRequired[i].resource} : have: {available:F2} need: {amtRequired:F2} |  Global Multiplier: {gblMult:P2}");

                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);

                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);

                    Logging.Msg(s: Localizer.Format("#MOAR-Insufficient", resourceRequired[i].resource, available, amtRequired));
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