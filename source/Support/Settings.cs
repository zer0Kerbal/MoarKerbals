// Settings.cs v1.0.0.0
#region CC BY-NC-SA 3.0
/* Settings.cs
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
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using KSP.Localization;

// This will add a tab to the Stock Settings in the Difficulty settings called "MoarKerbals"
// To use, reference the setting using the following:
//
//  HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbalsSettings>().requireLanded
//
// As it is set up, the option is disabled, so in order to enable it, the player would have
// to deliberately go in and change it
//
namespace MoarKerbals
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    /// <summary>Game Settings</summary>
    /// <seealso cref="GameParameters.CustomParameterNode" />
    public class Settings : GameParameters.CustomParameterNode
    {
        /// <summary>Gets the game mode.</summary>
        /// <value>The game mode.</value>
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }

        /// <summary>Gets the section.</summary>
        /// <value>The section.</value>

        public override string Section { get { return "#MOAR-002"; } }
        /// <summary>Gets the display section.</summary>
        /// <value>The display section.</value>
        public override string DisplaySection { get { return "#MOAR-002"; } }

        /// <summary>Gets the game settings section title.</summary>
        /// <value>The title.</value>
        public override string Title { get { return "#MOAR-Settings-Title-1"; } }
        /// <summary>Gets the section order.</summary>
        /// <value>The section order.</value>
        public override int SectionOrder { get { return 1; } }

        /// <summary>Turn sounds on or off GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-sound", toolTip = "#MOAR-Settings-sound-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool soundOn = true;

        /// <summary>Turns sending game mail on or off in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-mail", toolTip = "#MOAR-Settings-mail-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool gameMail = true;

        /// <summary>Require Living Kerbal switch in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-living", toolTip = "#MOAR-Settings-living-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool requireLivingKerbal = true;

        /// <summary>Returns the current game setting for requirelivingkerbal.</summary>
        /// <returns>requireLivingKerbal</returns>
        public static bool RequireLivingKerbal()
        { return HighLogic.CurrentGame.Parameters.CustomParams<Settings>().requireLivingKerbal; }

        /// <summary>sets colored paw in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-paw", toolTip = "#MOAR-Settings-paw-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool coloredPAW = true;

        /// <summary>Sets the globalScalingFactor in GameParameters</summary>
        [GameParameters.CustomFloatParameterUI("#MOAR-Settings-scale", toolTip = "#MOAR-Settings-scale-tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0.05f, maxValue = 5.0f, stepCount = 101, displayFormat = "F2", asPercentage = true)]
        public double globalKloningCostMultiplier = 1.0f;

        /// <summary>The time needed to produce a new kerbal</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-time", toolTip = "#MOAR-Settings-time-tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 3600, stepSize = 10)]
        public int kuddleTimeNeeded = 360; // 1 day

        [GameParameters.CustomIntParameterUI("#MOAR-Settings-update", toolTip = "#MOAR-Settings-update-tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 10, maxValue = 3600, stepSize = 10)]
        public int slowUpdateTime = 10;

        /// <summary>sound clip to be played during xxx events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-0", toolTip = "#MOAR-Settings-soundClip-0-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 5, stepSize = 1)]
        public int soundClip0 = 0;

        /// <summary>sound clip to be played during xxx events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-1", toolTip = "#MOAR-Settings-soundClip-1-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 5, stepSize = 1)]
        public int soundClip1 = 0;

        /// <summary>sound clip to be played during xxx events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-2", toolTip = "#MOAR-Settings-soundClip-2-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 5, stepSize = 1)]
        public int soundClip2 = 0;

        /// <summary>sound clip volume</summary>
        [GameParameters.CustomFloatParameterUI("#MOAR-Settings-soundVolume", toolTip = "#MOAR-Settings-soundVolume-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0f, maxValue = 1f, stepCount = 1, displayFormat = "F2", asPercentage = true)]
        public float soundVolume = 0.75f;

        [GameParameters.CustomParameterUI("#MOAR-Settings-qt", toolTip = "#MOAR-Settings-qt-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool quietMode = false;

#if true
        /// <summary>Gets a value indicating whether this instance has presets.</summary>
        /// <value><c>true</c> if this instance has presets; otherwise, <c>false</c>.</value>
        public override bool HasPresets { get { return true; } }
        /// <summary>Sets the difficulty preset.</summary>
        /// <param name="preset">The preset.</param>
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    requireLivingKerbal = false;
                    globalKloningCostMultiplier = 0.75f;
                    break;

                case GameParameters.Preset.Normal:
                    requireLivingKerbal = true;
                    globalKloningCostMultiplier = 1.0f;
                    break;

                case GameParameters.Preset.Moderate:
                    requireLivingKerbal = true;
                    globalKloningCostMultiplier = 2.0f;
                    break;

                case GameParameters.Preset.Hard:
                    requireLivingKerbal = true;
                    globalKloningCostMultiplier = 3.0f;
                    break;
            }
        }

#else
        public override bool HasPresets { get { return false; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
#endif

        /// <summary>Enabled</summary>
        /// <param name="member"></param>
        /// <param name="parameters"></param>
        /// <returns>true</returns>
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }

        /// <summary>Interactible</summary>
        /// <param name="member"></param>
        /// <param name="parameters"></param>
        /// <returns>true</returns>
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }

        /// <summary>ValidValues</summary>
        /// <param name="member"></param>
        /// <returns>null</returns>
        public override IList ValidValues(MemberInfo member) { return null; }
    }
    class DebugSettings : GameParameters.CustomParameterNode
    {
        /// <summary>Gets the game mode.</summary>
        /// <value>The game mode.</value>
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }

        /// <summary>Gets the section.</summary>
        /// <value>The section.</value>

        public override string Section { get { return "#MOAR-002"; } }
        /// <summary>Gets the display section.</summary>
        /// <value>The display section.</value>
        public override string DisplaySection { get { return "#MOAR-002"; } }

        /// <summary>Gets the game settings section title.</summary>
        /// <value>The title.</value>
        public override string Title { get { return "#MOAR-Settings-Title-2"; } }
        /// <summary>Gets the section order.</summary>
        /// <value>The section order.</value>
        public override int SectionOrder { get { return 2; } }

        public override bool HasPresets { get { return false; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;

        [GameParameters.CustomParameterUI("#MOAR-Settings-log", toolTip = "#MOAR-Settings-log-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool logging = false;

        [GameParameters.CustomParameterUI("#MOAR-Settings-dbg", toolTip = "#MOAR-Settings-dbg-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool debugMenu = true;

        /// <summary>Displays the installed version of MoarKerbals.dll</summary>
        [GameParameters.CustomParameterUI("MoarKerbals v: " + Version.Text)]
        public bool throwaway = false;
    }
}

