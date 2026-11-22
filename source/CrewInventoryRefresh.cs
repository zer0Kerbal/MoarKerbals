using System;
using System.Reflection;

namespace MoarKerbals
{
    internal static class CrewInventoryRefresh
    {
        // KIS is optional. Refresh its seat-labelled PAW events, without inventing a transfer
        // event or reinitializing inventories (which could move or clear existing items).
        internal static void Refresh(Part part)
        {
            if (part == null) return;
            foreach (PartModule module in part.Modules)
            {
                if (module == null || module.moduleName != "ModuleKISInventory") continue;
                try
                {
                    var method = module.GetType().GetMethod("UpdateContextMenu", BindingFlags.Public | BindingFlags.Instance,
                        null, Type.EmptyTypes, null);
                    if (method != null) method.Invoke(module, null);
                    else Logging.DLogWarning("KIS menu refresh unavailable: UpdateContextMenu was not found.");
                }
                catch (Exception error)
                {
                    // An optional UI integration must never undo an already completed birth/payment.
                    Logging.DLogWarning("KIS menu refresh failed: " + (error.InnerException ?? error).Message);
                }
            }
            try
            {
                var controller = UIPartActionController.Instance;
                if (controller == null) return;
                var window = controller.GetItem(part, false);
                if (window != null) window.displayDirty = true;
            }
            catch (Exception error) { Logging.DLogWarning("Crew PAW refresh failed: " + error.Message); }
        }
    }
}
