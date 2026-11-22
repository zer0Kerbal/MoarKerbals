using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoarKerbals
{
    // KSP dispatches saved custom parameters by Type.Name, without the namespace.
    // Recover uniquely identified legacy MoarKerbals sections after stock loading.
    internal static class SettingsPersistence
    {
        internal static int Migrate(Game game)
        {
            if (game == null || game.Parameters == null || game.config == null) return 0;
            var root = game.config.GetNode("GAME") ?? game.config;
            var parameters = root.GetNode("PARAMETERS");
            if (parameters == null) return 0;
            int count = 0;
            if (Migrate<MoarKerbalsGeneralSettings>(game.Parameters, parameters, "Settings",
                "globalKloningCostMultiplier", "kuddleTimeNeeded")) count++;
            if (Migrate<MoarKerbalsGameplaySettings>(game.Parameters, parameters, "Settings2",
                "kloneCivilians", "kuddleCivilians")) count++;
            if (Migrate<MoarKerbalsDisplaySettings>(game.Parameters, parameters, "Settings3",
                "coloredPAW", "quietMode")) count++;
            return count;
        }

        private static bool Migrate<T>(GameParameters target, ConfigNode parameters, string oldName,
            params string[] markers) where T : GameParameters.CustomParameterNode, new()
        {
            string newName = typeof(T).Name;
            // New saves are authoritative, even when legacy nodes also exist.
            if (parameters.HasNode(newName)) return false;
            var candidates = parameters.GetNodes(oldName).Where(n => markers.All(n.HasValue)).ToArray();
            if (candidates.Length == 0) return false;
            if (candidates.Length != 1)
            {
                Logging.DLogWarning("Ambiguous legacy settings for " + newName + "; migration skipped.");
                return false;
            }
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var migrated = new ConfigNode(newName);
            foreach (var field in fields)
                if (candidates[0].HasValue(field.Name)) migrated.AddValue(field.Name, candidates[0].GetValue(field.Name));

            // Parse into a temporary page first so a failed load cannot partly overwrite live settings.
            var parsed = new T();
            parsed.Load(migrated);
            var actual = target.CustomParams<T>();
            foreach (var field in fields) field.SetValue(actual, field.GetValue(parsed));
            // In-memory only. This also prevents a repeated callback from overwriting UI changes.
            parameters.AddNode(migrated);
            return true;
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class SettingsMigrationLoader : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GameEvents.onGameStateCreated.Add(OnGameCreated);
        }

        private void OnDestroy() => GameEvents.onGameStateCreated.Remove(OnGameCreated);

        private void OnGameCreated(Game game)
        {
            // The event supplies the newly constructed game, including during quickload;
            // HighLogic.CurrentGame may still point to the previous game here.
            try
            {
                int count = SettingsPersistence.Migrate(game);
                if (count != 0) Logging.DLog("Migrated " + count + " legacy difficulty settings pages.", true);
            }
            catch (Exception error) { Logging.DLogError("Settings migration failed: " + error); }
        }
    }
}
