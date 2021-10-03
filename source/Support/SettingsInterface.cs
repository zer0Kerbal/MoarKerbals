// SettingsInterface.cs v1.0.0.0
#region CC BY-NC-SA 3.0
/** SettingsInterface.cs
 * 
 * KGEx's library for the Kerbal Space Program, by zer0Kerbal
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
using System.Linq;
using System.Text;

namespace MoarKerbals
{
    class SettingsInterface
    {
        //public static bool RequireLivingKerbal()
        //{
        //    return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().requireLivingKerbal;
        //}

        internal static bool ColoredPAW()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().coloredPAW;
        }

        internal static bool InGameMail()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().gameMail;
        }

        internal static bool SoundOn()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().soundOn;
        }

        internal static double globalKloningCostMultiplier()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().globalKloningCostMultiplier;
        }

        internal static double kuddleTimeNeeded()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().kuddleTimeNeeded * 60;
        }
        internal static double slowUpdateTime()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().slowUpdateTime;
        }
    }
}
