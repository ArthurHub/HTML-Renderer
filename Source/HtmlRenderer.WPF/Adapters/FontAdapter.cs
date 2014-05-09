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

using System.Windows.Media;
using HtmlRenderer.Adapters;

namespace HtmlRenderer.WPF.Adapters
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
        private readonly Typeface _font;

        /// <summary>
        /// the size of the font
        /// </summary>
        private readonly double _size;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private readonly double _underlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private readonly double _height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double _whitespaceWidth = -1;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(Typeface font, double size)
        {
            _font = font;
            _size = size;
            _height = font.XHeight;
            _underlineOffset = font.UnderlinePosition;
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public Typeface Font
        {
            get { return _font; }
        }

        /// <summary>
        /// Gets the em-size of this Font measured in the units specified by the Unit property.
        /// </summary>
        public override double Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the em-size, in points, of this Font.
        /// // TODO:a check that the size is handled properly
        /// </summary>
        public override double SizeInPoints
        {
            get { return _size*_font.FontFamily.LineSpacing; }
        }

        /// <summary>
        /// Get the vertical offset of the font underline location from the top of the font.
        /// </summary>
        public override double UnderlineOffset
        {
            get { return _underlineOffset; }
        }

        /// <summary>
        /// The line spacing, in pixels, of this font.
        /// </summary>
        public override double Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Get the left padding, in pixels, of the font.
        /// </summary>
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
    }
}