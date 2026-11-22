// Based on CivilianPopulation recruitment (LinuxGuruGamer) and MoarKerbals (zer0Kerbal).
// See existing project licences and attribution.
using System;
using System.Linq;
using MoarKerbals.Core;

namespace MoarKerbals
{
    public class KerbalRecruitment : MoarKerbalsBase
    {
        [KSPField] public string recruitmentTraits = "Pilot,Engineer,Scientist";
        [KSPField(guiName = "#MOAR-Academy-00", groupName = "MoarKerbals", guiActive = true, guiActiveEditor = true, isPersistant = true),
            UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool KerbalRecruitmentEnabled;

        [KSPEvent(guiName = "Recruit civilian (random profession)", groupName = "MoarKerbals", guiActive = true)]
        public void RecruitKerbal()
        {
            if (!HighLogic.LoadedSceneIsFlight || !KerbalRecruitmentEnabled) return;
            var traits = Rules.Traits(recruitmentTraits, TraitExists);
            if (traits.Length == 0) { Logging.Msg("No valid recruitment professions are configured."); return; }
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>();
            var crew = (settings.entireVesselAcademy ? vessel.GetVesselCrew() : part.protoModuleCrew)
                .Where(k => k.type == ProtoCrewMember.KerbalType.Crew && k.trait == "Civilian").ToArray();
            if (crew.Length == 0) { Logging.Msg("No Civilians available to recruit."); return; }
            foreach (var civilian in crew)
            {
                string blocker = CompletionBlocker();
                if (blocker.Length != 0)
                {
                    Logging.Msg("Recruitment blocked: " + blocker + ". No payment taken.");
                    break;
                }
                // Preserve the old optional 25% dropout chance independently of how many professions exist.
                bool dropout = settings.dropOut && Random.Next(4) == 3;
                string trait = traits[Random.Next(traits.Length)];
                bool success = ExecutePaid(() =>
                {
                    if (civilian.trait != "Civilian" || !TraitExists(trait)) return false;
                    if (dropout) return true;
                    var host = vessel.parts.FirstOrDefault(p => p.protoModuleCrew.Contains(civilian));
                    if (host == null) return false;
                    civilian.UnregisterExperienceTraits(host);
                    try
                    {
                        KerbalRoster.SetExperienceTrait(civilian, trait);
                        if (civilian.trait != trait || civilian.experienceTrait == null || civilian.experienceTrait.TypeName != trait)
                            throw new InvalidOperationException("Profession could not be assigned.");
                        civilian.RegisterExperienceTraits(host);
                        return true;
                    }
                    catch
                    {
                        KerbalRoster.SetExperienceTrait(civilian, "Civilian");
                        civilian.RegisterExperienceTraits(host);
                        throw;
                    }
                });
                if (!success) break;
                Logging.Msg(dropout ? civilian.displayName + " did not graduate and remains a Civilian." :
                    civilian.displayName + " is now a " + civilian.experienceTrait.Title + ".");
                PlaySound("C", !dropout);
                NotifyCrewChanged();
                if (settings.recruitOnlyOne) break;
            }
        }

        public override string GetInfo() => RecipeInfo("Recruits Civilians into a random configured profession. Existing experience and flight history are preserved. Professions: " + recruitmentTraits);
    }
}
