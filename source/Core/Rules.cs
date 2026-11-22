using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MoarKerbals.Core
{
    public sealed class Ingredient
    {
        public readonly string Name;
        public readonly double Amount;
        public Ingredient(string name, double amount) { Name = name; Amount = amount; }
    }

    public static class Rules
    {
        public static bool FiniteNonNegative(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        public static bool HasRoom(int count, int capacity, bool needsSource) => count >= 0 && count < capacity && (!needsSource || count > 0);
        public static bool Accident(double probability, double roll) => probability > 0 && roll < Math.Min(1, probability);
        public static bool CanAfford(double balance, double cost) => FiniteNonNegative(cost) && (cost == 0 || balance >= cost);
        public static int VolumeCapacity(int nominalSeats, double scale, double nominalScale)
        {
            if (nominalSeats < 1 || !FiniteNonNegative(scale) || scale == 0 || !FiniteNonNegative(nominalScale) || nominalScale == 0)
                throw new ArgumentOutOfRangeException("scale", "Capacity needs positive finite scale values.");
            double capacity = nominalSeats * Math.Pow(scale / nominalScale, 3);
            if (!FiniteNonNegative(capacity) || capacity > 256) throw new ArgumentOutOfRangeException("scale", "Unsupported cabin size.");
            return Math.Max(1, (int)Math.Floor(capacity + 1e-8));
        }

        public static int PreserveOccupiedSeats(int capacity, IEnumerable<int> occupiedSeatIndices)
        {
            foreach (int index in occupiedSeatIndices) if (index >= capacity) capacity = index + 1;
            return capacity;
        }
        public static bool SituationAllowed(string situation, bool enforce, bool surface, bool orbit)
        {
            if (!enforce) return true;
            switch (situation)
            {
                case "PRELAUNCH":
                case "LANDED":
                case "SPLASHED": return surface;
                case "ORBITING": return orbit;
                default: return false;
            }
        }

        public static Ingredient[] ParseRecipe(string names, string amounts)
        {
            if (string.IsNullOrWhiteSpace(names) && string.IsNullOrWhiteSpace(amounts)) return new Ingredient[0];
            var keys = (names ?? "").Split(',');
            var values = (amounts ?? "").Split(',');
            if (keys.Length != values.Length) throw new FormatException("Recipe names and amounts have different lengths.");
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i].Trim();
                double amount;
                if (key.Length == 0 || !double.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out amount) || !FiniteNonNegative(amount))
                    throw new FormatException("Invalid recipe ingredient: " + key);
                double existing;
                result.TryGetValue(key, out existing);
                result[key] = existing + amount;
                if (!FiniteNonNegative(result[key])) throw new FormatException("Recipe amount overflow.");
            }
            return result.Select(p => new Ingredient(p.Key, p.Value)).ToArray();
        }

        public static string[] Traits(string configured, Func<string, bool> exists) =>
            (configured ?? "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0 && s != "Civilian" && exists(s))
                .Distinct(StringComparer.Ordinal).ToArray();

        // Positive requests consume; negative requests return that resource. Failed operations undo their own partial state.
        public static bool Transact(Ingredient[] recipe, double multiplier, Func<bool> affordable,
            Func<string, double, double> request, Func<bool> operation, Action debit)
        {
            if (!FiniteNonNegative(multiplier) || !affordable()) return false;
            var consumed = new List<Ingredient>();
            bool committed = false;
            try
            {
                foreach (var ingredient in recipe)
                {
                    double required = ingredient.Amount * multiplier;
                    if (!FiniteNonNegative(required)) return false;
                    if (required == 0) continue;
                    double received = request(ingredient.Name, required);
                    if (!FiniteNonNegative(received)) throw new InvalidOperationException("Invalid resource receipt.");
                    consumed.Add(new Ingredient(ingredient.Name, received));
                    if (received < required - Math.Max(1e-9, required * 1e-9)) return false;
                }
                if (!operation()) return false;
                debit();
                committed = true;
                return true;
            }
            finally
            {
                if (!committed)
                    for (int i = consumed.Count - 1; i >= 0; i--)
                        request(consumed[i].Name, -consumed[i].Amount);
            }
        }

        public static double Advance(double progress, double elapsed, double required, bool enabled, bool hasPair)
        {
            if (!hasPair) return 0;
            if (!FiniteNonNegative(progress)) progress = 0;
            if (!enabled || !FiniteNonNegative(elapsed)) return progress;
            return Math.Min(Math.Max(0, required), progress + elapsed);
        }
    }
}
