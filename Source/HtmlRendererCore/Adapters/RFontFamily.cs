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

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Adapter for platform specific font family object - define the available font families to use.<br/>
    /// Required for custom fonts handling: fonts that are not installed on the system.
    /// </summary>
    public abstract class RFontFamily
    {
        /// <summary>
        /// Gets the name of this Font Family.
        /// </summary>
        public abstract string Name { get; }
    }
}