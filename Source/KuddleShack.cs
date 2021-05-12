using System;
using System.Collections;
using System.Collections.Generic;
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
    public class KuddleShack : MoarKerbalBase
    {

        public override void OnStart(PartModule.StartState state)
        {
            Log.Info("KuddleShack.OnStart");
            base.OnStart(state);
            Events["ActivateKlone"].guiName = initiateAction;
            Events["ActivateKlone"].guiActive = false;
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GetMatingStatus();
            StartCoroutine(SlowUpdate());
        }

        bool readyToKuddle = false;
        const int SECS_IN_DAY = 21600;
        const int SECS_IN_HOUR = 3600;
        const int SECS_IN_MINUTE = 60;
        IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds((float)SettingsInterface.slowUpdateTime());
                if (!readyToKuddle)
                    Events["ActivateKlone"].guiActive = false;
                if (hasMatingPair)
                {
                    if (GatherResources(part, (float)SettingsInterface.slowUpdateTime() / SettingsInterface.kuddleTimeNeeded()))
                    {
                        int t = (int)(Planetarium.GetUniversalTime() - startMatingTimer);
                        if (t > 1f)
                        {
                            int days = t / SECS_IN_DAY;
                            int hours = (t - days * SECS_IN_DAY) / SECS_IN_HOUR;
                            int minutes = (t - days * SECS_IN_DAY - hours * SECS_IN_HOUR) / SECS_IN_MINUTE;
                            int seconds = t - days * SECS_IN_DAY - hours * SECS_IN_HOUR - minutes * SECS_IN_MINUTE;

                            string time = "";
                            if (t >= SECS_IN_DAY)
                                time += days + " days";
                            if (t >= SECS_IN_HOUR)
                            {
                                if (time.Length > 0) time += ", ";
                                time += hours + " hours";
                            }
                            if (t >= SECS_IN_MINUTE)
                            {
                                if (time.Length > 0) time += ", ";
                                time += minutes + " minutes";
                            }
                            if (time.Length > 0) time += ", ";
                            time += seconds + " seconds";
                            ScreenMessages.PostScreenMessage("Kuddle Time: " + time, 5f, ScreenMessageStyle.UPPER_CENTER);
                        }

                        if (Planetarium.GetUniversalTime() - startMatingTimer >= SettingsInterface.kuddleTimeNeeded())
                        {
                            readyToKuddle = true;
                            Events["ActivateKlone"].guiActive = true;
                            ScreenMessages.PostScreenMessage("Minimum Kuddle Time Reached", 5f, ScreenMessageStyle.UPPER_CENTER);
                        }
                    }
                    else
                        readyToKuddle = false;
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
                {
                    hasMatingPair = false;
                }
            }
        }
        void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> fta)
        {
            GetMatingStatus();
        }

        void onCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> hfta)
        {
            GetMatingStatus();
        }

        void onCrewOnEva(GameEvents.FromToAction<Part, Part> fta)
        {
            GetMatingStatus();
        }

        /// <summary>
        /// 
        /// </summary>
        [KSPEvent(active = true, guiActiveUncommand = true, guiActiveUnfocused = true, guiActive = true, guiName = "Complete Kuddling!")]
        public void ActivateKlone()
        {
            if (hasMatingPair && Planetarium.GetUniversalTime() - startMatingTimer >= SettingsInterface.kuddleTimeNeeded())
            {
                if (PartHasRoom(part)) //&& GatherResources(part))
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
                    ScreenMessages.PostScreenMessage("Insufficient time for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                else
                {
                    if (part.protoModuleCrew.Count == 2)
                        ScreenMessages.PostScreenMessage("One kerbal of each sex is needed for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                    else
                        ScreenMessages.PostScreenMessage("Two kerbals are needed for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                }

            }
        }


        private bool GatherResources(Part part, double percentage = 1f)
        {
            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * percentage * HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier
                    ;
                double available = vessel.RequestResource(part, resourceRequired[i].Resource.id, amtRequired, false);

                //debug:
                Log.Info("Resource: " + resourceRequired[i].resource + ", Needed: " + amtRequired + ", available: " + available);

                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);

                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);

                    ScreenMessages.PostScreenMessage("Insufficient " + resourceRequired[i].resource + " to start/continue Kuddling (" + available.ToString("F1") + "/" + amtRequired.ToString("F1") + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;

                }
            }
            return true;
        }

        //Checks to make sure there are two kerbals and that there is room to store the new kerbal
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
                ScreenMessages.PostScreenMessage("Kuddling requires both a male and female kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            else
            {
                if (part.protoModuleCrew.Count == 0)
                    ScreenMessages.PostScreenMessage("Kuddling requires both a male and female Kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                else if (part.protoModuleCrew.Count == part.CrewCapacity)
                    ScreenMessages.PostScreenMessage("No room left in Kuddle Shack", 5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
        }


        public override string GetInfo()
        {
            base.OnStart(StartState.None);
            string display = "\r\nInput:\r\n";
            for (int i = 0; i < resourceRequired.Count; i++)
                display += String.Format("{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n";

            display += "\r\nOutput:\r\n A brand new Kerbal.";
            return display;
        }

    }
}
