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
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.SkiaSharp.Utilities;

namespace TheArtOfDev.HtmlRenderer.SkiaSharp.Adapters
{
    /// <summary>
    /// Adapter for a SkiaSharp canvas.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        private static readonly SKSamplingOptions ImageSampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

        /// <summary>
        /// The canvas to draw on, null for a measure-only instance.
        /// </summary>
        private readonly SKCanvas _canvas;

        private readonly SKPaint _paint = new SKPaint();

        private bool _antiAlias;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="canvas">the canvas to draw on</param>
        /// <param name="initialClip">the initial clip of the graphics</param>
        public GraphicsAdapter(SKCanvas canvas, RRect initialClip)
            : base(SkiaSharpAdapter.Instance, initialClip)
        {
            ArgChecker.AssertArgNotNull(canvas, "canvas");

            _canvas = canvas;
        }

        /// <summary>
        /// Init a measure-only instance, text measuring needs no canvas.
        /// </summary>
        public GraphicsAdapter()
            : base(SkiaSharpAdapter.Instance, RRect.Empty)
        {
        }

        public override void PopClip()
        {
            _clipStack.Pop();
            _canvas?.Restore();
        }

        public override void PushClip(RRect rect)
        {
            _clipStack.Push(rect);
            if (_canvas != null)
            {
                _canvas.Save();
                _canvas.ClipRect(Utils.Convert(rect));
            }
        }

        public override void PushClipExclude(RRect rect)
        {
            _clipStack.Push(_clipStack.Peek());
            if (_canvas != null)
            {
                _canvas.Save();
                _canvas.ClipRect(Utils.Convert(rect), SKClipOperation.Difference);
            }
        }

        public override object SetAntiAliasSmoothingMode()
        {
            var prevMode = _antiAlias;
            _antiAlias = true;
            return prevMode;
        }

        public override void ReturnPreviousSmoothingMode(object prevMode)
        {
            if (prevMode is bool antiAlias)
                _antiAlias = antiAlias;
        }

        public override RSize MeasureString(string str, RFont font)
        {
            var fontAdapter = (FontAdapter)font;
            return new RSize(fontAdapter.MeasureText(str), fontAdapter.Height);
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = ((FontAdapter)font).BreakText(str, (float)maxWidth, out var width);
            charFitWidth = width;
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var fontAdapter = (FontAdapter)font;
            ResetPaint(true);
            _paint.Color = Utils.Convert(color);
            fontAdapter.DrawText(_canvas, str, (float)point.X, (float)point.Y + fontAdapter.Ascent, _paint, rtl);
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var skImage = ((ImageAdapter)image).Image;
            var translation = SKMatrix.CreateTranslation((float)translateTransformLocation.X, (float)translateTransformLocation.Y);

            // The rectangle is the part of the image to tile with
            // Subset returns null for a rectangle that isn't fully inside the image
            var subset = SKRectI.Round(Utils.Convert(dstRect));
            subset.Intersect(new SKRectI(0, 0, skImage.Width, skImage.Height));
            if (subset.IsEmpty || subset == new SKRectI(0, 0, skImage.Width, skImage.Height))
                return new BrushAdapter(skImage.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, ImageSampling, translation));

            using (var part = skImage.Subset(subset))
            {
                return new BrushAdapter(part.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, ImageSampling, translation));
            }
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            if (_canvas != null)
            {
                while (_clipStack.Count > 1)
                    PopClip();
            }

            _paint.Dispose();
        }

        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            x1 = (int)x1;
            x2 = (int)x2;
            y1 = (int)y1;
            y2 = (int)y2;

            // An odd width centered on a pixel edge would smear over two rows or columns
            var adj = ((PenAdapter)pen).StrokeWidth;
            if (Math.Abs(x1 - x2) < .1 && Math.Abs(adj % 2 - 1) < .1)
            {
                x1 += .5;
                x2 += .5;
            }
            if (Math.Abs(y1 - y2) < .1 && Math.Abs(adj % 2 - 1) < .1)
            {
                y1 += .5;
                y2 += .5;
            }

            ResetPaint(_antiAlias);
            using (((PenAdapter)pen).Apply(_paint))
            {
                _canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, _paint);
            }
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            var adj = ((PenAdapter)pen).StrokeWidth;
            if (Math.Abs(adj % 2 - 1) < .1)
            {
                x += .5;
                y += .5;
            }

            ResetPaint(_antiAlias);
            using (((PenAdapter)pen).Apply(_paint))
            {
                _canvas.DrawRect((float)x, (float)y, (float)width, (float)height, _paint);
            }
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            ResetPaint(_antiAlias);
            ((BrushAdapter)brush).Apply(_paint);
            _canvas.DrawRect((float)x, (float)y, (float)width, (float)height, _paint);
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            ResetPaint(false);
            _canvas.DrawImage(((ImageAdapter)image).Image, Utils.Convert(srcRect), Utils.Convert(destRect), ImageSampling, _paint);
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            ResetPaint(false);
            _canvas.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), ImageSampling, _paint);
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            ResetPaint(_antiAlias);
            using (((PenAdapter)pen).Apply(_paint))
            {
                _canvas.DrawPath(((GraphicsPathAdapter)path).GetPath(), _paint);
            }
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            ResetPaint(_antiAlias);
            ((BrushAdapter)brush).Apply(_paint);
            _canvas.DrawPath(((GraphicsPathAdapter)path).GetPath(), _paint);
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points == null || points.Length == 0)
                return;

            var skPoints = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                skPoints[i] = Utils.Convert(points[i]);

            using (var builder = new SKPathBuilder())
            {
                builder.AddPoly(skPoints, true);
                using (var path = builder.Detach())
                {
                    ResetPaint(_antiAlias);
                    ((BrushAdapter)brush).Apply(_paint);
                    _canvas.DrawPath(path, _paint);
                }
            }
        }

        #endregion

        private void ResetPaint(bool antiAlias)
        {
            _paint.Reset();
            _paint.IsAntialias = antiAlias;
        }
    }
}
