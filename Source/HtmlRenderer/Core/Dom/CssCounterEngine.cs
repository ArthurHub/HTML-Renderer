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
    /// Deliberately a subset of the full spec algorithm: counters thread forward through document order
    /// (a box inherits its parent's counters for its first child, and each child's resulting counters
    /// feed the next sibling) which covers ordinary sequential/nested counter usage, but
    /// <c>reversed(&lt;name&gt;)</c> and <c>counter-set</c> are not implemented, and a nested
    /// <c>counter-reset</c> for a name already in scope always starts a fresh inner value rather than
    /// the spec's more precise "new scope only among this element's own following siblings" rule -
    /// in practice indistinguishable from the full rule for the common case of one counter-reset per
    /// counter name in a document, which is what this port's test coverage exercises.
    /// </summary>
    internal static class CssCounterEngine
    {
        /// <summary>
        /// Walks <paramref name="box"/> and its descendants in document order, applying each box's own
        /// <c>counter-reset</c>/<c>counter-increment</c> (plus the implicit <c>list-item</c> counter a
        /// <c>display:list-item</c> box always increments) on top of <paramref name="inherited"/>, and
        /// storing the result on <see cref="CssBoxProperties.Counters"/>. Must run once, before layout,
        /// after every box's CSS properties are resolved (<c>counter-reset</c>/<c>counter-increment</c>
        /// need their final cascaded values).
        /// </summary>
        /// <returns>
        /// The counters visible immediately after <paramref name="box"/> and its whole subtree - what a
        /// following sibling should inherit.
        /// </returns>
        public static Dictionary<string, int> ResolveCounters(CssBox box, Dictionary<string, int> inherited)
        {
            var counters = new Dictionary<string, int>(inherited);

            ApplyCounterReset(box, counters);
            ApplyCounterIncrement(box, counters);

            box.Counters = new Dictionary<string, int>(counters);

            foreach (var child in box.Boxes)
            {
                counters = ResolveCounters(child, counters);
            }

            return counters;
        }

        private static void ApplyCounterReset(CssBox box, Dictionary<string, int> counters)
        {
            if (string.IsNullOrEmpty(box.CounterReset) || box.CounterReset == CssConstants.None) return;

            ForEachNameValuePair(box.CounterReset, defaultValue: 0, (name, value) => counters[name] = value);
        }

        private static void ApplyCounterIncrement(CssBox box, Dictionary<string, int> counters)
        {
            if (box.Display == CssConstants.ListItem && !CounterNameAppears(box.CounterIncrement, CssConstants.ListItem))
            {
                counters[CssConstants.ListItem] = GetOrDefault(counters, CssConstants.ListItem) + 1;
            }

            if (string.IsNullOrEmpty(box.CounterIncrement) || box.CounterIncrement == CssConstants.None) return;

            ForEachNameValuePair(box.CounterIncrement, defaultValue: 1,
                (name, value) => counters[name] = GetOrDefault(counters, name) + value);
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}.GetValueOrDefault(TKey)"/> stand-in: this project also
        /// targets netstandard2.0/net462, where that method doesn't exist.
        /// </summary>
        private static int GetOrDefault(Dictionary<string, int> counters, string name)
        {
            int value;
            return counters.TryGetValue(name, out value) ? value : 0;
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
