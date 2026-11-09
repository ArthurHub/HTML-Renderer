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
    /// Resolves CSS 2.1 §12.4 named counters (<c>counter-reset</c>/<c>counter-increment</c>) into a
    /// per-box snapshot of counter values, consulted by <c>content: counter()</c>
    /// (<see cref="CssContentEngine"/>).<br/>
    /// <c>reversed(&lt;name&gt;)</c> and <c>counter-set</c> are not implemented.
    /// </summary>
    internal static class CssCounterEngine
    {
        /// <summary>
        /// Walks <paramref name="root"/> and its descendants in document order, applying each box's own
        /// <c>counter-reset</c>/<c>counter-increment</c> (plus the implicit <c>list-item</c> counter a
        /// <c>display:list-item</c> box always increments) and storing the values in scope on
        /// <see cref="CssBoxProperties.Counters"/>. Must run once, before layout, after every box's CSS
        /// properties are resolved.
        /// </summary>
        public static void ResolveCounters(CssBox root)
        {
            ResolveSiblings(new[] { root }, new Dictionary<string, List<int>>());
        }

        /// <summary>
        /// Each counter name maps to a stack of instances. A reset creates a new instance that stays in
        /// scope for the box's following siblings and all their descendants, and goes out of scope again
        /// once the parent ends (§12.4.1). Boxes with <c>display:none</c> don't touch counters (§12.4.3).
        /// </summary>
        private static void ResolveSiblings(IEnumerable<CssBox> boxes, Dictionary<string, List<int>> counters)
        {
            var created = new HashSet<string>();

            foreach (var box in boxes)
            {
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
            if (box.Display == CssConstants.ListItem && !CounterNameAppears(box.CounterIncrement, CssConstants.ListItem))
            {
                Increment(counters, created, CssConstants.ListItem, 1);
            }

            if (string.IsNullOrEmpty(box.CounterIncrement) || box.CounterIncrement == CssConstants.None) return;

            ForEachNameValuePair(box.CounterIncrement, defaultValue: 1, (name, value) => Increment(counters, created, name, value));
        }

        private static void Reset(Dictionary<string, List<int>> counters, HashSet<string> created, string name, int value)
        {
            // A second reset among the same siblings replaces that instance instead of nesting a new one.
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
            // Incrementing a counter that isn't in scope behaves as if this box had reset it to 0.
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
        /// Parses a <c>counter-reset</c>/<c>counter-increment</c> value - a space-separated sequence of
        /// <c>&lt;name&gt; [&lt;integer&gt;]?</c> pairs - invoking <paramref name="apply"/> once per name,
        /// with <paramref name="defaultValue"/> when no integer follows a given name.
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
        /// Formats a resolved counter value as a string using the given CSS counter style (<c>decimal</c>,
        /// <c>decimal-leading-zero</c>, <c>lower-roman</c>, <c>upper-alpha</c>, etc.) - the single
        /// counter-style resolver <c>content: counter()</c> uses. Per
        /// <see href="https://www.w3.org/TR/css-counter-styles-3/">CSS Counter Styles Level 3 §2</see>, an
        /// unknown/invalid style, or a value an alphabetic/symbolic style cannot represent (0 or negative),
        /// falls back to <c>decimal</c> rather than rendering nothing.
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
                var formatted = CommonUtils.ConvertToAlphaNumber(number, style);

                // Alphabetic/symbolic styles have no representation for values outside their range (e.g.
                // 0 or negatives, for which ConvertToAlphaNumber yields the empty string) - such values
                // fall back to decimal too, per CSS Counter Styles Level 3 §2.
                return formatted.Length > 0 ? formatted : number.ToString(CultureInfo.InvariantCulture);
            }

            // Any other unknown/invalid style falls back to decimal.
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
