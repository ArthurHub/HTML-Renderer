using System;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// Content that can't be broken (css-break-3 §2, CSS 2.1 §13.3.5 for replaced elements).
    /// </summary>
    /// <remarks>
    /// <see cref="IsMonolithic"/> is a property of the content. <see cref="PaginatesItsOwnContent"/> is an
    /// implementation constraint, the table layout fragments its own subtree.
    /// </remarks>
    internal static class MonolithicContent
    {
        /// <summary>Whether §2 forbids breaking inside <paramref name="box"/>.</summary>
        internal static bool IsMonolithic(CssBox box) => IsReplaced(box) || IsScrollContainer(box);

        /// <summary>
        /// Whether <paramref name="box"/> is a replaced element (an image or a frame).
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
        /// Whether <paramref name="box"/> runs a layout engine that fragments its own subtree, i.e. is a table.
        /// </summary>
        internal static bool PaginatesItsOwnContent(CssBox box) => RunsAnEngineOfItsOwn(box.Display);

        /// <summary>
        /// The display-value half of <see cref="PaginatesItsOwnContent"/>.
        /// </summary>
        internal static bool RunsAnEngineOfItsOwn(string display) =>
            display is CssConstants.Table or CssConstants.InlineTable;

        /// <summary>
        /// Whether <paramref name="box"/> must be treated as an indivisible unit by its parent's
        /// fragmentation. Currently the same set as <see cref="IsMonolithic"/>.
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
