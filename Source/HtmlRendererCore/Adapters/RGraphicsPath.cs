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
    /// Adapter for platform specific graphics path object - used to render (draw/fill) path shape.
    /// </summary>
    public abstract class RGraphicsPath : IDisposable
    {
        /// <summary>
        /// Start path at the given point.
        /// </summary>
        public abstract void Start(double x, double y);
        
        /// <summary>
        /// Add stright line to the given point from te last point.
        /// </summary>
        public abstract void LineTo(double x, double y);
        
        /// <summary>
        /// Add circular arc of the given size to the given point from the last point.
        /// </summary>
        public abstract void ArcTo(double x, double y, double size, Corner corner);
        
        /// <summary>
        /// Release path resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// The 4 corners that are handled in arc rendering.
        /// </summary>
        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }
    }
}