// MoarKerbals logging. Original project attribution and licences apply.
using System;
using UnityEngine;

namespace MoarKerbals
{
    internal static class Logging
    {
        private static bool Verbose => HighLogic.CurrentGame != null && HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsDisplaySettings>().logging;
        internal static void DLog(string logMsg, bool xDebug = false)
        { if (xDebug || Verbose) Debug.Log("[MoarKerbals " + Version.Number + "] " + logMsg); }
        internal static void DLogWarning(string logMsg, bool xDebug = true)
        { if (xDebug || Verbose) Debug.LogWarning("[MoarKerbals] " + logMsg); }
        internal static void DLogError(string logMsg, bool xDebug = true)
        { Debug.LogError("[MoarKerbals] " + logMsg); }
        internal static void Msg(string s, params object[] args)
        {
            if (args.Length > 0) s = string.Format(s, args);
            Debug.Log("[MoarKerbals] " + s);
            if (HighLogic.CurrentGame != null && !HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsDisplaySettings>().quietMode)
                ScreenMessages.PostScreenMessage(s, 4.5f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
