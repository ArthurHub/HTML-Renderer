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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
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
        private static readonly RPoint[] _borderPts = new RPoint[4];

        #endregion


        /// <summary>
        /// Draws all the border of the box with respect to style, width, etc.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="box">the box to draw borders for</param>
        /// <param name="rect">the bounding rectangle to draw in</param>
        /// <param name="isFirst">is it the first rectangle of the element</param>
        /// <param name="isLast">is it the last rectangle of the element</param>
        public static void DrawBoxBorders(RGraphics g, CssBox box, RRect rect, bool isFirst, bool isLast)
        {
            if (rect.Width > 0 && rect.Height > 0)
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
        public static void DrawBorder(Border border, RGraphics g, CssBox box, RBrush brush, RRect rectangle)
        {
            SetInOutsetRectanglePoints(border, box, rectangle, true, true);
            g.DrawPolygon(brush, _borderPts);
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
        private static void DrawBorder(Border border, CssBox box, RGraphics g, RRect rect, bool isLineStart, bool isLineEnd)
        {
            var style = GetStyle(border, box);
            var color = GetColor(border, box, style);

            var borderPath = GetRoundedBorderPath(g, border, box, rect);
            if (borderPath != null)
            {
                // rounded border need special path
                Object prevMode = null;
                if (box.HtmlContainer != null && !box.HtmlContainer.AvoidGeometryAntialias && box.IsRounded)
                    prevMode = g.SetAntiAliasSmoothingMode();

                var pen = GetPen(g, style, color, GetWidth(border, box));
                using (borderPath)
                    g.DrawPath(pen, borderPath);

                g.ReturnPreviousSmoothingMode(prevMode);
            }
            else
            {
                // non rounded border
                if (style == CssConstants.Inset || style == CssConstants.Outset)
                {
                    // inset/outset border needs special rectangle
                    SetInOutsetRectanglePoints(border, box, rect, isLineStart, isLineEnd);
                    g.DrawPolygon(g.GetSolidBrush(color), _borderPts);
                }
                else
                {
                    // solid/dotted/dashed border draw as simple line
                    var pen = GetPen(g, style, color, GetWidth(border, box));
                    switch (border)
                    {
                        case Border.Top:
                            g.DrawLine(pen, Math.Ceiling(rect.Left), rect.Top + box.ActualBorderTopWidth / 2, rect.Right - 1, rect.Top + box.ActualBorderTopWidth / 2);
                            break;
                        case Border.Left:
                            g.DrawLine(pen, rect.Left + box.ActualBorderLeftWidth / 2, Math.Ceiling(rect.Top), rect.Left + box.ActualBorderLeftWidth / 2, Math.Floor(rect.Bottom));
                            break;
                        case Border.Bottom:
                            g.DrawLine(pen, Math.Ceiling(rect.Left), rect.Bottom - box.ActualBorderBottomWidth / 2, rect.Right - 1, rect.Bottom - box.ActualBorderBottomWidth / 2);
                            break;
                        case Border.Right:
                            g.DrawLine(pen, rect.Right - box.ActualBorderRightWidth / 2, Math.Ceiling(rect.Top), rect.Right - box.ActualBorderRightWidth / 2, Math.Floor(rect.Bottom));
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
        private static void SetInOutsetRectanglePoints(Border border, CssBox b, RRect r, bool isLineStart, bool isLineEnd)
        {
            switch (border)
            {
                case Border.Top:
                    _borderPts[0] = new RPoint(r.Left, r.Top);
                    _borderPts[1] = new RPoint(r.Right, r.Top);
                    _borderPts[2] = new RPoint(r.Right, r.Top + b.ActualBorderTopWidth);
                    _borderPts[3] = new RPoint(r.Left, r.Top + b.ActualBorderTopWidth);
                    if (isLineEnd)
                        _borderPts[2].X -= b.ActualBorderRightWidth;
                    if (isLineStart)
                        _borderPts[3].X += b.ActualBorderLeftWidth;
                    break;
                case Border.Right:
                    _borderPts[0] = new RPoint(r.Right - b.ActualBorderRightWidth, r.Top + b.ActualBorderTopWidth);
                    _borderPts[1] = new RPoint(r.Right, r.Top);
                    _borderPts[2] = new RPoint(r.Right, r.Bottom);
                    _borderPts[3] = new RPoint(r.Right - b.ActualBorderRightWidth, r.Bottom - b.ActualBorderBottomWidth);
                    break;
                case Border.Bottom:
                    _borderPts[0] = new RPoint(r.Left, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[1] = new RPoint(r.Right, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[2] = new RPoint(r.Right, r.Bottom);
                    _borderPts[3] = new RPoint(r.Left, r.Bottom);
                    if (isLineStart)
                        _borderPts[0].X += b.ActualBorderLeftWidth;
                    if (isLineEnd)
                        _borderPts[1].X -= b.ActualBorderRightWidth;
                    break;
                case Border.Left:
                    _borderPts[0] = new RPoint(r.Left, r.Top);
                    _borderPts[1] = new RPoint(r.Left + b.ActualBorderLeftWidth, r.Top + b.ActualBorderTopWidth);
                    _borderPts[2] = new RPoint(r.Left + b.ActualBorderLeftWidth, r.Bottom - b.ActualBorderBottomWidth);
                    _borderPts[3] = new RPoint(r.Left, r.Bottom);
                    break;
            }
        }

        /// <summary>
        /// Makes a border path for rounded borders.<br/>
        /// To support rounded dotted/dashed borders we need to use arc in the border path.<br/>
        /// Return null if the border is not rounded.<br/>
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="border">Desired border</param>
        /// <param name="b">Box which the border corresponds</param>
        /// <param name="r">the rectangle the border is enclosing</param>
        /// <returns>Beveled border path, null if there is no rounded corners</returns>
        private static RGraphicsPath GetRoundedBorderPath(RGraphics g, Border border, CssBox b, RRect r)
        {
            RGraphicsPath path = null;
            switch (border)
            {
                case Border.Top:
                    if (b.ActualCornerNw > 0 || b.ActualCornerNe > 0)
                    {
                        path = g.GetGraphicsPath();
                        path.Start(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2 + b.ActualCornerNw);

                        if (b.ActualCornerNw > 0)
                            path.ArcTo(r.Left + b.ActualBorderLeftWidth / 2 + b.ActualCornerNw, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNw, RGraphicsPath.Corner.TopLeft);

                        path.LineTo(r.Right - b.ActualBorderRightWidth / 2 - b.ActualCornerNe, r.Top + b.ActualBorderTopWidth / 2);

                        if (b.ActualCornerNe > 0)
                            path.ArcTo(r.Right - b.ActualBorderRightWidth / 2, r.Top + b.ActualBorderTopWidth / 2 + b.ActualCornerNe, b.ActualCornerNe, RGraphicsPath.Corner.TopRight);
                    }
                    break;
                case Border.Bottom:
                    if (b.ActualCornerSw > 0 || b.ActualCornerSe > 0)
                    {
                        path = g.GetGraphicsPath();
                        path.Start(r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2 - b.ActualCornerSe);

                        if (b.ActualCornerSe > 0)
                            path.ArcTo(r.Right - b.ActualBorderRightWidth / 2 - b.ActualCornerSe, r.Bottom - b.ActualBorderBottomWidth / 2, b.ActualCornerSe, RGraphicsPath.Corner.BottomRight);

                        path.LineTo(r.Left + b.ActualBorderLeftWidth / 2 + b.ActualCornerSw, r.Bottom - b.ActualBorderBottomWidth / 2);

                        if (b.ActualCornerSw > 0)
                            path.ArcTo(r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2 - b.ActualCornerSw, b.ActualCornerSw, RGraphicsPath.Corner.BottomLeft);
                    }
                    break;
                case Border.Right:
                    if (b.ActualCornerNe > 0 || b.ActualCornerSe > 0)
                    {
                        path = g.GetGraphicsPath();

                        bool noTop = b.BorderTopStyle == CssConstants.None || b.BorderTopStyle == CssConstants.Hidden;
                        bool noBottom = b.BorderBottomStyle == CssConstants.None || b.BorderBottomStyle == CssConstants.Hidden;
                        path.Start(r.Right - b.ActualBorderRightWidth / 2 - (noTop ? b.ActualCornerNe : 0), r.Top + b.ActualBorderTopWidth / 2 + (noTop ? 0 : b.ActualCornerNe));

                        if (b.ActualCornerNe > 0 && noTop)
                            path.ArcTo(r.Right - b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2 + b.ActualCornerNe, b.ActualCornerNe, RGraphicsPath.Corner.TopRight);

                        path.LineTo(r.Right - b.ActualBorderRightWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2 - b.ActualCornerSe);

                        if (b.ActualCornerSe > 0 && noBottom)
                            path.ArcTo(r.Right - b.ActualBorderRightWidth / 2 - b.ActualCornerSe, r.Bottom - b.ActualBorderBottomWidth / 2, b.ActualCornerSe, RGraphicsPath.Corner.BottomRight);
                    }
                    break;
                case Border.Left:
                    if (b.ActualCornerNw > 0 || b.ActualCornerSw > 0)
                    {
                        path = g.GetGraphicsPath();

                        bool noTop = b.BorderTopStyle == CssConstants.None || b.BorderTopStyle == CssConstants.Hidden;
                        bool noBottom = b.BorderBottomStyle == CssConstants.None || b.BorderBottomStyle == CssConstants.Hidden;
                        path.Start(r.Left + b.ActualBorderLeftWidth / 2 + (noBottom ? b.ActualCornerSw : 0), r.Bottom - b.ActualBorderBottomWidth / 2 - (noBottom ? 0 : b.ActualCornerSw));

                        if (b.ActualCornerSw > 0 && noBottom)
                            path.ArcTo(r.Left + b.ActualBorderLeftWidth / 2, r.Bottom - b.ActualBorderBottomWidth / 2 - b.ActualCornerSw, b.ActualCornerSw, RGraphicsPath.Corner.BottomLeft);

                        path.LineTo(r.Left + b.ActualBorderLeftWidth / 2, r.Top + b.ActualBorderTopWidth / 2 + b.ActualCornerNw);

                        if (b.ActualCornerNw > 0 && noTop)
                            path.ArcTo(r.Left + b.ActualBorderLeftWidth / 2 + b.ActualCornerNw, r.Top + b.ActualBorderTopWidth / 2, b.ActualCornerNw, RGraphicsPath.Corner.TopLeft);
                    }
                    break;
            }

            return path;
        }

        /// <summary>
        /// Get pen to be used for border draw respecting its style.
        /// </summary>
        private static RPen GetPen(RGraphics g, string style, RColor color, double width)
        {
            var p = g.GetPen(color);
            p.Width = width;
            switch (style)
            {
                case "solid":
                    p.DashStyle = RDashStyle.Solid;
                    break;
                case "dotted":
                    p.DashStyle = RDashStyle.Dot;
                    break;
                case "dashed":
                    p.DashStyle = RDashStyle.Dash;
                    break;
            }
            return p;
        }

        /// <summary>
        /// Get the border color for the given box border.
        /// </summary>
        private static RColor GetColor(Border border, CssBoxProperties box, string style)
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
        private static double GetWidth(Border border, CssBoxProperties box)
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
        private static RColor Darken(RColor c)
        {
            return RColor.FromArgb(c.R / 2, c.G / 2, c.B / 2);
        }

        #endregion
    }
}