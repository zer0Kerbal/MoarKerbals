// Derived from MoarKerbals by strideknight and zer0Kerbal; CC-BY-NC-SA-4.0.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using MoarKerbals.Core;

namespace MoarKerbals
{
    [KSPModule("KuddleShack")]
    public class KuddleShack : MoarKerbalsBase
    {
        [KSPField(guiName = "#MOAR-Kuddle-00", groupName = "MoarKerbals", guiActive = true, guiActiveEditor = true, isPersistant = true),
            UI_Toggle(disabledText = "Off", enabledText = "On")]
        public bool KuddleShackEnabled;
        [KSPField(guiName = "Next offspring", groupName = "MoarKerbals", guiActive = true)]
        public string timeRemaining = "--";
        [KSPField(guiName = "Status", groupName = "MoarKerbals", guiActive = true)]
        public string activityStatus = "Starting";
        [KSPField(guiName = "Pairs", groupName = "MoarKerbals", guiActive = true)]
        public string currentPair = "None";
        [KSPField(guiName = "Ready pairs", groupName = "MoarKerbals", guiActive = true)]
        public string readyPairs = "0 / 0";
        // Legacy fields mirror the first pair for migration and limited older-version recovery.
        [KSPField(isPersistant = true)] public double progressSeconds;
        [KSPField(isPersistant = true)] public string pairSignature = "";
        [KSPField(isPersistant = true)] public double timeCheckpointUT = -1;
        [KSPField(isPersistant = true)] public bool wasProgressing;
        [KSPField(isPersistant = true)] public int pairStateVersion;
        internal readonly List<KuddlePair> pairs = new List<KuddlePair>();
        private bool pairsInitialized;
        private bool hasPair;
        private List<PartModule> lights = new List<PartModule>();
        private bool? lightsOn;
        private bool started;
        private float nextDisplayTime;
        private bool lastCompletionFailed;
        private float nextHeadingTime;
        private double RequiredTime => Math.Max(0, SettingsInterface.kuddleTimeNeeded());

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            pairs.Clear();
            var saved = node.GetNodes("KUDDLE_PAIR");
            foreach (var record in saved)
            {
                var pair = KuddlePair.Read(record);
                if (pair != null) pairs.Add(pair);
                else Logging.DLogWarning("Ignoring an invalid saved KuddleShack pair.");
            }
            int version;
            pairStateVersion = int.TryParse(node.GetValue("pairStateVersion"), out version) ? version : 0;
            pairsInitialized = pairStateVersion >= 1 || saved.Length > 0;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (!HighLogic.LoadedSceneIsFlight) return;
            lights = SwitchLight.GetLightModule(part);
            started = true;
            SynchronizeProgress(false);
            UpdateDisplay();
        }

        public override void OnSave(ConfigNode node)
        {
            if (started && HighLogic.LoadedSceneIsFlight && vessel != null) SynchronizeProgress(FlightDriver.Pause);
            base.OnSave(node);
            node.RemoveNodes("KUDDLE_PAIR");
            foreach (var pair in pairs) pair.Write(node.AddNode("KUDDLE_PAIR"));
            node.SetValue("pairStateVersion", pairsInitialized ? 1 : pairStateVersion, true);
            node.SetValue("progressSeconds", progressSeconds.ToString("R", CultureInfo.InvariantCulture), true);
            node.SetValue("timeCheckpointUT", timeCheckpointUT.ToString("R", CultureInfo.InvariantCulture), true);
            node.SetValue("wasProgressing", wasProgressing, true);
            node.SetValue("pairSignature", pairSignature, true);
        }

        // Unity Update also runs on IDLE cabins; stock OnFixedUpdate only runs on ACTIVE parts.
        public void Update()
        {
            if (!started || !HighLogic.LoadedSceneIsFlight || vessel == null) return;
            bool changed = SynchronizeProgress(FlightDriver.Pause);
            if (changed || Time.unscaledTime >= nextDisplayTime)
            {
                UpdateDisplay();
                nextDisplayTime = Time.unscaledTime + 0.5f;
            }
        }

        public void LateUpdate()
        {
            if (!started || !HighLogic.LoadedSceneIsFlight || Time.unscaledTime < nextHeadingTime) return;
            nextHeadingTime = Time.unscaledTime + 0.5f;
            try { KuddleCrewHeadings.Refresh(part); }
            catch (Exception error) { Logging.DLogWarning("KuddleShack crew heading refresh: " + error.Message); }
        }

        private bool SynchronizeProgress(bool paused)
        {
            double now = Planetarium.GetUniversalTime();
            var crew = part.protoModuleCrew.Where(k => k.type == ProtoCrewMember.KerbalType.Crew).ToArray();
            var males = crew.Where(k => k.gender == ProtoCrewMember.Gender.Male).Select(k => k.name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            var females = crew.Where(k => k.gender == ProtoCrewMember.Gender.Female).Select(k => k.name).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            if (!pairsInitialized)
            {
                // Carry the old pair's earned progress/checkpoint into exactly that pair, once.
                foreach (var male in males)
                    foreach (var female in females)
                        if (KuddlePair.Signature(male, female) == pairSignature)
                            pairs.Add(new KuddlePair { Male = male, Female = female, Progress = progressSeconds,
                                Checkpoint = timeCheckpointUT, Running = wasProgressing });
                pairsInitialized = true;
            }
            int limit = Math.Max(0, part.CrewCapacity / 2);
            var retained = new List<KuddlePair>();
            var used = new HashSet<string>(StringComparer.Ordinal);
            // Preserve partners and independent clocks; do not reshuffle on crew ordering or arrivals.
            foreach (var pair in pairs)
                if (retained.Count < limit && males.Contains(pair.Male) && females.Contains(pair.Female) &&
                    !used.Contains(pair.Male) && !used.Contains(pair.Female))
                {
                    used.Add(pair.Male); used.Add(pair.Female); retained.Add(pair);
                }
            var freeMales = males.Where(n => !used.Contains(n)).ToArray();
            var freeFemales = females.Where(n => !used.Contains(n)).ToArray();
            for (int i = 0; i < Math.Min(freeMales.Length, freeFemales.Length) && retained.Count < limit; i++)
                retained.Add(new KuddlePair { Male = freeMales[i], Female = freeFemales[i], Checkpoint = now });
            bool changed = pairs.Count != retained.Count || !pairs.SequenceEqual(retained);
            pairs.Clear(); pairs.AddRange(retained);
            hasPair = pairs.Count > 0;
            foreach (var pair in pairs)
            {
                bool progressing = KuddleShackEnabled && isEnabled;
                double elapsed = pair.Checkpoint >= 0 ? now - pair.Checkpoint : 0;
                pair.Progress = Rules.Advance(pair.Progress, elapsed, RequiredTime, pair.Running && progressing && !paused, true);
                pair.Checkpoint = now;
                pair.Running = progressing;
            }
            if (changed) lastCompletionFailed = false;
            currentPair = pairs.Count == 1 ? GenderLabels.Pair(pairs[0].Male, pairs[0].Female) : hasPair ? pairs.Count + " pairs" :
                males.Length == 0 && females.Length == 0 ? "No eligible crew" : males.Length == 0 ?
                "Needs a male crew member" : females.Length == 0 ? "Needs a female crew member" : "Needs at least two seats";
            pairStateVersion = 1;
            MirrorLegacy(now);
            return changed;
        }

        private void MirrorLegacy(double now)
        {
            var pair = pairs.FirstOrDefault();
            progressSeconds = pair != null ? pair.Progress : 0;
            pairSignature = pair != null ? KuddlePair.Signature(pair.Male, pair.Female) : "";
            timeCheckpointUT = pair != null ? pair.Checkpoint : now;
            wasProgressing = pair != null && pair.Running;
        }

        private static string Duration(double remaining)
        {
            double seconds = Math.Ceiling(Math.Max(0, remaining));
            return string.Format("{0}h {1}m {2}s", (int)(seconds / 3600), (int)(seconds / 60) % 60, (int)seconds % 60);
        }

        private void UpdateDisplay()
        {
            int ready = pairs.Count(p => p.Progress >= RequiredTime);
            readyPairs = ready + " / " + pairs.Count;
            bool room = Rules.HasRoom(part.protoModuleCrew.Count, part.CrewCapacity, false);
            string blocker = ready > 0 ? CompletionBlocker() : "";
            string action = ready > 1 ? "Welcome ready Kerbals" : "Welcome new Kerbal";
            activityStatus = !isEnabled ? "Paused: module disabled" : !KuddleShackEnabled ? "Off: progress paused" :
                !hasPair ? "Waiting: " + currentPair : FlightDriver.Pause ? "Paused: game paused" : ready == 0 ?
                (room ? "In progress" : "In progress; needs a free seat to finish") : !room ? "Blocked: cabin full" :
                blocker.Length != 0 ? "Blocked: " + blocker : lastCompletionFailed ?
                "Ready: previous attempt failed; retry or check KSP.log" : "Ready: select " + action;
            var completion = Events["ActivateKuddling"];
            completion.guiName = action;
            completion.guiActive = completion.guiActiveUnfocused = isEnabled && KuddleShackEnabled && ready > 0 &&
                !FlightDriver.Pause && vessel.loaded && room && blocker.Length == 0;
            timeRemaining = hasPair ? Duration(RequiredTime - pairs.Max(p => p.Progress)) : "--";
            bool illuminate = isEnabled && KuddleShackEnabled && hasPair;
            if (lightsOn != illuminate)
            {
                foreach (var light in lights)
                {
                    try { if (illuminate) SwitchLight.On(light); else SwitchLight.Off(light); }
                    catch (Exception error) { Logging.DLogWarning("Optional cabin light: " + error.Message); }
                }
                lightsOn = illuminate;
            }
        }

        [KSPEvent(guiName = "Pair details", groupName = "MoarKerbals", guiActive = true)]
        public void ShowPairDetails()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            GenderSymbolFont.Ensure();
            SynchronizeProgress(FlightDriver.Pause);
            string details = hasPair ? string.Join("\n", pairs.Select(p => GenderLabels.Pair(p.Male, p.Female) + ": " +
                (p.Progress >= RequiredTime ? "Ready" : Duration(RequiredTime - p.Progress))).ToArray()) : currentPair;
            ScreenMessages.PostScreenMessage(details, 12f, ScreenMessageStyle.UPPER_CENTER);
        }

        [KSPEvent(guiName = "Welcome new Kerbal", groupName = "MoarKerbals", guiActive = false, guiActiveUnfocused = false)]
        public void ActivateKuddling()
        {
            if (!HighLogic.LoadedSceneIsFlight || vessel == null) return;
            SynchronizeProgress(FlightDriver.Pause);
            UpdateDisplay();
            if (!KuddleShackEnabled || !isEnabled || FlightDriver.Pause || !vessel.loaded) return;
            // Snapshot the ready cohort. Newborns/new pairs never enter this completion batch.
            var ready = pairs.Where(p => p.Progress >= RequiredTime).ToArray();
            int births = 0;
            foreach (var pair in ready)
            {
                SynchronizeProgress(FlightDriver.Pause);
                if (!KuddleShackEnabled || !isEnabled || FlightDriver.Pause || !vessel.loaded) break;
                if (!pairs.Contains(pair) || pair.Progress < RequiredTime) continue;
                if (!Rules.HasRoom(part.protoModuleCrew.Count, part.CrewCapacity, false)) break;
                string blocker = CompletionBlocker();
                if (blocker.Length != 0) { Logging.Msg("KuddleShack: " + blocker); break; }
                if (!ExecutePaid(() => CreateCrew(HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsGameplaySettings>().kuddleCivilians)))
                {
                    lastCompletionFailed = true;
                    break;
                }
                births++;
                pair.Progress = 0;
                pair.Checkpoint = Planetarium.GetUniversalTime();
                lastCompletionFailed = false;
            }
            SynchronizeProgress(FlightDriver.Pause);
            UpdateDisplay();
            if (births == 0 && lastCompletionFailed) activityStatus = "Completion failed: see on-screen message or KSP.log";
            if (births > 0)
            {
                Logging.Msg(births == 1 ? "A new Kerbal has joined the crew." : births + " new Kerbals have joined the crew.");
                PlaySound("B", true);
                NotifyCrewChanged();
            }
        }

        public override string GetInfo() => RecipeInfo("Independent simultaneous male/female pairs, up to half the seat capacity. Stable partners and separate timers persist while elsewhere. Each offspring needs a free seat and one recipe payment. Consumables use unlocked tanks on this vessel.");
    }

    internal sealed class KuddlePair
    {
        public string Male, Female;
        public double Progress;
        public double Checkpoint = -1;
        public bool Running;
        public static string Signature(string male, string female) => male.Length + ":" + male + female;
        public void Write(ConfigNode node)
        {
            node.SetValue("male", Male, true); node.SetValue("female", Female, true);
            node.SetValue("progress", Progress.ToString("R", CultureInfo.InvariantCulture), true);
            node.SetValue("checkpoint", Checkpoint.ToString("R", CultureInfo.InvariantCulture), true);
            node.SetValue("running", Running, true);
        }
        public static KuddlePair Read(ConfigNode node)
        {
            string male = node.GetValue("male"), female = node.GetValue("female");
            double progress, checkpoint;
            bool running;
            if (string.IsNullOrEmpty(male) || string.IsNullOrEmpty(female) || male == female ||
                !double.TryParse(node.GetValue("progress"), NumberStyles.Float, CultureInfo.InvariantCulture, out progress) ||
                !Rules.FiniteNonNegative(progress) ||
                !double.TryParse(node.GetValue("checkpoint"), NumberStyles.Float, CultureInfo.InvariantCulture, out checkpoint) ||
                double.IsNaN(checkpoint) || double.IsInfinity(checkpoint) || checkpoint < -1 ||
                !bool.TryParse(node.GetValue("running"), out running)) return null;
            return new KuddlePair { Male = male, Female = female, Progress = progress, Checkpoint = checkpoint, Running = running };
        }
    }
}
