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

using System.Drawing;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace TheArtOfDev.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly Brush _brush;

        /// <summary>
        /// If to dispose the brush when <see cref="Dispose"/> is called.<br/>
        /// Ignore dispose for cached brushes.
        /// </summary>
        private readonly bool _dispose;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(Brush brush, bool dispose)
        {
            _brush = brush;
            _dispose = dispose;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public Brush Brush
        {
            get { return _brush; }
        }

        public override void Dispose()
        {
            if (_dispose)
            {
                _brush.Dispose();
            }
        }
    }
}