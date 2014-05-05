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

namespace HtmlRenderer.Adapters
{
    /// <summary>
    /// TODO:a add doc
    /// </summary>
    public abstract class RFont
    {
        /// <summary>
        ///  Gets the em-size of this Font measured in the units specified by the Unit property.
        ///  </summary>
        public abstract double Size { get; }

        /// <summary>
        ///  Gets the em-size, in points, of this Font.
        ///  </summary>
        public abstract double SizeInPoints { get; }

        /// <summary>
        ///  The line spacing, in pixels, of this font.
        ///  </summary>
        public abstract double Height { get; }

        /// <summary>
        ///  Get the vertical offset of the font underline location from the top of the font.
        ///  </summary>
        public abstract double UnderlineOffset { get; }

        /// <summary>
        ///  Get the left padding, in pixels, of the font.
        ///  </summary>
        public abstract double LeftPadding { get; }

        public abstract double GetWhitespaceWidth(RGraphics graphics);
    }
}