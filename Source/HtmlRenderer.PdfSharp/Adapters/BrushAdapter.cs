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

using HtmlRenderer.Adapters;
using PdfSharp.Drawing;

namespace HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// Adapter for WinForms brushes objects for core.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        private readonly XBrush _brush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(XBrush brush)
        {
            _brush = brush;
        }

        /// <summary>
        /// The actual WinForms brush instance.
        /// </summary>
        public XBrush Brush
        {
            get { return _brush; }
        }

        public override void Dispose()
        { }
    }
}