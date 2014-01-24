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

using System.IO;
using HtmlRenderer.Core.SysEntities;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGlobal
    {
        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        IImage FromStream(Stream memoryStream);

        /// <summary>
        /// Get color instance from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color instance</returns>
        ColorInt ColorFromName(string colorName);
    }
}
