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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using HtmlRenderer.Core.Dom;
using HtmlRenderer.Core.Entities;

namespace HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Provides some drawing functionality
    /// </summary>
    internal static class RenderUtils
    {
        #region Fields and Consts

        /// <summary>
        /// cache of brush color to brush instance
        /// </summary>
        private static readonly Dictionary<Color, Brush> _brushesCache = new Dictionary<Color, Brush>();

        /// <summary>
        /// cache of pen color to pen instance
        /// </summary>
        private static readonly Dictionary<Color, Pen> _penCache = new Dictionary<Color, Pen>();

        /// <summary>
        /// image used to draw loading image icon
        /// </summary>
        private static Image _loadImage;

        /// <summary>
        /// image used to draw error image icon
        /// </summary>
        private static Image _errorImage;

        #endregion


        /// <summary>
        /// Check if the given color is visible if painted (has alpha and color values)
        /// </summary>
        /// <param name="color">the color to check</param>
        /// <returns>true - visible, false - not visible</returns>
        public static bool IsColorVisible(Color color)
        {
            return color.A > 0;
        }

        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        public static Brush GetSolidBrush(Color color)
        {
            if (color == Color.White)
            {
                return Brushes.White;
            }
            else if (color == Color.Black)
            {
                return Brushes.Black;
            }
            else if (!IsColorVisible(color))
            {
                return Brushes.Transparent;
            }
            else
            {
                Brush brush;
                if (!_brushesCache.TryGetValue(color, out brush))
                {
                    brush = new SolidBrush(color);
                    _brushesCache[color] = brush;
                }
                return brush;
            }
        }

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        public static Pen GetPen(Color color)
        {
            Pen pen;
            if (!_penCache.TryGetValue(color, out pen))
            {
                pen = new Pen(GetSolidBrush(color));
                _penCache[color] = pen;
            }
            else
            {
                pen.Width = 1;                
            }
            return pen;
        }

        /// <summary>
        /// Clip the region the graphics will draw on by the overflow style of the containing block.<br/>
        /// Recursively travel up the tree to find containing block that has overflow style set to hidden. if not
        /// block found there will be no clipping and null will be returned.
        /// </summary>
        /// <param name="g">the graphics to clip</param>
        /// <param name="box">the box that is rendered to get containing blocks</param>
        /// <returns>the prev region if clipped, otherwise null</returns>
        public static RectangleF ClipGraphicsByOverflow(IGraphics g, CssBox box)
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
                    g.SetClip(rect);
                    return prevClip;
                }
                else
                {
                    var cBlock = containingBlock.ContainingBlock;
                    if (cBlock == containingBlock)
                        return RectangleF.Empty;
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
        public static void ReturnClip(IGraphics g, RectangleF prevClip)
        {
            if (prevClip != RectangleF.Empty)
            {
                g.SetClip(prevClip);
            }
        }
        
        /// <summary>
        /// Draw image loading icon.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="r">the rectangle to draw icon in</param>
        public static void DrawImageLoadingIcon(IGraphics g, RectangleF r)
        {
            g.DrawRectangle(Pens.LightGray, r.Left + 3, r.Top + 3, 13, 14);
            var image = GetLoadImage();
            g.DrawImage(image, new RectangleF(r.Left + 4, r.Top + 4, image.Width, image.Height));
        }

        /// <summary>
        /// Draw image failed to load icon.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="r">the rectangle to draw icon in</param>
        public static void DrawImageErrorIcon(IGraphics g, RectangleF r)
        {
            g.DrawRectangle(Pens.LightGray, r.Left + 2, r.Top + 2, 15, 15);
            var image = GetErrorImage();
            g.DrawImage(image, new RectangleF(r.Left + 3, r.Top + 3, image.Width, image.Height));
        }

        /// <summary>
        /// Creates a rounded rectangle using the specified corner radius
        /// </summary>
        /// <param name="rect">Rectangle to round</param>
        /// <param name="nwRadius">Radius of the north east corner</param>
        /// <param name="neRadius">Radius of the north west corner</param>
        /// <param name="seRadius">Radius of the south east corner</param>
        /// <param name="swRadius">Radius of the south west corner</param>
        /// <returns>GraphicsPath with the lines of the rounded rectangle ready to be painted</returns>
        public static GraphicsPath GetRoundRect(RectangleF rect, float nwRadius, float neRadius, float seRadius, float swRadius)
        {
            //  NW-----NE
            //  |       |
            //  |       |
            //  SW-----SE

            var path = new GraphicsPath();

            nwRadius *= 2;
            neRadius *= 2;
            seRadius *= 2;
            swRadius *= 2;

            //NW ---- NE
            path.AddLine(rect.X + nwRadius, rect.Y, rect.Right - neRadius, rect.Y);

            //NE Arc
            if( neRadius > 0f )
            {
                path.AddArc(
                    RectangleF.FromLTRB(rect.Right - neRadius, rect.Top, rect.Right, rect.Top + neRadius),
                    -90, 90);
            }

            // NE
            //  |
            // SE
            path.AddLine(rect.Right, rect.Top + neRadius, rect.Right, rect.Bottom - seRadius);

            //SE Arc
            if( seRadius > 0f )
            {
                path.AddArc(
                    RectangleF.FromLTRB(rect.Right - seRadius, rect.Bottom - seRadius, rect.Right, rect.Bottom),
                    0, 90);
            }

            // SW --- SE
            path.AddLine(rect.Right - seRadius, rect.Bottom, rect.Left + swRadius, rect.Bottom);

            //SW Arc
            if( swRadius > 0f )
            {
                path.AddArc(
                    RectangleF.FromLTRB(rect.Left, rect.Bottom - swRadius, rect.Left + swRadius, rect.Bottom),
                    90, 90);
            }

            // NW
            // |
            // SW
            path.AddLine(rect.Left, rect.Bottom - swRadius, rect.Left, rect.Top + nwRadius);

            //NW Arc
            if( nwRadius > 0f )
            {
                path.AddArc(
                    RectangleF.FromLTRB(rect.Left, rect.Top, rect.Left + nwRadius, rect.Top + nwRadius),
                    180, 90);
            }

            path.CloseFigure();

            return path;
        }


        #region Private methods

        /// <summary>
        /// Get singleton instance of load image.
        /// </summary>
        /// <returns>image instance</returns>
        private static Image GetLoadImage()
        {
            if( _loadImage == null )
            {
                var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageLoad.png");
                if( stream != null )
                    _loadImage = Image.FromStream(stream);
            }
            return _loadImage;
        }

        /// <summary>
        /// Get singleton instance of error image.
        /// </summary>
        /// <returns>image instance</returns>
        private static Image GetErrorImage()
        {
            if( _errorImage == null )
            {
                var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageError.png");
                if (stream != null)
                    _errorImage = Image.FromStream(stream);
            }
            return _errorImage;
        }

        #endregion
    }
}