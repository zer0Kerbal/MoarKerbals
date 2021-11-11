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
using System.Linq;
using UnityEngine;
using KSP.Localization;
using System.Collections.Generic;

namespace MoarKerbals
{
    /// <summary>KloneBay part module</summary>
    [KSPModule("KloneBay")]
    public class KloneBay : MoarKerbalsBase
    {
        // private const string V = groupName; Localizer.Format("#MOAR-003", Version.Text);
        [KSPField]
        public double accidentRate;

        [KSPField]
        public bool allowOrbital = false;

        [KSPField]
        public bool allowSplashedOrLanded = true;

        protected AudioSource overload0;
        protected AudioSource kloning_success0;

        protected AudioSource overload1;
        protected AudioSource kloning_success1;

        [KSPField(guiName = "#MOAR-KloneBay-00",
                  groupName = "MoarKerbals",
                  groupDisplayName = "#MOAR-001",
                  groupStartCollapsed = true,
                  guiActive = true,
                  guiActiveEditor = true,
                  isPersistant = true),
                  UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool KloningKabinEnabled = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            Logging.DLog("Kloning: OnStart");

            RequireLivingKerbal = HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().coloredPAW)
                Fields["KloningKabinEnabled"].group.displayName = System.String.Format("<color=#BADA55>" + groupName + "</color>");
            else
                Fields["KloningKabinEnabled"].group.displayName = groupName;

            kloning_success0 = gameObject.AddComponent<AudioSource>();
            kloning_success0.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/kloning");
            kloning_success0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            kloning_success0.panStereo = 0;
            kloning_success0.rolloffMode = AudioRolloffMode.Linear;
            kloning_success0.Stop();

            kloning_success1 = gameObject.AddComponent<AudioSource>();
            kloning_success1.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/save");
            kloning_success1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            kloning_success1.panStereo = 0;
            kloning_success1.rolloffMode = AudioRolloffMode.Linear;
            kloning_success1.Stop();

            overload0 = gameObject.AddComponent<AudioSource>();
            overload0.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/overload");
            overload0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            overload0.panStereo = 0;
            overload0.rolloffMode = AudioRolloffMode.Linear;
            overload0.Stop();

            overload1 = gameObject.AddComponent<AudioSource>();
            overload1.clip = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/negative");
            overload1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
            overload1.panStereo = 0;
            overload1.rolloffMode = AudioRolloffMode.Linear;
            overload1.Stop();

            switch (accidentRate)
            {
                default:
                    accidentRate *= 100;
                    break;
                case < 0:
                    accidentRate = 0;
                    break;
                case > 1:
                    accidentRate = 100;
                    break;
            }

            //Events["ActivateKuddling"].guiName = "#MOAR-Kuddle-00"; // initiateAction;
            Events["ActivateKloning"].guiName = Localizer.Format("#MOAR-KloneBay-01"); //  initiateAction;
        }

        public void OnFixedUpdate()
        {
            RequireLivingKerbal = HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().coloredPAW)
                Fields["KloningKabinEnabled"].group.displayName = String.Format("<color=#BADA55>" + groupName + "</color>");
            else
                Fields["KloningKabinEnabled"].group.displayName = groupName;
        }

        //guiName = "Activate Kloning!"
        [KSPEvent(guiName = "Initiate Kloning! ", //"#MOAR-KloneBay-01",
                  groupName = "MoarKerbals",
                  isPersistent = true,
                  active = true,
                  guiActive = true,
                  guiActiveUncommand = true,
                  guiActiveUnfocused = true)]
        public void ActivateKloning()
        {
            Logging.DLog(logMsg: "Kloning: ActivateKloning");
            if (KloningKabinEnabled)
            {
                Logging.DLog(logMsg: "Kloning Kabin Enabled");
                // determine if vessel is landed or splashed or orbiting and if it kloning is allowed
                // display message if not allowed and return
                if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().situationallyAware)
                {
                    Logging.DLog(logMsg: "Situationally Aware");
                    switch (vessel.situation)
                    {
                        case Vessel.Situations.LANDED:
                            Logging.DLog(logMsg: $"Vessel Situation: {Vessel.Situations.LANDED}");
                            if (!allowSplashedOrLanded)
                            {
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-02-l"));
                                return;
                            }
                            break;
                        case Vessel.Situations.SPLASHED:
                            Logging.DLog(logMsg: $"Vessel Situation: {Vessel.Situations.SPLASHED}");
                            if (!allowSplashedOrLanded)
                            {
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-02-s"));
                                return;
                            }
                            break;
                        case Vessel.Situations.ORBITING:
                            Logging.DLog(logMsg: $"Vessel Situation: {Vessel.Situations.ORBITING}");
                            if (!allowOrbital)
                            {
                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-03"));
                                return;
                            }
                            break;
                        default:
                            Logging.DLog(logMsg: $"Vessel Situation (other): {vessel.situation}");
                            break;
                    }
                }

                // there has to be a better way to write the following. :D
                if (PartHasRoom(part) && GatherResources(part) && GatherCurrencies())
                {
                    var rnd = new System.Random();
                    double localDouble = rnd.Next(1, 101);
                    // No accidents
                    if (accidentRate == 0)
                    {
                        localDouble = 100d;
                    }
                    else
                    {
                        if (accidentRate <= localDouble)
                        {
                            Logging.DLog(logMsg: $"Accident: roll {localDouble:F0} vs {accidentRate}");
                            KloneKerbal();
                            // twins?
                            if (AdditionalBirths(rnd))
                            {
                                int count = 2;
                                while (accidentRate <= localDouble && count <= 9 && HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().KribbleMode)
                                {
                                    KloneKerbal();
                                    switch (count)
                                    {
                                        case 2: // twins
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-25", true));
                                            break;
                                        case 3: // triplets
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-26", true));
                                            break;
                                        case 4: // quadruplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-27", true));
                                            break;
                                        case 5: // quintuplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-28", true));
                                            break;
                                        case 6: // Sextuplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-29", true));
                                            break;
                                        case 7: // Septuplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-30", true));
                                            break;
                                        case 8: // Octuplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-31", true));
                                            break;
                                        case 9: // Nonuplets?
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-32", true));
                                            // no more please!
                                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-33", true));
                                            break;
                                        default:
                                            break;
                                    }
                                    count++;
                                }
                            }
                        }


                        //if (AdditionalBirths(rnd))
                        //{
                        //    Logging.Msg(Localizer.Format("#MOAR-KloneBay-25", true));
                        //    KloneKerbal();
                        //    // triplets? (Only if KribbleMode enabled)
                        //    if (AdditionalBirths(rnd) && HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().KribbleMode)
                        //    {
                        //        Logging.Msg(Localizer.Format("#MOAR-KloneBay-26", true));
                        //        KloneKerbal();
                        //        // quadruplets?
                        //        if (AdditionalBirths(rnd))
                        //        {
                        //            Logging.Msg(Localizer.Format("#MOAR-KloneBay-27", true));
                        //            KloneKerbal();
                        //            // quintuplets?
                        //            if (AdditionalBirths(rnd))
                        //            {
                        //                Logging.Msg(Localizer.Format("#MOAR-KloneBay-28", true));
                        //                KloneKerbal();
                        //                // Sextuplets?
                        //                if (AdditionalBirths(rnd))
                        //                {
                        //                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-29", true));
                        //                    KloneKerbal();
                        //                    // Septuplets?
                        //                    if (AdditionalBirths(rnd))
                        //                    {
                        //                        Logging.Msg(Localizer.Format("#MOAR-KloneBay-30", true));
                        //                        KloneKerbal();
                        //                        // Octuplets?
                        //                        if (AdditionalBirths(rnd))
                        //                        {
                        //                            Logging.Msg(Localizer.Format("#MOAR-KloneBay-31", true));
                        //                            KloneKerbal();
                        //                            // Nonuplets?
                        //                            if (AdditionalBirths(rnd))
                        //                            {
                        //                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-32", true));
                        //                                KloneKerbal();
                        //                                // no more please!
                        //                                Logging.Msg(Localizer.Format("#MOAR-KloneBay-33", true));
                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        // bad events follow
                        // if (!HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal) - don't need because switch factors that in
                        else
                        {
                            switch (part.protoModuleCrew.Count)
                            {
                                case int n when n <= 0:
                                    NoCrewBadResult(rnd);
                                    break;
                                case int n when n == 0:
                                    SoloCrewBadResult(rnd);
                                    break;
                                case int n when n > 1:
                                    ScaryMovieMode(rnd);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
                }
                //            HorrorMovieCrew.Add(crewMember);
            }
        }

        private bool AdditionalBirths(System.Random rnd)
        {
            double localDouble = 0d;
            if (accidentRate != 0)
            {
                localDouble = rnd.Next(0, 500);
                Logging.DLog(logMsg: $"AdditionalBirths: roll: {localDouble:F0} vs. {accidentRate}");
            }
            else Logging.DLog(logMsg: $"Accidents disabled by setting accidentRate to {accidentRate}");
            return (localDouble <= accidentRate) && PartHasRoom(part) && GatherResources(part) && GatherCurrencies();
        }

        /// <summary>Let's get bloody. Checks settings to see if their is a limits to the number of accident victims</summary>
        /// <param name="rnd">The random seed.</param>
        /// <returns></returns>
        private protected void ScaryMovieMode(System.Random rnd)
        {
            Logging.DLog(logMsg: "Kloning: ScaryMovieMode");
            // there is always one victim
            SoloCrewBadResult(rnd);
            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().QuentinTarantinoMode == 0) MaximumKarnage(rnd);
            else LetsGetBloody(rnd);
            return;
        }

        /// <summary>Let's get bloody. Limits the number of accident victims</summary>
        /// <param name="rnd">The random seed.</param>
        /// <returns></returns>
        // this can be combined with the 'MaximumKarnage' method, just use an if with maxFatalites.
        private protected void LetsGetBloody(System.Random rnd)
        {
            Logging.DLog(logMsg: "Kloning: LetsGetBloody");
            int maxFatalities = HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().QuentinTarantinoMode; // 1 to 10
            int fatalities = 0; // number of victims

            List<ProtoCrewMember> HorrorMovieCast = new List<ProtoCrewMember>(); // list of victims

            // until the maximum fatalities are reached, do foreach part.protomodulecrew and add members who fail rnd.Next(1, 101) <= accidentRate to the list of casualties
            foreach (ProtoCrewMember crewMember in part.protoModuleCrew)
            {
                if (fatalities <= maxFatalities)
                {
                    double localDouble = rnd.Next(0, 100);
                    Logging.DLog(logMsg: $"Fatalities: roll: {localDouble:F0} vs. {accidentRate}");
                    if (localDouble <= accidentRate)
                    {
                        HorrorMovieCast.Add(crewMember);
                        fatalities++;
                    }
                }
                else break;
            }

            // now we have a list of casualties, let's kill them
            // while CrewCount > 0 fatalities < maxFatalities 
            foreach (ProtoCrewMember victim in HorrorMovieCast)
            {
                switch (part.protoModuleCrew.Count)
                {
                    case < 1:
                        SoloCrewBadResult(rnd);
                        return;
                    case 1:
                        SoloCrewBadResult(rnd);
                        return;
                    case > 1:
                        MultipleCrewBadResult(victim, rnd);
                        break;
                }
            }
        }

        /// <summary>Maximizes the kerbal karnage. Unlimited number of accident victims
        /// Each kerbal in part makes a save vs accidentRate, if fail, is zapped by the accident</summary>
        /// <param name="rnd">The random seed.</param>
        /// <returns></returns>
        private protected void MaximumKarnage(System.Random rnd)
        {
            Logging.DLog(logMsg: "Kloning: MaximumKarnage");
            int maxFatalities = part.protoModuleCrew.Count; // total possible number of casualties
            int fatalities = 0; // number of victims

            List<ProtoCrewMember> HorrorMovieCast = new List<ProtoCrewMember>(); // list of victims

            // until the maximum fatalities are reached, do foreach part.protomodulecrew and add members who fail rnd.Next(1, 101) <= accidentRate to the list of casualties
            foreach (ProtoCrewMember crewMember in part.protoModuleCrew)
            {
                if (fatalities <= maxFatalities)
                {
                    double localDouble = rnd.Next(0, 100);
                    Logging.DLog(logMsg: $"Fatalities: roll: {localDouble:F0} vs. {accidentRate}");
                    if (localDouble <= accidentRate)
                    {
                        HorrorMovieCast.Add(crewMember);
                        Logging.DLog(logMsg: $"Kloning: {crewMember.displayName} is about to die... Needs food badly.");
                        fatalities++;
                    }
                }
                else break;
            }

            // now we have a list of casualties, let's kill them
            // while CrewCount > 0 fatalities < maxFatalities 
            foreach (ProtoCrewMember victim in HorrorMovieCast)
            {
                switch (part.protoModuleCrew.Count)
                {
                    case < 1:
                        SoloCrewBadResult(rnd);
                        return;
                    case 1:
                        SoloCrewBadResult(rnd);
                        return;
                    case > 1:
                        MultipleCrewBadResult(victim, rnd);
                        break;
                }
            }
        }

        private protected void MultipleCrewBadResult(ProtoCrewMember crewman, System.Random rnd)
        {
            string culprit = crewman.displayName;
            Logging.DLog(logMsg: $"Kloning: MultipleCrewBadResult culprit{culprit} did it in the kitchen with a frozen fish stick and custard!");

            part.RemoveCrewmember(crewman);
            Logging.Msg(Localizer.Format("#MOAR-KloneBay-19", crewman.name));

            crewman.Die();
            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) FailureSound();

            double localDouble = rnd.Next(0, 100);
            Logging.DLog(logMsg: $"roll: {localDouble:F0}");
            switch (localDouble)
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

        private void SoloCrewBadResult(System.Random rnd)
        {
            double localDouble = rnd.Next(0, 100);
            Logging.DLog(logMsg: $"Kloning: SoloCrewBadResult roll: {localDouble:F0}");
            switch (localDouble)
            {
                case < 05:
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-10", part.protoModuleCrew.First().name) + ".");
                    break;
                case < 30:
                    // Logging.Msg(culprit + " comes out with a goatee, yelling something about a Terran Empire.");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-11", part.protoModuleCrew.First().name) + ".");
                    break;
                case < 60:
                    // Logging.Msg(culprit + " mutates into a giant fly, but goes on to have a successful movie career.");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-12", part.protoModuleCrew.First().name) + ".");
                    break;
                case < 80:
                    //  Logging.Msg("You find the bay empty, a note pinned to the wall 'Gone back to the future'");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-13") + ".");
                    break;
                default:
                    // Logging.Msg("The kloning process failed.  You lost " + culprit + ", but at least you have pizza now.");
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-14", part.protoModuleCrew.First().name) + ".");
                    break;
            }
            zapCrewmember();
        }

        private static void NoCrewBadResult(System.Random rnd)
        {
            double localDouble = rnd.Next(0, 101);
            Logging.DLog(logMsg: $"Kloning: noCrewBadResults roll: {localDouble:F0}");
            switch (localDouble)
            {
                case < 05:
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-23") + ".");
                    break;
                case < 30:
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-20") + ".");
                    break;
                case < 60:
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-21") + ".");
                    break;
                case <= 100:
                    Logging.Msg(Localizer.Format("#MOAR-KloneBay-22") + ".");
                    break;
                default:
                    Logging.DLogWarning(logMsg: "Kloning: NoCrewBadResult out of bounds.");
                    break;
            }
        }

        private protected void KloneKerbal()
        {
            Logging.DLog(logMsg: "Kloning: KloneKerbal");
            DebitCurrencies();
            ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
            kerbal.suit = (ProtoCrewMember.KerbalSuit)HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().birthdaySuit;
            kerbal.trait = Localizer.Format("#MOAR-004");

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().kloneCivilians) KerbalRoster.SetExperienceTrait(kerbal, Localizer.Format("#MOAR-004")); ;
            part.AddCrewmember(kerbal);

            if (kerbal.seat != null)
                kerbal.seat.SpawnCrew();

            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            //Logging.Msg("Kloning Success!  " + kerbal.name + "(Lv " + kerbal.experienceLevel.ToString() + " " + kerbal.experienceTrait.Title + ") has joined your space program");
            Logging.Msg(Localizer.Format("#MOAR-KloneBay-04", kerbal.name, kerbal.experienceLevel.ToString(), kerbal.experienceTrait.Title), true);

            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) SuccessSound();
        }

        private protected void zapCrewmember()
        {
            Logging.DLog("Kloning: zapCrewmember");
            ProtoCrewMember crewman = part.protoModuleCrew.First();

            Logging.DLog(logMsg: $"Kloning: zapCrewmember {crewman.displayName}");

            part.RemoveCrewmember(crewman);

            Logging.Msg(Localizer.Format("#MOAR-KloneBay-19", crewman.name));
            crewman.Die();
            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().soundOn) FailureSound();
        }

        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private protected bool PartHasRoom(Part part)
        {
            Logging.DLog(logMsg: "Kloning: PartHasRoom");
            // see if there is room left for a new klone
            Logging.DLog(logMsg: $"Crew counts {part.protoModuleCrew.Count} / {part.CrewCapacity} and needs living? {HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal}");

            // occupiedSpace = if require a living kerbal to klone sets to 1, otherwise sets to 0
            int occupiedSpace = (HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal ? 1 : 0);

            /* logic graph
             * 
             * require: false && count < max then return true
             * require: true && if count > 0 && count < max - 1 then return true
             * else false
             *
            */

            if (part.protoModuleCrew.Count == (part.CrewCapacity - occupiedSpace))
            {
                Logging.Msg(Localizer.Format("#MOAR-KloneBay-17", part.partName)); // "No room left in Kloning Bay"
                Logging.DLog(logMsg: $"Kloning, No room left in: {part.partName}");
                return false;
            }

            // check for needing kerbal availability
            if (part.protoModuleCrew.Count == 0 && HighLogic.CurrentGame.Parameters.CustomParams<Settings2>().requireLivingKerbal)
            {
                Logging.Msg(Localizer.Format("#MOAR-KloneBay-16")); // "Kloning requires at least one test subject"
                Logging.DLog(logMsg: $"Kloning requires at least one test subject, no one in: {part.partName}");
                return false;
            }
            else Logging.DLog(logMsg: Localizer.Format(Localizer.Format("#MOAR-KloneBay-15"))); // "Kloning does not need a living kerbal"
            return true;
        }

        /// <summary>Play sound upon failure</summary>
        internal void FailureSound()
        {
            Logging.DLog(logMsg: "Kloning: FailureSound");
            int _soundSelection = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundClipA0;
            if (_soundSelection == 0)
            {
                var newRand = new System.Random();
                _soundSelection = newRand.Next(1, 3);
            }
            switch (_soundSelection)
            {
                case 1:
                    overload0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    overload0.Play();
                    return;
                case 2:
                    overload1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    overload1.Play();
                    return;
                default:
                    return;
            }
        }

        /// <summary>Play sound upon success</summary>
        internal void SuccessSound()
        {
            Logging.DLog(logMsg: "Kloning: SuccessSound");
            int _soundSelection = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundClipA1;
            if (_soundSelection == 0)
            {
                var newRand = new System.Random();
                _soundSelection = newRand.Next(1, 3);
            }
            switch (_soundSelection)
            {
                case 1:
                    kloning_success0.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    kloning_success0.Play();
                    return;
                case 2:
                    kloning_success1.volume = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundVolume;
                    kloning_success1.Play();
                    return;
                default:
                    return;
            }
        }

        /// <summary>Module information shown in editors</summary>
        private string info = string.Empty;

        private bool RequireLivingKerbal = true;

        /// <summary>What shows up in editor for the part</summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;
            if (info == string.Empty)
            {

                /* :::This is what it should look like with default settings:::
                 * 
                 * MoarKerbals
                 * Kerbthulhu Kinetics Program
                 * v 1.3.0.0
                 * 
                 * Input: (color)
                 * May or May not need a living Kerbal. Could also use frozen MinimusMint Ice Cream.
                 * 
                 * Required Resources: (color)
                 *   ElectricCharge: 8000
                 *   Oxidizer: 100
                 *   Ore: 500
                 * 
                 * Required Currency: (color)
                 *   Funds 1000
                 *   Science: 1
                 *   Reputation: 2
                 * 
                 * Output:
                 * Anything from one Kerbal to a deep dish pizza.
                 * 
                 */
                info += Localizer.Format("#MOAR_manu"); // #MOAR_manu = Kerbthulhu Kinetics Program
                info += "\n v" + Version.SText; // mod Version Number text

                info = String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-005") + ":</color>\r\n"); // Input

                info += String.Format(Localizer.Format("#MOAR-KloneBay-24" + "\r\n")); // May or May not need a living Kerbal. Could also use frozen MinimusMint Ice Cream."

                switch (RequireLivingKerbal)
                {
                    case true:
                        info += "\r\n Needs living Kerbal.";
                        break;
                    case false:
                        info += "\r\n Frozen sample used, does not needs living Kerbal.";
                        break;
                }

                // resource section header
                info += String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-008") + ":</color>\r\n"); // Required Resources:
                for (int i = 0; i < resourceRequired.Count; i++)
                {
                    //display += String.Format("{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n");
                    info += $"\t{resourceRequired[i].resource}: {resourceRequired[i].amount:F2} \r\n";
                }

                // currency section header
                if ((costFunds != 0) || (costScience != 0) || (costReputation != 0))
                {
                    info += String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-009") + "</color>\r\n");
                }

                if (costFunds != 0) info += $"\r\n\t{costFunds:F0} {Localizer.Format("#autoLOC_7001031")}";
                if (costScience != 0) info += $"\r\n\t{costScience:F0} {Localizer.Format("#autoLOC_7001032")}";
                if (costReputation != 0) info += $"\r\n\t{costReputation:F0} {Localizer.Format("#autoLOC_7001033")}";

                info = String.Format("\r\n<color=#FFFF19>" + Localizer.Format("#MOAR-006") + ":</color>\r\n"); // Output:
                info += String.Format(Localizer.Format("#MOAR-KloneBay-18") + "."); // Anything from one Kerbal to a deep dish pizza."

            }
            return info;
        }
    }
}
