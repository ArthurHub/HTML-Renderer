// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Globalization;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Counters (CSS 2.1 §12.4, CSS Lists 3 §4), resolved into a per-box snapshot that <c>content: counter()</c> reads.
    /// </summary>
    internal static class CssCounterEngine
    {
        /// <summary>
        /// Stores the counter values in scope on every box's <see cref="CssBoxProperties.Counters"/>,
        /// including the implicit <c>list-item</c> counter. Runs once after the cascade, before layout.
        /// </summary>
        public static void ResolveCounters(CssBox root)
        {
            ResolveSiblings(new[] { root }, new Dictionary<string, List<int>>());
        }

        /// <summary>
        /// Each counter name maps to a stack of instances, one per nested scope (CSS Lists 3 §4.3).
        /// A reset's instance is popped again once all its siblings are done. Values carry over in tree
        /// order because every box writes into the same instance (CSS Lists 3 §4.4.1).
        /// </summary>
        private static void ResolveSiblings(IEnumerable<CssBox> boxes, Dictionary<string, List<int>> counters)
        {
            var created = new HashSet<string>();

            foreach (var box in boxes)
            {
                // CSS 2.1 §12.4.3
                if (box.Display == CssConstants.None)
                    continue;

                ApplyCounterReset(box, counters, created);
                ApplyCounterIncrement(box, counters, created);

                var snapshot = new Dictionary<string, int>(counters.Count);
                foreach (var pair in counters)
                    snapshot[pair.Key] = pair.Value[pair.Value.Count - 1];
                box.Counters = snapshot;

                ResolveSiblings(box.Boxes, counters);
            }

            foreach (var name in created)
            {
                var instances = counters[name];
                instances.RemoveAt(instances.Count - 1);
                if (instances.Count == 0)
                    counters.Remove(name);
            }
        }

        private static void ApplyCounterReset(CssBox box, Dictionary<string, List<int>> counters, HashSet<string> created)
        {
            if (string.IsNullOrEmpty(box.CounterReset) || box.CounterReset == CssConstants.None) return;

            ForEachNameValuePair(box.CounterReset, defaultValue: 0, (name, value) => Reset(counters, created, name, value));
        }

        private static void ApplyCounterIncrement(CssBox box, Dictionary<string, List<int>> counters, HashSet<string> created)
        {
            // CSS Lists 3 §4.6
            if (box.Display == CssConstants.ListItem && !CounterNameAppears(box.CounterIncrement, CssConstants.ListItem))
            {
                Increment(counters, created, CssConstants.ListItem, 1);
            }

            if (string.IsNullOrEmpty(box.CounterIncrement) || box.CounterIncrement == CssConstants.None) return;

            ForEachNameValuePair(box.CounterIncrement, defaultValue: 1, (name, value) => Increment(counters, created, name, value));
        }

        private static void Reset(Dictionary<string, List<int>> counters, HashSet<string> created, string name, int value)
        {
            // A second reset among the same siblings replaces that instance instead of nesting a new one
            // (CSS Lists 3 §4.4.2).
            if (created.Add(name))
            {
                if (!counters.TryGetValue(name, out var instances))
                    counters[name] = instances = new List<int>();
                instances.Add(value);
            }
            else
            {
                var instances = counters[name];
                instances[instances.Count - 1] = value;
            }
        }

        private static void Increment(Dictionary<string, List<int>> counters, HashSet<string> created, string name, int value)
        {
            // Not in scope, instantiate it with 0 on this box (CSS Lists 3 §4.2)
            if (!counters.TryGetValue(name, out var instances))
            {
                Reset(counters, created, name, 0);
                instances = counters[name];
            }

            instances[instances.Count - 1] += value;
        }

        private static bool CounterNameAppears(string counterPropertyValue, string counterName)
        {
            if (string.IsNullOrEmpty(counterPropertyValue) || counterPropertyValue == CssConstants.None) return false;

            var found = false;
            ForEachNameValuePair(counterPropertyValue, 0, (name, _) => found |= name == counterName);
            return found;
        }

        /// <summary>
        /// Calls <paramref name="apply"/> for each name of a <c>counter-reset</c>/<c>counter-increment</c>
        /// value, with <paramref name="defaultValue"/> when no integer follows the name.
        /// </summary>
        private static void ForEachNameValuePair(string value, int defaultValue, Action<string, int> apply)
        {
            var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; i++)
            {
                var name = parts[i];
                var next = i + 1 < parts.Length ? parts[i + 1] : null;

                if (next != null && int.TryParse(next, NumberStyles.Integer, CultureInfo.InvariantCulture, out var explicitValue))
                {
                    apply(name, explicitValue);
                    i++;
                }
                else
                {
                    apply(name, defaultValue);
                }
            }
        }

        /// <summary>
        /// Formats a counter value for <c>content: counter()</c> (CSS 2.1 §12.4.2), an unknown style as
        /// decimal (CSS Counter Styles 3 §5).
        /// </summary>
        public static string FormatCounterValue(int number, string style)
        {
            if (string.IsNullOrEmpty(style) || style.Equals(CssConstants.Decimal, StringComparison.OrdinalIgnoreCase))
            {
                return number.ToString(CultureInfo.InvariantCulture);
            }

            if (style.Equals(CssConstants.DecimalLeadingZero, StringComparison.OrdinalIgnoreCase))
            {
                return number.ToString("00", CultureInfo.InvariantCulture);
            }

            if (IsAlphabeticCounterStyle(style))
            {
                // Empty for 0 and negative values
                var formatted = CommonUtils.ConvertToAlphaNumber(number, style);
                return formatted.Length > 0 ? formatted : number.ToString(CultureInfo.InvariantCulture);
            }

            return number.ToString(CultureInfo.InvariantCulture);
        }

        private static bool IsAlphabeticCounterStyle(string style)
        {
            return style.Equals(CssConstants.LowerRoman, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.UpperRoman, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.LowerAlpha, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.UpperAlpha, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.LowerLatin, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.UpperLatin, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.LowerGreek, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.Armenian, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.Georgian, StringComparison.OrdinalIgnoreCase)
                   || style.Equals(CssConstants.Hebrew, StringComparison.OrdinalIgnoreCase);
        }
    }
}
