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

using Microsoft.Graphics.Canvas.Brushes;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinUI.Adapters
{
    /// <summary>
    /// Adapter for Win2D brushes.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual Win2D brush instance.
        /// </summary>
        private readonly ICanvasBrush _brush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(ICanvasBrush brush)
        {
            _brush = brush;
        }

        /// <summary>
        /// The actual Win2D brush instance.
        /// </summary>
        public ICanvasBrush Brush
        {
            get { return _brush; }
        }

        /// <summary>
        /// Does nothing, the core also disposes the cached solid brushes, which would break their next use.
        /// </summary>
        /// <remarks>
        /// Gradient and texture brushes aren't cached, they're left to the finalizer.
        /// </remarks>
        public override void Dispose()
        {
        }
    }
}
