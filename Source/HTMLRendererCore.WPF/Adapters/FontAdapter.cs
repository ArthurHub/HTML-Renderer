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
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF Font.
    /// </summary>
    internal sealed class FontAdapter : RFont
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        private readonly Typeface _font;

        /// <summary>
        /// The glyph font for the font
        /// </summary>
        private readonly GlyphTypeface _glyphTypeface;

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
            _height = 96d / 72d * _size * _font.FontFamily.LineSpacing;
            _underlineOffset = 96d / 72d * _size * (_font.FontFamily.LineSpacing + font.UnderlinePosition);

            GlyphTypeface typeface;
            if (font.TryGetGlyphTypeface(out typeface))
            {
                _glyphTypeface = typeface;
            }
            else
            {
                foreach (var sysTypeface in Fonts.SystemTypefaces)
                {
                    if (sysTypeface.TryGetGlyphTypeface(out typeface))
                        break;
                }
            }
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public Typeface Font
        {
            get { return _font; }
        }

        public GlyphTypeface GlyphTypeface
        {
            get { return _glyphTypeface; }
        }

        public override double Size
        {
            get { return _size; }
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
    }
}