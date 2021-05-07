using KSP_Log;
using UnityEngine;

namespace MoarKerbals
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Init : MonoBehaviour
    {
        internal static Log Log;
        void Start()
        {
#if DEBUG
            Log = new Log("MoarKerbals", Log.LEVEL.INFO);
#else
            Log = new Log("MoarKerbals", Log.LEVEL.ERROR);
#endif
        }
    }
}
