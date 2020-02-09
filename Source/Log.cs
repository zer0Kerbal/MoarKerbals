using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoarKerbals
{
    class Log
    {
        /// <summary>
        /// sends the specific message to ingame mail and screen if Debug is defined
        /// For debugging use only.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="params">The parameters.</param>
        [ConditionalAttribute("DEBUG")]
        internal static void dbg(string msg, params object[] @params)
        {
            if (0 != @params.Length) msg = string.Format(msg, @params);
            ScreenMessages.PostScreenMessage(msg, 1, ScreenMessageStyle.UPPER_CENTER, true);
            UnityEngine.Debug.Log("[MoarKerbals] " + msg);
        }
    }
    /*    internal static class Logg
        {
            public enum LEVEL
            {
                OFF = 0,
                ERROR = 1,
                WARNING = 2,
                INFO = 3,
                DETAIL = 4,
                TRACE = 5
            };

            public static LEVEL level = LEVEL.INFO;

            private static readonly String PREFIX = "Biomatic" + ": ";

            public static LEVEL GetLevel()
            {
                return level;
            }

            public static void SetLevel(LEVEL level)
            {
                UnityEngine.Debug.Log("log level " + level);
                Log.level = level;
            }

            public static LEVEL GetLogLevel()
            {
                return level;
            }

            private static bool IsLevel(LEVEL level)
            {
                return level == Log.level;
            }

            public static bool IsLogable(LEVEL level)
            {
                return level <= Log.level;
            }

            public static void Trace(String msg)
            {
                if (IsLogable(LEVEL.TRACE))
                {
                    UnityEngine.Debug.Log(PREFIX + msg);
                }
            }

            public static void Detail(String msg)
            {
                if (IsLogable(LEVEL.DETAIL))
                {
                    UnityEngine.Debug.Log(PREFIX + msg);
                }
            }

            [ConditionalAttribute("DEBUG")]
            public static void Info(String msg)
            {
                if (IsLogable(LEVEL.INFO))
                {
                    UnityEngine.Debug.Log(PREFIX + msg);
                }
            }

            [ConditionalAttribute("DEBUG")]
            public static void Test(String msg)
            {
                //if (IsLogable(LEVEL.INFO))
                {
                    UnityEngine.Debug.LogWarning(PREFIX + "TEST:" + msg);
                }
            }

            public static void Warning(String msg)
            {
                if (IsLogable(LEVEL.WARNING))
                {
                    UnityEngine.Debug.LogWarning(PREFIX + msg);
                }
            }

            public static void Error(String msg)
            {
                if (IsLogable(LEVEL.ERROR))
                {
                    UnityEngine.Debug.LogError(PREFIX + msg);
                }
            }

            public static void Debug(bool debugMode, String msg)
            {
                if (debugMode)
                {
                    UnityEngine.Debug.LogError(PREFIX + msg);
                }
                else
                    Log.Info(msg);
            }

            public static void Exception(Exception e)
            {
                Log.Error("exception caught: " + e.GetType() + ": " + e.Message);
            }
        }*/
}
