using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoarKerbals
{
    [KSPModule("KloneBay")]
    public class KloneBay : PartModule
    {
        [KSPField]
        public string recipeIngredients;
        [KSPField]
        public string recipeAmounts;
        [KSPField]
        public float accidentRate;

        private string[] resourceList;
        private float[] resourceAmounts;
        private string[] str_amounts;

        protected AudioSource overload;
        protected AudioSource kloning_success;
 
        public override void OnStart(PartModule.StartState state)
        {
            resourceList = recipeIngredients.Split(',');
            str_amounts = recipeAmounts.Split(',');
            resourceAmounts = new float[str_amounts.Length];
            for (int i = 0; i < resourceList.Length; i++)
                resourceAmounts[i] = float.Parse(str_amounts[i]);
            if (accidentRate < 0)
                accidentRate = 0;
            else if (accidentRate > 1)
                accidentRate = 100;
            else
                accidentRate = accidentRate * 100;

            kloning_success = gameObject.AddComponent<AudioSource>();
            kloning_success.clip = GameDatabase.Instance.GetAudioClip("MoarKerbals/Sounds/kloning");
            kloning_success.volume = 0.8f;
            kloning_success.panStereo = 0;
            kloning_success.rolloffMode = AudioRolloffMode.Linear;
            kloning_success.Stop();

            overload = gameObject.AddComponent<AudioSource>();
            overload.clip = GameDatabase.Instance.GetAudioClip("MoarKerbals/Sounds/overload");
            overload.volume = 0.9f;
            overload.panStereo = 0;
            overload.rolloffMode = AudioRolloffMode.Linear;
            overload.Stop();
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
        [Obsolete]
        public void ActivateKlone()
        {
            if (PartHasRoom(part) && GatherResources(part))
            {
                System.Random rnd = new System.Random();
                if (accidentRate <= rnd.Next(1,100)) {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                    part.AddCrewmember(kerbal);
                    kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();
                    ScreenMessages.PostScreenMessage("Kloning Success!  " + kerbal.name + " has joined your space program", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    kloning_success.Play();
                }
                else {
                    int opt = rnd.Next(0,100);
                    string culprit = part.protoModuleCrew.First().name;
                    int count = part.protoModuleCrew.Count;
                    while (part.protoModuleCrew.Count > 0)
                    {
                        ProtoCrewMember crewman = part.protoModuleCrew.First();
                        part.RemoveCrewmember(crewman);
                        crewman.Die();
                    }
                    if (count > 1) {
                        if (opt < 30)
                            ScreenMessages.PostScreenMessage("A power surge opens a portal to R'lyeh and all your test subjects vanish!", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else if (opt < 60)
                            ScreenMessages.PostScreenMessage("Your Kerbals are lost in the Matrix!", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else if (opt < 85)
                            ScreenMessages.PostScreenMessage("Radiation turns " + culprit + " into a monstrous creature from the deep who eats your other kerbals before shambling off!", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else 
                            ScreenMessages.PostScreenMessage("The Kloning process failed.  This is going to take some cleaning.", 10f, ScreenMessageStyle.UPPER_CENTER); 
                    }
                    else {
                        if (opt < 30)
                            ScreenMessages.PostScreenMessage(culprit + " comes out with a goatee, yelling something about a Terran Empire.", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else if (opt < 60)
                            ScreenMessages.PostScreenMessage(culprit + " mutates into a giant fly, but goes on to have a successful movie career.", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else if (opt < 80)
                            ScreenMessages.PostScreenMessage("You find the bay empty, a note pinned to the wall 'Gone back to the future'", 10f, ScreenMessageStyle.UPPER_CENTER); 
                        else
                            ScreenMessages.PostScreenMessage("The Kloning process failed.  You lost " + culprit + ", but at least you have pizza now.", 10f, ScreenMessageStyle.UPPER_CENTER); 
                    }
                    overload.Play();
                }
            }
            GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }

        [Obsolete]
        private bool GatherResources(Part part)
        {
            //Steps through to gather resources
            for (int i = 0; i < resourceList.Length; i++)
            {
                float available = part.RequestResource(resourceList[i], resourceAmounts[i]);

                if (available != resourceAmounts[i])
                {
                    //Upon not having enough of a resource, returns all previously collected
                    part.RequestResource(resourceList[i], -available);
                    for (int j = 0; j < i; j++)
                        part.RequestResource(resourceList[j], -resourceAmounts[j]);
                    ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + resourceAmounts[i].ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
            }
            return true;
        }

        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private bool PartHasRoom(Part part)
        {
            if ((part.protoModuleCrew.Count < part.CrewCapacity) && (part.protoModuleCrew.Count > 0))
                return true;
            else
            {
                if (part.protoModuleCrew.Count == 0)
                    ScreenMessages.PostScreenMessage("Kloning requires at least one test subject", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                else if (part.protoModuleCrew.Count == part.CrewCapacity)
                    ScreenMessages.PostScreenMessage("No room left in Kloning Bay", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
        }

        public override string GetInfo()
        {
            string[] resList = recipeIngredients.Split(',');
            string[] amounts = recipeAmounts.Split(',');
            float[] resAmounts = new float[amounts.Length];

            for (int i = 0; i < resList.Length; i++)
                resAmounts[i] = float.Parse(amounts[i]);

            string display = "\r\nInput:\r\n";
            for (int i = 0; i < resList.Length; i++)
                display += String.Format("{0:0,0}", resAmounts[i]) + " " + resList[i] + "\r\n";

            display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
            return display;
        }

    }

    [KSPModule("KuddleShack")]
    public class KuddleShack : PartModule
    {
        [KSPField]
        public string recipeIngredients;
        [KSPField]
        public string recipeAmounts;
        [KSPField]
        public bool accidentRate;

        private string[] resourceList;
        private float[] resourceAmounts;
        private string[] str_amounts;

        public override void OnStart(PartModule.StartState state)
        {
            resourceList = recipeIngredients.Split(',');
            str_amounts = recipeAmounts.Split(',');
            resourceAmounts = new float[str_amounts.Length];
            for (int i = 0; i < resourceList.Length; i++)
                resourceAmounts[i] = float.Parse(str_amounts[i]);
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
        [Obsolete]
        public void ActivateKlone()
        {
            if (PartHasRoom(part) && GatherResources(part))
            {
                ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                part.AddCrewmember(kerbal);
                kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

                if (kerbal.seat != null)
                    kerbal.seat.SpawnCrew();

                GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
            }
        }

        [Obsolete]
        private bool GatherResources(Part part)
        {
            //Steps through to gather resources
            for (int i = 0; i < resourceList.Length; i++)
            {
                float available = part.RequestResource(resourceList[i], resourceAmounts[i]);
                //debug:
                print(" DEBUG: " + available.ToString());
                if (available != resourceAmounts[i])
                {
                    //Upon not having enough of a resource, returns all previously collected
                    part.RequestResource(resourceList[i], -available);
                    for (int j = 0; j < i; j++)
                        part.RequestResource(resourceList[j], -resourceAmounts[j]);
                    ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + resourceAmounts[i].ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    return false;
                }
            }
            return true;
        }

        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private bool PartHasRoom(Part part)
        {
            if ((part.protoModuleCrew.Count < part.CrewCapacity) && (part.protoModuleCrew.Count > 0))
                return true;
            else
            {
                if (part.protoModuleCrew.Count == 0)
                    ScreenMessages.PostScreenMessage("Kloning requires a test subject Kerbal", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                else if (part.protoModuleCrew.Count == part.CrewCapacity)
                    ScreenMessages.PostScreenMessage("No room left in Kloning Bay", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
        }

        public override string GetInfo()
        {
            string[] resList = recipeIngredients.Split(',');
            string[] amounts = recipeAmounts.Split(',');
            float[] resAmounts = new float[amounts.Length];

            for (int i = 0; i < resList.Length; i++)
                resAmounts[i] = float.Parse(amounts[i]);

            string display = "\r\nInput:\r\n";
            for (int i = 0; i < resList.Length; i++)
                display += String.Format("{0:0,0}", resAmounts[i]) + " " + resList[i] + "\r\n";

            display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
            return display;
        }

    }
}
