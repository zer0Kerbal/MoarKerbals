
? Based upon debuggingClass from the 'CivilianPopulation' mod for Kerbal Space Program
  https://github.com/linuxgurugamer/CivilianPopulation

    LinuxGuruGamer
    CC BY-NC 4.0 (Attribution-NonCommercial 4.0 International) (https://creativecommons.org/licenses/by-nc/4.0/)
    specifically: https://github.com/linuxgurugamer/CivilianPopulation

    This file has been modified extensively and is released under the same license.




using System;
using UnityEngine;
using KSP;

namespace MoarKerbals
{
    public class debuggingClass
    {
        public static string civilianResource = "CivilianGrowthCounter";

        public static string civilianTrait = "Civilian"; //? The string corresponding to the trait for civilians

        public static string modName = "[MoarKerbals]"; //String to use for my mod's name

        public static string outputString; //This is the information shown in the debug window of KSP

        public static Rect _windowPosition = new Rect(); //used as a debug window for KSP

        public static void myWindow(int windowId)
        //This method creates the debug window for KSP.
        {
            GUILayout.BeginHorizontal(GUILayout.Width(250f));
            string displayedString = debuggingClass.modName + outputString;
            GUILayout.Label(displayedString);
            GUILayout.EndHorizontal();

            displayedString = displayedString.Remove(0); //clear outputString of previous information
            GUI.DragWindow(); //? Makes the rectangle drag-able?
        }
    }
}