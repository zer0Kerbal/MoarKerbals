using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

// This will add a tab to the Stock Settings in the Difficulty settings called "MoarKerbals"
// To use, reference the setting using the following:
//
//  HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().requireLanded
//
// As it is set up, the option is disabled, so in order to enable it, the player would have
// to deliberately go in and change it
//
namespace MoarKerbals
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class MoarKerbals_Options : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "[WIP] Default Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "[WIP] MoarKerbals"; } }
        public override string DisplaySection { get { return "[WIP] MoarKerbals"; } }
        public override int SectionOrder { get { return 1; } }

        /// <summary>
        /// The needs EC to start in GameParameters
        /// </summary>
        [GameParameters.CustomParameterUI("Event Sounds On? (Default = True)",
            toolTip = "if set to yes, kloning events will have sound.",
            newGameOnly = false,
            unlockedDuringMission = true
            )]
        public bool SoundOn = true;

        /// <summary>
        /// The automatic switch in GameParameters
        /// </summary>
        [GameParameters.CustomParameterUI("Send In Game Mail? (Default = True)",
            toolTip = "if yes, certain major kloning events will send an in-game mail.",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool InGameMail = true;

        /// <summary>
        /// The automatic switch in GameParameters
        /// </summary>
        [GameParameters.CustomParameterUI("Require Living Kerbal? (Default = True)",
            toolTip = "if no, allow kloning without any (living) Kerbal's in the part.",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool RequireLivingKerbal = true;

        /// <summary>
        /// The colored paw
        /// </summary>
        [GameParameters.CustomParameterUI("Use Color in PAW? (Default = True)",
            toolTip = "allow color coding in ODFC PAW (part action window) / part RMB (right menu button).",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool coloredPAW = true;

        /// <summary>
        /// Sets the globalScalingFactor in GameParameters
        /// </summary>
        [GameParameters.CustomFloatParameterUI("Global Cost Multiplier (Default = 1)",
            toolTip = "Scales production and consumption Globally on all kloning modules.",
            newGameOnly = false,
            unlockedDuringMission = true,
            minValue = 0.05f,
            maxValue = 5.0f,
            stepCount = 101,
            displayFormat = "F2",
            asPercentage = true)]
        public double globalKloningCostMultiplier = 1.0f;


        // If you want to have some of the game settings default to enabled,  change 
        // the "if false" to "if true" and set the values as you like

        [GameParameters.CustomIntParameterUI("Kuddle time needed for Kuddling (minutes)", minValue = 0, maxValue = 3600, stepSize = 10,
            toolTip = "Two kerbals in the KuddleShack will need this much time to produce a new kerbal")]
        public int kuddleTimeNeeded = 360; // 1 day

        [GameParameters.CustomIntParameterUI("Kuddle time update intervals (seconds)", minValue = 10, maxValue = 3600, stepSize = 10,
            toolTip = "Two kerbals in the KuddleShack will need this much time to produce a new kerbal")]
        public int slowUpdateTime = 10; 

#if true        
        /// <summary>
        /// Gets a value indicating whether this instance has presets.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has presets; otherwise, <c>false</c>.
        /// </value>
        public override bool HasPresets { get { return true; } }
        /// <summary>
        /// Sets the difficulty preset.
        /// </summary>
        /// <param name="preset">The preset.</param>
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    RequireLivingKerbal = false;
                    globalKloningCostMultiplier = 0.75f;
                    break;

                case GameParameters.Preset.Normal:
                    RequireLivingKerbal = true;
                    globalKloningCostMultiplier = 1.0f;
                    break;

                case GameParameters.Preset.Moderate:
                    RequireLivingKerbal = true;
                    globalKloningCostMultiplier = 2.0f;
                    break;

                case GameParameters.Preset.Hard:
                    RequireLivingKerbal = true;
                    globalKloningCostMultiplier = 3.0f;
                    break;
            }
        }

#else
        public override bool HasPresets { get { return false; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public override IList ValidValues(MemberInfo member) { return null; }
    }
}

   