using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using static MoarKerbals.Init;

namespace MoarKerbals
{
    /// <summary>
    /// 
    /// </summary>
    [KSPModule("KuddleShack")]
    public class KuddleShack : PartModule
    {
        [KSPField]
        public string recipeIngredients;
        [KSPField]
        public string recipeAmounts;
        [KSPField]
        public string initiateAction = "Initiate Kuddling!";


        private string[] resList; // = recipeIngredients.Split(',');
        private string[] resourceList;
        private double[] resourceAmounts;
        private string[] str_amounts;

        public override void OnStart(PartModule.StartState state)
        {
            resourceList = recipeIngredients.Split(',');
            str_amounts = recipeAmounts.Split(',');
            resList = recipeIngredients.Split(',');
            resourceAmounts = new double[str_amounts.Length];
            for (int i = 0; i < resourceList.Length; i++)
                resourceAmounts[i] = double.Parse(str_amounts[i]);


            Log.Info("OnStart, resourceList.Length: " + resourceList.Length);
            Log.Info("OnStart, str_amounts.Length: " + str_amounts.Length);
            for (int i = 0; i < resourceList.Length; i++)
            {
                Log.Info("resourceList[" + i + "]: " + resourceList[i] + ": " + resourceAmounts[i]);
            }

                Events["ActivateKlone"].guiName = initiateAction;

            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GetMatingStatus();
            StartCoroutine(SlowUpdate());
        }

        IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds((float)SettingsInterface.slowUpdateTime());
                Events["ActivateKlone"].active = false;
                if (hasMatingPair)
                {
                    if (GatherResources(part, (float)SettingsInterface.slowUpdateTime() / SettingsInterface.kuddleTimeNeeded()))
                    {
                        double t = (Planetarium.GetUniversalTime() - startMatingTimer);
                        if (t > 1f)
                            ScreenMessages.PostScreenMessage("Kuddle Time: " + t.ToString("F0"), 3.5f, ScreenMessageStyle.UPPER_CENTER);
                        Events["ActivateKlone"].active = true;
                    }
                }
            }
        }

        [KSPField]
        bool hasMale = false;
        [KSPField]
        bool hasFemale = false;
        [KSPField]
        bool hasMatingPair = false;
        [KSPField]
        double startMatingTimer = 0;
        //const double KUDDLE_TIME = 21600;
        //const float SLOW_UPDATE_TIME = 5;

        void GetMatingStatus()
        {
            hasMale = false;
            hasFemale = false;

            foreach (var k in part.protoModuleCrew)
            {
                if (k.gender == ProtoCrewMember.Gender.Male)
                    hasMale = true;
                if (k.gender == ProtoCrewMember.Gender.Female)
                    hasFemale = true;
            }

            bool newHasMatingPair = hasMale && hasFemale;
            if (!hasMatingPair && newHasMatingPair)
            {
                startMatingTimer = Planetarium.GetUniversalTime();
                hasMatingPair = true;
            }
            else
            {
                if (!newHasMatingPair)
                    hasMatingPair = false;
            }
        }
        void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> fta)
        {
            //Log.Info("onCrewBoardVessel, From part: " + fta.from.partInfo.title + ", to part: " + fta.to.partInfo.title);
            GetMatingStatus();
        }

        void onCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> hfta)
        {
            //Log.Info("onCrewTransferred, ProtoCrewmember: " + hfta.host.KerbalRef.crewMemberName + ", to part: " + hfta.to.partInfo.title);
            GetMatingStatus();
        }

        void onCrewOnEva(GameEvents.FromToAction<Part, Part> fta)
        {
            //Log.Info("onCrewOnEva, from part: " + fta.from.partInfo.title + ", to part: " + fta.to.vessel.vesselName);
            GetMatingStatus();
        }

        /// <summary>
        /// 
        /// </summary>
        [KSPEvent(active = true, guiActive = true, guiName = "Initiate Kuddling!")]
        public void ActivateKlone()
        {
            if (hasMatingPair && Planetarium.GetUniversalTime() - startMatingTimer > SettingsInterface.kuddleTimeNeeded())
            {
                if (PartHasRoom(part) ) //&& GatherResources(part))
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                    part.AddCrewmember(kerbal);
                    kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();

                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
                }
            }
            else
            {
                if (hasMatingPair)
                    ScreenMessages.PostScreenMessage("Insufficient time for reproduction", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                else
                {
                    if (part.protoModuleCrew.Count == 2)
                        ScreenMessages.PostScreenMessage("One kerbal of each sex is needed for reproduction", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    else
                        ScreenMessages.PostScreenMessage("Two kerbals are needed for reproduction", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                }

            }
        }


        private bool GatherResources(Part part, double percentage = 1f)
        {
            //Steps through to gather resources
            for (int i = 0; i < resourceList.Length; i++)
            {
                double available = part.RequestResource(resourceList[i], resourceAmounts[i] * percentage);
                //debug:
                //Log.dbg("Costs: {1} : resourceAmounts: {2}", res  resList[i], resourceList[i]), available, resourceAmounts[i]);
                if (available < resourceAmounts[i] * percentage)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    part.RequestResource(resourceList[i], -available);
                    for (int j = 0; j < i; j++)
                        part.RequestResource(resourceList[j], -resourceAmounts[j]);
                    ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start/continue Kuddling (" + available.ToString() + "/" + (resourceAmounts[i] * percentage).ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;

                }
            }
            return true;
        }

        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private bool PartHasRoom(Part part)
        {
            if ((part.protoModuleCrew.Count < part.CrewCapacity) && ((part.protoModuleCrew.Count > 0)))
            {
                bool male = false, female = false;
                foreach (var k in part.protoModuleCrew)
                {
                    if (!male)
                        male = (k.gender == ProtoCrewMember.Gender.Male);
                    if (!female)
                        female = (k.gender == ProtoCrewMember.Gender.Female);
                }
                if (male && female)
                    return true;
                ScreenMessages.PostScreenMessage("Kuddling requires both a male and female kerbal", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            else
            {
                if (part.protoModuleCrew.Count == 0)
                    ScreenMessages.PostScreenMessage("Kuddling requires both a male and female Kerbal", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                else if (part.protoModuleCrew.Count == part.CrewCapacity)
                    ScreenMessages.PostScreenMessage("No room left in Kuddle Shack", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
        }

        public override string GetInfo()
        {
            string[] resList = recipeIngredients.Split(',');
            string[] amounts = recipeAmounts.Split(',');
            double[] resAmounts = new double[amounts.Length];

            for (int i = 0; i < resList.Length; i++)
                resAmounts[i] = double.Parse(amounts[i]);

            string display = "\r\nInput:\r\n";
            for (int i = 0; i < resList.Length; i++)
                display += String.Format("{0:0,0}", resAmounts[i]) + " " + resList[i] + "\r\n";

            display += "\r\nOutput:\r\n A brand new Kerbal.";
            return display;
        }

    }
}
