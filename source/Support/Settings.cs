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

        /// <summary>sound clip to be played during kloning success events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-A0", toolTip = "#MOAR-Settings-soundClip-A0-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 2, stepSize = 1)]
        public int soundClipA0 = 1;

        /// <summary>sound clip to be played during kloning failure events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-A1", toolTip = "#MOAR-Settings-soundClip-A1-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 3, stepSize = 1)]
        public int soundClipA1 = 1;

        /// <summary>sound clip to be played during kuddling success events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-B0", toolTip = "#MOAR-Settings-soundClip-B0-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 2, stepSize = 1)]
        public int soundClipB0 = 1;

        /// <summary>sound clip to be played during kuddling failure events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-B1", toolTip = "#MOAR-Settings-soundClip-B1-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 2, stepSize = 1)]
        public int soundClipB1 = 1;

        /// <summary>sound clip to be played during recruitment success events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-C0", toolTip = "#MOAR-Settings-soundClip-C0-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 2, stepSize = 1)]
        public int soundClipC0 = 1;

        /// <summary>sound clip to be played during recruitment failure events</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-C1", toolTip = "#MOAR-Settings-soundClip-C1-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 2, stepSize = 1)]
        public int soundClipC1 = 1;

        ///// <summary>sound clip to be played during recruitment failure events</summary>
        //[GameParameters.CustomIntParameterUI("#MOAR-Settings-soundClip-C2", toolTip = "#MOAR-Settings-soundClip-2-Tip",
        //    newGameOnly = false, unlockedDuringMission = true,
        //    minValue = 0, maxValue = 2, stepSize = 1)]
        //public int soundClip2 = 0;

        /// <summary>sound clip volume</summary>
        [GameParameters.CustomFloatParameterUI("#MOAR-Settings-soundVolume", toolTip = "#MOAR-Settings-soundVolume-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0f, maxValue = 1f, stepCount = 1, displayFormat = "F2", asPercentage = true)]
        public float soundVolume = 0.75f;

#if false 
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
                    // requireLivingKerbal = false;
                    // globalKloningCostMultiplier = 0.75f;
                    break;

                case GameParameters.Preset.Normal:
                    // requireLivingKerbal = true;
                    // globalKloningCostMultiplier = 1.0f;
                    break;

                case GameParameters.Preset.Moderate:
                    // requireLivingKerbal = true;
                    // globalKloningCostMultiplier = 2.0f;
                    break;

                case GameParameters.Preset.Hard:
                    // requireLivingKerbal = true;
                    // globalKloningCostMultiplier = 3.0f;
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
    class Settings2 : GameParameters.CustomParameterNode
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

        /// <summary>Turn sounds on or off GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-sound", toolTip = "#MOAR-Settings-sound-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool soundOn = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-va", toolTip = "#MOAR-Settings-va-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool entireVesselAcademy = false;

        [GameParameters.CustomParameterUI("#MOAR-Settings-recruitOnlyOne", toolTip = "#MOAR-Settings-recruitOnlyOne-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool recruitOnlyOne = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-do", toolTip = "#MOAR-Settings-do-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool dropOut = true;

        /// <summary>Require Living Kerbal for kloning switch in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-living", toolTip = "#MOAR-Settings-living-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool requireLivingKerbal = true;

        /// <summary>Enable Kribble mode: aka multiple klones/kuddles switch in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-multipleBirths", toolTip = "#MOAR-Settings-multipleBirths-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool KribbleMode = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-kc", toolTip = "#MOAR-Settings-kc-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool kloneCivilians = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-bc", toolTip = "#MOAR-Settings-bc-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool kuddleCivilians = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-sa", toolTip = "#MOAR-Settings-sa-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool situationallyAware = true;

        /// <summary>which suit new kerbals start with</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-Suit", toolTip = "#MOAR-Settings-Suit-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 3, stepSize = 1)]
        public int birthdaySuit = 0;

        /// <summary>the QuentinTarantino mode</summary>
        [GameParameters.CustomIntParameterUI("#MOAR-Settings-accident", toolTip = "#MOAR-Settings-accident-Tip",
            newGameOnly = false, unlockedDuringMission = true,
            minValue = 0, maxValue = 11, stepSize = 1)]
        public int QuentinTarantinoMode = 0;
    }
    class Settings3 : GameParameters.CustomParameterNode
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
        public override string Title { get { return "#MOAR-Settings-Title-3"; } }
        /// <summary>Gets the section order.</summary>
        /// <value>The section order.</value>
        public override int SectionOrder { get { return 2; } }

        public override bool HasPresets { get { return false; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;

        /// <summary>Displays the installed version of MoarKerbals.dll</summary>
        [GameParameters.CustomParameterUI("MoarKerbals v: " + Version.Text)]
        public bool throwaway = false;

        /// <summary>Turns sending game mail on or off in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-mail", toolTip = "#MOAR-Settings-mail-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool gameMail = false;

        /// <summary>sets colored paw in GameParameters</summary>
        [GameParameters.CustomParameterUI("#MOAR-Settings-paw", toolTip = "#MOAR-Settings-paw-tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool coloredPAW = true;

        [GameParameters.CustomParameterUI("#MOAR-Settings-qt", toolTip = "#MOAR-Settings-qt-Tip",
     newGameOnly = false, unlockedDuringMission = true)]
        public bool quietMode = false;

        [GameParameters.CustomParameterUI("#MOAR-Settings-log", toolTip = "#MOAR-Settings-log-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool logging = false;

        [GameParameters.CustomParameterUI("#MOAR-Settings-dbg", toolTip = "#MOAR-Settings-dbg-Tip",
            newGameOnly = false, unlockedDuringMission = true)]
        public bool debugMenu = false;
    }
}

