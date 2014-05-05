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

using HtmlRenderer.Interfaces;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
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
        private readonly XFont _font;

        /// <summary>
        /// the vertical offset of the font underline location from the top of the font.
        /// </summary>
        private double _underlineOffset = -1;

        /// <summary>
        /// Cached font height.
        /// </summary>
        private double _height = -1;

        /// <summary>
        /// Cached font whitespace width.
        /// </summary>
        private double _whitespaceWidth = -1;


        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public FontAdapter(XFont font)
        {
            _font = font;
        }

        /// <summary>
        /// the underline win-forms font.
        /// </summary>
        public XFont Font
        {
            get { return _font; }
        }

        /// <summary>
        /// Gets the em-size of this Font measured in the units specified by the Unit property.
        /// </summary>
        public double Size
        {
            get { return _font.Size; }
        }

        /// <summary>
        /// Gets the em-size, in points, of this Font.
        /// </summary>
        public double SizeInPoints
        {
            get { return _font.Size; }
        }

        /// <summary>
        /// Get the vertical offset of the font underline location from the top of the font.
        /// </summary>
        public double UnderlineOffset
        {
            get { return _underlineOffset; }
        }

        /// <summary>
        /// The line spacing, in pixels, of this font.
        /// </summary>
        public double Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Get the left padding, in pixels, of the font.
        /// </summary>
        public double LeftPadding
        {
            get { return _height / 6f; }
        }


        public double GetWhitespaceWidth(GraphicsBase graphics)
        {
            if( _whitespaceWidth < 0 )
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
