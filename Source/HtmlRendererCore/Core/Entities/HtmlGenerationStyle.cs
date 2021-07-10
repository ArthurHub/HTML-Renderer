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

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Controls the way styles are generated when html is generated.
    /// </summary>
    public enum HtmlGenerationStyle
    {
        /// <summary>
        /// styles are not generated at all
        /// </summary>
        None = 0,

        /// <summary>
        /// style are inserted in style attribute for each html tag
        /// </summary>
        Inline = 1,

        /// <summary>
        /// style section is generated in the head of the html
        /// </summary>
        InHeader = 2
    }
}