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

using HtmlRenderer.Core.Dom;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;

namespace HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Provides some drawing functionality
    /// </summary>
    internal static class RenderUtils
    {
        /// <summary>
        /// Check if the given color is visible if painted (has alpha and color values)
        /// </summary>
        /// <param name="color">the color to check</param>
        /// <returns>true - visible, false - not visible</returns>
        public static bool IsColorVisible(ColorInt color)
        {
            return color.A > 0;
        }

        /// <summary>
        /// Clip the region the graphics will draw on by the overflow style of the containing block.<br/>
        /// Recursively travel up the tree to find containing block that has overflow style set to hidden. if not
        /// block found there will be no clipping and null will be returned.
        /// </summary>
        /// <param name="g">the graphics to clip</param>
        /// <param name="box">the box that is rendered to get containing blocks</param>
        /// <returns>the previous region if clipped, otherwise null</returns>
        public static RectangleInt ClipGraphicsByOverflow(IGraphics g, CssBox box)
        {
            var containingBlock = box.ContainingBlock;
            while (true)
            {
                if (containingBlock.Overflow == CssConstants.Hidden)
                {
                    var prevClip = g.GetClip();
                    var rect = box.ContainingBlock.ClientRectangle;
                    rect.X -= 2; // atodo: find better way to fix it
                    rect.Width += 2;
                    rect.Offset(box.HtmlContainer.ScrollOffset);
                    rect.Intersect(prevClip);
                    g.SetClipReplace(rect);
                    return prevClip;
                }
                else
                {
                    var cBlock = containingBlock.ContainingBlock;
                    if (cBlock == containingBlock)
                        return RectangleInt.Empty;
                    containingBlock = cBlock;
                }
            }
        }

        /// <summary>
        /// Return original clip region to the graphics object.<br/>
        /// Should be used with <see cref="ClipGraphicsByOverflow"/> return value to return clip back to original.
        /// </summary>
        /// <param name="g">the graphics to clip</param>
        /// <param name="prevClip">the region to set on the graphics (null - ignore)</param>
        public static void ReturnClip(IGraphics g, RectangleInt prevClip)
        {
            if (prevClip != RectangleInt.Empty)
            {
                g.SetClipReplace(prevClip);
            }
        }

        /// <summary>
        /// Draw image loading icon.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="htmlContainer"></param>
        /// <param name="r">the rectangle to draw icon in</param>
        public static void DrawImageLoadingIcon(IGraphics g, HtmlContainerInt htmlContainer, RectangleInt r)
        {
            g.DrawRectangle(g.GetPen(ColorInt.LightGray), r.Left + 3, r.Top + 3, 13, 14);
            var image = htmlContainer.Global.GetLoadImage();
            g.DrawImage(image, new RectangleInt(r.Left + 4, r.Top + 4, image.Width, image.Height));
        }

        /// <summary>
        /// Draw image failed to load icon.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="htmlContainer"></param>
        /// <param name="r">the rectangle to draw icon in</param>
        public static void DrawImageErrorIcon(IGraphics g, HtmlContainerInt htmlContainer, RectangleInt r)
        {
            g.DrawRectangle(g.GetPen(ColorInt.LightGray), r.Left + 2, r.Top + 2, 15, 15);
            var image = htmlContainer.Global.GetErrorImage();
            g.DrawImage(image, new RectangleInt(r.Left + 3, r.Top + 3, image.Width, image.Height));
        }

        /// <summary>
        /// Creates a rounded rectangle using the specified corner radius
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="rect">Rectangle to round</param>
        /// <param name="nwRadius">Radius of the north east corner</param>
        /// <param name="neRadius">Radius of the north west corner</param>
        /// <param name="seRadius">Radius of the south east corner</param>
        /// <param name="swRadius">Radius of the south west corner</param>
        /// <returns>GraphicsPath with the lines of the rounded rectangle ready to be painted</returns>
        public static IGraphicsPath GetRoundRect(IGraphics g, RectangleInt rect, float nwRadius, float neRadius, float seRadius, float swRadius)
        {
            //  NW-----NE
            //  |       |
            //  |       |
            //  SW-----SE

            var path = g.GetGraphicsPath();

            nwRadius *= 2;
            neRadius *= 2;
            seRadius *= 2;
            swRadius *= 2;

            //NW ---- NE
            path.AddLine(rect.X + nwRadius, rect.Y, rect.Right - neRadius, rect.Y);

            //NE Arc
            if( neRadius > 0f )
            {
                path.AddArc(rect.Right - neRadius, rect.Top, neRadius, neRadius, -90, 90);
            }

            // NE
            //  |
            // SE
            path.AddLine(rect.Right, rect.Top + neRadius, rect.Right, rect.Bottom - seRadius);

            //SE Arc
            if( seRadius > 0f )
            {
                path.AddArc(rect.Right - seRadius, rect.Bottom - seRadius, seRadius, seRadius, 0, 90);
            }

            // SW --- SE
            path.AddLine(rect.Right - seRadius, rect.Bottom, rect.Left + swRadius, rect.Bottom);

            //SW Arc
            if( swRadius > 0f )
            {
                path.AddArc(rect.Left, rect.Bottom - swRadius, swRadius, swRadius, 90, 90);
            }

            // NW
            // |
            // SW
            path.AddLine(rect.Left, rect.Bottom - swRadius, rect.Left, rect.Top + swRadius);

            //NW Arc
            if( nwRadius > 0f )
            {
                path.AddArc(rect.Left, rect.Top, nwRadius, nwRadius,180, 90);
            }

            return path;
        }
    }
}