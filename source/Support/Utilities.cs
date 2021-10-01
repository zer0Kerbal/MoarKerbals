/**
 * Utilities.cs
 * 
 * Thunder Aerospace Corporation's library for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
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
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;

namespace MoarKerbals
{
    class Utilities
    {
        internal static void msg(string msg, params object[] @params)
        {
            if (0 != @params.Length) msg = string.Format(msg, @params);
            ScreenMessages.PostScreenMessage(msg, 10f, ScreenMessageStyle.UPPER_CENTER, true);
            if (SettingsInterface.InGameMail()) UnityEngine.Debug.Log("[MoarKerbals] " + msg);
        }

        #if false

        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        public class KSPAssemblyDependency : Attribute
        {
            public string name;
            public int versionMajor;
            public int versionMinor;

            public extern KSPAssemblyDependency(string name, int versionMajor, int versionMinor);
        }
        public static string GetDllVersion<T>(T t)
        {
            System.Reflection.Assembly assembly = t.GetType().Assembly;
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static bool GetValue(ConfigNode config, string name, bool currentValue)
        {
            bool newValue;
            if (config.HasValue(name) && bool.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static int GetValue(ConfigNode config, string name, int currentValue)
        {
            int newValue;
            if (config.HasValue(name) && int.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static double GetValue(ConfigNode config, string name, double currentValue)
        {
            double newValue;
            if (config.HasValue(name) && double.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static float GetValue(ConfigNode config, string name, float currentValue)
        {
            float newValue;
            if (config.HasValue(name) && float.TryParse(config.GetValue(name), out newValue))
            {
                return newValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static string GetValue(ConfigNode config, string name, string currentValue)
        {
            if (config.HasValue(name))
            {
                return config.GetValue(name);
            }
            else
            {
                return currentValue;
            }
        }

        public static T GetValue<T>(ConfigNode config, string name, T currentValue) where T : IComparable, IFormattable, IConvertible
        {
            if (config.HasValue(name))
            {
                string stringValue = config.GetValue(name);
                if (Enum.IsDefined(typeof(T), stringValue))
                {
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
            }

            return currentValue;
        }
#endif 
    }
}
