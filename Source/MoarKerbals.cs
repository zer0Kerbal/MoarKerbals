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
		public double accidentRate;

		private string[] resourceList;
		private double[] resourceAmounts;
		private string[] str_amounts;
		private string[] resList; // = recipeIngredients.Split(',');

		protected AudioSource overload;
		protected AudioSource kloning_success;

		public override void OnStart(PartModule.StartState state)
		{
			resourceList = recipeIngredients.Split(',');
			str_amounts = recipeAmounts.Split(',');
			resList = recipeIngredients.Split(',');
			resourceAmounts = new double[str_amounts.Length];
			for (int i = 0; i < resourceList.Length; i++)
				resourceAmounts[i] = double.Parse(str_amounts[i]);
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
		}

		[KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
		public void ActivateKlone()
		{
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
			double gblMult = 1; // HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;
			
			string[] resList = recipeIngredients.Split(',');
			string[] amounts = recipeAmounts.Split(',');
			double[] resAmounts = new double[amounts.Length];

			for (int i = 0; i < resList.Length; i++)
				resAmounts[i] = double.Parse(amounts[i]) * gblMult;

			string display = "\r\nInput:\r\n";
			for (int i = 0; i < resList.Length; i++)
				display += String.Format("{0:0,0}", resAmounts[i]) + " " + resList[i] + "\r\n";

			display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
			//if (SettingsInterface.RequireLivingKerbal()) display += "\r\n Needs living Kerbal.";
			return display;
		}

		//Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
		private bool PartHasRoom(Part part)
		{
			if (!SettingsInterface.RequireLivingKerbal())
			{
				//ScreenMessages.PostScreenMessage("Kloning does not need a living kerbal", 3.5f, ScreenMessageStyle.UPPER_CENTER);
				Utilities.msg("Kloning does not need a living kerbal", 3.5f, ScreenMessageStyle.UPPER_CENTER);
			}
			else if (part.protoModuleCrew.Count == 0)
			{
				Utilities.msg("Kloning requires at least one test subject", 3.5f, ScreenMessageStyle.UPPER_CENTER);
				//Log.dbg(String.Format("Kloning requires at least one test subject, no one in: {0}", part.name.ToString()));
				return false;
			}
			// Log.dbg("Crew counts {0} = {1}", part.protoModuleCrew.Count, part.CrewCapacity);
			if (part.protoModuleCrew.Count == part.CrewCapacity)
			{
				Utilities.msg("No room left in Kloning Bay", 3.5f, ScreenMessageStyle.UPPER_CENTER);
				//Log.dbg("Kloning requires at least one test subject, No room left in: ", part.name);
				return false;
			}
			return true;
		}

		private bool GatherResources(Part part)
		{
			double gblMult = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;
			//Steps through to gather resources
			for (int i = 0; i < resourceList.Length; i++)
			{
				//Log.dbg(String.Format("GlobalScale: {#.##}", gblMult));
				double available = part.RequestResource(resourceList[i], (resourceAmounts[i] * gblMult));
				//debug:
				string dStr;
				dStr = " DEBUG: needs: " + resList[i] + "\n\nresourceAmounts: " + available.ToString() + "\n\nneed: " + (resourceAmounts[i] * gblMult).ToString();
				Utilities.msg(dStr, 5f, ScreenMessageStyle.UPPER_LEFT);

				//Log.dbg(String.Format(" DEBUG: {1} : resourceAmounts: {2} need: {3}", resList[i].ToString(),  available.ToString(), resourceAmounts[i].ToString()));
				if (available < (resourceAmounts[i] * gblMult))
				{
					//Upon not having enough of a resource, returns all previously collected
					part.RequestResource(resourceList[i], -available);
					for (int j = 0; j < i; j++)
						part.RequestResource(resourceList[j], -(resourceAmounts[i] * gblMult));

					//ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + (resourceAmounts[i] * gblMult).ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);
					Utilities.msg("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + (resourceAmounts[i] * gblMult).ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);
					return false;
				}
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
