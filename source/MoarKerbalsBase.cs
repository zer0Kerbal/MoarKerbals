using System;
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;
using MoarKerbals.Core;

namespace MoarKerbals
{
    public class MoarKerbalsBase : PartModule
    {
        [KSPField] public string recipeIngredients = "";
        [KSPField] public string recipeAmounts = "";
        [KSPField] public string initiateAction = "";
        [KSPField] public float costFunds;
        [KSPField] public float costScience;
        [KSPField] public float costReputation;
        internal readonly List<ResourceRequired> resourceRequired = new List<ResourceRequired>();
        internal static string groupName => Localizer.Format("#MOAR-003", Version.SText);
        private Ingredient[] recipe = new Ingredient[0];
        private bool recipeValid;
        private string recipeError = "Recipe not initialized";
        protected static readonly System.Random Random = new System.Random();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            InitializeRecipe();
            if (HighLogic.CurrentGame == null) return;
            string title = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsDisplaySettings>().coloredPAW ? "<color=#BADA55>" + groupName + "</color>" : groupName;
            foreach (BaseField field in Fields) if (field.group != null && field.group.name == "MoarKerbals") field.group.displayName = title;
        }

        protected void InitializeRecipe()
        {
            resourceRequired.Clear();
            recipeValid = false;
            try
            {
                recipe = Rules.ParseRecipe(recipeIngredients, recipeAmounts);
                foreach (var item in recipe)
                {
                    var resource = new ResourceRequired(item.Name, item.Amount);
                    if (resource.Resource == null) throw new FormatException("Unknown resource: " + item.Name);
                    resourceRequired.Add(resource);
                }
                if (!Rules.FiniteNonNegative(costFunds) || !Rules.FiniteNonNegative(costScience) || !Rules.FiniteNonNegative(costReputation))
                    throw new FormatException("Costs must be finite and non-negative.");
                recipeValid = true;
                recipeError = "";
            }
            catch (Exception error) { recipeError = error.Message; Logging.DLogError("Invalid recipe: " + error.Message); }
        }

        // Read-only preflight for the PAW. ExecutePaid still validates and charges atomically on click.
        protected string CompletionBlocker()
        {
            if (!recipeValid) return "invalid configuration: " + recipeError;
            if (HighLogic.CurrentGame == null || part == null) return "game unavailable";
            double multiplier = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGeneralSettings>().globalKloningCostMultiplier;
            if (!Rules.FiniteNonNegative(multiplier)) return "invalid cost multiplier";
            var mode = HighLogic.CurrentGame.Mode;
            if (mode == Game.Modes.CAREER)
            {
                if (Funding.Instance == null || Reputation.Instance == null || ResearchAndDevelopment.Instance == null)
                    return "career balances unavailable";
                if (!Rules.CanAfford(Funding.Instance.Funds, costFunds)) return "insufficient funds (need " + costFunds + ")";
                if (!Rules.CanAfford(Reputation.Instance.reputation, costReputation)) return "insufficient reputation (need " + costReputation + ")";
            }
            if (mode == Game.Modes.CAREER || mode == Game.Modes.SCIENCE_SANDBOX)
                if (ResearchAndDevelopment.Instance == null || !Rules.CanAfford(ResearchAndDevelopment.Instance.Science, costScience))
                    return "insufficient science (need " + costScience + ")";
            var resources = new RecipeResources(part);
            foreach (var item in recipe)
            {
                double needed = item.Amount * multiplier;
                if (!Rules.FiniteNonNegative(needed)) return "invalid resource cost";
                if (needed == 0) continue;
                double available, capacity;
                resources.Totals(item.Name, out available, out capacity);
                if (available + 1e-7 < needed)
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "need {0:0.##} {1} (available {2:0.##})", needed, item.Name, available);
            }
            return "";
        }

        protected bool ExecutePaid(Func<bool> operation)
        {
            if (!HighLogic.LoadedSceneIsFlight || vessel == null) return false;
            try
            {
                string blocker = CompletionBlocker();
                if (blocker.Length != 0)
                {
                    Logging.Msg("MoarKerbals: " + blocker + ". No payment taken.");
                    return false;
                }
                var resources = new RecipeResources(part);
                bool result = Rules.Transact(recipe, HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGeneralSettings>().globalKloningCostMultiplier,
                    CanAfford, resources.Request,
                    operation, DebitCurrencies);
                if (!result) Logging.Msg("MoarKerbals: operation unavailable, or insufficient resources/currency. No payment taken.");
                return result;
            }
            catch (Exception error) { Logging.DLogError("Operation failed: " + error); return false; }
        }

        private bool CanAfford()
        {
            var mode = HighLogic.CurrentGame.Mode;
            if (mode == Game.Modes.CAREER)
                return Funding.Instance != null && Reputation.Instance != null && ResearchAndDevelopment.Instance != null &&
                    Rules.CanAfford(Funding.Instance.Funds, costFunds) && Rules.CanAfford(Reputation.Instance.reputation, costReputation) &&
                    Rules.CanAfford(ResearchAndDevelopment.Instance.Science, costScience);
            return mode != Game.Modes.SCIENCE_SANDBOX || (ResearchAndDevelopment.Instance != null && Rules.CanAfford(ResearchAndDevelopment.Instance.Science, costScience));
        }

        private void DebitCurrencies()
        {
            var mode = HighLogic.CurrentGame.Mode;
            if (mode == Game.Modes.CAREER)
            {
                if (costFunds != 0) Funding.Instance.AddFunds(-costFunds, TransactionReasons.CrewRecruited);
                if (costReputation != 0) Reputation.Instance.AddReputation(-costReputation, TransactionReasons.CrewRecruited);
            }
            if ((mode == Game.Modes.CAREER || mode == Game.Modes.SCIENCE_SANDBOX) && costScience != 0)
                ResearchAndDevelopment.Instance.AddScience(-costScience, TransactionReasons.CrewRecruited);
        }

        protected static bool TraitExists(string name)
        {
            Experience.ExperienceTraitConfig config;
            return KerbalRoster.TryGetExperienceTraitConfig(name, out config);
        }

        protected bool CreateCrew(bool civilian, ProtoCrewMember source = null)
        {
            if (!Rules.HasRoom(part.protoModuleCrew.Count, part.CrewCapacity, false) ||
                (source == null && civilian && !TraitExists("Civilian")) ||
                (source != null && (!part.protoModuleCrew.Contains(source) || !TraitExists(source.trait)))) return false;
            var roster = HighLogic.CurrentGame.CrewRoster;
            ProtoCrewMember kerbal = null;
            try
            {
                if (source != null)
                {
                    // Generate a new identity; never rename or shallow-copy a roster member.
                    for (int attempt = 0; attempt < 100 && kerbal == null; attempt++)
                    {
                        string name = CrewGenerator.GetRandomName(source.gender, Random);
                        var candidate = KerbalCloning.Create(source, name, HighLogic.CurrentGame.Mode);
                        if (roster.AddCrewMember(candidate)) kerbal = candidate;
                    }
                    if (kerbal == null) throw new InvalidOperationException("Could not allocate a unique clone name.");
                }
                else
                {
                    kerbal = roster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    var suit = (ProtoCrewMember.KerbalSuit)HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().birthdaySuit;
                    if (Enum.IsDefined(typeof(ProtoCrewMember.KerbalSuit), suit)) kerbal.suit = suit;
                    if (civilian) KerbalRoster.SetExperienceTrait(kerbal, "Civilian");
                }
                if (!part.AddCrewmember(kerbal)) throw new InvalidOperationException("No seat available.");
                kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                vessel.SpawnCrew();
                return true;
            }
            catch (Exception error)
            {
                if (kerbal != null)
                {
                    if (part.protoModuleCrew.Contains(kerbal)) part.RemoveCrewmember(kerbal);
                    roster.Remove(kerbal);
                }
                Logging.DLogError("Crew creation rolled back: " + error.Message);
                return false;
            }
        }

        protected void NotifyCrewChanged()
        {
            CrewInventoryRefresh.Refresh(part);
            GameEvents.onVesselChange.Fire(vessel);
            GameEvents.onVesselWasModified.Fire(vessel);
        }

        protected void PlaySound(string category, bool success)
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().soundOn) return;
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGeneralSettings>();
            string[] clips;
            int choice;
            switch (category)
            {
                case "A":
                    clips = success ? new[] { "kloning", "save" } : new[] { "overload", "negative", "wilhelm" };
                    choice = success ? settings.soundClipA0 : settings.soundClipA1;
                    break;
                case "B":
                    clips = success ? new[] { "kuddleshack", "positive" } : new[] { "misc_sound", "negative" };
                    choice = success ? settings.soundClipB0 : settings.soundClipB1;
                    break;
                default:
                    clips = success ? new[] { "positive", "Rise05" } : new[] { "negative", "misc_sound" };
                    choice = success ? settings.soundClipC0 : settings.soundClipC1;
                    break;
            }
            if (choice == 0) choice = Random.Next(1, clips.Length + 1);
            string clip = clips[Math.Max(0, Math.Min(clips.Length - 1, choice - 1))];
            var sound = GameDatabase.Instance.GetAudioClip("KerbthulhuKineticsProgram/MoarKerbals/Sounds/" + clip);
            if (sound != null) AudioSource.PlayClipAtPoint(sound, part.transform.position,
                settings.soundVolume);
        }

        protected string RecipeInfo(string description) => "MoarKerbals " + Version.Number + "\n" + description +
            "\nResources: " + recipeIngredients + " (" + recipeAmounts + ")\nCost per operation: " + costFunds + " funds, " + costScience + " science, " + costReputation + " reputation.";
    }
}
