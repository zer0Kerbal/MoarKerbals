using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static MoarKerbals.Logging;
using KSP.Localization;

namespace MoarKerbals
{
    /// <summary>KuddleShack module</summary>
    [KSPModule("KuddleShack")]
    public class KuddleShack : MoarKerbalBase
    {

        /// <summary>kuddling success sound</summary>
        protected AudioSource kuddling_success;

        public override void OnStart(PartModule.StartState state)
        {
            Logging.DLog("KuddleShack.OnStart", true);
            base.OnStart(state);
            Events["ActivateKlone"].guiName = initiateAction;
            Events["ActivateKlone"].guiActive = false;
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GetLightingModules();
            GetMatingStatus();
            StartCoroutine(SlowUpdate());

            kuddling_success = gameObject.AddComponent<AudioSource>();
            kuddling_success.clip = GameDatabase.Instance.GetAudioClip("MoarKerbals/Sounds/kuddleshack");
            kuddling_success.volume = 0.9f;
            kuddling_success.panStereo = 0;
            kuddling_success.rolloffMode = AudioRolloffMode.Linear;
            kuddling_success.Stop();
        }

        private List<PartModule> modulesLight = new List<PartModule>();

        void GetLightingModules()
        {
            // Check for lightable modules
            if (part.Modules.Contains<ModuleColorChanger>())
            {
                ModuleColorChanger partM = part.Modules.GetModule<ModuleColorChanger>();
                modulesLight.Add(partM);
            }
            if (part.Modules.Contains<ModuleLight>())
            {
                foreach (ModuleLight partM in part.Modules.GetModules<ModuleLight>())
                    modulesLight.Add(partM);
            }
            if (part.Modules.Contains<ModuleAnimateGeneric>())
            {
                foreach (ModuleAnimateGeneric partM in part.Modules.GetModules<ModuleAnimateGeneric>())
                    modulesLight.Add(partM);
            }
            if (part.Modules.Contains("WBILight"))
            {
                foreach (PartModule partM in part.Modules)
                {
                    if (partM.ClassName == "WBILight")
                        modulesLight.Add(partM);
                }
            }
            if (part.Modules.Contains("ModuleKELight"))
            {
                foreach (PartModule partM in part.Modules)
                {
                    if (partM.ClassName == "ModuleKELight")
                        modulesLight.Add(partM);
                }
            }
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
                            //Logging.Msg("Kuddle Time: " + time, 5f, ScreenMessageStyle.UPPER_CENTER);
                            Logging.Msg(Localizer.Format("#MOAR-Shack-01", time), 5f, ScreenMessageStyle.UPPER_CENTER);
                        }

                        if (Planetarium.GetUniversalTime() - startMatingTimer >= SettingsInterface.kuddleTimeNeeded())
                        {
                            readyToKuddle = true;
                            Events["ActivateKlone"].guiActive = true;
                            //Logging.Msg("Minimum Kuddle Time Reached", 5f, ScreenMessageStyle.UPPER_CENTER);
                            Logging.Msg(Localizer.Format("#MOAR-Shack-02"), 5f, ScreenMessageStyle.UPPER_CENTER);
                        }
                    }
                    else
                        readyToKuddle = false;
                }
            }
        }

        /// <summary>has x kerbal</summary>
        [KSPField]
        bool hasMale = false;
        /// <summary>has y kerbal</summary>
        [KSPField]
        bool hasFemale = false;
        /// <summary>has mating pair of kerbals</summary>
        [KSPField]
        bool hasMatingPair = false;
        /// <summary>start mating timer</summary>
        [KSPField]
        double startMatingTimer = 0;


        /// <summary>GetMatingStatus</summary>
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
                SwitchLight.On(modulesLight);
            }
            else
            {
                if (!newHasMatingPair)
                {
                    hasMatingPair = false;
                    SwitchLight.Off(modulesLight);
                }
            }
        }

        /// <summary> the crew board vessel.</summary>
        /// <param name="fta">The fta.</param>
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
        //[KSPEvent(active = true, guiActiveUncommand = true, guiActiveUnfocused = true, guiActive = true, guiName = "Complete Kuddling!")]
        [KSPEvent(active = true, guiActiveUncommand = true, guiActiveUnfocused = true, guiActive = true, guiName = "#MOAR-Shack-03")]
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

                    //Logging.Msg("Kuddling Success!  " + kerbal.name + "(Lv " + kerbal.experienceLevel.ToString() + " " + kerbal.experienceTrait.Title + ") has joined your space program");
                    Logging.Msg(Localizer.Format("#MOAR-Shack-11", kerbal.name, kerbal.experienceLevel.ToString(), kerbal.experienceTrait.Title));
                    if (SettingsInterface.SoundOn()) kuddling_success.Play();

                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
                }
            }
            else
            {
                if (hasMatingPair)
                    //Logging.Msg("Insufficient time for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Shack-04"), 5f, ScreenMessageStyle.UPPER_CENTER);
                else
                {
                    if (part.protoModuleCrew.Count == 2)
                        //Logging.Msg("One kerbal of each sex is needed for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                        Logging.Msg(Localizer.Format("#MOAR-Shack-05"), 5f, ScreenMessageStyle.UPPER_CENTER);
                    else
                        //Logging.Msg("Two kerbals are needed for reproduction", 5f, ScreenMessageStyle.UPPER_CENTER);
                        Logging.Msg(Localizer.Format("#MOAR-Shack-06"), 5f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }


        private bool GatherResources(Part part, double percentage = 1f)
        {
            //Steps through to gather resources
            for (int i = 0; i < resourceRequired.Count; i++)
            {
                double amtRequired = resourceRequired[i].amount * percentage * HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier; ;
                double available = vessel.RequestResource(part, resourceRequired[i].Resource.id, amtRequired, false);

                //debug:
                DLog("Resource: " + resourceRequired[i].resource + ", Needed: " + amtRequired + ", available: " + available);

                if (available + 0.0001f < amtRequired)
                {
                    //Upon not having enough of a resource, returns all previously collected
                    vessel.RequestResource(part, resourceRequired[i].Resource.id, -available, false);

                    for (int j = 0; j < i; j++)
                        vessel.RequestResource(part, resourceRequired[j].Resource.id, -amtRequired, false);

                    //Logging.Msg("Insufficient " + resourceRequired[i].resource + " to start/continue Kuddling (" + available.ToString("F1") + "/" + amtRequired.ToString("F1") + ")", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Shack-07", resourceRequired[i].resource, available.ToString(), amtRequired.ToString()), 5f, ScreenMessageStyle.UPPER_CENTER);

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
                //Logging.Msg("Kuddling requires both a male and female kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                Logging.Msg(Localizer.Format("#MOAR-Shack-08"), 5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            else
            {
                if (part.protoModuleCrew.Count == 0)
                    //Logging.Msg("Kuddling requires both a male and female Kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Shack-08"), 5f, ScreenMessageStyle.UPPER_CENTER);
                else if (part.protoModuleCrew.Count == part.CrewCapacity)
                    //Logging.Msg("No room left in Kuddle Shack", 5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.Msg(Localizer.Format("#MOAR-Shack-09"), 5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
        }


        public override string GetInfo()
        {
            base.OnStart(StartState.None);

            //string display = "\r\nInput:\r\n";
            string display = String.Format("\r\n" + Localizer.Format("#MOAR-005") + ":\r\n");

            for (int i = 0; i < resourceRequired.Count; i++)
                display += String.Format("{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n";

            //display += "\r\nOutput:\r\n A brand new Kerbal.";
            display += String.Format("\r\n" + Localizer.Format("#MOAR-006") + ":\r\n" + Localizer.Format("#MOAR-Shack-10") + ".");

            return display;
        }

    }
}
