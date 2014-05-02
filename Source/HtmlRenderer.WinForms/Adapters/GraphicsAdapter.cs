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
        /// used for <see cref="MeasureString(string,IFont,double,out int,out int)"/> calculation.
        /// </summary>
        private static readonly int[] _charFit = new int[1];

        /// <summary>
        /// used for <see cref="MeasureString(string,IFont,double,out int,out int)"/> calculation.
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
        /// The string format to use for rendering strings for GDI+ text rendering
        /// </summary>
        private static readonly StringFormat _stringFormat2;

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

        /// <summary>
        /// If text alignment was set to RTL
        /// </summary>
        private bool _setRtl;

        #endregion


        /// <summary>
        /// Init static resources.
        /// </summary>
        static GraphicsAdapter()
        {
            _stringFormat = new StringFormat(StringFormat.GenericTypographic);
            _stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces;

            _stringFormat2 = new StringFormat(StringFormat.GenericTypographic);
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
        public RRect GetClip()
        {
            RectangleF clip;
            if (_hdc == IntPtr.Zero)
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
        /// Sets the clipping region of this Graphics to the result of the specified operation combining the current clip region and the rectangle specified by a Rectangle structure.
        /// </summary>
        /// <param name="rect">Rectangle structure to combine.</param>
        public void SetClipReplace(RRect rect)
        {
            ReleaseHdc();
            _g.SetClip(Utils.Convert(rect), CombineMode.Replace);
        }

        /// <summary>
        /// Sets the clipping region of this Graphics to the result of the specified operation combining the current clip region and the rectangle specified by a Rectangle structure.
        /// </summary>
        /// <param name="rect">Rectangle structure to combine.</param>
        public void SetClipExclude(RRect rect)
        {
            ReleaseHdc();
            _g.SetClip(Utils.Convert(rect), CombineMode.Exclude);
        }

        /// <summary>
        /// Set the graphics smooth mode to use anti-alias.<br/>
        /// Use <see cref="ReturnPreviousSmoothingMode"/> to return back the mode used.
        /// </summary>
        /// <returns>the previous smooth mode before the change</returns>
        public Object SetAntiAliasSmoothingMode()
        {
            ReleaseHdc();
            var prevMode = _g.SmoothingMode;
            _g.SmoothingMode = SmoothingMode.AntiAlias;
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
                ReleaseHdc();
                _g.SmoothingMode = (SmoothingMode)prevMode;
            }
        }

        /// <summary>
        /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC
        /// using the given font <paramref name="font"/>.
        /// </summary>
        /// <param name="str">the string to measure</param>
        /// <param name="font">the font to measure string with</param>
        /// <returns>the size of the string</returns>
        public RSize MeasureString(string str, IFont font)
        {
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                var fontAdapter = (FontAdapter)font;
                var realFont = fontAdapter.Font;
                _characterRanges[0] = new CharacterRange(0, str.Length);
                _stringFormat.SetMeasurableCharacterRanges(_characterRanges);
                var size = _g.MeasureCharacterRanges(str, realFont, RectangleF.Empty, _stringFormat)[0].GetBounds(_g).Size;

                if (font.Height < 0)
                {
                    var height = realFont.Height;
                    var descent = realFont.Size * realFont.FontFamily.GetCellDescent(realFont.Style) / realFont.FontFamily.GetEmHeight(realFont.Style);
                    fontAdapter.SetMetrics(height, (int)Math.Round((height - descent + .5f)));
                }

                return Utils.Convert(size);
            }
            else
            {
                SetFont(font);
                var size = new Size();
                Win32Utils.GetTextExtentPoint32(_hdc, str, str.Length, ref size);

                if (font.Height < 0)
                {
                    TextMetric lptm;
                    Win32Utils.GetTextMetrics(_hdc, out lptm);
                    ((FontAdapter)font).SetMetrics(size.Height, lptm.tmHeight - lptm.tmDescent + lptm.tmUnderlined + 1);
                }

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
        /// <param name="charFitWidth">the width that only the fitted characters take</param>
        /// <returns>the size of the string</returns>
        public RSize MeasureString(string str, IFont font, double maxWidth, out int charFit, out int charFitWidth)
        {
            charFit = 0;
            charFitWidth = 0;
            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();

                var size = MeasureString(str, font);

                for (int i = 1; i <= str.Length; i++)
                {
                    charFit = i - 1;
                    RSize pSize = MeasureString(str.Substring(0, i), font);
                    if (pSize.Height <= size.Height && pSize.Width < maxWidth)
                        charFitWidth = (int)pSize.Width;
                    else
                        break;
                }

                return size;
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
        /// <param name="rtl">is to render the string right-to-left (true - RTL, false - LTR)</param>
        public void DrawString(string str, IFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            var pointConv = Utils.ConvertRound(point);
            var colorConv = Utils.Convert(color);

            if (_useGdiPlusTextRendering)
            {
                ReleaseHdc();
                SetRtlAlign(rtl);
                var brush = ((BrushAdapter)CacheUtils.GetSolidBrush(color)).Brush;
                _g.DrawString(str, ((FontAdapter)font).Font, brush, (int)(Math.Round(point.X) + (rtl ? size.Width : 0)), (int)Math.Round(point.Y), _stringFormat2);
            }
            else
            {
                if (color.A == 255)
                {
                    SetFont(font);
                    SetTextColor(colorConv);
                    SetRtlAlign(rtl);

                    Win32Utils.TextOut(_hdc, pointConv.X, pointConv.Y, str, str.Length);
                }
                else
                {
                    InitHdc();
                    SetRtlAlign(rtl);
                    DrawTransparentText(_hdc, str, font, pointConv, Utils.ConvertRound(size), colorConv);
                }
            }
        }

        /// <summary>
        /// Get color pen.
        /// </summary>
        /// <param name="color">the color to get the pen for</param>
        /// <returns>pen instance</returns>
        public IPen GetPen(RColor color)
        {
            return CacheUtils.GetPen(color);
        }

        /// <summary>
        /// Get solid color brush.
        /// </summary>
        /// <param name="color">the color to get the brush for</param>
        /// <returns>solid color brush instance</returns>
        public IBrush GetSolidBrush(RColor color)
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
        public IBrush GetLinearGradientBrush(RRect rect, RColor color1, RColor color2, double angle)
        {
            return new BrushAdapter(new LinearGradientBrush(Utils.Convert(rect), Utils.Convert(color1), Utils.Convert(color2), (float)angle), true);
        }

        /// <summary>
        /// Get TextureBrush object that uses the specified image and bounding rectangle.
        /// </summary>
        /// <param name="image">The Image object with which this TextureBrush object fills interiors.</param>
        /// <param name="dstRect">A Rectangle structure that represents the bounding rectangle for this TextureBrush object.</param>
        /// <param name="translateTransformLocation">The dimension by which to translate the transformation</param>
        public IBrush GetTextureBrush(IImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new TextureBrush(((ImageAdapter)image).Image, Utils.Convert(dstRect));
            brush.TranslateTransform((float)translateTransformLocation.X, (float)translateTransformLocation.Y);
            return new BrushAdapter(brush, true);
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
            ReleaseHdc();
            if (_releaseGraphics)
                _g.Dispose();

            if (_useGdiPlusTextRendering && _setRtl)
                _stringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
        }


        #region Delegate graphics methods

        /// <summary>
        /// Draws a line connecting the two points specified by the coordinate pairs.
        /// </summary>
        /// <param name="pen"><see cref="T:System.Drawing.Pen"/> that determines the color, width, and style of the line. </param>
        /// <param name="x1">The x-coordinate of the first point. </param><param name="y1">The y-coordinate of the first point. </param>
        /// <param name="x2">The x-coordinate of the second point. </param><param name="y2">The y-coordinate of the second point. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="pen"/> is null.</exception>
        public void DrawLine(IPen pen, double x1, double y1, double x2, double y2)
        {
            ReleaseHdc();
            _g.DrawLine(((PenAdapter)pen).Pen, (float)x1, (float)y1, (float)x2, (float)y2);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A Pen that determines the color, width, and style of the rectangle. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param>
        /// <param name="width">The width of the rectangle to draw. </param>
        /// <param name="height">The height of the rectangle to draw. </param>
        public void DrawRectangle(IPen pen, double x, double y, double width, double height)
        {
            ReleaseHdc();
            _g.DrawRectangle(((PenAdapter)pen).Pen, (float)x, (float)y, (float)width, (float)height);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill. </param>
        /// <param name="width">Width of the rectangle to fill. </param>
        /// <param name="height">Height of the rectangle to fill. </param>
        public void DrawRectangle(IBrush brush, double x, double y, double width, double height)
        {
            ReleaseHdc();
            _g.FillRectangle(((BrushAdapter)brush).Brush, (float)x, (float)y, (float)width, (float)height);
        }

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect">Rectangle structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        public void DrawImage(IImage image, RRect destRect, RRect srcRect)
        {
            ReleaseHdc();
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), Utils.Convert(srcRect), GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Draws the specified Image at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image to draw. </param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. </param>
        public void DrawImage(IImage image, RRect destRect)
        {
            ReleaseHdc();
            _g.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
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
        public void DrawPath(IBrush brush, IGraphicsPath path)
        {
            ReleaseHdc();
            _g.FillPath(((BrushAdapter)brush).Brush, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by Point structures.
        /// </summary>
        /// <param name="brush">Brush that determines the characteristics of the fill. </param>
        /// <param name="points">Array of Point structures that represent the vertices of the polygon to fill. </param>
        public void DrawPolygon(IBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
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
            if (_hdc == IntPtr.Zero)
            {
                var clip = _g.Clip.GetHrgn(_g);

                _hdc = _g.GetHdc();
                _setRtl = false;
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
        private void SetFont(IFont font)
        {
            InitHdc();
            Win32Utils.SelectObject(_hdc, ((FontAdapter)font).HFont);
        }

        /// <summary>
        /// Set the text color of the device context.
        /// </summary>
        private void SetTextColor(Color color)
        {
            InitHdc();
            int rgb = (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R;
            Win32Utils.SetTextColor(_hdc, rgb);
        }

        /// <summary>
        /// Change text align to Left-to-Right or Right-to-Left if required.
        /// </summary>
        private void SetRtlAlign(bool rtl)
        {
            if (_setRtl)
            {
                if (!rtl)
                {
                    if (_useGdiPlusTextRendering)
                        _stringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
                    else
                        Win32Utils.SetTextAlign(_hdc, Win32Utils.TextAlignDefault);
                }
            }
            else if (rtl)
            {
                if (_useGdiPlusTextRendering)
                    _stringFormat2.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                else
                    Win32Utils.SetTextAlign(_hdc, Win32Utils.TextAlignRtl);
            }
            _setRtl = rtl;
        }

        /// <summary>
        /// Special draw logic to draw transparent text using GDI.<br/>
        /// 1. Create in-memory DC<br/>
        /// 2. Copy background to in-memory DC<br/>
        /// 3. Draw the text to in-memory DC<br/>
        /// 4. Copy the in-memory DC to the proper location with alpha blend<br/>
        /// </summary>
        private static void DrawTransparentText(IntPtr hdc, string str, IFont font, Point point, Size size, Color color)
        {
            IntPtr dib;
            var memoryHdc = Win32Utils.CreateMemoryHdc(hdc, size.Width, size.Height, out dib);

            try
            {
                // copy target background to memory HDC so when copied back it will have the proper background
                Win32Utils.BitBlt(memoryHdc, 0, 0, size.Width, size.Height, hdc, point.X, point.Y, Win32Utils.BitBltCopy);

                // Create and select font
                Win32Utils.SelectObject(memoryHdc, ((FontAdapter)font).HFont);
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