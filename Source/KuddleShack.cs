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
	[KSPModule("KuddleShack")]	
	public class KuddleShack : PartModule
	{
		[KSPField]
		public string recipeIngredients;
		[KSPField]
		public string recipeAmounts;
		[KSPField]
		public bool accidentRate;


		private string[] resList; // = recipeIngredients.Split(',');
		private string[] resourceList;
		private double[] resourceAmounts;
		private string[] str_amounts;

		public override void OnStart(PartModule.StartState state)
		{
			resourceList = recipeIngredients.Split(',');
			str_amounts = recipeAmounts.Split(',');
			resList = recipeIngredients.Split(',');
			resourceAmounts = new double[str_amounts.Length];
			for (int i = 0; i < resourceList.Length; i++)
				resourceAmounts[i] = double.Parse(str_amounts[i]);
		}

		[KSPEvent(active = true, guiActive = true, guiName = "Initiate Kloning!")]
		
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

										 
		private bool GatherResources(Part part)
		{
			//Steps through to gather resources
			for (int i = 0; i < resourceList.Length; i++)
			{
				Log.dbg("GlobalScale: {1}", SettingsInterface.globalKloningCostMultiplier());
				double available = part.RequestResource(resourceList[i], resourceAmounts[i]);
				//debug:
				//Log.dbg("Costs: {1} : resourceAmounts: {2}", res  resList[i], resourceList[i]), available, resourceAmounts[i]);
				if (available != resourceAmounts[i])
				{
					//Upon not having enough of a resource, returns all previously collected
					part.RequestResource(resourceList[i], -available);
					for (int j = 0; j < i; j++)
						part.RequestResource(resourceList[j], -resourceAmounts[j]);
					ScreenMessages.PostScreenMessage("Insufficient " + resourceList[i] + " to start Kloning (" + available.ToString() + "/" + resourceAmounts[i].ToString() + ")", 3.5f, ScreenMessageStyle.UPPER_CENTER);				 return false;
																							
				}
			}
			return true;
		}

		//Checks to make sure there is at least one kerbal as a DNA source and that there is room to store the new kerbal
		private bool PartHasRoom(Part part)
		{
			if ((part.protoModuleCrew.Count < part.CrewCapacity) && ((part.protoModuleCrew.Count > 0) || SettingsInterface.RequireLivingKerbal()))
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
			double[] resAmounts = new double[amounts.Length];

			for (int i = 0; i < resList.Length; i++)
				resAmounts[i] = double.Parse(amounts[i]);

			string display = "\r\nInput:\r\n";
			for (int i = 0; i < resList.Length; i++)
				display += String.Format("{0:0,0}", resAmounts[i]) + " " + resList[i] + "\r\n";

			display += "\r\nOutput:\r\n Anything from one Kerbal to a deep dish pizza.";
			return display;
		}

	}
}
