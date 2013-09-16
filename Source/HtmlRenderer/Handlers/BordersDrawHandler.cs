// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using HtmlRenderer.Dom;
using HtmlRenderer.Entities;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Handlers
{
    /// <summary>
    /// Contains all the complex paint code to paint different style borders.
    /// </summary>
    internal static class BordersDrawHandler
    {
        #region Fields and Consts

        /// <summary>
        /// used for all border paint to use the same points and not create new array each time.
        /// </summary>
        private static readonly PointF[] _borderPts = new PointF[4];

        #endregion


        /// <summary>
        /// Draws all the border of the box with respect to style, width, etc.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="box">the box to draw borders for</param>
        /// <param name="rect">the bounding rectangle to draw in</param>
        /// <param name="isFirst">is it the first rectangle of the element</param>
        /// <param name="isLast">is it the last rectangle of the element</param>
        public static void DrawBoxBorders(IGraphics g, CssBox box, RectangleF rect, bool isFirst, bool isLast)
        {
            if( rect.Width > 0 && rect.Height > 0 )
            {
                if (!(string.IsNullOrEmpty(box.BorderTopStyle) || box.BorderTopStyle == CssConstants.None || box.BorderTopStyle == CssConstants.Hidden) && box.ActualBorderTopWidth > 0)
                {
                    DrawBorder(Border.Top, box, g, rect, isFirst, isLast);
                }
                if (isFirst && !(string.IsNullOrEmpty(box.BorderLeftStyle) || box.BorderLeftStyle == CssConstants.None || box.BorderLeftStyle == CssConstants.Hidden) && box.ActualBorderLeftWidth > 0)
                {
                    DrawBorder(Border.Left, box, g, rect, true, isLast);
                }
                if (!(string.IsNullOrEmpty(box.BorderBottomStyle) || box.BorderBottomStyle == CssConstants.None || box.BorderBottomStyle == CssConstants.Hidden) && box.ActualBorderBottomWidth > 0)
                {
                    DrawBorder(Border.Bottom, box, g, rect, isFirst, isLast);
                }
                if (isLast && !(string.IsNullOrEmpty(box.BorderRightStyle) || box.BorderRightStyle == CssConstants.None || box.BorderRightStyle == CssConstants.Hidden) && box.ActualBorderRightWidth > 0)
                {
                    DrawBorder(Border.Right, box, g, rect, isFirst, true);
                }
            }
        }

        /// <summary>
        /// Draw simple border.
        /// </summary>
        /// <param name="border">Desired border</param>
        /// <param name="g">the device to draw to</param>
        /// <param name="box">Box which the border corresponds</param>
        /// <param name="brush">the brush to use</param>
        /// <param name="rectangle">the bounding rectangle to draw in</param>
        /// <returns>Beveled border path, null if there is no rounded corners</returns>
        public static void DrawBorder(Border border, IGraphics g, CssBox box, Brush brush, RectangleF rectangle)
        {
            SetInOutsetRectanglePoints(border, box, rectangle, true, true);
            g.FillPolygon(brush, _borderPts);
        }


        #region Private methods

        /// <summary>
        /// Draw specific border (top/bottom/left/right) with the box data (style/width/rounded).<br/>
        /// </summary>
        /// <param name="border">desired border to draw</param>
        /// <param name="box">the box to draw its borders, contain the borders data</param>
        /// <param name="g">the device to draw into</param>
        /// <param name="rect">the rectangle the border is enclosing</param>
        /// <param name="isLineStart">Specifies if the border is for a starting line (no bevel on left)</param>
        /// <param name="isLineEnd">Specifies if the border is for an ending line (no bevel on right)</param>
        private static void DrawBorder(Border border, CssBox box, IGraphics g, RectangleF rect, bool isLineStart, bool isLineEnd)
        {
            var style = GetStyle(border, box);
            var color = GetColor(border, box, style);

            var borderPath = GetRoundedBorderPath(border, box, rect);
            if (borderPath != null)
            {
                // rounded border need special path
                var smooth = g.SmoothingMode;
                if (box.HtmlContainer != null && !box.HtmlContainer.AvoidGeometryAntialias && box.IsRounded)
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                var pen = GetPen(style, color, GetWidth(border, box));
                using (borderPath)
                    g.DrawPath(pen, borderPath);

                g.SmoothingMode = smooth;
            }
            else
            {
                // non rounded border
                if( style == CssConstants.Inset || style == CssConstants.Outset )
                {
                    // inset/outset border needs special rectangle
                    SetInOutsetRectanglePoints(border, box, rect, isLineStart, isLineEnd);
                    g.FillPolygon(RenderUtils.GetSolidBrush(color), _borderPts);
                }
                else
                {
                    // solid/dotted/dashed border draw as simple line
                    var pen = GetPen(style, color, GetWidth(border, box));
                    switch (border)
                    {
                        case Border.Top:
                            g.DrawLine(pen, rect.Left, rect.Top + box.ActualBorderTopWidth / 2, rect.Right -1, rect.Top + box.ActualBorderTopWidth / 2);
                            break;
                        case Border.Left:
                            g.DrawLine(pen, rect.Left + box.ActualBorderLeftWidth / 2, rect.Top, rect.Left + box.ActualBorderLeftWidth / 2, rect.Bottom);
                            break;
                        case Border.Bottom:
                            g.DrawLine(pen, rect.Left, rect.Bottom - box.ActualBorderBottomWidth / 2, rect.Right -1, rect.Bottom - box.ActualBorderBottomWidth / 2);
                            break;
                        case Border.Right:
                            g.DrawLine(pen, rect.Right - box.ActualBorderRightWidth / 2, rect.Top, rect.Right - box.ActualBorderRightWidth / 2, rect.Bottom);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set rectangle for inset/outset border as it need diagonal connection to other borders.
        /// </summary>
        /// <param name="border">Desired border</param>
        /// <param name="b">Box which the border corresponds</param>
        /// <param name="r">the rectangle the border is enclosing</param>
        /// <param name="isLineStart">Specifies if the border is for a starting line (no bevel on left)</param>
        /// <param name="isLineEnd">Specifies if the border is for an ending line (no bevel on right)</param>
        /// <returns>Beveled border path, null if there is no rounded corners</returns>
        private static void SetInOutsetRectanglePoints(Border border, CssBox b, RectangleF r, bool isLineStart, bool isLineEnd)
        {
            switch( border )
            {
                case Border.Top:
                    _borderPts[0] = new PointF(r.Left, r.Top);
                    _borderPts[1] = new PointF(r.Right, r.Top);
                    _borderPts[2] = new PointF(r.Right, r.Top + b.ActualBorderTopWidth);
                    _borderPts[3] = new PointF(r.Left, r.Top + b.ActualBorderTopWidth);
                    if( isLineEnd )
                        _borderPts[2].X -= b.ActualBorderRightWidth;
                    if( isLineStart )
                        _borderPts[3].X += b.ActualBorderLeftWidth;
                    break;
                case Border.Right:
                    _borderPts[0] = new PointF(r.Right - b.ActualBorderRightWidth, r.Top + b.ActualBorderTopWidth);
                    _borderPts[1] = new PointF(r.Right, r.Top);
                    _borderPts[2] = new PointF(r.Right, r.Bottom);
                    _borderPts[3] = new PointF(r.Right - b.ActualBorderRightWidth, r.Bottom - b.ActualBorderBottomWidth);
                    break;
                case Border.Bottom:
                    _borderPts[0] = new PointF(r.Left, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[1] = new PointF(r.Right, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[2] = new PointF(r.Right, r.Bottom);
                    _borderPts[3] = new PointF(r.Left, r.Bottom);
                    if( isLineStart )
                        _borderPts[0].X += b.ActualBorderLeftWidth;
                    if( isLineEnd )
                        _borderPts[1].X -= b.ActualBorderRightWidth;
                    break;
                case Border.Left:
                    _borderPts[0] = new PointF(r.Left, r.Top);
                    _borderPts[1] = new PointF(r.Left + b.ActualBorderLeftWidth, r.Top + b.ActualBorderTopWidth);
                    _borderPts[2] = new PointF(r.Left + b.ActualBorderLeftWidth, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[3] = new PointF(r.Left, r.Bottom);
                    break;
            }
        }

        /// <summary>
        /// Makes a border path for rounded borders.<br/>
        /// To support rounded dotted/dashed borders we need to use arc in the border path.<br/>
        /// Return null if the border is not rounded.<br/>
        /// </summary>
        /// <param name="border">Desired border</param>
        /// <param name="b">Box which the border corresponds</param>
        /// <param name="r">the rectangle the border is enclosing</param>
        /// <returns>Beveled border path, null if there is no rounded corners</returns>
        private static GraphicsPath GetRoundedBorderPath(Border border, CssBox b, RectangleF r)
        {
            GraphicsPath path = null;

            switch( border )
            {
                case Border.Top:
                    if( b.ActualCornerNW > 0 || b.ActualCornerNE > 0 )
                    {
                        path = new GraphicsPath();

                        if (b.ActualCornerNW > 0)
                            path.AddArc(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNW * 2, b.ActualCornerNW * 2, 180f, 90f);
                        else
                            path.AddLine(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2, r.Left + b.ActualBorderLeftWidth, r.Top + b.ActualBorderTopWidth / 2);

                        if (b.ActualCornerNE > 0)
                            path.AddArc(r.Right - b.ActualCornerNE * 2 - b.ActualBorderRightWidth / 2, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNE * 2, b.ActualCornerNE * 2, 270f, 90f);
                        else
                            path.AddLine(r.Right - b.ActualCornerNE * 2 - b.ActualBorderRightWidth, r.Top + b.ActualBorderTopWidth / 2, r.Right - b.ActualBorderRightWidth / 2, r.Top + b.ActualBorderTopWidth / 2);
                    }
                    break;
                case Border.Bottom:
                    if (b.ActualCornerSW > 0 || b.ActualCornerSE > 0)
                    {
                        path = new GraphicsPath();

                        if (b.ActualCornerSE > 0)
                            path.AddArc(r.Right - b.ActualCornerNE * 2 - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualCornerSE * 2 - b.ActualBorderBottomWidth / 2, b.ActualCornerSE * 2, b.ActualCornerSE * 2, 0f, 90f);
                        else
                            path.AddLine(r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2, r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2 - .1f);

                        if (b.ActualCornerSW > 0)
                            path.AddArc(r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualCornerSW * 2 - b.ActualBorderBottomWidth / 2, b.ActualCornerSW * 2, b.ActualCornerSW * 2, 90f, 90f);
                        else
                            path.AddLine(r.Left + b.ActualBorderLeftWidth / 2 + .1f, r.Bottom - b.ActualBorderBottomWidth / 2, r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2);
                    }
                    break;
                case Border.Right:
                    if( b.ActualCornerNE > 0 || b.ActualCornerSE > 0 )
                    {
                        path = new GraphicsPath();

                        if (b.ActualCornerNE > 0 && (b.BorderTopStyle == CssConstants.None || b.BorderTopStyle == CssConstants.Hidden))
                            path.AddArc(r.Right - b.ActualCornerNE * 2 - b.ActualBorderRightWidth / 2, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNE * 2, b.ActualCornerNE * 2, 270f, 90f);
                        else
                            path.AddLine(r.Right - b.ActualBorderRightWidth / 2, r.Top + b.ActualCornerNE + b.ActualBorderTopWidth / 2, r.Right - b.ActualBorderRightWidth / 2, r.Top + b.ActualCornerNE + b.ActualBorderTopWidth / 2 + .1f);

                        if (b.ActualCornerSE > 0 && (b.BorderBottomStyle == CssConstants.None || b.BorderBottomStyle == CssConstants.Hidden))
                            path.AddArc(r.Right - b.ActualCornerSE * 2 - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualCornerSE * 2 - b.ActualBorderBottomWidth / 2, b.ActualCornerSE * 2, b.ActualCornerSE * 2, 0f, 90f);
                        else
                            path.AddLine(r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualCornerSE - b.ActualBorderBottomWidth / 2 -.1f, r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualCornerSE - b.ActualBorderBottomWidth / 2);
                    }
                    break;
                case Border.Left:
                    if( b.ActualCornerNW > 0 || b.ActualCornerSW > 0 )
                    {
                        path = new GraphicsPath();

                        if (b.ActualCornerSW > 0 && (b.BorderTopStyle == CssConstants.None || b.BorderTopStyle == CssConstants.Hidden))
                            path.AddArc(r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualCornerSW * 2 - b.ActualBorderBottomWidth / 2, b.ActualCornerSW * 2, b.ActualCornerSW * 2, 90f, 90f);
                        else
                            path.AddLine(r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualCornerSW - b.ActualBorderBottomWidth / 2, r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualCornerSW - b.ActualBorderBottomWidth / 2 -.1f);

                        if (b.ActualCornerNW > 0 && (b.BorderBottomStyle == CssConstants.None || b.BorderBottomStyle == CssConstants.Hidden))
                            path.AddArc(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNW * 2, b.ActualCornerNW * 2, 180f, 90f);
                        else
                            path.AddLine(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualCornerNW + b.ActualBorderTopWidth / 2 + .1f, r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualCornerNW + b.ActualBorderTopWidth / 2);
                    }
                    break;
            }

            return path;
        }

        /// <summary>
        /// Get pen to be used for border draw respecting its style.
        /// </summary>
        private static Pen GetPen(string style, Color color, float width)
        {
            var p = RenderUtils.GetPen(color);
            p.Width = width;
            switch (style)
            {
                case "solid":
                    p.DashStyle = DashStyle.Solid;
                    break;
                case "dotted":
                    p.DashStyle = DashStyle.Dot;

                    break;
                case "dashed":
                    p.DashStyle = DashStyle.Dash;
                    if (p.Width < 2)
                        p.DashPattern = new[] { 4, 4f }; // better looking
                    break;
            }
            return p;
        }

        /// <summary>
        /// Get the border color for the given box border.
        /// </summary>
        private static Color GetColor(Border border, CssBoxProperties box, string style)
        {
            switch (border)
            {
                case Border.Top:
                    return style == CssConstants.Inset ? Darken(box.ActualBorderTopColor) : box.ActualBorderTopColor;
                case Border.Right:
                    return style == CssConstants.Outset ? Darken(box.ActualBorderRightColor) : box.ActualBorderRightColor;
                case Border.Bottom:
                    return style == CssConstants.Outset ? Darken(box.ActualBorderBottomColor) : box.ActualBorderBottomColor;
                case Border.Left:
                    return style == CssConstants.Inset ? Darken(box.ActualBorderLeftColor) : box.ActualBorderLeftColor;
                default:
                    throw new ArgumentOutOfRangeException("border");
            }
        }

        /// <summary>
        /// Get the border width for the given box border.
        /// </summary>
        private static float GetWidth(Border border, CssBoxProperties box)
        {
            switch (border)
            {
                case Border.Top:
                    return box.ActualBorderTopWidth;
                case Border.Right:
                    return box.ActualBorderRightWidth;
                case Border.Bottom:
                    return box.ActualBorderBottomWidth;
                case Border.Left:
                    return box.ActualBorderLeftWidth;
                default:
                    throw new ArgumentOutOfRangeException("border");
            }
        }

        /// <summary>
        /// Get the border style for the given box border.
        /// </summary>
        private static string GetStyle(Border border, CssBoxProperties box)
        {
            switch (border)
            {
                case Border.Top:
                    return box.BorderTopStyle;
                case Border.Right:
                    return box.BorderRightStyle;
                case Border.Bottom:
                    return box.BorderBottomStyle;
                case Border.Left:
                    return box.BorderLeftStyle;
                default:
                    throw new ArgumentOutOfRangeException("border");
            }
        }

        /// <summary>
        /// Makes the specified color darker for inset/outset borders.
        /// </summary>
        private static Color Darken(Color c)
        {
            return Color.FromArgb(c.R / 2, c.G / 2, c.B / 2);
        }

        #endregion
    }
}
