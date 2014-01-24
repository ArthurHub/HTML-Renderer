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
using HtmlRenderer.Core.SysEntities;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// Interface for the graphics methods required for HTML rendering.<br/>
    /// The core HTML Renderer components use this interface for rendering logic, implementing this
    /// interface in different platform: WinForms, WPF, Silver-light, Mono, PdfSharp, etc.
    /// </summary>
    public interface IGraphics : IDisposable
    {
        /// <summary>
        /// Gets a <see cref="T:System.Drawing.RectangleF"/> structure that bounds the clipping region of this <see cref="T:System.Drawing.Graphics"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Drawing.RectangleF"/> structure that represents a bounding rectangle for the clipping region of this <see cref="T:System.Drawing.Graphics"/>.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        RectangleInt GetClip();

        /// <summary>
        /// Gets or sets the rendering quality for this <see cref="T:System.Drawing.Graphics"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Drawing.Drawing2D.SmoothingMode"/> values.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        SizeInt MeasureString(string str, Font font);

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
        SizeInt MeasureString(string str, Font font, float maxWidth, out int charFit, out int charFitWidth);

        /// <summary>
        /// Draw the given string using the given font and foreground color at given location.
        /// </summary>
        /// <param name="str">the string to draw</param>
        /// <param name="font">the font to use to draw the string</param>
        /// <param name="color">the text color to set</param>
        /// <param name="point">the location to start string draw (top-left)</param>
        /// <param name="size">used to know the size of the rendered text for transparent text support</param>
        void DrawString(String str, Font font, ColorInt color, PointInt point, SizeInt size);

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the line. </param><param name="x1">The x-coordinate of the first point. </param><param name="y1">The y-coordinate of the first point. </param><param name="x2">The x-coordinate of the second point. </param><param name="y2">The y-coordinate of the second point. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        void DrawLine(IPen pen, float x1, float y1, float x2, float y2);

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A <see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the rectangle. </param><param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param><param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param><param name="width">The width of the rectangle to draw. </param><param name="height">The height of the rectangle to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        void DrawRectangle(IPen pen, float x, float y, float width, float height);

        void FillRectangle(IBrush getSolidBrush, float left, float top, float width, float height);

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param>
        /// <param name="destRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception>
        void DrawImage(IImage image, RectangleInt destRect, RectangleInt srcRect);

        /// <summary>
        /// Draws the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param><param name="destRect"><see cref="T:System.Drawing.Rectangle"/> structure that specifies the location and size of the drawn image. </param><exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        void DrawImage(IImage image, RectangleInt destRect);

        /// <summary>
        /// Fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> that represents the path to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        void FillPath(IBrush brush, GraphicsPath path);

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.PointF"/> structures.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="points">Array of <see cref="T:System.Drawing.PointF"/> structures that represent the vertices of the polygon to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="points"/> is null.</exception>
        void FillPolygon(IBrush brush, PointInt[] points);

        /// <summary>
        /// Sets the clipping region of this <see cref="T:System.Drawing.Graphics"/> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <param name="rect"><see cref="T:System.Drawing.RectangleF"/> structure to combine. </param><param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode"/> enumeration that specifies the combining operation to use. </param><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        void SetClip(RectangleInt rect, CombineMode combineMode = CombineMode.Replace);

        /// <summary>
        /// Draws a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the path. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        void DrawPath(IPen pen, GraphicsPath path);

        /// <summary>
        /// Get color pen.
        /// </summary>
        /// <param name="color">the color to get the pen for</param>
        /// <returns>pen instance</returns>
        IPen GetPen(ColorInt color);

        /// <summary>
        /// Get solid color brush.
        /// </summary>
        /// <param name="color">the color to get the brush for</param>
        /// <returns>solid color brush instance</returns>
        IBrush GetSolidBrush(ColorInt color);

        /// <summary>
        /// Get linear gradient color brush from <paramref name="color1"/> to <paramref name="color2"/>.
        /// </summary>
        /// <param name="rect">the rectangle to get the brush for</param>
        /// <param name="color1">the start color of the gradient</param>
        /// <param name="color2">the end color of the gradient</param>
        /// <param name="angle">the angle to move the gradient from start color to end color in the rectangle</param>
        /// <returns>linear gradient color brush instance</returns>
        IBrush GetLinearGradientBrush(RectangleInt rect, ColorInt color1, ColorInt color2, float angle);

        /// <summary>
        /// Get TextureBrush object that uses the specified image and bounding rectangle.
        /// </summary>
        /// <param name="image">The Image object with which this TextureBrush object fills interiors.</param>
        /// <param name="dstRect">A Rectangle structure that represents the bounding rectangle for this TextureBrush object.</param>
        /// <param name="translateTransformLocation">The dimension by which to translate the transformation</param>
        IBrush GetTextureBrush(IImage image, RectangleInt dstRect, PointInt translateTransformLocation);
    }
}
