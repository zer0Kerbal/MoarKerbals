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
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().RequireLivingKerbal;
        }

        internal static bool ColoredPAW()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().coloredPAW;
        }

        internal static bool InGameMail()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().InGameMail;
        }

        internal static bool SoundOn()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().SoundOn;
        }

        internal static double globalKloningCostMultiplier()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarKerbals_Options>().globalKloningCostMultiplier;
        }
    }
}
