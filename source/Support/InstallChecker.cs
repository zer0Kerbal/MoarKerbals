/*
 * InstallChecker.cs
 * version 2.1.0.0
*/

/* 
 * Based on the InstallChecker from the Kethane mod for Kerbal Space Program by Majiir.
 * https://github.com/Majiir/Kethane/blob/b93b1171ec42b4be6c44b257ad31c7efd7ea1702/Plugin/InstallChecker.cs
 * 
 * This file has been modified extensively by zer0Kerbal (zer0Kerbal@hotmail.com).
 */
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP.Localization;

namespace MoarKerbals
{
    /// <summary>MainMenu feedback</summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallChecker : MonoBehaviour
    {
        private const string MODNAME = "MoarKerbals";
        private const string FOLDERNAME = "KerbthulhuKineticsProgram/MoarKerbals";
        private const string EXPECTEDPATH = FOLDERNAME + "/Plugins";

        protected void Start()
        {
            // Search for this mod's DLL existing in the wrong location. This will also detect duplicate copies because only one can be in the right place.
            var assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name).Where(a => a.url != EXPECTEDPATH);
            if (assemblies.Any())
            {
                var badPaths = assemblies.Select(a => a.path).Select(p => Uri.UnescapeDataString(new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath)).MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar)));
                _ = PopupDialog.SpawnPopupDialog
                (
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    dialogName: "InstallChecker",
                    title: Localizer.Format("#MOAR-IC-00" + " InstallChecker", Version.SText),
                    message: Localizer.Format("#MOAR-IC-01", Version.Text, Localizer.Format("#MOAR-IC-00"), FOLDERNAME, String.Join("\n", badPaths.ToArray())),
                    //"Incorrect <<1>> v<<2>> installation.\n<<1>> has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/<<3>>. Do not move any files from inside that folder.\n\nIncorrect path(s):\n<<4>>
                    buttonMessage: "OK",
                    persistAcrossScenes: false,
                    skin: HighLogic.UISkin
                );
                //Debug.Log("Incorrect " + MODNAME + " Installation: " + MODNAME + " has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/" + EXPECTEDPATH + ". Do not move any files from inside that folder.\n\nIncorrect path(s):\n" + String.Join("\n", badPaths.ToArray())
                Debug.Log(logMsg: $"[{MODNAME}] Incorrect {MODNAME} v {Version.Text} Installation: {MODNAME} has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/{EXPECTEDPATH}. Do not move any files from inside that folder.\n\nIncorrect path(s):\n{String.Join("\n", badPaths.ToArray())}.");
            }

            //// Check for Module Manager
            //if (!AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.StartsWith("ModuleManager") && a.url == ""))
            //{
            //    PopupDialog.SpawnPopupDialog("Missing Module Manager",
            //        modName + " requires the Module Manager mod in order to function properly.\n\nPlease download from http://forum.kerbalspaceprogram.com/threads/55219 and copy to the KSP/GameData/ directory.",
            //        "OK", false, HighLogic.Skin);
            //}

            CleanupOldVersions();
        }

        [KSPAddon(KSPAddon.Startup.Instantly, true)]
        internal class Startup : MonoBehaviour
        {
            /// <summary>the meat and potatoes</summary>
            private void Start()
            {
                string v = "n/a";
                AssemblyTitleAttribute attributes = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false);
                string title = attributes?.Title;
                if (title == null)
                {
                    title = "TitleNotAvailable";
                }
                v = Assembly.GetExecutingAssembly().FullName;
                if (v == null)
                {
                    v = "VersionNotAvailable";
                }
                Debug.Log("[" + title + "] Version " + v);
            }
        }

        /// <summary>Tries to fix the install if it was installed over the top of a previous version</summary>
        void CleanupOldVersions()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Debug.LogError("-ERROR- " + this.GetType().FullName + "[" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " +
                   "Exception caught while cleaning up old files.\n" + ex.Message + "\n" + ex.StackTrace);

            }
        }
    }
}
