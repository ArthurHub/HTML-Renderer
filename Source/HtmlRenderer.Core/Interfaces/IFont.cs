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

namespace HtmlRenderer.Core.Interfaces
{
    /// <summary>
    /// atodo: add doc
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// Gets the em-size of this Font measured in the units specified by the Unit property.
        /// </summary>
        float Size { get; }

        /// <summary>
        /// Gets the em-size, in points, of this Font.
        /// </summary>
        float SizeInPoints { get; }

        /// <summary>
        /// The line spacing, in pixels, of this font.
        /// </summary>
        float Height { get; }

        /// <summary>
        /// Get the vertical offset of the font underline location from the top of the font.
        /// </summary>
        float UnderlineOffset { get; }

        /// <summary>
        /// Get the left padding, in pixels, of the font.
        /// </summary>
        float LeftPadding { get; }

        float GetWhitespaceWidth(IGraphics graphics);
    }
}
