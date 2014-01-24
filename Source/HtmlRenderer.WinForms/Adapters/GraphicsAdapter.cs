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
using HtmlRenderer.Core;
using HtmlRenderer.Core.SysEntities;
using HtmlRenderer.Core.Utils;
using HtmlRenderer.WinForms.Utilities;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : IGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// used for <see cref="MeasureString(string,System.Drawing.Font,float,out int,out int)"/> calculation.
        /// </summary>
        private static readonly int[] _charFit = new int[1];

        /// <summary>
        /// used for <see cref="MeasureString(string,System.Drawing.Font,float,out int,out int)"/> calculation.
        /// </summary>
        private static readonly int[] _charFitWidth = new int[1000];

        /// <summary>
        /// Used for GDI+ measure string.
        /// </summary>
        private static readonly CharacterRange[] _characterRanges = new CharacterRange[1];

        /// <summary>
        /// The string format to use for measuring strings for GDI+ text rendering
        /// </summary>
        private static readonly StringFormat _stringFormat;

        /// <summary>
        /// The wrapped WinForms graphics object
        /// </summary>
        private readonly Graphics _g;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool _useGdiPlusTextRendering;

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool _releaseGraphics;

        /// <summary>
        /// the initialized HDC used
        /// </summary>
        private IntPtr _hdc;

        #endregion

        /// <summary>
        /// Init static resources.
        /// </summary>
        static GraphicsAdapter()
        {
            _stringFormat = new StringFormat(StringFormat.GenericDefault);
            _stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces;
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the win forms graphics object to use</param>
        /// <param name="useGdiPlusTextRendering">Use GDI+ text rendering to measure/draw text</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(Graphics g, bool useGdiPlusTextRendering, bool releaseGraphics = false)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            _g = g;
            _useGdiPlusTextRendering = useGdiPlusTextRendering;
            _releaseGraphics = releaseGraphics;
        }

        /// <summary>
        /// Gets the bounding clipping region of this graphics.
        /// </summary>
        /// <returns>The bounding rectangle for the clipping region</returns>
        public RectangleInt GetClip()
        {
            RectangleF clip;
            if( _hdc == IntPtr.Zero )
            {
                clip = _g.ClipBounds;
            }
            else
            {
                Rectangle lprc;
                Win32Utils.GetClipBox(_hdc, out lprc);
                clip = lprc;
            }
            return Utils.Convert(clip);
        }

        /// <summary>
        /// Sets the clipping region of this <see cref="T:System.Drawing.Graphics"/> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <param name="rect"><see cref="T:System.Drawing.RectangleF"/> structure to combine. </param>
        /// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode"/> enumeration that specifies the combining operation to use. </param>
        public void SetClip(RectangleInt rect, CombineMode combineMode = CombineMode.Replace)
        {
            ReleaseHdc();
            _g.SetClip(Utils.Convert(rect), combineMode);
        }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        public SizeInt MeasureString(string str, Font font)
        {
            if( _useGdiPlusTextRendering )
            {
                ReleaseHdc();
                _characterRanges[0] = new CharacterRange(0, str.Length);
                _stringFormat.SetMeasurableCharacterRanges(_characterRanges);
                var size = _g.MeasureCharacterRanges(str, font, RectangleF.Empty, _stringFormat)[0].GetBounds(_g).Size;
                return Utils.Convert(size);
            }
            else
            {
                SetFont(font);

                var size = new Size();
                Win32Utils.GetTextExtentPoint32(_hdc, str, str.Length, ref size);
                return Utils.Convert(size);
            }
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
        public SizeInt MeasureString(string str, Font font, float maxWidth, out int charFit, out int charFitWidth)
        {
            if( _useGdiPlusTextRendering )
            {
                ReleaseHdc();
                throw new NotSupportedException("Char fit string measuring is not supported for GDI+ text rendering");
            }
            else
            {
                SetFont(font);

                var size = new Size();
                Win32Utils.GetTextExtentExPoint(_hdc, str, str.Length, (int)Math.Round(maxWidth), _charFit, _charFitWidth, ref size);
                charFit = _charFit[0];
                charFitWidth = charFit > 0 ? _charFitWidth[charFit - 1] : 0;
                return Utils.Convert(size);
            }
        }

        /// <summary>
        /// Draw the given string using the given font and foreground color at given location.
        /// </summary>
        /// <param name="str">the string to draw</param>
        /// <param name="font">the font to use to draw the string</param>
        /// <param name="color">the text color to set</param>
        /// <param name="point">the location to start string draw (top-left)</param>
        /// <param name="size">used to know the size of the rendered text for transparent text support</param>
        public void DrawString(String str, Font font, ColorInt color, PointInt point, SizeInt size)
        {
            var pointConv = Utils.ConvertRound(point);
            var colorConv = Utils.Convert(color);

            if( _useGdiPlusTextRendering )
            {
                ReleaseHdc();
                var brush = ((BrushAdapter)CacheUtils.GetSolidBrush(color)).Brush;
                _g.DrawString(str, font, brush, point.X - FontsUtils.GetFontLeftPadding(font)*.8f, point.Y);
            }
            else
            {
                if( color.A == 255 )
                {
                    SetFont(font);
                    SetTextColor(colorConv);

                    Win32Utils.TextOut(_hdc, pointConv.X, pointConv.Y, str, str.Length);
                }
                else
                {
                    InitHdc();
                    DrawTransparentText(_hdc, str, font, pointConv, Utils.ConvertRound(size), colorConv);
                }
            }
        }

        /// <summary>
        /// Get color pen.
        /// </summary>
        /// <param name="color">the color to get the pen for</param>
        /// <returns>pen instance</returns>
        public IPen GetPen(ColorInt color)
        {
            return CacheUtils.GetPen(color);
        }

        /// <summary>
        /// Get solid color brush.
        /// </summary>
        /// <param name="color">the color to get the brush for</param>
        /// <returns>solid color brush instance</returns>
        public IBrush GetSolidBrush(ColorInt color)
        {
            return CacheUtils.GetSolidBrush(color);
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
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), angle), true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ReleaseHdc();
            if(_releaseGraphics)
                _g.Dispose();
        }
        

        #region Delegate graphics methods

        /// <summary>
        /// Gets or sets the rendering quality for this <see cref="T:System.Drawing.Graphics"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Drawing.Drawing2D.SmoothingMode"/> values.
        /// </returns>
        public SmoothingMode SmoothingMode
        {
            get
            {
                ReleaseHdc();
                return _g.SmoothingMode;
            }
            set
            {
                ReleaseHdc();
                _g.SmoothingMode = value;
            }
        }

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the line. </param>
        /// <param name="x1">The x-coordinate of the first point. </param><param name="y1">The y-coordinate of the first point. </param>
        /// <param name="x2">The x-coordinate of the second point. </param><param name="y2">The y-coordinate of the second point. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
        {
            ReleaseHdc();
            _g.DrawLine(((PenAdapter)pen).Pen, x1, y1, x2, y2);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A <see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the rectangle. </param><param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param><param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param><param name="width">The width of the rectangle to draw. </param><param name="height">The height of the rectangle to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawRectangle(IPen pen, float x, float y, float width, float height)
        {
            ReleaseHdc();
            _g.DrawRectangle(((PenAdapter)pen).Pen, x, y, width, height);
        }

        public void FillRectangle(IBrush brush, float left, float top, float width, float height)
        {
            ReleaseHdc();
            _g.FillRectangle(((BrushAdapter)brush).Brush, left, top, width, height);
        }

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param>
        /// <param name="destRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception>
        public void DrawImage(IImage image, RectangleInt destRect, RectangleInt srcRect)
        {
            ReleaseHdc();
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), Utils.Convert(srcRect), GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Draws the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param><param name="destRect"><see cref="T:System.Drawing.Rectangle"/> structure that specifies the location and size of the drawn image. </param><exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void DrawImage(IImage image, RectangleInt destRect)
        {
            ReleaseHdc();
            _g.DrawImage(( (ImageAdapter)image ).Image, Utils.Convert(destRect));
        }

        /// <summary>
        /// Draws a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the path. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> to draw. </param><exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void DrawPath(IPen pen, GraphicsPath path)
        {
            _g.DrawPath(((PenAdapter)pen).Pen, path);
        }

        /// <summary>
        /// Fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> that represents the path to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void FillPath(IBrush brush, GraphicsPath path)
        {
            ReleaseHdc();
            _g.FillPath(((BrushAdapter)brush).Brush, path);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.PointF"/> structures.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="points">Array of <see cref="T:System.Drawing.PointF"/> structures that represent the vertices of the polygon to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="points"/> is null.</exception>
        public void FillPolygon(IBrush brush, PointInt[] points)
        {
            if( points != null && points.Length > 0 )
            {
                ReleaseHdc();
                _g.FillPolygon(((BrushAdapter)brush).Brush, Utils.Convert(points));
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Init HDC for the current graphics object to be used to call GDI directly.
        /// </summary>
        private void InitHdc()
        {
            if(_hdc == IntPtr.Zero)
            {
                var clip = _g.Clip.GetHrgn(_g);

                _hdc = _g.GetHdc();
                Win32Utils.SetBkMode(_hdc, 1);

                Win32Utils.SelectClipRgn(_hdc, clip);

                Win32Utils.DeleteObject(clip);
            }
        }

        /// <summary>
        /// Release current HDC to be able to use <see cref="Graphics"/> methods.
        /// </summary>
        private void ReleaseHdc()
        {
            if (_hdc != IntPtr.Zero)
            {
                Win32Utils.SelectClipRgn(_hdc, IntPtr.Zero);
                _g.ReleaseHdc(_hdc);
                _hdc = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Set a resource (e.g. a font) for the specified device context.
        /// WARNING: Calling Font.ToHfont() many times without releasing the font handle crashes the app.
        /// </summary>
        private void SetFont(Font font)
        {
            InitHdc();
            Win32Utils.SelectObject(_hdc, FontsUtils.GetCachedHFont(font));
        }

        /// <summary>
        /// Set the text color of the device context.
        /// </summary>
        private void SetTextColor(Color color)
        {
            InitHdc();
            int rgb = ( color.B & 0xFF ) << 16 | ( color.G & 0xFF ) << 8 | color.R;
            Win32Utils.SetTextColor(_hdc, rgb);
        }

        /// <summary>
        /// Special draw logic to draw transparent text using GDI.<br/>
        /// 1. Create in-memory DC<br/>
        /// 2. Copy background to in-memory DC<br/>
        /// 3. Draw the text to in-memory DC<br/>
        /// 4. Copy the in-memory DC to the proper location with alpha blend<br/>
        /// </summary>
        private static void DrawTransparentText(IntPtr hdc, string str, Font font, Point point, Size size, Color color)
        {
            IntPtr dib;
            var memoryHdc = Win32Utils.CreateMemoryHdc(hdc, size.Width, size.Height, out dib);

            try
            {
                // copy target background to memory HDC so when copied back it will have the proper background
                Win32Utils.BitBlt(memoryHdc, 0, 0, size.Width, size.Height, hdc, point.X, point.Y, Win32Utils.BitBltCopy);

                // Create and select font
                Win32Utils.SelectObject(memoryHdc, FontsUtils.GetCachedHFont(font));
                Win32Utils.SetTextColor(memoryHdc, (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R);

                // Draw text to memory HDC
                Win32Utils.TextOut(memoryHdc, 0, 0, str, str.Length);

                // copy from memory HDC to normal HDC with alpha blend so achieve the transparent text
                Win32Utils.AlphaBlend(hdc, point.X, point.Y, size.Width, size.Height, memoryHdc, 0, 0, size.Width, size.Height, new BlendFunction(color.A));
            }
            finally
            {
                Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);                
            }
        }

        #endregion
    }
}