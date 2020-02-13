using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoarKerbals
{
    class SettingsInterface
    {
        public static bool RequireLivingKerbal()
        {
            Log.dbg("RequireLivingKerbal: " + HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().RequireLivingKerbal);
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().RequireLivingKerbal;
        }

        internal static bool ColoredPAW()
        {
            Log.dbg("coloredPAW: " + HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().coloredPAW);
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().coloredPAW;
        }

        internal static bool InGameMail()
        {
            Log.dbg("InGameMail: " + HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().InGameMail);
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().InGameMail;
        }

        internal static bool SoundOn()
        {
            Log.dbg("SoundOn: " + HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().SoundOn);
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().SoundOn;
        }

        internal static double globalKloningCostMultiplier()
        {
            Log.dbg("globalKloningCostMultiplier: " + HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier);
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;
        }
    }
}
