using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Classifies <c>break-before</c>/<c>break-after</c>/<c>break-inside</c> values (CSS 2.1 §13.3.1,
    /// css-break-3 §3). Only pages fragment, so column and region values never force or avoid a break.
    /// <c>left</c> and <c>right</c> don't force a break.
    /// </summary>
    internal static class BreakValues
    {
        /// <summary>
        /// Whether <paramref name="value"/> forces a page break. The legacy <c>always</c> is accepted on
        /// <c>break-before</c>/<c>break-after</c> too, they share their value with <c>page-break-*</c>.
        /// </summary>
        internal static bool IsForcedBreak(string value) =>
            value is CssConstants.Page or CssConstants.Always;

        /// <summary>Whether <paramref name="value"/> avoids a page break.</summary>
        internal static bool AvoidsBreak(string value) =>
            value is CssConstants.Avoid or CssConstants.AvoidPage;
    }
}
