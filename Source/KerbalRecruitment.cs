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
        [KSPEvent(guiName = "Recruit Kerbal", active = true, guiActive = true)]
        void recruitKerbal()
        {
            //Debug.Log(debuggingClass.modName + "Kerbal Recruitment Button pressed!");
            ScreenMessages.PostScreenMessage("Kerbal Recruitment Button pressed!", 3.5f, ScreenMessageStyle.UPPER_CENTER);
            bool changedTrait = false;
            List<ProtoCrewMember> vesselCrew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewMember in vesselCrew)
            {
                if (crewMember.trait == debuggingClass.civilianTrait && changedTrait == false)
                {
                    crewMember.trait = getRandomTrait();
                    changedTrait = true;
            ScreenMessages.PostScreenMessage(crewMember.name + " is now a " + crewMember.trait + "!", 3.5f, ScreenMessageStyle.UPPER_CENTER);
              //      Debug.Log(debuggingClass.modName + crewMember.name + " is now a " + crewMember.trait + "!");
                }
            }
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
            ScreenMessages.PostScreenMessage("Created trait:  " + kerbalTrait, 3.5f, ScreenMessageStyle.UPPER_CENTER);
           // Debug.Log(debuggingClass.modName + "Created trait:  " + kerbalTrait);
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
