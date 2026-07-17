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

using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.Adapters
{
    /// <summary>
    /// A multi-stop linear gradient "brush" for the PdfSharp backend. Unlike <see cref="BrushAdapter"/>,
    /// this does not wrap a real <c>PdfSharp.Drawing.XBrush</c> - <c>XLinearGradientBrush</c> in the
    /// PDFsharp 1.50 package this project depends on only supports 2 colors, no stop list. Instead this
    /// just carries the gradient line and stops; <see cref="GraphicsAdapter"/>'s <c>DrawPath</c>/
    /// <c>DrawRectangle</c> special-case this type and paint it as a series of adjacent 2-color
    /// <c>XLinearGradientBrush</c> bands, one per consecutive stop pair - each band is a real 2-color
    /// linear gradient by definition, so the composite is an exact piecewise-linear rendering, not an
    /// approximation.
    /// </summary>
    internal sealed class GradientBrushAdapter : RBrush
    {
        public GradientBrushAdapter(RPoint p1, RPoint p2, (RColor Color, double Position)[] stops)
        {
            P1 = p1;
            P2 = p2;
            Stops = stops;
        }

        public RPoint P1 { get; }

        public RPoint P2 { get; }

        public (RColor Color, double Position)[] Stops { get; }

        public override void Dispose()
        { }
    }
}
