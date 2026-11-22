// Derived from MoarKerbals by strideknight and zer0Kerbal; CC-BY-NC-SA-4.0.
using System;
using System.Linq;
using KSP.Localization;
using MoarKerbals.Core;

namespace MoarKerbals
{
    [KSPModule("KloneBay")]
    public class KloneBay : MoarKerbalsBase
    {
        [KSPField] public double accidentRate;
        [KSPField] public bool allowOrbital;
        [KSPField] public bool allowSplashedOrLanded = true;
        [KSPField(guiName = "#MOAR-KloneBay-00", groupName = "MoarKerbals", guiActive = true, guiActiveEditor = true, isPersistant = true),
            UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool KloningKabinEnabled;

        [KSPField(isPersistant = true)] public string sourceKerbalName = "";
        [KSPField(guiName = "Clone source", groupName = "MoarKerbals", guiActive = true)]
        public string sourceDisplay = "";

        private ProtoCrewMember Source(bool selectDefault)
        {
            var source = part.protoModuleCrew.FirstOrDefault(k => k.name == sourceKerbalName);
            if (source == null && selectDefault && string.IsNullOrEmpty(sourceKerbalName))
            {
                source = part.protoModuleCrew.FirstOrDefault();
                if (source != null) sourceKerbalName = source.name;
            }
            return source;
        }

        public void Update()
        {
            // Stock OnUpdate is skipped for IDLE parts; the source selector must still work there.
            if (!HighLogic.LoadedSceneIsFlight || part == null) return;
            bool copy = HighLogic.CurrentGame != null && HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes;
            Fields[nameof(sourceDisplay)].guiActive = copy;
            Events[nameof(NextCloneSource)].guiActive = copy && part.protoModuleCrew.Count > 0;
            var source = copy ? Source(true) : null;
            sourceDisplay = source == null ? "Select a source aboard this cabin" : source.displayName + " (" + (source.experienceTrait == null ? source.trait : source.experienceTrait.Title) + ", level " + source.experienceLevel + ")";
        }

        [KSPEvent(guiName = "Next clone source", groupName = "MoarKerbals", guiActive = true)]
        public void NextCloneSource()
        {
            if (part.protoModuleCrew.Count == 0) return;
            int index = part.protoModuleCrew.FindIndex(k => k.name == sourceKerbalName);
            sourceKerbalName = part.protoModuleCrew[(index + 1) % part.protoModuleCrew.Count].name;
        }

        [KSPEvent(guiName = "#MOAR-KloneBay-01", groupName = "MoarKerbals", guiActive = true, guiActiveUnfocused = true)]
        public void ActivateKloning()
        {
            if (!HighLogic.LoadedSceneIsFlight || !KloningKabinEnabled || !SituationAllowed() || !HasRoom()) return;
            bool copy = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().cloneSourceAttributes;
            var source = copy ? Source(true) : null;
            if (copy && source == null)
            {
                Logging.Msg("Kloning needs the selected source aboard this cabin. Select a clone source. No payment taken.");
                return;
            }
            bool accident = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().QuentinTarantinoMode > 0
                && Rules.Accident(accidentRate, Random.NextDouble());
            bool paid = ExecutePaid(() =>
            {
                if (!HasRoom()) return false;
                if (accident) { HandleAccident(); return true; }
                return CreateCrew(HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().kloneCivilians, source);
            });
            if (!paid) return;
            if (!accident)
            {
                int births = 1;
                if (accidentRate > 0 && HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().KribbleMode && Random.NextDouble() < Math.Min(1, accidentRate) * 0.4)
                {
                    // Each additional birth is a real operation and pays once; a full bay never consumes resources.
                    while (births < 9 && HasRoom() && ExecutePaid(() => CreateCrew(HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().kloneCivilians, source))) births++;
                }
                Logging.Msg("Kloning complete: " + births + " new Kerbal(s).");
                PlaySound("A", true);
            }
            NotifyCrewChanged();
        }

        private bool HasRoom()
        {
            bool result = Rules.HasRoom(part.protoModuleCrew.Count, part.CrewCapacity,
                HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().requireLivingKerbal);
            if (!result) Logging.Msg("Kloning needs a free seat and, if enabled, a living source Kerbal.");
            return result;
        }

        private bool SituationAllowed()
        {
            return Rules.SituationAllowed(vessel.situation.ToString(),
                HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().situationallyAware,
                allowSplashedOrLanded, allowOrbital);
        }

        private void HandleAccident()
        {
            var crew = part.protoModuleCrew.ToArray();
            int limit = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().QuentinTarantinoMode;
            if (limit <= 0) return;
            int affected = 0;
            foreach (var kerbal in crew)
            {
                if (affected >= limit) break;
                // Retain potentially fatal accidents without exceeding the configured victim cap.
                if (Random.NextDouble() >= (crew.Length == 1 ? 0.2 : Math.Min(1, Math.Max(0, accidentRate)))) continue;
                part.RemoveCrewmember(kerbal);
                kerbal.Die();
                affected++;
            }
            Logging.Msg("Kloning failed. Kerbals lost: " + affected + ".");
            PlaySound("A", false);
        }

        public override string GetInfo() => RecipeInfo("Creates crew in a free seat. Enable source-attribute cloning in MoarKerbals difficulty settings to copy a selected occupant with a new name, normal stock EVA equipment, empty KIS inventory and fresh mission history. Accident probability when enabled: " + accidentRate.ToString("P0") + ". Maximum affected = 0 disables accidents entirely. Each attempt/birth is charged once.");
    }
}
