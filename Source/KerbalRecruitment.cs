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
using KSP.Localization;

namespace MoarKerbals
{
    /// <summary>PartModule: KerbalRecruitment</summary>
    /// <seealso cref="PartModule" />
    public class KerbalRecruitment : PartModule
    {
        /// <summary>KerbalJob Enum - kerbal professions</summary>
        enum KerbalJob
        {
            Pilot,
            Engineer,
            Scientist
        }

        /// <summary>Recruits the kerbal.</summary>
        [KSPEvent(guiName = "#MOAR-Recruitment-01", active = true, guiActive = true)]
        void recruitKerbal()
        {
            Logging.Msg("Kerbal Recruitment Button pressed!", 3.5f, ScreenMessageStyle.UPPER_CENTER);

            bool changedTrait = false;
            List<ProtoCrewMember> vesselCrew = vessel.GetVesselCrew();
            foreach (ProtoCrewMember crewMember in vesselCrew)
            {
                Logging.Msg(crewMember.name + " : " + crewMember.trait + ": " + crewMember.type, 3.5f, ScreenMessageStyle.UPPER_CENTER);
                if (crewMember.trait == Localizer.Format("#MOAR-004") && changedTrait == false)
                {
                    crewMember.trait = getRandomTrait();
                    changedTrait = true;
                    KerbalRoster.SetExperienceTrait(crewMember, crewMember.trait);
                    Logging.Msg(crewMember.name + " is now a " + crewMember.trait + "!", 3.5f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            if (!changedTrait) Logging.Msg("No civilians available to recruit");
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
            Logging.Msg(String.Format("Created trait:  {0}", kerbalTrait));
            return kerbalTrait;
        }

        public override string GetInfo()
        {
            //string display = "\r\nInput:\r\n One Civilian Kerbal";
            string display = String.Format("\r\n" + Localizer.Format("#MOAR-005") + ":\r\n" + Localizer.Format("#MOAR-Recruitment-03") + " " + Localizer.Format("#MOAR-004") + " " + Localizer.Format("#MOAR-Recruitment-04") + ".\r\n");

            //display += "\r\nOutput:\r\n Pilot, Engineer, Scientist Kerbal (random) eating a MinmusMint ice cream cone.";
            display += String.Format(Localizer.Format("#MOAR-006") + ":\r\n" + Localizer.Format("#MOAR-Recruitment-06") + ".\r\n");

            return display;
        }
    }
}
