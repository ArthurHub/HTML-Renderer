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

using PdfSharp.Drawing;
using System;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.PdfSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Adapters
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
        private readonly XGraphics _g;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        /// <summary>
        /// Used to measure and draw strings
        /// </summary>
        private static readonly XStringFormat _stringFormat;

        #endregion


        static GraphicsAdapter()
        {
            _stringFormat = new XStringFormat();
            _stringFormat.Alignment = XStringAlignment.Near;
            _stringFormat.LineAlignment = XLineAlignment.Near;
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the win forms graphics object to use</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(XGraphics g, bool releaseGraphics = false)
            : base(PdfSharpAdapter.Instance, new RRect(0, 0, double.MaxValue, double.MaxValue))
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
            _g.IntersectClip(Utils.Convert(rect));
        }

        public override void PushClipExclude(RRect rect)
        { }

        public override Object SetAntiAliasSmoothingMode()
        {
            var prevMode = _g.SmoothingMode;
            _g.SmoothingMode = XSmoothingMode.AntiAlias;
            return prevMode;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        {
            if (prevMode != null)
            {
                _g.SmoothingMode = (XSmoothingMode)prevMode;
            }
        }

        public override RSize MeasureString(string str, RFont font)
        {
            var fontAdapter = (FontAdapter)font;
            var realFont = fontAdapter.Font;
            var size = _g.MeasureString(str, realFont, _stringFormat);

            if (font.Height < 0)
            {
                var height = realFont.Height;
                var descent = realFont.Size * realFont.FontFamily.GetCellDescent(realFont.Style) / realFont.FontFamily.GetEmHeight(realFont.Style);
                fontAdapter.SetMetrics(height, (int)Math.Round((height - descent + 1f)));
            }

            return Utils.Convert(size);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            // there is no need for it - used for text selection
            throw new NotSupportedException();
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var xBrush = ((BrushAdapter)_adapter.GetSolidBrush(color)).Brush;
            _g.DrawString(str, ((FontAdapter)font).Font, (XBrush)xBrush, point.X, point.Y, _stringFormat);
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
            _g.DrawLine(((PenAdapter)pen).Pen, x1, y1, x2, y2);
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            _g.DrawRectangle(((PenAdapter)pen).Pen, x, y, width, height);
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            var xBrush = ((BrushAdapter)brush).Brush;
            var xTextureBrush = xBrush as XTextureBrush;
            if (xTextureBrush != null)
            {
                xTextureBrush.DrawRectangle(_g, x, y, width, height);
            }
            else
            {
                _g.DrawRectangle((XBrush)xBrush, x, y, width, height);

                // handle bug in PdfSharp that keeps the brush color for next string draw
                if (xBrush is XLinearGradientBrush)
                    _g.DrawRectangle(XBrushes.White, 0, 0, 0.1, 0.1);
            }
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), Utils.Convert(srcRect), XGraphicsUnit.Point);
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            _g.DrawPath(((PenAdapter)pen).Pen, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.DrawPath((XBrush)((BrushAdapter)brush).Brush, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                _g.DrawPolygon((XBrush)((BrushAdapter)brush).Brush, Utils.Convert(points), XFillMode.Winding);
            }
        }

        #endregion
    }
}