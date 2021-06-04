/*
?  Based upon KerbalRecruitment from the 'CivilianPopulation' mod for Kerbal Space Program
    https://github.com/linuxgurugamer/CivilianPopulation

    LinuxGuruGamer
    CC BY-NC 4.0 (Attribution-NonCommercial 4.0 International) (https://creativecommons.org/licenses/by-nc/4.0/)
    specifically: https://github.com/linuxgurugamer/CivilianPopulation

    This file has been modified extensively and is released under the same license.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace MoarKerbals
{
    public class KerbalRecruitment : PartModule
    {
        enum KerbalJob
        {
            Pilot,
            Engineer,
            Scientist
        }

        [KSPEvent(guiName = "Recruit Kerbal", active = true, guiActive = true)]
        void recruitKerbal()
        {
            //Debug.Log(debuggingClass.modName + "Kerbal Recruitment Button pressed!");
            Log.dbg("Kerbal Recruitment Button pressed!", 3.5f, ScreenMessageStyle.UPPER_CENTER);
            bool changedTrait = false;
            List<ProtoCrewMember> vesselCrew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewMember in vesselCrew)
            {
                Log.dbg(crewMember.name + " : " + crewMember.trait + ": " + crewMember.type, 3.5f, ScreenMessageStyle.UPPER_CENTER);
                //if (crewMember.trait == debuggingClass.civilianTrait && changedTrait == false)
                if (crewMember.trait == "Civilian" && changedTrait == false)
                {
                    crewMember.trait = getRandomTrait();
                    changedTrait = true;
                    // KerbalRoster.SetExperienceTrait(crewMember, KerbalRoster.pilotTrait);
                    KerbalRoster.SetExperienceTrait(crewMember, crewMember.trait);
                    //HighLogic.CurrentGame.CrewRoster.Save(HighLogic.CurrentGame());
                    Utilities.msg(crewMember.name + " is now a " + crewMember.trait + "!", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    //      Debug.Log(debuggingClass.modName + crewMember.name + " is now a " + crewMember.trait + "!");
                }
            }
            if (!changedTrait) Utilities.msg("No civilians available to recruit");
            //if (changedTrait) GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }

        private string getRandomTrait()
        {
            int numberOfClasses = 3;
            string kerbalTrait = "";
            int randNum;
            System.Random newRand = new System.Random();
            randNum = newRand.Next() % numberOfClasses;
            if (randNum == 0)
                kerbalTrait = "Pilot";
            if (randNum == 1)
                kerbalTrait = "Engineer";
            if (randNum == 2)
                kerbalTrait = "Scientist";
            // ScreenMessages.PostScreenMessage("Created trait:  " + kerbalTrait, 3.5f, ScreenMessageStyle.UPPER_CENTER);
            Log.dbg(String.Format("Created trait:  {0}", kerbalTrait));
            return kerbalTrait;
        }
        public override string GetInfo()
        {
            string display = "\r\nInput:\r\n One Civilian Kerbal";

            display += "\r\nOutput:\r\n Pilot, Engineer, Scientest Kerbal (random) eating a MinmusMint icecream cone.";
            return display;
        }
    }
}
