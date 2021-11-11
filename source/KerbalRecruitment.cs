/*
?  Based upon KerbalRecruitment from the 'CivilianPopulation' mod for Kerbal Space Program
    https://github.com/linuxgurugamer/CivilianPopulation

    LinuxGuruGamer
    CC BY-NC 4.0 (Attribution-NonCommercial 4.0 International) (https://creativecommons.org/licenses/by-nc/4.0/)
    specifically: https://github.com/linuxgurugamer/CivilianPopulation

    This file has been modified extensively.

*/

using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace MoarKerbals
{
    /// <summary>PartModule: KerbalRecruitment</summary>
    /// <seealso cref="PartModule" />
    public class KerbalRecruitment : MoarKerbalsBase // PartModule
    {
        /// <summary>KerbalJob Enum - kerbal professions</summary>
        enum KerbalJob
        {
            Pilot,
            Engineer,
            Scientist
        }

        /// <summary>KerbalRecuitmentEnabled - is the module enabled (default = false)</summary>
        [KSPField(guiName = "#MOAR-Academy-00",
                            groupName = "MoarKerbals",
                            guiActive = true,
                            guiActiveEditor = true,
                            isPersistant = true),
                            UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool KerbalRecruitmentEnabled = false;

        /// <summary>The graduation sound</summary>
        protected AudioSource graduation0;

        /// <summary>The graduation sound</summary>
        protected AudioSource graduation1;

        /// <summary>The graduation sound</summary>
        protected AudioSource dropOut0;

        /// <summary>The graduation sound</summary>
        protected AudioSource dropOut1;

        /// <summary>onStart</summary>
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            Logging.DLog("KerbalAcademy.OnStart");

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().coloredPAW)
                Fields["KerbalRecruitmentEnabled"].group.displayName = System.String.Format("<color=#BADA55>" + groupName + "</color>");
            else
                Fields["KerbalRecruitmentEnabled"].group.displayName = groupName;

            graduation0 = gameObject.AddComponent<AudioSource>();
            graduation0.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/positive");
            graduation0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            graduation0.panStereo = 0;
            graduation0.rolloffMode = AudioRolloffMode.Linear;
            graduation0.Stop();

            graduation1 = gameObject.AddComponent<AudioSource>();
            graduation1.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/Rise05");
            graduation1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            graduation1.panStereo = 0;
            graduation1.rolloffMode = AudioRolloffMode.Linear;
            graduation1.Stop();

            dropOut0 = gameObject.AddComponent<AudioSource>();
            dropOut0.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/negative");
            dropOut0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            dropOut0.panStereo = 0;
            dropOut0.rolloffMode = AudioRolloffMode.Linear;
            dropOut0.Stop();

            dropOut1 = gameObject.AddComponent<AudioSource>();
            dropOut1.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/misc_sound");
            dropOut1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            dropOut1.panStereo = 0;
            dropOut1.rolloffMode = AudioRolloffMode.Linear;
            dropOut1.Stop();

            Events["RecruitKerbal"].guiName = Localizer.Format("#MOAR-Academy-01"); //initiateAction;
        }

        private protected void OnFixedUpdate()
        {
            // base.OnFixedUpdate();
            //Logging.DLog("KerbalAcademy.FixedUpdate", true);

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().coloredPAW)
                Fields["KerbalRecruitmentEnabled"].group.displayName = System.String.Format("<color=#BADA55>" + groupName + "</color>");
            else
                Fields["KerbalRecruitmentEnabled"].group.displayName = groupName;
        }

        /// <summary>Recruits the kerbal.</summary>
        [KSPEvent(guiName = "#MOAR-Academy-01",
                  groupName = "MoarKerbals",
                  active = true,
                  guiActive = true)]
        void RecruitKerbal()
        {
            Logging.DLog(logMsg: $"Academy: RecruitKerbal");
            if (KerbalRecruitmentEnabled)
            {
                Logging.DLog(logMsg: "Academy: Recruitment Button pressed!");

                bool changedTrait = false;
                List<ProtoCrewMember> vesselCrew;

                // need to be able to only affect one part
                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().entireVesselAcademy)
                {
                    vesselCrew = vessel.GetVesselCrew();
                }
                else
                {
                    // List<ProtoCrewMember> vesselCrew = vessel.GetVesselCrew();
                    vesselCrew = this.part.protoModuleCrew;
                }
                foreach (ProtoCrewMember crewMember in vesselCrew)
                {
                    //Logging.Msg(crewMember.name + " : " + crewMember.trait + ": " + crewMember.type, 3.5f, ScreenMessageStyle.UPPER_CENTER);
                    Logging.DLog(logMsg: Localizer.Format("#MOAR-Academy-06", crewMember.name, crewMember.trait, crewMember.type));

                    //if (crewMember.trait == "Civilian" && changedTrait == false)
                    if (crewMember.trait == Localizer.Format("#MOAR-004") && changedTrait == false)
                    {
                        if (GatherResources(part) && GatherCurrencies())
                        {
                            DebitCurrencies();
                            crewMember.trait = getRandomTrait();
                            if (crewMember.trait == Localizer.Format("#MOAR-004"))
                            {

                                Logging.Msg(s: Localizer.Format("#MOAR-Academy-08", crewMember.name));
                                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) FailureSound();
                                return;
                            }
                            crewMember.type = ProtoCrewMember.KerbalType.Crew;
                            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().recruitOnlyOne) changedTrait = true;
                            //KerbalRoster.SetExperienceTrait(crewMember, getRandomTrait());
                            KerbalRoster.SetExperienceTrait(crewMember, crewMember.trait);

                            Logging.Msg(s: Localizer.Format("#MOAR-Academy-07", crewMember.name, crewMember.trait)); // crewMember.name + " is now a " + crewMember.trait + "!"
                            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) SuccessSound();
                        }
                    }
                    if (!HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().recruitOnlyOne) changedTrait = true;
                }
                if (changedTrait) GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
                else Logging.Msg(s: "No civilians available to recruit");
            }
        }

        /// <summary>Play sound upon failure</summary>
        private protected void FailureSound()
        {
            Logging.DLog(logMsg: $"Academy: Failure Sound");
            int _soundSelection = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundClipC0;
            if (_soundSelection == 0)
            {
                System.Random newRand = new System.Random();
                _soundSelection = newRand.Next(1, 3);
            }
            switch (_soundSelection)
            {
                case 1:
                    dropOut0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    dropOut0.Play();
                    return;
                case 2:
                    dropOut1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    dropOut1.Play();
                    return;
                default:
                    return;
            }
        }

        /// <summary>Play sound upon success</summary>
        private protected void SuccessSound()
        {
            Logging.DLog(logMsg: $"Academy: Success Sound");
            int _soundSelection = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundClipC1;
            if (_soundSelection == 0)
            {
                System.Random newRand = new System.Random();
                _soundSelection = newRand.Next(1, 3);
            }
            switch (_soundSelection)
            {
                case 1:
                    graduation0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    graduation0.Play();
                    return;
                case 2:
                    graduation1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    graduation1.Play();
                    return;
                default:
                    return;
            }
        }

        private protected string getRandomTrait()
        {
            Logging.DLog(logMsg: $"Academy: getRandomTrait");

            int numberOfClasses = 3;
            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().dropOut) numberOfClasses = 4;

            string kerbalTrait = "";
            System.Random newRand = new System.Random();

            switch (newRand.Next() % numberOfClasses)
            {
                case 0:
                    kerbalTrait = Localizer.Format("#autoLOC_8005006"); //  "Pilot";
                    break;
                case 1:
                    kerbalTrait = Localizer.Format("#autoLOC_8005007"); // "Engineer"; 
                    break;
                case 2:
                    kerbalTrait = Localizer.Format("#autoLOC_8005008"); // "Scientist"; 
                    break;
                case 3:
                    kerbalTrait = Localizer.Format("#MOAR-004"); // "Civilian"; 
                    if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) FailureSound();
                    break;
                default:
                    break;
            }
            Logging.Msg(s: String.Format("Created trait:  {0}", kerbalTrait));
            return kerbalTrait;
        }

        /// <summary>What shows up in editor for the part</summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            //string display = "\r\n<color=#BADA55>Input:</color>\r\n One Civilian Kerbal";
            string display = String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-005") + ":</color>\r\n" + Localizer.Format("#MOAR-Academy-03") + " " + Localizer.Format("#MOAR-004") + " " + Localizer.Format("#MOAR-Academy-04") + ".\r\n");

            display += String.Format("\r\n\t" + Localizer.Format("#MOAR-Kuddle-13") + "\r\n");

            for (int i = 0; i < resourceRequired.Count; i++)
                display += String.Format("\t{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n";

            //display += "\r\n<color=#BADA55>Output:</color>\r\n Pilot, Engineer, Scientist Kerbal (random)\reating a MinmusMint ice cream cone.";
            display += String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-006") + ":</color>\r\n" + Localizer.Format("#MOAR-Academy-05") + ".\r\n");

            return display;
        }
    }
}
