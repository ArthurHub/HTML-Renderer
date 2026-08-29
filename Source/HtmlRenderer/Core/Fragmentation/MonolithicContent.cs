using System;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Classifies content that cannot be broken, per
    /// https://www.w3.org/TR/css-break-3/#monolithic (CSS Fragmentation Level 3 §2). Ported from
    /// PeachPDF's <c>MonolithicContent</c>, reduced to what this port's scope needs: no flex/grid/columns
    /// (so <see cref="PaginatesItsOwnContent"/> narrows to "is a table"), no vertical writing mode, no
    /// box-decoration-break clone insets, and HTML-Renderer's own (smaller) set of replaced element types.
    /// </summary>
    /// <remarks>
    /// <see cref="IsMonolithic"/> is css-break-3 §2's own set: a property of the content, which no user
    /// agent may break. <see cref="PaginatesItsOwnContent"/> is an implementation constraint - the table
    /// engine fragments its own subtree, so the driver must not hand it a half-laid-out one. Keeping the
    /// two apart is the point of this file, exactly as in PeachPDF.
    /// </remarks>
    internal static class MonolithicContent
    {
        /// <summary>Whether §2 forbids breaking inside <paramref name="box"/>.</summary>
        internal static bool IsMonolithic(CssBox box) => IsReplaced(box) || IsScrollContainer(box);

        /// <summary>
        /// Whether <paramref name="box"/> is a replaced element, whose content the UA cannot fragment
        /// because it has no fragmentable inner structure. HTML-Renderer's replaced-element set is
        /// smaller than PeachPDF's - no &lt;object&gt;/&lt;video&gt;, inline SVG, or form-field widgets.
        /// </summary>
        internal static bool IsReplaced(CssBox box) => box is CssBoxImage or CssBoxFrame;

        /// <summary>
        /// Whether <paramref name="box"/> is a scroll container - §2's "elements with <c>overflow</c>
        /// other than <c>visible</c> or <c>clip</c>". The root element is excluded (its overflow
        /// propagates to the viewport rather than making it a scroll container, CSS Overflow 3 §3.3); the
        /// body is excluded only while the root's own overflow is still <c>visible</c>, per the same
        /// section's propagation rule.
        /// </summary>
        internal static bool IsScrollContainer(CssBox box) =>
            box.Overflow != CssConstants.Visible && !IsViewportPropagationSource(box);

        private static bool IsViewportPropagationSource(CssBox box)
        {
            if (IsRootElement(box)) return true;

            if (!IsNamed(box, "body") || box.ParentBox is not { } parent || !IsRootElement(parent))
                return false;

            return parent.Overflow == CssConstants.Visible;
        }

        private static bool IsRootElement(CssBox box) => box.ParentBox == null || IsNamed(box, "html");

        private static bool IsNamed(CssBox box, string name) =>
            string.Equals(box.HtmlTag?.Name, name, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Whether <paramref name="box"/> runs a layout engine that fragments its own subtree. In
        /// PeachPDF this covers flex, grid, table and multi-column; none of the first three exist in
        /// HTML-Renderer, so this narrows to table/inline-table.
        /// </summary>
        internal static bool PaginatesItsOwnContent(CssBox box) => RunsAnEngineOfItsOwn(box.Display);

        /// <summary>
        /// The display-value half of <see cref="PaginatesItsOwnContent"/>. Kept as its own method (rather
        /// than inlined) so a future engine addition only has to widen this one place, mirroring PeachPDF's
        /// shape even though it currently names only one display value.
        /// </summary>
        internal static bool RunsAnEngineOfItsOwn(string display) =>
            display is CssConstants.Table or CssConstants.InlineTable;

        /// <summary>
        /// Whether <paramref name="box"/> must be treated as an indivisible unit by its parent's own
        /// fragmentation. In PeachPDF this also covers unresumable vertical-writing-mode content; that
        /// doesn't exist in HTML-Renderer, so this is currently the same set as <see cref="IsMonolithic"/>.
        /// Kept as a separate name (rather than inlined at call sites) so a future reason can be added here
        /// without touching every caller.
        /// </summary>
        internal static bool IsMonolithicForFragmentation(CssBox box) => IsMonolithic(box);

        /// <summary>
        /// Whether content <paramref name="height"/> tall fits in no fragmentainer at all - §2's
        /// overflow-rather-than-slice rule. Content with nowhere to fit must not be treated as breakable:
        /// moving it only repeats the question on the next fragmentainer.
        /// </summary>
        internal static bool FitsNoFragmentainer(double height, HtmlContainerInt container) =>
            height >= container.PageSize.Height;

        /// <summary>
        /// Whether content <paramref name="height"/> tall fits inside a content band <paramref name="bandHeight"/>
        /// tall. Not the negation of <see cref="FitsNoFragmentainer"/>: this asks "will it fit there?" about
        /// one specific band, where an exact fit fits; that one asks "could this ever fit anywhere?" and
        /// treats an exact fit as not fitting.
        /// </summary>
        internal static bool FitsInBand(double height, double bandHeight) => height <= bandHeight;
    }
}
