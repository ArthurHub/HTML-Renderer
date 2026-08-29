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

using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Handlers;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS box for hr element.
    /// </summary>
    internal sealed class CssBoxHr : CssBox
    {
        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="parent">the parent box of this box</param>
        /// <param name="tag">the html tag data of this box</param>
        public CssBoxHr(CssBox parent, HtmlTag tag)
            : base(parent, tag)
        {
            Display = CssConstants.Block;
        }

        /// <summary>
        /// Measures the bounds of box and children, recursively.<br/>
        /// Performs layout of the DOM structure creating lines by set bounds restrictions.
        /// </summary>
        /// <param name="g">Device context to use</param>
        protected override void PerformLayoutImp(RGraphics g)
        {
            if (Display == CssConstants.None)
                return;

            RectanglesReset();

            var prevSibling = DomUtils.GetPreviousSibling(this);
            double left = ContainingBlock.Location.X + ContainingBlock.ActualPaddingLeft + ActualMarginLeft + ContainingBlock.ActualBorderLeftWidth;
            // The baseline for this rule's own collapsed top margin, matching CssBox.PerformLayoutImp's
            // own placement: a self-collapsing prevSibling (CSS 2.1 8.3.1) contributes no space of its own
            // and must not be anchored against directly (see FindMarginAnchorSibling).
            var marginAnchor = FindMarginAnchorSibling(prevSibling);
            // StaticBottom (not ActualBottom): a relatively-positioned previous sibling's visual offset must
            // not drag this rule down with it (CSS 2.1 9.4.3), matching the same fix in CssBox.PerformLayoutImp.
            double top = (marginAnchor == null && ParentBox != null ? ParentBox.ClientTop : ParentBox == null ? Location.Y : 0) + MarginTopCollapse(prevSibling) + (marginAnchor != null ? marginAnchor.StaticBottom + marginAnchor.ActualBorderBottomWidth : 0);
            Location = new RPoint(left, top);
            ActualBottom = top;

            //width at 100% (or auto)
            double minwidth = GetMinimumWidth();
            double width = ContainingBlock.Size.Width
                           - ContainingBlock.ActualPaddingLeft - ContainingBlock.ActualPaddingRight
                           - ContainingBlock.ActualBorderLeftWidth - ContainingBlock.ActualBorderRightWidth
                           - ActualMarginLeft - ActualMarginRight - ActualBorderLeftWidth - ActualBorderRightWidth;

            //Check width if not auto
            if (Width != CssConstants.Auto && !string.IsNullOrEmpty(Width))
            {
                width = CssValueParser.ParseLength(Width, width, this);
            }

            if (width < minwidth || width >= 9999)
                width = minwidth;

            double height = ActualHeight;
            if (height < 1)
            {
                height = Size.Height + ActualBorderTopWidth + ActualBorderBottomWidth;
            }
            if (height < 1)
            {
                height = 2;
            }
            if (height <= 2 && ActualBorderTopWidth < 1 && ActualBorderBottomWidth < 1)
            {
                BorderTopStyle = BorderBottomStyle = CssConstants.Solid;
                BorderTopWidth = "1px";
                BorderBottomWidth = "1px";
            }

            Size = new RSize(width, height);

            ActualBottom = Location.Y + ActualPaddingTop + ActualPaddingBottom + height;
        }

        /// <summary>
        /// Draws the rule itself at <paramref name="rect"/> (already offset) - an <c>&lt;hr&gt;</c> has
        /// no separate background/border step shared with other replaced elements (it draws each border
        /// edge itself, not via <see cref="BordersDrawHandler.DrawBoxBorders"/>). Called by
        /// <see cref="Paint.Content.HrFragmentPainter"/>.
        /// </summary>
        internal void DrawHrContent(RGraphics g, RRect rect)
        {
            if (rect.Height > 2 && RenderUtils.IsColorVisible(ActualBackgroundColor))
            {
                g.DrawRectangle(g.GetSolidBrush(ActualBackgroundColor), rect.X, rect.Y, rect.Width, rect.Height);
            }

            var b1 = g.GetSolidBrush(ActualBorderTopColor);
            BordersDrawHandler.DrawBorder(Border.Top, g, this, b1, rect);

            if (rect.Height > 1)
            {
                var b2 = g.GetSolidBrush(ActualBorderLeftColor);
                BordersDrawHandler.DrawBorder(Border.Left, g, this, b2, rect);

                var b3 = g.GetSolidBrush(ActualBorderRightColor);
                BordersDrawHandler.DrawBorder(Border.Right, g, this, b3, rect);

                var b4 = g.GetSolidBrush(ActualBorderBottomColor);
                BordersDrawHandler.DrawBorder(Border.Bottom, g, this, b4, rect);
            }
        }
    }
}