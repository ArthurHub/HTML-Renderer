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

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Adapter for platform specific brush objects - used to fill graphics (rectangles, polygons and paths).<br/>
    /// The brush can be solid color, gradient or image.
    /// </summary>
    public abstract class RBrush : IDisposable
    {
        public abstract void Dispose();
    }
}