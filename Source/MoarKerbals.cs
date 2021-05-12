using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoarKerbals
{
    /// <summary>
    /// 
    /// </summary>
    [KSPModule("KloneBay")]
    public class KloneBay : MoarKerbalBase
    {
#if false
		[KSPField]
		public string recipeIngredients;
		[KSPField]
		public string recipeAmounts;
		[KSPField]
		public string initiateAction = "Initiate Kloning!";
#endif

        [KSPField]
        public double accidentRate;
        [KSPField]
        public bool allowOrbital = false;
        [KSPField]
        public bool allowSplashedOrLanded = true;

#if false
		private string[] resourceList;
		private double[] resourceAmounts;
		private string[] str_amounts;
		private string[] resList; // = recipeIngredients.Split(',');
#endif

        protected AudioSource overload;
        protected AudioSource kloning_success;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
#if false
			resourceList = recipeIngredients.Split(',');
			str_amounts = recipeAmounts.Split(',');
			resList = recipeIngredients.Split(',');
			resourceAmounts = new double[str_amounts.Length];
			for (int i = 0; i < resourceList.Length; i++)
			{
				resourceList[i] = resourceList[i].Trim();
				resourceAmounts[i] = double.Parse(str_amounts[i]);
			}
#endif

            if (accidentRate < 0)
                accidentRate = 0;
            else if (accidentRate > 1)
                accidentRate = 100;
            else
                accidentRate *= 100;

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

        [KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
        public void ActivateKlone()
        {
            if (this.vessel.LandedOrSplashed)
            {
                if (!allowSplashedOrLanded)
                {
                    Utilities.msg("Unable to klone kerbals while landed or splashed");
                    return;
                }
            }
            else
            {
                if (!allowOrbital)
                {
                    Utilities.msg("Unable to klone kerbals while in space");
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
                    //kerbal.type = ProtoCrewMember.KerbalType.Crew;
                    kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();


                    Utilities.msg("Kloning Success!  " + kerbal.name + "(Lv " + kerbal.experienceLevel.ToString() + " " + kerbal.experienceTrait.Title + ") has joined your space program");
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
                        // crewman.rosterStatus = ProtoCrewMember.RosterStatus.Missing;

                    }
                    if (count > 1)
                    {
                        if (opt < 30)
                            Utilities.msg("A power surge opens a portal to R'lyeh and all your test subjects vanish!");
                        else if (opt < 60)
                            Utilities.msg("Your Kerbals are lost in the Matrix!");
                        else if (opt < 85)
                            Utilities.msg("Radiation turns " + culprit + " into a monstrous creature from the deep who eats your other kerbals before shambling off!");
                        else Utilities.msg("The Kloning process failed.  This is going to take some cleaning.");
                    }
                    else
                    {
                        if (opt < 30)
                            Utilities.msg(culprit + " comes out with a goatee, yelling something about a Terran Empire.");
                        else if (opt < 60)
                            Utilities.msg(culprit + " mutates into a giant fly, but goes on to have a successful movie career.");
                        else if (opt < 80)
                            Utilities.msg("You find the bay empty, a note pinned to the wall 'Gone back to the future'");
                        else
                            Utilities.msg("The Kloning process failed.  You lost " + culprit + ", but at least you have pizza now.");
                    }
                    if (SettingsInterface.SoundOn()) overload.Play();
                }
            }
            GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }


        public override string GetInfo()
        {
            base.OnStart(StartState.None);

            double gblMult = 1; // HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;

            string display = "\r\nInput:\r\n";
            for (int i = 0; i < resourceRequired.Count; i++)
                display += String.Format("\r\n{0:0,0}", resourceRequired[i].amount) + " " + resourceRequired[i].resource + "\r\n";
            display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
            //if (SettingsInterface.RequireLivingKerbal()) display += "\r\n Needs living Kerbal.";
            return display;
        }

        //Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
        private bool PartHasRoom(Part part)
        {
            if (!SettingsInterface.RequireLivingKerbal())
            {
                //ScreenMessages.PostScreenMessage("Kloning does not need a living kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
                Utilities.msg("Kloning does not need a living kerbal", 5f, ScreenMessageStyle.UPPER_CENTER);
            }
            else if (part.protoModuleCrew.Count == 0)
            {
                Utilities.msg("Kloning requires at least one test subject", 5f, ScreenMessageStyle.UPPER_CENTER);
                //Log.dbg(String.Format("Kloning requires at least one test subject, no one in: {0}", part.name.ToString()));
                return false;
            }
            // Log.dbg("Crew counts {0} = {1}", part.protoModuleCrew.Count, part.CrewCapacity);
            if (part.protoModuleCrew.Count == part.CrewCapacity)
            {
                Utilities.msg("No room left in Kloning Bay", 5f, ScreenMessageStyle.UPPER_CENTER);
                //Log.dbg("Kloning requires at least one test subject, No room left in: ", part.name);
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
    }
}
