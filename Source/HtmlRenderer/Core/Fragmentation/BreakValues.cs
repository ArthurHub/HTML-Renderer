using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Classifies a cascaded <c>break-before</c>/<c>break-after</c>/<c>break-inside</c> value, per
    /// https://www.w3.org/TR/css-break-3/#break-between (CSS Fragmentation Level 3 §3.1/§3.2). Ported from
    /// PeachPDF's <c>BreakValues</c>, reduced to this port's scope: pages only (no multi-column, so no
    /// <c>column</c>/<c>avoid-column</c> handling) and no directional breaks (no <c>left</c>/<c>right</c>/
    /// <c>recto</c>/<c>verso</c>/<c>@page :left</c>/<c>:right</c> matching - see the plan's scope decision).
    /// </summary>
    /// <remarks>
    /// One home for every question layout asks about a break value, so a future widening of the accepted
    /// value set only has to change one place.
    /// </remarks>
    internal static class BreakValues
    {
        /// <summary>
        /// Whether <paramref name="value"/> forces a page break: <c>page</c>, or the legacy
        /// <c>page-break-before</c>/<c>page-break-after: always</c> value, which HTML-Renderer's CSS
        /// engine accepts directly on the modern properties too (see <c>BreakMode</c>) rather than
        /// normalizing it away at parse time - so both spellings are classified here.
        /// </summary>
        internal static bool IsForcedBreak(string value) =>
            value is CssConstants.Page or CssConstants.Always;

        /// <summary>
        /// Whether <paramref name="value"/> forbids a break - <c>avoid</c> (both <c>break-inside</c> and
        /// the legacy <c>page-break-inside</c> use it) or <c>avoid-page</c>.
        /// </summary>
        internal static bool AvoidsBreak(string value) =>
            value is CssConstants.Avoid or CssConstants.AvoidPage;
    }
}
