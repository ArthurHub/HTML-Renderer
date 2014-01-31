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
using System.Drawing;
using System.Drawing.Drawing2D;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.Entities;
using HtmlRenderer.Interfaces;
using HtmlRenderer.PdfSharp.Utilities;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : IGraphics
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

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the win forms graphics object to use</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(XGraphics g, bool releaseGraphics = false)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _releaseGraphics = releaseGraphics;
        }

        /// <summary>
        /// Gets the bounding clipping region of this graphics.
        /// </summary>
        /// <returns>The bounding rectangle for the clipping region</returns>
        public RectangleInt GetClip()
        {
            RectangleF clip = _g.Graphics.ClipBounds;
            return Utils.Convert(clip);
        }

        /// <summary>
        /// Sets the clipping region of this Graphics to the result of the specified operation combining the current clip region and the rectangle specified by a Rectangle structure.
        /// </summary>
        /// <param name="rect">Rectangle structure to combine.</param>
        public void SetClipReplace(RectangleInt rect)
        {
            _g.Graphics.SetClip(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), CombineMode.Replace);
        }

        /// <summary>
        /// Sets the clipping region of this Graphics to the result of the specified operation combining the current clip region and the rectangle specified by a Rectangle structure.
        /// </summary>
        /// <param name="rect">Rectangle structure to combine.</param>
        public void SetClipExclude(RectangleInt rect)
        {
            _g.Graphics.SetClip(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), CombineMode.Exclude);
        }

        /// <summary>
        /// Set the graphics smooth mode to use anti-alias.<br/>
        /// Use <see cref="ReturnPreviousSmoothingMode"/> to return back the mode used.
        /// </summary>
        /// <returns>the previous smooth mode before the change</returns>
        public Object SetAntiAliasSmoothingMode()
        {
            var prevMode = _g.SmoothingMode;
            _g.SmoothingMode = XSmoothingMode.AntiAlias;
            return prevMode;
        }

        /// <summary>
        /// Return to previous smooth mode before anti-alias was set as returned from <see cref="SetAntiAliasSmoothingMode"/>.
        /// </summary>
        /// <param name="prevMode">the previous mode to set</param>
        public void ReturnPreviousSmoothingMode(Object prevMode)
        {
            if (prevMode != null)
            {
                _g.SmoothingMode = (XSmoothingMode)prevMode;
            }
        }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        public SizeInt MeasureString(string str, IFont font)
        {
                var fontAdapter = (FontAdapter)font;
                var realFont = fontAdapter.Font;
                var size = _g.MeasureString(str, realFont);

                if (font.Height < 0)
                {
                    var height = realFont.Height;
                    var descent = realFont.Size * realFont.FontFamily.GetCellDescent(realFont.Style) / realFont.FontFamily.GetEmHeight(realFont.Style);
                    fontAdapter.SetMetrics(height, (int)Math.Round(( height - descent + .5f )));
                }

                return Utils.Convert(size);
            
        }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.<br/>
        /// Restrict the width of the string and get the number of characters able to fit in the restriction and
        /// the width those characters take.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <param name="maxWidth">the max width to render the string in</param>
        /// <param name="charFit">the number of characters that will fit under <see cref="maxWidth"/> restriction</param>
        /// <param name="charFitWidth"></param>
        /// <returns>the size of the string</returns>
        public SizeInt MeasureString(string str, IFont font, float maxWidth, out int charFit, out int charFitWidth)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Draw the given string using the given font and foreground color at given location.
        /// </summary>
        /// <param name="str">the string to draw</param>
        /// <param name="font">the font to use to draw the string</param>
        /// <param name="color">the text color to set</param>
        /// <param name="point">the location to start string draw (top-left)</param>
        /// <param name="size">used to know the size of the rendered text for transparent text support</param>
        public void DrawString(String str, IFont font, ColorInt color, PointInt point, SizeInt size)
        {
            var brush = new XSolidBrush(Utils.Convert(color));
            _g.DrawString(str, ( (FontAdapter)font ).Font, brush, point.X - font.LeftPadding*.8f, point.Y);
        }

        /// <summary>
        /// Get color pen.
        /// </summary>
        /// <param name="color">the color to get the pen for</param>
        /// <returns>pen instance</returns>
        public IPen GetPen(ColorInt color)
        {
            return new PenAdapter(new XPen(Utils.Convert(color)));
        }

        /// <summary>
        /// Get solid color brush.
        /// </summary>
        /// <param name="color">the color to get the brush for</param>
        /// <returns>solid color brush instance</returns>
        public IBrush GetSolidBrush(ColorInt color)
        {
            return new BrushAdapter(new XSolidBrush(Utils.Convert(color)));
        }

        /// <summary>
        /// Get linear gradient color brush from <paramref name="color1"/> to <paramref name="color2"/>.
        /// </summary>
        /// <param name="rect">the rectangle to get the brush for</param>
        /// <param name="color1">the start color of the gradient</param>
        /// <param name="color2">the end color of the gradient</param>
        /// <param name="angle">the angle to move the gradient from start color to end color in the rectangle</param>
        /// <returns>linear gradient color brush instance</returns>
        public IBrush GetLinearGradientBrush(RectangleInt rect, ColorInt color1, ColorInt color2, float angle)
        {
            XLinearGradientMode mode;
            if(angle < 45)
                mode = XLinearGradientMode.ForwardDiagonal;
            else if (angle < 90)
                mode = XLinearGradientMode.Vertical;
            else if (angle < 135)
                mode = XLinearGradientMode.BackwardDiagonal;
            else
                mode = XLinearGradientMode.Horizontal;
            return new BrushAdapter(new XLinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), mode));
        }

        /// <summary>
        /// Get TextureBrush object that uses the specified image and bounding rectangle.
        /// </summary>
        /// <param name="image">The Image object with which this TextureBrush object fills interiors.</param>
        /// <param name="dstRect">A Rectangle structure that represents the bounding rectangle for this TextureBrush object.</param>
        /// <param name="translateTransformLocation">The dimension by which to translate the transformation</param>
        public IBrush GetTextureBrush(IImage image, RectangleInt dstRect, PointInt translateTransformLocation)
        {
            // atodo: handle missing TextureBrush
//            var brush = new TextureBrush(((ImageAdapter)image).Image, Utils.Convert(dstRect));
//            brush.TranslateTransform(translateTransformLocation.X, translateTransformLocation.Y);
//            return new BrushAdapter(brush, true);
            return new BrushAdapter(new XSolidBrush(XColors.DeepPink));
        }

        /// <summary>
        /// Get GraphicsPath object.
        /// </summary>
        /// <returns>graphics path instance</returns>
        public IGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_releaseGraphics)
                _g.Dispose();
        }
        

        #region Delegate graphics methods

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the line. </param>
        /// <param name="x1">The x-coordinate of the first point. </param><param name="y1">The y-coordinate of the first point. </param>
        /// <param name="x2">The x-coordinate of the second point. </param><param name="y2">The y-coordinate of the second point. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
        {
            _g.DrawLine(((PenAdapter)pen).Pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A Pen that determines the color, width, and style of the rectangle. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="width">The width of the rectangle to draw. </param>
        /// <param name="height">The height of the rectangle to draw. </param>
        public void DrawRectangle(IPen pen, float x, float y, float width, float height)
        {
            _g.DrawRectangle(((PenAdapter)pen).Pen, x, y, width, height);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="width">Width of the rectangle to fill. </param>
        /// <param name="height">Height of the rectangle to fill. </param>
        public void FillRectangle(IBrush brush, float x, float y, float width, float height)
        {
            _g.DrawRectangle(((BrushAdapter)brush).Brush, x, y, width, height);
        }

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect">Rectangle structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        public void DrawImage(IImage image, RectangleInt destRect, RectangleInt srcRect)
        {
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), Utils.Convert(srcRect), XGraphicsUnit.Point);
        }

        /// <summary>
        /// Draws the specified Image at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. </param>
        public void DrawImage(IImage image, RectangleInt destRect)
        {
            _g.DrawImage(( (ImageAdapter)image ).Image, Utils.Convert(destRect));
        }

        /// <summary>
        /// Draws a GraphicsPath.
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the path. </param>
        /// <param name="path">GraphicsPath to draw. </param>
        public void DrawPath(IPen pen, IGraphicsPath path)
        {
            _g.DrawPath(((PenAdapter)pen).Pen, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        /// <summary>
        /// Fills the interior of a GraphicsPath.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="path">GraphicsPath that represents the path to fill. </param>
        public void FillPath(IBrush brush, IGraphicsPath path)
        {
            _g.DrawPath(((BrushAdapter)brush).Brush, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by Point structures.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="points">Array of Point structures that represent the vertices of the polygon to fill. </param>
        public void FillPolygon(IBrush brush, PointInt[] points)
        {
            if( points != null && points.Length > 0 )
            {
                _g.DrawPolygon(( (BrushAdapter)brush ).Brush, Utils.Convert(points), XFillMode.Winding);
            }
        }

        #endregion
    }
}