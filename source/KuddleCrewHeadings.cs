using System;
using TMPro;
using MoarKerbals.Core;

namespace MoarKerbals
{
    internal static class KuddleCrewHeadings
    {
        internal static void Refresh(Part part)
        {
            GenderSymbolFont.Ensure();
            var controller = UIPartActionController.Instance;
            if (controller == null || part == null) return;
            var window = controller.GetItem(part, false);
            if (window == null) return;
            foreach (var label in window.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string text = label.text;
                foreach (var kerbal in part.protoModuleCrew)
                    text = GenderLabels.Heading(text, kerbal.displayName, kerbal.gender == ProtoCrewMember.Gender.Male);
                if (text != label.text) label.text = text;
            }
        }
    }
}
