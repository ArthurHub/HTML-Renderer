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
using System.IO;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// aTODO: add doc
    /// </summary>
    public interface IImage : IDisposable
    {
        /// <summary>
        /// Get the width, in pixels, of the image.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Get the height, in pixels, of the image.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Saves this image to the specified stream in PNG format.
        /// </summary>
        /// <param name="stream">The Stream where the image will be saved. </param>
        void Save(MemoryStream stream);
    }
}
