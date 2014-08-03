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
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Adapter for platform specific graphics rendering object - used to render graphics and text in platform specific context.<br/>
    /// The core HTML Renderer components use this class for rendering logic, extending this
    /// class in different platform: WinForms, WPF, Metro, PDF, etc.
    /// </summary>
    public abstract class RGraphics : IDisposable
    {
        #region Fields/Consts

        /// <summary>
        /// the global adapter
        /// </summary>
        protected readonly RAdapter _adapter;

        /// <summary>
        /// Te clipping bound stack as clips are pushed/poped to/from the graphics
        /// </summary>
        protected readonly Stack<RRect> _clipStack = new Stack<RRect>();

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        protected RGraphics(RAdapter adapter, RRect initialClip)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            _adapter = adapter;
            _clipStack.Push(initialClip);
        }

        /// <summary>
        /// Get color pen.
        /// </summary>
        /// <param name="color">the color to get the pen for</param>
        /// <returns>pen instance</returns>
        public RPen GetPen(RColor color)
        {
            return _adapter.GetPen(color);
        }

        /// <summary>
        /// Get solid color brush.
        /// </summary>
        /// <param name="color">the color to get the brush for</param>
        /// <returns>solid color brush instance</returns>
        public RBrush GetSolidBrush(RColor color)
        {
            return _adapter.GetSolidBrush(color);
        }

        /// <summary>
        /// Get linear gradient color brush from <paramref name="color1"/> to <paramref name="color2"/>.
        /// </summary>
        /// <param name="rect">the rectangle to get the brush for</param>
        /// <param name="color1">the start color of the gradient</param>
        /// <param name="color2">the end color of the gradient</param>
        /// <param name="angle">the angle to move the gradient from start color to end color in the rectangle</param>
        /// <returns>linear gradient color brush instance</returns>
        public RBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return _adapter.GetLinearGradientBrush(rect, color1, color2, angle);
        }

        /// <summary>
        /// Gets a Rectangle structure that bounds the clipping region of this Graphics.
        /// </summary>
        /// <returns>A rectangle structure that represents a bounding rectangle for the clipping region of this Graphics.</returns>
        public RRect GetClip()
        {
            return _clipStack.Peek();
        }

        /// <summary>
        /// Pop the latest clip push.
        /// </summary>
        public abstract void PopClip();

        /// <summary>
        /// Push the clipping region of this Graphics to interception of current clipping rectangle and the given rectangle.
        /// </summary>
        /// <param name="rect">Rectangle to clip to.</param>
        public abstract void PushClip(RRect rect);

        /// <summary>
        /// Push the clipping region of this Graphics to exclude the given rectangle from the current clipping rectangle.
        /// </summary>
        /// <param name="rect">Rectangle to exclude clipping in.</param>
        public abstract void PushClipExclude(RRect rect);

        /// <summary>
        /// Set the graphics smooth mode to use anti-alias.<br/>
        /// Use <see cref="ReturnPreviousSmoothingMode"/> to return back the mode used.
        /// </summary>
        /// <returns>the previous smooth mode before the change</returns>
        public abstract Object SetAntiAliasSmoothingMode();

        /// <summary>
        /// Return to previous smooth mode before anti-alias was set as returned from <see cref="SetAntiAliasSmoothingMode"/>.
        /// </summary>
        /// <param name="prevMode">the previous mode to set</param>
        public abstract void ReturnPreviousSmoothingMode(Object prevMode);

        /// <summary>
        /// Get TextureBrush object that uses the specified image and bounding rectangle.
        /// </summary>
        /// <param name="image">The Image object with which this TextureBrush object fills interiors.</param>
        /// <param name="dstRect">A Rectangle structure that represents the bounding rectangle for this TextureBrush object.</param>
        /// <param name="translateTransformLocation">The dimension by which to translate the transformation</param>
        public abstract RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation);

        /// <summary>
        /// Get GraphicsPath object.
        /// </summary>
        /// <returns>graphics path instance</returns>
        public abstract RGraphicsPath GetGraphicsPath();

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        public abstract RSize MeasureString(string str, RFont font);

        /// <summary>
        /// Measure the width of string under max width restriction calculating the number of characters that can fit and the width those characters take.<br/>
        /// Not relevant for platforms that don't render HTML on UI element.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <param name="maxWidth">the max width to calculate fit characters</param>
        /// <param name="charFit">the number of characters that will fit under <see cref="maxWidth"/> restriction</param>
        /// <param name="charFitWidth">the width that only the characters that fit into max width take</param>
        public abstract void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth);

        /// <summary>
        /// Draw the given string using the given font and foreground color at given location.
        /// </summary>
        /// <param name="str">the string to draw</param>
        /// <param name="font">the font to use to draw the string</param>
        /// <param name="color">the text color to set</param>
        /// <param name="point">the location to start string draw (top-left)</param>
        /// <param name="size">used to know the size of the rendered text for transparent text support</param>
        /// <param name="rtl">is to render the string right-to-left (true - RTL, false - LTR)</param>
        public abstract void DrawString(String str, RFont font, RColor color, RPoint point, RSize size, bool rtl);

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the line. </param>
        /// <param name="x1">The x-coordinate of the first point. </param>
        /// <param name="y1">The y-coordinate of the first point. </param>
        /// <param name="x2">The x-coordinate of the second point. </param>
        /// <param name="y2">The y-coordinate of the second point. </param>
        public abstract void DrawLine(RPen pen, double x1, double y1, double x2, double y2);

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A Pen that determines the color, width, and style of the rectangle. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="width">The width of the rectangle to draw. </param>
        /// <param name="height">The height of the rectangle to draw. </param>
        public abstract void DrawRectangle(RPen pen, double x, double y, double width, double height);

        /// <summary>
        /// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="width">Width of the rectangle to fill. </param>
        /// <param name="height">Height of the rectangle to fill. </param>
        public abstract void DrawRectangle(RBrush brush, double x, double y, double width, double height);

        /// <summary>
        /// Draws the specified portion of the specified <see cref="RImage"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect">Rectangle structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        public abstract void DrawImage(RImage image, RRect destRect, RRect srcRect);

        /// <summary>
        /// Draws the specified Image at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. </param>
        public abstract void DrawImage(RImage image, RRect destRect);

        /// <summary>
        /// Draws a GraphicsPath.
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the path. </param>
        /// <param name="path">GraphicsPath to draw. </param>
        public abstract void DrawPath(RPen pen, RGraphicsPath path);

        /// <summary>
        /// Fills the interior of a GraphicsPath.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="path">GraphicsPath that represents the path to fill. </param>
        public abstract void DrawPath(RBrush brush, RGraphicsPath path);

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by Point structures.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="points">Array of Point structures that represent the vertices of the polygon to fill. </param>
        public abstract void DrawPolygon(RBrush brush, RPoint[] points);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
    }
}