using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MoarKerbals
{
    internal static class KerbalCloning
    {
        internal const string TrainingPrefix = "MoarCloneTraining_";
        private static readonly HashSet<string> registered = new HashSet<string>();
        // Only personal attributes: no inventory, seat, runtime state, mission history or third-party records.
        private static readonly string[] attributes = {
            "gender", "trait", "brave", "dumb", "badS", "veteran", "suit", "hero",
            "hasHelmetOn", "hasNeckRingOn", "hasVisorDown", "lightR", "lightG", "lightB",
            "suitTexturePath", "normalTexturePath", "spritePath", "comboId"
        };

        internal static ConfigNode Template(ProtoCrewMember source, string name, string homeBody)
        {
            if (float.IsNaN(source.experience) || float.IsInfinity(source.experience) || source.experience < 0)
                throw new InvalidOperationException("Source Kerbal has invalid experience.");
            var saved = new ConfigNode("KERBAL");
            source.Save(saved);
            var node = new ConfigNode("KERBAL");
            foreach (string attribute in attributes)
                if (saved.HasValue(attribute)) node.AddValue(attribute, saved.GetValue(attribute));
            node.AddValue("name", name);
            node.AddValue("type", source.type.ToString());
            node.AddValue("state", "Available");
            node.AddValue("idx", -1);
            node.AddValue("inactive", false);
            node.AddNode("INVENTORY"); // Explicitly empty, rather than the stock EVA default loadout.
            node.AddNode("FLIGHT_LOG");
            var career = node.AddNode("CAREER_LOG");
            if (source.experience > 0)
            {
                string training = TrainingPrefix + BitConverter.ToUInt32(BitConverter.GetBytes(source.experience), 0).ToString("X8");
                Register(training);
                var log = new FlightLog();
                // A new inherited-skills credit, not copies of the source's missions/achievements.
                log.AddEntry(training, homeBody);
                log.Save(career);
            }
            return node;
        }

        internal static ProtoCrewMember Create(ProtoCrewMember source, string name, Game.Modes mode)
        {
            var clone = new ProtoCrewMember(mode, Template(source, name, FlightGlobals.GetHomeBody().name), ProtoCrewMember.KerbalType.Crew);
            // Fresh stock EVA equipment, not a copy of the source's items. KIS seat inventories are untouched.
            clone.SetDefaultInventory();
            clone.experience = source.experience;
            clone.experienceLevel = source.experienceLevel;
            return clone;
        }

        internal static void Register(string type)
        {
            if (string.IsNullOrEmpty(type) || !type.StartsWith(TrainingPrefix, StringComparison.Ordinal) || type.Length != TrainingPrefix.Length + 8) return;
            uint bits;
            if (!uint.TryParse(type.Substring(TrainingPrefix.Length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bits)) return;
            float xp = BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
            if (float.IsNaN(xp) || float.IsInfinity(xp) || xp <= 0 || !registered.Add(type)) return;
            KerbalRoster.AddExperienceType(type, "Inherited skills (<<1>>)", 0, xp);
        }

        internal static void RegisterSavedTraining(ConfigNode root)
        {
            if (root == null) return;
            var game = root.GetNode("GAME") ?? root;
            var roster = game.GetNode("ROSTER");
            if (roster == null) return;
            foreach (var kerbal in roster.GetNodes("KERBAL"))
            {
                RegisterCrewTraining(kerbal);
            }
        }

        internal static void RegisterCrewTraining(ConfigNode kerbal)
        {
            var career = kerbal == null ? null : kerbal.GetNode("CAREER_LOG");
            if (career == null) return;
            var log = new FlightLog();
            log.Load(career);
            foreach (var entry in log.Entries) Register(entry.type);
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class CloneTrainingLoader : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GameEvents.onGameStateLoad.Add(OnLoad);
            // Stock fires this inside the PCM constructor, before experience is calculated.
            GameEvents.onProtoCrewMemberLoad.Add(OnCrewLoad);
        }
        private void OnDestroy()
        {
            GameEvents.onGameStateLoad.Remove(OnLoad);
            GameEvents.onProtoCrewMemberLoad.Remove(OnCrewLoad);
        }
        private void OnCrewLoad(GameEvents.FromToAction<ProtoCrewMember, ConfigNode> data)
        {
            try { KerbalCloning.RegisterCrewTraining(data.to); }
            catch (Exception error) { Logging.DLogError("Clone crew training load failed: " + error); }
        }
        private void OnLoad(ConfigNode node)
        {
            try { KerbalCloning.RegisterSavedTraining(node); }
            catch (Exception error) { Logging.DLogError("Clone training load failed: " + error); }
        }
    }
}
