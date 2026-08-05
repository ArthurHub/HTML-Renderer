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
    /// The colour scheme the rendering surface presents, as reported by the platform adapter and
    /// queried by the <c>prefers-color-scheme</c> media feature (Media Queries 5 §5.1).
    /// </summary>
    public enum RColorScheme
    {
        /// <summary>
        /// A light surface - dark content on a light background. The default for any adapter that has
        /// no system theme to follow.
        /// </summary>
        Light,

        /// <summary>
        /// A dark surface - light content on a dark background.
        /// </summary>
        Dark
    }
}
