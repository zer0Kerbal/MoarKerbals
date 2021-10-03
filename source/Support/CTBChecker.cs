// CTBChecker.cs v1.0.0.0
#region CC BY-NC-SA 3.0
/**
*Utilities.cs
 * 
 * KGEx's library for the Kerbal Space Program, by zer0Kerbal
 * 
 * (C) Copyright 2021, zer0Kerbal
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
 * is purely coincidental.
 */
#endregion
#region Author: zer0Kerbal (zer0Kerbal@hotmail.com)
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace MoarKerbals
{

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class CTBChecker : MonoBehaviour
    {
        public static bool CTB = false;

        protected void Start()
        {
            if (!AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.StartsWith("ClickThroughBlocker") && a.url != ""))
            {

                Debug.Log("ClickThroughBlocker Installed");
                CTB = true;
            }
            else
            {
                Debug.Log("ClickThroughBlocker Not Installed");
                CTB = false;
            }
        }

        /*
		// then in code:
		//
		// using ClickThroughFix;
		void onGUI()
		{
			if (CTB)
			{
				ClickThruBlocker.GUILayoutWindow();
				ClickThruBlocker.GUIWindow();
			}
			else
			{
				GUILayout.Window();
				GUI.Window();
			}
		}
		...
		*/
    }
}
