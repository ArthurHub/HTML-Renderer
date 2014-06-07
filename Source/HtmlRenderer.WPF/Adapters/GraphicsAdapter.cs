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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HtmlRenderer.Adapters;
using HtmlRenderer.Adapters.Entities;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WPF.Utilities;

namespace HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WinForms Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// The wrapped WinForms graphics object
        /// </summary>
        private readonly DrawingContext _g;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the WPF graphics object to use</param>
        /// <param name="initialClip">the initial clip of the graphics</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(DrawingContext g, RRect initialClip, bool releaseGraphics = false)
            : base(WpfAdapter.Instance, initialClip)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _releaseGraphics = releaseGraphics;
        }

        public override RBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(color1), Utils.Convert(color2), angle));
        }

        public override void PopClip()
        {
            _g.Pop();
            _clipStack.Pop();
        }

        public override void PushClip(RRect rect)
        {
            _clipStack.Push(rect);
            _g.PushClip(new RectangleGeometry(Utils.Convert(rect)));
        }

        public override void PushClipExclude(RRect rect)
        {
            var geometry = new CombinedGeometry();
            geometry.Geometry1 = new RectangleGeometry(Utils.Convert(_clipStack.Peek()));
            geometry.Geometry2 = new RectangleGeometry(Utils.Convert(rect));
            geometry.GeometryCombineMode = GeometryCombineMode.Exclude;

            _clipStack.Push(_clipStack.Peek());
            _g.PushClip(geometry);
        }

        public override Object SetAntiAliasSmoothingMode()
        {
            return null;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        { }

        public override RSize MeasureString(string str, RFont font)
        {
            double width = 0;
            GlyphTypeface glyphTypeface = ((FontAdapter)font).GlyphTypeface;
            if (glyphTypeface != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(str[i]))
                    {
                        ushort glyph = glyphTypeface.CharacterToGlyphMap[str[i]];
                        double advanceWidth = glyphTypeface.AdvanceWidths[glyph];
                        width += advanceWidth;
                    }
                    else
                    {
                        width = 0;
                        break;
                    }
                }
            }

            if (width <= 0)
            {
                var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, Brushes.Red);
                return new RSize(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
            }

            return new RSize(width * font.Size * 96d / 72d, font.Height);
        }

        public override RSize MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            double width = 0;
            charFit = -1;
            charFitWidth = -1;
            GlyphTypeface glyphTypeface = ((FontAdapter)font).GlyphTypeface;
            if (glyphTypeface != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(str[i]))
                    {
                        ushort glyph = glyphTypeface.CharacterToGlyphMap[str[i]];
                        double advanceWidth = glyphTypeface.AdvanceWidths[glyph];

                        if (charFit == -1 && !(width + advanceWidth < maxWidth))
                        {
                            charFit = i;
                            charFitWidth = width;
                        }
                        width += advanceWidth;
                    }
                    else
                    {
                        width = 0;
                        break;
                    }
                }
            }

            if (width <= 0)
            {
                var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, Brushes.Red);
                charFit = str.Length;
                charFitWidth = formattedText.WidthIncludingTrailingWhitespace;
                return new RSize(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
            }

            return new RSize(width * font.Size * 96d / 72d, font.Height);
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var colorConv = ((BrushAdapter)_adapter.GetSolidBrush(color)).Brush;

            //return;

            bool glyphRendered = false;
            GlyphTypeface glyphTypeface = ((FontAdapter)font).GlyphTypeface;
            if (glyphTypeface != null)
            {
                double width = 0;
                ushort[] glyphs = new ushort[str.Length];
                double[] widths = new double[str.Length];

                int i = 0;
                for (; i < str.Length; i++)
                {
                    ushort glyph;
                    if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(str[i], out glyph))
                        break;

                    glyphs[i] = glyph;
                    width += glyphTypeface.AdvanceWidths[glyph];
                    widths[i] = 96d / 72d * font.Size * glyphTypeface.AdvanceWidths[glyph];
                }

                if (i >= str.Length)
                {
                    point.Y += glyphTypeface.Baseline * font.Size * 96d / 72d;
                    point.X += rtl ? 96d / 72d * font.Size * width : 0;

                    glyphRendered = true;
                    var glyphRun = new GlyphRun(glyphTypeface, rtl ? 1 : 0, false, 96d / 72d * font.Size, glyphs, Utils.ConvertRound(point), widths, null, null, null, null, null, null);
                    _g.DrawGlyphRun(colorConv, glyphRun);
                }
            }

            if (!glyphRendered)
            {
                var formattedText = new FormattedText(str, CultureInfo.CurrentCulture, rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight, ((FontAdapter)font).Font, 96d / 72d * font.Size, colorConv);
                point.X += rtl ? formattedText.Width : 0;
                _g.DrawText(formattedText, Utils.ConvertRound(point));
            }
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new ImageBrush(((ImageAdapter)image).Image);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.Tile;
            brush.Viewport = Utils.Convert(dstRect);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Transform = new TranslateTransform(translateTransformLocation.X, translateTransformLocation.Y);
            brush.Freeze();
            return new BrushAdapter(brush);
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            if (_releaseGraphics)
                _g.Close();
        }


        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            _g.DrawLine(((PenAdapter)pen).CreatePen(), new Point(x1, y1), new Point(x2, y2));
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            var pen2 = ((PenAdapter)pen).CreatePen();
            _g.DrawRectangle(null, pen2, new Rect(x, y, width, height));
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            var brush2 = ((BrushAdapter)brush).Brush;
            _g.DrawRectangle(brush2, null, new Rect(x, y, width, height));
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            CroppedBitmap croppedImage = new CroppedBitmap(((ImageAdapter)image).Image, new Int32Rect((int)srcRect.X, (int)srcRect.Y, (int)srcRect.Width, (int)srcRect.Height));
            _g.DrawImage(croppedImage, Utils.Convert(destRect));
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            _g.DrawGeometry(null, ((PenAdapter)pen).CreatePen(), ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.DrawGeometry(((BrushAdapter)brush).Brush, null, ((GraphicsPathAdapter)path).GetClosedGeometry());
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                var g = new StreamGeometry();
                using (var context = g.Open())
                {
                    context.BeginFigure(Utils.ConvertRound(points[0]), true, true);
                    for (int i = 1; i < points.Length; i++)
                        context.LineTo(Utils.ConvertRound(points[i]), false, true);
                }
                g.Freeze();

                _g.DrawGeometry(((BrushAdapter)brush).Brush, null, g);
            }
        }

        #endregion
    }
}