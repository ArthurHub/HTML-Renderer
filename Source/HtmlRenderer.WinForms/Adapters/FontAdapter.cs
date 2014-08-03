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
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Font object for core.
    /// </summary>
    internal sealed class FontAdapter : RFont
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
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private float _underlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private float _height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double _whitespaceWidth = -1;

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
                if (_hFont == IntPtr.Zero)
                    _hFont = _font.ToHfont();
                return _hFont;
            }
        }

        public override double Size
        {
            get { return _font.Size; }
        }

        public override double UnderlineOffset
        {
            get { return _underlineOffset; }
        }

        public override double Height
        {
            get { return _height; }
        }

        public override double LeftPadding
        {
            get { return _height / 6f; }
        }

        public override double GetWhitespaceWidth(RGraphics graphics)
        {
            if (_whitespaceWidth < 0)
            {
                _whitespaceWidth = graphics.MeasureString(" ", this).Width;
            }
            return _whitespaceWidth;
        }

        /// <summary>
        /// Set font metrics to be cached for the font for future use.
        /// </summary>
        /// <param name="height">the full height of the font</param>
        /// <param name="underlineOffset">the vertical offset of the font underline location from the top of the font.</param>
        internal void SetMetrics(int height, int underlineOffset)
        {
            _height = height;
            _underlineOffset = underlineOffset;
        }
    }
}