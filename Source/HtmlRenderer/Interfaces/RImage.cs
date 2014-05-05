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
    /// TODO:a add doc
    /// </summary>
    public abstract class RImage : IDisposable
    {
        /// <summary>
        ///  Get the width, in pixels, of the image.
        ///  </summary>
        public abstract double Width { get; }

        /// <summary>
        ///  Get the height, in pixels, of the image.
        ///  </summary>
        public abstract double Height { get; }

        /// <summary>
        ///  Saves this image to the specified stream in PNG format.
        ///  </summary><param name="stream">The Stream where the image will be saved. </param>
        public abstract void Save(MemoryStream stream);

        public abstract void Dispose();
    }
}