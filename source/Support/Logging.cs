// Logging.cs v1.0.0.0
#region CC BY-NC-SA 3.0
/* KGEx's library for the Kerbal Space Program, by zer0Kerbal
 * 
 * (C) Copyright 2019, 2021 zer0Kerbal
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that KGEx is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.*/
#endregion
#region Author: zer0Kerbal (zer0Kerbal@hotmail.com)
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;


namespace MoarKerbals
{

    //    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    //    public class Init : MonoBehaviour
    //    {
    //        internal static Log Log;
    //        void Start()
    //        {
    //            if (HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().logging)
    //            {
    //                Log = new Log("MoarKerbals", Log.LEVEL.INFO);
    //#if DEBUG
    //                Log = new Log("MoarKerbals", Log.LEVEL.ERROR);
    //#endif
    //            }
    //        }
    //    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    class Logger : MonoBehaviour
    {
        public List<string> logs = new List<string>();
        public static Logger instance;
        string directory;

        public void Awake()
        {
            logs.Add("Using MoarKerbals " + Version.Number);
            instance = this;
            directory = KSPUtil.ApplicationRootPath + "/GameData/KerbthulhuKineticsProgram/MoarKerbals/Logs/";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            DirectoryInfo source = new DirectoryInfo(directory);
            foreach (FileInfo fi in source.GetFiles())
            {
                var creationTime = fi.CreationTime;
                if (creationTime < (DateTime.Now - new TimeSpan(1, 0, 0, 0)))
                {
                    fi.Delete();
                }
            }
        }

        internal void addToLog(string logMsg)
        {
            logs.Add(logMsg);
        }

        public void OnDisable()
        {
            if (logs.Count() == 1) return;
            string path = directory + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
            using (StreamWriter writer = File.AppendText(path))
            {
                foreach (string s in logs)
                {
                    writer.WriteLine(s);
                }
            }
        }
    }

    class Logging
    {
        /// <summary>Add messages to the log. Prepends "MoarKerbals v{0}]: ", Version.Text
        /// if #DEBUG defined, or if Debug.Settings.logging to create log entry in KSP.log
        /// will always create a log entry in MoarKerbals.log</summary>
        /// <param name="logMsg">The message.</param>
        /// <param name="xDebug">default = false; log if true</param>
        internal static void DLog(string logMsg, bool xDebug = false)
        {
            logMsg = string.Format("[MoarKerbals v{0}]: ", Version.Text) + logMsg;
            Logger.instance.addToLog(logMsg);
            if (xDebug || HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().logging)
            {
                //Debug.LogWarning(logMsg);
                Debug.Log(logMsg);
            }
#if DEBUG
                        else { Debug.Log(logMsg); }
#endif
        }

        /// <summary>LogWarning: Add messages to the log. Prepends "MoarKerbals v{0}]: ", Version.Text</summary>
        /// <param name="logMsg">The message.</param>
        /// <param name="xDebug">require DEBUG setting to create log entry</param>
        internal static void DLogWarning(string logMsg, bool xDebug = true)
        {
            logMsg = string.Format("[MoarKerbals v{0}][WRN]: ", Version.Text) + logMsg;
            Logger.instance.addToLog(logMsg);
            if (xDebug || HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().logging)
            {
                Debug.LogWarning(logMsg);
            }
#if DEBUG
                else { Debug.LogWarning(logMsg); }
#endif
        }

        /// <summary>LogError: Add messages to the log. Prepends "MoarKerbals v{0}]: ", Version.Text</summary>
        /// <param name="logMsg">The message.</param>
        /// <param name="xDebug">require DEBUG setting to create log entry</param>
        internal static void DLogError(string logMsg, bool xDebug = true)
        {
            logMsg = string.Format("[MoarKerbals v{0}][ERR]: ", Version.Text) + logMsg;
            Logger.instance.addToLog(logMsg);
            if (xDebug || HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().logging)
            {
                Debug.LogError(logMsg);
            }
#if DEBUG
                else { Debug.LogError(logMsg); }
#endif
        }

        /// <summary>sends the specific message to in game mail and screen if Debug is defined
        /// For debugging use only.</summary>
        /// <param name="s">The Message.</param>
        /// <param name="params">The parameters.</param>
        internal static void Msg(string s, params object[] @params)
        {
            if (0 != @params.Length)
            {
                s = string.Format(s, @params);
            }
            Logger.instance.addToLog(s);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<Settings3>().quietMode) ScreenMessages.PostScreenMessage(s, (float)4.5, ScreenMessageStyle.UPPER_CENTER, true);
            DLog(s);
        }
    }
}


/// <summary>Messages to the screen in the specified format.</summary>
/// <param name="s">The format.</param>
/// <param name="args">The arguments.</param>
//private void Message(string s, params object[] args)
//{
//    ScreenMessages.PostScreenMessage(string.Format(s, args), 3f, 0);
//}
// }