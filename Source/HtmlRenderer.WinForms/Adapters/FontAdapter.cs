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
using HtmlRenderer.Core.Interfaces;

namespace HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Font object for core.
    /// </summary>
    internal sealed class FontAdapter : IFont
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly Font _font;

        /// <summary>
        /// a handle to this Font.
        /// </summary>
        private IntPtr _hFont;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private float _height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private float _whitespaceWidth = -1;


        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(Font font)
        {
            _font = font;
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public Font Font
        {
            get { return _font; }
        }

        /// <summary>
        /// Get the handle to this Font.
        /// </summary>
        public IntPtr HFont
        {
            get
            {
                if( _hFont == IntPtr.Zero )
                    _hFont = _font.ToHfont();
                return _hFont;
            }
        }

        /// <summary>
        /// Gets the em-size of this Font measured in the units specified by the Unit property.
        /// </summary>
        public float Size
        {
            get { return _font.Size; }
        }

        /// <summary>
        /// Gets the em-size, in points, of this Font.
        /// </summary>
        public float SizeInPoints
        {
            get { return _font.SizeInPoints; }
        }

        /// <summary>
        /// Gets the ascent of the font.
        /// </summary>
        /// <remarks>
        /// Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        /// </remarks>
        public float Ascent
        {
            get { return _font.Size * _font.FontFamily.GetCellAscent(_font.Style) / _font.FontFamily.GetEmHeight(_font.Style); }
        }

        /// <summary>
        /// Gets the descent of the font.
        /// </summary>
        /// <remarks>
        /// Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        /// </remarks>
        public float Descent
        {
            get { return _font.Size * _font.FontFamily.GetCellDescent(_font.Style) / _font.FontFamily.GetEmHeight(_font.Style); }
        }

        /// <summary>
        /// Gets the line spacing of the font
        /// </summary>
        /// <remarks>
        /// Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        /// </remarks>
        public float LineSpacing
        {
            get { return _font.Size * _font.FontFamily.GetLineSpacing(_font.Style) / _font.FontFamily.GetEmHeight(_font.Style); }
        }

        /// <summary>
        /// The line spacing, in pixels, of this font.
        /// </summary>
        public float Height
        {
            get
            {
                if (_height < 0)
                {
                    _height = _font.GetHeight();
                }
                return _height;
            }
        }

        /// <summary>
        /// Get the left padding, in pixels, of the font.
        /// </summary>
        public float LeftPadding
        {
            get { return Height / 6f; }
        }


        public float WhitespaceWidth
        {
            get
            {
                if (_whitespaceWidth < 0)
                {
                    using (var g = Graphics.FromHdc(IntPtr.Zero))
                    using (var mg = new GraphicsAdapter(g, false))
                    {
                        _whitespaceWidth = mg.MeasureString(" ", this).Width;
                    }
                }
                return _whitespaceWidth;
            }
        }

    }
}
