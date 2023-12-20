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

using SkiaSharp;
using System;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for Skia Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// The wrapped SkiaSharp graphics object
        /// </summary>
        private readonly SKCanvas _g;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        /// <summary>
        /// Used to measure and draw strings
        /// </summary>
        private static readonly SKTextAlign _stringFormat;

        #endregion


        static GraphicsAdapter()
        {
            _stringFormat = new SKTextAlign();
            //_stringFormat.Alignment = XStringAlignment.Near;
            //_stringFormat.LineAlignment = XLineAlignment.Near;
            _stringFormat = SKTextAlign.Left;
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the win forms graphics object to use</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(SKCanvas g, bool releaseGraphics = false)
            : base(SkiaSharpAdapter.Instance, new RRect(0, 0, double.MaxValue, double.MaxValue))
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _releaseGraphics = releaseGraphics;
        }

        public override void PopClip()
        {
            _clipStack.Pop();
            _g.Restore();
        }

        public override void PushClip(RRect rect)
        {
            _clipStack.Push(rect);
            _g.Save();
            _g.ClipRect(Utils.Convert(rect));
        }

        public override void PushClipExclude(RRect rect)
        { }

        bool _antiAlias = true;
        public override Object SetAntiAliasSmoothingMode()
        {
            /*
            var prevMode = _;
            _g.SmoothingMode = XSmoothingMode.AntiAlias;

            return prevMode;
            */

            var prevMode = _antiAlias;
            _antiAlias = true;
            return prevMode;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        {
            /*
            if (prevMode != null)
            {
                _g.SmoothingMode = (XSmoothingMode)prevMode;
            }*/
            if (prevMode != null)
            {
                _antiAlias = (bool)prevMode;
            }
        }

        public override RSize MeasureString(string str, RFont font)
        {
            var fontAdapter = (FontAdapter)font;
            var realFont = fontAdapter.Font;
            var p = new SKPaint(realFont);
            var boundingRect = new SKRect();
            var measuredWidth = p.MeasureText(str, ref boundingRect);

            float width;
            if (str == " ")
            {
                width = measuredWidth;
            }
            else
            {
                //The bounding rect width looks a bit odd.  We'll take the midpoint until I can work out why this is.
                width = measuredWidth;// measuredWidth;// (boundingRect.Width + measuredWidth) / 2;
            }

            var height = realFont.Metrics.XMax + realFont.Metrics.XMin;

            if (font.Height < 0)
            {
                var descent = realFont.Metrics.Descent;
                fontAdapter.SetMetrics((int)Math.Round(height, MidpointRounding.AwayFromZero), (int)Math.Round((height - descent + 1f), MidpointRounding.AwayFromZero));
            }

            return new RSize(width, height);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            // there is no need for it - used for text selection
            throw new NotSupportedException();
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            //var xBrush = ((BrushAdapter)_adapter.GetSolidBrush(color)).Brush;
            var skiaFont = ((FontAdapter)font).Font;
            var p = new SKPaint
            {
                IsAntialias = _antiAlias,
                FilterQuality = SKFilterQuality.Medium,
                Color = Utils.Convert(color)
            };
            _g.DrawText(str, (float)point.X, (float)point.Y - skiaFont.Metrics.Ascent, skiaFont, p);
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            return new BrushAdapter(new XTextureBrush(((ImageAdapter)image).Image, Utils.Convert(dstRect), Utils.Convert(translateTransformLocation)));
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            if (_releaseGraphics)
                _g.Dispose();
        }


        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            _g.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, GetPaint(pen));
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            _g.DrawRect((float)x, (float)y, (float)width, (float)height, GetPaint(pen));
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            var untypedBrush = ((BrushAdapter)brush).Brush;
            if (untypedBrush is XTextureBrush textureBrush)
            {
                textureBrush.DrawRectangle(_g, (float)x, (float)y, (float)width, (float)height);
            }
            else if (untypedBrush is SKBrush skBrush)
            {
                var p = skBrush.GetPaint().Clone();
                p.IsAntialias = _antiAlias;
                _g.DrawRect((float)x, (float)y, (float)width, (float)height, p);
            }
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            ((ImageAdapter)image).DrawImage(_g, Utils.Convert(destRect), Utils.Convert(srcRect));
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            ((ImageAdapter)image).DrawImage(_g, Utils.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            _g.DrawPath(((GraphicsPathAdapter)path).GraphicsPath, GetPaint(pen));
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.DrawPath(((GraphicsPathAdapter)path).GraphicsPath, GetPaint(brush));
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            var p = GetPaint(brush);

            if (points != null && points.Length > 0)
            {
                //(XBrush)((BrushAdapter)brush).Brush
                var path = new SKPath();
                path.AddPoly(Utils.Convert(points));
                _g.DrawPath(path, p);
            }
        }

        private SKPaint GetPaint(RBrush brush)
        {
            SKBrush skBrush = (SKBrush)((BrushAdapter)brush).Brush;
            var p = skBrush.GetPaint().Clone();
            p.IsAntialias = _antiAlias;
            return p;
        }

        private SKPaint GetPaint(RPen pen)
        {
            var skPaint =((PenAdapter)pen).Pen.Clone();
            skPaint.IsAntialias = _antiAlias;
            return skPaint;
        }

        #endregion
    }
}