// MoarKerbals.cs v1.3.0.0
#region CC BY-NC-SA 4.0
/* MoarKerbals.cs v1.0.0.0
 * 
 * KGEx's library for the Kerbal Space Program, by zer0Kerbal
 * 
 * (C) Copyright 2014, strideknight
 * (C) Copyright 2019, 2021 zer0Kerbal
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Kerbthulhu Kinetics Program is a fictitious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 * 
 * Note that KGEx is a fictitious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.*/
#endregion
#region Author: zer0Kerbal (zer0Kerbal@hotmail.com) original Author: strideknight
#endregion


#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace MoarKerbals
{
    /// <summary>KloneBay part module</summary>
    [KSPModule("KloneBay")]
    public class KloneBay : MoarKerbalBase
    {
        /// <summary>The accident rate</summary>
        [KSPField]
        public double accidentRate;

        /// <summary>allow orbital activity</summary>
        [KSPField]
        public bool allowOrbital = false;

        /// <summary>allow splashed or landed activity</summary>
        [KSPField]
        public bool allowSplashedOrLanded = true;

        /// <summary>overload sound</summary>
        protected AudioSource overload;
        /// <summary>kloning success sound</summary>
        protected AudioSource kloning_success;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            switch (accidentRate)
            {
                case < 0.0:
                    accidentRate = 0;
                    break;
                case > 1.0:
                    accidentRate = 100;
                    break;
                default:
                    accidentRate *= 100;
                    break;
            }
            //if (accidentRate < 0) accidentRate = 0;
            //else if (accidentRate > 1) accidentRate = 100;
            //else accidentRate *= 100;

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

            Events["ActivateKlone"].guiName = initiateAction;
        }

        //[KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
        [KSPEvent(active = true, guiActive = true, guiName = "#MOAR-KloneBay-01")]
        public void ActivateKlone()
        {
            if (this.vessel.LandedOrSplashed)
            {
                if (!allowSplashedOrLanded)
                {
                    // Logging.Msg("Unable to klone kerbals while landed or splashed");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-02"));
                    return;
                }
            }
            else
            {
                if (!allowOrbital)
                {
                    // Logging.Msg("Unable to klone kerbals while in space");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-03"));
                    return;
                }
            }
            if (PartHasRoom(part) && GatherResources(part))
            {
                System.Random rnd = new System.Random();
                if (accidentRate <= rnd.Next(1, 100))
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                    part.AddCrewmember(kerbal);
                    kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();


                    //Logging.Msg("Kloning Success!  " + kerbal.name + "(Lv " + kerbal.experienceLevel.ToString() + " " + kerbal.experienceTrait.Title + ") has joined your space program");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-04", kerbal.name, kerbal.experienceLevel.ToString(), kerbal.experienceTrait.Title), 5f, ScreenMessageStyle.UPPER_CENTER);
                    if (SettingsInterface.SoundOn()) kloning_success.Play();
                }
                else
                {
                    int opt = rnd.Next(0, 100);
                    string culprit = part.protoModuleCrew.First().name;
                    int count = part.protoModuleCrew.Count;
                    while (part.protoModuleCrew.Count > 0)
                    {
                        ProtoCrewMember crewman = part.protoModuleCrew.First();
                        part.RemoveCrewmember(crewman);
                        crewman.Die();
                    }
                    //if (count > 1)
                    //{
                    //    if (opt < 30)
                    //        Logging.Msg("A power surge opens a portal to R'lyeh and all your test subjects vanish!");
                    //    else if (opt < 60)
                    //        Logging.Msg("Your Kerbals are lost in the Matrix!");
                    //    else if (opt < 85)
                    //        Logging.Msg("Radiation turns " + culprit + " into a monstrous creature from the deep who eats your other kerbals before shambling off!");
                    //    else Logging.Msg("The Kloning process failed.  This is going to take some cleaning.");
                    //}
                    //else
                    //{
                    //    if (opt < 30)
                    //        Logging.Msg(culprit + " comes out with a goatee, yelling something about a Terran Empire.");
                    //    else if (opt < 60)
                    //        Logging.Msg(culprit + " mutates into a giant fly, but goes on to have a successful movie career.");
                    //    else if (opt < 80)
                    //        Logging.Msg("You find the bay empty, a note pinned to the wall 'Gone back to the future'");
                    //    else
                    //        Logging.Msg("The Kloning process failed.  You lost " + culprit + ", but at least you have pizza now.");
                    //}

                    if (count > 1)
                    {
                        switch (opt)
                        {
                            case < 10:
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-05") + ".");
                                break;
                            case < 30:
                                // Logging.Msg("A power surge opens a portal to R'lyeh and all your test subjects vanish!");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-06") + "!");
                                break;
                            case < 60:
                                // Logging.Msg("Your Kerbals are lost in the Matrix!");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-07") + "!");
                                break;
                            case < 85:
                                // Logging.Msg("Radiation turns " + culprit + " into a monstrous creature from the deep who eats your other kerbals before shambling off!");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-08", culprit) + "!");
                                break;
                            default:
                                // Logging.Msg("The Kloning process failed.  This is going to take some cleaning.");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-09") + ".");
                                break;
                        }
                    }
                    else
                    {
                        switch (opt)
                        {
                            case < 05:
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-10", culprit) + ".");
                                break;
                            case < 30:
                                // Logging.Msg(culprit + " comes out with a goatee, yelling something about a Terran Empire.");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-11", culprit) + ".");
                                break;
                            case < 60:
                                // Logging.Msg(culprit + " mutates into a giant fly, but goes on to have a successful movie career.");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-12", culprit) + ".");
                                break;
                            case < 80:
                                //  Logging.Msg("You find the bay empty, a note pinned to the wall 'Gone back to the future'");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-13") + ".");
                                break;
                            default:
                                // Logging.Msg("The Kloning process failed.  You lost " + culprit + ", but at least you have pizza now.");
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-14", culprit) + ".");
                                break;
                        }
                    }
                    if (SettingsInterface.SoundOn()) overload.Play();
                }
            }
            GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }


        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private bool PartHasRoom(Part part)
        {
            if (!Settings.RequireLivingKerbal())
            {
                // Logging.Msg("Kloning does not need a living kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                Logging.Msg(Localizer.Format("#MOAR-KloneBay-15"), 5f, ScreenMessageStyle.UPPER_CENTER);
            }
            else if (part.protoModuleCrew.Count == 0)
            {
                // Logging.Msg("Kloning requires at least one test subject", 5f, ScreenMessageStyle.UPPER_CENTER);
                Logging.Msg(Localizer.Format("#MOAR-KloneBay-16"), 5f, ScreenMessageStyle.UPPER_CENTER);

                Logging.DLog(String.Format("Kloning requires at least one test subject, no one in: {0}", part.name.ToString()));
                return false;
            }
            Logging.DLog(String.Format("Crew counts {0} = {1}", part.protoModuleCrew.Count, part.CrewCapacity));
            if (part.protoModuleCrew.Count == part.CrewCapacity)
            {
                // Logging.Msg("No room left in Kloning Bay", 5f, ScreenMessageStyle.UPPER_CENTER);
                Logging.Msg(Localizer.Format("#MOAR-KloneBay-17"), 5f, ScreenMessageStyle.UPPER_CENTER);

                Logging.DLog(String.Format("Kloning requires at least one test subject, No room left in: ", part.name.ToString()));
                return false;
            }
            return true;
        }


#if false
		ProtoCrewMember CreateRandomKerbal()
		{
			var roster = HighLogic.CurrentGame.CrewRoster;

			var newMember = roster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
			return newMember;
		}
#endif

        public override string GetInfo()
        {
            base.OnStart(StartState.None);

            //double gblMult = 1; // HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsSettings>().globalKloningCostMultiplier;

            //string display = "\r\nInput:\r\n";
            string display = String.Format("\r\n" + Localizer.Format("#MOAR-005") + ":\r\n");

            for (int i = 0; i < resourceRequired.Count; i++)
                display += String.Format("{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n";


            //display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
            display += String.Format("\r\n" + Localizer.Format("#MOAR-006") + ":\r\n" + Localizer.Format("#MOAR-KloneBay-18") + ".");

            //if (SettingsInterface.RequireLivingKerbal()) display += "\r\n Needs living Kerbal.";

            return display;
        }
    }
}
