using System;

namespace MoarKerbals.Core
{
    internal static class GenderLabels
    {
        internal static string Name(string name, bool male) => name + (male ? " \u2642" : " \u2640");
        internal static string Pair(string male, string female) => Name(male, true) + " + " + Name(female, false);

        internal static string Heading(string text, string name, bool male)
        {
            // Crew headings include stock volume/mass suffixes. Inventory buttons and other text are untouched.
            if (text == name) return Name(name, male);
            if (text != null && text.StartsWith(name + " (", StringComparison.Ordinal))
                return Name(name, male) + text.Substring(name.Length);
            return text;
        }

        // Signed distance for two simple vector glyphs. Positive distances are outside the strokes.
        // The runtime creates a small SDF font atlas; no external font or stock asset modification is needed.
        internal static double Distance(bool male, double x, double y)
        {
            double ring = Math.Abs(Math.Sqrt((x - 28) * (x - 28) + (y - 47) * (y - 47)) - 14) - 2.5;
            if (male)
                return Math.Min(ring, Math.Min(Segment(x, y, 38, 57, 53, 72),
                    Math.Min(Segment(x, y, 41, 72, 53, 72), Segment(x, y, 53, 60, 53, 72))));
            return Math.Min(ring, Math.Min(Segment(x, y, 28, 10, 28, 33), Segment(x, y, 18, 19, 38, 19)));
        }

        private static double Segment(double x, double y, double ax, double ay, double bx, double by)
        {
            double dx = bx - ax, dy = by - ay;
            double t = Math.Max(0, Math.Min(1, ((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy)));
            return Math.Sqrt(Math.Pow(x - ax - t * dx, 2) + Math.Pow(y - ay - t * dy, 2)) - 2.5;
        }
    }
}
