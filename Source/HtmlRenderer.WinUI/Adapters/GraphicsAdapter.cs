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
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WinUI.Utilities;
using Windows.Foundation;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D Graphics.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// The Win2D drawing session, null for a measure-only instance.
        /// </summary>
        private readonly CanvasDrawingSession _g;

        /// <summary>
        /// Text layouts and geometries need a device, also when only measuring.
        /// </summary>
        private readonly CanvasDevice _device;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        /// <summary>
        /// The active clip layers, one per <see cref="PushClip"/>/<see cref="PushClipExclude"/>, disposed in reverse order.
        /// </summary>
        private readonly Stack<ClipLayer> _layerStack = new Stack<ClipLayer>();

        /// <summary>
        /// A clip layer and the geometry masking it, D2D reads the mask until the layer is popped.
        /// Both are null on a measure-only instance.
        /// </summary>
        private readonly struct ClipLayer
        {
            public ClipLayer(CanvasActiveLayer layer, CanvasGeometry mask)
            {
                Layer = layer;
                Mask = mask;
            }

            private CanvasActiveLayer Layer { get; }

            private CanvasGeometry Mask { get; }

            public void Dispose()
            {
                if (Layer != null)
                    Layer.Dispose();
                if (Mask != null)
                    Mask.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the Win2D drawing session to use</param>
        /// <param name="initialClip">the initial clip of the graphics</param>
        /// <param name="device">the device the session was opened on</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(CanvasDrawingSession g, RRect initialClip, CanvasDevice device, bool releaseGraphics = false)
            : base(WinUIAdapter.Instance, initialClip)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            ArgChecker.AssertArgNotNull(device, "device");

            _g = g;
            _device = device;
            _releaseGraphics = releaseGraphics;
        }

        /// <summary>
        /// Init a measurement-only instance with no live drawing session.
        /// </summary>
        public GraphicsAdapter(CanvasDevice device)
            : base(WinUIAdapter.Instance, RRect.Empty)
        {
            ArgChecker.AssertArgNotNull(device, "device");

            _g = null;
            _device = device;
            _releaseGraphics = false;
        }

        /// <summary>
        /// Init a measurement-only instance against the adapter's own shared device.
        /// </summary>
        public GraphicsAdapter()
            : this(WinUIAdapter.Instance.Device)
        {
        }

        public override void PopClip()
        {
            _layerStack.Pop().Dispose();
            _clipStack.Pop();
        }

        public override void PushClip(RRect rect)
        {
            _clipStack.Push(rect);
            _layerStack.Push(_g != null
                ? new ClipLayer(_g.CreateLayer(1f, Utils.Convert(rect)), null)
                : new ClipLayer(null, null));
        }

        public override void PushClipExclude(RRect rect)
        {
            var current = _clipStack.Peek();
            _clipStack.Push(current);

            if (_g == null)
            {
                _layerStack.Push(new ClipLayer(null, null));
                return;
            }

            CanvasGeometry combined;
            using (var full = CanvasGeometry.CreateRectangle(_device, Utils.Convert(current)))
            using (var excluded = CanvasGeometry.CreateRectangle(_device, Utils.Convert(rect)))
            {
                combined = full.CombineWith(excluded, Matrix3x2.Identity, CanvasGeometryCombine.Exclude);
            }

            _layerStack.Push(new ClipLayer(_g.CreateLayer(1f, combined), combined));
        }

        public override object SetAntiAliasSmoothingMode()
        {
            if (_g == null)
                return null;

            var prev = _g.Antialiasing;
            _g.Antialiasing = CanvasAntialiasing.Antialiased;
            return prev;
        }

        public override void ReturnPreviousSmoothingMode(object prevMode)
        {
            if (_g != null && prevMode is CanvasAntialiasing prev)
                _g.Antialiasing = prev;
        }

        public override RSize MeasureString(string str, RFont font)
        {
            using (var layout = new CanvasTextLayout(_device, str, ((FontAdapter)font).TextFormat, float.MaxValue, float.MaxValue))
            {
                return new RSize(layout.LayoutBoundsIncludingTrailingWhitespace.Width, font.Height);
            }
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            var fontAdapter = (FontAdapter)font;
            using (var layout = new CanvasTextLayout(_device, str, fontAdapter.TextFormat, float.MaxValue, float.MaxValue))
            {
                if (maxWidth <= 0)
                {
                    charFit = 0;
                    charFitWidth = 0;
                    return;
                }

                var width = layout.LayoutBoundsIncludingTrailingWhitespace.Width;
                if (width <= maxWidth)
                {
                    charFit = str.Length;
                    charFitWidth = width;
                    return;
                }

                bool isInside = layout.HitTest((float)maxWidth, 0f, out CanvasTextLayoutRegion region);
                if (isInside)
                {
                    charFit = region.CharacterIndex;
                    charFitWidth = region.LayoutBounds.X;
                }
                else
                {
                    charFit = str.Length;
                    charFitWidth = width;
                }
            }
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var fontAdapter = (FontAdapter)font;
            var format = fontAdapter.TextFormat;
            CanvasTextFormat rtlFormat = null;

            if (rtl)
            {
                rtlFormat = new CanvasTextFormat
                {
                    FontFamily = format.FontFamily,
                    FontSize = format.FontSize,
                    FontWeight = format.FontWeight,
                    FontStyle = format.FontStyle,
                    WordWrapping = CanvasWordWrapping.NoWrap,
                    Direction = CanvasTextDirection.RightToLeftThenTopToBottom,
                };
                format = rtlFormat;
            }

            try
            {
                using (var layout = new CanvasTextLayout(_device, str, format, float.MaxValue, float.MaxValue))
                {
                    var x = point.X;
                    if (rtl)
                        x += layout.LayoutBounds.Width;

                    _g.DrawTextLayout(layout, new Vector2((float)x, (float)point.Y), Utils.Convert(color));
                }
            }
            finally
            {
                rtlFormat?.Dispose();
            }
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new CanvasImageBrush(_device)
            {
                Image = ((ImageAdapter)image).Bitmap,
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                SourceRectangle = Utils.Convert(dstRect),
                Transform = Matrix3x2.CreateTranslation((float)translateTransformLocation.X, (float)translateTransformLocation.Y),
            };
            return new BrushAdapter(brush);
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter(_device);
        }

        public override void Dispose()
        {
            while (_layerStack.Count > 0)
                _layerStack.Pop().Dispose();

            if (_releaseGraphics)
                _g.Dispose();
        }

        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            x1 = (int)x1;
            x2 = (int)x2;
            y1 = (int)y1;
            y2 = (int)y2;

            var adj = pen.Width;
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

            var penAdapter = (PenAdapter)pen;
            _g.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, penAdapter.Brush, (float)pen.Width, penAdapter.StrokeStyle);
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            var adj = pen.Width;
            if (Math.Abs(adj % 2 - 1) < .1)
            {
                x += .5;
                y += .5;
            }

            var penAdapter = (PenAdapter)pen;
            _g.DrawRectangle(new Rect(x, y, width, height), penAdapter.Brush, (float)pen.Width, penAdapter.StrokeStyle);
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            _g.FillRectangle(new Rect(x, y, width, height), ((BrushAdapter)brush).Brush);
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            _g.DrawImage(((ImageAdapter)image).Bitmap, Utils.ConvertRound(destRect), Utils.Convert(srcRect));
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            _g.DrawImage(((ImageAdapter)image).Bitmap, Utils.ConvertRound(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            var penAdapter = (PenAdapter)pen;
            _g.DrawGeometry(((GraphicsPathAdapter)path).GetClosedGeometry(), penAdapter.Brush, (float)pen.Width, penAdapter.StrokeStyle);
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            _g.FillGeometry(((GraphicsPathAdapter)path).GetClosedGeometry(), ((BrushAdapter)brush).Brush);
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                using (var pathBuilder = new CanvasPathBuilder(_device))
                {
                    pathBuilder.BeginFigure((float)points[0].X, (float)points[0].Y);
                    for (int i = 1; i < points.Length; i++)
                        pathBuilder.AddLine((float)points[i].X, (float)points[i].Y);
                    pathBuilder.EndFigure(CanvasFigureLoop.Closed);

                    using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
                    {
                        _g.FillGeometry(geometry, ((BrushAdapter)brush).Brush);
                    }
                }
            }
        }

        #endregion
    }
}
